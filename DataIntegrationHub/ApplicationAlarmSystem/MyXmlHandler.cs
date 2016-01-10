using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;

namespace ApplicationAlarmSystem
{
    class MyXmlHandler{

        private string _xmlfilepath;
        private string _xsdfilepath;
        private bool _isvalid = true;
        private string _validationMessage;

        public MyXmlHandler(string xmlfile, string xsdfile)
        {
            _xmlfilepath = xmlfile;
            _xsdfilepath = xsdfile;
        }

        public void CreateXML()
        {
            //gerar por codigo um ficheiro XML
            //alarmsRules.xml
            XmlDocument doc = new XmlDocument();

            //Create the XML declaration, and append it to XML document
            XmlDeclaration dec = doc.CreateXmlDeclaration("1.0", "utf-8", null);
            doc.AppendChild(dec);

            //Create the root element
            XmlElement root = doc.CreateElement("Rules");
            doc.AppendChild(root);

            //Create Rules
            //XmlElement rule = doc.CreateElement("Rule");
            //rule.SetAttributeNode("2");
            //doc.CreateElement(".CreateNavigator("type", "T");
            //rule.SetAttribute("min", "0");
            //rule.SetAttribute("max", "20");

            //root.AppendChild(rule);

            doc.Save(@"alarmsRules.xml");
        }

        public void CreateXMLRule()
        {

            XmlDocument doc = new XmlDocument();
            XmlDeclaration dec = doc.CreateXmlDeclaration("1.0", "utf-8", null);
            doc.AppendChild(dec);

            //Create the root element
            XmlElement root = doc.CreateElement("Rules");
            doc.AppendChild(root);

            //Create Rules
            //XmlElement rule = doc.CreateElement("Rule");
            //rule.SetAttributeNode("2");
            //doc.CreateElement(".CreateNavigator("type", "T");
            //rule.SetAttribute("min", "0");
            //rule.SetAttribute("max", "20");

            //root.AppendChild(rule);

            doc.Save(@"alarmsRules.xml");
        }

        public void deleteRules(string channel)
        {
            XmlDocument doc1 = new XmlDocument();
            doc1.Load(_xmlfilepath);
            XmlNode _channelExist = doc1.SelectSingleNode("/Rules/Rule[channel='"+ channel +"']");
            if (_channelExist != null)
            {
                // get its parent node
                XmlNode parent = _channelExist.ParentNode;

                // remove the child node
                parent.RemoveChild(_channelExist);

                // verify the new XML structure
                string newXML = _channelExist.OuterXml;
            }
            doc1.Save(_xmlfilepath);
        }

        public void updateRules(string channel, int min, int max)
        {
            XmlDocument doc1 = new XmlDocument();
            doc1.Load(_xmlfilepath);
            //verificar se o channel existe
            XmlNode _channelExist = doc1.SelectSingleNode("/Rules/Rule[channel='"+ channel +"']");
            //para atualizar o channel, min e max
            XmlNodeList _channelUpdate = doc1.SelectNodes("/Rules/Rule[channel='" + channel + "']");

            if (_channelExist == null){
                XmlElement newel = doc1.CreateElement("Rule");
                XmlNode x = doc1.GetElementsByTagName("Rules")[0];
                x.AppendChild(newel);

                XmlElement _createChan = doc1.CreateElement("channel");
                _createChan.InnerText = channel;
                XmlElement _createMin = doc1.CreateElement("min");
                _createMin.InnerText = Convert.ToString(min);
                XmlElement _createMax = doc1.CreateElement("max");
                _createMax.InnerText = Convert.ToString(max);
                newel.AppendChild(_createChan);
                newel.AppendChild(_createMin);
                newel.AppendChild(_createMax);
            }else{
 
                foreach (XmlNode item in _channelUpdate)
                {
                    item["min"].InnerText = Convert.ToString(min);
                    item["max"].InnerText = Convert.ToString(max);
                }
            }
            doc1.Save(_xmlfilepath);
        }


        public bool checkChannelExists(string channel)
        {
            XmlDocument doc1 = new XmlDocument();
            doc1.Load(_xmlfilepath);
            XmlNode _channelExist = doc1.SelectSingleNode("/Rules/Rule[channel='" + channel + "']");

            if (_channelExist == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public bool validateXml()
        {
            _isvalid = true;

            if (!File.Exists(_xmlfilepath))
            {
                FileStream Fs = new FileStream(_xmlfilepath, FileMode.CreateNew);
                Fs.Close();
            }

            XmlDocument doc = new XmlDocument();
            
            try
            {
                doc.Load(_xmlfilepath);
                ValidationEventHandler eventHandler = new ValidationEventHandler(MyEvent);
                //doc.Schemas.Add(null, );//_xsdfilepath
                Assembly myAssembly = Assembly.GetExecutingAssembly();
                using (Stream schemaStream = myAssembly.GetManifestResourceStream("ApplicationAlarmSystem.alarmsRules.xsd"))
                {
                    XmlSchema schema = XmlSchema.Read(schemaStream, null);
                    doc.Schemas.Add(schema);
                }
                doc.Validate(eventHandler);
            }
            catch (XmlException ex)
            {
                _isvalid = false;
                _validationMessage = string.Format("Error: {0}", ex.ToString()); // ex.Message
            }
            return _isvalid;
        }

        private void MyEvent(Object sender, ValidationEventArgs args)
        {
            _isvalid = false;

            switch (args.Severity)
            {
                case XmlSeverityType.Error:
                    _validationMessage = string.Format("Error: {0}", args.Message);
                    break;
                case XmlSeverityType.Warning:
                    _validationMessage = string.Format("Warning: {0}", args.Message);
                    break;
                default:
                    break;
            }
        }
        
    }
}
