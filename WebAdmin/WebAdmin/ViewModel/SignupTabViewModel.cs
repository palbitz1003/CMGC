using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Net.Http;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Printing;
using WebAdmin.View;

namespace WebAdmin.ViewModel
{
    public class SignupTabViewModel : TabViewModelBase
    {

        #region Properties
        public override string Header { get { return "Signup"; } }

        private readonly List<string> _defaultTeeTimes = new List<string>
        { 
            "6:00", "6:07", "6:15", "6:22", "6:30", "6:37", "6:45", "6:52",
            "7:00", "7:07", "7:15", "7:22", "7:30", "7:37", "7:45", "7:52",
            "8:00", "8:07", "8:15", "8:22", "8:30", "8:37", "8:45", "8:52",
            "9:00", "9:07", "9:15", "9:22", "9:30", "9:37", "9:45", "9:52",
            "10:00", "10:07", "10:15", "10:22", "10:30", "10:37", "10:45", "10:52",
            "11:00", "11:07", "11:15", "11:22", "11:30", "11:37", "11:45", "11:52"};

        private Visibility _getTournamentsVisible;
        public Visibility GetTournamentsVisible { get { return _getTournamentsVisible; } set { _getTournamentsVisible = value; OnPropertyChanged(); } }

        private Visibility _gotTournamentsVisible;
        public Visibility GotTournamentsVisible { get { return _gotTournamentsVisible; } set { _gotTournamentsVisible = value; OnPropertyChanged(); } }

        private int _tournamentNameIndex;
        public int TournamentNameIndex
        {
            get { return _tournamentNameIndex; }
            set
            {
                if (_tournamentNameIndex != value)
                {
                    _tournamentNameIndex = value;
                    OnPropertyChanged();
                }
            }
        }

        private TrulyObservableCollection<TournamentName> _tournamentNames;
        public TrulyObservableCollection<TournamentName> TournamentNames
        {
            get { return _tournamentNames; }
            set { _tournamentNames = value; OnPropertyChanged(); }
        }

        private List<string> _teeTimes;
        public List<string> TeeTimes { get { return _teeTimes; } set { _teeTimes = value; OnPropertyChanged(); } }

        private int _firstTeeTimeIndex;
        public int FirstTeeTimeIndex { get { return _firstTeeTimeIndex; } 
            set 
            { 
                _firstTeeTimeIndex = value; 
                OnPropertyChanged();
                UpdateTournamentTeeTimes();
            } 
        }

        private int _todoSelection;
        public int TodoSelection { get { return _todoSelection; } 
            set { 
                if (value != -1) 
                { 
                    TodoSelectionChanged(value); 
                }
                _todoSelection = -1;
            } 
        }

        private int _removeSelection;
        public int RemoveSelection { get { return _removeSelection; } 
            set 
            { 
                if (value != -1) 
                { 
                    RemoveSelectionChanged(value); 
                }
                _removeSelection = -1;
            } 
        }

        private int _teeTimeSelection;
        public int TeeTimeSelection { get { return _teeTimeSelection; }
            set 
            { 
                _teeTimeSelection = value;
                TeeTimesSelectionChanged(value);
                OnPropertyChanged();
            } 
        }

        private TrulyObservableCollection<TeeTimeRequest> _teeTimeRequestsUnassigned;
        public TrulyObservableCollection<TeeTimeRequest> TeeTimeRequestsUnassigned { 
            get { return _teeTimeRequestsUnassigned; }
            set { _teeTimeRequestsUnassigned = value; OnPropertyChanged(); } 
        }

        private TrulyObservableCollection<TeeTimeRequest> _teeTimeRequestsAssigned;
        public TrulyObservableCollection<TeeTimeRequest> TeeTimeRequestsAssigned { 
            get { return _teeTimeRequestsAssigned; } 
            set { _teeTimeRequestsAssigned = value; OnPropertyChanged(); } 
        }

        public List<TeeTimeRequest> TeeTimeRequests { get; set; }

        private TrulyObservableCollection<TeeTime> _tournamentTeeTimes;
        public TrulyObservableCollection<TeeTime> TournamentTeeTimes
        {
            get { return _tournamentTeeTimes; }
            set { _tournamentTeeTimes = value; OnPropertyChanged(); }
        }

        private List<TeeTime> _removedTeeTimes;

        private string _ggTeeTimeFile;
        public string GgTeeTimeFile
        {
            get { return _ggTeeTimeFile; }
            set { _ggTeeTimeFile = value; OnPropertyChanged(); }
        }

        private string _waitingListFile;
        public string WaitingListFile
        {
            get { return _waitingListFile; }
            set { _waitingListFile = value; OnPropertyChanged(); }
        }

        private bool _orderBySignupDate;
        public bool OrderBySignupDate { get { return _orderBySignupDate; } 
            set 
            { 
                _orderBySignupDate = value; 
                OnPropertyChanged();
                SortTeeTimeRequests();
            } 
        }

        private bool _groupMode;
        public bool GroupMode 
        { 
            get { return _groupMode; } 
            set
            {
                if (_groupMode == value) return;

                _groupMode = value; 
                OnPropertyChanged();
                if (_groupMode)
                {
                    SwitchToGroupMode();
                }
                else
                {
                    SwitchToIndividualMode();
                }
            } 
        }

