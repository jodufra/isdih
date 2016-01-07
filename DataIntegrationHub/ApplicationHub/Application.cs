using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ApplicationHub.Models;

namespace ApplicationHub
{
    public class Application
    {
        public static void Main(string[ ] args)
        {
            CommunicationHubController CHController = null;
            bool Ignore = false; // Used to ignore main menu so that on settings we create a refresh effect 
            int OpMM = -1, OpSM = -1; // Opts for menus
            // Global opts
            int Delay = Properties.Settings.Default.Delay, Port = Properties.Settings.Default.Port;
            string IpAddress = Properties.Settings.Default.IpAddress;
            string ZeroMqInitError = null;

            do
            {
                if (Ignore==false)
                    OpMM = MenuMain(CHController, ZeroMqInitError);
                switch (OpMM)
                {
                    case 1:
                        try
                        {
                            ZeroMQ.ZContext.Create();
                        }
                        catch (Exception e)
                        {
                            ZeroMqInitError = e.Message.ToString();
                            break;
                        }

                        if (CHController == null)
                        {
                            CHController = CommunicationHubController.CreateInstance();
                        }
                        else
                        {
                            if(CHController.IsSensorWorking()) CHController.StopSensor(); else CHController.StartSensor();    
                        }

                        break;
                    case 2:
                        OpSM = MenuSettings(Delay, IpAddress, Port);
                        Ignore = false;
                        do{
                            switch (OpSM)
                            {
                                case 1:
                                    bool Convert1 = false;
                                    do
                                    {
                                        Console.Write("->Set Delay (ms): ");
                                        Convert1 = Int32.TryParse(Console.ReadLine(), out Delay);

                                        if (Delay < 1000 || Delay > Int32.MaxValue) Console.WriteLine(">Value must be bigger then 1000 and lower than " + Int32.MaxValue + " !");
                                    } while (Convert1 == false || Delay < 1000 || Delay > Int32.MaxValue);

                                    //Save Setting
                                    Properties.Settings.Default.Delay = Delay;
                                    Properties.Settings.Default.Save();

                                    if (CHController != null && CHController.IsSensorWorking())
                                    {
                                        CHController.SensorReset();
                                    }

                                    Ignore = true;
                                    OpSM = 0;
                                    break;
                                case 2:
                                    IPAddress Ip;
                                    string TempIp;
                                    do
                                    {
                                        Console.Write("->Set Ip Address: ");
                                        TempIp = Console.ReadLine().ToString();

                                        if (!string.IsNullOrEmpty(TempIp) && !IPAddress.TryParse(TempIp, out Ip)) Console.WriteLine(">Invalid Ip Address!");
                                    } while (!string.IsNullOrEmpty(TempIp) && !IPAddress.TryParse(TempIp, out Ip));

                                    //Save Setting
                                    Properties.Settings.Default.IpAddress = TempIp;
                                    Properties.Settings.Default.Save();

                                    if (CHController != null)
                                        CHController.ResetPublisher();
                                    
                                    IpAddress = TempIp;
                                    Ignore = true;
                                    OpSM = 0;
                                    break;
                                case 3:
                                    bool convert2 = false;
                                    do
                                    {
                                        Console.Write("->Set Port number: ");
                                        convert2 = Int32.TryParse(Console.ReadLine(), out Port);
                                    } while (convert2 == false);

                                    //Save Setting
                                    Properties.Settings.Default.Port = Port;
                                    Properties.Settings.Default.Save();

                                    if (CHController != null)
                                        CHController.ResetPublisher();

                                    Ignore = true;
                                    OpSM = 0;
                                    break;

                            }
                        } while (OpSM != 0);
                        
                        break;
                }
            } while (OpMM != 0);

            Environment.Exit(0); // To kill all threads in background correctly
        }

        public static int MenuMain(CommunicationHubController CHController, string zeroMqInitError)
        {
            int op;
            do
            {
                Console.Clear();
                Console.WriteLine("------------- Hub App -------------");
                Console.WriteLine("[1] " + (CHController == null ? "Start" : (CHController.IsSensorWorking() ? "Stop" : "Start")) + " Hub Data Recoding");
                Console.WriteLine("[2] Settings");
                Console.WriteLine("[0] Terminate");
                if (!String.IsNullOrEmpty(zeroMqInitError))
                {
                    Console.WriteLine(zeroMqInitError + " The ZeroMQ dll's are missing, unable to start!");
                }
                Int32.TryParse(Console.ReadLine(), out op);
            } while (op != 1 && op != 2 && op != 0);

            return op;
        }

        private static int MenuSettings(int Delay, string Ip, int Port)
        {
            int op;
            do
            {
                Console.Clear();
                Console.WriteLine("-------- Settings --------");
                Console.WriteLine("[1] Set Delay: " + (Delay == 0 ? "<Not Defined>" : Delay.ToString()));
                Console.WriteLine("[2] Set Ip Address: " + (string.IsNullOrEmpty(Ip) ? "<Not Defined>" : Ip));
                Console.WriteLine("[3] Set Port: " + (Port == 0 ? "<Not Defined>" : Port.ToString()));
                Console.WriteLine("[0] exit");
                Int32.TryParse(Console.ReadLine(), out op);
            } while (op != 1 && op != 2 && op != 3 && /*op != 4 &&*/ op != 0);

            return op;
        }
    }
}
