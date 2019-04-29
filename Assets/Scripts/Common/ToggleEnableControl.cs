using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleEnableControl : MonoBehaviour
{
    [SerializeField]
    KeyCode _key = KeyCode.C;

    [SerializeField]
    GameObject _target = null;

    private void Update()
    {
        if (Input.GetKeyDown(_key))
        {
            _target.SetActive(!_target.activeSelf);
        }
    }
}
