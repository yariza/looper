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

	[SerializeField, Range(0, 1)]
	float _scaleSpeed = 0.1f;

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

	[SerializeField]
	bool _enableScaling = false;

	[SerializeField]
	KeyCode _scaleXDown = KeyCode.Semicolon;
	[SerializeField]
	KeyCode _scaleXUp = KeyCode.Quote;
	[SerializeField]
	KeyCode _scaleYDown = KeyCode.LeftBracket;
	[SerializeField]
	KeyCode _scaleYUp = KeyCode.RightBracket;
	[SerializeField]
	KeyCode _scaleZDown = KeyCode.Minus;
	[SerializeField]
	KeyCode _scaleZUp = KeyCode.Equals;

    [Header("Save Prefs")]
    [SerializeField]
	string _playerPrefKey = "transform_control";

	[SerializeField, Range(0.5f, 10)]
	float _timeBeforeWrite = 2f;

    #endregion

    #region Private fields

	float _lastChangeTime = 0;
	Vector3 _cachedLocalPosition;
	Quaternion _cachedLocalRotation;
	Vector3 _cachedLocalScale;
	Vector3 _lastLocalPosition;
	Quaternion _lastLocalRotation;
	Vector3 _lastLocalScale;

    #endregion

    #region Unity events

    private void Start()
    {
		Vector3 position = transform.localPosition;
		Quaternion rotation = transform.localRotation;
		Vector3 scale = transform.localScale;

		if (GetSavedOffset(ref position, ref rotation, ref scale))
		{
			_lastLocalPosition = _cachedLocalPosition = transform.localPosition = position;
			_lastLocalRotation = _cachedLocalRotation = transform.localRotation = rotation;
			_lastLocalScale = _cachedLocalScale = transform.localScale = scale;
			Debug.Log("retrieved saved offset");
		}
    }

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

		if (_enableScaling)
		{
			Vector3 dScale = Vector3.zero;
			if (Input.GetKey(_scaleXDown))
			{
				dScale.x -= _scaleSpeed;
			}
			if (Input.GetKey(_scaleXUp))
			{
				dScale.x += _scaleSpeed;
			}
			if (Input.GetKey(_scaleYDown))
			{
				dScale.y -= _scaleSpeed;
			}
			if (Input.GetKey(_scaleYUp))
			{
				dScale.y += _scaleSpeed;
			}
			if (Input.GetKey(_scaleZDown))
			{
				dScale.z -= _scaleSpeed;
			}
			if (Input.GetKey(_scaleZUp))
			{
				dScale.z += _scaleSpeed;
			}
			dScale = dScale * Time.deltaTime + Vector3.one;
			var oldScale = transform.localScale;
			transform.localScale = new Vector3(
				oldScale.x * dScale.x,
				oldScale.y * dScale.y,
				oldScale.z * dScale.z
			);
		}

        var curLocalPosition = transform.localPosition;
		var curLocalRotation = transform.localRotation;
		var curLocalScale = transform.localScale;
		if (curLocalPosition != _cachedLocalPosition || curLocalRotation != _cachedLocalRotation || curLocalScale != _cachedLocalScale)
		{
			if (_lastLocalPosition != curLocalPosition || _lastLocalRotation != curLocalRotation || _lastLocalScale != curLocalScale)
			{
				_lastLocalPosition = curLocalPosition;
				_lastLocalRotation = curLocalRotation;
				_lastLocalScale = curLocalScale;
				_lastChangeTime = Time.time;
			}
			else if (Time.time - _lastChangeTime > _timeBeforeWrite)
			{
				_cachedLocalPosition = transform.localPosition;
				_cachedLocalRotation = transform.localRotation;
				_cachedLocalScale = transform.localScale;
				SaveOffset(_cachedLocalPosition, _cachedLocalRotation, _cachedLocalScale);
			}
		}
    }

	void SaveOffset(Vector3 position, Quaternion rotation, Vector3 scale)
	{
		string posXKey = _playerPrefKey + "_pos_x";
		PlayerPrefs.SetFloat(posXKey, position.x);
		string posYKey = _playerPrefKey + "_pos_y";
		PlayerPrefs.SetFloat(posYKey, position.y);
		string posZKey = _playerPrefKey + "_pos_z";
		PlayerPrefs.SetFloat(posZKey, position.z);

		string rotXKey = _playerPrefKey + "_rot_x";
		PlayerPrefs.SetFloat(rotXKey, rotation.x);
		string rotYKey = _playerPrefKey + "_rot_y";
		PlayerPrefs.SetFloat(rotYKey, rotation.y);
		string rotZKey = _playerPrefKey + "_rot_z";
		PlayerPrefs.SetFloat(rotZKey, rotation.z);
		string rotWKey = _playerPrefKey + "_rot_w";
		PlayerPrefs.SetFloat(rotWKey, rotation.w);

		string scaXKey = _playerPrefKey + "_sca_x";
		PlayerPrefs.SetFloat(scaXKey, scale.x);
		string scaYKey = _playerPrefKey + "_sca_y";
		PlayerPrefs.SetFloat(scaYKey, scale.y);
		string scaZKey = _playerPrefKey + "_sca_z";
		PlayerPrefs.SetFloat(scaZKey, scale.z);

		PlayerPrefs.Save();
		Debug.Log("saved calibration offset");
	}

	bool GetSavedOffset(ref Vector3 position, ref Quaternion rotation, ref Vector3 scale)
	{
		float posX;
		string posXKey = _playerPrefKey + "_pos_x";
		if (!PlayerPrefs.HasKey(posXKey))
			return false;
		posX = PlayerPrefs.GetFloat(posXKey);

		float posY;
		string posYKey = _playerPrefKey + "_pos_y";
		if (!PlayerPrefs.HasKey(posYKey))
			return false;
		posY = PlayerPrefs.GetFloat(posYKey);

		float posZ;
		string posZKey = _playerPrefKey + "_pos_z";
		if (!PlayerPrefs.HasKey(posZKey))
			return false;
		posZ = PlayerPrefs.GetFloat(posZKey);

		float rotX;
		string rotXKey = _playerPrefKey + "_rot_x";
		if (!PlayerPrefs.HasKey(rotXKey))
			return false;
		rotX = PlayerPrefs.GetFloat(rotXKey);

		float rotY;
		string rotYKey = _playerPrefKey + "_rot_y";
		if (!PlayerPrefs.HasKey(rotYKey))
			return false;
		rotY = PlayerPrefs.GetFloat(rotYKey);

		float rotZ;
		string rotZKey = _playerPrefKey + "_rot_z";
		if (!PlayerPrefs.HasKey(rotZKey))
			return false;
		rotZ = PlayerPrefs.GetFloat(rotZKey);

		float rotW;
		string rotWKey = _playerPrefKey + "_rot_w";
		if (!PlayerPrefs.HasKey(rotWKey))
			return false;
		rotW = PlayerPrefs.GetFloat(rotWKey);

		float scaX;
		string scaXKey = _playerPrefKey + "_sca_x";
		if (!PlayerPrefs.HasKey(scaXKey))
			return false;
		scaX = PlayerPrefs.GetFloat(scaXKey);

		float scaY;
		string scaYKey = _playerPrefKey + "_sca_y";
		if (!PlayerPrefs.HasKey(scaYKey))
			return false;
		scaY = PlayerPrefs.GetFloat(scaYKey);

		float scaZ;
		string scaZKey = _playerPrefKey + "_sca_z";
		if (!PlayerPrefs.HasKey(scaZKey))
			return false;
		scaZ = PlayerPrefs.GetFloat(scaZKey);

		position = new Vector3(posX, posY, posZ);
		rotation = new Quaternion(rotX, rotY, rotZ, rotW);
		scale = new Vector3(scaX, scaY, scaZ);
		return true;
	}

    #endregion
}
