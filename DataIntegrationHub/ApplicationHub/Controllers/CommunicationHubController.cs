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
        private ZContext Context;
        private ZSocket PublisherSocket;
        private SensorDataController SDataCollector;
        //private XmlDocument Doc;
        //private String XmlName;
        //private String CurrentDate;
        //private XmlElement Root;
        private bool Disposed;


        public static CommunicationHubController CreateInstance()
        {
            Instance = new CommunicationHubController();
            return Instance;
        }

        private CommunicationHubController()
        { 
            CreatePublisher();
            SDataCollector = SensorDataController.CreateInstance();
        }

        public void OnSensorDataReceived(Record record)
        {
            if (Context != null)
            {
                //Record is converted into Xml to be sent to Subscribers 
                XmlSerializer Serializer = new XmlSerializer(typeof(Record));
                StringWriter SWriter = new StringWriter();
                XmlWriter XmlWriter = XmlWriter.Create(SWriter);
                XmlSerializerNamespaces Ns = new XmlSerializerNamespaces();
                Ns.Add("", "");
                Record RecordToSend = record;
                Serializer.Serialize(SWriter, RecordToSend, Ns);
                var XmlString = SWriter.ToString(); // Record To XML

                var ZFrame = new ZFrame(XmlString); // Create a frame of the Xml
                PublisherSocket.Send(ZFrame); //Send the Xml to subs
               
                Console.WriteLine(XmlString); //Debug
                //Console.WriteLine("Receibed from sensor: " + record.Log);
                XmlWriter.Close();
                XmlWriter.Dispose();
                SWriter.Close();
                SWriter.Dispose();
            }
        }

        public bool StopSensor()
        {
            return SDataCollector.StopStart();
        }

        public bool StartSensor()
        {
            return SDataCollector.StopStart();
        }

        public bool IsSensorWorking()
        {
            return SDataCollector.GetState();
        }


        public void SensorReset()
        {
            SDataCollector.Dispose();
            SDataCollector = SensorDataController.CreateInstance();
        }

        protected void CreatePublisher()
        {
            Context = new ZContext();
            PublisherSocket = new ZSocket(Context, ZSocketType.PUB);
            PublisherSocket.Bind("tcp://" + Properties.Settings.Default.IpAddress + ":" + Properties.Settings.Default.Port);
            
        }

        public void ResetPublisher()
        {
            CreatePublisher();
        }

        protected virtual void Dispose(bool Disposing)
        {
            if (!Disposed)
            {
                if (Disposing)
                {
                    Context.Dispose();
                    PublisherSocket.Dispose();
                    SDataCollector.Dispose();

                   /*_ZFrame.Close();
                   _ZFrame.Dispose();
                   XmlWriter.Close();
                   XmlWriter.Dispose();
                   SWriter.Close();
                   SWriter.Dispose();*/
                }

            }
            Disposed = true;
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
