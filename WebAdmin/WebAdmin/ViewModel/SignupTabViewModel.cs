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
            "6:00 AM", "6:10 AM", "6:20 AM", "6:30 AM", "6:40 AM", "6:50 AM",
            "7:00 AM", "7:10 AM", "7:20 AM", "7:30 AM", "7:40 AM", "7:50 AM",
            "8:00 AM", "8:10 AM", "8:20 AM", "8:30 AM", "8:40 AM", "8:50 AM",
            "9:00 AM", "9:10 AM", "9:20 AM", "9:30 AM", "9:40 AM", "9:50 AM",
            "10:00 AM", "10:10 AM", "10:20 AM", "10:30 AM", "10:40 AM", "10:50 AM",
            "11:00 AM", "11:10 AM", "11:20 AM", "11:30 AM", "11:40 AM", "11:50 AM",
        };
        //{ 
        //    "6:00 AM", "6:07 AM", "6:15 AM", "6:22 AM", "6:30 AM", "6:37 AM", "6:45 AM", "6:52 AM",
        //    "7:00 AM", "7:07 AM", "7:15 AM", "7:22 AM", "7:30 AM", "7:37 AM", "7:45 AM", "7:52 AM",
        //    "8:00 AM", "8:07 AM", "8:15 AM", "8:22 AM", "8:30 AM", "8:37 AM", "8:45 AM", "8:52 AM",
        //    "9:00 AM", "9:07 AM", "9:15 AM", "9:22 AM", "9:30 AM", "9:37 AM", "9:45 AM", "9:52 AM",
        //    "10:00 AM", "10:07 AM", "10:15 AM", "10:22 AM", "10:30 AM", "10:37 AM", "10:45 AM", "10:52 AM",
        //    "11:00 AM", "11:07 AM", "11:15 AM", "11:22 AM", "11:30 AM", "11:37 AM", "11:45 AM", "11:52 AM"};

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

        DateTime _lastSelectionTime = DateTime.Now;
        private int _lastSelection = -1;

        private int _todoSelection;
        public int TodoSelection { get { return _todoSelection; } 
            set {
                // There is a timing bug. Sometimes, when the unassigned list
                // is updated during a selection index change event, there is a 2nd event
                // that comes in right away with the same value.
                var timeSinceLastEvent = DateTime.Now - _lastSelectionTime;
                if ((_lastSelection == value) &&  (timeSinceLastEvent.TotalMilliseconds < 150))
                {
                    System.Diagnostics.Debug.WriteLine("Duplicate selection event ms: " + timeSinceLastEvent.TotalMilliseconds);
                    return;
                }
                _lastSelectionTime = DateTime.Now;
                _lastSelection = value;

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

        //public ICommand PrintCommand { get { return new ModelCommand(Print); } }

        public ICommand UploadGgCsvCommand { get { return new ModelCommand(UploadGgCsv); } }

        public ICommand UploadWaitingListFileCommand { get { return new ModelCommand(async s => await UploadWaitingListFile(s)); } }
        #endregion

        public SignupTabViewModel()
        {
            TournamentNames = new TrulyObservableCollection<TournamentName>();
            TeeTimeRequests = new List<TeeTimeRequest>();
            TeeTimeRequestsUnassigned = new TrulyObservableCollection<TeeTimeRequest>();
            TeeTimeRequestsAssigned = new TrulyObservableCollection<TeeTimeRequest>();
            TournamentTeeTimes = new TrulyObservableCollection<TeeTime>();
            TodoSelection = -1;
            RemoveSelection = -1;
            TeeTimes = new List<string>();
            InitTeeTimes();
            FirstTeeTimeIndex = 0;
            GetTournamentsVisible = Visibility.Visible;
            GotTournamentsVisible = Visibility.Collapsed;

            // Only allow the start time to be shifted by 2hrs (the first 16
            // tee times)
            for (int i = 0; (i < 16) && (i < TournamentTeeTimes.Count); i++)
            {
                TeeTimes.Add(TournamentTeeTimes[i].StartTime);
            }
        }

        public void InitTeeTimes()
        {
            ClearPlayers();
            TournamentTeeTimes.Clear();

            // TODO shotgun
            for(int i = FirstTeeTimeIndex; i < _defaultTeeTimes.Count; i++)
            {
                TournamentTeeTimes.Add(new TeeTime { StartTime = _defaultTeeTimes[i] });
            }
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
            var oldTournamentTeeTimes = TournamentTeeTimes;
            TournamentTeeTimes = new TrulyObservableCollection<TeeTime>();
            InitTeeTimes();

            int i;
            for(i = 0; (i < oldTournamentTeeTimes.Count) && (i < TournamentTeeTimes.Count); i++)
            {
                if (oldTournamentTeeTimes[i].Players.Count > 0)
                {
                    // Copy players to new list
                    foreach (var player in oldTournamentTeeTimes[i].Players)
                    {
                        TournamentTeeTimes[i].AddPlayer(player);
                    }

                    // Need to update the TeeTime object in the assigned list to the new TeeTime object.
                    // There can be multiple assigned objects pointing to a single tee time.
                    for (int assignedIndex = 0; assignedIndex < TeeTimeRequestsAssigned.Count; assignedIndex++)
                    {
                        if (TeeTimeRequestsAssigned[assignedIndex].TeeTime == oldTournamentTeeTimes[i])
                        {
                            TeeTimeRequestsAssigned[assignedIndex].TeeTime = TournamentTeeTimes[i];
                        }
                    }
                }
            }

            // Changing the TeeTime object does not trigger a property
            // changed event (intentionally), so do something to trigger
            // the list to update in the UI.
            var save = TeeTimeRequestsAssigned;
            TeeTimeRequestsAssigned = null;
            TeeTimeRequestsAssigned = save;

            // If there were people at the end of the old list that would
            // lose their tee time, put them back in the unassigned list.
            for (; i < oldTournamentTeeTimes.Count; i++)
            {
                if (oldTournamentTeeTimes[i].Players.Count > 0)
                {
                    // Copy the list, since removing a player from the unassigned list
                    // will alter the player list.
                    List<Player> players = new List<Player>();
                    foreach (var player in oldTournamentTeeTimes[i].Players)
                    {
                        players.Add(player);
                    }
                    foreach (var playerToRemove in players)
                    {
                        var removed = false;
                        // Start at the end of the assigned list to find the player
                        for (int assignedIndex = TeeTimeRequestsAssigned.Count - 1; !removed && (assignedIndex >= 0); assignedIndex--)
                        {
                            // Go through all the players in the tee time request. Copy
                            // the list first since removing a tee time request from the unassigned
                            // list will change the list.
                            List<Player> assignedPlayers = new List<Player>();
                            foreach (var player in TeeTimeRequestsAssigned[assignedIndex].Players)
                            {
                                assignedPlayers.Add(player);
                            }
                            for (int assignedPlayerIndex = 0; !removed && (assignedPlayerIndex < assignedPlayers.Count); assignedPlayerIndex++)
                            {
                                // If the players match, remove them from both the assigned list and the tee time
                                if (assignedPlayers[assignedPlayerIndex] == playerToRemove)
                                {
                                    RemoveSelectionChanged(assignedIndex);
                                    removed = true;
                                }
                            }
                        }
                    }
                }
            }

            // Reset the current tee time selection to be
            // the first tee time with openings.
            i = 0;
            for (; i < TournamentTeeTimes.Count; i++)
            {
                if (TournamentTeeTimes[i].Players.Count < 4)
                {
                    // This also updates the unassigned list
                    TeeTimeSelection = i;
                    break;
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

            // Create a new list and assign it as a whole, rather
            // than modifying the existing list. (fewer change events)
            TrulyObservableCollection<TeeTimeRequest> teeTimeRequestsUnassigned = new TrulyObservableCollection<TeeTimeRequest>();
            foreach (var teeTimeRequest in TeeTimeRequests)
            {
                if (teeTimeRequest.Players.Count <= playerCount)
                {
                    teeTimeRequestsUnassigned.Add(teeTimeRequest);
                }
            }

            TeeTimeRequestsUnassigned = teeTimeRequestsUnassigned;
        }

        private void TodoSelectionChanged(int selectionIndex)
        {
            if (selectionIndex < 0) return;

            var selectedItem = TeeTimeRequestsUnassigned[selectionIndex];

            if (TeeTimeSelection < 0)
            {
                // By assigning the property, it triggers
                // rebuilding the unassigned list. So,
                // assign the variable directly.
                _teeTimeSelection = 0;
            }

            TeeTimeRequest teeTimeRequest = selectedItem;
            TeeTime teeTime = TournamentTeeTimes[TeeTimeSelection];

            // Check for room in this tee time
            if (teeTimeRequest.Players.Count > (4 - teeTime.Players.Count))
            {
                MessageBox.Show("Not enough room for all players at " + teeTime.StartTime);
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
                        // This also updates the unassigned list
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
                    // This also updates the unassigned list
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
                // This also updates the unassigned list
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

                UpdateUnassignedList(4 - teeTime.Players.Count);
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
                    tw.WriteLine("Tee Time,Last Name,First Name,GHIN,Team Id,Email");

                    int teamId = 0;
                    for (int teeTimeNumber = 0; teeTimeNumber < TournamentTeeTimes.Count; teeTimeNumber++)
                    {
                        for (int player = 0; player < 4; player++)
                        {
                            // TODO: handle shotgun
                            string playerLastName = string.Empty;
                            string playerFirstName = string.Empty;
                            string playerGhin = string.Empty;
                            string playerEmail = string.Empty;

                            if (player < TournamentTeeTimes[teeTimeNumber].Players.Count)
                            {
                                string[] fields = TournamentTeeTimes[teeTimeNumber].Players[player].Name.Split(',');
                                if (fields.Length > 1)
                                {
                                    playerLastName = fields[0].Trim();
                                    playerFirstName = fields[1].Trim();
                                }
                                else
                                {
                                    playerLastName = fields[0];
                                }

                                // Check for Jr or Jr. in the last name and move to first name
                                fields = playerLastName.Split(' ');
                                if (fields.Length > 1)
                                {
                                    if (fields[fields.Length - 1].ToLower().StartsWith("jr"))
                                    {
                                        // Move the Jr. to the first name
                                        playerFirstName += " " + fields[fields.Length - 1];
                                        // Remove the Jr. from the last name
                                        playerLastName = playerLastName.Replace(fields[fields.Length - 1], "").Trim();
                                    }
                                }
                                playerGhin = TournamentTeeTimes[teeTimeNumber].Players[player].GHIN;
                                playerEmail = TournamentTeeTimes[teeTimeNumber].Players[player].Email;
                                // Make sure the email address is valid
                                if (string.IsNullOrEmpty(playerEmail) || !playerEmail.Contains("@"))
                                {
                                    playerEmail = string.Empty;
                                }
                            }

                            // Only write out tee time entries if there is a player
                            if (!string.IsNullOrEmpty(playerLastName))
                            {
                                if ((TournamentNames[TournamentNameIndex].TeamSize == 4) && (player == 0))
                                {
                                    // Update team ID on the first player of the group
                                    teamId++;
                                }
                                else if ((TournamentNames[TournamentNameIndex].TeamSize == 2) && (player % 2 == 0))
                                {
                                    // Update team ID on the first player and third player of group
                                    teamId++;
                                }
                                else if (TournamentNames[TournamentNameIndex].TeamSize == 1)
                                {
                                    // For individual tournaments, update the team number per player
                                    teamId++;
                                }

                                // Note: Golf Genius requires Last Name, so it is not enough to provide just the handle
                                //string handle = string.Empty;
                                //if (player < TournamentTeeTimes[teeTimeNumber].Players.Count)
                                //{
                                //    handle = '"' + TournamentTeeTimes[teeTimeNumber].Players[player].Name + '"';
                                //    playerGhin = TournamentTeeTimes[teeTimeNumber].Players[player].GHIN;
                                //}

                                tw.Write(TournamentTeeTimes[teeTimeNumber].StartTime + ",");
                                tw.Write(playerLastName + ",");
                                tw.Write(playerFirstName + ",");
                                //tw.Write(handle + ",");
                                tw.Write(playerGhin + ",");
                                tw.Write(teamId + ",");
                                tw.Write(playerEmail);
                                tw.WriteLine();                             
                            }
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

                    TeeTimeRequests = LoadSignupsFromWebResponseJson(responseString);

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
            player.Position = 0;
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
        // Tee Time,Last Name,First Name,GHIN
        // 7:07 AM,Albitz,Paul,9079663

        private void UploadGgCsv(object o)
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
                ClearPlayers();
                TournamentTeeTimes.Clear();

                TeeTimeRequests.Clear();
                TeeTimeRequestsUnassigned.Clear();
                TeeTimeRequestsAssigned.Clear();

                string[][] lines = CSVParser.Parse(tr);

                int teeTimeColumn = -1;
                int lastNameColumn = -1;
                int firstNameColumn = -1;
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
                    else if (string.Compare(lines[0][col], "last name", true) == 0)
                    {
                        lastNameColumn = col;
                    }
                    else if (string.Compare(lines[0][col], "first name", true) == 0)
                    {
                        firstNameColumn = col;
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
                if (lastNameColumn == -1)
                {
                    throw new ApplicationException(GgTeeTimeFile + ": did not find header column: Last Name");
                }
                if (firstNameColumn == -1)
                {
                    throw new ApplicationException(GgTeeTimeFile + ": did not find header column: First Name");
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
                        if (string.IsNullOrEmpty(line[lastNameColumn]))
                        {
                            throw new ApplicationException(GgTeeTimeFile + " (line " + lineIndex + "): Last Name is empty");
                        }
                        // Since the GHIN is not actually used, don't require it
                        // First name is optional too

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

                            bool added = false;
                            for (int timeIndex = 0; (timeIndex < TournamentTeeTimes.Count) && !added; timeIndex++)
                            {
                                var existingTime = DateTime.Parse(TournamentTeeTimes[timeIndex].StartTime);
                                var newTime = DateTime.Parse(tt.StartTime);
                                if (newTime < existingTime)
                                {
                                    TournamentTeeTimes.Insert(timeIndex, tt);
                                    added = true;
                                }
                            }
                            if (!added)
                            {
                                TournamentTeeTimes.Add(tt);
                            }
                            
                        }

                        Player player = new Player();
                        player.Position = tt.Players.Count + 1;
                        if (!string.IsNullOrEmpty(line[firstNameColumn]))
                        {
                            player.Name = line[lastNameColumn].Trim() + ", " + line[firstNameColumn].Trim();
                        }
                        else
                        {
                            player.Name = line[lastNameColumn].Trim();
                        }
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

        //private void Print(object o)
        //{
        //    _teeTimesDirty = false;

        //    var diag = new PrintDialog();
        //    var paragraph = new Paragraph();
        //    foreach (var teeTime in TournamentTeeTimes)
        //    {
        //        if (teeTime.Players.Count > 0)
        //        {
        //            paragraph.Inlines.Add(new Run(teeTime + Environment.NewLine));
        //        }
        //    }

        //    if (paragraph.Inlines.Count > 0)
        //    {
        //        var doc = new FlowDocument(paragraph);
        //        doc.PagePadding = new Thickness(100);

        //        bool? print = diag.ShowDialog();
        //        if (print == true)
        //        {
        //            diag.PrintDocument(((IDocumentPaginatorSource) doc).DocumentPaginator, "Tee Times");
        //        }
        //    }
        //    else
        //    {
        //        MessageBox.Show("There are no tee times with players assigned");
        //    }
        //}

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
