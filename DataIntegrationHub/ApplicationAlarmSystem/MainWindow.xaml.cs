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

namespace ApplicationAlarmSystem
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MyXmlHandler myxml = new MyXmlHandler("c:\\temp\\alarmsRules.xml","c:\\temp\\alarmsRules.xsd");


        public MainWindow()
        {
            InitializeComponent();
            
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
                var ruls = from c in XElement.Load("c:\\temp\\alarmsRules.xml").Elements("Rule") select c;
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
            System.Diagnostics.Process.Start("c:\\temp\\alarmsRules.xml");
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //myxml.CreateXML();
            myxml.updateRules(comboBoxChannel.SelectedValue.ToString(), int.Parse(txtMin.Text), int.Parse(txtMax.Text));
            updateListView();
           
        }  

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            myxml.deleteRules(comboBoxChannel.SelectedValue.ToString());
            updateListView();
        }

    }
}
