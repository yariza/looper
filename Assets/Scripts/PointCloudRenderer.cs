using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointCloudRenderer : MonoBehaviour
{
    [SerializeField]
    RenderTexture _positionTex;

    [SerializeField]
    RenderTexture _colorTex;

    [SerializeField]
    Material _material;

    [SerializeField, Range(0, 0.5f)]
    float _particleSize = 0.1f;

    [SerializeField]
    bool _flip;

    int _numPoints;

    Camera _camera;

    private void Start()
    {
        _numPoints = _positionTex.width * _positionTex.height;
        _camera = Camera.main;
    }

    private void OnRenderObject()
    {
        _material.SetTexture("_PositionBuffer", _positionTex);
        _material.SetTexture("_ColorBuffer", _colorTex);
        _material.SetFloat("_ParticleSize", _particleSize);
        _material.SetMatrix("_ModelMat", transform.localToWorldMatrix);
        if (_flip)
        {
            _material.EnableKeyword("FLIP");
        }
        else
        {
            _material.DisableKeyword("FLIP");
        }
        _material.SetPass(0);
        Graphics.DrawProcedural(MeshTopology.Triangles, 3, _numPoints);
    }

    private void Update()
    {
    }
}
