using ApplicationLib;
using ApplicationLib.Entities;
using ApplicationLib.Repositories;
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
        private XmlDocument Doc;
        private String XmlName;
        private String CurrentDate;
        private XmlElement Root;
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

            CurrentDate = DateTime.Now.ToString("dd-MM-yy");
            XmlName = "DataHub_" + CurrentDate + ".xml";

            if (!File.Exists(@XmlName))
            {
                XmlDocCreate();
            }
            else
            {
                XmlDocRead();
            }

            
            //Apanhar os dados do sensor
            //estar a enviar para os subscribers
            //criar um ficheiro novo cada dia com os dados
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

                var _ZFrame = new ZFrame(XmlString); // Create a frame of the Xml
                PublisherSocket.Send(_ZFrame); //Send the Xml to subs

                //Save to Xml File Daily
                if(Doc != null){
                    XmlElement NewRecord = Doc.CreateElement("Record");
                    NewRecord.SetAttribute("IdRecord", record.IdRecord.ToString());
                    NewRecord.SetAttribute("NodeId", record.NodeId.ToString());
                    NewRecord.SetAttribute("Channel", record.Channel.ToString());
                    NewRecord.SetAttribute("DateCreated", record.DateCreated.ToString());
                    NewRecord.SetAttribute("DateCreatedTicks", record.DateCreatedTicks.ToString());
                    NewRecord.SetAttribute("Value", record.Value.ToString());
                    //Root.AppendChild(NewRecord);
                    Doc.Save(@XmlName);
                }
                

                //Clear Mem ??
                _ZFrame.Close();
                _ZFrame.Dispose();
                XmlWriter.Close();
                XmlWriter.Dispose();
                SWriter.Close();
                SWriter.Dispose();

                //Console.WriteLine(XmlString);
                //Console.WriteLine("Receibed from sensor: " + record.Log);
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
            /*if(Context != null){
                Context.Dispose();
                PublisherSocket.Dispose();
            }*/
            Context = new ZContext();
            PublisherSocket = new ZSocket(Context, ZSocketType.PUB);
            PublisherSocket.Bind("tcp://" + Properties.Settings.Default.IpAddress + ":" + Properties.Settings.Default.Port);
        }

        public void ResetPublisher()
        {
            CreatePublisher();
        }

        private void XmlDocCreate()
        {
            try
            {
                Doc = new XmlDocument();
                XmlDeclaration dec = Doc.CreateXmlDeclaration("1.0", "utf-16", null);
                Doc.AppendChild(dec);
                Root = Doc.CreateElement("Records");
                Doc.AppendChild(Root);
            }
            catch (Exception)
            {
                Doc = null;
            }
          
        }

        private void XmlDocRead()
        {
            try
            {
                Doc = new XmlDocument();
                Doc.Load(@XmlName);
              
            }
            catch
            {
                Doc = null;
            }
        }

        //--
        protected virtual void Dispose(bool Disposing)
        {
            if (!Disposed)
            {
                if (Disposing)
                {
                    Context.Dispose();
                    PublisherSocket.Dispose();
                    SDataCollector.Dispose();
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
