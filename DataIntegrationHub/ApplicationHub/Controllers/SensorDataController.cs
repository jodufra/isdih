using ApplicationHub.Models;
using ApplicationLib.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Threading.Tasks;
using SensorNode = SensorNodeDll.SensorNodeDll;

namespace ApplicationHub
{
    public class SensorDataController
    {
        public static SensorDataController Instance;
        public static SensorDataController CreateInstance()
        {
            Instance = new SensorDataController();
            return Instance;
        }

        private SensorNode _sensorNode;
        private SensorDataController()
        {
            _sensorNode = new SensorNode();
            _sensorNode.Initialize(OnSensorDataRecieved, 250);
        }

        private void OnSensorDataRecieved(string data)
        {
            if (!Settings.Instance.IsWorking) _sensorNode.Stop();

            CommunicationHubController.Instance.OnSensorDataReceived(RecordBuilder.BuildRecord(data));
        }

    }

}
