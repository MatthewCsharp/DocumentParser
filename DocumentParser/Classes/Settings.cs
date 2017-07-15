using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace DocumentParser.Classes {
    public class Settings {
        private readonly string SettingsXmlPath = "./Settings.xml";
        public string ProcessedFolder;
        public bool Running;
        public string ScanFolder;

        public Settings() {
            XDocument xdoc;
            try {
                xdoc = XDocument.Load(SettingsXmlPath);
                SetVariables(xdoc);
            }
            catch (Exception x) {
                File.Delete(SettingsXmlPath);
                new XDocument(new XElement("Credentials", new XElement("ScanFolder", @"C:\Auto"), new XElement("ProcessedFolder", @"C:\Auto\Completed"), new XElement("Running", @"True"))).Save(SettingsXmlPath);
                xdoc = XDocument.Load(SettingsXmlPath);
                SetVariables(xdoc);
            }
        }

        private void SetVariables(XDocument xdoc) {
            ScanFolder = xdoc.Document.Descendants("ScanFolder").First().Value;
            ProcessedFolder = xdoc.Document.Descendants("ProcessedFolder").First().Value;
            Running = xdoc.Document.Descendants("Running").First().Value == "True";
        }

        public void UpdateSetting(string settingName, string value) {
            var xdoc = XDocument.Load(SettingsXmlPath);
            xdoc.Document.Descendants(settingName).First().Value = value;
            xdoc.Save(SettingsXmlPath);

            SetVariables(xdoc);
        }
    }
}