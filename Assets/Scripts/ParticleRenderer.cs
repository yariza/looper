using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class ParticleRenderer : MonoBehaviour
{
    #region Serialized fields

    [Header("Resources")]
    [SerializeField]
    ComputeShader _kernelShader = null;

    [SerializeField]
    RenderTexture _positionTex = null;

    [SerializeField]
    RenderTexture _colorTex = null;

    [Header("History Parameters")]
    [SerializeField, Range(1, 4096)]
    int _historySize = 64;

    [SerializeField, Range(0.01f, 1f)]
    float _frameInterval = 0.05f;

    [SerializeField]
    bool _flip = false;

    [SerializeField]
    float _effectiveLength = 0;

    public enum RenderType
    {
        Particles,
        Mesh,
    }
    [Header("Render Parameters")]
    [SerializeField]
    RenderType _renderType = RenderType.Particles;

    [SerializeField]
    Material _particleMaterial = null;

    [SerializeField, Range(0, 0.5f)]
    float _particleSize = 0.1f;

    [SerializeField]
    CameraEvent _cameraEvent = CameraEvent.AfterForwardOpaque;

    #endregion

    #region Fields

    MaterialPropertyBlock _propertyBlock;
    Camera _camera;
    CommandBuffer _commandBuffer;

    ComputeBuffer[] _positionHistoryBuffer;
    ComputeBuffer _particlePositionBuffer;
    ComputeBuffer _scratchPositionBuffer;
    Vector2 _bufferResolution;
    float _lastHistoryFrameTime;
    int _currentHistoryIndex;

    Mesh _mesh;

    int _kernelReduceBuffer;
    int _kernelCopyInputToBuffer;
    int _kernelInitParticleBuffer;
    int _kernelUpdateParticleBuffer;

    int _idPositionBuffer;
    int _idColorBuffer;
    int _idInputPositionTex;
    int _idInputColorTex;
    int _idParticlePositionBuffer;
    int _idParticleColorBuffer;
    int _idDestinationPositionBuffer;
    int _idDestinationColorBuffer;
    int _idBufferSize;
    int _idResolution;
    int _idFeedbackInv;
    int _idFeedbackSize;

    static float deltaTime
    {
        get
        {
            var isEditor = !Application.isPlaying || Time.frameCount < 2;
            return isEditor ? 1.0f / 10 : Time.deltaTime;
        }
    }

    const int INPUT_WIDTH = 512;
    const int INPUT_HEIGHT = 424;
    const int FEEDBACK_INV = 4;
    // const int BUFFER_SIZE = INPUT_WIDTH * INPUT_HEIGHT * 2;
    // 4^0 + 4^1 + ... + 4^8
    const int BUFFER_SIZE = 87381;
    // 4^0 + 4^1 + ... + 4^7
    const int FEEDBACK_SIZE = 21845;

    #endregion

    #region Unity events

    private void Awake()
    {
        _propertyBlock = new MaterialPropertyBlock();

        _camera = Camera.main;
        _commandBuffer = new CommandBuffer();

        _kernelReduceBuffer = _kernelShader.FindKernel("ReduceBuffer");
        _kernelCopyInputToBuffer = _kernelShader.FindKernel("CopyInputToBuffer");
        _kernelInitParticleBuffer = _kernelShader.FindKernel("InitParticleBuffer");
        _kernelUpdateParticleBuffer = _kernelShader.FindKernel("UpdateParticleBuffer");

        _idPositionBuffer = Shader.PropertyToID("_PositionBuffer");
        _idColorBuffer = Shader.PropertyToID("_ColorBuffer");
        _idParticlePositionBuffer = Shader.PropertyToID("_ParticlePositionBuffer");
        _idParticleColorBuffer = Shader.PropertyToID("_ParticleColorBuffer");
        _idDestinationPositionBuffer = Shader.PropertyToID("_DestinationPositionBuffer");
        _idDestinationColorBuffer = Shader.PropertyToID("_DestinationColorBuffer");
        _idInputPositionTex = Shader.PropertyToID("_InputPositionTex");
        _idInputColorTex = Shader.PropertyToID("_InputColorTex");
        _idBufferSize = Shader.PropertyToID("_BufferSize");
        _idFeedbackInv = Shader.PropertyToID("_FeedbackInv");
        _idFeedbackSize = Shader.PropertyToID("_FeedbackSize");
        _idResolution = Shader.PropertyToID("_Resolution");
    }

    private void OnEnable()
    {
        _camera.AddCommandBuffer(_cameraEvent, _commandBuffer);

        _positionHistoryBuffer = new ComputeBuffer[_historySize];
        for (int i = 0; i < _historySize; i++)
        {
            _positionHistoryBuffer[i] = new ComputeBuffer(BUFFER_SIZE, sizeof(float) * 4);
        }
        _currentHistoryIndex = 0;

        _particlePositionBuffer = new ComputeBuffer(BUFFER_SIZE, sizeof(float) * 4);

        _scratchPositionBuffer = new ComputeBuffer(BUFFER_SIZE, sizeof(float) * 4);

        {
            // init particle buffer
            _kernelShader.SetBuffer(_kernelInitParticleBuffer, _idParticlePositionBuffer, _particlePositionBuffer);
            _kernelShader.SetInt(_idBufferSize, BUFFER_SIZE);
            _kernelShader.SetInts(_idResolution, INPUT_WIDTH, INPUT_HEIGHT);

            const int threadsPerGroup = 512;
            int groupsX = BUFFER_SIZE / threadsPerGroup;
            _kernelShader.Dispatch(_kernelInitParticleBuffer, groupsX, 1, 1);
        }
    }

    private void OnDisable()
    {
        if (_camera != null)
        {
            _camera.RemoveCommandBuffer(_cameraEvent, _commandBuffer);
        }

        for (int i = 0; i < _historySize; i++)
        {
            _positionHistoryBuffer[i].Release();
        }
        _positionHistoryBuffer = null;

        _particlePositionBuffer.Release();
        _particlePositionBuffer = null;

        _scratchPositionBuffer.Release();
        _scratchPositionBuffer = null;
    }

    private void Update()
    {
        var positionBuffer = _positionHistoryBuffer[_currentHistoryIndex];

        if (Time.time - _lastHistoryFrameTime > _frameInterval)
        {
            // update history
            {
                // reduce buffer
                _kernelShader.SetBuffer(_kernelReduceBuffer, _idPositionBuffer, positionBuffer);
                _kernelShader.SetBuffer(_kernelReduceBuffer, _idDestinationPositionBuffer, _scratchPositionBuffer);
                _kernelShader.SetInt(_idBufferSize, BUFFER_SIZE);
                _kernelShader.SetInt(_idFeedbackInv, FEEDBACK_INV);

                const int threadsPerGroup = 512;
                int groupsX = BUFFER_SIZE / threadsPerGroup;
                _kernelShader.Dispatch(_kernelReduceBuffer, groupsX, 1, 1);
            }
            {
                // copy input to buffer
                _kernelShader.SetBuffer(_kernelCopyInputToBuffer, _idDestinationPositionBuffer, _scratchPositionBuffer);
                _kernelShader.SetTexture(_kernelCopyInputToBuffer, _idInputPositionTex, _positionTex);
                _kernelShader.SetTexture(_kernelCopyInputToBuffer, _idInputColorTex, _colorTex);
                _kernelShader.SetInt(_idBufferSize, BUFFER_SIZE);
                _kernelShader.SetInt(_idFeedbackSize, FEEDBACK_SIZE);
                _kernelShader.SetInts(_idResolution, INPUT_WIDTH, INPUT_HEIGHT);

                const int threadsPerGroup = 8;
                int groupsX = INPUT_WIDTH / 2 / threadsPerGroup;
                int groupsY = (INPUT_HEIGHT / 2 + threadsPerGroup - 1) / threadsPerGroup;
                _kernelShader.Dispatch(_kernelCopyInputToBuffer, groupsX, groupsY, 1);
            }

            // swap scratch and position buffers
            var tempBuffer = _scratchPositionBuffer;
            _scratchPositionBuffer = positionBuffer;
            positionBuffer = _positionHistoryBuffer[_currentHistoryIndex] = tempBuffer;

            _lastHistoryFrameTime = Time.time;
            _currentHistoryIndex = (_currentHistoryIndex + 1) % _historySize;
        }

        // {
        //     // update particle buffer
        //     _kernelShader.SetBuffer(_kernelUpdateParticleBuffer, _idParticlePositionBuffer, _particlePositionBuffer);
        //     _kernelShader.SetBuffer(_kernelUpdateParticleBuffer, _idParticleColorBuffer, _particleColorBuffer);
        //     _kernelShader.SetBuffer(_kernelUpdateParticleBuffer, _idPositionBuffer, positionBuffer);
        //     _kernelShader.SetBuffer(_kernelUpdateParticleBuffer, _idColorBuffer, colorBuffer);
        //     _kernelShader.SetInt(_idBufferSize, BUFFER_SIZE);
            
        //     const int threadsPerGroup = 512;
        //     int groupsX = BUFFER_SIZE / threadsPerGroup;
        //     _kernelShader.Dispatch(_kernelUpdateParticleBuffer, groupsX, 1, 1);
        // }
    }

    private void OnRenderObject()
    {
        var frameIndex = _currentHistoryIndex;

        if (_renderType == RenderType.Particles)
        {
            _particleMaterial.SetBuffer(_idParticlePositionBuffer, _positionHistoryBuffer[frameIndex]);
            _particleMaterial.SetFloat("_ParticleSize", _particleSize);
            if (_flip)
            {
                _particleMaterial.EnableKeyword("FLIP");
            }
            else
            {
                _particleMaterial.DisableKeyword("FLIP");
            }
            _particleMaterial.SetMatrix("_ModelMat", transform.localToWorldMatrix);
            _particleMaterial.SetPass(0);
            Graphics.DrawProcedural(MeshTopology.Points, 1, BUFFER_SIZE);
        }
        else if (_renderType == RenderType.Mesh)
        {

        }
    }

    private void OnValidate()
    {
        _effectiveLength = _frameInterval * _historySize;
    }

    #endregion

    #region Private methods


    #endregion
}
