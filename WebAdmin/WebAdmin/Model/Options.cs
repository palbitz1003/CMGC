using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using System.IO;
using System.Threading.Tasks;

namespace WebAdmin
{
    [Serializable]
    public class Options
    {
        public string WebSite { get; set; }
        public string ScriptFolder { get; set; }
        public string WaitListFileName { get; set; }
        public string RosterFileName { get; set; }
        public string GHINFileName { get; set; }
        public string LocalHandicapFileName { get; set; }
        public string LastCSVResultFolder { get; set; }

        public Options()
        {
            WebSite = "www.coronadomensgolf.org";
            ScriptFolder = "v2";
        }

        public static Options Load(string fileName)
        {
            if(string.IsNullOrEmpty(fileName) || !File.Exists(fileName))
            {
                return new Options();
            }
            else
            {
                using (StreamReader sr = new StreamReader(fileName))
                {
                    // If you disable debugging "Just My Code", you get an exception here as
                    // .NET tries to find a pre-compiled dll for serializing the Options class.
                    XmlSerializer xs = new XmlSerializer(typeof(Options));
                    try
                    {
                        return (Options)xs.Deserialize(sr);
                    }
                    catch (Exception)
                    {
                        System.Windows.MessageBox.Show("Error loading options.xml file.  Starting over with default options.");
                        return new Options();
                    }
                }
            }
        }

        public void Save(string fileName)
        {
            if(File.Exists(fileName))
            {
                File.Delete(fileName);
            }

            using(StreamWriter sw = new StreamWriter(fileName))
            {
                // If you disable debugging "Just My Code", you get an exception here as
                // .NET tries to find a pre-compiled dll for serializing the Options class.
                XmlSerializer xs = new XmlSerializer(typeof(Options));
                xs.Serialize(sw, this);
            }
        }
    }
}
