using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;
using System.Windows;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Net.Http;
using System.IO;
using WebAdmin.View;

namespace WebAdmin.ViewModel
{
    public class AdminTabViewModel : TabViewModelBase
    {
        #region Properties
        public override string Header { get { return "Admin"; } }

        private DateTime _localHandicapDate;
        public DateTime LocalHandicapDate { get { return _localHandicapDate; } set { _localHandicapDate = value; OnPropertyChanged(); } }

        private string _playerName;
        public string PlayerName { get { return _playerName; } set { _playerName = value; OnPropertyChanged(); } }

        private string _playerGhin;
        public string PlayerGhin { get { return _playerGhin; } set { _playerGhin = value; OnPropertyChanged(); } }

        private string _playerDues;
        public string PlayerDues { get { return _playerDues; } set { _playerDues = value; OnPropertyChanged(); } }
        #endregion

        #region Commands
        public ICommand SubmitWaitingListCommand { get { return new ModelCommand(async s => await SubmitWaitingList(s)); } }
        public ICommand SubmitGhinCommand { get { return new ModelCommand(async s => await SubmitGhin(s)); } }
        public ICommand SubmitLocalHandicapCommand { get { return new ModelCommand(async s => await SubmitLocalHandicap(s)); } }
        public ICommand LoginCommand { get { return new ModelCommand(s => Login(s)); } }
        public ICommand GetDuesCommand { get { return new ModelCommand(async s => await GetDues(s)); } }
        public ICommand PayDuesCommand { get { return new ModelCommand(async s => await PayDues(s)); } }
        #endregion

        private void Login(object s)
        {
            Credentials.GetLoginAndPassword();
        }

        public AdminTabViewModel()
        {
            DateTime d;
            if (File.Exists(Options.LocalHandicapFileName))
            {
                d = File.GetLastWriteTime(Options.LocalHandicapFileName);
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

        private async Task SubmitWaitingList(object s)
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
                            MessageBox.Show("Waiting list uploaded");
                        }
                        else
                        {
                            Credentials.CheckForInvalidPassword(responseString);
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

        #region GHIN
        private async Task SubmitGhin(object s)
        {
            string[] membershipTypes = await GetMembershipTypesFromWeb();

            foreach (var ghinEntry in GHINEntries)
            {
                if (string.IsNullOrEmpty(ghinEntry.MembershipType))
                {
                    throw new ArgumentException(string.Format("Membership type is empty for \"{0}\"", ghinEntry.LastNameFirstName));
                }
                if (!membershipTypes.Contains(ghinEntry.MembershipType))
                {
                    throw new ArgumentException(string.Format("Unexpected membership type \"{0}\" for {1}. Allowed types are {2}.",
                        ghinEntry.MembershipType, ghinEntry.LastNameFirstName, string.Join(", ", membershipTypes)));
                }
            }

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

                        values.Add(new KeyValuePair<string, string>(
                            string.Format("GHIN[{0}][Email]", chunkIndex),
                             GHINEntries[i].Email));

                        values.Add(new KeyValuePair<string, string>(
                            string.Format("GHIN[{0}][Birthdate]", chunkIndex),
                             GHINEntries[i].Birthday.ToString("yyyy-MM-dd")));

                        values.Add(new KeyValuePair<string, string>(
                            string.Format("GHIN[{0}][MembershipType]", chunkIndex),
                             GHINEntries[i].MembershipType));

                        chunkIndex++;
                        // If too much data is sent at once, you get an error 503
                        if (values.Count >= 400)
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

                    MessageBox.Show("Roster updated");

                }
            }
            else
            {
                MessageBox.Show("No roster entries to submit.  Was there a problem loading the roster file?");
            }
        }

        private async Task<string[]> GetMembershipTypesFromWeb()
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(WebAddresses.BaseAddress);

