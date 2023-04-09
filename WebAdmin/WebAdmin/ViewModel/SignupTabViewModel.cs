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
using System.Collections;
using System.Windows.Threading;

namespace WebAdmin.ViewModel
{
    public class SignupTabViewModel : TabViewModelBase
    {
        private enum TeeTimeStatus { TeeTime, Waitlisted, Cancelled };
        public enum OrderTeeTimeRequestsByEnum { RequestedTime, LastTeeTime, BlindDraw, HistoricalTeeTimes };

        #region Properties
        public override string Header { get { return "Signup"; } }

        private List<string> _defaultTeeTimes;

        private readonly List<string> _defaultTeeTimes78 = new List<string>
        {
            "6:00 AM", "6:07 AM", "6:15 AM", "6:22 AM", "6:30 AM", "6:37 AM", "6:45 AM", "6:52 AM",
            "7:00 AM", "7:07 AM", "7:15 AM", "7:22 AM", "7:30 AM", "7:37 AM", "7:45 AM", "7:52 AM",
            "8:00 AM", "8:07 AM", "8:15 AM", "8:22 AM", "8:30 AM", "8:37 AM", "8:45 AM", "8:52 AM",
            "9:00 AM", "9:07 AM", "9:15 AM", "9:22 AM", "9:30 AM", "9:37 AM", "9:45 AM", "9:52 AM",
            "10:00 AM", "10:07 AM", "10:15 AM", "10:22 AM", "10:30 AM", "10:37 AM", "10:45 AM", "10:52 AM",
            "11:00 AM", "11:07 AM", "11:15 AM", "11:22 AM", "11:30 AM", "11:37 AM", "11:45 AM", "11:52 AM", "12:00 PM"
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

        private string _teeTimeSource;
        public string TeeTimeSource { get { return _teeTimeSource; } set { _teeTimeSource = value; OnPropertyChanged(); } }

        private List<string> _teeTimes;
        public List<string> TeeTimes { get { return _teeTimes; } set { _teeTimes = value; OnPropertyChanged(); } }

        private int _firstTeeTimeIndex;
        public int FirstTeeTimeIndex { get { return _firstTeeTimeIndex; } 
            set 
            { 
                _firstTeeTimeIndex = value; 
                OnPropertyChanged();
                UpdateTournamentTeeTimes();
                UpdateBlindDrawPlayerCount();
            } 
        }

        private int _blindDrawPlayerCount;
        public int BlindDrawPlayerCount { get { return _blindDrawPlayerCount; } set { _blindDrawPlayerCount = value; OnPropertyChanged(); } }

        private int _monthsOfTeeTimeDataToLoad;
        public int MonthsOfTeeTimeDataToLoad
        {
            get { return _monthsOfTeeTimeDataToLoad; }
            set {
                _monthsOfTeeTimeDataToLoad = value;
                Options.MonthsOfTeeTimeDataToLoad = value;
                OnPropertyChanged();
            }
        }

        DateTime _lastSelectionTime = DateTime.Now;
        private int _lastSelection = -1;

        private int _todoSelection = -1;
        public int TodoSelection { get { return _todoSelection; } 
            set {
                // There is a timing bug. Sometimes, when the unassigned list
                // is updated during a selection index change event, there is a 2nd event
                // that comes in right away with the same value.
                var timeSinceLastEvent = DateTime.Now - _lastSelectionTime;
                if ((_lastSelection == value) &&  (timeSinceLastEvent.TotalMilliseconds < 1000))
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

        DateTime _lastRemoveSelectionTime = DateTime.Now;
        private int _lastRemoveSelection = -1;

        private int _removeSelection = -1;
        public int RemoveSelection { get { return _removeSelection; } 
            set 
            {
                var timeSinceLastEvent = DateTime.Now - _lastRemoveSelectionTime;
                if ((_lastRemoveSelection == value) && (timeSinceLastEvent.TotalMilliseconds < 1000))
                {
                    System.Diagnostics.Debug.WriteLine("Duplicate remove selection event ms: " + timeSinceLastEvent.TotalMilliseconds);

                    // Since the ListBox thinks the same item has been selected, you can't click
                    // on it again. Setting to an empty list clears the selected item without
                    // triggering another selection event.
                    var savedList = TeeTimeRequestsAssigned;
                    TeeTimeRequestsAssigned = new TrulyObservableCollection<TeeTimeRequest>();
                    TeeTimeRequestsAssigned = savedList;
                    return;
                }
                _lastRemoveSelectionTime = DateTime.Now;
                _lastRemoveSelection = value;

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
        public TrulyObservableCollection<Player> PlayersToRemoveFromSignup { get; set; }
        public TrulyObservableCollection<ReplacePlayer> PlayersToReplaceFromSignup { get; set; }

        private string _teeTimeFile;
        public string TeeTimeFile
        {
            get { return _teeTimeFile; }
            set { _teeTimeFile = value; OnPropertyChanged(); }
        }

        private OrderTeeTimeRequestsByEnum _orderTeeTimeRequestsBy;
        public OrderTeeTimeRequestsByEnum OrderTeeTimeRequestsBy
        {
            get { return _orderTeeTimeRequestsBy; }
            set
            {
                _orderTeeTimeRequestsBy = value;
                OnPropertyChanged();

                if ((value == OrderTeeTimeRequestsByEnum.HistoricalTeeTimes) && (UnprocessedHistoricalTeeTimeData == null))
                {
                    MessageBox.Show("Note: Sorting by requested tee times, since historical data has not been loaded yet");
                }
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

        private bool _teeTimesDirty = false;

        private int _currentNumberOfPlayersShowing;

        private Random _randomNumberGenerator;

        private TournamentAndTeeTimes[] UnprocessedHistoricalTeeTimeData = null;
        private List<PlayerTeeTimeHistory> PlayerTeeTimeHistoryByName = null;
        private Hashtable PlayerTeeTimeHistoryHashTableByGhin = null;
        private bool RecalculateBlindDrawOnSelection = false;
        private List<string> BlockedTeeTimes;
        private List<string> PreviouslyWaitlistedGhins;
        private bool WorkingOnSignups = false;
        #endregion

        #region Commands
        public ICommand GetTournamentsCommand { get { return new ModelCommand(async s => await GetTournaments(s)); } }

        public ICommand LoadSignupsCommand { get { return new ModelCommand(async s => await LoadSignupsFromWeb(s)); } }

        public ICommand LoadTeetimesCommand { get { return new ModelCommand(async s => await LoadTeeTimesFromWeb(s)); } }

        public ICommand UploadTeetimesCommand { get { return new ModelCommand(async s => await UploadToWeb(s)); } }

        public ICommand SaveAsCsvCommand { get { return new ModelCommand(SaveTeeTimesAsCsv); } }

        public ICommand AddPlayerCommand { get { return new ModelCommand(AddPlayer); } }

        public ICommand RemovePlayerCommand { get { return new ModelCommand(RemovePlayer); } }

        public ICommand ReplacePlayerCommand { get { return new ModelCommand(ReplacePlayerCmd); } }

        public ICommand ChangePartnersInTeeTimeCommand { get { return new ModelCommand(ChangePartnersInTeeTime); } }

        public ICommand LoadTeeTimesAndWaitlistCsvCommand { get { return new ModelCommand(LoadTeeTimesAndWaitlistCsv); } }

        public ICommand LoadHistoricalTeeTimesDataCommand { get { return new ModelCommand(async s => await LoadHistoricalTeeTimesDataAsync(s)); } }

        public ICommand SaveTeeTimeHistoryAsCsvCommand { get { return new ModelCommand(SaveTeeTimeHistoryAsCsv); } }

        public ICommand BlockTeeTimeCommand { get { return new ModelCommand(BlockTeeTime); } }
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

            BlockedTeeTimes = Options.BlockedOutTeeTimes;
            _defaultTeeTimes = _defaultTeeTimes78;
            InitFirstTeeTimeList();
            InitTeeTimes();
            ResetBlockedTeeTimes();

            GetTournamentsVisible = Visibility.Visible;
            GotTournamentsVisible = Visibility.Collapsed;
            AllowTeeTimeIntervalAdjust = true;
            _randomNumberGenerator = new Random();
            CancelledPlayers = new TrulyObservableCollection<Player>();
            PlayersToRemoveFromSignup = new TrulyObservableCollection<Player>();
            PlayersToReplaceFromSignup = new TrulyObservableCollection<ReplacePlayer>();
            TeeTimeSource = "";
            MonthsOfTeeTimeDataToLoad = Options.MonthsOfTeeTimeDataToLoad;

            // Need to change XAML default if you change default here
            OrderTeeTimeRequestsBy = OrderTeeTimeRequestsByEnum.RequestedTime;

            PreviouslyWaitlistedGhins = new List<string>();
        }

        public void InitTeeTimes()
        {
            ClearPlayersFromAllTeeTimes();
            TournamentTeeTimes = new TrulyObservableCollection<TeeTime>();

            int firstTime = (FirstTeeTimeIndex < 0) ? 0 : FirstTeeTimeIndex;
            for (int i = firstTime; i < _defaultTeeTimes.Count; i++)
            {
                TournamentTeeTimes.Add(new TeeTime { StartTime = _defaultTeeTimes[i] });
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

                foreach (var ttr in TeeTimeRequests)
                {
                    ttr.ShowBlindDrawValue = OrderTeeTimeRequestsBy == OrderTeeTimeRequestsByEnum.BlindDraw;
                    ttr.ShowLastTeeTime = OrderTeeTimeRequestsBy == OrderTeeTimeRequestsByEnum.LastTeeTime;
                }

                switch (OrderTeeTimeRequestsBy)
                {
                    case OrderTeeTimeRequestsByEnum.HistoricalTeeTimes:
                        if (UnprocessedHistoricalTeeTimeData == null)
                        {
                            TeeTimeRequests.Sort(new TeeTimeRequestSort());
                        }
                        else
                        {
                            TeeTimeRequests.Sort(new HistoricalTeeTimeSort());
                        }
                        break;
                    case OrderTeeTimeRequestsByEnum.LastTeeTime:
                        if (UnprocessedHistoricalTeeTimeData == null)
                        {
                            TeeTimeRequests.Sort(new TeeTimeRequestSort());
                        }
                        else
                        {
                            TeeTimeRequests.Sort(new LastTeeTimeSort());
                        }
                        break;
                    case OrderTeeTimeRequestsByEnum.BlindDraw:
                        TeeTimeRequests.Sort(new BlindDrawKeySort());
                        break;
                    default:
                    case OrderTeeTimeRequestsByEnum.RequestedTime:
                        TeeTimeRequests.Sort(new TeeTimeRequestSort());
                        break;
                }

                UpdateUnassignedList(_currentNumberOfPlayersShowing);
            }
        }

        private void ResetBlockedTeeTimes()
        {
            for (int i = 0; i < TournamentTeeTimes.Count; i++)
            {
                for (int j = 0; j < BlockedTeeTimes.Count; j++)
                {
                    if (string.Compare(TournamentTeeTimes[i].StartTime, BlockedTeeTimes[j]) == 0)
                    {
                        if (TournamentTeeTimes[i].Players.Count == 0)
                        {
                            TournamentTeeTimes[i].BlockedOut = true;
                        }
                        break;
                    }
                }
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

            ResetBlockedTeeTimes();

            // Reset the current tee time selection to be
            // the first tee time with openings.
            i = 0;
            for (; i < TournamentTeeTimes.Count; i++)
            {
                if ((TournamentTeeTimes[i].Players.Count < 4) && !TournamentTeeTimes[i].BlockedOut)
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

        private void LoadPreviouslyWaitlistedGhins()
        {
            if (string.IsNullOrWhiteSpace(Options.SignupWaitListFileName))
            {
                PreviouslyWaitlistedGhins.Clear();
                return;
            }

            if (!File.Exists(Options.SignupWaitListFileName))
            {
                throw new FileNotFoundException("Historical waiting list file does not exist: " + Options.SignupWaitListFileName);
            }

            string[][] csvFileEntries;
            using (TextReader tr = new StreamReader(Options.SignupWaitListFileName))
            {
                csvFileEntries = CSVParser.Parse(tr);
            }

            int ghinCol = -1;
            for (int col = 0; col < csvFileEntries[0].Length; col++)
            {
                if (string.Compare(csvFileEntries[0][col], "ghin", true) == 0)
                {
                    ghinCol = col;
                    break;
                }
            }

            if (ghinCol == -1)
            {
                throw new ArgumentException("Historical waiting list file does not contain a column header with \"GHIN\"");
            }

            PreviouslyWaitlistedGhins.Clear();
            for (int row = 1; row < csvFileEntries.Length; row++)
            {
                if (!string.IsNullOrWhiteSpace(csvFileEntries[row][ghinCol]))
                {
                    PreviouslyWaitlistedGhins.Add(csvFileEntries[row][ghinCol].Trim());
                }
            }
        }

        private void MarkPlayersPreviouslyWaitlisted()
        {
            foreach (var request in TeeTimeRequests)
            {
                for (int i = 0; i < request.Players.Count; i++)
                {
                    request.Players[i].PreviouslyWaitlisted = PreviouslyWaitlistedGhins.Contains(request.Players[i].GHIN);
                }
            }
        }

        public void UpdateUnassignedList(int playerCount)
        {
            // If the tee time is full, just show all the unassigned players,
            // otherwise there are times you expect to see the waitlist (unassigned players)
            // and the list looks empty
            if (playerCount == 0)
            {
                playerCount = 4;
            }
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

            // Here's how to do the assignment on a new thread
            //Dispatcher.CurrentDispatcher.BeginInvoke(
            //                    new Action(() => TeeTimeRequestsUnassigned = teeTimeRequestsUnassigned));

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
                teeTime.StartTime = ((i < 60) ? "01:" : "02:") +  (i % 60).ToString("D2") + " PM";

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

             
            if (teeTimeRequest.Waitlisted)
            {
                teeTimeRequest.Waitlisted = false;
                if (WorkingOnSignups)
                {
                    // If players were selected from the waitlist, 
                    // recalculate the blind draw waitlist, but only
                    // if data was loaded from signups -- not when it is
                    // loaded from the website tee times or csv file.
                    //
                    // Reduce the blind draw number to keep them off the waitlist
                    teeTimeRequest.BlindDrawValue = 1000;
                    BlindDraw();
                    // Re-sort since new players may have been marked as waitlisted
                    SortTeeTimeRequests();
                    return;
                }
            }

            if (teeTime.BlockedOut)
            {
                MessageBox.Show(teeTime.StartTime + " is blocked out.");
                ClearTodoSelection();
                return;
            }

            // Check for room in this tee time
            if (teeTimeRequest.Players.Count > (4 - teeTime.Players.Count))
            {
                MessageBox.Show((teeTime.Players.Count == 4) 
                    ? "Tee time is full" 
                    : "Not enough room for all players at " + teeTime.StartTime);
                ClearTodoSelection();
                return;
            }

            CheckTeeTimeVsPreferredTime(teeTime.StartTime, teeTimeRequest.Preference);

            // Add the players to the tee time
            _teeTimesDirty = true;
            bool gaveAfter11Msg = false;
            foreach (var player in teeTimeRequest.Players)
            {
                teeTime.AddPlayer(player);
                player.TeeTime = teeTime;

                if (!gaveAfter11Msg)
                {
                    gaveAfter11Msg = CheckLastTeeTime(teeTime.StartTime, player.GHIN, player.Name);
                }
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
                    if ((TournamentTeeTimes[i].Players.Count < 4) && !TournamentTeeTimes[i].BlockedOut)
                    {
                        teeTimeFound = true;
                        // This also updates the unassigned list
                        TeeTimeSelection = i;
                        break;
                    }
                }

                if (!teeTimeFound)
                {
                    ClearTodoSelection();

                    // show all remaining unassigned
                    UpdateUnassignedList(4);
                }
            }
            else
            {
                ClearTodoSelection();

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

        private void CheckTeeTimeVsPreferredTime(string teeTime, string preferredTime)
        {
            // Check for selecting a signup which requested a later time 
            // than the current tee time
            // string is either of the form "6am-12pm" or "None"
            string[] h1 = preferredTime.Split(new string[] { "am" }, StringSplitOptions.None);
            if (h1.Length > 1)
            {
                int requestStartHour;
                if (int.TryParse(h1[0], out requestStartHour))
                {
                    // string is of the form "7:15"
                    string[] h2 = teeTime.Split(':');
                    if (h2.Length > 1)
                    {
                        int teeTimeStartHour;
                        if (int.TryParse(h2[0], out teeTimeStartHour))
                        {
                            if (teeTimeStartHour < requestStartHour)
                            {
                                MessageBox.Show("Note: Tee time is " + teeTime + ", which is earlier than the requested time " + preferredTime);
                            }
                        }
                    }
                }
            }
        }

        private bool CheckLastTeeTime(string teeTime, string ghin, string name)
        {
            if (PlayerTeeTimeHistoryHashTableByGhin == null) return false;

            if (PlayerTeeTimeHistoryHashTableByGhin.ContainsKey(ghin))
            {
                var ptth = (PlayerTeeTimeHistory)PlayerTeeTimeHistoryHashTableByGhin[ghin];

                string[] h1 = ptth.LastTeeTime.Split(':');
                if (h1.Length > 1)
                {
                    int hour;
                    if (int.TryParse(h1[0], out hour))
                    {
                        if (hour >= 11)
                        {
                            string[] h2 = teeTime.Split(':');
                            if (h2.Length > 1)
                            {
                                int teeTimeStartHour;
                                if (int.TryParse(h2[0], out teeTimeStartHour))
                                {
                                    if (teeTimeStartHour >= 11)
                                    {
                                        MessageBox.Show("Note: " + name + " had a late tee time (" + ptth.LastTeeTime + ") last time also.");
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return false;
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

            // Check for empty tee time list
            int playerCount = 0;
            for (int i = 0; i < TournamentTeeTimes.Count; i++)
            {
                for (int player = 0; player < TournamentTeeTimes[i].Players.Count; player++)
                {
                    playerCount++;
                }
            }
            if (playerCount == 0)
            {
                MessageBoxResult result = MessageBox.Show("There are no players assigned tee times. Upload empty tee time list?",
                                          "Confirmation",
                                          MessageBoxButton.YesNo,
                                          MessageBoxImage.Question);
                if (result != MessageBoxResult.Yes)
                {
                    return;
                }
            }

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
                    var orderBy = OrderTeeTimeRequestsBy;

                    // order by blind draw to upload to web
                    OrderTeeTimeRequestsBy = OrderTeeTimeRequestsByEnum.BlindDraw;

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
                        OrderTeeTimeRequestsBy = orderBy;
                    }
                }

                if (CancelledPlayers.Count > 0)
                {
                    for (int i = 0; i < CancelledPlayers.Count; i++)
                    {
                        values.Add(new KeyValuePair<string, string>(
                            string.Format("CancelledPlayer[{0}][Position]", i),
                            i.ToString(CultureInfo.InvariantCulture)));

                        values.Add(new KeyValuePair<string, string>(
                            string.Format("CancelledPlayer[{0}][GHIN]", i),
                            CancelledPlayers[i].GHIN.ToString(CultureInfo.InvariantCulture)));

                        values.Add(new KeyValuePair<string, string>(
                            string.Format("CancelledPlayer[{0}][Name]", i),
                            CancelledPlayers[i].Name));
                    }
                }

                if (PlayersToRemoveFromSignup.Count > 0)
                {
                    for (int i = 0; i < PlayersToRemoveFromSignup.Count; i++)
                    {
                        values.Add(new KeyValuePair<string, string>(
                            string.Format("Remove[{0}][GHIN]", i),
                            PlayersToRemoveFromSignup[i].GHIN.ToString(CultureInfo.InvariantCulture)));

                        values.Add(new KeyValuePair<string, string>(
                            string.Format("Remove[{0}][Name]", i),
                            PlayersToRemoveFromSignup[i].Name));
                    }
                }

                if (PlayersToReplaceFromSignup.Count > 0)
                {
                    for (int i = 0; i < PlayersToReplaceFromSignup.Count; i++)
                    {
                        values.Add(new KeyValuePair<string, string>(
                            string.Format("Replace[{0}][GHIN]", i),
                            PlayersToReplaceFromSignup[i].Remove.GHIN.ToString(CultureInfo.InvariantCulture)));

                        values.Add(new KeyValuePair<string, string>(
                            string.Format("Replace[{0}][Name]", i),
                            PlayersToReplaceFromSignup[i].Remove.Name));

                        values.Add(new KeyValuePair<string, string>(
                            string.Format("Replace[{0}][NewGHIN]", i),
                            PlayersToReplaceFromSignup[i].Add.GHIN.ToString(CultureInfo.InvariantCulture)));

                        values.Add(new KeyValuePair<string, string>(
                            string.Format("Replace[{0}][NewName]", i),
                            PlayersToReplaceFromSignup[i].Add.Name));

                        values.Add(new KeyValuePair<string, string>(
                            string.Format("Replace[{0}][NewExtra]", i),
                            PlayersToReplaceFromSignup[i].Add.Extra));
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
                        MessageBoxResult result = MessageBox.Show("Tee times uploaded. Save as CSV also?",
                                          "Confirmation",
                                          MessageBoxButton.YesNo,
                                          MessageBoxImage.Question);
                        if (result == MessageBoxResult.Yes)
                        {
                            SaveTeeTimesAsCsv(o);
                        }
                        PlayersToRemoveFromSignup.Clear();
                        PlayersToReplaceFromSignup.Clear();
                        if (WorkingOnSignups)
                        {
                            ConvertWaitingListToSinglePlayers();
                        }
                        WorkingOnSignups = false;
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

        private void ConvertWaitingListToSinglePlayers()
        {
            List<TeeTimeRequest> singlePlayers = new List<TeeTimeRequest>();

            OrderTeeTimeRequestsBy = OrderTeeTimeRequestsByEnum.BlindDraw;

            int blindDrawNumber = 1000;
            foreach (var request in TeeTimeRequestsUnassigned)
            {
                foreach (var player in request.Players)
                {
                    var teeTimeRequest = new TeeTimeRequest();
                    teeTimeRequest.Players.Add(player);
                    teeTimeRequest.Waitlisted = true;
                    teeTimeRequest.Preference = request.Preference;
                    teeTimeRequest.BlindDrawValue = blindDrawNumber++;
                    singlePlayers.Add(teeTimeRequest);
                }
            }

            TeeTimeRequests = singlePlayers;
            UpdateUnassignedList(4);
        }

        private void SaveTeeTimesAsCsv(object o)
        {
            int teamId = 0;
            string teeTimesFileName = "Teetimes - " + TournamentNames[TournamentNameIndex].Name.Replace('/', '-');

            // Append an index to the file name and search for the first one that
            // is not used already
            string defaultFolder = string.IsNullOrEmpty(Options.LastCSVTeeTimesFolder) ? "c:/" : Options.LastCSVTeeTimesFolder;
            for (int i = 1; i < 200; i++)
            {
                string ttfnWithIndex = (Path.Combine(defaultFolder, teeTimesFileName + " " + i + ".csv"));
                if (!File.Exists(ttfnWithIndex))
                {
                    teeTimesFileName = ttfnWithIndex;
                    break;
                }
            }

            // Configure save file dialog box
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = Path.GetFileName(teeTimesFileName);
            dlg.DefaultExt = ".csv"; // Default file extension
            dlg.Filter = "CSV File (.csv)|*.csv"; // Filter files by extension

            // Show save file dialog box
            bool? result = dlg.ShowDialog();

            if (result != true)
            {
                return;
            }

            Options.LastCSVTeeTimesFolder = Path.GetDirectoryName(dlg.FileName);
            teeTimesFileName = dlg.FileName;
            TeeTimeFile = dlg.FileName; // TeeTimeFile may not be needed anymore

            SaveAsCsv(
                TournamentTeeTimes, 
                TournamentNames[TournamentNameIndex].TeamSize,
                out bool _teeTimesDirty,
                ref teeTimesFileName,
                ref teamId,
                false,
                TeeTimeStatus.TeeTime);

            // If the tee times were saved and there are still 
            // tee time requests, save the remaining requests
            // as waitlisted players.
            if (TeeTimeRequests.Count > 0)
            {
                // save setting
                var orderBy = OrderTeeTimeRequestsBy;

                // order by blind draw to save in file
                OrderTeeTimeRequestsBy = OrderTeeTimeRequestsByEnum.BlindDraw;

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
                    OrderTeeTimeRequestsBy = orderBy;
                }
            }

            if (CancelledPlayers.Count > 0)
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
        private static void SaveAsCsv(TrulyObservableCollection<TeeTime> tournamentTeeTimes, int teamSize,
            out bool teeTimesDirty, ref string finalFileName, ref int teamId, bool appendToFile, TeeTimeStatus teeTimeStatus)
        {
            teeTimesDirty = false;
                
            using (TextWriter tw = new StreamWriter(finalFileName, appendToFile))
            {
                if (!appendToFile)
                {
                    tw.WriteLine("Start Time,Tee Status,Team Id,Last Name,First Name,GHIN,Flight,Email,Extra,Tee");
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
                        string playerTee = string.Empty;

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
                            playerTee = tournamentTeeTimes[teeTimeNumber].Players[player].Tee;

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
                                else if (playerExtra.Contains("F1") || playerExtra.ToLower().Contains("flight1"))
                                {
                                    tw.Write("1");
                                }
                                else if (playerExtra.Contains("F2") || playerExtra.ToLower().Contains("flight2"))
                                {
                                    tw.Write("2");
                                }
                                else if (playerExtra.Contains("F3") || playerExtra.ToLower().Contains("flight3"))
                                {
                                    tw.Write("3");
                                }
                                else if (playerExtra.Contains("F4") || playerExtra.ToLower().Contains("flight4"))
                                {
                                    tw.Write("4");
                                }
                                else if (playerExtra.Contains("F5") || playerExtra.ToLower().Contains("flight5"))
                                {
                                    tw.Write("5");
                                }
                            }

                            tw.Write("," + playerEmail + ",");

                            if (!string.IsNullOrEmpty(playerExtra))
                            {
                                // Member/Guest
                                if ((playerExtra == "M") || (playerExtra == "G"))
                                {
                                    tw.Write(playerExtra);
                                }
                            }

                            tw.Write("," + playerTee);

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
                if (MessageBox.Show("Some tee times have been filled in, players added, or players removed. Do you want to continue? Your current work will be lost.",
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
                    PlayersToRemoveFromSignup.Clear();
                    PlayersToReplaceFromSignup.Clear();
                    AllowTeeTimeIntervalAdjust = true;

                    if (string.IsNullOrEmpty(responseString))
                    {
                        MessageBox.Show("Website gave empty response for tee time request list");
                    }
                    else
                    {
                        TeeTimeRequests = LoadSignupsFromWebResponseJson(responseString);

                        LoadPreviouslyWaitlistedGhins();

                        MarkPlayersPreviouslyWaitlisted();

                        AssignBlindDrawNumbers();
                    }
                    
                    UpdateUnassignedList(4);

                    InitTeeTimes();
                    ResetBlockedTeeTimes();

                    // Blind draw after tee times cleared
                    RecalculateBlindDrawOnSelection = true;

                    CalculateHistoricalTeeTimeMeanAndStdevForTeeTimeRequests();

                    if ((TeeTimeRequests.Count > 0) && 
                        (OrderTeeTimeRequestsBy == OrderTeeTimeRequestsByEnum.HistoricalTeeTimes) && 
                        (UnprocessedHistoricalTeeTimeData == null))
                    {
                        MessageBox.Show("Note: Sorting by requested tee times, since historical data has not been loaded yet");
                    }

                    BlindDraw();
                    SortTeeTimeRequests();
                }
            }

            TeeTimeSource = "Tee times were loaded from the website signups";
            WorkingOnSignups = true;

            GroupMode = true;

            if ((UnprocessedHistoricalTeeTimeData == null) || (PlayerTeeTimeHistoryHashTableByGhin.Count == 0))
            {
                await LoadHistoricalTeeTimesDataAsync(o);
                // Since we have historical data, include that in the sort.
                // The historical data may have adjusted blind draw numbers, so repeat the blind draw.
                BlindDraw();
                SortTeeTimeRequests();
            }
        }

        private void AssignBlindDrawNumbers()
        {

            foreach (var request in TeeTimeRequests)
            {
                int playersPreviouslyWaitlisted = 0;
                bool isBoardMember = false;
                bool isGuest = false;
                foreach (var player in request.Players)
                {
                    if (player.PreviouslyWaitlisted)
                    {
                        playersPreviouslyWaitlisted++;
                    }

                    if (string.Compare(player.SignupPriority, "B", true) == 0)
                    {
                        isBoardMember = true;
                    }

                    if (string.Compare(player.Extra, "G", true) == 0)
                    {
                        isGuest = true;
                    }
                }

                /*
                 * 500 - Board members
                 * 1000-1999 - Member-Guest tournament and group has a guest
                 * 2000-4999 - Someone in group was previously on waitlist
                 * 5000-5999 - Someone in group has played 2 or fewer rounds
                 * 6000-9999 - None of the above criteria apply
                 */
                if (isBoardMember)
                {
                    // Put board members at the highest priority
                    request.BlindDrawValue = 500;
                }
                else if (isGuest && TournamentNames[TournamentNameIndex].MemberGuest)
                {
                    // For the member-guest, favor those groups with a guest
                    request.BlindDrawValue = _randomNumberGenerator.Next(1000, 1999);
                }
                else if (playersPreviouslyWaitlisted != 0)
                {
                    // Give groups with previously waitlist players more favorable numbers
                    request.BlindDrawValue = _randomNumberGenerator.Next(2000, 4999);
                }
                else
                {
                    // Worst priority to groups with no one waitlisted
                    request.BlindDrawValue = _randomNumberGenerator.Next(6000, 9999);
                }
            }
        }

        private void AdjustBlindDrawNumbersForInfrequentPlayers()
        {
            foreach (var request in TeeTimeRequests)
            {
                bool infrequentPlayer = false;
                foreach (var player in request.Players)
                {
                    if ((player.TeeTimeCount >= 0) && (player.TeeTimeCount <= 2))
                    {
                        infrequentPlayer = true;
                    }
                }

                if (infrequentPlayer)
                {
                    int newBlindDrawNumber = _randomNumberGenerator.Next(5000, 5999);
                    if (newBlindDrawNumber < request.BlindDrawValue)
                    {
                        request.BlindDrawValue = newBlindDrawNumber;
                    }
                }
            }
        }

        private void BlindDraw()
        {
            if (!RecalculateBlindDrawOnSelection) return;

            // If a player is selected from the waitlist, recalculate
            // the waitlist. That has to take into account how many
            // players have tee times already.
            int playersWithTeeTimes = 0;
            foreach (var teeTime in TournamentTeeTimes)
            {
                playersWithTeeTimes += teeTime.Players.Count;
            }

            // Copy the tee time request list and sort the copy
            List<TeeTimeRequest> teeTimeRequestsCopy = new List<TeeTimeRequest>();
            foreach (var ttr in TeeTimeRequests)
            {
                teeTimeRequestsCopy.Add(ttr);
            }

            teeTimeRequestsCopy.Sort(new BlindDrawKeySort());

            int playerCount = playersWithTeeTimes;
            for (int i = 0; i < teeTimeRequestsCopy.Count; i++)
            {
                teeTimeRequestsCopy[i].Waitlisted = playerCount >= BlindDrawPlayerCount;
                if (!teeTimeRequestsCopy[i].Waitlisted)
                {
                    playerCount += teeTimeRequestsCopy[i].Players.Count;
                }
            }
            // At this point it might be the case that if the waitlist is 172 that we have 
            // 174 players not waitlisted. Go backwards to find a pair or 2 singles to put on the
            // waitlist to get an exact number
            for (int i = teeTimeRequestsCopy.Count - 1; (i >= 0) && (playerCount > BlindDrawPlayerCount); i--)
            {
                if (!teeTimeRequestsCopy[i].Waitlisted)
                {
                    if (playerCount - teeTimeRequestsCopy[i].Players.Count >= BlindDrawPlayerCount)
                    {
                        teeTimeRequestsCopy[i].Waitlisted = true;
                        playerCount -= teeTimeRequestsCopy[i].Players.Count;
                    }
                }
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
                int hourDifference =  a.GetHour() - b.GetHour();

                // If the hours are the same, sort by average start time
                if (hourDifference == 0)
                {
                    // put those with no data last
                    if ((a.StartTimeAverageInSeconds == 0) && (b.StartTimeAverageInSeconds != 0)) return 1;
                    if ((a.StartTimeAverageInSeconds != 0) && (b.StartTimeAverageInSeconds == 0)) return -1;

                    return (int)(a.StartTimeAverageInSeconds - b.StartTimeAverageInSeconds);
                }
                return hourDifference;
            }
        }

        private class HistoricalTeeTimeSort : IComparer<TeeTimeRequest>
        {
            public int Compare(TeeTimeRequest a, TeeTimeRequest b)
            {
                // Put all the waitlisted groups at the end
                if (a.Waitlisted && !b.Waitlisted) return 1;
                if (!a.Waitlisted && b.Waitlisted) return -1;
                // Sort the waitlisted groups by the blind draw value
                if (a.Waitlisted && b.Waitlisted) return a.BlindDrawValue - b.BlindDrawValue;

                // put those with no data last
                if ((a.StartTimeAverageInSeconds == 0) && (b.StartTimeAverageInSeconds != 0)) return 1;
                if ((a.StartTimeAverageInSeconds != 0) && (b.StartTimeAverageInSeconds == 0)) return -1;

                // Sort the non-waitlisted groups by average start time
                return (int)(a.StartTimeAverageInSeconds - b.StartTimeAverageInSeconds);
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

        private class LastTeeTimeSort : IComparer<TeeTimeRequest>
        {
            public int Compare(TeeTimeRequest a, TeeTimeRequest b)
            {
                // Put all the waitlisted groups at the end
                if (a.Waitlisted && !b.Waitlisted) return 1;
                if (!a.Waitlisted && b.Waitlisted) return -1;
                // Sort the waitlisted groups by the blind draw value
                if (a.Waitlisted && b.Waitlisted) return a.BlindDrawValue - b.BlindDrawValue;

                // LastTeeTime is just a string
                if ((a.LastTeeTime == null) || (b.LastTeeTime == null)) return 0;
                string aTime = a.LastTeeTime[1] == ':' ? ("0" + a.LastTeeTime) : a.LastTeeTime;
                string bTime = b.LastTeeTime[1] == ':' ? ("0" + b.LastTeeTime) : b.LastTeeTime;
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

            if (!CheckContinue()) return;

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

                    TeeTimeRequests.Clear();
                    TeeTimeRequestsUnassigned.Clear();
                    TeeTimeRequestsAssigned.Clear();
                    TournamentTeeTimes.Clear();
                    CancelledPlayers.Clear();
                    PlayersToRemoveFromSignup.Clear();
                    PlayersToReplaceFromSignup.Clear();

                    AllowTeeTimeIntervalAdjust = false;

                    var teeTimeComposite = LoadTeeTimeCompositeFromWebResponseJson(responseString);

                    LoadTeeTimesFromWebResponse(teeTimeComposite.TeeTimes);
                    FillInAssignedListFromTournamentTeeTimes();

                    LoadWaitingListFromWebResponse(teeTimeComposite.WaitListPlayers);
                    RecalculateBlindDrawOnSelection = false;

                    if (teeTimeComposite.CancelledPlayers != null)
                    {
                        CancelledPlayers = teeTimeComposite.CancelledPlayers;
                    }
                    
                    SelectOpenTeeTime();
                }
            }

            GroupMode = false;
            TeeTimeSource = "Tee times were loaded from the website";
            WorkingOnSignups = false;
        }

        protected TeeTimeComposite LoadTeeTimeCompositeFromWebResponseJson(string webResponse)
        {
            if (string.IsNullOrEmpty(webResponse))
            {
                return new TeeTimeComposite();
            }

            if (webResponse.StartsWith("JSON error:"))
            {
                throw new Exception(webResponse);
            }

            var jss = new JavaScriptSerializer();
            var teeTimeComposite = jss.Deserialize<TeeTimeComposite>(webResponse);

            if (teeTimeComposite == null)
            {
                return new TeeTimeComposite();
            }

            return teeTimeComposite;
        }

        protected void LoadTeeTimesFromWebResponse(TrulyObservableCollection<TeeTime> tournamentTeeTimes)
        {
            if (tournamentTeeTimes == null)
            {
                return;
            }

            // Initialize the first tee time combo box.
            int lowestTeeTimeIndex = -1;
            for (int teeTimeIndex = 0; teeTimeIndex < tournamentTeeTimes.Count; teeTimeIndex++)
            {
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
            ResetBlockedTeeTimes();
            UpdateBlindDrawPlayerCount();

            // Need to tie player to the tee time
            foreach (var teeTime in TournamentTeeTimes)
            {
                foreach (var player in teeTime.Players)
                {
                    player.TeeTime = teeTime;
                }
            }
        }

        

        protected void LoadWaitingListFromWebResponse(TrulyObservableCollection<Player> players)
        {
            if (players == null)
            {
                return;
            }

            foreach (var player in players)
            {
                var teeTimeRequest = new TeeTimeRequest();
                teeTimeRequest.Players.Add(player);
                teeTimeRequest.Waitlisted = true;
                teeTimeRequest.Preference = "None";
                teeTimeRequest.BlindDrawValue = player.Position; 
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

        private void BlockTeeTime(object o)
        {
            TeeTime teeTime = TournamentTeeTimes[TeeTimeSelection];

            if (teeTime.Players.Count > 0)
            {
                MessageBox.Show(Application.Current.MainWindow,
                                "There are " + teeTime.Players.Count + " players at " + teeTime.StartTime + ". You can only block an empty tee time.");
                return;
            }

            // Toggle the boolean
            teeTime.BlockedOut = !teeTime.BlockedOut;
            UpdateBlindDrawPlayerCount();

            // Rebuild the blocked list
            BlockedTeeTimes = new List<string>();
            for (int i = 0; i < TournamentTeeTimes.Count; i++)
            {
                if (TournamentTeeTimes[i].BlockedOut)
                {
                    BlockedTeeTimes.Add(TournamentTeeTimes[i].StartTime);
                }
            }
            // Save for next time
            Options.BlockedOutTeeTimes = BlockedTeeTimes;

            // Select the first time that is not full after the blocked time.
            for (int i = TeeTimeSelection; i < TournamentTeeTimes.Count; i++)
            {
                if (!TournamentTeeTimes[i].BlockedOut && (TournamentTeeTimes[i].Players.Count < 4))
                {
                    TeeTimeSelection = i;
                    break;
                }
            }
        }

        private void UpdateBlindDrawPlayerCount()
        {
            int count = 0;
            for (int i = 0; i < TournamentTeeTimes.Count; i++)
            {
                if (!TournamentTeeTimes[i].BlockedOut)
                {
                    count += 4;
                }
            }

            BlindDrawPlayerCount = count;
        }

        private Player GetPlayerToAdd(bool replacingPlayer)
        {
            List<GHINEntry> ghinList = new List<GHINEntry>();
            try
            {
                ghinList = GHINEntry.LoadGHIN(Options.GHINFileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show(Application.Current.MainWindow, "Error: " + ex.Message);
                return null;
            }

            AddPlayerWindow apw = new AddPlayerWindow();
            Player player = new Player();
            player.Position = 0;
            apw.DataContext = player;
            apw.Player = player;
            apw.GHINList = ghinList;
            apw.RequiresFlight = false;
            apw.AllowGuest = TournamentNames[TournamentNameIndex].MemberGuest;
            apw.Owner = Application.Current.MainWindow;

            // See if any of the players assigned a tee time has a flight
            for (int i = 0; (i < TournamentTeeTimes.Count) && !apw.RequiresFlight; i++)
            {
                for (int j = 0; (j < TournamentTeeTimes[i].Players.Count) && !apw.RequiresFlight; j++)
                {
                    if (TournamentTeeTimes[i].Players[j].Extra.ToLower().Contains("f"))
                    {
                        apw.RequiresFlight = true;
                    }
                }
            }

            apw.ShowDialog();
            if (apw.DialogResult.HasValue && apw.DialogResult.Value)
            {
                foreach (var request in TeeTimeRequestsUnassigned)
                {
                    foreach (var p in request.Players)
                    {
                        if (!string.IsNullOrEmpty(player.GHIN) && (String.CompareOrdinal(player.GHIN, p.GHIN) == 0))
                        {
                            if (replacingPlayer)
                            {
                                return p;
                            }
                            else
                            {
                                MessageBox.Show(Application.Current.MainWindow,
                                    player.Name + " is already in the signup list at tee time preference " + request.Preference);
                                return null;
                            }
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
                            return null;
                        }
                    }
                }

                return player;
            }

            return null;
        }

        private void RemoveFromCancelledList(Player player)
        {
            // Remove from the cancelled list if player is there
            Player cancelledPlayer = null;
            foreach (var p in CancelledPlayers)
            {
                if (!string.IsNullOrEmpty(player.GHIN) && (String.CompareOrdinal(player.GHIN, p.GHIN) == 0))
                {
                    cancelledPlayer = p;
                    break;
                }
            }
            if (cancelledPlayer != null)
            {
                CancelledPlayers.Remove(cancelledPlayer);
            }
        }

        private void AddPlayer(object o)
        {
            Player player = GetPlayerToAdd(false);

            if (player == null) return;

            TeeTimeRequest ttr = new TeeTimeRequest();
            ttr.Players = new TrulyObservableCollection<Player> { player };
            ttr.Preference = "None";

            RemoveFromCancelledList(player);

            if (!string.IsNullOrEmpty(player.GHIN))
            {
                List<string> ghinNumbers = new List<string> { player.GHIN };
                double mean;
                double stdev;
                int count;
                string lastTeeTime;
                CalculateHistoricalTeeTimeMeanAndStdev(ghinNumbers, out mean, out stdev, out count, out lastTeeTime);
                ttr.StartTimeAverageInSeconds = mean;
                ttr.StartTimeStandardDeviationInSeconds = stdev;
                ttr.TeeTimeCount = count;
                if (TournamentNames[TournamentNameIndex].MemberGuest)
                {
                    player.Extra = "M";
                }
            }
            else if(TournamentNames[TournamentNameIndex].MemberGuest)
            {
                AddGhinWindow agw = new AddGhinWindow();
                agw.Owner = Application.Current.MainWindow;
                agw.ShowDialog();
                player.GHIN = agw.Ghin;
                player.Extra = "G";
            }

            foreach (var ttr2 in TeeTimeRequests)
            {
                // Add to the end of the waitlist if there is one
                if (ttr2.Waitlisted)
                {
                    ttr.Waitlisted = true;
                    ttr.BlindDrawValue = ttr2.BlindDrawValue + 1;
                }
            }

            TeeTimeRequests.Add(ttr);
            SortTeeTimeRequests();

            _teeTimesDirty = true;
        }

        private Player GetPlayerToRemove()
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
                return rpw.Player;
            }

            return null;
        }

        private TeeTimeRequest FindTeeTimeRequest(Player player)
        {
            foreach (var request in TeeTimeRequests)
            {
                foreach (var p in request.Players)
                {
                    if (player == p)
                    {
                        return request;
                    }
                }
            }

            return null;
        }

        private int FindTeeTimeRequestIndex(Player player)
        {
            for (int i = 0; i < TournamentTeeTimes.Count; i++)
            {
                foreach (var p in TournamentTeeTimes[i].Players)
                {
                    if (player == p)
                    {
                        return i;
                    }
                }
            }

            return -1;
        }

        private void RemovePlayer(object o)
        {
            Player playerToRemove = GetPlayerToRemove();

            if (playerToRemove == null) return;

            MessageBoxResult result = MessageBox.Show("Are you sure you want to remove " + playerToRemove.Name + "?",
                                    "Confirmation",
                                    MessageBoxButton.YesNo,
                                    MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            _teeTimesDirty = true;

            // First look for the player in the unassigned tee time requests
            TeeTimeRequest ttr = FindTeeTimeRequest(playerToRemove);

            if (ttr != null)
            {
                if ((TournamentNames[TournamentNameIndex].TeamSize == 2) && 
                    (ttr.Players.Count == 4) &&
                    (ttr.Players.IndexOf(playerToRemove) < 2))
                {
                    // Move the first pair to the end
                    // before removing the player, so the remaining
                    // un-paired player is at the end of the list.
                    // This makes it easier to add a new partner to
                    // the un-paired player.
                    ttr.Players.Move(0, 3);
                    ttr.Players.Move(0, 3);
                }
                ttr.Players.Remove(playerToRemove);
                if (ttr.Players.Count == 0)
                {
                    TeeTimeRequests.Remove(ttr);
                }
                UpdateUnassignedList(_currentNumberOfPlayersShowing);
                CancelledPlayers.Add(playerToRemove);
                PlayersToRemoveFromSignup.Add(playerToRemove);
                return;
            }

            // If the player is not in the unassigned list, they must 
            // be part of a tee time.
            int teeTimeIndex = FindTeeTimeRequestIndex(playerToRemove);

            if (teeTimeIndex != -1)
            {
                if ((TournamentNames[TournamentNameIndex].TeamSize == 2) &&
                    (TournamentTeeTimes[teeTimeIndex].Players.Count == 4) &&
                    (TournamentTeeTimes[teeTimeIndex].Players.IndexOf(playerToRemove) < 2))
                {
                    // Move the first pair to the end
                    // before removing the player, so the remaining
                    // un-paired player is at the end of the list.
                    // This makes it easier to add a new partner to
                    // the un-paired player.
                    TournamentTeeTimes[teeTimeIndex].Players.Move(0, 3);
                    TournamentTeeTimes[teeTimeIndex].Players.Move(0, 3);
                }

                // Remove the player from the tee time itself
                TournamentTeeTimes[teeTimeIndex].RemovePlayer(playerToRemove);
                TeeTimeSelection = teeTimeIndex;

                // Tee time requests are moved to the assigned list when 
                // the request has been given a tee time. Remove the player
                // from the request on that list.
                ttr = null;
                foreach (var request in TeeTimeRequestsAssigned)
                {
                    foreach (var p in request.Players)
                    {
                        if (playerToRemove == p)
                        {
                            ttr = request;
                        }
                    }
                }

                if (ttr != null)
                {
                    ttr.RemovePlayer(playerToRemove);
                    if (ttr.Players.Count == 0)
                    {
                        TeeTimeRequestsAssigned.Remove(ttr);
                    }
                    CancelledPlayers.Add(playerToRemove);
                    PlayersToRemoveFromSignup.Add(playerToRemove);
                }
            }
        }

        private void ReplacePlayerCmd(object o)
        {
            Player playerToRemove = GetPlayerToRemove();

            if (playerToRemove == null) return;

            Player playerToAdd = GetPlayerToAdd(true);

            if (playerToAdd == null) return;

            MessageBoxResult result = MessageBox.Show("Is " + playerToAdd.Name + " to replace " + playerToRemove.Name + "?",
                                        "Confirmation",
                                        MessageBoxButton.YesNo,
                                        MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                // Add the removed player and new player to the replace list
                // to upload to the website
                ReplacePlayer rp = new ReplacePlayer();
                rp.Remove = playerToRemove;
                rp.Add = playerToAdd;
                PlayersToReplaceFromSignup.Add(rp);
                RemoveFromCancelledList(playerToAdd);
                CancelledPlayers.Add(playerToRemove);

                // If replacing with a player from the waiting list, there will be only
                // a single player in the tee time request
                TeeTimeRequest ttr = FindTeeTimeRequest(playerToAdd);
                if ((ttr != null) && (ttr.Players.Count == 1))
                {
                    TeeTimeRequests.Remove(ttr);
                    UpdateUnassignedList(4);
                }

                ttr = FindTeeTimeRequest(playerToRemove);

                // fix up the tee time request in the unassigned list
                if (ttr != null)
                {
                    ttr.ReplacePlayer(playerToRemove, playerToAdd);
                    return;
                }

                // If the player is not in the unassigned list, they must 
                // be part of a tee time.
                int teeTimeIndex = FindTeeTimeRequestIndex(playerToRemove);

                if (teeTimeIndex != -1)
                {
                    _teeTimesDirty = true;
                    TournamentTeeTimes[teeTimeIndex].ReplacePlayer(playerToRemove, playerToAdd);
                    playerToAdd.TeeTime = TournamentTeeTimes[teeTimeIndex];

                    // Fix up the tee time request in the assigned list
                    ttr = null;
                    foreach (var request in TeeTimeRequestsAssigned)
                    {
                        foreach (var p in request.Players)
                        {
                            if (playerToRemove == p)
                            {
                                ttr = request;
                            }
                        }
                    }

                    if (ttr != null)
                    {
                        ttr.ReplacePlayer(playerToRemove, playerToAdd);
                    }
                }
            }
        }

        private void ChangePartnersInTeeTime(object o)
        {
            if (TeeTimeSelection < 0)
            {
                MessageBox.Show("Select a tee time first",
                                          "Confirmation",
                                          MessageBoxButton.OK,
                                          MessageBoxImage.None);
                return;
            }
            if (TournamentTeeTimes[TeeTimeSelection].Players.Count < 3)
            {
                MessageBox.Show("There are " + TournamentTeeTimes[TeeTimeSelection].Players.Count + " players at " + 
                                TournamentTeeTimes[TeeTimeSelection].StartTime + " -- not enough players to swap partners",
                                          "Confirmation",
                                          MessageBoxButton.OK,
                                          MessageBoxImage.None);
                return;
            }

            MessageBoxResult result = MessageBox.Show("Make " + TournamentTeeTimes[TeeTimeSelection].Players[2].Name +
                                                        " the parnter of " + TournamentTeeTimes[TeeTimeSelection].Players[0].Name + "?",
                                          "Confirmation",
                                          MessageBoxButton.YesNo,
                                          MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                TournamentTeeTimes[TeeTimeSelection].Players.Move(2, 1);
                // Send property changed event to make UI update
                TournamentTeeTimes[TeeTimeSelection].PlayersChanged();
                _teeTimesDirty = true;
                return;
            }

            if (TournamentTeeTimes[TeeTimeSelection].Players.Count == 4)
            {
                result = MessageBox.Show("Make " + TournamentTeeTimes[TeeTimeSelection].Players[3].Name +
                                                        " the parnter of " + TournamentTeeTimes[TeeTimeSelection].Players[0].Name + "?",
                                          "Confirmation",
                                          MessageBoxButton.YesNo,
                                          MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    TournamentTeeTimes[TeeTimeSelection].Players.Move(3, 1);
                    // Send property changed event to make UI update
                    TournamentTeeTimes[TeeTimeSelection].PlayersChanged();
                    _teeTimesDirty = true;
                    return;
                }
            }

            MessageBox.Show("No change made",
                                          "Confirmation",
                                          MessageBoxButton.OK,
                                          MessageBoxImage.None);
        }

        //
        // Expected format of tee sheet file: header followed by individual lines.
        // 
        // Tee Time	Last Name	First Name	GHIN	Team Id	Email	Flight	OverEighty	Waitlisted
        // 6:15 AM,	Albitz,Paul,9079663,1,palbitz@san.rr.com,FALSE,FALSE


        private void LoadTeeTimesAndWaitlistCsv(object o)
        {
            if (!CheckContinue()) return;

            // Create OpenFileDialog 
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            // Set filter for file extension and default file extension 
            dlg.DefaultExt = ".csv";
            dlg.Filter = "CSV Files (*.csv)|*.csv";

            // Display OpenFileDialog by calling ShowDialog method 
            Nullable<bool> result = dlg.ShowDialog();

            // Get the selected file name and display in a TextBox 
            if (result != true)
            {
                return;
            }

            // TeeTimeFile could be made local to this file, since it
            // is not shown on the UI anymore.
            TeeTimeFile = dlg.FileName;

            if (string.IsNullOrEmpty(TeeTimeFile))
            {
                MessageBox.Show("Please fill in the tee sheet file");
                return;
            }

            if (!File.Exists(TeeTimeFile))
            {
                MessageBox.Show("File does not exist: " + TeeTimeFile);
                return;
            }

            _teeTimesDirty = false;

            AllowTeeTimeIntervalAdjust = false;

            using (TextReader tr = new StreamReader(TeeTimeFile))
            {
                ClearPlayersFromAllTeeTimes();
                //TournamentTeeTimes.Clear();

                TeeTimeRequests.Clear();
                TeeTimeRequestsUnassigned.Clear();
                TeeTimeRequestsAssigned.Clear();
                CancelledPlayers.Clear();
                PlayersToRemoveFromSignup.Clear();

                string[][] lines = CSVParser.Parse(tr);

                int teeTimeColumn = -1;
                int lastNameColumn = -1;
                int firstNameColumn = -1;
                int ghinColumn = -1;
                int emailColumn = -1;
                int flightColumn = -1;
                int statusColumn = -1;
                int extraColumn = -1;
                int teeColumn = -1;

                if (lines.Length == 0)
                {
                    throw new ApplicationException(TeeTimeFile + ": has 0 lines");
                }

                for (int col = 0; col < lines[0].Length; col++)
                {
                    if (string.Compare(lines[0][col], "start time", true) == 0)
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
                    else if ((string.Compare(lines[0][col], "status", true) == 0) || (string.Compare(lines[0][col], "tee status", true) == 0))
                    {
                        statusColumn = col;
                    }
                    else if (string.Compare(lines[0][col], "extra", true) == 0)
                    {
                        extraColumn = col;
                    }
                    else if (string.Compare(lines[0][col], "tee", true) == 0)
                    {
                        teeColumn = col;
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
                    throw new ApplicationException(TeeTimeFile + ": did not find header column: Tee Status");
                }
                // Allow csv to be missing "Tee" column

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
                        if (string.IsNullOrEmpty(line[teeTimeColumn]) && string.IsNullOrEmpty(line[statusColumn]) &&
                            string.IsNullOrEmpty(line[firstNameColumn]) && string.IsNullOrEmpty(line[lastNameColumn]))
                        {
                            // line must be empty
                            continue;
                        }

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
                            // Skip over any waitlisted/cancelled lines that might have been cut and pasted into the tee time list
                            if (teeTimeStatus != TeeTimeStatus.TeeTime)
                            {
                                continue;
                            }
                            throw new ApplicationException(TeeTimeFile + " (line " + lineIndex + "): Last Name is empty");
                        }
                        if (string.IsNullOrEmpty(line[firstNameColumn]))
                        {
                            // Skip over any waitlisted/cancelled lines that might have been cut and pasted into the tee time list
                            if (teeTimeStatus != TeeTimeStatus.TeeTime)
                            {
                                continue;
                            }
                            throw new ApplicationException(TeeTimeFile + " (line " + lineIndex + "): First Name is empty");
                        }
                        if (string.IsNullOrEmpty(line[ghinColumn]))
                        {
                            // Skip over any waitlisted/cancelled lines that might have been cut and pasted into the tee time list
                            if (teeTimeStatus != TeeTimeStatus.TeeTime)
                            {
                                continue;
                            }
                            throw new ApplicationException(TeeTimeFile + " (line " + lineIndex + "): GHIN is empty");
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
                        if (extraColumn != -1)
                        {
                            if ((line[extraColumn] == "M") || (line[extraColumn] == "G"))
                            {
                                player.Extra = line[extraColumn];
                            }
                        }
                        if (teeColumn != -1)
                        {
                            player.Tee = line[teeColumn];
                        }
                        if(string.IsNullOrEmpty(player.Tee))
                        {
                            player.Tee = "W";
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
            RecalculateBlindDrawOnSelection = false;
            SelectOpenTeeTime();
            FillInAssignedListFromTournamentTeeTimes();
            GroupMode = false;
            TeeTimeSource = "Tee times were loaded from file: " + Path.GetFileName(TeeTimeFile);
            WorkingOnSignups = false;
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
                if ((TournamentTeeTimes[i].Players.Count < 4) && !TournamentTeeTimes[i].BlockedOut)
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

        private async Task LoadHistoricalTeeTimesDataAsync(object o)
        {
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
                    values.Add(new KeyValuePair<string, string>("months",
                        MonthsOfTeeTimeDataToLoad.ToString(CultureInfo.InvariantCulture)));

                    var content = new FormUrlEncodedContent(values);

                    var teeTimesResponse = await client.PostAsync(WebAddresses.ScriptFolder + WebAddresses.GetHistoricalTeeTimeData, content);
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

                    LoadHistoricalTeeTimeDataFromWebResponseJson(responseString);
                }
            }

            if ((TeeTimeRequests.Count > 0) && (OrderTeeTimeRequestsBy == OrderTeeTimeRequestsByEnum.HistoricalTeeTimes))
            {
                SortTeeTimeRequests();
            }
            
        }

        protected void LoadHistoricalTeeTimeDataFromWebResponseJson(string webResponse)
        {
            PlayerTeeTimeHistoryByName = new List<PlayerTeeTimeHistory>();
            PlayerTeeTimeHistoryHashTableByGhin = new Hashtable();
            UnprocessedHistoricalTeeTimeData = null;

            var playerTeeTimeHistoryHashTableByName = new Hashtable();

            if (string.IsNullOrEmpty(webResponse))
            {
                return;
            }

            if (webResponse.StartsWith("JSON error:"))
            {
                throw new Exception(webResponse);
            }

            var jss = new JavaScriptSerializer();
            UnprocessedHistoricalTeeTimeData = jss.Deserialize<TournamentAndTeeTimes[]>(webResponse);

            if (UnprocessedHistoricalTeeTimeData == null)
            {
                return;
            }

            // Extract the tee time data and store it in a way to access by GHIN or name.
            for (int tournamentIndex = 0; tournamentIndex < UnprocessedHistoricalTeeTimeData.Length; tournamentIndex++)
            {
                // tournaments during COVID may not have tee times
                if (UnprocessedHistoricalTeeTimeData[tournamentIndex].TeeTimes == null)
                {
                    continue;
                }
                for (int teeTimeIndex = 0; teeTimeIndex < UnprocessedHistoricalTeeTimeData[tournamentIndex].TeeTimes.Length; teeTimeIndex++)
                {
                    for (int playerIndex = 0; playerIndex < UnprocessedHistoricalTeeTimeData[tournamentIndex].TeeTimes[teeTimeIndex].Players.Count; playerIndex++)
                    {
                        var ghin = UnprocessedHistoricalTeeTimeData[tournamentIndex].TeeTimes[teeTimeIndex].Players[playerIndex].GHIN;
                        var name = UnprocessedHistoricalTeeTimeData[tournamentIndex].TeeTimes[teeTimeIndex].Players[playerIndex].Name;
                        PlayerTeeTimeHistory ptth = null;

                        // skip over any ghin numbers that are 0 or non-numeric
                        if (int.TryParse(ghin, out int ghinInt) && (ghinInt != 0))
                        {
                            
                            if (!PlayerTeeTimeHistoryHashTableByGhin.ContainsKey(ghin))
                            {
                                ptth = new PlayerTeeTimeHistory();
                                ptth.Name = name;
                                ptth.GHIN = ghin;

                                // Create a nullable DateTime for each tournament
                                ptth.TeeTimes = new DateTime?[UnprocessedHistoricalTeeTimeData.Length];

                                // Add player to hash table
                                PlayerTeeTimeHistoryHashTableByGhin.Add(ghin, ptth);

                                if (playerTeeTimeHistoryHashTableByName.ContainsKey(name))
                                {
                                    // Hmmm. Player with 2 GHIN numbers. Let's hope the first one is the right one.
                                    Logging.Log("Tee Time History" , name + " has GHIN " + ghin + " and " + 
                                        ((PlayerTeeTimeHistory)playerTeeTimeHistoryHashTableByName[name]).GHIN);
                                }
                                else
                                {
                                    playerTeeTimeHistoryHashTableByName.Add(ptth.Name, ptth);
                                }

                                // Add to the list by name
                                PlayerTeeTimeHistoryByName.Add(ptth);
                            }
                            else
                            {
                                ptth = (PlayerTeeTimeHistory)PlayerTeeTimeHistoryHashTableByGhin[ghin];
                            }

                            DateTime startTime = DateTime.ParseExact(UnprocessedHistoricalTeeTimeData[tournamentIndex].TeeTimes[teeTimeIndex].StartTime,
                                "HH:mm:ss", CultureInfo.InvariantCulture);

                            ptth.TeeTimes[tournamentIndex] = UnprocessedHistoricalTeeTimeData[tournamentIndex].Tournament.StartDate + new TimeSpan(startTime.Hour, startTime.Minute, 0);

                        }
                        else
                        {
                            // Older tee times didn't have GHIN numbers. Try looking them up by name and use the name as the index if it exists.
                            if (playerTeeTimeHistoryHashTableByName.ContainsKey(name))
                            {
                                ptth = (PlayerTeeTimeHistory)playerTeeTimeHistoryHashTableByName[name];

                                DateTime startTime = DateTime.ParseExact(UnprocessedHistoricalTeeTimeData[tournamentIndex].TeeTimes[teeTimeIndex].StartTime,
                                "HH:mm:ss", CultureInfo.InvariantCulture);

                                ptth.TeeTimes[tournamentIndex] = UnprocessedHistoricalTeeTimeData[tournamentIndex].Tournament.StartDate + new TimeSpan(startTime.Hour, startTime.Minute, 0);
                            }
                        }
                    }
                }
            }
            PlayerTeeTimeHistoryByName.Sort();

            foreach (var ptth in PlayerTeeTimeHistoryByName)
            {
                double mean;
                double stdev;
                int count;
                string lastTeeTime;

                List<string> ghinNumbers = new List<string>();
                ghinNumbers.Add(ptth.GHIN);
                CalculateHistoricalTeeTimeMeanAndStdev(ghinNumbers, out mean, out stdev, out count, out lastTeeTime);
                ptth.StartTimeAverageInSeconds = mean;
                ptth.StartTimeStandardDeviationInSeconds = stdev;
                ptth.TeeTimeCount = count;
                ptth.LastTeeTime = lastTeeTime;
            }

            CalculateHistoricalTeeTimeMeanAndStdevForTeeTimeRequests();
        }

        private void CalculateHistoricalTeeTimeMeanAndStdevForTeeTimeRequests()
        {
            if (TeeTimeRequests != null)
            {
                foreach (var ttr in TeeTimeRequests)
                {
                    double mean;
                    double stdev;
                    int count;
                    string lastTeeTime;

                    List<string> ghinNumbers = new List<string>();
                    foreach (var player in ttr.Players)
                    {
                        ghinNumbers.Add(player.GHIN);
                        player.TeeTimeCount = GetTeeTimesPerGhin(player.GHIN);
                    }
                    CalculateHistoricalTeeTimeMeanAndStdev(ghinNumbers, out mean, out stdev, out count, out lastTeeTime);
                    ttr.StartTimeAverageInSeconds = mean;
                    ttr.StartTimeStandardDeviationInSeconds = stdev;
                    ttr.TeeTimeCount = count;
                    ttr.LastTeeTime = lastTeeTime;
                }
            }

            AdjustBlindDrawNumbersForInfrequentPlayers();
        }

        private int GetTeeTimesPerGhin(string ghin)
        {
            if (PlayerTeeTimeHistoryHashTableByGhin == null) return -1;

            int teeTimeCount = 0;

            if (PlayerTeeTimeHistoryHashTableByGhin.ContainsKey(ghin))
            {
                var ptth = (PlayerTeeTimeHistory)PlayerTeeTimeHistoryHashTableByGhin[ghin];

                for (int teeTimeIndex = 0; teeTimeIndex < ptth.TeeTimes.Length; teeTimeIndex++)
                {
                    if (ptth.TeeTimes[teeTimeIndex] != null)
                    {
                        teeTimeCount++;
                    }
                }
            }

            return teeTimeCount;
        }

        private void CalculateHistoricalTeeTimeMeanAndStdev(List<string> ghinNumbers, out double mean, out double stdev, out int teeTimeCount, out string lastTeeTime)
        {
            mean = 0;
            stdev = 0;
            teeTimeCount = 0;
            lastTeeTime = "00:00";

            if (PlayerTeeTimeHistoryHashTableByGhin == null) return;

            double totalSeconds = 0;

            foreach (var ghin in ghinNumbers)
            {
                if (PlayerTeeTimeHistoryHashTableByGhin.ContainsKey(ghin))
                {
                    var ptth = (PlayerTeeTimeHistory)PlayerTeeTimeHistoryHashTableByGhin[ghin];

                    for (int teeTimeIndex = 0; teeTimeIndex < ptth.TeeTimes.Length; teeTimeIndex++)
                    {
                        if (ptth.TeeTimes[teeTimeIndex] != null)
                        {
                            teeTimeCount++;
                            TimeSpan ts = new TimeSpan(ptth.TeeTimes[teeTimeIndex].Value.Hour, ptth.TeeTimes[teeTimeIndex].Value.Minute, ptth.TeeTimes[teeTimeIndex].Value.Second);
                            totalSeconds += ts.TotalSeconds;

                            if (string.Compare(lastTeeTime, "00:00") == 0)
                            {
                                lastTeeTime = ptth.TeeTimes[teeTimeIndex].Value.ToShortTimeString();
                            }
                        }
                    }
                }
            }

            if (teeTimeCount > 0)
            {
                mean = totalSeconds / teeTimeCount;

                double sumOfSquares = 0;
                foreach (var ghin in ghinNumbers)
                {
                    if (PlayerTeeTimeHistoryHashTableByGhin.ContainsKey(ghin))
                    {
                        var ptth = (PlayerTeeTimeHistory)PlayerTeeTimeHistoryHashTableByGhin[ghin];

                        for (int teeTimeIndex = 0; teeTimeIndex < ptth.TeeTimes.Length; teeTimeIndex++)
                        {
                            if (ptth.TeeTimes[teeTimeIndex] != null)
                            {
                                TimeSpan ts = new TimeSpan(ptth.TeeTimes[teeTimeIndex].Value.Hour, ptth.TeeTimes[teeTimeIndex].Value.Minute, ptth.TeeTimes[teeTimeIndex].Value.Second);
                                double totalSecondsMinusMean = ts.TotalSeconds - mean;
                                sumOfSquares += totalSecondsMinusMean * totalSecondsMinusMean;
                            }
                        }
                    }
                }

                stdev = Math.Sqrt(sumOfSquares / teeTimeCount);
            }
        }

        private void SaveTeeTimeHistoryAsCsv(object o)
        {
            // Configure save file dialog box
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = "Tee Time History - Last 12 Months";
            dlg.DefaultExt = ".csv"; // Default file extension
            dlg.Filter = "CSV File (.csv)|*.csv"; // Filter files by extension

            // Show save file dialog box
            bool? result = dlg.ShowDialog();

            if (result != true)
            {
                return;
            }

            using (TextWriter tw = new StreamWriter(dlg.FileName))
            {
                tw.Write("Name,GHIN,Avg Start Time,Stdev,Last Tee Time,Tee Time Count");
                for (int tournamentIndex = 0; tournamentIndex < UnprocessedHistoricalTeeTimeData.Length; tournamentIndex++)
                {
                    tw.Write("," + UnprocessedHistoricalTeeTimeData[tournamentIndex].Tournament.StartDate.ToShortDateString());
                }
                tw.WriteLine();

                foreach (var ptth in PlayerTeeTimeHistoryByName)
                {
                    tw.Write("\"" + ptth.Name + "\"," + ptth.GHIN);

                    TimeSpan time = TimeSpan.FromSeconds(ptth.StartTimeAverageInSeconds);
                    tw.Write("," + time.ToString(@"hh\:mm"));
                    time = TimeSpan.FromSeconds(ptth.StartTimeStandardDeviationInSeconds);
                    tw.Write("," + time.ToString(@"hh\:mm"));
                    tw.Write("," + ptth.LastTeeTime);
                    tw.Write("," + ptth.TeeTimeCount);

                    for (int teeTimeIndex = 0; teeTimeIndex < ptth.TeeTimes.Length; teeTimeIndex++)
                    {
                        if (ptth.TeeTimes[teeTimeIndex] == null)
                        {
                            tw.Write(",");
                        }
                        else
                        {
                            tw.Write("," + ptth.TeeTimes[teeTimeIndex].Value.ToShortTimeString());
                        }
                    }
                    tw.WriteLine();
                }
            }

            /* The tee time request file has not been very useful, so disable it
            
            if ((TeeTimeRequests != null) && (TeeTimeRequests.Count > 0))
            {
                dlg.FileName = "Tee Time Request History - " + TournamentNames[TournamentNameIndex].Name.Replace('/', '-');

                // Show save file dialog box
                result = dlg.ShowDialog();

                if (result != true)
                {
                    return;
                }

                using (TextWriter tw = new StreamWriter(dlg.FileName))
                {
                    tw.WriteLine("Names,Avg Start Time,Stdev,Tee Time Count,Last Tee Time");

                    foreach (var ttr in TeeTimeRequestsAssigned)
                    {
                        tw.Write("\"" + ttr.PlayerList + "\"");
                        TimeSpan time = TimeSpan.FromSeconds(ttr.StartTimeAverageInSeconds);
                        tw.Write("," + time.ToString(@"hh\:mm"));
                        time = TimeSpan.FromSeconds(ttr.StartTimeStandardDeviationInSeconds);
                        tw.Write("," + time.ToString(@"hh\:mm"));
                        tw.Write("," + ttr.TeeTimeCount);
                        tw.Write("," + ttr.LastTeeTime);
                        tw.WriteLine();
                    }

                    foreach (var ttr in TeeTimeRequests)
                    {
                        tw.Write("\"" + ttr.PlayerList + "\"");
                        TimeSpan time = TimeSpan.FromSeconds(ttr.StartTimeAverageInSeconds);
                        tw.Write("," + time.ToString(@"hh\:mm"));
                        time = TimeSpan.FromSeconds(ttr.StartTimeStandardDeviationInSeconds);
                        tw.Write("," + time.ToString(@"hh\:mm"));
                        tw.Write("," + ttr.TeeTimeCount);
                        tw.Write("," + ttr.LastTeeTime);
                        tw.WriteLine();
                    }
                }
            }
            */
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
