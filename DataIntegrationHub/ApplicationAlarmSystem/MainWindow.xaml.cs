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
        private static string STOPPED = "Stopped";
        private static string RUNNING = "Running";
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

            addressSubTb.Text = Properties.Settings.Default.IpAddressSub;
            portSubTb.Text = Properties.Settings.Default.PortSub.ToString();
            addressPubTb.Text = Properties.Settings.Default.IpAddressPub;
            portPubTb.Text = Properties.Settings.Default.PortPub.ToString();
            
            if (myxml.validateXml())
                updateListView();
            else
            {
                logLst.Items.Add("The XML rules file is not valid!");
                myxml.CreateXML();
                
            }
        }
        
        private class DAL_OCUSMA
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
            if (alarmsRules.SelectedIndex > -1)
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
            try
            {
                System.Diagnostics.Process.Start("notepad", System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"alarmsRules.xml"));
            }
            catch (Exception)
            {
                MessageBox.Show("File not found!");
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
                stateLb.Content = RUNNING;
                logLst.Items.Add("Status Changed to: " + RUNNING);
                logLst.Items.Add("Connected to Hub on: " + Properties.Settings.Default.IpAddressSub + ":" + Properties.Settings.Default.PortSub);
                logLst.Items.Add("Publishing to address: " + Properties.Settings.Default.IpAddressPub + ":" + Properties.Settings.Default.PortPub);
                disableHubConnSetts(false);
                ThreadStart ts = new ThreadStart(ConsumeData);
                threadData = new Thread(ts);
                threadData.Start();
            }
            else
            {
                starStopBtn.Content = START;
                stateLb.Content = STOPPED;
                logLst.Items.Add("Status Changed to: " + STOPPED);
                disableHubConnSetts(true);
                threadData.Abort();
                threadData = null;
                DisposeConnections();
                CancelAction();
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
            dataSubSocket.Connect("tcp://" + Properties.Settings.Default.IpAddressSub + ":" + Properties.Settings.Default.PortSub.ToString());
            dataSubSocket.SubscribeAll();

            while (true)
            {
                try
                {
                    var frame = dataSubSocket.ReceiveFrame();
                    XmlSerializer serializer = new XmlSerializer(typeof(Record));
                    Record record = (Record)serializer.Deserialize(new StringReader(frame.ReadString()));

                    this.Dispatcher.Invoke((Action)(() =>
                    {
                        for (int i = 0; i < listas.Count(); i++)
                        {
                            if (listas[i].Channel.Contains(record.Channel))
                            {
                                if (record.Value < listas[i].Min || record.Value > listas[i].Max)
                                {
                                    Console.WriteLine("Alert Generated on Channel:" + listas[i].Channel + ":" + listas[i].Min + "-" + listas[i].Max + "  Value:" + record.Value);
                                    var alertFrame = new ZFrame("Alert on Channel: " + record.Channel + " with Value: " + record.Value + " | Rules Min: " + listas[i].Min + " Max: " + listas[i].Max); // Create a frame of the Xml
                                    alarmPubSocket.Send(alertFrame);
                                }
                            }
                        }
                    }));
                }
                catch { }
            }
        }

         private void disableHubConnSetts(bool action)
        {
            saveBtn.IsEnabled = action;
            addressSubTb.IsEnabled = action;
            portSubTb.IsEnabled = action;
            addressPubTb.IsEnabled = action;
            portPubTb.IsEnabled = action;
            alarmsRules.IsEnabled = action;
            comboBoxChannel.IsEnabled = action;
            txtMin.IsEnabled = action;
            txtMax.IsEnabled = action;
            btnUpdate.IsEnabled = action;
            btnDelete.IsEnabled = action;
            btnCancel.IsEnabled = action;
            AddBtn.IsEnabled = action;
        }

        public object ExitFrame(object f)
        {
            ((DispatcherFrame)f).Continue = false;

            return null;
        }

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
            if (!String.IsNullOrEmpty(addressPubTb.Text) && IPAddress.TryParse(addressPubTb.Text, out ip))
            {
                if (!String.IsNullOrEmpty(portPubTb.Text))
                {
                    if (!String.IsNullOrEmpty(addressSubTb.Text) && IPAddress.TryParse(addressSubTb.Text, out ip))
                    {
                        if (!String.IsNullOrEmpty(portSubTb.Text))
                        {
                            Properties.Settings.Default.IpAddressSub = addressSubTb.Text;
                            Properties.Settings.Default.PortSub = Convert.ToInt32(portSubTb.Text);
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
                    portPubTb.Text = Properties.Settings.Default.PortSub.ToString();
                }
            }
            else
            {
                MessageBox.Show("Empty or Invalid Publisher Ip Address!");
                addressPubTb.Text = Properties.Settings.Default.IpAddressSub;
            }
        }

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

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(txtMin.Text) || String.IsNullOrEmpty(txtMax.Text) || string.IsNullOrEmpty(comboBoxChannel.Text))
            {
                MessageBox.Show("Empty or Invalid rules!");
            }
            else
            {
                myxml.updateRules(comboBoxChannel.SelectedValue.ToString(), int.Parse(txtMin.Text), int.Parse(txtMax.Text));
                updateListView();
                CancelAction();
            }
        }

        private void CancelAction()
        {
            comboBoxChannel.SelectedValue = null;
            txtMax.Text = "";
            txtMin.Text = "";
            btnUpdate.IsEnabled = false;
            btnDelete.IsEnabled = false;
            btnCancel.IsEnabled = false;
        }

        private void logBtn_Click(object sender, RoutedEventArgs e)
        {
            logLst.Items.Clear();
        }

        private void AddBtn_Click(object sender, RoutedEventArgs e)
        {
            if (listas.Count < 3)
            {
                if (comboBoxChannel.SelectedIndex > 0)
                {
                    if (myxml.checkChannelExists(comboBoxChannel.Text))
                    {
                        MessageBox.Show("Only One Rule Per Channel!");
                    }
                    else
                    {
                        myxml.updateRules(comboBoxChannel.SelectedValue.ToString(), 0, 0);
                        updateListView();
                        CancelAction();
                    }
                }
                else
                {
                    MessageBox.Show("Select a Channel!");
                }
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            CancelAction();
        }
    }
}
