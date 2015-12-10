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
        private SensorDataCollector _collector;
        private BackgroundWorker _worker;
        private SensorDataController()
        {
            _sensorNode = new SensorNode();
            _sensorNode.Initialize(OnSensorDataRecieved, 250);
            _collector = new SensorDataCollector(_sensorNode);
            _collector.SetWorker(_worker = new BackgroundWorker());
        }

        private void OnSensorDataRecieved(string data)
        {
            Console.WriteLine(data);
        }

    }

    public class SensorDataCollector : IBackgroundWorker
    {
        private enum SensorDataCollectorState { Working, Interrupted }

        private SensorDataCollectorState _state;
        private SensorNode _sensorNode;
        public SensorDataCollector(SensorNode sensorNode)
        {
            this._sensorNode = sensorNode;
            this._state = SensorDataCollectorState.Working;
        }

        public void DoWork(object o, System.ComponentModel.DoWorkEventArgs args)
        {
            BackgroundWorker b = o as BackgroundWorker;
            while (_state == SensorDataCollectorState.Working)
            {
                _sensorNode.DoWork();
                b.ReportProgress(0);
            }
            _sensorNode.Stop();
        }

        public void ProgressChanged(object o, System.ComponentModel.ProgressChangedEventArgs args)
        {
            if (!Settings.Instance.IsWorking) _state = SensorDataCollectorState.Interrupted;
            Console.WriteLine("ProgressChanged");
        }

        public void RunWorkerCompleted(object o, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            BackgroundWorker b = o as BackgroundWorker;
            if (_state == SensorDataCollectorState.Working)
            {
                ExceptionReporter.WriteReport(e.Error, Settings.Instance.AppBaseDirectory);
                _state = SensorDataCollectorState.Interrupted;
            }

        }

        public void SetWorker(BackgroundWorker worker)
        {
            worker.DoWork += new DoWorkEventHandler(DoWork);
            worker.ProgressChanged += new ProgressChangedEventHandler(ProgressChanged);
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(RunWorkerCompleted);
            worker.WorkerReportsProgress = true;
            worker.WorkerSupportsCancellation = true;
        }
    }
}
