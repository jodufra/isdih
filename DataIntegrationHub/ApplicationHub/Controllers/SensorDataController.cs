using ApplicationHub.Models;
using ApplicationLib;
using ApplicationLib.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SensorNode = SensorNodeDll.SensorNodeDll;

namespace ApplicationHub
{
    public class SensorDataController : IDisposable
    {
        public static SensorDataController Instance;
        private bool disposed;

        public static SensorDataController CreateInstance()
        {
            Instance = new SensorDataController();
            return Instance;
        }

        private SensorNode _sensorNode = null;
        
        private SensorDataController()
        {
            _sensorNode = new SensorNode();
            _sensorNode.Initialize(OnSensorDataRecieved, Properties.Settings.Default.Delay);
        }

        public void OnSensorDataRecieved(string data)
        {
            if (!Settings.Instance.IsWorking)
            {
                _sensorNode.Stop();
                return;
            }
            CommunicationHubController.Instance.OnSensorDataReceived(RecordBuilder.BuildRecord(data));
        }

        public bool StopStart()
        {
            if (_sensorNode != null)
            {
                if (Settings.Instance.IsWorking)
                {
                    Settings.Instance.IsWorking = false;
                    //_sensorNode.Stop();
                    return false;
                }
                else
                {
                    _sensorNode = new SensorNode();
                    _sensorNode.Initialize(OnSensorDataRecieved, Properties.Settings.Default.Delay);
                    Settings.Instance.IsWorking = true;
                    return true;
                }
            }
            return false;
        }

        public bool GetState()
        {
            return Settings.Instance.IsWorking;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    _sensorNode.Stop();
                    _sensorNode = null;
                }
            }
            disposed = true;
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }

}