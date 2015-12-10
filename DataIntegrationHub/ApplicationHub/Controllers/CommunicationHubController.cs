using ApplicationLib.Entities;
using ApplicationLib.Repositories;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationHub
{
    public class CommunicationHubController
    {
        public static CommunicationHubController Instance;
        public static CommunicationHubController CreateInstance()
        {
            Instance = new CommunicationHubController();
            return Instance;
        }

        private CommunicationHubController()
        {

        }
    }
}
