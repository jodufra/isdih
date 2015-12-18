using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationHub
{
    public class Application
    {
        public static void Main(string[ ] args)
        {
            CommunicationHubController ProgramController;
            bool Ignore = false; // Used to ignore main menu so that on settings we create a refresh effect 
            int OpMM = -1, OpSM = -1; // Opts for menus
            // Global opts
            int Delay = Properties.Settings.Default.Delay, Port = Properties.Settings.Default.Port;
            string IpAddress = Properties.Settings.Default.IpAddress;
            bool Startup = Properties.Settings.Default.AutoStart;
 
            do
            {
                if (Ignore==false)
                    OpMM = MenuMain();
                switch (OpMM)
                {
                    case 1:
                        //ainda n sei bem
                        ProgramController = CommunicationHubController.CreateInstance();
                        
                        break;
                    case 2:
                        OpSM = MenuSettings(Delay, IpAddress, Port, Startup);
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
                                        //fazer verificação
                                    } while (convert2 == false);

                                    //Save Setting
                                    Properties.Settings.Default.Port = Port;
                                    Properties.Settings.Default.Save();

                                    Ignore = true;
                                    OpSM = 0;
                                    break;
                                case 4:
                                    string temp;
                                    do
                                    {
                                        Console.Write(Environment.NewLine + "->Set Auto Start On Startup (y/n): ");
                                        temp = Console.ReadKey().KeyChar.ToString().ToLower();
                                     
                                        if (!temp.Equals("n") && !temp.Equals("y")) Console.WriteLine(Environment.NewLine + ">Value must be y or n!");
                                    } while (!temp.Equals("n") && !temp.Equals("y"));

                                    //Save Setting
                                    Properties.Settings.Default.AutoStart = (temp.Equals("y"))? true : false;
                                    Properties.Settings.Default.Save();
                                    Startup = (temp.Equals("y")) ? true : false;

                                    Ignore = true;
                                    OpSM = 0;
                                    break;

                            }
                        } while (OpSM != 0);
                        break;
                }
            } while (OpMM != 0);   
        }

        public static int MenuMain()
        {
            int op;
            do
            {
                Console.Clear();
                Console.WriteLine("------------- Hub App -------------");
                Console.WriteLine("[1] Start Recoding Sensor Data");
                Console.WriteLine("[2] Settings");
                Console.WriteLine("[0] Terminate");
                Int32.TryParse(Console.ReadLine(), out op);
            } while (op != 1 && op != 2 && op != 0);

            return op;
        }

        private static int MenuSettings(int Delay, string Ip, int Port, bool Startup)
        {
            int op;
            do
            {
                Console.Clear();
                Console.WriteLine("-------- Settings --------");
                Console.WriteLine("[1] Set Delay: " + (Delay == 0 ? "<Not Defined>" : Delay.ToString()));
                Console.WriteLine("[2] Set Ip Address: " + (string.IsNullOrEmpty(Ip) ? "<Not Defined>" : Ip));
                Console.WriteLine("[3] Set Port: " + (Port == 0 ? "<Not Defined>" : Port.ToString()));
                Console.WriteLine("[4] Set Auto Start On Startup: " + (Startup.Equals(' ') ? "<Not Defined>" : ((Startup) ? "Yes" : "No")));
                Console.WriteLine("[0] exit");
                Int32.TryParse(Console.ReadLine(), out op);
            } while (op != 1 && op != 2 && op != 3 && op != 4 && op != 0);

            return op;
        }
    }
}