                using (new WaitCursor())
                {
                    var responseString = await client.GetStringAsync(WebAddresses.ScriptFolder + WebAddresses.GetMembershipTypes);

                    Logging.Log("GetMembershipTypesFromWeb", responseString);

                    var jss = new JavaScriptSerializer();
                    string[] membershipTypes = jss.Deserialize<string[]>(responseString);

                    return membershipTypes;
                }
            }
        }
        #endregion

        #region Local Handicap
        private class LocalHandicapEntry
        {
            public int Ghin { get; set; }
            public string ScgaHandicap { get; set; }
            public string LocalHandicap { get; set; }
        }

        private async Task SubmitLocalHandicap(object s)
        {
            var localHandicap = LoadGgHandicapIndexes(Options.LocalHandicapFileName);

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
                            localHandicap[i].Ghin.ToString()));

                        values.Add(new KeyValuePair<string, string>(
                            string.Format("LocalHandicap[{0}][SCGAHandicap]", chunkIndex),
                             localHandicap[i].ScgaHandicap));

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

                    MessageBox.Show("Local Handicap updated");

                }
            }
        }

        //private List<LocalHandicapEntry> LoadLocalHandicap(string localHandicapFileName)
        //{
        //    if (!File.Exists(localHandicapFileName))
        //    {
        //        throw new FileNotFoundException("File does not exist: " + localHandicapFileName);
        //    }
        //    List<LocalHandicapEntry> entries = new List<LocalHandicapEntry>();
        //    string[][] csvFileEntries;
        //    using (TextReader tr = new StreamReader(localHandicapFileName))
        //    {
        //        csvFileEntries = CSVParser.Parse(tr);
        //    }

        //    int ghinColumn = 2;
        //    int scgaHandicapColumn = 4;
        //    int localHandicapColumn = 5;

        //    for (int row = 1; row < csvFileEntries.Length; row++)
        //    {
        //        if (!string.IsNullOrEmpty(csvFileEntries[row][0]))
        //        {
        //            LocalHandicapEntry localHandicapEntry = new LocalHandicapEntry();

        //            int ghinNumber;
        //            if (!int.TryParse(csvFileEntries[row][ghinColumn], out ghinNumber))
        //            {
        //                throw new ArgumentException(string.Format("Invalid GHIN number on row {0}: '{1}'", row + 1, csvFileEntries[row][ghinColumn]));
        //            }

        //            localHandicapEntry.Ghin = ghinNumber;
        //            localHandicapEntry.ScgaHandicap = csvFileEntries[row][scgaHandicapColumn];
        //            localHandicapEntry.LocalHandicap = csvFileEntries[row][localHandicapColumn];

        //            entries.Add(localHandicapEntry);
        //        }
        //    }

        //    return entries;
        //}

        private List<LocalHandicapEntry> LoadGgHandicapIndexes(string ggMasterRosterFile)
        {
            if (!File.Exists(ggMasterRosterFile))
            {
                throw new FileNotFoundException("File does not exist: " + ggMasterRosterFile);
            }
            List<LocalHandicapEntry> entries = new List<LocalHandicapEntry>();
            string[][] csvFileEntries;
            using (TextReader tr = new StreamReader(ggMasterRosterFile))
            {
                csvFileEntries = CSVParser.Parse(tr);
            }

            int ghinColumn = -1;
            int scgaIndexColumn = -1;

            int headerRow = -1;
            for (int row = 0; (row < csvFileEntries.Length) && (row < 4); row++)
            {
                for (int col = 0; (col < csvFileEntries[row].Length) && (ghinColumn == -1); col++)
                {
                    if (csvFileEntries[row][col].Contains("GHIN"))
                    {
                        ghinColumn = col;
                        headerRow = row;
                    }
                }

                for (int col = 0; (col < csvFileEntries[row].Length) && (scgaIndexColumn == -1); col++)
                {
                    if (csvFileEntries[row][col].Contains("Index"))
                    {
                        scgaIndexColumn = col;
                    }
                }
            }

            if (headerRow == -1)
            {
                throw new ArgumentException("Failed to find a header row that had 'GHIN' in one of the column headers");
            }

            if (scgaIndexColumn == -1)
            {
                throw new ArgumentException("Failed to find a header row that had 'Index' in one of the column headers");
            }

            for (int row = headerRow + 1; row < csvFileEntries.Length; row++)
            {
                if (!string.IsNullOrEmpty(csvFileEntries[row][0]))
                {
                    LocalHandicapEntry localHandicapEntry = new LocalHandicapEntry();

                    int ghinNumber;
                    if (!int.TryParse(csvFileEntries[row][ghinColumn], out ghinNumber))
                    {
                        throw new ArgumentException(string.Format("Invalid GHIN number on row {0}: '{1}'", row + 1, csvFileEntries[row][ghinColumn]));
                    }

                    localHandicapEntry.Ghin = ghinNumber;
                    if (string.IsNullOrEmpty(csvFileEntries[row][scgaIndexColumn]))
                    {
                        localHandicapEntry.ScgaHandicap = "0";
                        localHandicapEntry.LocalHandicap = "0";
                    }
                    else
                    {
                        localHandicapEntry.ScgaHandicap = csvFileEntries[row][scgaIndexColumn];
                        localHandicapEntry.LocalHandicap = localHandicapEntry.ScgaHandicap;  // We don't have local handicaps for now
                    }

                    entries.Add(localHandicapEntry);
                }
            }

            return entries;
        }
        #endregion

        private async Task GetDues(object s)
        {
            if(string.IsNullOrEmpty(PlayerName))
            {
                MessageBox.Show("Fill in the player name");
                return;
            }

            if (string.IsNullOrEmpty(PlayerGhin))
            {
                MessageBox.Show("Fill in the player GHIN");
                return;
            }

            await LoadDuesFromWeb();
        }

        private async Task PayDues(object s)
        {
            if (string.IsNullOrEmpty(PlayerName))
            {
                MessageBox.Show("Fill in the player name");
                return;
            }

            if (string.IsNullOrEmpty(PlayerGhin))
            {
                MessageBox.Show("Fill in the player GHIN");
                return;
            }

            if (string.IsNullOrEmpty(PlayerDues))
            {
                MessageBox.Show("Fill in the dues amount");
                return;
            }

            int dues;
            if (!int.TryParse(PlayerDues, out dues))
            {
                MessageBox.Show("Dues amount is not a number");
                return;
            }

            // cancelled password input
            if (string.IsNullOrEmpty(Credentials.LoginPassword))
            {
                return;
            }

            await SubmitDues();
        }

        private async Task LoadDuesFromWeb()
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(WebAddresses.BaseAddress);

                using (new WaitCursor())
                {
                    var values = new List<KeyValuePair<string, string>>();

                    values.Add(new KeyValuePair<string, string>("GHIN", PlayerGhin));

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

        private async Task SubmitDues()
        {

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(WebAddresses.BaseAddress);

                var values = new List<KeyValuePair<string, string>>();

                values.Add(new KeyValuePair<string, string>("Login", Credentials.LoginName));
                values.Add(new KeyValuePair<string, string>("Password", Credentials.LoginPassword));

                values.Add(new KeyValuePair<string, string>("GHIN", PlayerGhin));
                values.Add(new KeyValuePair<string, string>("Name", PlayerName));
                values.Add(new KeyValuePair<string, string>("Payment", PlayerDues));

                bool sent = await HttpSend(client, HtmlRequestType.Post, values,
                    WebAddresses.ScriptFolder + WebAddresses.SubmitDues);

                if (sent)
                {
                    MessageBox.Show("Dues updated");
                }
            }
        }
    }
}
