using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.ComponentModel;
using System.Web.Script.Serialization;
using System.Windows;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Net.Http;
using System.IO;
using WebAdmin.View;

namespace WebAdmin.ViewModel
{
    public class AdminTabViewModel : TabViewModelBase, ITabViewModel
    {
        

        #region Properties
        public override string Header { get { return "Admin"; } }

        private DateTime _localHandicapDate;
        public DateTime LocalHandicapDate { get { return _localHandicapDate; } set { _localHandicapDate = value; OnPropertyChanged("LocalHandicapDate"); } }

        private string _playerName;
        public string PlayerName { get { return _playerName; } set { _playerName = value; OnPropertyChanged("PlayerName"); } }

        private string _playerGHIN;
        public string PlayerGHIN { get { return _playerGHIN; } set { _playerGHIN = value; OnPropertyChanged("PlayerGHIN"); } }

        private string _playerDues;
        public string PlayerDues { get { return _playerDues; } set { _playerDues = value; OnPropertyChanged("PlayerDues"); } }
        #endregion

        #region Commands
        public ICommand SubmitWaitingListCommand { get { return new ModelCommand(s => SubmitWaitingList(s)); } }
        public ICommand SubmitRosterCommand { get { return new ModelCommand(s => SubmitRoster(s)); } }
        public ICommand SubmitGHINCommand { get { return new ModelCommand(s => SubmitGHIN(s)); } }
        public ICommand SubmitLocalHandicapCommand { get { return new ModelCommand(s => SubmitLocalHandicap(s)); } }
        public ICommand LoginCommand { get { return new ModelCommand(s => Login(s)); } }
        public ICommand GetDuesCommand { get { return new ModelCommand(s => GetDues(s)); } }
        public ICommand PayDuesCommand { get { return new ModelCommand(s => PayDues(s)); } }
        #endregion

        private void Login(object s)
        {
            Credentials.GetLoginAndPassword();
        }

        public AdminTabViewModel()
        {
            DateTime d;
            if (File.Exists(TabViewModelBase.Options.LocalHandicapFileName))
            {
                d = File.GetLastWriteTime(TabViewModelBase.Options.LocalHandicapFileName);
            }
            else
            {
                d = DateTime.Now;
            }
                LocalHandicapDate = new DateTime(d.Year, d.Month, (d.Day >= 15) ? 15 : 1);
        }
        

        #region Waiting List
        private class WaitingListEntry
        {
            public int Position { get; set; }
            public string Name { get; set; }
            public DateTime DateAdded { get; set; }
        }

        private async void SubmitWaitingList(object s)
        {
            var waitingList = LoadWaitingList(Options.WaitListFileName);

            // cancelled password input
            if (string.IsNullOrEmpty(Credentials.LoginPassword))
            {
                return;
            }

            if ((waitingList != null) && (waitingList.Count > 0))
            {
                using (new WaitCursor())
                {
                    using (var client = new HttpClient())
                    {
                        client.BaseAddress = new Uri(WebAddresses.BaseAddress);

                        var values = new List<KeyValuePair<string, string>>();

                        values.Add(new KeyValuePair<string, string>("Login", Credentials.LoginName));
                        values.Add(new KeyValuePair<string, string>("Password", Credentials.LoginPassword));

                        for (int i = 0; i < waitingList.Count; i++)
                        {
                            values.Add(new KeyValuePair<string, string>(
                                string.Format("WaitingList[{0}][Position]", i),
                                waitingList[i].Position.ToString()));

                            values.Add(new KeyValuePair<string, string>(
                                string.Format("WaitingList[{0}][Name]", i),
                                 waitingList[i].Name));

                            values.Add(new KeyValuePair<string, string>(
                                string.Format("WaitingList[{0}][DateAdded]", i),
                                 waitingList[i].DateAdded.ToString("yyyy-MM-dd")));

                        }

                        var content = new FormUrlEncodedContent(values);

                        var response = await client.PostAsync(WebAddresses.ScriptFolder + WebAddresses.SubmitWaitingList, content);
                        var responseString = await response.Content.ReadAsStringAsync();
                        System.Diagnostics.Debug.WriteLine(responseString);

                        if (responseString.StartsWith("Success", StringComparison.InvariantCultureIgnoreCase))
                        {
                            System.Windows.MessageBox.Show("Waiting list uploaded");
                        }
                        else
                        {
                            TabViewModelBase.Credentials.CheckForInvalidPassword(responseString);
                            Logging.Log(WebAddresses.ScriptFolder + WebAddresses.SubmitWaitingList, responseString);

                            HtmlDisplayWindow displayWindow = new HtmlDisplayWindow();
                            displayWindow.WebBrowser.NavigateToString(responseString);
                            displayWindow.Owner = App.Current.MainWindow;
                            displayWindow.ShowDialog();
                        }
                    }
                }
            }
        }

