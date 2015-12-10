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
        }
    }
}
