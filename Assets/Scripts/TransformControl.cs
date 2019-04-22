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

    #endregion

    #region Unity events

    private void Update()
    {
        Vector3 dPos = Vector3.zero;
        if (Input.GetKey(KeyCode.Keypad4))
        {
            dPos.x -= 1f;
        }
        if (Input.GetKey(KeyCode.Keypad6))
        {
            dPos.x += 1f;
        }
        if (Input.GetKey(KeyCode.Keypad5))
        {
            dPos.z -= 1f;
        }
        if (Input.GetKey(KeyCode.Keypad8))
        {
            dPos.z += 1f;
        }
        if (Input.GetKey(KeyCode.KeypadMinus))
        {
            dPos.y -= 1f;
        }
        if (Input.GetKey(KeyCode.KeypadPlus))
        {
            dPos.y += 1f;
        }
        dPos *= _speed * Time.deltaTime;

        transform.localPosition += dPos;

        Vector3 dEuler = Vector3.zero;
        if (Input.GetKey(KeyCode.Keypad1))
        {
            dEuler.y -= 1f;
        }
        if (Input.GetKey(KeyCode.Keypad3))
        {
            dEuler.y += 1f;
        }
        if (Input.GetKey(KeyCode.Keypad7))
        {
            dEuler.x -= 1f;
        }
        if (Input.GetKey(KeyCode.Keypad9))
        {
            dEuler.x += 1f;
        }
        dEuler *= _rotationSpeed * Time.deltaTime;

        transform.localEulerAngles += dEuler;
    }
        
    #endregion
}