        private List<WaitingListEntry> LoadWaitingList(string waitingListFileName)
        {
            if(!File.Exists(waitingListFileName))
            {
                throw new FileNotFoundException("File does not exist: " + waitingListFileName);
            }
            List<WaitingListEntry> entries = new List<WaitingListEntry>();
            string[][] csvFileEntries;
            using (TextReader tr = new StreamReader(waitingListFileName))
            {
                csvFileEntries = CSVParser.Parse(tr);
            }

            for (int i = 0; i < csvFileEntries.Length; i++)
            {
                if (i == 0)
                {
                    if (string.Compare(csvFileEntries[0][0], "position", true) != 0)
                    {
                        throw new ArgumentException("Waiting list file line 1 does not have 'POSITION' in field 1");
                    }
                    if (string.Compare(csvFileEntries[0][1], "name", true) != 0)
                    {
                        throw new ArgumentException("Waiting list file line 1 does not have 'NAME' in field 2");
                    }
                    if (string.Compare(csvFileEntries[0][2], "date added", true) != 0)
                    {
                        throw new ArgumentException("Waiting list file line 1 does not have 'DATE ADDED' in field 3");
                    }
                }
                else if ((csvFileEntries[i].Length > 2) &&
                    // skip if all 3 are empty
                    !(string.IsNullOrEmpty(csvFileEntries[i][0]) &&
                      string.IsNullOrEmpty(csvFileEntries[i][1]) &&
                      string.IsNullOrEmpty(csvFileEntries[i][2])))
                {
                    WaitingListEntry wle = new WaitingListEntry();
                    wle.Name = csvFileEntries[i][1];

                    int position;
                    if (!int.TryParse(csvFileEntries[i][0], out position))
                    {
                        throw new ArgumentException(string.Format(
                            "Waiting list file error on line {0}: position field is not an integer: {1} ",
                            i + 1, csvFileEntries[i][0]));
                    }
                    wle.Position = position;

                    DateTime dt;
                    if (!DateTime.TryParse(csvFileEntries[i][2], out dt))
                    {
                        throw new ArgumentException(string.Format(
                            "Waiting list file error on line {0}: date added field is not a date: {1} ",
                            i + 1, csvFileEntries[i][2]));
                    }
                    wle.DateAdded = dt;

                    entries.Add(wle);
                }
            }

            return entries;
        }
        #endregion

        #region Roster
        private class RosterEntry : GHINEntry
        {
            public bool Active { get; set; }
            public string Email { get; set; }
            public DateTime Birthday { get; set; }
        }

