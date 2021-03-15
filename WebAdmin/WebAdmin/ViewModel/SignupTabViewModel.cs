using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Net.Http;
using System.Windows.Input;
using WebAdmin.View;
using System.Web.Script.Serialization;

namespace WebAdmin.ViewModel
{
    public class SignupTabViewModel : TabViewModelBase
    {

        private enum TeeTimeStatus { TeeTime, Waitlisted, Cancelled };

        #region Properties
        public override string Header { get { return "Signup"; } }

        private List<string> _defaultTeeTimes;

        private readonly List<string> _defaultTeeTimes10 = new List<string>
        {
            "6:00 AM", "6:10 AM", "6:20 AM", "6:30 AM", "6:40 AM", "6:50 AM",
            "7:00 AM", "7:10 AM", "7:20 AM", "7:30 AM", "7:40 AM", "7:50 AM",
            "8:00 AM", "8:10 AM", "8:20 AM", "8:30 AM", "8:40 AM", "8:50 AM",
            "9:00 AM", "9:10 AM", "9:20 AM", "9:30 AM", "9:40 AM", "9:50 AM",
            "10:00 AM", "10:10 AM", "10:20 AM", "10:30 AM", "10:40 AM", "10:50 AM",
            "11:00 AM", "11:10 AM", "11:20 AM", "11:30 AM", "11:40 AM", "11:50 AM",
        };

        private readonly List<string> _defaultTeeTimes78 = new List<string>
        {
            "6:00 AM", "6:07 AM", "6:15 AM", "6:22 AM", "6:30 AM", "6:37 AM", "6:45 AM", "6:52 AM",
            "7:00 AM", "7:07 AM", "7:15 AM", "7:22 AM", "7:30 AM", "7:37 AM", "7:45 AM", "7:52 AM",
            "8:00 AM", "8:07 AM", "8:15 AM", "8:22 AM", "8:30 AM", "8:37 AM", "8:45 AM", "8:52 AM",
            "9:00 AM", "9:07 AM", "9:15 AM", "9:22 AM", "9:30 AM", "9:37 AM", "9:45 AM", "9:52 AM",
            "10:00 AM", "10:07 AM", "10:15 AM", "10:22 AM", "10:30 AM", "10:37 AM", "10:45 AM", "10:52 AM",
            "11:00 AM", "11:07 AM", "11:15 AM", "11:22 AM", "11:30 AM", "11:37 AM", "11:45 AM", "11:52 AM"
        };

