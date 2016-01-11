using ApplicationDbLibrary.Entities;
using ApplicationDbLibrary.Repositories;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
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
using System.Xml;
using System.Xml.Schema;
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
        private static string STATISTICS = "Statistics";
        private string aysPath;
        private string amsPath;
        private string adsPath;
        private List<string> filesToUpdateLst;
        private Thread scThread;
        private string statisticsPath;
        private CancellationTokenSource cts;

        public MainWindow()
        {
            InitializeComponent();
            CheckZeroMQLibs();
            statisticsPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @STATISTICS);

            addressTb.Text = Properties.Settings.Default.IpAddress;
            portTb.Text = Properties.Settings.Default.Port.ToString();

            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None); // open
            var connectionStringsSection = (ConnectionStringsSection)config.GetSection("connectionStrings"); // get cs
            if (String.IsNullOrEmpty(Properties.Settings.Default.ConnectionStringChanged))
            {
                connectionStringsSection.ConnectionStrings["Connection"].ConnectionString = Properties.Settings.Default.ConnectionStringOriginal;
            }
            else
            {
                connectionStringsSection.ConnectionStrings["Connection"].ConnectionString = Properties.Settings.Default.ConnectionStringChanged;
            }

            config.Save();
            ConfigurationManager.RefreshSection("connectionStrings");
            csTb.Text = connectionStringsSection.ConnectionStrings["Connection"].ConnectionString; // show

        }

        private void btnStartStop_Click(object sender, RoutedEventArgs e)
        {
            CheckZeroMQLibs();
            CreateSubsriber();
        }

        private void CreateSubsriber()
        {
            if (scThread == null)
            {
                btnStartStop.Content = STOP;
                statusLb.Content = RUNNING;
                logLb.Items.Add("Status Changed to: " + RUNNING);
                logLb.Items.Add("Connected to Hub on: " + Properties.Settings.Default.IpAddress + ":" + Properties.Settings.Default.Port);
                DisableDBAndHubSetts(false);
                ThreadStart ts = new ThreadStart(SubscribeAndConsume);
                scThread = new Thread(ts);
                scThread.Start();
                StatisticalData();
            }
            else
            {
                btnStartStop.Content = START;
                logLb.Items.Add("Status Changed to: " + STOPPED);
                statusLb.Content = STOPPED;
                DisableDBAndHubSetts(true);
                scThread.Abort();
                scThread = null;
                if (cts != null)
                    cts.Cancel();
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
                XmlSerializer serializer = new XmlSerializer(typeof(Record));
                Record record = (Record)serializer.Deserialize(new StringReader(frame.ReadString()));
                try
                {
                    this.Dispatcher.Invoke((Action)(() =>
                    {
                        logLb.Items.Add(record.Log);
                        if (logLb.Items.Count >= Int16.MaxValue)
                            logLb.Items.Clear();
                    }));
                    RecordRepository.Save(record);
                }
                catch (Exception ex)
                {
                    this.Dispatcher.Invoke((Action)(() =>
                    {
                        logLb.Items.Add(ex.Message);
                    }));
                }
            }
        }

        private void DisableDBAndHubSetts(bool action)
        {
            addressTb.IsEnabled = action;
            portTb.IsEnabled = action;
            addressTb.IsEnabled = action;
            testBtn.IsEnabled = action;
            resetBtn.IsEnabled = action;
            saveBtn.IsEnabled = false;
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
                }
                else
                {
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

        private void testBtn_Click(object sender, RoutedEventArgs e)
        {
            string provider = "MySql.Data.MySqlClient";
            DbProviderFactory factory = DbProviderFactories.GetFactory(provider);
            using (DbConnection conn = factory.CreateConnection())
            {
                try
                {
                    conn.ConnectionString = csTb.Text;
                    conn.Open();
                    MessageBox.Show("Connection OK!");
                    saveBtn.IsEnabled = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    saveBtn.IsEnabled = false;
                }


            }
        }

        private void saveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("The application will have to restart, are you shure?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
            {
                //no
                if (String.IsNullOrEmpty(Properties.Settings.Default.ConnectionStringChanged))
                {
                    csTb.Text = Properties.Settings.Default.ConnectionStringOriginal;
                }
                else
                {
                    csTb.Text = Properties.Settings.Default.ConnectionStringChanged;
                }
                saveBtn.IsEnabled = false;
            }
            else
            {
                //yes
                Properties.Settings.Default.ConnectionStringChanged = csTb.Text;
                Properties.Settings.Default.Save();
                Properties.Settings.Default.Reload();
                AppRestart();
            }
        }

        private void AppRestart()
        {
            System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
            Environment.Exit(Environment.ExitCode);
        }

        private void csTb_TextChanged(object sender, TextChangedEventArgs e)
        {
            saveBtn.IsEnabled = false;
        }

        private void resetBtn_Click(object sender, RoutedEventArgs e)
        {
            if (csTb.Text.Equals(Properties.Settings.Default.ConnectionStringOriginal))
            {
                MessageBox.Show("Same Connection String as the original!");
            }
            else
            {
                if (MessageBox.Show("The application will have to restart, are you shure?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    Properties.Settings.Default.ConnectionStringChanged = "";
                    Properties.Settings.Default.Save();
                    Properties.Settings.Default.Reload();
                    AppRestart();
                }
            }
        }

        private void openStatFolderBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!Directory.Exists(statisticsPath)) Directory.CreateDirectory(statisticsPath);
                System.Diagnostics.Process.Start("explorer", @statisticsPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void StatisticalData()
        {
            //Init Scheduler
            var dateNow = DateTime.Now;
            var date = new DateTime(dateNow.Year, dateNow.Month, dateNow.Day + 1, 0, 1, 0);
            filesToUpdateLst = new List<string>();
            executeAt(date);
        }

        private void executeAt(DateTime date)
        {
            cts = new CancellationTokenSource();
            var dateNow = DateTime.Now;
            TimeSpan ts;
            if (date > dateNow)
                ts = date - dateNow;
            else
            {
                date = date.AddMinutes(1);
                ts = date - dateNow;
            }

            aysPath = @System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, STATISTICS, DateTime.Now.Year.ToString() + "\\all_" + DateTime.Now.Year.ToString() + "_stats.xml");
            amsPath = @System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, STATISTICS, DateTime.Now.Year.ToString() + "\\" + DateTime.Now.ToString("MMMM", CultureInfo.InvariantCulture) + "\\all_" + DateTime.Now.ToString("MMMM", CultureInfo.InvariantCulture) + "_stats.xml");
            adsPath = @System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, STATISTICS, DateTime.Now.Year.ToString() + "\\" + DateTime.Now.ToString("MMMM", CultureInfo.InvariantCulture) + "\\day_" + DateTime.Now.Day + "_stats.xml");
            filesToUpdateLst.Clear();
            filesToUpdateLst.Add(aysPath);
            filesToUpdateLst.Add(amsPath);
            filesToUpdateLst.Add(adsPath);

            Task.Delay(ts).ContinueWith((x) =>
            {
                this.Dispatcher.Invoke((Action)(() =>
                {
                    logLb.Items.Add("Saving statistics of " + DateTime.Now.AddDays(-1).ToShortDateString());
                }));
                UpdateAllStatistics();
                executeAt(new DateTime(dateNow.Year, dateNow.Month, dateNow.Day + 1, 0, 1, 0));
            }, cts.Token);
        }

        private void UpdateAllStatistics()
        {
            if (ValidateFolderStructure() && ValidateFiles())
            {
                UpdateFiles();
                this.Dispatcher.Invoke((Action)(() =>
                {
                    logLb.Items.Add("Statistics were saved");
                }));
            }
        }

        private void UpdateFiles()
        {
            var options = new Dictionary<FilterOptionRecord, Object>();

            int count;
            var now = DateTime.Now;
            options.Add(FilterOptionRecord.DateCreatedMin, new DateTime(now.Year, now.Month, now.Day - 1));
            options.Add(FilterOptionRecord.DateCreatedMax, new DateTime(now.Year, now.Month, now.Day).AddTicks(-1));
            List<Record> lst = RecordRepository.Get(null, null, null, out count, OrderOptionRecord.DateCreatedAsc, options);

            var min = lst.GroupBy(p => p.Channel).Select(p => new { Channel = p.Key, Value = p.Min(c => (float?)c.Value) });
            var avg = lst.GroupBy(p => p.Channel).Select(p => new { Channel = p.Key, Value = p.Average(c => (float?)c.Value) });
            var max = lst.GroupBy(p => p.Channel).Select(p => new { Channel = p.Key, Value = p.Max(c => (float?)c.Value) });

            foreach (var file in filesToUpdateLst)
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(file);
                XmlNodeList statis = doc.SelectNodes("/statistics");

                foreach (var channel in avg.Where(p => p.Value.HasValue).Select(p => p.Channel))
                {
                    XmlElement statistic = doc.CreateElement("statistic");
                    statistic.SetAttribute("date", new DateTime(now.Year, now.Month, now.Day - 1).ToShortDateString());
                    statistic.SetAttribute("channel", channel);
                    statistic.SetAttribute("min", min.Where(p => p.Channel == channel).Select(p => p.Value.Value).First().ToString());
                    statistic.SetAttribute("avg", avg.Where(p => p.Channel == channel).Select(p => p.Value.Value).First().ToString());
                    statistic.SetAttribute("max", max.Where(p => p.Channel == channel).Select(p => p.Value.Value).First().ToString());
                    statis[0].AppendChild(statistic);
                }
                doc.Save(file);
            }
        }

        private bool ValidateFiles()
        {
            try
            {
                if (!ValidateFile(aysPath))
                    throw new Exception();

                if (!ValidateFile(amsPath))
                    throw new Exception();

                if (!ValidateFile(adsPath))
                    throw new Exception();

                //all_year_stats.xml
                //string aysPath = @System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, STATISTICS, DateTime.Now.Year.ToString() + "\\all_" + DateTime.Now.Year.ToString() + "_stats.xml");
                //all_month_stats.xml
                //string amsPath = @System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, STATISTICS, DateTime.Now.Year.ToString() + "\\" + DateTime.Now.ToString("MMMM", CultureInfo.InvariantCulture) + "\\all_" + DateTime.Now.ToString("MMMM", CultureInfo.InvariantCulture) + "_stats.xml");
                //day_dayNum_stats.xml
                //string adsPath = @System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, STATISTICS, DateTime.Now.Year.ToString() + "\\" + DateTime.Now.ToString("MMMM", CultureInfo.InvariantCulture) + "\\day_" + DateTime.Now.Day + "_stats.xml");

                return true;
            }
            catch (Exception)
            {
                logLb.Items.Add("Unable to Create or Access The Statistics Files!");
                return false;
            }
        }

        private bool ValidateFile(String filename)
        {
            if (!File.Exists(filename)) CreateXmlDoc(filename);
            var result = true;
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(filename);
                Assembly myAssembly = Assembly.GetExecutingAssembly();
                using (Stream schemaStream = myAssembly.GetManifestResourceStream("ApplicationDBPersisTence.statistics.xsd"))
                {
                    XmlSchema schema = XmlSchema.Read(schemaStream, null);
                    doc.Schemas.Add(schema);
                }
                doc.Validate(delegate(Object sender, ValidationEventArgs args)
                {
                    logLb.Items.Add(string.Format((args.Severity == XmlSeverityType.Error ? "Error" : "Warning") + ": {0}", args.Message));
                    result = false;
                });
            }
            catch
            {
                result = false;
            }
            return result;
        }

        private void CreateXmlDoc(string path)
        {
            XmlDocument doc = new XmlDocument();
            XmlDeclaration dec = doc.CreateXmlDeclaration("1.0", "utf-8", null);
            doc.AppendChild(dec);
            XmlElement root = doc.CreateElement("statistics");
            doc.AppendChild(root);
            doc.Save(path);
        }

        private bool ValidateFolderStructure()
        {
            try
            {
                //Statistics Folder
                if (!Directory.Exists(statisticsPath)) Directory.CreateDirectory(statisticsPath);
                //year Folder
                string yearFolderPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @STATISTICS, @DateTime.Now.Year.ToString());
                if (!Directory.Exists(yearFolderPath)) Directory.CreateDirectory(yearFolderPath);
                //Month Folder
                string monthFolderPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @STATISTICS, @DateTime.Now.Year.ToString(), @DateTime.Now.ToString("MMMM", CultureInfo.InvariantCulture));
                if (!Directory.Exists(monthFolderPath)) Directory.CreateDirectory(monthFolderPath);

                return true;
            }
            catch (Exception)
            {
                logLb.Items.Add("Unable to Create The Statistics Folder Structure");
                return false;
            }
        }
    }
}
