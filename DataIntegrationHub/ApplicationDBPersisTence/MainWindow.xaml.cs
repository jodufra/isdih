using ApplicationLib.Entities;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Serialization;
using ZeroMQ;

namespace ApplicationDBPersisTence
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static string STOPPED = "Stopped";
        private static string RUNNING = "Running";
        private static string START = "Start";
        private static string STOP = "Stop";
        private Thread thread;

        public MainWindow()
        {
            InitializeComponent();
            CheckZeroMQLibs();
            addressTb.Text = Properties.Settings.Default.IpAddress;
            portTb.Text = Properties.Settings.Default.Port.ToString();
        }

        private void btnStartStop_Click(object sender, RoutedEventArgs e)
        {
            CheckZeroMQLibs();
            CreateSubsriber();
        }

        private void CreateSubsriber()
        {
            if (thread == null)
            {
                btnStartStop.Content = STOP;
                statusLb.Content = RUNNING;
                logLb.Items.Add("Status Changed to: " + RUNNING);
                logLb.Items.Add("Connected to Hub on: " + Properties.Settings.Default.IpAddress + ":" + Properties.Settings.Default.Port);
                DisableDBAndHubSetts(false);
                ThreadStart ts = new ThreadStart(SubscribeAndConsume);
                thread = new Thread(ts);
                thread.Start();
            }
            else
            {
                btnStartStop.Content = START;
                logLb.Items.Add("Status Changed to: " + STOPPED);
                statusLb.Content = STOPPED;
                DisableDBAndHubSetts(true);
                thread.Abort();
                thread = null;
            }
        }

        private void SubscribeAndConsume()
        {
            ZContext context = new ZContext();
            ZSocket subscriber = new ZSocket(context, ZSocketType.SUB);
            subscriber.Connect("tcp://" + Properties.Settings.Default.IpAddress + ":" + Properties.Settings.Default.Port.ToString());
            subscriber.SubscribeAll();
              
            while (true)
            {
                var frame = subscriber.ReceiveFrame();
                //Console.WriteLine(frame.ReadString());

                //XmlDocument doc1 = new XmlDocument();
                //doc1.Load(frame.ReadString());
                //XmlNode _channelExist = doc1.no("channel");

                XmlSerializer serializer = new XmlSerializer(typeof(Record));
                Record record = (Record)serializer.Deserialize(new StringReader(frame.ReadString()));
                this.Dispatcher.Invoke((Action)(() =>
                {
                    logLb.Items.Add(record.ToString());
                }));
                //Console.WriteLine("channel: " + record.Channel + " -> " + record.Value);
                //subscriber.Send(new ZFrame("asd"));

                //var lol = ((DataRowView)((ListView)sender).SelectedItem)["cislo_bytu"].ToString();
                //Console.WriteLine(lol);

            }
        }

        private void DisableDBAndHubSetts(bool action)
        {
            addressTb.IsEnabled = action;
            portTb.IsEnabled = action;
            addressTb.IsEnabled = action;
        }


        private void saveHubSettBtn_Click(object sender, RoutedEventArgs e)
        {
            IPAddress ip;
            if (!String.IsNullOrEmpty(addressTb.Text) && IPAddress.TryParse(addressTb.Text, out ip))
            {
                if (!String.IsNullOrEmpty(portTb.Text))
                {
                    Properties.Settings.Default.IpAddress = addressTb.Text;
                    Properties.Settings.Default.Port = Convert.ToInt32(portTb.Text);
                    Properties.Settings.Default.Save();
                    MessageBox.Show("Hub Connection Saved!");
                }else{
                    MessageBox.Show("Invalid Port!");
                    portTb.Text = Properties.Settings.Default.Port.ToString();
                }
            }
            else
            {
                MessageBox.Show("Empty or Invalid Ip Address!");
                addressTb.Text = Properties.Settings.Default.IpAddress;
            }
                
        }

        private void PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);
        }

        private static bool IsTextAllowed(string text)
        {
            Regex regex = new Regex("[^0-9.-]+");
            return !regex.IsMatch(text);
        }

        private void portTb_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(String)))
            {
                String text = (String)e.DataObject.GetData(typeof(String));
                if (!IsTextAllowed(text))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }

        private void CheckZeroMQLibs()
        {
            try
            {
                ZeroMQ.ZContext.Create();
            }
            catch (Exception e)
            {
                logLb.Items.Add("The ZeroMQ dll's are missing from your windows directory!");
                this.IsEnabled = false;
            }
        }

        private void clearLogBtn_Click(object sender, RoutedEventArgs e)
        {
            logLb.Items.Clear();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Environment.Exit(0);
        }
    }
}
