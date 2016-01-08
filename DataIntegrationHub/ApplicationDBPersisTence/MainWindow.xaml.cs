using ApplicationLib.Entities;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
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
        public MainWindow()
        {
            InitializeComponent();
            addressTb.Text = Properties.Settings.Default.IpAddress;
            portTb.Text = Properties.Settings.Default.Port.ToString();
        }

        private void btnStartStop_Click(object sender, RoutedEventArgs e)
        {
            logRtb.AppendText("Status Changed to: Running" + Environment.NewLine);
            ZContext context = new ZContext();
            ZSocket subscriber = new ZSocket(context, ZSocketType.SUB);

            subscriber.Connect("tcp://" + Properties.Settings.Default.IpAddress + ":" + Properties.Settings.Default.Port.ToString());

            subscriber.SubscribeAll();

            btnStartStop.Content = "Stop";

            while (true)
            {
                var frame = subscriber.ReceiveFrame();
                //Console.WriteLine(frame.ReadString());

                //XmlDocument doc1 = new XmlDocument();
                //doc1.Load(frame.ReadString());
                //XmlNode _channelExist = doc1.no("channel");

                XmlSerializer serializer = new XmlSerializer(typeof(Record));
                Record record = (Record)serializer.Deserialize(new StringReader(frame.ReadString()));
                Console.WriteLine("channel: " + record.Channel + " -> " + record.Value);
                //subscriber.Send(new ZFrame("asd"));

                //var lol = ((DataRowView)((ListView)sender).SelectedItem)["cislo_bytu"].ToString();
                //Console.WriteLine(lol);


            }
            
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
    }
}
