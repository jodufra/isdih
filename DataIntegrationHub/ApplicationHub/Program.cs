using ApplicationLib.Entities;
using ApplicationLib.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationHub
{
    public class Program
    {
        static void Main(string[] args)
        {
            Program.Instance.Initialize();
        }

        static Program Instance = new Program();
        private object _threadLock;
        private MessageQueue _sensorMessageQueue;
        public MessageQueue sensorMessageQueue
        {
            get { lock (_threadLock) { return _sensorMessageQueue; } }
            set { lock (_threadLock) { _sensorMessageQueue = value; } }
        }
        private bool _isWorking;
        public bool IsWorking
        {
            get { lock (_threadLock) { return _isWorking; } }
            set { lock (_threadLock) { _isWorking = value; } }
        }

        private Program()
        {
            _threadLock = new object();
            //_sensorMessageQueue = MessageQueue.Create(@".\Private$\newPrivQueue1");

        }

        public void Initialize()
        {
            IsWorking = true;
            RecordRepository recordRepository = new RecordRepository(DbContextFactory.GetContextPerRequest());
            var errors = recordRepository.Save(new ApplicationLib.Record() { NodeId = 1, Channel = "T", Value = 21.5F, DateCreated = DateTime.Now, DateCreatedTicks = DateTime.Now.Ticks });
        }


        private void SensorDataListener()
        {
            while (IsWorking)
            {

            }
        }
    }
}
