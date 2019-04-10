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

    int _numPoints;

    private void Start()
    {
        _numPoints = _positionTex.width * _positionTex.height;
    }

    private void Update()
    {
        _material.SetTexture("_PositionBuffer", _positionTex);
        _material.SetTexture("_ColorBuffer", _colorTex);
        _material.SetFloat("_ParticleSize", 1f);
        _material.SetPass(0);
        Graphics.DrawProcedural(MeshTopology.Triangles, 3, _numPoints);
    }
}
