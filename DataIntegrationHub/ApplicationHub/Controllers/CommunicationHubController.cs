using ApplicationLib;
using ApplicationLib.Entities;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Threading.Tasks;
using ZeroMQ;
using System.Xml.Serialization;
using System.Xml;
using System.Threading;

namespace ApplicationHub
{
    public class CommunicationHubController : IDisposable
    {
        public static CommunicationHubController Instance;
        private ZContext dataPubContext;
        private ZSocket dataPubSocket;
        private ZContext alarmSubContext;
        private ZSocket alarmSubSocket;
        private SensorDataController sDataCollector;
        private bool disposed;
        private Thread thread;

        public static CommunicationHubController CreateInstance()
        {
            Instance = new CommunicationHubController();
            return Instance;
        }

        private CommunicationHubController()
        { 
            CreatePublisher();
            CreateSubscriver();
            sDataCollector = SensorDataController.CreateInstance();
        }

        private void CreatePublisher()
        {
            dataPubContext = new ZContext();
            dataPubSocket = new ZSocket(dataPubContext, ZSocketType.PUB);
            dataPubSocket.Bind("tcp://" + Properties.Settings.Default.IpAddress + ":" + Properties.Settings.Default.Port);
        }

        private void CreateSubscriver()
        {
            ThreadStart ts = new ThreadStart(ConsumeAlarms);
            thread = new Thread(ts);
            thread.Start();
        }

        private void ConsumeAlarms()
        {
            alarmSubContext = new ZContext();
            alarmSubSocket = new ZSocket(alarmSubContext, ZSocketType.SUB);
            alarmSubSocket.Connect("tcp://" + Properties.Settings.Default.IpAddressSub + ":" + Properties.Settings.Default.PortSub);
            alarmSubSocket.SubscribeAll();

            while(true){
                try {
                    var alarmFrame = alarmSubSocket.ReceiveFrame(); //Receiving alarm, but no use!
                    Console.WriteLine(alarmFrame.ToString()); //Debug ###Remover
                }
                catch { }
            }
        }

        public void OnSensorDataReceived(Record record)
        {
            if (dataPubContext != null)
            {
                try { 
                    //Record is converted into Xml to be sent to Subscribers 
                    XmlSerializer Serializer = new XmlSerializer(typeof(Record));
                    StringWriter SWriter = new StringWriter();
                    XmlWriter XmlWriter = XmlWriter.Create(SWriter);
                    XmlSerializerNamespaces Ns = new XmlSerializerNamespaces();
                    Ns.Add("", "");
                    Record RecordToSend = record;
                    Serializer.Serialize(SWriter, RecordToSend, Ns);
                    var XmlString = SWriter.ToString(); // Record To XML

                    var zFrame = new ZFrame(XmlString); // Create a frame of the Xml
                    dataPubSocket.Send(zFrame); //Send the Xml to subs
               
                    //Console.WriteLine(XmlString); //Debug ###Remover
                    XmlWriter.Close();
                    XmlWriter.Dispose();
                    SWriter.Close();
                    SWriter.Dispose();
                    zFrame.Dispose();
                }
                catch { }
            }
        }

        public bool StopSensor()
        {
            thread.Abort();
            thread = null;
            DisposeConnections();
            return sDataCollector.StopStart();
        }

        public bool StartSensor()
        {
            CreatePublisher();
            CreateSubscriver();
            return sDataCollector.StopStart();
        }

        private void DisposeConnections()
        {
            dataPubSocket.Dispose();
            dataPubContext.Dispose();
            alarmSubSocket.Dispose();
            alarmSubContext.Dispose();
        }

        public bool IsSensorWorking()
        {
            return sDataCollector.GetState();
        }

        public void SensorReset()
        {
            sDataCollector = SensorDataController.CreateInstance();
            sDataCollector.Dispose();   
        }

        public void ResetPublisher()
        {
            CreatePublisher();
        }

        protected virtual void Dispose(bool Disposing)
        {
            if (!disposed)
            {
                if (Disposing)
                {
                    dataPubContext.Dispose();
                    dataPubSocket.Dispose();
                    alarmSubContext.Dispose();
                    alarmSubSocket.Dispose();
                    sDataCollector.Dispose();
                   /*_ZFrame.Close();
                   _ZFrame.Dispose();
                   XmlWriter.Close();
                   XmlWriter.Dispose();
                   SWriter.Close();
                   SWriter.Dispose();*/
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
