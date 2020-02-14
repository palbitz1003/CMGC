using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebAdmin.ViewModel;

namespace WebAdmin
{
    public class Score
    {
        public float ScoreRound1 { get; set; }
        public float ScoreRound2 { get; set; }
        public DateTime Date { get; set; }
        public int Flight { get; set; }
        public int TeamNumber { get; set; }
        public string Name1 { get; set; }
        public string Name2 { get; set; }
        public string Name3 { get; set; }
        public string Name4 { get; set; }
        public float ScoreTotal { get; set; }

        public void AddToList(List<KeyValuePair<string, string>> kvpList, int index)
        {
            kvpList.Add(new KeyValuePair<string, string>(
                            string.Format("ResultsScores[{0}][ScoreRound1]", index), ScoreRound1.ToString()));
            kvpList.Add(new KeyValuePair<string, string>(
                string.Format("ResultsScores[{0}][ScoreRound2]", index), ScoreRound2.ToString()));

            kvpList.Add(new KeyValuePair<string, string>(
                            string.Format("ResultsScores[{0}][Date]", index), Date.ToString("yyyy-MM-dd")));

            kvpList.Add(new KeyValuePair<string, string>(
                            string.Format("ResultsScores[{0}][Flight]", index), Flight.ToString()));

            kvpList.Add(new KeyValuePair<string, string>(
                            string.Format("ResultsScores[{0}][TeamNumber]", index), TeamNumber.ToString()));

            AddPlayer(kvpList, index, Name1, "Name1", "GHIN1");
            AddPlayer(kvpList, index, Name2, "Name2", "GHIN2");
            AddPlayer(kvpList, index, Name3, "Name3", "GHIN3");
            AddPlayer(kvpList, index, Name4, "Name4", "GHIN4");

            kvpList.Add(new KeyValuePair<string, string>(
                                string.Format("ResultsScores[{0}][ScoreTotal]", index), ScoreTotal.ToString()));
        }

        public Score()
        {
            Name1 = string.Empty;
            Name2 = string.Empty;
            Name3 = string.Empty;
            Name4 = string.Empty;
        }

        private string HandleName(string name)
        {
            name = name.Trim();
            if (name == ",") return string.Empty;
            return name;
        }

        private void AddPlayer(List<KeyValuePair<string, string>> kvpList, int index, string name, string htmlName, string htmlGHIN)
        {
            name = HandleName(name);

            kvpList.Add(new KeyValuePair<string, string>(
                            string.Format("ResultsScores[{0}][{1}]", index, htmlName), name));

            GHINEntry gi = GHINEntry.FindName(TabViewModelBase.GHINEntries, name);
            int ghinNumber = 0;

            if (gi != null)
            {
                ghinNumber = gi.GHIN;

            }

            kvpList.Add(new KeyValuePair<string, string>(
                    string.Format("ResultsScores[{0}][{1}]", index, htmlGHIN), ghinNumber.ToString()));

            if ((gi == null) && !string.IsNullOrEmpty(name))
            {
                Logging.Log("Submit scores results: No GHIN entry for: " + name);
            }
        }
    }
}
