using UnityEngine;
using System.Collections;
using Windows.Kinect;
using System;

public class DepthSourceManager : GameSingleton<DepthSourceManager>
{
    [System.Serializable]
    public enum UpdateModel
    {
        Polling,
        Event,
    };
    public UpdateModel _UpdateModel = UpdateModel.Polling;

    public delegate void DepthFrameArrivedCallback(ushort[] data);
    public event DepthFrameArrivedCallback OnDepthFrameArrived;

    private KinectSensor _Sensor;
    private DepthFrameReader _Reader;
    private ushort[] _Data;

    public ushort[] GetData()
    {
        return _Data;
    }

    void Start () 
    {
        try
        {
            _Sensor = KinectSensor.GetDefault();
        }
        catch (Exception e)
        {
            Debug.LogWarning("Kinect dll not installed.");
        }

        if (_Sensor != null) 
        {
            _Reader = _Sensor.DepthFrameSource.OpenReader();
            _Data = new ushort[_Sensor.DepthFrameSource.FrameDescription.LengthInPixels];

            if (_UpdateModel == UpdateModel.Event)
            {
                _Reader.FrameArrived += _OnFrameArrived;
            }

            if (!_Sensor.IsOpen)
            {
                _Sensor.Open();
            }
        }
    }

    private void _OnFrameArrived(object sender, DepthFrameArrivedEventArgs e)
    {
        var frame = _Reader.AcquireLatestFrame();
        if (frame != null)
        {
            frame.CopyFrameDataToArray(_Data);
            frame.Dispose();
            frame = null;
        }
        if (OnDepthFrameArrived != null)
        {
            OnDepthFrameArrived(_Data);
        }
    }

    void Update ()
    {
        if (_UpdateModel != UpdateModel.Polling) return;

        if (_Reader != null)
        {
            var frame = _Reader.AcquireLatestFrame();
            if (frame != null)
            {
                frame.CopyFrameDataToArray(_Data);
                frame.Dispose();
                frame = null;
            }
        }
    }
    
    void OnApplicationQuit()
    {
        if (_Reader != null)
        {
            _Reader.Dispose();

            if (_UpdateModel == UpdateModel.Event)
            {
                _Reader.FrameArrived -= _OnFrameArrived;
            }
            _Reader = null;
        }

        if (_Sensor != null)
        {
            if (_Sensor.IsOpen)
            {
                _Sensor.Close();
            }
            
            _Sensor = null;
        }
    }
}