        private async void SubmitRoster(object s)
        {
            var roster = LoadRoster(Options.RosterFileName);

            // cancelled password input
            if (string.IsNullOrEmpty(Credentials.LoginPassword))
            {
                return;
            }

            if ((roster != null) && (roster.Count > 0))
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(WebAddresses.BaseAddress);

                    var values = new List<KeyValuePair<string, string>>();

                    values.Add(new KeyValuePair<string, string>("Login", Credentials.LoginName));
                    values.Add(new KeyValuePair<string, string>("Password", Credentials.LoginPassword));

                    int chunkIndex = 0;
                    for (int i = 0; i < roster.Count; i++)
                    {
                        values.Add(new KeyValuePair<string, string>(
                            string.Format("Roster[{0}][LastName]", chunkIndex),
                            roster[i].LastName));

                        values.Add(new KeyValuePair<string, string>(
                            string.Format("Roster[{0}][FirstName]", chunkIndex),
                             roster[i].FirstName));

                        values.Add(new KeyValuePair<string, string>(
                            string.Format("Roster[{0}][GHIN]", chunkIndex),
                             roster[i].GHIN.ToString()));

                        values.Add(new KeyValuePair<string, string>(
                            string.Format("Roster[{0}][Email]", chunkIndex),
                             roster[i].Email));

                        values.Add(new KeyValuePair<string, string>(
                            string.Format("Roster[{0}][Birthdate]", chunkIndex),
                             roster[i].Birthday.ToString("yyyy-MM-dd")));

                        chunkIndex++;
                        // If too much data is sent at once, you get an error 503
                        if (values.Count >= 500)
                        {
                            bool sent = await HttpSend(client, HtmlRequestType.Post, values, WebAddresses.ScriptFolder + WebAddresses.SubmitRoster);
                            chunkIndex = 0;

                            // Send partial list
                            if (!sent)
                            {
                                return;
                            }

                            // start over with a new list
                            values.Clear();

                            values.Add(new KeyValuePair<string, string>("Login", Credentials.LoginName));
                            values.Add(new KeyValuePair<string, string>("Password", Credentials.LoginPassword));
                        }

                    }

                    if (chunkIndex > 0)
                    {
                        bool sent = await HttpSend(client, HtmlRequestType.Post, values, WebAddresses.ScriptFolder + WebAddresses.SubmitRoster);

                        if (!sent)
                        {
                            return;
                        }
                    }

                    System.Windows.MessageBox.Show("Roster updated");

                }
            }
        }

        private List<RosterEntry> LoadRoster(string rosterFileName)
        {
            if(!File.Exists(rosterFileName))
            {
                throw new FileNotFoundException("File does not exist: " + rosterFileName);
            }
            List<RosterEntry> entries = new List<RosterEntry>();
            List<RosterEntry> entriesMissingGHIN = new List<RosterEntry>();
            string[][] csvFileEntries;
            using (TextReader tr = new StreamReader(rosterFileName))
            {
                csvFileEntries = CSVParser.Parse(tr);
            }

            int lastNameColumn = -1;
            int firstNameColumn = -1;
            int ghinColumn = -1;
            int emailColumn = -1;
            int birthdateColumn = -1;

            bool emptyLine = false;
            for (int row = 0; (row < csvFileEntries.Length) && !emptyLine; row++)
            {
                if (row == 0)
                {
                    for(int col = 0; col < csvFileEntries[row].Length; col++)
                    {
                        if (!string.IsNullOrEmpty(csvFileEntries[row][col]))
                        {
                            switch (csvFileEntries[row][col].ToLower())
                            {
                                case "last name":
                                    lastNameColumn = col;
                                    break;
                                case "first name":
                                    firstNameColumn = col;
                                    break;
                                case "ghin #":
                                    ghinColumn = col;
                                    break;
                                case "email":
                                    emailColumn = col;
                                    break;
                                case "birthdate":
                                    birthdateColumn = col;
                                    break;
                            }
                        }
                    }

                    if (lastNameColumn == -1) { throw new KeyNotFoundException("Unable to find 'Last Name' in the file header"); }
                    if (firstNameColumn == -1) { throw new KeyNotFoundException("Unable to find 'First Name' in the file header"); }
                    if (ghinColumn == -1) { throw new KeyNotFoundException("Unable to find 'GHIN #' in the file header"); }
                    if (emailColumn == -1) { throw new KeyNotFoundException("Unable to find 'Email' in the file header"); }
                    if (birthdateColumn == -1) { throw new KeyNotFoundException("Unable to find 'Birthdate' in the file header"); }
                }
                else
                {
                    if ((csvFileEntries[row].Length >= birthdateColumn) && 
                        !string.IsNullOrEmpty(csvFileEntries[row][1]) && 
                        !string.IsNullOrEmpty(csvFileEntries[row][2]))
                    {
                        RosterEntry re = new RosterEntry();
                        re.LastName = csvFileEntries[row][lastNameColumn];
                        re.FirstName = csvFileEntries[row][firstNameColumn];

                        if (string.IsNullOrEmpty(csvFileEntries[row][ghinColumn]))
                        {
                            entriesMissingGHIN.Add(re);
                        }
                        else
                        {
                            int ghinNumber;
                            if (!int.TryParse(csvFileEntries[row][ghinColumn], out ghinNumber))
                            {
                                throw new ArgumentException(string.Format("Invalid GHIN number on row {0}: '{1}'", row + 1, csvFileEntries[row][ghinColumn]));
                            }

                            re.GHIN = ghinNumber;
                            re.Email = csvFileEntries[row][emailColumn].Trim();

                            DateTime dt = new DateTime(2014, 1, 1);
                            if (!string.IsNullOrEmpty(csvFileEntries[row][birthdateColumn]))
                            {
                                if (!DateTime.TryParse(csvFileEntries[row][birthdateColumn], out dt))
                                {
                                    throw new ArgumentException(string.Format("Invalid birthdate on row {0}: '{1}'", row + 1, csvFileEntries[row][birthdateColumn]));
                                }
                            }
                            re.Birthday = dt;

                            entries.Add(re);
                        }
                    }
                    else
                    {
                        emptyLine = true;
                        for(int col = 0; col < csvFileEntries[row].Length; col++)
                        {
                            if(!string.IsNullOrEmpty(csvFileEntries[row][col]))
                            {
                                emptyLine = false;
                                break;
                            }
                        }
                    }
                }
            }

            if(entriesMissingGHIN.Count > 0)
            {
                string names = string.Empty;
                for(int i = 0; i < entriesMissingGHIN.Count; i++)
                {
                    if (i > 0) names += ", ";
                    names += entriesMissingGHIN[i].LastName;
                }

                System.Windows.MessageBox.Show("Skipping entries that do not have a GHIN number: " + names);
            }
            return entries;
        }
        #endregion

        #region GHIN
        private async void SubmitGHIN(object s)
        {
            if ((GHINEntries != null) && (GHINEntries.Count > 0))
            {
                // cancelled password input
                if (string.IsNullOrEmpty(Credentials.LoginPassword))
                {
                    return;
                }

                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(WebAddresses.BaseAddress);

                    var values = new List<KeyValuePair<string, string>>();

                    values.Add(new KeyValuePair<string, string>("Login", Credentials.LoginName));
                    values.Add(new KeyValuePair<string, string>("Password", Credentials.LoginPassword));

                    bool firstChunk = true;
                    int chunkIndex = 0;
                    for (int i = 0; i < GHINEntries.Count; i++)
                    {
                        if (firstChunk)
                        {
                            firstChunk = false;
                            values.Add(new KeyValuePair<string, string>("SetAllInactive", "1"));
                        }

                        values.Add(new KeyValuePair<string, string>(
                            string.Format("GHIN[{0}][LastName]", chunkIndex),
                            GHINEntries[i].LastName));

                        values.Add(new KeyValuePair<string, string>(
                            string.Format("GHIN[{0}][FirstName]", chunkIndex),
                             GHINEntries[i].FirstName));

                        values.Add(new KeyValuePair<string, string>(
                            string.Format("GHIN[{0}][GHIN]", chunkIndex),
                             GHINEntries[i].GHIN.ToString()));

                        chunkIndex++;
                        // If too much data is sent at once, you get an error 503
                        if (values.Count >= 500)
                        {
                            bool sent = await HttpSend(client, HtmlRequestType.Post, values, WebAddresses.ScriptFolder + WebAddresses.SubmitGHIN);
                            chunkIndex = 0;

                            // Send partial list
                            if (!sent)
                            {
                                return;
                            }

                            // start over with a new list
                            values.Clear();

                            values.Add(new KeyValuePair<string, string>("Login", Credentials.LoginName));
                            values.Add(new KeyValuePair<string, string>("Password", Credentials.LoginPassword));
                        }

                    }

                    if (chunkIndex > 0)
                    {
                        bool sent = await HttpSend(client, HtmlRequestType.Post, values, WebAddresses.ScriptFolder + WebAddresses.SubmitGHIN);

                        if (!sent)
                        {
                            return;
                        }
                    }

                    System.Windows.MessageBox.Show("GHIN updated");

                }
            }
            else
            {
                System.Windows.MessageBox.Show("No GHIN entries to submit.  Was there a problem loading the GHIN file?");
            }
        }

        
        #endregion

        #region Local Handicap
        private class LocalHandicapEntry
        {
            public int GHIN { get; set; }
            public string SCGAHandicap { get; set; }
            public string LocalHandicap { get; set; }
        }

        private async void SubmitLocalHandicap(object s)
        {
            var localHandicap = LoadLocalHandicap(Options.LocalHandicapFileName);

            // cancelled password input
            if (string.IsNullOrEmpty(Credentials.LoginPassword))
            {
                return;
            }

            if ((localHandicap != null) && (localHandicap.Count > 0))
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(WebAddresses.BaseAddress);

                    var values = new List<KeyValuePair<string, string>>();

                    values.Add(new KeyValuePair<string, string>("Login", Credentials.LoginName));
                    values.Add(new KeyValuePair<string, string>("Password", Credentials.LoginPassword));
                    values.Add(new KeyValuePair<string, string>("LocalHandicapDate", LocalHandicapDate.ToString("yyyy-MM-dd")));

                    int chunkIndex = 0;
                    bool firstChunk = true;
                    for (int i = 0; i < localHandicap.Count; i++)
                    {
                        if (firstChunk)
                        {
                            firstChunk = false;
                            values.Add(new KeyValuePair<string, string>("ClearTable", "1"));
                        }

                        values.Add(new KeyValuePair<string, string>(
                            string.Format("LocalHandicap[{0}][GHIN]", chunkIndex),
                            localHandicap[i].GHIN.ToString()));

                        values.Add(new KeyValuePair<string, string>(
                            string.Format("LocalHandicap[{0}][SCGAHandicap]", chunkIndex),
                             localHandicap[i].SCGAHandicap));

                        values.Add(new KeyValuePair<string, string>(
                            string.Format("LocalHandicap[{0}][LocalHandicap]", chunkIndex),
                             localHandicap[i].LocalHandicap));

                        chunkIndex++;
                        // If too much data is sent at once, you get an error 503
                        if (values.Count >= 500)
                        {
                            bool sent = await HttpSend(client, HtmlRequestType.Post, values, WebAddresses.ScriptFolder + WebAddresses.SubmitLocalHandicap);
                            chunkIndex = 0;

                            // Send partial list
                            if (!sent)
                            {
                                return;
                            }

                            // start over with a new list
                            values.Clear();

                            values.Add(new KeyValuePair<string, string>("Login", Credentials.LoginName));
                            values.Add(new KeyValuePair<string, string>("Password", Credentials.LoginPassword));
                        }

                    }

                    if (chunkIndex > 0)
                    {
                        bool sent = await HttpSend(client, HtmlRequestType.Post, values, WebAddresses.ScriptFolder + WebAddresses.SubmitLocalHandicap);

                        if (!sent)
                        {
                            return;
                        }
                    }

                    System.Windows.MessageBox.Show("Local Handicap updated");

                }
            }
        }

        private List<LocalHandicapEntry> LoadLocalHandicap(string localHandicapFileName)
        {
            if (!File.Exists(localHandicapFileName))
            {
                throw new FileNotFoundException("File does not exist: " + localHandicapFileName);
            }
            List<LocalHandicapEntry> entries = new List<LocalHandicapEntry>();
            string[][] csvFileEntries;
            using (TextReader tr = new StreamReader(localHandicapFileName))
            {
                csvFileEntries = CSVParser.Parse(tr);
            }

            int ghinColumn = 2;
            int scgaHandicapColumn = 4;
            int localHandicapColumn = 5;

            for (int row = 1; row < csvFileEntries.Length; row++)
            {
                if (!string.IsNullOrEmpty(csvFileEntries[row][0]))
                {
                    LocalHandicapEntry localHandicapEntry = new LocalHandicapEntry();

                    int ghinNumber;
                    if (!int.TryParse(csvFileEntries[row][ghinColumn], out ghinNumber))
                    {
                        throw new ArgumentException(string.Format("Invalid GHIN number on row {0}: '{1}'", row + 1, csvFileEntries[row][ghinColumn]));
                    }

                    localHandicapEntry.GHIN = ghinNumber;
                    localHandicapEntry.SCGAHandicap = csvFileEntries[row][scgaHandicapColumn];
                    localHandicapEntry.LocalHandicap = csvFileEntries[row][localHandicapColumn];

                    entries.Add(localHandicapEntry);
                }
            }

            return entries;
        }
        #endregion

        private async void GetDues(object s)
        {
            if(string.IsNullOrEmpty(PlayerName))
            {
                MessageBox.Show("Fill in the player name");
                return;
            }

            if (string.IsNullOrEmpty(PlayerGHIN))
            {
                MessageBox.Show("Fill in the player GHIN");
                return;
            }

            LoadDuesFromWeb();
        }

        private async void PayDues(object s)
        {
            if (string.IsNullOrEmpty(PlayerName))
            {
                MessageBox.Show("Fill in the player name");
                return;
            }

            if (string.IsNullOrEmpty(PlayerGHIN))
            {
                MessageBox.Show("Fill in the player GHIN");
                return;
            }

            if (string.IsNullOrEmpty(PlayerDues))
            {
                MessageBox.Show("Fill in the dues amount");
                return;
            }

            // cancelled password input
            if (string.IsNullOrEmpty(Credentials.LoginPassword))
            {
                return;
            }

            SubmitDues();
        }

        private async void LoadDuesFromWeb()
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(WebAddresses.BaseAddress);

                using (new WaitCursor())
                {
                    var values = new List<KeyValuePair<string, string>>();

                    values.Add(new KeyValuePair<string, string>("GHIN", PlayerGHIN));

                    var content = new FormUrlEncodedContent(values);

                    var response = await client.PostAsync(WebAddresses.ScriptFolder + WebAddresses.GetDues, content);
                    var responseString = await response.Content.ReadAsStringAsync();

                    Logging.Log("LoadDuesFromWeb", responseString);

                    var jss = new JavaScriptSerializer();
                    WebDues dues = jss.Deserialize<WebDues>(responseString);

                    PlayerDues = dues.Payment.ToString();

                }
            }
        }

        private async void SubmitDues()
        {

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(WebAddresses.BaseAddress);

                var values = new List<KeyValuePair<string, string>>();

                values.Add(new KeyValuePair<string, string>("Login", Credentials.LoginName));
                values.Add(new KeyValuePair<string, string>("Password", Credentials.LoginPassword));

                values.Add(new KeyValuePair<string, string>("GHIN", PlayerGHIN));
                values.Add(new KeyValuePair<string, string>("Name", PlayerName));
                values.Add(new KeyValuePair<string, string>("Payment", PlayerDues));

                bool sent = await HttpSend(client, HtmlRequestType.Post, values,
                    WebAddresses.ScriptFolder + WebAddresses.SubmitDues);

                if (sent)
                {
                    System.Windows.MessageBox.Show("Dues updated");
                }
            }
        }
    }
}
