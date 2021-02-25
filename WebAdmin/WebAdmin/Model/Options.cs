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
        public bool TeeTimeInterval78 { get; set; }
        public bool TeeTimeInterval98 { get; set; }
        public bool TeeTimeInterval10 { get; set; }
        public bool Block52TeeTimes { get; set; }

        public Options()
        {
            WebSite = "www.coronadomensgolf.org";
            ScriptFolder = "v2";
            TeeTimeInterval78 = true;
            TeeTimeInterval98 = false;
            TeeTimeInterval10 = false;
            Block52TeeTimes = false;
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
                        var options = (Options)xs.Deserialize(sr);

                        // Sanity checks on the interval choice. Only 1 can be true.
                        if (!options.TeeTimeInterval78 && !options.TeeTimeInterval98 && !options.TeeTimeInterval10)
                        {
                            options.TeeTimeInterval78 = true;
                        }
                        else
                        {
                            int countTrue = 0;
                            if (options.TeeTimeInterval78) countTrue++;
                            if (options.TeeTimeInterval98) countTrue++;
                            if (options.TeeTimeInterval10) countTrue++;
                            if (countTrue > 1)
                            {
                                options.TeeTimeInterval78 = true;
                                options.TeeTimeInterval98 = false;
                                options.TeeTimeInterval10 = false;
                            }
                        }
                        return options;
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