        private bool _teeTimesDirty = false;

        private int _currentNumberOfPlayersShowing;
        #endregion

        #region Commands
        public ICommand GetTournamentsCommand { get { return new ModelCommand(async s => await GetTournaments(s)); } }

        public ICommand LoadSignupsCommand { get { return new ModelCommand(async s => await LoadSignupsFromWeb(s)); } }

        public ICommand LoadTeetimesCommand { get { return new ModelCommand(async s => await LoadTeeTimesFromWeb(s)); } }

        public ICommand UploadTeetimesCommand { get { return new ModelCommand(async s => await UploadToWeb(s)); } }

        public ICommand SaveAsCsvCommand { get { return new ModelCommand(SaveAsCsv); } }

        public ICommand AddPlayerCommand { get { return new ModelCommand(AddPlayer); } }

        public ICommand PrintCommand { get { return new ModelCommand(Print); } }

        public ICommand UploadVpCsvCommand { get { return new ModelCommand(UploadVpCsv); } }

        public ICommand UploadWaitingListFileCommand { get { return new ModelCommand(async s => await UploadWaitingListFile(s)); } }
        #endregion

        public SignupTabViewModel()
        {
            TournamentNames = new TrulyObservableCollection<TournamentName>();
            TeeTimeRequests = new List<TeeTimeRequest>();
            TeeTimeRequestsUnassigned = new TrulyObservableCollection<TeeTimeRequest>();
            TeeTimeRequestsAssigned = new TrulyObservableCollection<TeeTimeRequest>();
            TournamentTeeTimes = new TrulyObservableCollection<TeeTime>();
            _removedTeeTimes = new List<TeeTime>();
            TodoSelection = -1;
            RemoveSelection = -1;
            TeeTimes = new List<string>();
            InitTeeTimes();
            FirstTeeTimeIndex = 0;
            GetTournamentsVisible = Visibility.Visible;
            GotTournamentsVisible = Visibility.Collapsed;
        }

        public void InitTeeTimes()
        {
            _removedTeeTimes.Clear();
            ClearPlayers();
            TournamentTeeTimes.Clear();
            if (File.Exists("TeeTimes.txt"))
            {
                LoadTeeTimesFromFile("TeeTimes.txt");
            }
            else
            {
                // TODO shotgun
                foreach (var time in _defaultTeeTimes)
                {
                    TournamentTeeTimes.Add(new TeeTime { StartTime = time });
                }
            }

            // TODO shotgun
            for(int i = 0; (i < 16) && (i < TournamentTeeTimes.Count); i++)
            {
                TeeTimes.Add(TournamentTeeTimes[i].StartTime);
            }

            UpdateTournamentTeeTimes();
        }

        public void SortTeeTimeRequests()
        {
            if (TeeTimeRequests != null)
            {
                if (OrderBySignupDate)
                {
                    TeeTimeRequests.Sort(new SubmitKeySort());
                }
                else
                {
                    TeeTimeRequests.Sort(new TeeTimeRequestSort());
                }
                UpdateUnassignedList(_currentNumberOfPlayersShowing);
            }
        }

        private void UpdateTournamentTeeTimes()
        {
            for(int i = _removedTeeTimes.Count - 1; i >= 0; i--)
            {
                TournamentTeeTimes.Insert(0, _removedTeeTimes[i]);
            }
            _removedTeeTimes.Clear();

            for(int i = 0; i < TournamentTeeTimes.Count; i++)
            {
                if(TournamentTeeTimes[i].StartTime != TeeTimes[FirstTeeTimeIndex])
                {
                    _removedTeeTimes.Add(TournamentTeeTimes[i]);
                }
                else
                {
                    break;
                }
            }

            if (TournamentTeeTimes.Count == _removedTeeTimes.Count)
            {
                // Something went wrong.  Don't remove anything.
                _removedTeeTimes.Clear();
            }
            else
            {
                foreach (var teeTime in _removedTeeTimes)
                {
                    TournamentTeeTimes.Remove(teeTime);
                }
            }
        }

        private async Task GetTournaments(object o)
        {
            string responseString = await GetTournamentNames();

            LoadTournamentNamesFromWebResponse(responseString, TournamentNames, false);
        }

        protected override void OnTournamentsUpdated()
        {
            if(TournamentNames.Count > 0)
            {
                TournamentNameIndex = 0;
            }
            else
            {
                TournamentNameIndex = -1;
            }

            for (int i = 0; i < TournamentNames.Count; i++ )
            {
                if ((DateTime.Now > TournamentNames[i].SignupStartDate) && (DateTime.Now <= TournamentNames[i].EndDate))
                {
                    TournamentNameIndex = i;
                    break;
                }
            }

            if(TournamentNames.Count > 0)
            {
                GetTournamentsVisible = Visibility.Collapsed;
                GotTournamentsVisible = Visibility.Visible;
            }
        }

