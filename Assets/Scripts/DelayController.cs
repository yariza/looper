using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class DelayController : MonoBehaviour
{
    #region Serialized fields
    [SerializeField]
    ParticleRenderer _particle = null;

    [SerializeField, Range(0, 10)]
    int _numTaps = 5;

    [SerializeField, Range(0, 1)]
    float _feedback = 0.5f;

    [SerializeField, Range(0, 1)]
    float _volume = 1;

    [SerializeField, Range(1f, 30f)]
    float _positionHistoryFrequency = 15f;

    [SerializeField, Range(0.5f, 2f)]
    float _multiplier = 1f;

    [SerializeField]
    AudioMixerGroup _output = null;

    [SerializeField]
    AudioRolloffMode _rolloff = AudioRolloffMode.Logarithmic;

    [SerializeField]
    float _minDist = 1.0f;

    [SerializeField]
    float _maxDist = 100f;

    #endregion

    #region Private fields

    AudioClip _clip;
    AudioSource[] _sources;
    Transform _cameraTransform;
    Vector3[] _positionHistory;
    float _currentHistoryIndex;
    float _positionHistoryStartTime;
    int _prevMicrophonePosition = 0;

    #endregion

    #region Unity events

    private void Awake()
    {
        var length = (_numTaps + 1) * _particle.effectiveLength * _multiplier;
        _clip = Microphone.Start(null, true, Mathf.CeilToInt(length), 44100);

        _sources = new AudioSource[_numTaps];
        for (int i = 0; i < _numTaps; i++)
        {
            var go = new GameObject("audio tap " + i);
            var source = _sources[i] = go.AddComponent<AudioSource>();
            source.clip = _clip;
            source.loop = true;
            source.spatialize = true;
            source.spatializePostEffects = true;
            source.spatialBlend = 1f;
            source.timeSamples = _clip.samples - (i + 1) * Mathf.FloorToInt(_particle.effectiveLength * _multiplier * 44100);
            source.volume = _volume * Mathf.Pow(0.5f, i);
            source.outputAudioMixerGroup = _output;
            source.rolloffMode = _rolloff;
            source.minDistance = _minDist;
            source.maxDistance = _maxDist;
            source.Play();
        }

        var positionLength = Mathf.CeilToInt(length * _positionHistoryFrequency);
        _positionHistory = new Vector3[positionLength];
        _currentHistoryIndex = 0f;
        _positionHistoryStartTime = Time.time;

        _cameraTransform = Camera.main.transform;
    }

    private void Update()
    {
        var micPosition = Microphone.GetPosition(null);

        // get camera positions
        for (int i = 0; i < _sources.Length; i++)
        {
            var source = _sources[i];
            var findex = _currentHistoryIndex - (i + 1) * _particle.effectiveLength * _multiplier * _positionHistoryFrequency;
            findex = (findex + _positionHistory.Length);

            var left = Mathf.FloorToInt(findex) % _positionHistory.Length;
            var right = (left + 1) % _positionHistory.Length;
            var t = findex - Mathf.Floor(findex);
            var pos = Vector3.Lerp(_positionHistory[left], _positionHistory[right], t);
            source.transform.position = pos;
        }

        // write camera position
        var nextIndex = _currentHistoryIndex + Time.deltaTime * _positionHistoryFrequency;
        for (var curIndex = _currentHistoryIndex; curIndex < nextIndex; curIndex++)
        {
            var i = Mathf.FloorToInt(curIndex) % _positionHistory.Length;
            _positionHistory[i] = _cameraTransform.position;
        }
        _currentHistoryIndex = nextIndex % _positionHistory.Length;

        if (micPosition < _prevMicrophonePosition)
        {
            // Debug.Log("recalibrate");
            // recalibrate source clips
            for (int i = 0; i < _sources.Length; i++)
            {
                var source = _sources[i];
                source.timeSamples = (_clip.samples + micPosition - Mathf.FloorToInt((i + 1) * _particle.effectiveLength * _multiplier * 44100)) % _clip.samples;
            }
        }

        _prevMicrophonePosition = micPosition;
    }

    private void OnDestroy()
    {
        Microphone.End(null);
    }

    #endregion
}
