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
using ApplicationHub.Models;

namespace ApplicationHub
{
    public class CommunicationHubController
    {
        public static CommunicationHubController Instance;
        private ZContext Context;
        private ZSocket PublisherSocket;

        public static CommunicationHubController CreateInstance()
        {
            Instance = new CommunicationHubController();
            return Instance;
        }

        private CommunicationHubController()
        {
            Context = new ZContext();
            PublisherSocket = new ZSocket(Context, ZSocketType.PUB);
            PublisherSocket.Bind("tcp://" + Properties.Settings.Default.IpAddress + ":" + Properties.Settings.Default.Port);

            SensorDataController DataCollector = SensorDataController.CreateInstance();

            //Settings.Instance.IsWorking
            while (true)
            {

            }
            
            
            
            
            
            //check if conf file exists
            //Yes
            //read ops
            //check if ok value
            //If yes
            //set value and start reading values and send
            //no
            //say "unable to read delay value", ask for delay and save
            //not ok value
            //say "not defined", ask for value
            //no 
            //create file
            //ask for value 
            //ask for port
            //save
            //
            //criar opções para o user
            //definir delay
            //defenir email (para o final)

            //Aplicar Lib ZeroMQ
            //Criar instancia
            //definir porta??? ip??
            //Apanhar os dados do sensor
            //estar a enviar para os subscribers
            //criar um fichiero novo cada dia com os dados
            //Comando para terminar a app
        }

        

        public void OnSensorDataReceived(Record record)
        {
            Console.WriteLine(record.Log);
        }
    }
}
