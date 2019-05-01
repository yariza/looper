using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointCloudRenderer : MonoBehaviour
{
    [Header("Resources")]
    [SerializeField]
    RenderTexture _positionTex = null;

    [SerializeField]
    RenderTexture _colorTex = null;

    public enum RenderType
    {
        Particles,
        Mesh,
    }
    [Header("Render Parameters")]
    [SerializeField]
    RenderType _type = RenderType.Particles;

    [SerializeField]
    Material _particleMaterial = null;

    [SerializeField, Range(0, 0.5f)]
    float _particleSize = 0.1f;

    [SerializeField]
    bool _flip = false;

    [SerializeField]
    Material _meshMaterial = null;

    int _numPoints;

    Camera _camera;

    private void Start()
    {
        _numPoints = _positionTex.width * _positionTex.height;
        _camera = Camera.main;
    }

    private void OnRenderObject()
    {
        if (_type == RenderType.Particles)
        {
            _particleMaterial.SetTexture("_PositionBuffer", _positionTex);
            _particleMaterial.SetTexture("_ColorBuffer", _colorTex);
            _particleMaterial.SetFloat("_ParticleSize", _particleSize);
            _particleMaterial.SetMatrix("_ModelMat", transform.localToWorldMatrix);
            if (_flip)
            {
                _particleMaterial.EnableKeyword("FLIP");
            }
            else
            {
                _particleMaterial.DisableKeyword("FLIP");
            }
            _particleMaterial.SetPass(0);
            Graphics.DrawProcedural(MeshTopology.Triangles, 3, _numPoints);
        }
        else if (_type == RenderType.Mesh)
        {
            _meshMaterial.SetTexture("_PositionBuffer", _positionTex);
            _meshMaterial.SetTexture("_ColorBuffer", _colorTex);
            _meshMaterial.SetMatrix("_ModelMat", transform.localToWorldMatrix);
            if (_flip)
            {
                _meshMaterial.EnableKeyword("FLIP");
            }
            else
            {
                _meshMaterial.DisableKeyword("FLIP");
            }
            _meshMaterial.SetPass(0);
            Graphics.DrawProcedural(MeshTopology.Triangles, 3, (_positionTex.width - 1) * (_positionTex.height - 1) * 2);
        }
    }

    private void Update()
    {
    }
}
