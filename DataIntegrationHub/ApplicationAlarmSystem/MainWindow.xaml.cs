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
using System.Data;

namespace ApplicationAlarmSystem
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MyXmlHandler myxml = new MyXmlHandler(@"alarmsRules.xml",@"alarmsRules.xsd");
        private ZContext context;
        private ZSocket subscriber;

        public MainWindow()
        {
            InitializeComponent();
            
            if (myxml.validateXml())
                updateListView();
            else
            {
                MessageBox.Show("O ficheiro xml Não é válido! O ficheiro XML foi criado!");
                myxml.CreateXML();
                //updateListView();
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
        }


        private void updateListView()
        {
            //Se o ficheiro nao existir, cria um ficheiro XML

            //Carrega as lista para a listView
            List<Rule> RuleList = new List<Rule>();
            RuleList = DAL_OCUSMA.LoadOCUSMA();
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
            }
        }  

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            myxml.deleteRules(comboBoxChannel.SelectedValue.ToString());
            updateListView();
        }

        private void starStopBtn_Click(object sender, RoutedEventArgs e)
        {
            context = new ZContext();
            subscriber = new ZSocket(context, ZSocketType.SUB);

            subscriber.Connect("tcp://" + Properties.Settings.Default.IpAddress + ":" + Properties.Settings.Default.Port.ToString());

            subscriber.SubscribeAll();

            starStopBtn.Content = "Stop";

            while (true)
            {
                var frame = subscriber.ReceiveFrame();
                //Console.WriteLine(frame.ReadString());
                
                //XmlDocument doc1 = new XmlDocument();
                //doc1.Load(frame.ReadString());
                //XmlNode _channelExist = doc1.no("channel");
                
                XmlSerializer serializer = new XmlSerializer(typeof(Record));
                Record record =(Record) serializer.Deserialize(new StringReader(frame.ReadString()));
                Console.WriteLine("channel: " + record.Channel + " -> " + record.Value);

                var lol = ((DataRowView)((ListView)sender).SelectedItem)["cislo_bytu"].ToString();
                Console.WriteLine(lol);
                
                


            }

        }
        public object ExitFrame(object f)
        {
            ((DispatcherFrame)f).Continue = false;

            return null;
        }

        private void txtMin_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!char.IsDigit(e.Text, e.Text.Length - 1))
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
    }
}