        private readonly List<string> _defaultTeeTimes98 = new List<string>
        {
            "6:00 AM", "6:09 AM", "6:17 AM", "6:26 AM", "6:34 AM", "6:43 AM", "6:51 AM",
            "7:00 AM", "7:09 AM", "7:17 AM", "7:26 AM", "7:34 AM", "7:43 AM", "7:51 AM",
            "8:00 AM", "8:09 AM", "8:17 AM", "8:26 AM", "8:34 AM", "8:43 AM", "8:51 AM",
            "9:00 AM", "9:09 AM", "9:17 AM", "9:26 AM", "9:34 AM", "9:43 AM", "9:51 AM",
            "10:00 AM", "10:09 AM", "10:17 AM", "10:26 AM", "10:34 AM", "10:43 AM", "10:51 AM",
            "11:00 AM", "11:09 AM", "11:17 AM", "11:26 AM", "11:34 AM", "11:43 AM", "11:51 AM"

        };

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
                BlindDrawPlayerCount = TournamentTeeTimes.Count * 4;
            } 
        }

        private int _blindDrawPlayerCount;
        public int BlindDrawPlayerCount { get { return _blindDrawPlayerCount; } set { _blindDrawPlayerCount = value; OnPropertyChanged(); } }

        DateTime _lastSelectionTime = DateTime.Now;
        private int _lastSelection = -1;

        private int _todoSelection = -1;
        public int TodoSelection { get { return _todoSelection; } 
            set {
                // There is a timing bug. Sometimes, when the unassigned list
                // is updated during a selection index change event, there is a 2nd event
                // that comes in right away with the same value.
                var timeSinceLastEvent = DateTime.Now - _lastSelectionTime;
                if ((_lastSelection == value) &&  (timeSinceLastEvent.TotalMilliseconds < 150))
                {
                    System.Diagnostics.Debug.WriteLine("Duplicate selection event ms: " + timeSinceLastEvent.TotalMilliseconds);

                    // Since the ListBox thinks the same item has been selected, you can't click
                    // on it again. ClearTodoSelection() clears the selected item without
                    // triggering another selection event.
                    ClearTodoSelection();
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

        public TrulyObservableCollection<Player> CancelledPlayers { get; set; }

        private string _teeTimeFile;
        public string TeeTimeFile
        {
            get { return _teeTimeFile; }
            set { _teeTimeFile = value; OnPropertyChanged(); }
        }

        private string _waitingListFile;
        public string WaitingListFile
        {
            get { return _waitingListFile; }
            set { _waitingListFile = value; OnPropertyChanged(); }
        }

        private bool _orderByBlindDraw;
        public bool OrderByBlindDraw { get { return _orderByBlindDraw; } 
            set 
            { 
                _orderByBlindDraw = value; 
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

        private bool _allowTeeTimeIntervalAdjust;
        public bool AllowTeeTimeIntervalAdjust
        {
            get { return _allowTeeTimeIntervalAdjust; }
            set { _allowTeeTimeIntervalAdjust = value; OnPropertyChanged(); }
        }

        private bool _block52TeeTimes;
        public bool Block52TeeTimes
        {
            get { return _block52TeeTimes; }
            set
            {
                _block52TeeTimes = value;
                InitTeeTimes();
                BlindDrawPlayerCount = TournamentTeeTimes.Count * 4;
                Options.Block52TeeTimes = _block52TeeTimes;
                OnPropertyChanged();
            }
        }

        private bool _teeTimeInterval78;
        public bool TeeTimeInterval78
        {
            get { return _teeTimeInterval78; }
            set
            {
                _teeTimeInterval78 = value;
                if (_teeTimeInterval78)
                {
                    _defaultTeeTimes = _defaultTeeTimes78;
                    InitFirstTeeTimeList();
                    InitTeeTimes();
                }
                Options.TeeTimeInterval78 = _teeTimeInterval78;
                OnPropertyChanged();
            }
        }

        private bool _teeTimeInterval10;
        public bool TeeTimeInterval10
        {
            get { return _teeTimeInterval10; }
            set
            {
                _teeTimeInterval10 = value;
                if (_teeTimeInterval10)
                {
                    _defaultTeeTimes = _defaultTeeTimes10;
                    InitFirstTeeTimeList();
                    InitTeeTimes();
                }

                Options.TeeTimeInterval10 = _teeTimeInterval10;
                OnPropertyChanged();
            }
        }

        private bool _teeTimeInterval98;
        public bool TeeTimeInterval98
        {
            get { return _teeTimeInterval98; }
            set
            {
                _teeTimeInterval98 = value;
                if (_teeTimeInterval98)
                {
                    _defaultTeeTimes = _defaultTeeTimes98;
                    InitFirstTeeTimeList();
                    InitTeeTimes();
                }

                Options.TeeTimeInterval98 = _teeTimeInterval98;
                OnPropertyChanged();
            }
        }

        private bool _teeTimesDirty = false;

        private int _currentNumberOfPlayersShowing;

        private Random _randomNumberGenerator;
        #endregion

        #region Commands
        public ICommand GetTournamentsCommand { get { return new ModelCommand(async s => await GetTournaments(s)); } }

        public ICommand LoadSignupsCommand { get { return new ModelCommand(async s => await LoadSignupsFromWeb(s)); } }

        public ICommand LoadTeetimesCommand { get { return new ModelCommand(async s => await LoadTeeTimesFromWeb(s)); } }

        public ICommand UploadTeetimesCommand { get { return new ModelCommand(async s => await UploadToWeb(s)); } }

        public ICommand SaveAsCsvCommand { get { return new ModelCommand(SaveTeeTimesAsCsv); } }

        public ICommand AddPlayerCommand { get { return new ModelCommand(AddPlayer); } }

        public ICommand RemovePlayerCommand { get { return new ModelCommand(RemovePlayer); } }

        public ICommand LoadTeeTimesAndWaitlistCsvCommand { get { return new ModelCommand(LoadTeeTimesAndWaitlistCsv); } }

        public ICommand UploadWaitingListFileCommand { get { return new ModelCommand(async s => await UploadWaitingListFile(s)); } }
        #endregion

        public SignupTabViewModel()
        {
            

            TournamentNames = new TrulyObservableCollection<TournamentName>();
            TeeTimeRequests = new List<TeeTimeRequest>();
            TeeTimeRequestsUnassigned = new TrulyObservableCollection<TeeTimeRequest>();
            TeeTimeRequestsAssigned = new TrulyObservableCollection<TeeTimeRequest>();
            TournamentTeeTimes = new TrulyObservableCollection<TeeTime>();
            // TodoSelection defaults to -1, Setting it here triggers the
            // property setter code which is not needed.
            //TodoSelection = -1;
            RemoveSelection = -1;
            TeeTimeInterval10 = Options.TeeTimeInterval10; // initializes list
            TeeTimeInterval78 = Options.TeeTimeInterval78;
            TeeTimeInterval98 = Options.TeeTimeInterval98;
            Block52TeeTimes = Options.Block52TeeTimes;
            GetTournamentsVisible = Visibility.Visible;
            GotTournamentsVisible = Visibility.Collapsed;
            AllowTeeTimeIntervalAdjust = true;
            _randomNumberGenerator = new Random();
            CancelledPlayers = new TrulyObservableCollection<Player>();
        }

        public void InitTeeTimes()
        {
            ClearPlayersFromAllTeeTimes();
            TournamentTeeTimes.Clear();

            // TODO shotgun
            int firstTime = (FirstTeeTimeIndex < 0) ? 0 : FirstTeeTimeIndex;
            for (int i = firstTime; i < _defaultTeeTimes.Count; i++)
            {
                if (Block52TeeTimes && _defaultTeeTimes[i].Contains(":52"))
                {
                    // skip
                }
                else
                {
                    TournamentTeeTimes.Add(new TeeTime { StartTime = _defaultTeeTimes[i] });
                }
            }
        }

        public void InitFirstTeeTimeList()
        {
            var teeTimes = new List<string>();
            // Only allow the start time to be shifted by 2hrs (the first 16
            // tee times)
            for (int i = 0; (i < 16) && (i < _defaultTeeTimes.Count); i++)
            {
                teeTimes.Add(_defaultTeeTimes[i]);
            }

            TeeTimes = teeTimes;
            FirstTeeTimeIndex = 0;
        }

        private void ClearTodoSelection()
        {
            // Clear the unassigned list selection without triggering
            // a new event by assigning an empty list and the restoring
            // the original list.
            var savedList = TeeTimeRequestsUnassigned;
            TeeTimeRequestsUnassigned = new TrulyObservableCollection<TeeTimeRequest>();
            TeeTimeRequestsUnassigned = savedList;
        }

        public void SortTeeTimeRequests()
        {
            if (TeeTimeRequests != null)
            {
                // First, reset the unassigned list to an empty list to make
                // sure nothing is "selected". If so, changing the list will
                // trigger a selected event.
                ClearTodoSelection();

                if (OrderByBlindDraw)
                {
                    TeeTimeRequests.Sort(new BlindDrawKeySort());
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

            bool found = false;
            for (int i = 0; i < TournamentNames.Count; i++ )
            {
                if ((DateTime.Now > TournamentNames[i].SignupStartDate) && (DateTime.Now <= TournamentNames[i].EndDate)
                    && !TournamentNames[i].AnnouncementOnly)
                {
                    found = true;
                    TournamentNameIndex = i;
                    break;
                }
            }

            // If nothing found between signup start and tournament end, then find
            // the last tournament that finished.
            if (!found)
            {
                for (int i = TournamentNames.Count - 1; i >= 0 ; i--)
                {
                    if ((DateTime.Now >= TournamentNames[i].EndDate) && !TournamentNames[i].AnnouncementOnly)
                    {
                        TournamentNameIndex = i;
                        break;
                    }
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

        private TrulyObservableCollection<TeeTime> ConvertUnassignedToTeeTimes()
        {
            TrulyObservableCollection<TeeTime> teeTimeList = new TrulyObservableCollection<TeeTime>();

            for (int i = 0; i < TeeTimeRequests.Count; i++)
            {
                TeeTime teeTime = new TeeTime();
                // Set the start time to be 12:00, 12:01, 12:02, etc. just to show groups.
                // The actual time doesn't matter.
                teeTime.StartTime = ((i < 60) ? "12:" : "01:") +  (i % 60).ToString("D2") + " PM";

                foreach (var player in TeeTimeRequests[i].Players)
                {
                    teeTime.AddPlayer(player);
                }

                teeTimeList.Add(teeTime);
            }

            return teeTimeList;
        }

        private TrulyObservableCollection<TeeTime> ConvertCancelledPlayersToTeeTimes()
        {
            TrulyObservableCollection<TeeTime> teeTimeList = new TrulyObservableCollection<TeeTime>();

            for (int i = 0; i < CancelledPlayers.Count; i++)
            {
                TeeTime teeTime = new TeeTime();
                // Actual tee times are 06:00 to 11:52. Waitlist are 12:00 to 01:52.
                // Put all the cancelled at 03:00 and 04:00.
                // The actual time doesn't matter.
                teeTime.StartTime = ((i < 60) ? "03:" : "04:") + (i % 60).ToString("D2") + " PM";

                teeTime.AddPlayer(CancelledPlayers[i]);

                teeTimeList.Add(teeTime);
            }

            return teeTimeList;
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
                MessageBox.Show((teeTime.Players.Count == 4) 
                    ? "Tee time is full" 
                    : "Not enough room for all players at " + teeTime.StartTime);
                ClearTodoSelection();
                return;
            }

            // Add the players to the tee time
            _teeTimesDirty = true;
            foreach (var player in teeTimeRequest.Players)
            {
                teeTime.AddPlayer(player);
                player.TeeTime = teeTime;
            }
            teeTimeRequest.TeeTime = teeTime;

            AllowTeeTimeIntervalAdjust = false;

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

                // If the unassigned list is empty, there may be more unassigned
                // players but they don't fit into this group. Move to the next tee
                // time, which would allow a group of 4 players, so anyone left on
                // the unassigned list displays.
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
                player.TeeTime = null;
            }
            teeTimeRequest.TeeTime = null;

            TeeTimeRequests.Insert(0, teeTimeRequest);
            SortTeeTimeRequests();
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

                        values.Add(new KeyValuePair<string, string>(
                            string.Format("TeeTime[{0}][Extra][{1}]", i, player),
                            TournamentTeeTimes[i].Players[player].Extra));

                        values.Add(new KeyValuePair<string, string>(
                            string.Format("TeeTime[{0}][SignupKey][{1}]", i, player),
                            TournamentTeeTimes[i].Players[player].SignupKey.ToString()));
                    }
                }

                // Upload the waiting list
                if (TeeTimeRequests.Count > 0)
                {
                    // save setting
                    var orderBy = OrderByBlindDraw;

                    // order by blind draw to upload to web
                    OrderByBlindDraw = true;

                    try
                    {
                        int position = 0;
                        for (int i = 0; i < TeeTimeRequests.Count; i++)
                        {
                            foreach (var player in TeeTimeRequests[i].Players)
                            {
                                values.Add(new KeyValuePair<string, string>(
                                    string.Format("SignUpsWaitingList[{0}][Position]", position),
                                    position.ToString(CultureInfo.InvariantCulture)));

                                values.Add(new KeyValuePair<string, string>(
                                    string.Format("SignUpsWaitingList[{0}][GHIN1]", position),
                                    player.GHIN.ToString(CultureInfo.InvariantCulture)));

                                values.Add(new KeyValuePair<string, string>(
                                    string.Format("SignUpsWaitingList[{0}][Name1]", position),
                                    player.Name));

                                position++;
                            }
                        }
                    }
                    finally
                    {
                        // Restore order-by choice
                        OrderByBlindDraw = orderBy;
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

        private void SaveTeeTimesAsCsv(object o)
        {
            int teamId = 0;
            string teeTimesFileName = "Teetimes - " + TournamentNames[TournamentNameIndex].Name;

            bool savedFile = SaveAsCsv(
                TournamentTeeTimes, 
                TournamentNames[TournamentNameIndex].TeamSize,
                out bool _teeTimesDirty,
                ref teeTimesFileName,
                ref teamId,
                false,
                TeeTimeStatus.TeeTime);

            if (savedFile)
            {
                TeeTimeFile = teeTimesFileName;
            }

            // If the tee times were saved and there are still 
            // tee time requests, save the remaining requests
            // as waitlisted players.
            if (savedFile && (TeeTimeRequests.Count > 0))
            {
                // save setting
                var orderBy = OrderByBlindDraw;

                // order by blind draw to save in file
                OrderByBlindDraw = true;

                var unassignedTeeTimes = ConvertUnassignedToTeeTimes();

                try
                {
                    SaveAsCsv(
                        unassignedTeeTimes,
                        TournamentNames[TournamentNameIndex].TeamSize,
                        out bool ignore,
                        ref teeTimesFileName,
                        ref teamId,
                        true,
                        TeeTimeStatus.Waitlisted);
                }
                finally
                {
                    // Restore order-by choice
                    OrderByBlindDraw = orderBy;
                }
            }

            if (savedFile && (CancelledPlayers.Count > 0))
            {
                var cancelledPlayers = ConvertCancelledPlayersToTeeTimes();

                SaveAsCsv(
                        cancelledPlayers,
                        TournamentNames[TournamentNameIndex].TeamSize,
                        out bool ignore,
                        ref teeTimesFileName,
                        ref teamId,
                        true,
                        TeeTimeStatus.Cancelled);
            }
        }

        // Make this routine static to make sure it is not using anything that is not passed in
        private static bool SaveAsCsv(TrulyObservableCollection<TeeTime> tournamentTeeTimes, int teamSize,
            out bool teeTimesDirty, ref string finalFileName, ref int teamId, bool appendToFile, TeeTimeStatus teeTimeStatus)
        {
            teeTimesDirty = true;

            if (!appendToFile)
            {
                // Configure save file dialog box
                Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
                dlg.FileName = finalFileName;
                dlg.DefaultExt = ".csv"; // Default file extension
                dlg.Filter = "CSV File (.csv)|*.csv"; // Filter files by extension

                // Show save file dialog box
                bool? result = dlg.ShowDialog();

                if (result != true)
                {
                    return false;
                }

                finalFileName = dlg.FileName;
            }

            teeTimesDirty = false;
                
            using (TextWriter tw = new StreamWriter(finalFileName, appendToFile))
            {
                if (!appendToFile)
                {
                    tw.WriteLine("Tee Time,Status,Team Id,Last Name,First Name,GHIN,Flight,Email");
                }

                for (int teeTimeNumber = 0; teeTimeNumber < tournamentTeeTimes.Count; teeTimeNumber++)
                {
                    for (int player = 0; player < 4; player++)
                    {
                        // TODO: handle shotgun
                        string playerLastName = string.Empty;
                        string playerFirstName = string.Empty;
                        string playerGhin = string.Empty;
                        string playerEmail = string.Empty;
                        string playerExtra = string.Empty;

                        if (player < tournamentTeeTimes[teeTimeNumber].Players.Count)
                        {
                            // GG requires last name and first name separately
                            string[] fields = tournamentTeeTimes[teeTimeNumber].Players[player].Name.Split(',');
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
                            playerGhin = tournamentTeeTimes[teeTimeNumber].Players[player].GHIN;
                            playerExtra = tournamentTeeTimes[teeTimeNumber].Players[player].Extra;
                            playerEmail = tournamentTeeTimes[teeTimeNumber].Players[player].Email;
                            // Make sure the email address is valid
                            if (string.IsNullOrEmpty(playerEmail) || !playerEmail.Contains("@"))
                            {
                                playerEmail = string.Empty;
                            }

                        }

                        // Only write out tee time entries if there is a player
                        if (!string.IsNullOrEmpty(playerLastName))
                        {
                            if ((teamSize == 4) && (player == 0))
                            {
                                // Update team ID on the first player of the group
                                teamId++;
                            }
                            else if ((teamSize == 2) && (player % 2 == 0))
                            {
                                // Update team ID on the first player and third player of group
                                teamId++;
                            }
                            else if (teamSize == 1)
                            {
                                // For individual tournaments, update the team number per player
                                teamId++;
                            }

                            tw.Write(tournamentTeeTimes[teeTimeNumber].StartTime + ",");
                            tw.Write(teeTimeStatus.ToString() + ",");
                            tw.Write(teamId + ",");
                            tw.Write(playerLastName + ",");
                            tw.Write(playerFirstName + ",");
                            tw.Write(playerGhin + ",");

                            if (!string.IsNullOrEmpty(playerExtra))
                            {
                                // Write the flight number
                                if (playerExtra.Contains("CH"))
                                {
                                    tw.Write("0");
                                }
                                else if (playerExtra.Contains("F1"))
                                {
                                    tw.Write("1");
                                }
                                else if (playerExtra.Contains("F2"))
                                {
                                    tw.Write("2");
                                }
                                else if (playerExtra.Contains("F3"))
                                {
                                    tw.Write("3");
                                }
                                else if (playerExtra.Contains("F4"))
                                {
                                    tw.Write("4");
                                }
                                else if (playerExtra.Contains("F5"))
                                {
                                    tw.Write("5");
                                }
                            }

                            tw.Write("," + playerEmail + ",");

                            tw.WriteLine();                             
                        }
                    }
                }
            }

            return true;
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

            // cancelled password input
            if (string.IsNullOrEmpty(Credentials.LoginPassword))
            {
                return;
            }

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(WebAddresses.BaseAddress);

                using (new WaitCursor())
                {
                    var values = new List<KeyValuePair<string, string>>();

                    values.Add(new KeyValuePair<string, string>("Login", Credentials.LoginName));
                    values.Add(new KeyValuePair<string, string>("Password", Credentials.LoginPassword));

                    values.Add(new KeyValuePair<string, string>("tournament",
                        TournamentNames[TournamentNameIndex].TournamentKey.ToString(CultureInfo.InvariantCulture)));

                    var content = new FormUrlEncodedContent(values);

                    var response = await client.PostAsync(WebAddresses.ScriptFolder + WebAddresses.GetSignups, content);
                    var responseString = await response.Content.ReadAsStringAsync();

                    Logging.Log("LoadSignupsFromWeb", responseString);

                    if (!IsValidJson(responseString))
                    {
                        Credentials.CheckForInvalidPassword(responseString);

                        HtmlDisplayWindow displayWindow = new HtmlDisplayWindow();
                        displayWindow.WebBrowser.NavigateToString(responseString);
                        displayWindow.Owner = Application.Current.MainWindow;
                        displayWindow.ShowDialog();
                        return;
                    }

                    TeeTimeRequests.Clear();
                    TeeTimeRequestsUnassigned.Clear();
                    TeeTimeRequestsAssigned.Clear();
                    CancelledPlayers.Clear();
                    AllowTeeTimeIntervalAdjust = true;

                    if (string.IsNullOrEmpty(responseString))
                    {
                        MessageBox.Show("Website gave empty response for tee time request list");
                    }
                    else
                    {
                        TeeTimeRequests = LoadSignupsFromWebResponseJson(responseString);

                        BlindDraw();

                        if (OrderByBlindDraw)
                        {
                            TeeTimeRequests.Sort(new BlindDrawKeySort());
                        }
                        else
                        {
                            TeeTimeRequests.Sort(new TeeTimeRequestSort());
                        }
                    }
                    
                    UpdateUnassignedList(4);

                    InitTeeTimes();
                }
            }

            GroupMode = true;
        }

        private void BlindDraw()
        {
            foreach (var request in TeeTimeRequests)
            {
                if (request.Paid)
                {
                    // Selecting lower numbers ensures those that have paid will
                    // be higher in blind draw list
                    request.BlindDrawValue = _randomNumberGenerator.Next(1000, 2999);
                }
                else
                {
                    request.BlindDrawValue = _randomNumberGenerator.Next(3000, 9999);
                }
            }

            TeeTimeRequests.Sort(new BlindDrawKeySort());

            int playerCount = 0;
            for (int i = 0; i < TeeTimeRequests.Count; i++)
            {
                TeeTimeRequests[i].Waitlisted = playerCount >= BlindDrawPlayerCount;
                playerCount += TeeTimeRequests[i].Players.Count;
            }
        }

        private class TeeTimeRequestSort : IComparer<TeeTimeRequest>
        {
            public int Compare(TeeTimeRequest a, TeeTimeRequest b)
            {
                // Put all the waitlisted groups at the end
                if (a.Waitlisted && !b.Waitlisted) return 1;
                if (!a.Waitlisted && b.Waitlisted) return -1;
                // Sort the waitlisted groups by the blind draw value
                if(a.Waitlisted && b.Waitlisted) return a.BlindDrawValue - b.BlindDrawValue;

                // Sort the non-waitlisted groups by requested time
                return a.GetHour() - b.GetHour();
            }
        }

        private class BlindDrawKeySort : IComparer<TeeTimeRequest>
        {
            public int Compare(TeeTimeRequest a, TeeTimeRequest b)
            {
                return a.BlindDrawValue - b.BlindDrawValue;
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

            // cancelled password input
            if (string.IsNullOrEmpty(Credentials.LoginPassword))
            {
                return;
            }

            _teeTimesDirty = false;
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(WebAddresses.BaseAddress);

                using (new WaitCursor())
                {
                    var values = new List<KeyValuePair<string, string>>();

                    values.Add(new KeyValuePair<string, string>("Login", Credentials.LoginName));
                    values.Add(new KeyValuePair<string, string>("Password", Credentials.LoginPassword));

                    values.Add(new KeyValuePair<string, string>("tournament",
                        TournamentNames[TournamentNameIndex].TournamentKey.ToString(CultureInfo.InvariantCulture)));

                    var content = new FormUrlEncodedContent(values);

                    var teeTimesResponse = await client.PostAsync(WebAddresses.ScriptFolder + WebAddresses.GetTeeTimes, content);
                    var responseString = await teeTimesResponse.Content.ReadAsStringAsync();

                    Logging.Log(WebAddresses.ScriptFolder + WebAddresses.GetTeeTimes, responseString);

                    if (!IsValidJson(responseString))
                    {
                        Credentials.CheckForInvalidPassword(responseString);
                        
                        HtmlDisplayWindow displayWindow = new HtmlDisplayWindow();
                        displayWindow.WebBrowser.NavigateToString(responseString);
                        displayWindow.Owner = Application.Current.MainWindow;
                        displayWindow.ShowDialog();
                        return;
                    }

                    // Have to create a new content variable, or you get a cannot dispose exception
                    var content2 = new FormUrlEncodedContent(values);

                    var signupsWaitlingListResponse = await client.PostAsync(WebAddresses.ScriptFolder + WebAddresses.GetSignUpsWaitingList, content2);
                    var responseString2 = await signupsWaitlingListResponse.Content.ReadAsStringAsync();
                    Logging.Log(WebAddresses.ScriptFolder + WebAddresses.GetSignUpsWaitingList, responseString2);

                    if (!IsValidJson(responseString2))
                    {
                        Credentials.CheckForInvalidPassword(responseString2);

                        HtmlDisplayWindow displayWindow = new HtmlDisplayWindow();
                        displayWindow.WebBrowser.NavigateToString(responseString2);
                        displayWindow.Owner = Application.Current.MainWindow;
                        displayWindow.ShowDialog();
                        return;
                    }

                    TeeTimeRequests.Clear();
                    TeeTimeRequestsUnassigned.Clear();
                    TeeTimeRequestsAssigned.Clear();
                    TournamentTeeTimes.Clear();
                    CancelledPlayers.Clear();

                    AllowTeeTimeIntervalAdjust = false;

                    LoadTeeTimesFromWebResponseJson(responseString);
                    FillInAssignedListFromTournamentTeeTimes();

                    LoadWaitingListFromWebResponseJson(responseString2);
                    
                    SelectOpenTeeTime();
                }
            }

            GroupMode = false;
        }

        protected void LoadTeeTimesFromWebResponseJson(string webResponse)
        {
            if (string.IsNullOrEmpty(webResponse))
            {
                return;
            }

            if (webResponse.StartsWith("JSON error:"))
            {
                throw new Exception(webResponse);
            }

            var jss = new JavaScriptSerializer();
            var tournamentTeeTimes = jss.Deserialize<TrulyObservableCollection<TeeTime>>(webResponse);

            // Initialize the first tee time combo box and the block :52 checkbox based
            // on the values in the tee time list.
            int lowestTeeTimeIndex = -1;
            for (int teeTimeIndex = 0; teeTimeIndex < tournamentTeeTimes.Count; teeTimeIndex++)
            {

                // If the tee time list contains :52, then make sure that
                // time is available
                if (Block52TeeTimes && tournamentTeeTimes[teeTimeIndex].StartTime.Contains(":52"))
                {
                    Block52TeeTimes = false;
                }
                for (int i = 0; i < _defaultTeeTimes.Count; i++)
                {
                    if (String.CompareOrdinal(_defaultTeeTimes[i], tournamentTeeTimes[teeTimeIndex].StartTime) == 0)
                    {
                        if ((lowestTeeTimeIndex == -1) || (i < lowestTeeTimeIndex))
                        {
                            lowestTeeTimeIndex = i;
                        }
                        break;
                    }
                }
            }

            if (lowestTeeTimeIndex != -1)
            {
                FirstTeeTimeIndex = lowestTeeTimeIndex;
            }

            TournamentTeeTimes = tournamentTeeTimes;

            // Need to tie player to the tee time
            foreach (var teeTime in TournamentTeeTimes)
            {
                foreach (var player in teeTime.Players)
                {
                    player.TeeTime = teeTime;
                }
            }
        }

        protected void LoadWaitingListFromWebResponseJson(string webResponse)
        {
            if (string.IsNullOrEmpty(webResponse))
            {
                return;
            }

            if (webResponse.StartsWith("JSON error:"))
            {
                throw new Exception(webResponse);
            }

            var jss = new JavaScriptSerializer();
            var players = jss.Deserialize<TrulyObservableCollection<Player>>(webResponse);

            foreach (var player in players)
            {
                var teeTimeRequest = new TeeTimeRequest();
                teeTimeRequest.Players.Add(player);
                teeTimeRequest.Waitlisted = true;
                teeTimeRequest.Preference = "None";
                teeTimeRequest.BlindDrawValue = player.Position;  // the order in the file is the blind draw order
                TeeTimeRequests.Add(teeTimeRequest);
            }
        }

        public void ClearPlayersFromAllTeeTimes()
        {
            foreach (var teeTime in TournamentTeeTimes)
            {
                teeTime.ClearPlayers();
            }
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

                foreach (var teeTime in TournamentTeeTimes)
                {
                    foreach (var p in teeTime.Players)
                    {
                        if (!string.IsNullOrEmpty(player.GHIN) && (String.CompareOrdinal(player.GHIN, p.GHIN) == 0))
                        {
                            MessageBox.Show(Application.Current.MainWindow,
                                player.Name + " is already in the signup list at tee time preference " + teeTime.StartTime);
                            return;
                        }
                    }
                }

                TeeTimeRequestsUnassigned.Insert(0, ttr);
            }
        }

        private void RemovePlayer(object o)
        {
            List<Player> playerList = new List<Player>();
            foreach (var request in TeeTimeRequests)
            {
                foreach (var p in request.Players)
                {
                    playerList.Add(p);
                }
            }
            foreach (var teeTime in TournamentTeeTimes)
            {
                foreach (var p in teeTime.Players)
                {
                    playerList.Add(p);
                }
            }

            RemovePlayerWindow rpw = new RemovePlayerWindow();
            //rpw.DataContext = player;
            rpw.PlayerList = playerList;
            rpw.Owner = Application.Current.MainWindow;

            rpw.ShowDialog();
            if (rpw.DialogResult.HasValue && rpw.DialogResult.Value)
            {
                if (rpw.Player != null)
                {
                    MessageBoxResult result = MessageBox.Show("Are you sure you want to remove " + rpw.Player.Name + "?",
                                          "Confirmation",
                                          MessageBoxButton.YesNo,
                                          MessageBoxImage.Question);
                    if (result != MessageBoxResult.Yes)
                    {
                        return;
                    }

                    TeeTimeRequest ttr = null;
                    foreach (var request in TeeTimeRequests)
                    {
                        foreach (var p in request.Players)
                        {
                            if (rpw.Player == p)
                            {
                                ttr = request;
                            }
                        }
                    }

                    if (ttr != null)
                    {
                        ttr.Players.Remove(rpw.Player);
                        if (ttr.Players.Count == 0)
                        {
                            TeeTimeRequests.Remove(ttr);
                        }
                        UpdateUnassignedList(_currentNumberOfPlayersShowing);
                        CancelledPlayers.Add(rpw.Player);
                        return;
                    }

                    int teeTimeIndex = -1;
                    for(int i = 0; (i < TournamentTeeTimes.Count) && (teeTimeIndex == -1); i++)
                    {
                        foreach (var p in TournamentTeeTimes[i].Players)
                        {
                            if (rpw.Player == p)
                            {
                                teeTimeIndex = i;
                            }
                        }
                    }

                    if (teeTimeIndex != -1)
                    {
                        // Remove the player from the tee time itself
                        TournamentTeeTimes[teeTimeIndex].RemovePlayer(rpw.Player);
                        TeeTimeSelection = teeTimeIndex;

                        // Tee time requests are move to the assigned list when 
                        // the request has been given a tee time. Remove the player
                        // from the request on that list.
                        ttr = null;
                        foreach (var request in TeeTimeRequestsAssigned)
                        {
                            foreach (var p in request.Players)
                            {
                                if (rpw.Player == p)
                                {
                                    ttr = request;
                                }
                            }
                        }

                        if (ttr != null)
                        {
                            ttr.Players.Remove(rpw.Player);
                            if (ttr.Players.Count == 0)
                            {
                                TeeTimeRequestsAssigned.Remove(ttr);
                            }
                            CancelledPlayers.Add(rpw.Player);
                        }
                    }
                }
            }
        }

        //
        // Expected format of tee sheet file: header followed by individual lines.
        // 
        // Tee Time	Last Name	First Name	GHIN	Team Id	Email	Flight	OverEighty	Waitlisted
        // 6:15 AM,	Albitz,Paul,9079663,1,palbitz@san.rr.com,FALSE,FALSE


        private void LoadTeeTimesAndWaitlistCsv(object o)
        {
            if(string.IsNullOrEmpty(TeeTimeFile))
            {
                MessageBox.Show("Please fill in the tee sheet file");
                return;
            }

            if (!File.Exists(TeeTimeFile))
            {
                MessageBox.Show("File does not exist: " + TeeTimeFile);
                return;
            }

            AllowTeeTimeIntervalAdjust = false;

            using (TextReader tr = new StreamReader(TeeTimeFile))
            {
                ClearPlayersFromAllTeeTimes();
                //TournamentTeeTimes.Clear();

                TeeTimeRequests.Clear();
                TeeTimeRequestsUnassigned.Clear();
                TeeTimeRequestsAssigned.Clear();
                CancelledPlayers.Clear();

                string[][] lines = CSVParser.Parse(tr);

                int teeTimeColumn = -1;
                int lastNameColumn = -1;
                int firstNameColumn = -1;
                int ghinColumn = -1;
                int emailColumn = -1;
                int flightColumn = -1;
                int statusColumn = -1;

                if (lines.Length == 0)
                {
                    throw new ApplicationException(TeeTimeFile + ": has 0 lines");
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
                    else if (string.Compare(lines[0][col], "email", true) == 0)
                    {
                        emailColumn = col;
                    }
                    else if (string.Compare(lines[0][col], "flight", true) == 0)
                    {
                        flightColumn = col;
                    }
                    else if (string.Compare(lines[0][col], "status", true) == 0)
                    {
                        statusColumn = col;
                    }
                }

                if (teeTimeColumn == -1)
                {
                    throw new ApplicationException(TeeTimeFile + ": did not find header column: Tee Time");
                }
                if (lastNameColumn == -1)
                {
                    throw new ApplicationException(TeeTimeFile + ": did not find header column: Last Name");
                }
                if (firstNameColumn == -1)
                {
                    throw new ApplicationException(TeeTimeFile + ": did not find header column: First Name");
                }
                if (ghinColumn == -1)
                {
                    throw new ApplicationException(TeeTimeFile + ": did not find header column: GHIN");
                }
                if (emailColumn == -1)
                {
                    throw new ApplicationException(TeeTimeFile + ": did not find header column: Email");
                }
                if (flightColumn == -1)
                {
                    throw new ApplicationException(TeeTimeFile + ": did not find header column: Flight");
                }
                if (statusColumn == -1)
                {
                    throw new ApplicationException(TeeTimeFile + ": did not find header column: Status");
                }

                // Initialize the first tee time combo box and the block :52 checkbox based
                // on the values in the tee time list.
                int lowestTeeTimeIndex = -1;
                for (int lineIndex = 1; lineIndex < lines.Length; lineIndex++)
                {
                    string[] line = lines[lineIndex];
                    if (line.Length > 0)
                    {
                        if (!string.IsNullOrEmpty(line[teeTimeColumn]))
                        {
                            // If the tee time list contains :52, then make sure that
                            // time is available
                            if (Block52TeeTimes && line[teeTimeColumn].Contains(":52"))
                            {
                                Block52TeeTimes = false;
                            }
                                for (int i = 0; i < _defaultTeeTimes.Count; i++)
                            {
                                if (String.CompareOrdinal(_defaultTeeTimes[i], line[teeTimeColumn]) == 0)
                                {
                                    if ((lowestTeeTimeIndex == -1) || (i < lowestTeeTimeIndex))
                                    {
                                        lowestTeeTimeIndex = i;
                                    }
                                    break;
                                }
                            }
                        }
                    }
                }

                if (lowestTeeTimeIndex != -1)
                {
                    FirstTeeTimeIndex = lowestTeeTimeIndex;
                }

                for (int lineIndex = 1, playerIndex = 0; lineIndex < lines.Length; lineIndex++, playerIndex++)
                {
                    TeeTimeStatus teeTimeStatus = TeeTimeStatus.TeeTime;
                    string[] line = lines[lineIndex];
                    if (line.Length > 0)
                    {
                        if (line[statusColumn].ToLower().Contains("wait"))
                        {
                            teeTimeStatus = TeeTimeStatus.Waitlisted;
                        }
                        else if (line[statusColumn].ToLower().Contains("cancel"))
                        {
                            teeTimeStatus = TeeTimeStatus.Cancelled;
                        }
                        else if (!line[statusColumn].ToLower().Contains("teetime"))
                        {
                            throw new ApplicationException(TeeTimeFile + " (line " + lineIndex + "): Status must be: TeeTime, Waitlisted, or Cancelled ");
                        }

                        if (string.IsNullOrEmpty(line[teeTimeColumn]))
                        {
                            throw new ApplicationException(TeeTimeFile + " (line " + lineIndex + "): Tee Time is empty");
                        }
                        if (string.IsNullOrEmpty(line[lastNameColumn]))
                        {
                            // Skip over any waitlisted lines that might have been cut and pasted into the tee time list
                            if (teeTimeStatus == TeeTimeStatus.Waitlisted)
                            {
                                continue;
                            }
                            throw new ApplicationException(TeeTimeFile + " (line " + lineIndex + "): Last Name is empty");
                        }

                        Player player = new Player();
                        if (!string.IsNullOrEmpty(line[firstNameColumn]))
                        {
                            player.Name = line[lastNameColumn].Trim() + ", " + line[firstNameColumn].Trim();
                        }
                        else
                        {
                            player.Name = line[lastNameColumn].Trim();
                        }
                        player.GHIN = line[ghinColumn].Trim();

                        if (emailColumn != -1)
                        {
                            player.Email = line[emailColumn].Trim();
                        }
                        if (flightColumn != -1)
                        {
                            int flight;
                            if (int.TryParse(line[flightColumn], out flight))
                            {
                                // flight 0 is championship flight
                                if (flight == 0)
                                {
                                    player.Extra = "CH";
                                }
                                else
                                {
                                    player.Extra = "F" + flight;
                                }
                            }
                        }

                        if (teeTimeStatus == TeeTimeStatus.TeeTime)
                        {
                            TeeTime tt = null;
                            foreach (var teeTime in TournamentTeeTimes)
                            {
                                if (String.CompareOrdinal(teeTime.StartTime, line[teeTimeColumn]) == 0)
                                {
                                    tt = teeTime;
                                    break;
                                }
                            }

                            if (tt == null)
                            {
                                throw new ApplicationException("Unable to find tee time " + line[teeTimeColumn] + " in the tee time list. Please select the correct set of tee times before loading this file");
                            }

                            player.Position = tt.Players.Count + 1;
                            player.TeeTime = tt;
                            tt.AddPlayer(player);
                        }
                        else if (teeTimeStatus == TeeTimeStatus.Waitlisted)
                        {
                            var teeTimeRequest = new TeeTimeRequest();
                            teeTimeRequest.Players.Add(player);
                            teeTimeRequest.Waitlisted = true;
                            teeTimeRequest.Preference = "None";
                            teeTimeRequest.BlindDrawValue = lineIndex;  // the order in the file is the blind draw order
                            TeeTimeRequests.Add(teeTimeRequest);
                        }
                        else if (teeTimeStatus == TeeTimeStatus.Cancelled)
                        {
                            CancelledPlayers.Add(player);
                        }
                    }
                }
            }
            SelectOpenTeeTime();
            FillInAssignedListFromTournamentTeeTimes();
            GroupMode = false;
        }

        private void FillInAssignedListFromTournamentTeeTimes()
        {
            TeeTimeRequestsAssigned.Clear();

            if (TournamentTeeTimes != null)
            {
                foreach (var teeTime in TournamentTeeTimes)
                {
                    foreach (var player in teeTime.Players)
                    {
                        TeeTimeRequest teeTimeRequest = new TeeTimeRequest();
                        teeTimeRequest.Preference = "None";
                        teeTimeRequest.Players.Add(player);
                        teeTimeRequest.TeeTime = player.TeeTime;
                        TeeTimeRequestsAssigned.Add(teeTimeRequest);
                    }
                }
            }
        }

        private void SelectOpenTeeTime()
        {
            // Reset the current tee time selection to be
            // the first tee time with openings.
            bool openTeeTimeFound = false;
            for (int i = 0; i < TournamentTeeTimes.Count; i++)
            {
                if (TournamentTeeTimes[i].Players.Count < 4)
                {
                    // This also updates the unassigned list
                    TeeTimeSelection = i;
                    openTeeTimeFound = true;
                    break;
                }
            }
            if (!openTeeTimeFound)
            {
                // Show the complete unassigned list
                UpdateUnassignedList(4);
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

                if (lines.Length > 0)
                {
                    if ((lines[0][0].ToLower() != "tee time") || (lines[0][1].ToLower() != "last name") 
                        || (lines[0][2].ToLower() != "first name") || (lines[0][3].ToLower() != "ghin"))
                    {
                        MessageBox.Show("Expected 1st 3 columns on line 1 to be: TeeTime,Last Name,First Name,GHIN", "Format Error");
                        return;
                    }

                    for (int lineIndex = 1; lineIndex < lines.Length; lineIndex++)
                    {
                        string[] line = lines[lineIndex];
                        if ((line.Length > 2) && (!string.IsNullOrWhiteSpace(line[1])))
                        {
                            var waitingListEntry = new SignUpWaitingListEntry();
                            waitingList.Add(waitingListEntry);

                            waitingListEntry.Position = waitingList.Count;

                            // columns are TeeTime,Last Name,First Name 
                            string name = line[1].Trim() + ", " + line[2].Trim();
                            waitingListEntry.Name1 = name;
                            int ghin = 0;
                            if (int.TryParse(line[3].Trim(), out ghin))
                            {
                                waitingListEntry.GHIN1 = ghin;
                            }
                        }
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

        private static bool IsValidJson(string strInput)
        {
            if (string.IsNullOrWhiteSpace(strInput)) { return false; }

            strInput = strInput.Trim();
            if ((strInput.StartsWith("{") && strInput.EndsWith("}")) || //For object
                (strInput.StartsWith("[") && strInput.EndsWith("]"))) //For array
            {
                return true;
                /*
                try
                {
                    var obj = JToken.Parse(strInput);
                    return true;
                }
                catch (JsonReaderException jex)
                {
                    //Exception in parsing json
                    Console.WriteLine(jex.Message);
                    return false;
                }
                catch (Exception ex) //some other exception
                {
                    Console.WriteLine(ex.ToString());
                    return false;
                }
                */
            }
            else
            {
                return false;
            }
        }
    }
}
