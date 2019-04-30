using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformControl : MonoBehaviour
{
    #region Serialized fields

    [SerializeField, Range(0, 1)]
    float _speed = 0.1f;

    [SerializeField, Range(0, 90)]
    float _rotationSpeed = 20f;

    [Header("Keys")]
    [SerializeField]
    KeyCode _left = KeyCode.Keypad4;
    [SerializeField]
    KeyCode _right = KeyCode.Keypad6;
    [SerializeField]
    KeyCode _back = KeyCode.Keypad5;
    [SerializeField]
    KeyCode _forward = KeyCode.Keypad8;
    [SerializeField]
    KeyCode _down = KeyCode.KeypadMinus;
    [SerializeField]
    KeyCode _up = KeyCode.KeypadPlus;
    [SerializeField]
    KeyCode _turnLeft = KeyCode.Keypad1;
    [SerializeField]
    KeyCode _turnRight = KeyCode.Keypad3;
    [SerializeField]
    KeyCode _turnDown = KeyCode.Keypad7;
    [SerializeField]
    KeyCode _turnUp = KeyCode.Keypad9;

    #endregion

    #region Unity events

    private void Update()
    {
        Vector3 dPos = Vector3.zero;
        if (Input.GetKey(_left))
        {
            dPos.x -= 1f;
        }
        if (Input.GetKey(_right))
        {
            dPos.x += 1f;
        }
        if (Input.GetKey(_back))
        {
            dPos.z -= 1f;
        }
        if (Input.GetKey(_forward))
        {
            dPos.z += 1f;
        }
        if (Input.GetKey(_down))
        {
            dPos.y -= 1f;
        }
        if (Input.GetKey(_up))
        {
            dPos.y += 1f;
        }
        dPos *= _speed * Time.deltaTime;

        transform.position += transform.TransformDirection(dPos);

        Vector3 dEuler = Vector3.zero;
        if (Input.GetKey(_turnLeft))
        {
            dEuler.y -= 1f;
        }
        if (Input.GetKey(_turnRight))
        {
            dEuler.y += 1f;
        }
        if (Input.GetKey(_turnDown))
        {
            dEuler.x -= 1f;
        }
        if (Input.GetKey(_turnUp))
        {
            dEuler.x += 1f;
        }
        dEuler *= _rotationSpeed * Time.deltaTime;

        transform.localEulerAngles += dEuler;
    }

    #endregion
}