        public void UpdateUnassignedList(int playerCount)
        {
            _currentNumberOfPlayersShowing = playerCount;

            TeeTimeRequestsUnassigned.Clear();
            foreach(var teeTimeRequest in TeeTimeRequests)
            {
                if(teeTimeRequest.Players.Count <= playerCount)
                {
                    TeeTimeRequestsUnassigned.Add(teeTimeRequest);
                }
            }
        }

        private void TodoSelectionChanged(int selectionIndex)
        {
            if (selectionIndex < 0) return;

            var selectedItem = TeeTimeRequestsUnassigned[selectionIndex];

            if (TeeTimeSelection < 0)
            {
                TeeTimeSelection = 0;
            }

            TeeTimeRequest teeTimeRequest = selectedItem;
            TeeTime teeTime = TournamentTeeTimes[TeeTimeSelection];

            // Check for room in this tee time
            if (teeTimeRequest.Players.Count > (4 - teeTime.Players.Count))
            {
                MessageBox.Show("Not enough room for all players at this tee time");
                TodoSelection = -1;
                return;
            }

            // Add the players to the tee time
            _teeTimesDirty = true;
            foreach (var player in teeTimeRequest.Players)
            {
                teeTime.AddPlayer(player);
            }
            teeTimeRequest.TeeTime = teeTime;

            TeeTimeRequests.Remove(teeTimeRequest);
            TeeTimeRequestsAssigned.Add(teeTimeRequest);
            TeeTimeRequestsAssigned = TrulyObservableCollection<TeeTimeRequest>.Sort(TeeTimeRequestsAssigned,
                    new TeeTimeSort());

            // Select the next tee time
            if (teeTime.Players.Count == 4)
            {
                bool teeTimeFound = false;
                for (int i = TeeTimeSelection + 1; i < TournamentTeeTimes.Count; i++)
                {
                    if (TournamentTeeTimes[i].Players.Count < 4)
                    {
                        teeTimeFound = true;
                        TeeTimeSelection = i;
                        break;
                    }
                }

                if (!teeTimeFound)
                {
                    // show all remaining unassigned
                    UpdateUnassignedList(4);
                }
            }
            else
            {
                // Only show the unassigned that fit into this tee time
                UpdateUnassignedList(4 - teeTime.Players.Count);
                if ((TeeTimeRequestsUnassigned.Count == 0) && (TeeTimeRequests.Count > 0))
                {
                    TeeTimeSelection = TeeTimeSelection + 1;
                }
            }
        }

        private void RemoveSelectionChanged(int selectionIndex)
        {
            var selectedItem = TeeTimeRequestsAssigned[selectionIndex];

            TeeTimeRequest teeTimeRequest = selectedItem;
            TeeTime teeTime = teeTimeRequest.TeeTime;

            _teeTimesDirty = true;
            foreach (var player in teeTimeRequest.Players)
            {
                teeTime.RemovePlayer(player);
            }
            teeTimeRequest.TeeTime = null;

            TeeTimeRequests.Insert(0, teeTimeRequest);
            TeeTimeRequestsAssigned.Remove(teeTimeRequest);

            int teeTimeIndex = TournamentTeeTimes.IndexOf(teeTime);
            if (teeTimeIndex != TeeTimeSelection)
            {
                TeeTimeSelection = teeTimeIndex;
            }
            else
            {
                UpdateUnassignedList(4 - teeTime.Players.Count);
            }
        }

        private void TeeTimesSelectionChanged(int selectedIndex)
        {

            if (selectedIndex >= 0)
            {
                TeeTime teeTime = TournamentTeeTimes[selectedIndex];
                if (teeTime.Players.Count > 0)
                {
                    //RemoveListBox.ScrollIntoView(teeTime.Players[0].TeeTimeRequest);
                }
                if (teeTime.Players.Count < 4)
                {
                    UpdateUnassignedList(4 - teeTime.Players.Count);
                }
                else
                {
                    UpdateUnassignedList(4);
                }
            }
        }

        private async Task UploadToWeb(object o)
        {
            _teeTimesDirty = false;

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

                for (int i = 0; i < TournamentTeeTimes.Count; i++)
                {
                    values.Add(new KeyValuePair<string, string>(
                        string.Format("TeeTime[{0}][TournamentKey]", i),
                        TournamentNames[TournamentNameIndex].TournamentKey.ToString(CultureInfo.InvariantCulture)));

                    values.Add(new KeyValuePair<string, string>(
                        string.Format("TeeTime[{0}][StartTime]", i),
                        TournamentTeeTimes[i].StartTime));

                    // TODO: handle shotgun
                    values.Add(new KeyValuePair<string, string>(
                        string.Format("TeeTime[{0}][StartHole]", i),
                        "1"));

                    for (int player = 0; player < TournamentTeeTimes[i].Players.Count; player++)
                    {
                        values.Add(new KeyValuePair<string, string>(
                        string.Format("TeeTime[{0}][Player][{1}]", i, player),
                        TournamentTeeTimes[i].Players[player].Name));

                        values.Add(new KeyValuePair<string, string>(
                        string.Format("TeeTime[{0}][GHIN][{1}]", i, player),
                        TournamentTeeTimes[i].Players[player].GHIN));
                    }
                }

                var content = new FormUrlEncodedContent(values);

                //Logging.Log("Signup Tee Time UploadToWeb", values.ToString());

                using (new WaitCursor())
                {
                    var response = await client.PostAsync(WebAddresses.ScriptFolder + WebAddresses.SubmitTeeTimes, content);
                    var responseString = await response.Content.ReadAsStringAsync();

                    if (responseString.StartsWith("Success", StringComparison.InvariantCultureIgnoreCase))
                    {
                        MessageBox.Show("Tee times uploaded");
                    }
                    else
                    {
                        Credentials.CheckForInvalidPassword(responseString);
                        Logging.Log(WebAddresses.ScriptFolder + WebAddresses.SubmitWaitingList, responseString);

                        HtmlDisplayWindow displayWindow = new HtmlDisplayWindow();
                        displayWindow.WebBrowser.NavigateToString(responseString);
                        displayWindow.Owner = Application.Current.MainWindow;
                        displayWindow.ShowDialog();
                    }
                }

                // TODO error handling
            }
        }

