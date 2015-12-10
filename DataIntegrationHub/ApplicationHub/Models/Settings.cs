using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationHub.Models
{
    public class Settings
    {
        private static object _staticThreadLock = new object();

        private static Settings _settings = new Settings();
        public static Settings Instance { get { lock (_staticThreadLock) { return _settings; } } }


        private object _threadLock;

        public string AppBaseDirectory { get { lock (_threadLock) { return AppDomain.CurrentDomain.BaseDirectory; } } }
        public string AlarmConfigQueueName { get { lock (_threadLock) { return ConfigurationManager.AppSettings["AlarmConfigQueueName"]; } } }
        public string DbPersistenceQueueName { get { lock (_threadLock) { return ConfigurationManager.AppSettings["DbPersistenceQueueName"]; } } }

        private bool _isWorking;
        public bool IsWorking
        {
            get { lock (_threadLock) { return _isWorking; } }
            set { lock (_threadLock) { _isWorking = value; } }
        }

        private Settings()
        {
            _threadLock = new object();
            _isWorking = false;
        }
    }
}
