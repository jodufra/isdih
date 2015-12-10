using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationHub
{
    public class Application
    {
        static void Main(string[] args)
        {
            CommunicationHubController programController = CommunicationHubController.CreateInstance();
            SensorDataController dataCollector = SensorDataController.CreateInstance();
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
    }
}