        private void SaveAsCsv(object o)
        {
            // Configure save file dialog box
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = "Teetimes"; // Default file name
            dlg.DefaultExt = ".csv"; // Default file extension
            dlg.Filter = "CSV File (.csv)|*.csv"; // Filter files by extension

            // Show save file dialog box
            bool? result = dlg.ShowDialog();

            // Process save file dialog box results
            if (result == true)
            {
                _teeTimesDirty = false;

                using (TextWriter tw = new StreamWriter(dlg.FileName))
                {
                    for (int i = 0; i < TournamentTeeTimes.Count; i++)
                    {
                        if (TournamentTeeTimes[i].Players.Count > 0)
                        {
                            // TODO: handle shotgun
                            tw.Write(TournamentTeeTimes[i].StartTime);

                            for (int player = 0; player < TournamentTeeTimes[i].Players.Count; player++)
                            {
                                // quote the name if it contains a comma
                                string name = TournamentTeeTimes[i].Players[player].Name.Contains(',')
                                    ? '"' + TournamentTeeTimes[i].Players[player].Name + '"'
                                    : TournamentTeeTimes[i].Players[player].Name;

                                tw.Write("," + name + 
                                    "," + TournamentTeeTimes[i].Players[player].GHIN +
                                    "," + _tournamentTeeTimes[i].Players[player].Handicap);
                            }
                            tw.WriteLine();
                        }
                    }
                }
            }
        }

        private bool CheckContinue()
        {
            if (_teeTimesDirty)
            {
                if (MessageBox.Show("Some tee times have been filled in. Do you want to continue? Your current work will be lost.",
                    "Verify Continue",
                    MessageBoxButton.OKCancel) != MessageBoxResult.OK)
                {
                    return false;
                }
            }

            return true;
        }

        private async Task LoadSignupsFromWeb(object o)
        {
            if (TournamentNames.Count == 0)
            {
                MessageBox.Show("You must select a touranment first");
                return;
            }

            if (!CheckContinue()) return;

            _teeTimesDirty = false;

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(WebAddresses.BaseAddress);

                using (new WaitCursor())
                {
                    var values = new List<KeyValuePair<string, string>>();

                    values.Add(new KeyValuePair<string, string>("tournament",
                        TournamentNames[TournamentNameIndex].TournamentKey.ToString(CultureInfo.InvariantCulture)));

                    var content = new FormUrlEncodedContent(values);

                    var response = await client.PostAsync(WebAddresses.ScriptFolder + WebAddresses.GetSignups, content);
                    var responseString = await response.Content.ReadAsStringAsync();

                    Logging.Log("LoadSignupsFromWeb", responseString);

                    TeeTimeRequests.Clear();
                    TeeTimeRequestsUnassigned.Clear();
                    TeeTimeRequestsAssigned.Clear();

                    TeeTimeRequests = LoadSignupsFromWebResponse(responseString);

                    if (OrderBySignupDate)
                    {
                        TeeTimeRequests.Sort(new SubmitKeySort());
                    }
                    else
                    {
                        TeeTimeRequests.Sort(new TeeTimeRequestSort());
                    }
                    
                    UpdateUnassignedList(4);

                    InitTeeTimes();
                }
            }

