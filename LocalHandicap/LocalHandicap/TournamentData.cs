using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using System.Windows.Forms;

namespace LocalHandicap
{
    [Serializable]
    public class TournamentData
    {
        public TournamentData() { }

        public string Name;

        public void SaveTournamentData(string tdFile)
        {
            try
            {
                using (FileStream fs = new FileStream(tdFile, FileMode.Create))
                {
                    XmlSerializer xs = new XmlSerializer(typeof(TournamentData));

                    xs.Serialize(fs, this);

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to save tournament data to " + tdFile + ": " + ex.Message);
            }
        }

        public void LoadTournamentData(string tdFile)
        {
            TournamentData td = new TournamentData();
            if (File.Exists(tdFile))
            {
                try
                {
                    using (FileStream fs = new FileStream(tdFile, FileMode.Open))
                    {
                        XmlSerializer xs = new XmlSerializer(typeof(TournamentData));
                        td = (TournamentData)xs.Deserialize(fs);
                        this.Name = td.Name;

                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to read tournament data from " + tdFile + ": " + ex.Message);
                }
            }
        }
    }
}
