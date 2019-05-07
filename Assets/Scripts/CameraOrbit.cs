using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraOrbit : MonoBehaviour
{
    #region Serialized fields

    [SerializeField]
    Transform _target;

    [SerializeField, Range(0, 10)]
    float _distance = 1f;

    [SerializeField, Range(0, 10)]
    float _distanceLFOAmplitude = 0.0f;

    [SerializeField, Range(0.001f, 10f)]
    float _distanceLFOFrequency = 0.1f;

    [SerializeField, Range(0, 90)]
    float _yawSpeed = 5f;

    [SerializeField, Range(-90, 90)]
    float _pitch = 0f;

    [SerializeField, Range(-90, 90)]
    float _pitchLFOAmplitude = 0f;

    [SerializeField, Range(0.001f, 10f)]
    float _pitchLFOFrequency = 0.1f;

    [SerializeField]
    bool _enabled = true;
    [SerializeField]
    KeyCode _toggleEnable = KeyCode.V;

    #endregion

    #region Private fields

    float _distanceLFOPhase = 0f;
    float _pitchLFOPhase = 0f;
    float _yaw = 0f;

    #endregion

    #region Unity events

    private void Update()
    {
        if (Input.GetKeyDown(_toggleEnable))
        {
            _enabled = !_enabled;
        }

        if (!_enabled) return;

        var targetPos = _target.position;

        var pitch = _pitch;
        pitch += Mathf.Sin(_pitchLFOPhase) * _pitchLFOAmplitude;
        var yaw = _yaw;

        var rot = Quaternion.Euler(pitch, yaw, 0f);
        
        var distance = _distance;
        distance += Mathf.Sin(_distanceLFOPhase) * _distanceLFOAmplitude;

        var cameraPos = targetPos - rot * Vector3.forward * distance;

        transform.SetPositionAndRotation(cameraPos, rot);

        _yaw = (_yaw + _yawSpeed * Time.deltaTime) % 360.0f;
        _pitchLFOPhase = (_pitchLFOPhase + _pitchLFOFrequency * Time.deltaTime) % (2 * Mathf.PI);
        _distanceLFOPhase = (_distanceLFOPhase + _distanceLFOFrequency * Time.deltaTime) % (2 * Mathf.PI);
    }

    #endregion
}