            GroupMode = true;
        }

        private class TeeTimeRequestSort : IComparer<TeeTimeRequest>
        {
            public int Compare(TeeTimeRequest a, TeeTimeRequest b)
            {

                return a.GetHour() - b.GetHour();
            }
        }

        private class SubmitKeySort : IComparer<TeeTimeRequest>
        {
            public int Compare(TeeTimeRequest a, TeeTimeRequest b)
            {
                return a.SignupKey - b.SignupKey;
            }
        }

        private class PlayerOrderSort : IComparer<Player>
        {
            public int Compare(Player x, Player y)
            {
                return x.Position - y.Position;
            }
        }

        private class TeeTimeSort : IComparer<TeeTimeRequest>
        {
            public int Compare(TeeTimeRequest a, TeeTimeRequest b)
            {
                if ((a.TeeTime == null) || (b.TeeTime == null)) return 0;
                string aTime = a.TeeTime.StartTime[1] == ':' ? ("0" + a.TeeTime.StartTime) : a.TeeTime.StartTime;
                string bTime = b.TeeTime.StartTime[1] == ':' ? ("0" + b.TeeTime.StartTime) : b.TeeTime.StartTime;
                return string.CompareOrdinal(aTime, bTime);
            }
        }

        private async Task LoadTeeTimesFromWeb(object o)
        {
            if (!CheckContinue()) return;

            _teeTimesDirty = false;
            _removedTeeTimes.Clear();
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(WebAddresses.BaseAddress);

                using (new WaitCursor())
                {
                    var values = new List<KeyValuePair<string, string>>();

                    values.Add(new KeyValuePair<string, string>("tournament",
                        TournamentNames[TournamentNameIndex].TournamentKey.ToString(CultureInfo.InvariantCulture)));

                    var content = new FormUrlEncodedContent(values);

                    var response = await client.PostAsync(WebAddresses.ScriptFolder + WebAddresses.GetTeeTimes, content);
                    var responseString = await response.Content.ReadAsStringAsync();

                    Logging.Log("LoadTeeTimesFromWeb", responseString);

                    LoadTeeTimesFromWebResponse(responseString);
                    LoadSignupsFromTeeTimes();
                }
            }

            GroupMode = false;
        }

        public void LoadTeeTimesFromWebResponse(string webResponse)
        {
            TournamentTeeTimes.Clear();

            string[] responseLines = webResponse.Split('\n');

            string[][] lines = CSVParser.Parse(responseLines);
            int lineNumber = 0;
            for (int lineIndex = 0, playerIndex = 0; lineIndex < lines.Length; lineIndex++, playerIndex++)
            {
                lineNumber++;
                string[] fields = lines[lineIndex];

                if (fields.Length == 0) continue;

                TeeTime teeTime = new TeeTime();

                teeTime.StartTime = fields[0];

                // The rest of the fields are Name/GHIN doubles
                int playerPosition = 1;
                for (int i = 1; i < fields.Length; i += 3)
                {
                    Player player = new Player();

                    if (string.IsNullOrWhiteSpace(fields[i]))
                    {
                        // TODO: this is only needed for partner tournaments
                        //if (teeTime.Players.Count > 0)
                        //{
                        //    teeTime.AddPlayer(player);
                        //}
                    }
                    else
                    {
                        player.Position = playerPosition;
                        playerPosition++;
                        player.Name = fields[i].Trim();
                        if (string.IsNullOrWhiteSpace(fields[i + 1]))
                        {
                            throw new ArgumentException(string.Format("Website response: line {0}: missing GHIN number: {1}", lineNumber, string.Join(",", fields)));
                        }
                        player.GHIN = fields[i + 1].Trim();
                        if (string.IsNullOrWhiteSpace(fields[i + 1]))
                        {
                            throw new ArgumentException(string.Format("Website response: line {0}: missing handicap: {1}", lineNumber, string.Join(",", fields)));
                        }
                        player.Handicap = fields[i + 2].Trim();

                        player.TeeTime = teeTime;
                        teeTime.AddPlayer(player);
                    }
                }

                TournamentTeeTimes.Add(teeTime);
            }
        }

        public void LoadTeeTimesFromFile(string fileName)
        {
            _removedTeeTimes.Clear();
            TournamentTeeTimes.Clear();
            TeeTimes.Clear();
            using (TextReader tr = new StreamReader(fileName))
            {
                string line;
                while ((line = tr.ReadLine()) != null)
                {
                    if (!string.IsNullOrEmpty(line))
                    {
                        TournamentTeeTimes.Add(new TeeTime { StartTime = line });
                        if (TeeTimes.Count < 16)
                        {
                            TeeTimes.Add(line);
                        }
                    }
                }
            }
        }

        public void ClearPlayers()
        {
            foreach (var teeTime in TournamentTeeTimes)
            {
                teeTime.ClearPlayers();
            }
        }

        public void LoadSignupsFromTeeTimes()
        {
            TeeTimeRequests.Clear();
            TeeTimeRequestsUnassigned.Clear();
            TeeTimeRequestsAssigned.Clear();

            foreach (var teeTime in TournamentTeeTimes)
            {
                foreach (var player in teeTime.Players)
                {
                    TeeTimeRequest teeTimeRequest = new TeeTimeRequest();
                    teeTimeRequest.Players.Add(player);
                    teeTimeRequest.TeeTime = player.TeeTime;
                    //player.TeeTimeRequest = teeTimeRequest;
                    TeeTimeRequestsAssigned.Add(teeTimeRequest);
                }
            }

            UpdateUnassignedList(4);
        }

        private void LoadSignupsFromFile()
        {
            string[] signUpFiles = Directory.GetFiles(".", "*TournamentEntries.csv");
            switch (signUpFiles.Length)
            {
                case 0: MessageBox.Show("No TournamentEntries.csv files found "); return;
                case 1: LoadFromFile(signUpFiles[0]); break;
                default: MessageBox.Show("More than 1 TournamentEntries.csv files found"); return;
            }
        }

        public void LoadFromFile(string fileName)
        {
            TeeTimeRequests.Clear();
            TeeTimeRequestsUnassigned.Clear();
            TeeTimeRequestsAssigned.Clear();

            using (TextReader tr = new StreamReader(fileName))
            {
                string line;
                int lineNumber = 0;

                while ((line = tr.ReadLine()) != null)
                {
                    lineNumber++;
                    TeeTimeRequest teeTimeRequest = new TeeTimeRequest();

                    string[] fields = line.Split(',');
                    teeTimeRequest.Preference = fields[0];

                    // The rest of the fields are Name/GHIN/handicap triples
                    int playerPosition = 1;
                    for (int i = 1; i < fields.Length; i += 3)
                    {
                        if (string.IsNullOrEmpty(fields[i]))
                        {
                            continue;
                        }

                        Player player = new Player();
                        player.Position = playerPosition;
                        playerPosition++;
                        player.Name = fields[i];
                        if (string.IsNullOrEmpty(fields[i + 1]))
                        {
                            throw new ArgumentException(string.Format("{0}: line {1}: missing GHIN number", fileName, lineNumber));
                        }
                        player.GHIN = fields[i + 1];

                        //player.TeeTimeRequest = teeTimeRequest;
                        teeTimeRequest.Players.Add(player);
                    }

                    if (teeTimeRequest.Players.Count == 0)
                    {
                        throw new ArgumentException(string.Format("{0}: line {1} does not have any players", fileName, lineNumber));
                    }

                    TeeTimeRequests.Add(teeTimeRequest);
                }
            }

            UpdateUnassignedList(4);
        }

        private void AddPlayer(object o)
        {
            List<GHINEntry> ghinList = new List<GHINEntry>();
            try
            {
                ghinList = GHINEntry.LoadGHIN(Options.GHINFileName);
            }
            catch (Exception)
            {
                // no error if file doesn't exist
            }

            AddPlayerWindow apw = new AddPlayerWindow();
            Player player = new Player();
            player.Position = 1;
            apw.DataContext = player;
            apw.Player = player;
            apw.GHINList = ghinList;
            apw.Owner = Application.Current.MainWindow;

            apw.ShowDialog();
            if (apw.DialogResult.HasValue && apw.DialogResult.Value)
            {
                TeeTimeRequest ttr = new TeeTimeRequest();
                ttr.Players = new TrulyObservableCollection<Player> { player };
                ttr.Preference = "None";

                // Only the last name is entered on the web site, so 
                // only use the last name here.
                string[] fields = player.Name.Split(',');
                player.Name = fields[0].Trim();

                foreach (var request in TeeTimeRequestsUnassigned)
                {
                    foreach (var p in request.Players)
                    {
                        if (!string.IsNullOrEmpty(player.GHIN) && (String.CompareOrdinal(player.GHIN, p.GHIN) == 0))
                        {
                            MessageBox.Show(Application.Current.MainWindow,
                                player.Name + " is already in the signup list at tee time preference " + request.Preference);
                            return;
                        }
                    }
                }

                foreach (var request in TeeTimeRequestsAssigned)
                {
                    foreach (var p in request.Players)
                    {
                        if (!string.IsNullOrEmpty(player.GHIN) && (String.CompareOrdinal(player.GHIN, p.GHIN) == 0))
                        {
                            MessageBox.Show(Application.Current.MainWindow,
                                player.Name + " is already in the signup list at tee time preference " + request.Preference);
                            return;
                        }
                    }
                }

                TeeTimeRequestsUnassigned.Insert(0, ttr);
            }
        }

        //
        // Expected format of tee sheet file: header followed by individual lines.
        // 
        // Tee Time,Handle,GHIN
        // 7:00 AM,"Albitz, Paul",9079663

        private void UploadVpCsv(object o)
        {
            if(string.IsNullOrEmpty(GgTeeTimeFile))
            {
                MessageBox.Show("Please fill in the tee sheet file");
                return;
            }

            if (!File.Exists(GgTeeTimeFile))
            {
                MessageBox.Show("File does not exist: " + GgTeeTimeFile);
                return;
            }

            using (TextReader tr = new StreamReader(GgTeeTimeFile))
            {
                _removedTeeTimes.Clear();
                ClearPlayers();
                TournamentTeeTimes.Clear();

                TeeTimeRequests.Clear();
                TeeTimeRequestsUnassigned.Clear();
                TeeTimeRequestsAssigned.Clear();

                string[][] lines = CSVParser.Parse(tr);

                int teeTimeColumn = -1;
                int handleColumn = -1;
                int ghinColumn = -1;

                if (lines.Length == 0)
                {
                    throw new ApplicationException(GgTeeTimeFile + ": has 0 lines");
                }

                for (int col = 0; col < lines[0].Length; col++)
                {
                    if (string.Compare(lines[0][col], "tee time", true) == 0)
                    {
                        teeTimeColumn = col;
                    }
                    else if (string.Compare(lines[0][col], "handle", true) == 0)
                    {
                        handleColumn = col;
                    }
                    else if (string.Compare(lines[0][col], "ghin", true) == 0)
                    {
                        ghinColumn = col;
                    }
                }

                if (teeTimeColumn == -1)
                {
                    throw new ApplicationException(GgTeeTimeFile + ": did not find header column: Tee Time");
                }
                if (handleColumn == -1)
                {
                    throw new ApplicationException(GgTeeTimeFile + ": did not find header column: Handle");
                }
                if (ghinColumn == -1)
                {
                    throw new ApplicationException(GgTeeTimeFile + ": did not find header column: GHIN");
                }

                for(int lineIndex = 1, playerIndex = 0; lineIndex < lines.Length; lineIndex++, playerIndex++)
                {
                    string[] line = lines[lineIndex];
                    if (line.Length > 0)
                    {
                        if (string.IsNullOrEmpty(line[teeTimeColumn]))
                        {
                            throw new ApplicationException(GgTeeTimeFile + " (line " + lineIndex + "): Tee Time is empty");
                        }
                        if (string.IsNullOrEmpty(line[handleColumn]))
                        {
                            throw new ApplicationException(GgTeeTimeFile + " (line " + lineIndex + "): Handle (last name, first name) is empty");
                        }
                        // Since the GHIN is not actually used, don't require it

                        TeeTime tt = null;
                        foreach(var teeTime in TournamentTeeTimes)
                        {
                            if(String.CompareOrdinal(teeTime.StartTime, line[teeTimeColumn]) == 0)
                            {
                                tt = teeTime;
                                break;
                            }
                        }

                        if(tt == null)
                        {
                            tt = new TeeTime();
                            tt.StartTime = line[teeTimeColumn];
                            TournamentTeeTimes.Add(tt);
                        }

                        Player player = new Player();
                        player.Position = 1;
                        player.Name = line[handleColumn].Trim();
                        player.GHIN = line[ghinColumn].Trim();

                        player.TeeTime = tt;
                        tt.AddPlayer(player);
                    }
                }
            }
        }

        private async Task UploadWaitingListFile(object o)
        {
            if (string.IsNullOrEmpty(WaitingListFile))
            {
                MessageBox.Show("Please fill in the waiting list file");
                return;
            }

            if (!File.Exists(WaitingListFile))
            {
                MessageBox.Show("File does not exist: " + WaitingListFile);
                return;
            }

            List<SignUpWaitingListEntry> waitingList = new List<SignUpWaitingListEntry>();

            using (TextReader tr = new StreamReader(WaitingListFile))
            {
                string[][] lines = CSVParser.Parse(tr);

                for (int lineIndex = 0, playerIndex = 0; lineIndex < lines.Length; lineIndex++, playerIndex++)
                {
                    string[] line = lines[lineIndex];
                    if (line.Length > 0)
                    {
                        SignUpWaitingListEntry waitingListEntry = new SignUpWaitingListEntry();

                        waitingListEntry.Position = waitingList.Count;

                        waitingListEntry.Name1 = line[0].Trim();
                        if(line.Length > 1)
                        {
                            waitingListEntry.Name2 = line[1].Trim();
                        }
                        if (line.Length > 2)
                        {
                            waitingListEntry.Name3 = line[2].Trim();
                        }
                        if (line.Length > 3)
                        {
                            waitingListEntry.Name4 = line[3].Trim();
                        }
                        waitingList.Add(waitingListEntry);
                    }
                }
            }

            using (var client = new HttpClient())
            {
                // cancelled password input
                if (string.IsNullOrEmpty(Credentials.LoginPassword))
                {
                    return;
                }

                client.BaseAddress = new Uri(WebAddresses.BaseAddress);

                var values = new List<KeyValuePair<string, string>>();

                values.Add(new KeyValuePair<string, string>("Login", Credentials.LoginName));
                values.Add(new KeyValuePair<string, string>("Password", Credentials.LoginPassword));

                values.Add(new KeyValuePair<string, string>("Tournament",
                        TournamentNames[TournamentNameIndex].TournamentKey.ToString(CultureInfo.InvariantCulture)));

                for (int i = 0; i < waitingList.Count; i++)
                {
                    values.Add(new KeyValuePair<string, string>(
                        string.Format("SignUpsWaitingList[{0}][Position]", i),
                        waitingList[i].Position.ToString(CultureInfo.InvariantCulture)));

                    values.Add(new KeyValuePair<string, string>(
                        string.Format("SignUpsWaitingList[{0}][GHIN1]", i),
                        waitingList[i].GHIN1.ToString(CultureInfo.InvariantCulture)));

                    values.Add(new KeyValuePair<string, string>(
                        string.Format("SignUpsWaitingList[{0}][Name1]", i),
                        waitingList[i].Name1));

                    values.Add(new KeyValuePair<string, string>(
                        string.Format("SignUpsWaitingList[{0}][GHIN2]", i),
                        waitingList[i].GHIN2.ToString(CultureInfo.InvariantCulture)));

                    values.Add(new KeyValuePair<string, string>(
                        string.Format("SignUpsWaitingList[{0}][Name2]", i),
                        waitingList[i].Name2));

                    values.Add(new KeyValuePair<string, string>(
                        string.Format("SignUpsWaitingList[{0}][GHIN3]", i),
                        waitingList[i].GHIN3.ToString(CultureInfo.InvariantCulture)));

                    values.Add(new KeyValuePair<string, string>(
                        string.Format("SignUpsWaitingList[{0}][Name3]", i),
                        waitingList[i].Name3));

                    values.Add(new KeyValuePair<string, string>(
                        string.Format("SignUpsWaitingList[{0}][GHIN4]", i),
                        waitingList[i].GHIN4.ToString(CultureInfo.InvariantCulture)));

                    values.Add(new KeyValuePair<string, string>(
                        string.Format("SignUpsWaitingList[{0}][Name4]", i),
                        waitingList[i].Name4));

                }

                var content = new FormUrlEncodedContent(values);

                using (new WaitCursor())
                {
                    var response = await client.PostAsync(WebAddresses.ScriptFolder + WebAddresses.SubmitSignUpsWaitingList, content);
                    var responseString = await response.Content.ReadAsStringAsync();

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
                        displayWindow.Owner = Application.Current.MainWindow;
                        displayWindow.ShowDialog();
                    }
                }
            }
        }

        private void Print(object o)
        {
            _teeTimesDirty = false;

            var diag = new PrintDialog();
            var paragraph = new Paragraph();
            foreach (var teeTime in TournamentTeeTimes)
            {
                if (teeTime.Players.Count > 0)
                {
                    paragraph.Inlines.Add(new Run(teeTime + Environment.NewLine));
                }
            }

            if (paragraph.Inlines.Count > 0)
            {
                var doc = new FlowDocument(paragraph);
                doc.PagePadding = new Thickness(100);

                bool? print = diag.ShowDialog();
                if (print == true)
                {
                    diag.PrintDocument(((IDocumentPaginatorSource) doc).DocumentPaginator, "Tee Times");
                }
            }
            else
            {
                MessageBox.Show("There are no tee times with players assigned");
            }
        }

        private void SwitchToGroupMode()
        {
            Dictionary<int, TeeTimeRequest> ttrMap = new Dictionary<int, TeeTimeRequest>();

            if ((TeeTimeRequests != null) && (TeeTimeRequests.Count > 0))
            {
                foreach (var teeTimeRequest in TeeTimeRequests)
                {
                    if (ttrMap.ContainsKey(teeTimeRequest.SignupKey))
                    {
                        ttrMap[teeTimeRequest.SignupKey].Players.Add(teeTimeRequest.Players[0]);
                    }
                    else
                    {
                        ttrMap.Add(teeTimeRequest.SignupKey, teeTimeRequest);
                    }
                }

                TeeTimeRequests.Clear();
                foreach (var ttr in ttrMap.Values)
                {
                    ttr.Players = TrulyObservableCollection<Player>.Sort(ttr.Players, new PlayerOrderSort());
                    TeeTimeRequests.Add(ttr);
                }
                SortTeeTimeRequests();
            }

            ttrMap.Clear();

            if ((TeeTimeRequestsAssigned != null) && (TeeTimeRequestsAssigned.Count > 0))
            {
                foreach (var teeTimeRequest in TeeTimeRequestsAssigned)
                {
                    if (ttrMap.ContainsKey(teeTimeRequest.SignupKey))
                    {
                        ttrMap[teeTimeRequest.SignupKey].Players.Add(teeTimeRequest.Players[0]);
                    }
                    else
                    {
                        ttrMap.Add(teeTimeRequest.SignupKey, teeTimeRequest);
                    }
                }

                TeeTimeRequestsAssigned.Clear();
                foreach (var ttr in ttrMap.Values)
                {
                    TeeTimeRequestsAssigned.Add(ttr);
                }

                TeeTimeRequestsAssigned = TrulyObservableCollection<TeeTimeRequest>.Sort(TeeTimeRequestsAssigned,
                    new TeeTimeSort());
            }
        }

        private void SwitchToIndividualMode()
        {
            if ((TeeTimeRequests != null) && (TeeTimeRequests.Count > 0))
            {
                List<TeeTimeRequest> individualTeeTimeRequests = new List<TeeTimeRequest>();

                foreach (var teeTimeRequest in TeeTimeRequests)
                {
                    foreach (var player in teeTimeRequest.Players)
                    {
                        TeeTimeRequest ttr = (TeeTimeRequest) teeTimeRequest.Clone();
                        ttr.Players = new TrulyObservableCollection<Player>();
                        ttr.Players.Add(player);

                        individualTeeTimeRequests.Add(ttr);
                    }
                }

                TeeTimeRequests = individualTeeTimeRequests;
                SortTeeTimeRequests();
            }

            if ((TeeTimeRequestsAssigned != null) && (TeeTimeRequestsAssigned.Count > 0))
            {
                TrulyObservableCollection<TeeTimeRequest> individualTeeTimeRequestsAssigned = new TrulyObservableCollection<TeeTimeRequest>();

                foreach (var teeTimeRequest in TeeTimeRequestsAssigned)
                {
                    foreach (var player in teeTimeRequest.Players)
                    {
                        TeeTimeRequest ttr = (TeeTimeRequest)teeTimeRequest.Clone();
                        ttr.Players = new TrulyObservableCollection<Player>();
                        ttr.Players.Add(player);

                        individualTeeTimeRequestsAssigned.Add(ttr);
                    }
                }

                TeeTimeRequestsAssigned = TrulyObservableCollection<TeeTimeRequest>.Sort(individualTeeTimeRequestsAssigned,
                    new TeeTimeSort());
            }
        }
    }
}
