using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using System.IO;

using System.Xml;
using System.Xml.Schema;
using System.Xml.Linq;
using Microsoft.Win32;
using ZeroMQ;
using System.Windows.Threading;
using ApplicationLib;
using System.Xml.Serialization;
using ApplicationLib.Entities;
using System.Data;
using System.Threading;
using System.Net;

namespace ApplicationAlarmSystem
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ZContext dataSubContext;
        private ZSocket dataSubSocket;
        private ZContext alarmPubContext;
        private ZSocket alarmPubSocket;
        private static string START = "Start";
        private static string STOP = "Stop";
        private MyXmlHandler myxml = new MyXmlHandler(@"alarmsRules.xml",@"alarmsRules.xsd");
        private List<Rule> listas = new List<Rule>();
        private Thread threadData;

        public MainWindow()
        {
            InitializeComponent();
            btnUpdate.IsEnabled = false;
            btnDelete.IsEnabled = false;
            btnCancel.IsEnabled = false;
            

            CheckZeroMQLibs();

            addressTb.Text = Properties.Settings.Default.IpAddress;
            addressBtn.Text = Properties.Settings.Default.IpAddress;
            portBtn.Text = Properties.Settings.Default.Port.ToString();
            portPubTb.Text = Properties.Settings.Default.PortPub.ToString();
            
            if (myxml.validateXml())
                updateListView();
            else
            {
                MessageBox.Show("O ficheiro xml Não é válido! O ficheiro XML foi criado!");
                myxml.CreateXML();
                
            }
        }
        
        class DAL_OCUSMA
        {
        
            public static List<Rule> LoadOCUSMA()
            {
                List<Rule> items = new List<Rule>();
                var ruls = from c in XElement.Load("alarmsRules.xml").Elements("Rule") select c;
                
                foreach (var rules in ruls)
                {                 
                    Rule lRule = new Rule
                    {
                        Channel = rules.Element("channel").Value,
                        Min = int.Parse(rules.Element("min").Value),
                        Max = int.Parse(rules.Element("max").Value)  
                    };
                    items.Add(lRule);
                    //Console.WriteLine(items[0].Channel);
                    
                }
                return items;
            }

        }

        public class Rule
        {
            public string Channel { get; set; }
            public int Min { get; set; }
            public int Max { get; set; }
        }

        private void alarmsRules_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var selectedItem = (dynamic)alarmsRules.SelectedItems[0];
            comboBoxChannel.SelectedValue = selectedItem.Channel;
            txtMax.Text = Convert.ToString(selectedItem.Max);
            txtMin.Text = Convert.ToString(selectedItem.Min);
            //MessageBox.Show(selectedItem.Channel);
            btnUpdate.IsEnabled = true;
            btnDelete.IsEnabled = true;
            btnCancel.IsEnabled = true;

        }

        private void updateListView()
        {
            //Se o ficheiro nao existir, cria um ficheiro XML

            //Carrega as lista para a listView
            List<Rule> RuleList = new List<Rule>();
            RuleList = DAL_OCUSMA.LoadOCUSMA();
            listas = RuleList;
            alarmsRules.ItemsSource = RuleList;

        }

        private void btnXML_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(@"alarmsRules.xml");
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(txtMin.Text) || String.IsNullOrEmpty(txtMax.Text) || string.IsNullOrEmpty(comboBoxChannel.Text))
            {
                MessageBox.Show("Por favor preenche.");               
            }
            else {           
            myxml.updateRules(comboBoxChannel.SelectedValue.ToString(), int.Parse(txtMin.Text), int.Parse(txtMax.Text));
            updateListView();
            comboBoxChannel.SelectedValue = null;
            txtMax.Text = "";
            txtMin.Text = "";
            btnUpdate.IsEnabled = false;
            btnDelete.IsEnabled = false;
            btnCancel.IsEnabled = false;
            }
        }  

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            myxml.deleteRules(comboBoxChannel.SelectedValue.ToString());
            updateListView();
            comboBoxChannel.SelectedValue = null;
            txtMax.Text = "";
            txtMin.Text = "";
            btnUpdate.IsEnabled = false;
            btnDelete.IsEnabled = false;
            btnCancel.IsEnabled = false;
        }

        private void starStopBtn_Click(object sender, RoutedEventArgs e)
        {
            CheckZeroMQLibs();
            CreateSubscriberAndPublisher();
        }

        private void CreateSubscriberAndPublisher()
        {
            if (threadData == null)
            {
                CreateAlarmPublisher();
                starStopBtn.Content = STOP;
                disableHubConnSetts(false);
                ThreadStart ts = new ThreadStart(ConsumeData);
                threadData = new Thread(ts);
                threadData.Start();
            }
            else
            {
                starStopBtn.Content = START;
                disableHubConnSetts(true);
                threadData.Abort();
                threadData = null;
                DisposeConnections();
            }
        }

        private void DisposeConnections()
        {
            alarmPubSocket.Dispose();
            alarmPubContext.Dispose();
            dataSubSocket.Dispose();
            dataSubContext.Dispose();  
        }

        private void CreateAlarmPublisher()
        {
            alarmPubContext = new ZContext();
            alarmPubSocket = new ZSocket(alarmPubContext, ZSocketType.PUB);
            alarmPubSocket.Bind("tcp://" + Properties.Settings.Default.IpAddressPub + ":" + Properties.Settings.Default.PortPub);
        }

        private void ConsumeData()
        {
            dataSubContext = new ZContext();
            dataSubSocket = new ZSocket(dataSubContext, ZSocketType.SUB);
            dataSubSocket.Connect("tcp://" + Properties.Settings.Default.IpAddress + ":" + Properties.Settings.Default.Port.ToString());
            dataSubSocket.SubscribeAll();

            while (true)
            {
                try
                {
                    var frame = dataSubSocket.ReceiveFrame();
                    //Console.WriteLine(frame.ReadString());
                    //XmlDocument doc1 = new XmlDocument();
                    //doc1.Load(frame.ReadString());
                    //XmlNode _channelExist = doc1.no("channel");
                    XmlSerializer serializer = new XmlSerializer(typeof(Record));
                    Record record = (Record)serializer.Deserialize(new StringReader(frame.ReadString()));

                    this.Dispatcher.Invoke((Action)(() =>
                    {
                        logLst.Items.Add(record.ToString());
                        var zFrame = new ZFrame("Alarm!!!"); // Create a frame of the Xml
                        logLst.Items.Add(zFrame.ToString());
                        alarmPubSocket.Send(zFrame); //Send the Xml to subs
                    }));
                    //Console.WriteLine("channel: " + record.Channel + " -> " + record.Value);
                    //subscriber.Send(new ZFrame("asd"));


                XmlSerializer serializer = new XmlSerializer(typeof(Record));
                Record record = (Record)serializer.Deserialize(new StringReader(frame.ReadString()));
                //Console.WriteLine("channel: " + record.Channel + " -> " + record.Value);
                //subscriber.Send(new ZFrame("asd"));

                //var lol = ((DataRowView)((ListView)sender).SelectedItem)["cislo_bytu"].ToString();
                //Console.WriteLine(lol);

                for (int i = 0; i < listas.Count(); i++)
                {
                    if (listas[i].Channel.Contains(record.Channel))
                        if (listas[i].Min>record.Value||record.Value>listas[i].Max)
                        {
                            Console.WriteLine("Alerta canal:"+ listas[i].Channel + ":"+ listas[i].Min + "-" + listas[i].Max + "  Value:" + record.Value);
                        }
                        else
                        {
                            Console.WriteLine("Canal:"+listas[i].Channel+"OK!");
                        }
                }

            }
        }

         private void disableHubConnSetts(bool action)
        {
            saveBtn.IsEnabled = action;
            addressTb.IsEnabled = action;
            portTb.IsEnabled = action;
            addressPubTb.IsEnabled = action;
            portPubTb.IsEnabled = action;
        }

        public object ExitFrame(object f)
        {
            ((DispatcherFrame)f).Continue = false;

            return null;
        }

        //verify if the digit isn't a integer
        private void txtMin_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!char.IsDigit(e.Text, e.Text.Length - 1) )
            {
                e.Handled = true;
            }
        }

        
        private void txtMax_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!char.IsDigit(e.Text, e.Text.Length - 1))
            {
                e.Handled = true;
            }
        }

        private void saveBtn_Click(object sender, RoutedEventArgs e)
        {
            IPAddress ip;
            if (!String.IsNullOrEmpty(addressTb.Text) && IPAddress.TryParse(addressTb.Text, out ip))
            {
                if (!String.IsNullOrEmpty(portTb.Text))
                {
                    if (!String.IsNullOrEmpty(addressPubTb.Text) && IPAddress.TryParse(addressPubTb.Text, out ip))
                    {
                        if (!String.IsNullOrEmpty(portPubTb.Text))
                        {
                            Properties.Settings.Default.IpAddress = addressTb.Text;
                            Properties.Settings.Default.Port = Convert.ToInt32(portTb.Text);
                            Properties.Settings.Default.IpAddressPub = addressPubTb.Text;
                            Properties.Settings.Default.PortPub = Convert.ToInt32(portPubTb.Text);
                            Properties.Settings.Default.Save();
                            MessageBox.Show("Hub Connections Saved!");
                        }
                        else
                        {
                            MessageBox.Show("Invalid Subscriver Port!");
                            portPubTb.Text = Properties.Settings.Default.PortPub.ToString();
                        }
                    }
                    else
                    {
                        MessageBox.Show("Empty or Invalid Subscriber Ip Address!");
                        addressPubTb.Text = Properties.Settings.Default.IpAddressPub;
                    }
                }
                else
                {
                    MessageBox.Show("Invalid Publisher Port!");
                    portTb.Text = Properties.Settings.Default.Port.ToString();
                }
            }
            else
            {
                MessageBox.Show("Empty or Invalid Publisher Ip Address!");
                addressTb.Text = Properties.Settings.Default.IpAddress;
            }
        }

        //restrict the copy+cut+paste
        private void HandleCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {

            if (e.Command == ApplicationCommands.Cut ||
                 e.Command == ApplicationCommands.Copy ||
                 e.Command == ApplicationCommands.Paste)
            {
                e.CanExecute = false;
                e.Handled = true;
            }

        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            comboBoxChannel.SelectedValue = null;
            txtMax.Text = "";
            txtMin.Text = "";
            btnUpdate.IsEnabled = false;
            btnDelete.IsEnabled = false;
            btnCancel.IsEnabled = false;
        }

        private void comboBoxChannel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            btnUpdate.IsEnabled = true;
            btnDelete.IsEnabled = true;
            btnCancel.IsEnabled = true;
        }

         private void CheckZeroMQLibs()
        {
            try
            {
                ZeroMQ.ZContext.Create();
            }
            catch (Exception e)
            {
                logLst.Items.Add("The ZeroMQ dll's are missing from your windows directory!");
                this.IsEnabled = false;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Environment.Exit(0);
        }
    }
}
