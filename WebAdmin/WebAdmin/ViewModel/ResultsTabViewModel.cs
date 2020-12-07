using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Security.RightsManagement;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Net.Http;
using System.IO;
using WebAdmin.View;

namespace WebAdmin.ViewModel
{
    public class ResultsTabViewModel : TabViewModelBase, ITabViewModel
    {
        #region Properties
        public override string Header { get { return "Results"; } }
        #endregion

        private const string ResultsPool = "ResultsPool";
        private const string ResultsChits = "ResultsChits";
        private const string FlightNameTable = "FlightNames";
        private const string MatchPlayResultsScores = "MatchPlayResultsScores";
        private const string GolfGeniusResultsLink = "GolfGeniusResultsLink";

        private Visibility _getTournamentsVisible;
        public Visibility GetTournamentsVisible { get { return _getTournamentsVisible; } set { _getTournamentsVisible = value; OnPropertyChanged(); } }

        private Visibility _gotTournamentsVisible;
        public Visibility GotTournamentsVisible { get { return _gotTournamentsVisible; } set { _gotTournamentsVisible = value; OnPropertyChanged(); } }

        private bool _updateTournamentAllowed;
        public bool UpdateTournamentsAllowed { get { return _updateTournamentAllowed; } set { _updateTournamentAllowed = value; OnPropertyChanged(); } }

        private Visibility _is2DayTournament;
        public Visibility Is2DayTournament { get { return _is2DayTournament; } set { _is2DayTournament = value; OnPropertyChanged(); } }

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
                    if (value >= 0)
                    {
                        UpdateTournamentsAllowed = true;
                        Is2DayTournament = (TournamentNames[value].StartDate != TournamentNames[value].EndDate) 
                                                    ? Visibility.Visible : Visibility.Collapsed;
                        IsEclectic = TournamentNames[value].IsEclectic;
                        MatchPlay = TournamentNames[value].MatchPlay;
                        IsStableford = TournamentNames[value].IsStableford;
                        LoadClosestToThePinFromWeb();
                    }
                    else
                    {
                        UpdateTournamentsAllowed = false;
                        Is2DayTournament = Visibility.Collapsed;
                    }
                    ResetFileNames();
                }
            }
        }

        private TrulyObservableCollection<TournamentName> _tournamentNames;
        public TrulyObservableCollection<TournamentName> TournamentNames
        {
            get { return _tournamentNames; }
            set { _tournamentNames = value; OnPropertyChanged(); }
        }

        private TrulyObservableCollection<ClosestToThePin> _closestToThePinsDay1;
        public TrulyObservableCollection<ClosestToThePin> ClosestToThePinsDay1
        {
            get { return _closestToThePinsDay1; }
            set { _closestToThePinsDay1 = value; OnPropertyChanged(); }
        }

        private TrulyObservableCollection<ClosestToThePin> _closestToThePinsDay2;
        public TrulyObservableCollection<ClosestToThePin> ClosestToThePinsDay2
        {
            get { return _closestToThePinsDay2; }
            set { _closestToThePinsDay2 = value; OnPropertyChanged(); }
        }

        private string _ggTournamentResultsCsvFileName;
        public string GgTournamentResultsCsvFileName {
            get { return _ggTournamentResultsCsvFileName; }
            set
            {
                _ggTournamentResultsCsvFileName = value;
                //AddScoresEntriesGg(value);
                OnPropertyChanged();
            }
        }

        private string _csvFolderName;
        public string CSVFolderName {  get { return _csvFolderName;} set { _csvFolderName = value; OnPropertyChanged();} }

        private string _csvScoresFileName;
        public string CsvScoresFileName { get { return _csvScoresFileName; } set { _csvScoresFileName = value; OnPropertyChanged(); } }

        private string _csvChitsFileName;
        public string CsvChitsFileName { 
            get { return _csvChitsFileName; } 
            set 
            { 
                _csvChitsFileName = value;
                UpdateChitsWinnings();
                OnPropertyChanged(); 
            } 
        }

        private string _ggTournamentResultsLink;
        public string GgTournamentResultsLink
        {
            get { return _ggTournamentResultsLink; }
            set
            {
                _ggTournamentResultsLink = value;
                OnPropertyChanged();
            }
        }

        //private List<KeyValuePair<string, string>>[] _csvDay1PoolKvp;

        //private ObservableCollection<string> _csvDay1PoolFileName;
        //public ObservableCollection<string> CsvDay1PoolFileName
        //{
        //    get { return _csvDay1PoolFileName; }
        //    set { _csvDay1PoolFileName = value; OnPropertyChanged(); }
        //}

        //private List<KeyValuePair<string, string>>[] _csvDay2PoolKvp;

        //private ObservableCollection<string> _csvDay2PoolFileName;
        //public ObservableCollection<string> CsvDay2PoolFileName
        //{
        //    get { return _csvDay2PoolFileName; }
        //    set { _csvDay2PoolFileName = value; OnPropertyChanged(); }
        //}

        //private ObservableCollection<string> _csvDay1PoolTotal;
        //public ObservableCollection<string> CsvDay1PoolTotal
        //{
        //    get { return _csvDay1PoolTotal; }
        //    set { _csvDay1PoolTotal = value; OnPropertyChanged(); }
        //}

        //private ObservableCollection<string> _csvDay2PoolTotal;
        //public ObservableCollection<string> CsvDay2PoolTotal
        //{
        //    get { return _csvDay2PoolTotal; }
        //    set { _csvDay2PoolTotal = value; OnPropertyChanged(); }
        //}

        private List<KeyValuePair<string, string>> _kvpChitsList;
        private List<List<KeyValuePair<string, string>>> _kvpScoresList;

        private string _chitsTotal;
        public string ChitsTotal { get { return _chitsTotal; } set { _chitsTotal = value; OnPropertyChanged(); } }

        private string _chitsFlights;
        public string ChitsFlights { get { return _chitsFlights; } set { _chitsFlights = value; OnPropertyChanged(); }}

        private ObservableCollection<EventWinnings> _eventWinningsList;
        public ObservableCollection<EventWinnings> EventWinningsList { get { return _eventWinningsList; } set { _eventWinningsList = value; OnPropertyChanged(); } }

        private bool _isEclectic;
        public bool IsEclectic { get { return _isEclectic; } set { _isEclectic = value; OnPropertyChanged(); } }

        private bool _isStableford;
        public bool IsStableford { get { return _isStableford; } set { _isStableford = value; OnPropertyChanged(); } }

        private bool _matchPlay;
        public bool MatchPlay { get { return _matchPlay; } set { _matchPlay = value; OnPropertyChanged(); } }

        #region Commands
        public ICommand GetTournamentsCommand { get { return new ModelCommand(async s => await GetTournaments(s)); } }
        public ICommand SubmitClosestToThePinCommand { get { return new ModelCommand(async s => await SubmitClosestToThePin(s)); } }
        public ICommand ClearClosestToThePinCommand { get { return new ModelCommand(async s => await ClearClosestToThePin(s)); } }

        public ICommand SubmitCsvCommand { get { return new ModelCommand(async s => await SubmitCsv(s)); } }
        public ICommand ClearCsvCommand { get { return new ModelCommand(async s => await ClearCsv(s)); } }

        public ICommand SubmitGgResultsLinkCommand { get { return new ModelCommand(async s => await SubmitGgResultsLink(s)); } }
        public ICommand ClearGgResultsLinkCommand { get { return new ModelCommand(async s => await ClearGgResultsLink(s)); } }

        //public ICommand CSVDay1PoolAdjustCommand { get { return new ModelCommand(s => CSVDay1PoolAdjust(s)); } }
        //public ICommand CSVDay2PoolAdjustCommand { get { return new ModelCommand(s => CSVDay2PoolAdjust(s)); } }
        public ICommand ChitsAdjustCommand { get { return new ModelCommand(s => ChitsAdjust(s)); } }
        #endregion

        public ResultsTabViewModel()
        {
            TournamentNames = new TrulyObservableCollection<TournamentName>();
            GetTournamentsVisible = Visibility.Visible;
            GotTournamentsVisible = Visibility.Collapsed;
            Is2DayTournament = Visibility.Collapsed;
            TournamentNameIndex = -1;

            CreateEmptyClosestToThePin();
            //CsvDay1PoolFileName = new ObservableCollection<string>();
            //CsvDay2PoolFileName = new ObservableCollection<string>();
            //CsvDay1PoolTotal = new ObservableCollection<string>();
            //CsvDay2PoolTotal = new ObservableCollection<string>();

            //for(int i = 0; i < 4; i++)
            //{
            //    CsvDay1PoolFileName.Add(string.Empty);
            //    CsvDay2PoolFileName.Add(string.Empty);

            //    CsvDay1PoolTotal.Add("$0 Day 1");
            //    CsvDay2PoolTotal.Add("$0 Day 2");
            //}
            ChitsTotal = "$0 Total chits";
            ChitsFlights = string.Empty;

            //_csvDay1PoolKvp = new List<KeyValuePair<string, string>>[4];
            //_csvDay2PoolKvp = new List<KeyValuePair<string, string>>[4];

            //CsvDay1PoolFileName.CollectionChanged += CsvDay1PoolFileName_CollectionChanged;
            //CsvDay2PoolFileName.CollectionChanged += CsvDay2PoolFileName_CollectionChanged;
        }

        private void ResetFileNames()
        {
            CSVFolderName = string.Empty;
            CsvScoresFileName = string.Empty;
            CsvChitsFileName = string.Empty;

            //if (CsvDay1PoolFileName != null)
            //{
            //    for (int i = 0; i < CsvDay1PoolFileName.Count; i++)
            //    {
            //        CsvDay1PoolFileName[i] = string.Empty;
            //        CsvDay2PoolFileName[i] = string.Empty;
            //    }
            //}
        }

        //void CsvDay1PoolFileName_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        //{
        //    int fileIndex = e.NewStartingIndex;
        //    _csvDay1PoolKvp[fileIndex] = new List<KeyValuePair<string, string>>();

        //    if (!string.IsNullOrEmpty(CsvDay1PoolFileName[fileIndex]))
        //    {
        //        AddPoolEntries(CsvDay1PoolFileName[fileIndex], fileIndex + 1, 0, _csvDay1PoolKvp[fileIndex]);
        //        LoadAdjustments(CsvDay1PoolFileName[fileIndex], _csvDay1PoolKvp[fileIndex]);
        //    }
        //    CsvDay1PoolTotal[fileIndex] = "$" + GetWinningsTotal(_csvDay1PoolKvp[fileIndex], ResultsPool).ToString("F0") + " Day 1";
        //}

        //void CsvDay2PoolFileName_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        //{
        //    int fileIndex = e.NewStartingIndex;
        //    _csvDay2PoolKvp[fileIndex] = new List<KeyValuePair<string, string>>();

        //    if (!string.IsNullOrEmpty(CsvDay2PoolFileName[fileIndex]))
        //    {
        //        AddPoolEntries(CsvDay2PoolFileName[fileIndex], fileIndex + 1, 1, _csvDay2PoolKvp[fileIndex]);
        //        LoadAdjustments(CsvDay2PoolFileName[fileIndex], _csvDay2PoolKvp[fileIndex]);
        //    }
        //    CsvDay2PoolTotal[fileIndex] = "$" + GetWinningsTotal(_csvDay2PoolKvp[fileIndex], ResultsPool).ToString("F0") + " Day 2";
        //}

        private void UpdateChitsWinnings()
        {
            _kvpChitsList = new List<KeyValuePair<string, string>>();

            if (!string.IsNullOrEmpty(CsvChitsFileName))
            {
                AddChitsEntries(CsvChitsFileName, _kvpChitsList);
                LoadAdjustments(CsvChitsFileName, _kvpChitsList);
            }

            ChitsTotal = "$" + GetWinningsTotal(_kvpChitsList, ResultsChits).ToString("F0") + " Total chits";
            ChitsFlights = GetWinningsFlights(_kvpChitsList, ResultsChits);
        }

        private float GetWinningsTotal(List<KeyValuePair<string, string>> kvpPoolList, string eventName)
        {
            float winnings = 0;
            bool indexFound = true;
            for (int index = 0; (index < kvpPoolList.Count) && indexFound; index++)
            {
                string key = string.Format("{0}[{1}][Winnings]", eventName, index);

                indexFound = false;
                foreach (var kvp in kvpPoolList)
                {
                    if (kvp.Key == key)
                    {
                        winnings += float.Parse(kvp.Value);
                        indexFound = true;
                        break;
                    }
                }
            }

            return winnings;
        }

        private string GetWinningsFlights(List<KeyValuePair<string, string>> kvpPoolList, string eventName)
        {
            Dictionary<int, string> indexToFlight = new Dictionary<int, string>();
            List<string> flightNames = new List<string>();
            List<int> winnings = new List<int>();
            string flightWinnings = "Flight chits: ";

            Regex exprFlightName = new Regex(eventName + @"\[(\d+)\]\[FlightName\]");

            for (int index = 0; index < kvpPoolList.Count; index++)
            {
                Match m = exprFlightName.Match(kvpPoolList[index].Key);
                if (m.Success)
                {
                    indexToFlight[int.Parse(m.Groups[1].Value)] = kvpPoolList[index].Value;

                    if (!flightNames.Contains(kvpPoolList[index].Value))
                    {
                        flightNames.Add(kvpPoolList[index].Value);
                        winnings.Add(0);
                    }
                }
            }

            Regex exprWinnings = new Regex(eventName + @"\[(\d+)\]\[Winnings\]");

            for (int index = 0; index < kvpPoolList.Count; index++)
            {
                Match m = exprWinnings.Match(kvpPoolList[index].Key);
                if (m.Success)
                {
                    try
                    {
                        int i = int.Parse(m.Groups[1].Value);
                        int i2 = flightNames.IndexOf(indexToFlight[i]);
                        winnings[i2] += int.Parse(kvpPoolList[index].Value);
                    }
                    catch (Exception)
                    {
                        throw new ApplicationException("Failed to collect flight winnings for line: " + kvpPoolList[index].Key);
                    }

                }
            }

            for (int index = 0; index < winnings.Count; index++)
            {
                flightWinnings += (index > 0) ? "/" : string.Empty;
                flightWinnings += "$" + winnings[index];
            }

            return flightWinnings;
        }


        private void CreateEmptyClosestToThePin()
        {
            ClosestToThePinsDay1 = new TrulyObservableCollection<ClosestToThePin>();
            ClosestToThePinsDay1.Add(new ClosestToThePin { Hole = 5 });
            ClosestToThePinsDay1.Add(new ClosestToThePin { Hole = 9 });
            ClosestToThePinsDay1.Add(new ClosestToThePin { Hole = 11 });
            ClosestToThePinsDay1.Add(new ClosestToThePin { Hole = 15 });

            ClosestToThePinsDay2 = new TrulyObservableCollection<ClosestToThePin>();
            ClosestToThePinsDay2.Add(new ClosestToThePin { Hole = 5 });
            ClosestToThePinsDay2.Add(new ClosestToThePin { Hole = 9 });
            ClosestToThePinsDay2.Add(new ClosestToThePin { Hole = 11 });
            ClosestToThePinsDay2.Add(new ClosestToThePin { Hole = 15 });
        }

        private async Task GetTournaments(object o)
        {
            string responseString = await GetTournamentNames();

            LoadTournamentNamesFromWebResponse(responseString, TournamentNames, false);
        }

        protected override void OnTournamentsUpdated()
        {
            TournamentNameIndex = -1;

            // Grab the last tournament that has started
            for (int i = TournamentNames.Count - 1; i >= 0; i--)
            {
                if ((DateTime.Now >= TournamentNames[i].StartDate) && !TournamentNames[i].AnnouncementOnly)
                {
                    TournamentNameIndex = i;
                    break;
                }
            }

            if (TournamentNames.Count > 0)
            {
                GetTournamentsVisible = Visibility.Collapsed;
                GotTournamentsVisible = Visibility.Visible;
            }
        }

        private async Task LoadClosestToThePinFromWeb()
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(WebAddresses.BaseAddress);

                var values = new List<KeyValuePair<string, string>>();

                values.Add(new KeyValuePair<string, string>("TournamentKey",
                    TournamentNames[TournamentNameIndex].TournamentKey.ToString()));

                var content = new FormUrlEncodedContent(values);

                string responseString = string.Empty;
                using (new WaitCursor())
                {
                    var response = await client.PostAsync(WebAddresses.ScriptFolder + WebAddresses.GetClosestToThePin, content);
                    responseString = await response.Content.ReadAsStringAsync();
                }

                Logging.Log("LoadClosestToThePinFromWeb", responseString);

                LoadSignupsFromWebResponse(responseString);
            }
        }

        public void LoadSignupsFromWebResponse(string webResponse)
        {

            string[] lines = webResponse.Split('\n');
            string[][] csvParsedLines = CSVParser.Parse(lines);

            CreateEmptyClosestToThePin();

            int lineNumber = 0;
            foreach (var fields in csvParsedLines)
            {
                lineNumber++;
                if ((fields.Length == 0 ) || string.IsNullOrWhiteSpace(fields[0])) continue;

                ClosestToThePin ctp = new ClosestToThePin();

                if (fields.Length < 7)
                {
                    throw new ArgumentException(string.Format("Website response: line {0}: contains fewer than 7 fields: {1}", lineNumber, lines[lineNumber - 1]));
                }

                int hole;
                if (!int.TryParse(fields[0], out hole))
                {
                    throw new ArgumentException(string.Format("Website response: line {0}: bad hole number: {1}", lineNumber, lines[lineNumber - 1]));
                }
                ctp.Hole = hole;

                DateTime dt;
                if (!DateTime.TryParse(fields[1], out dt))
                {
                    throw new ArgumentException(string.Format("Website response: line {0}: bad date: {1}", lineNumber, lines[lineNumber - 1]));
                }
                ctp.Date = dt;

                int ghin;
                if (!int.TryParse(fields[2], out ghin))
                {
                    throw new ArgumentException(string.Format("Website response: line {0}: bad ghin number: {1}", lineNumber, lines[lineNumber - 1]));
                }
                ctp.GHIN = ghin;

                ctp.Player = fields[3];
                ctp.Distance = fields[4];
                ctp.Prize = fields[5];
                ctp.Business = fields[6];

                if (TournamentNames[TournamentNameIndex].StartDate == ctp.Date)
                {
                    for (int i = 0; i < ClosestToThePinsDay1.Count; i++ )
                    {
                        if(ClosestToThePinsDay1[i].Hole == ctp.Hole)
                        {
                            ClosestToThePinsDay1[i] = ctp;
                            break;
                        }
                    }
                }
                else if (TournamentNames[TournamentNameIndex].EndDate == ctp.Date)
                {
                    for (int i = 0; i < ClosestToThePinsDay2.Count; i++)
                    {
                        if (ClosestToThePinsDay2[i].Hole == ctp.Hole)
                        {
                            ClosestToThePinsDay2[i] = ctp;
                            break;
                        }
                    }
                }
                else
                {
                    throw new ArgumentException(string.Format("Website response: line {0}: date does not match tournament start date or end date: {1}", lineNumber, lines[lineNumber - 1]));
                }
            }
        }

        private async Task SubmitClosestToThePin(object o)
        {
            if (TournamentNameIndex == -1)
            {
                MessageBox.Show("Please select a tournament first");
                return;
            }

            List<ClosestToThePin> ctpList = new List<ClosestToThePin>();
            foreach(var ctp in _closestToThePinsDay1)
            {
                ctp.Date = TournamentNames[TournamentNameIndex].StartDate;
            }
            ctpList.AddRange(ClosestToThePinsDay1);
            foreach(var ctp in _closestToThePinsDay2)
            {
                ctp.Date = TournamentNames[TournamentNameIndex].StartDate.AddDays(1);
            }
            ctpList.AddRange(ClosestToThePinsDay2);

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

                values.Add(new KeyValuePair<string, string>("TournamentKey", 
                    TournamentNames[TournamentNameIndex].TournamentKey.ToString()));

                bool nothingSubmitted = true;
                bool firstChunk = true;
                int chunkIndex = 0;
                for (int i = 0; i < ctpList.Count; i++)
                {
                    if (firstChunk)
                    {
                        firstChunk = false;
                        values.Add(new KeyValuePair<string, string>("ClearClosestToPin", "1"));
                    }

                    if (string.IsNullOrEmpty(ctpList[i].Player))
                    {
                        continue;
                    }

                    values.Add(new KeyValuePair<string, string>(
                        string.Format("CTP[{0}][Hole]", chunkIndex),
                        ctpList[i].Hole.ToString()));

                    values.Add(new KeyValuePair<string, string>(
                        string.Format("CTP[{0}][Date]", chunkIndex),
                        ctpList[i].Date.ToString("yyyy-MM-dd")));

                    values.Add(new KeyValuePair<string, string>(
                        string.Format("CTP[{0}][Name]", chunkIndex),
                         ctpList[i].Player));

                    values.Add(new KeyValuePair<string, string>(
                        string.Format("CTP[{0}][GHIN]", chunkIndex),
                         ctpList[i].GHIN.ToString()));

                    values.Add(new KeyValuePair<string, string>(
                        string.Format("CTP[{0}][Distance]", chunkIndex),
                         ctpList[i].Distance));

                    values.Add(new KeyValuePair<string, string>(
                        string.Format("CTP[{0}][Prize]", chunkIndex),
                         ctpList[i].Prize));

                    values.Add(new KeyValuePair<string, string>(
                        string.Format("CTP[{0}][Business]", chunkIndex),
                         ctpList[i].Business));

                    chunkIndex++;
                    // If too much data is sent at once, you get an error 503
                    if (values.Count >= 500)
                    {
                        bool sent = await HttpSend(client, HtmlRequestType.Post, values, WebAddresses.ScriptFolder + WebAddresses.SubmitClosestToThePin);
                        chunkIndex = 0;
                        nothingSubmitted = false;

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
                    bool sent = await HttpSend(client, HtmlRequestType.Post, values, WebAddresses.ScriptFolder + WebAddresses.SubmitClosestToThePin);
                    nothingSubmitted = false;

                    if (!sent)
                    {
                        return;
                    }
                }

                if (nothingSubmitted)
                {
                    MessageBox.Show("Everything is empty.  Nothing was submitted.");
                }
                else
                {
                    MessageBox.Show("Closest to the pin updated");
                }

            }
        }

        private async Task ClearClosestToThePin(object o)
        {
            if (TournamentNameIndex == -1)
            {
                MessageBox.Show("Please select a tournament first");
                return;
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

                values.Add(new KeyValuePair<string, string>("TournamentKey",
                    TournamentNames[TournamentNameIndex].TournamentKey.ToString()));

                values.Add(new KeyValuePair<string, string>("ClearClosestToPin", "1"));

                bool sent = await HttpSend(client, HtmlRequestType.Post, values, WebAddresses.ScriptFolder + WebAddresses.SubmitClosestToThePin);

                if (!sent)
                {
                    return;
                }

                CreateEmptyClosestToThePin();

                System.Windows.MessageBox.Show("Closest to the pin cleared");

            }
        }

        private async Task<bool> ClearResults(string result)
        {
            // cancelled password input
            if (string.IsNullOrEmpty(Credentials.LoginPassword))
            {
                return false;
            }

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(WebAddresses.BaseAddress);

                using (var multipartFormDataContent = new MultipartFormDataContent())
                {
                    var values = new[]
                            {
                                new KeyValuePair<string, string>("Login", Credentials.LoginName),
                                new KeyValuePair<string, string>("Password", Credentials.LoginPassword),
                                new KeyValuePair<string, string>("TournamentKey", TournamentNames[TournamentNameIndex].TournamentKey.ToString()),
                                new KeyValuePair<string, string>("Action", "Clear"),
                                new KeyValuePair<string, string>("Result", result)
                            };

                    foreach (var keyValuePair in values)
                    {
                        multipartFormDataContent.Add(new StringContent(keyValuePair.Value),
                            String.Format("\"{0}\"", keyValuePair.Key));
                    }

                    var requestUri = WebAddresses.ScriptFolder + WebAddresses.SubmitResultsCsv;

                    string responseString;
                    using (new WaitCursor())
                    {
                        try
                        {
                            Status.Message = "Clearing " + result + " results ...";
                            var response = client.PostAsync(requestUri, multipartFormDataContent).Result;
                            responseString = await response.Content.ReadAsStringAsync();
                            System.Diagnostics.Debug.WriteLine(responseString);
                        }
                        finally
                        {
                            Status.Message = string.Empty;
                        }
                    }

                    if (responseString.StartsWith("Success", StringComparison.InvariantCultureIgnoreCase))
                    {
                        return true;
                    }
                    else
                    {
                        Credentials.CheckForInvalidPassword(responseString);
                        Logging.Log(requestUri, responseString);

                        HtmlDisplayWindow displayWindow = new HtmlDisplayWindow();
                        displayWindow.WebBrowser.NavigateToString(responseString);
                        displayWindow.Owner = App.Current.MainWindow;
                        displayWindow.ShowDialog();
                    }
                }
            }
            return false;
        }

        private async Task<bool> SubmitResultsCsv(List<KeyValuePair<string, string>> kvpList, string result, bool clearResult)
        {
            //System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            //sw.Start();

            // cancelled password input
            if (string.IsNullOrEmpty(Credentials.LoginPassword))
            {
                return false;
            }

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(WebAddresses.BaseAddress);

                using (var multipartFormDataContent = new MultipartFormDataContent())
                {
                    var values = new[]
                            {
                                new KeyValuePair<string, string>("Login", Credentials.LoginName),
                                new KeyValuePair<string, string>("Password", Credentials.LoginPassword),
                                new KeyValuePair<string, string>("TournamentKey", TournamentNames[TournamentNameIndex].TournamentKey.ToString()),
                                new KeyValuePair<string, string>("Action", "Submit")
                            };

                    

                    foreach (var keyValuePair in values)
                    {
                        multipartFormDataContent.Add(new StringContent(keyValuePair.Value),
                            String.Format("\"{0}\"", keyValuePair.Key));
                    }

                    if (clearResult)
                    {
                        multipartFormDataContent.Add(new StringContent(result),
                            String.Format("\"{0}\"", "Clear"));
                    }

                    foreach(var keyValuePair in kvpList)
                    {
                        multipartFormDataContent.Add(new StringContent(keyValuePair.Value),
                            String.Format("\"{0}\"", keyValuePair.Key));
                    }

                    var requestUri = WebAddresses.ScriptFolder + WebAddresses.SubmitResultsCsv;

                    string responseString;
                    using (new WaitCursor())
                    {
                        try
                        {
                            Status.Message = "Submitting " + result + " results ...";
                            var response = client.PostAsync(requestUri, multipartFormDataContent).Result;
                            responseString = await response.Content.ReadAsStringAsync();
                            System.Diagnostics.Debug.WriteLine(responseString);
                        }
                        finally
                        {
                            Status.Message = string.Empty;
                        }
                    }

                    if (responseString.StartsWith("Success", StringComparison.InvariantCultureIgnoreCase))
                    {
                        //sw.Stop();
                        //System.Diagnostics.Debug.WriteLine("Submit took " + sw.ElapsedMilliseconds + "ms");
                        return true;
                    }

                    Credentials.CheckForInvalidPassword(responseString);
                    Logging.Log(requestUri, responseString);

                    HtmlDisplayWindow displayWindow = new HtmlDisplayWindow();
                    displayWindow.WebBrowser.NavigateToString(responseString);
                    displayWindow.Owner = App.Current.MainWindow;
                    displayWindow.ShowDialog();
                }
            }
            return false;
        }

        private int GetFlight(string fileName)
        {
            string[] fields = System.IO.Path.GetFileNameWithoutExtension(fileName).Split(' ');
            for (int i = 0; i < fields.Length; i++)
            {
                if (fields[i].ToLower() == "flight")
                {
                    int flight;
                    if (((i + 1) < fields.Length) && int.TryParse(fields[i + 1], out flight))
                    {
                        return flight;
                    }
                    else
                    {
                        return 1;
                    }
                }
            }

            throw new ApplicationException("File name does not contain 'flight': " + fileName);
        }

        //private void AddPoolEntries(string file, int flight, int day, List<KeyValuePair<string, string>> kvpList)
        //{
        //    string fullPath = Path.Combine(CSVFolderName, file);
        //    if(!File.Exists(fullPath))
        //    {
        //        throw new ApplicationException("File doesn't exist: " + fullPath);
        //    }

        //    int index = 0;
        //    foreach(var kvp in kvpList)
        //    {
        //        string key = string.Format("{0}[{1}][Flight]", ResultsPool, index);
        //        if(kvp.Key == key)
        //        {
        //            index++;
        //        }
        //    }
        //    using(TextReader tr = new StreamReader(fullPath))
        //    {
        //        string[][] lines = CSVParser.Parse(tr);
        //        foreach(var line in lines)
        //        {
        //            if (line.Length > 0)
        //            {
        //                if ((line.Length != 6) && (line.Length != 15))
        //                {
        //                    throw new ApplicationException(file + ": does not have 6 fields (total score) or 15 fields (skins): " + string.Join(", ", line));
        //                }

        //                if (line.Length == 6)
        //                {
        //                    kvpList.Add(new KeyValuePair<string, string>(
        //                        string.Format("{0}[{1}][Flight]", ResultsPool, index), flight.ToString()));

        //                    // since these are not skins, set the hole number to 0
        //                    kvpList.Add(new KeyValuePair<string, string>(
        //                        string.Format("{0}[{1}][Hole]", ResultsPool, index), "0"));

        //                    DateTime dt;
        //                    if (TournamentNameIndex >= 0)
        //                    {
        //                        dt = TournamentNames[TournamentNameIndex].StartDate.AddDays(day);
        //                    }
        //                    else if (!DateTime.TryParse(line[0], out dt))
        //                    {
        //                        throw new ArgumentException(file + ": invalid date: " + lines[0]);
        //                    }

        //                    kvpList.Add(new KeyValuePair<string, string>(
        //                        string.Format("{0}[{1}][Date]", ResultsPool, index), dt.ToString("yyyy-MM-dd")));

        //                    kvpList.Add(new KeyValuePair<string, string>(
        //                        string.Format("{0}[{1}][Place]", ResultsPool, index), line[2]));

        //                    kvpList.Add(new KeyValuePair<string, string>(
        //                        string.Format("{0}[{1}][Score]", ResultsPool, index), line[4]));

        //                    float winnings;
        //                    if (!float.TryParse(line[5], out winnings))
        //                    {
        //                        throw new ArgumentException(file + ": winnings must be a decimal number: " + line[5]);
        //                    }
        //                    // round to a multiple of 5
        //                    int w = ((int)((winnings + 2.5f) / 5f)) * 5;

        //                    kvpList.Add(new KeyValuePair<string, string>(
        //                        string.Format("{0}[{1}][Winnings]", ResultsPool, index), w.ToString()));

        //                    // field 3 looks like "(1 )  Albitz, Paul , 9079663"
        //                    int paren = line[3].IndexOf(')');
        //                    if (paren < 0)
        //                    {
        //                        throw new ArgumentException(file + ": expected team number within parentheses: " + line[3]);
        //                    }
        //                    string team = line[3].Substring(0, paren);
        //                    team = team.Trim('(');
        //                    string rest = line[3].Substring(paren + 1);
        //                    string[] fields = rest.Split(',');

        //                    if (fields.Length != 3)
        //                    {
        //                        throw new ArgumentException(file + ": expected 3 fields for last name, first name, GHIN: " + rest);
        //                    }

        //                    kvpList.Add(new KeyValuePair<string, string>(
        //                        string.Format("{0}[{1}][TeamNumber]", ResultsPool, index), team.Trim()));

        //                    kvpList.Add(new KeyValuePair<string, string>(
        //                        string.Format("{0}[{1}][Name]", ResultsPool, index), fields[0].Trim() + ", " + fields[1].Trim()));

        //                    kvpList.Add(new KeyValuePair<string, string>(
        //                        string.Format("{0}[{1}][GHIN]", ResultsPool, index), fields[2].Trim()));
        //                }

        //                else // skins files have 15 fields
        //                {
        //                    kvpList.Add(new KeyValuePair<string, string>(
        //                            string.Format("{0}[{1}][Flight]", ResultsPool, index), flight.ToString()));

        //                    // with skins, set the Place field to 0
        //                    kvpList.Add(new KeyValuePair<string, string>(
        //                        string.Format("{0}[{1}][Place]", ResultsPool, index), "0"));

        //                    DateTime dt;
        //                    if (TournamentNameIndex >= 0)
        //                    {
        //                        dt = TournamentNames[TournamentNameIndex].StartDate.AddDays(day);
        //                    }
        //                    else if (!DateTime.TryParse(line[1], out dt))
        //                    {
        //                        throw new ArgumentException(file + ": invalid date: " + lines[1]);
        //                    }

        //                    kvpList.Add(new KeyValuePair<string, string>(
        //                        string.Format("{0}[{1}][Date]", ResultsPool, index), dt.ToString("yyyy-MM-dd")));

        //                    kvpList.Add(new KeyValuePair<string, string>(
        //                        string.Format("{0}[{1}][Hole]", ResultsPool, index), line[9]));

        //                    kvpList.Add(new KeyValuePair<string, string>(
        //                        string.Format("{0}[{1}][Score]", ResultsPool, index), line[10]));

        //                    float winnings;
        //                    if (!float.TryParse(line[11], out winnings))
        //                    {
        //                        throw new ArgumentException(file + ": winnings must be a decimal number: " + line[11]);
        //                    }
        //                    // round to a multiple of 5
        //                    int w = ((int)((winnings + 2.5f) / 5f)) * 5;

        //                    kvpList.Add(new KeyValuePair<string, string>(
        //                        string.Format("{0}[{1}][Winnings]", ResultsPool, index), w.ToString()));

        //                    kvpList.Add(new KeyValuePair<string, string>(
        //                        string.Format("{0}[{1}][TeamNumber]", ResultsPool, index), "0"));

        //                    kvpList.Add(new KeyValuePair<string, string>(
        //                        string.Format("{0}[{1}][Name]", ResultsPool, index), line[8]));

        //                    GHINEntry gi = GHINEntry.FindName(GHINEntries, line[8]);
        //                    int ghinNumber = 0;

        //                    if (gi != null)
        //                    {
        //                        ghinNumber = gi.GHIN;

        //                    }

        //                    kvpList.Add(new KeyValuePair<string, string>(
        //                        string.Format("{0}[{1}][GHIN]", ResultsPool, index), ghinNumber.ToString()));
        //                }
        //                index++;
        //            }
        //        }
        //    }
        //}

        private void AddChitsEntries(string file, List<KeyValuePair<string, string>> kvpList)
        {
            if (string.IsNullOrEmpty(file))
            {
                return;
            }

            string fullPath = Path.Combine(CSVFolderName, file);
            if (!File.Exists(fullPath))
            {
                throw new ApplicationException("File doesn't exist: " + fullPath);
            }

            int index = 0;
            string currentFlight = null;
            int currentFlightIndex = 1;
            using (TextReader tr = new StreamReader(fullPath))
            {
                string[][] lines = CSVParser.Parse(tr);
                foreach (var line in lines)
                {
                    if (line.Length > 0)
                    {
                        if (line.Length < 7)
                        {
                            throw new ApplicationException(file + ": does not have 7 fields: " + string.Join(", ", line));
                        }

                        if(string.IsNullOrEmpty(line[0]) && string.IsNullOrEmpty(line[1]))
                        {
                            // skip empty lines
                            continue;
                        }

                        float winnings;
                        if (!float.TryParse(line[6], out winnings))
                        {
                            throw new ArgumentException(file + ": winnings must be a decimal number: " + line[6]);
                        }
                        // round to a multiple of 5
                        int w = ((int)((winnings + 2.5f) / 5f)) * 5;

                        // skip over players with winnings 0
                        if (w == 0) continue;

                        DateTime dt;
                        if (!DateTime.TryParse(line[0], out dt))
                        {
                            throw new ArgumentException(file + ": invalid date: " + line[0]);
                        }

                        kvpList.Add(new KeyValuePair<string, string>(
                            string.Format("{0}[{1}][Date]",ResultsChits , index), dt.ToString("yyyy-MM-dd")));

                        kvpList.Add(new KeyValuePair<string, string>(
                            string.Format("{0}[{1}][Place]", ResultsChits , index), line[3]));

                        kvpList.Add(new KeyValuePair<string, string>(
                            string.Format("{0}[{1}][Score]", ResultsChits , index), line[5]));

                        kvpList.Add(new KeyValuePair<string, string>(
                            string.Format("{0}[{1}][Winnings]", ResultsChits , index), w.ToString()));

                        // flight looks like FLT. 1 (0-19)
                        // or: "A"  (QUALIFIER)
                        // or: Champions ( 55 & Old
                        // or: FLIGHT A
                        if(string.IsNullOrEmpty(currentFlight))
                        {
                            currentFlight = line[2];
                        }
                        if(!string.Equals(currentFlight, line[2]))
                        {
                            currentFlight = line[2];
                            currentFlightIndex++;
                        }
                        if (!currentFlight.ToLower().Contains("flight") && !currentFlight.ToLower().Contains("flt"))
                        {
                            kvpList.Add(new KeyValuePair<string, string>(
                                string.Format("{0}[{1}][FlightName]", ResultsChits , index), "Flight " + currentFlight));
                        }
                        else
                        {
                            kvpList.Add(new KeyValuePair<string, string>(
                                string.Format("{0}[{1}][FlightName]", ResultsChits , index), currentFlight));
                        }

                        kvpList.Add(new KeyValuePair<string, string>(
                            string.Format("{0}[{1}][Flight]", ResultsChits , index), currentFlightIndex.ToString()));

                        // field 3 looks like "(1 )  Albitz, Paul , 9079663"
                        int paren = line[4].IndexOf(')');
                        if (paren < 0)
                        {
                            throw new ArgumentException(file + ": expected team number within parentheses: " + line[4]);
                        }
                        string team = line[4].Substring(0, paren);
                        team = team.Trim('(');
                        string rest = line[4].Substring(paren + 1);
                        string[] fields = rest.Split(',');

                        if (fields.Length != 3)
                        {
                            throw new ArgumentException(file + ": expected 3 fields for last name, first name, GHIN: " + rest);
                        }

                        kvpList.Add(new KeyValuePair<string, string>(
                            string.Format("{0}[{1}][TeamNumber]", ResultsChits , index), team.Trim()));

                        kvpList.Add(new KeyValuePair<string, string>(
                            string.Format("{0}[{1}][Name]", ResultsChits , index), fields[0].Trim() + ", " + fields[1].Trim()));

                        kvpList.Add(new KeyValuePair<string, string>(
                            string.Format("{0}[{1}][GHIN]", ResultsChits , index), fields[2].Trim()));

                        index++;
                    }
                }
            }
        }

        private List<List<KeyValuePair<string, string>>> AddMatchPlayEntries(string file)
        {
            if (string.IsNullOrEmpty(file))
            {
                return null;
            }
            string fullPath = Path.Combine(CSVFolderName, file);
            if (!File.Exists(fullPath))
            {
                throw new ApplicationException("File doesn't exist: " + fullPath);
            }

            List<List<KeyValuePair<string, string>>> masterList = new List<List<KeyValuePair<string, string>>>();
            List<KeyValuePair<string, string>> kvpList = new List<KeyValuePair<string, string>>();
            masterList.Add(kvpList);

            bool header = true;
            int index = 0;
            using (TextReader tr = new StreamReader(fullPath))
            {
                string[][] lines = CSVParser.Parse(tr);
                foreach (var line in lines)
                {
                    if(header)
                    {
                        header = false;
                        if(string.CompareOrdinal(line[0], "Round") != 0)
                        {
                            throw new ApplicationException(file +
                                ": This file does not look like match play results because line 1 does not look like the expected header: Round,Match Number,Player 1,Player 2");
                        }

                        
                    } 
                    else if (line.Length > 0)
                    {
                        if (line.Length != 4)
                        {
                            throw new ApplicationException(file + ": does not have 4 fields: " +
                                                           string.Join(", ", line));
                        }

                        if (string.IsNullOrEmpty(line[0]) && string.IsNullOrEmpty(line[1]))
                        {
                            // skip empty lines
                            continue;
                        }

                        kvpList.Add(new KeyValuePair<string, string>(
                            string.Format("{0}[{1}][Round]", MatchPlayResultsScores, index), line[0]));
                        kvpList.Add(new KeyValuePair<string, string>(
                            string.Format("{0}[{1}][MatchNumber]", MatchPlayResultsScores, index), line[1]));
                        kvpList.Add(new KeyValuePair<string, string>(
                            string.Format("{0}[{1}][Player1]", MatchPlayResultsScores, index), line[2]));
                        kvpList.Add(new KeyValuePair<string, string>(
                            string.Format("{0}[{1}][Player2]", MatchPlayResultsScores, index), line[3]));
                        index++;
                    }
                }
            }

            return masterList;
        }

        private int findCol(string[] line, string header, string fullPath)
        {
            for (int col = 0; col < line.Length; col++)
            {
                if (string.Compare(line[col], header, true) == 0)
                {
                    return col;
                }
            }
            throw new ApplicationException("Failed to find column header: " + header + " in " + fullPath);
        }

        private int[] findAllCol(string[] line, string header, string fullPath)
        {
            List<int> cols = new List<int>();

            string lowerCaseHeader = header.ToLower();
            for (int col = 0; col < line.Length; col++)
            {
                if (line[col].ToLower().StartsWith(lowerCaseHeader))
                {
                    cols.Add(col);
                }
            }

            if (cols.Count == 0)
            {
                throw new ApplicationException("Failed to find column header: " + header + " in " + fullPath);
            }

            return cols.ToArray();
        }

        private void ReadGgResultsFile(
            string fullPath,
            string[] flightNames,
            List<List<KeyValuePair<string, string>>> kvpScoresList, 
            List<KeyValuePair<string, string>> kvpChitsList, 
            ref int chitsIndex)
        {
            if (string.IsNullOrEmpty(fullPath))
            {
                return;
            }
            if (!File.Exists(fullPath))
            {
                throw new ApplicationException("File doesn't exist: " + fullPath);
            }

            List<KeyValuePair<string, string>> kvpList = null;

            List<Score> scoreList = new List<Score>();

            int maxDivisions = 4;
            bool findHeaders = true;
            int dateColumn = -1;
            int lastNameCol = -1;
            int firstNameCol = -1;
            int ghinCol = -1;
            int teamIdCol = -1;
            int divisionNameCol = -1;
            int flightNameCol = -1;
            int flightNumberCol = -1;
            int[] roundScoreCols = null;
            int[] cumulativeScoreCols = null;
            int[] purseCols = null;
            int[] rankCols = null;

            bool isMultiDay = TournamentNames[TournamentNameIndex].StartDate != TournamentNames[TournamentNameIndex].EndDate;

            int lineNumber = 1;
            using (TextReader tr = new StreamReader(fullPath))
            {
                string[][] lines = CSVParser.Parse(tr);
                foreach (var line in lines)
                {
                    if (findHeaders)
                    {
                        dateColumn = findCol(line, "Round Short Date", fullPath);
                        lastNameCol = findCol(line, "Last Name", fullPath);
                        firstNameCol = findCol(line, "First Name", fullPath);
                        ghinCol = findCol(line, "GHIN", fullPath);
                        teamIdCol = findCol(line, "Team Id", fullPath);
                        divisionNameCol = findCol(line, "Division Name", fullPath);
                        flightNameCol = findCol(line, "Flight Name", fullPath);
                        flightNumberCol = findCol(line, "Flight Number", fullPath);

                        // Find entries with multiple columns
                        try
                        {
                            roundScoreCols = findAllCol(line, "Round Score", fullPath);
                        }
                        catch
                        {
                            // For a 1 day, it is only called "Score"
                            roundScoreCols = findAllCol(line, "Score", fullPath);
                        }
                        if (isMultiDay)
                        {
                            cumulativeScoreCols = findAllCol(line, "Cumulative Score", fullPath);
                        }
                        purseCols = findAllCol(line, "Purse", fullPath);
                        rankCols = findAllCol(line, "Rank", fullPath);
                        findHeaders = false;

                        // TODO: error handling for columns not found
                    }
                    else
                    {
                        lineNumber++;

                        // skip empty lines
                        if (string.IsNullOrWhiteSpace(line[dateColumn]) && string.IsNullOrWhiteSpace(line[lastNameCol]))
                        {
                            continue;
                        }

                        // if players are "removed" there are a number of empty columns.
                        if (string.IsNullOrWhiteSpace(line[divisionNameCol]))
                        {
                            continue;
                        }

                        // Some rows are used for recording which player's drive is used. There is no GHIN, so skip those
                        if (IsEclectic && string.IsNullOrWhiteSpace(line[ghinCol]))
                        {
                            continue;
                        }

                        Score score = new Score();
                        // Not all tournaments are 2 day. If round 2
                        // is -1, then it wasn't provided.
                        score.ScoreRound2 = -1;

                        // Read round 1 score. 
                        // There are 8 columns of "round score" (2 per division) data and only 2 of them will be filled in
                        int scoreColIndex = 0;
                        score.ScoreRound1 = 0;
                        bool round1ScoreFound = false;
                        for (; scoreColIndex < roundScoreCols.Length; scoreColIndex++)
                        {
                            if (!string.IsNullOrWhiteSpace(line[roundScoreCols[scoreColIndex]]))
                            {
                                int scoreRound1;
                                if (int.TryParse(line[roundScoreCols[scoreColIndex]], out scoreRound1))
                                {
                                    round1ScoreFound = true;
                                    score.ScoreRound1 = scoreRound1;
                                    if (IsEclectic && (score.ScoreRound1 < 30))
                                    {
                                        // For an eclectic, GG provides score relative to par
                                        score.ScoreRound1 += 72;
                                    }
                                    break;
                                }
                                else if ((string.Compare(line[roundScoreCols[scoreColIndex]], "E") == 0) && IsEclectic)
                                {
                                    score.ScoreRound1 = 72;
                                }
                            }
                        }

                        // Multi-division tournaments will have 1 .csv per division. Those players
                        // not in the division will be empty. Skip over those.
                        if (!round1ScoreFound)
                        {
                            continue;
                        }

                        // Read round 2 score.
                        // There are 8 columns of "round score" (2 per division) data and only 2 of them will be filled in
                        score.ScoreRound2 = 0;
                        for (scoreColIndex++; scoreColIndex < roundScoreCols.Length; scoreColIndex++)
                        {
                            if (!string.IsNullOrWhiteSpace(line[roundScoreCols[scoreColIndex]]))
                            {
                                int scoreRound2;
                                if (int.TryParse(line[roundScoreCols[scoreColIndex]], out scoreRound2))
                                {
                                    score.ScoreRound2 = scoreRound2;
                                    if (IsEclectic && (score.ScoreRound2 < 30))
                                    {
                                        // For an eclectic, GG provides score relative to par
                                        score.ScoreRound2 += 72;
                                    }
                                    break;
                                }
                                else if ((string.Compare(line[roundScoreCols[scoreColIndex]], "E") == 0) && IsEclectic)
                                {
                                    score.ScoreRound2 = 72;
                                }
                            }
                        }

                        // Read tournament date
                        DateTime dateTime;
                        if (!DateTime.TryParse(line[dateColumn], out dateTime))
                        {
                            throw new ArgumentException(fullPath + ": line " + lineNumber + ": invalid date: " + lines[dateColumn]);
                        }
                        score.Date = dateTime;

                        // Read flight number
                        int flightNumber = 0;
                        if (string.IsNullOrWhiteSpace(line[flightNumberCol]))
                        {
                            // For some tournaments, everyone is in the same flight and so there is no number
                            flightNumber = 1;
                        }
                        else if (!int.TryParse(line[flightNumberCol], out flightNumber))
                        {
                            throw new ArgumentException(fullPath + ": line " + lineNumber + ": unable to determine flight number");
                        }
                        score.Flight = flightNumber;

                        // Allow names for flight numbers 0 to 9
                        if ((flightNumber < flightNames.Length) && string.IsNullOrEmpty(flightNames[flightNumber]))
                        {
                            flightNames[flightNumber] = line[flightNameCol];
                        }

                        string lastNameFirstName = line[lastNameCol] + ", " + line[firstNameCol];

                        // Read team number
                        int teamNumber = 0;
                        if (!int.TryParse(line[teamIdCol], out teamNumber))
                        {
                            throw new ArgumentException(fullPath + ": line " + lineNumber + ": unable to determine team number");
                        }
                        score.TeamNumber = teamNumber;

                        // The results file contains a single line for each player, even if this is a team event.
                        // Check to see if there is a score already for someone with the same team number (the teammates)
                        bool foundTeamMember = false;
                        for (int scoreIndex = 0; !foundTeamMember && (scoreIndex < scoreList.Count); scoreIndex++)
                        {
                            if (scoreList[scoreIndex].TeamNumber == teamNumber)
                            {
                                foundTeamMember = true;

                                // Add the chits data if there is a purse for this entry
                                // The chits results are reported individually, not as a team.
                                chitsIndex = AddGgChits(kvpChitsList, chitsIndex, line, purseCols, rankCols, flightNameCol, ghinCol, scoreList[scoreIndex], lastNameFirstName);

                                // For scores, put all the names in a single score object
                                if (string.IsNullOrEmpty(scoreList[scoreIndex].Name2))
                                {
                                    scoreList[scoreIndex].Name2 = lastNameFirstName;
                                }
                                else if (string.IsNullOrEmpty(scoreList[scoreIndex].Name3))
                                {
                                    scoreList[scoreIndex].Name3 = lastNameFirstName;
                                }
                                else if (string.IsNullOrEmpty(scoreList[scoreIndex].Name4))
                                {
                                    scoreList[scoreIndex].Name4 = lastNameFirstName;
                                }
                                else
                                {
                                    throw new ArgumentException(fullPath + ": line " + lineNumber + ": this is the 5th player with team number " + teamNumber);
                                }
                            }
                        }

                        // If we just added a name to an existing scores entry, there is no need
                        // to create another scores entry. The chits data has already been recorded.
                        if (foundTeamMember) continue;

                        if (string.IsNullOrEmpty(line[lastNameCol]) || string.IsNullOrEmpty(line[firstNameCol]))
                        {
                            throw new ArgumentException(fullPath + ": line " + lineNumber + ": empty last name or first name");
                        }
                        score.Name1 = lastNameFirstName;

                        // Read total (cumulative) score
                        if (isMultiDay)
                        {
                            int cumulativeScoreColIndex = 0;
                            score.ScoreTotal = -1;
                            float cumulativeScore = 0;
                            for (; cumulativeScoreColIndex < cumulativeScoreCols.Length; cumulativeScoreColIndex++)
                            {
                                if (!string.IsNullOrEmpty(line[cumulativeScoreCols[cumulativeScoreColIndex]]))
                                {
                                    if (float.TryParse(line[cumulativeScoreCols[cumulativeScoreColIndex]], out cumulativeScore))
                                    {
                                        if (IsEclectic && (cumulativeScore < 30))
                                        {
                                            // For an eclectic, GG provides score relative to par, even for the "cumulative score"
                                            cumulativeScore += 72;
                                        }
                                        else if (IsStableford && (score.ScoreRound2 > 0) && (cumulativeScore < (score.ScoreRound1 + score.ScoreRound2)))
                                        {
                                            // GG can put just round 1 in the cumulative column ...
                                            cumulativeScore = score.ScoreRound1 + score.ScoreRound2;
                                        }
                                        score.ScoreTotal = cumulativeScore;
                                        break;
                                    }
                                    else if ((string.Compare(line[cumulativeScoreCols[cumulativeScoreColIndex]], "E") == 0) && IsEclectic)
                                    {
                                        score.ScoreTotal = 72;
                                    }
                                }
                            }

                            // There will only be a cumulative score if there is at least a round 1 score.
                            //if ((score.ScoreTotal == -1) && (score.ScoreRound1 != 0))
                            //{
                            // Cumulative score can be DNF or NS. Leave it at -1. Filter out these entries below.
                            //throw new ArgumentException(fullPath + ": line " + lineNumber + ": no cumulative score");
                            //}
                        }
                        else
                        {
                            // For a single day tournament, round 1 is the total score
                            score.ScoreTotal = score.ScoreRound1;
                        }

                        scoreList.Add(score);

                        // Add the chits entries if the purse is non-zero
                        chitsIndex = AddGgChits(kvpChitsList, chitsIndex, line, purseCols, rankCols, flightNameCol, ghinCol, score, score.Name1);
                    }
                }
            }

            // Now, filter out the scores for people that were disqualified or
            // didn't play round 1 or round 2. Those people are omitted from 
            // the website results.

            // First, check to see if any round 2 is filled in. If
            // not, then this is just round 1 scores.
            bool isRound1 = true;
            foreach (var score in scoreList)
            {
                if (score.ScoreRound2 > 0)
                {
                    isRound1 = false;
                    break;
                }
            }

            // Create the key-value-pairs for the scores data. Filter out
            // players for which there is no data for rounds.
            int index = 0;
            foreach (var score in scoreList)
            {
                // If both rounds are 0, then don't display it.  
                // If round 1 is 0 and round 2 was not provided, then it is a 1 day and don't display it.
                // If all of round 2 is 0, then this is day 1 of a 2 day tournament.
                // If one round is large (non-stableford) and the other is 0 and the scores are from day 2 of a 2 day tournament, then don't display it. 
                // If round 1 is 0, round 2 is valid, and not an eclectic, then the player was disqualified from round 1, but played round 2. (could be stableford)
                // For a stableford, 0 is a valid value.
                // Also, if the name field is empty, then skip it.
                if (((score.ScoreRound1 == 0) && (score.ScoreRound2 == 0)) ||
                    ((score.ScoreRound1 == 0) && (score.ScoreRound2 == -1)) ||
                    ((score.ScoreRound1 > 55) && (score.ScoreRound2 == 0) && !isRound1) ||
                    ((score.ScoreRound1 != 0) && (score.ScoreRound2 == 0) && !isRound1 && (score.ScoreTotal == -1)) ||
                    //((score.ScoreRound1 == 0) && (score.ScoreRound2 > 0) && !IsEclectic) ||
                    string.IsNullOrEmpty(score.Name1.Trim()) || (score.Name1.Trim() == ","))
                {
                    continue;
                }

                // Split up the data so it is uploaded in several "chunks"
                if ((index % 40) == 0)
                {
                    kvpList = new List<KeyValuePair<string, string>>();
                    kvpScoresList.Add(kvpList);
                    index = 0;
                }

                // Add the score data to the key-value-pair list
                score.AddToList(kvpList, index);

                index++;
            }
        }

        private int AddGgChits(
            List<KeyValuePair<string, string>> kvpChitsList,
            int chitsIndex,
            string[] line,
            int[] purseCols,
            int[] rankCols,
            int flightNameCol,
            int ghinCol,
            Score score,
            string name)
        {
            // If the purse is non-zero, there are chits to report
            float purse;
            for (int purseColIndex = 0; purseColIndex < purseCols.Length; purseColIndex++)
            {
                string purseWithoutDollarSign = line[purseCols[purseColIndex]].Replace("$", "");
                if (float.TryParse(purseWithoutDollarSign, out purse))
                {
                    if (purse > 0)
                    {
                        purse = purse / TournamentNames[TournamentNameIndex].TeamSize;

                        kvpChitsList.Add(new KeyValuePair<string, string>(
                            string.Format("{0}[{1}][Winnings]", ResultsChits, chitsIndex), purse.ToString()));

                        kvpChitsList.Add(new KeyValuePair<string, string>(
                            string.Format("{0}[{1}][Date]", ResultsChits, chitsIndex), score.Date.ToString("yyyy-MM-dd")));

                        int place = 0;
                        for (int rankIndex = 0; rankIndex < rankCols.Length; rankIndex++)
                        {
                            if (int.TryParse(line[rankCols[rankIndex]], out place))
                            {
                                break;
                            }
                        }
                        kvpChitsList.Add(new KeyValuePair<string, string>(
                            string.Format("{0}[{1}][Place]", ResultsChits, chitsIndex), place.ToString()));

                        kvpChitsList.Add(new KeyValuePair<string, string>(
                            string.Format("{0}[{1}][Score]", ResultsChits, chitsIndex), score.ScoreTotal.ToString()));

                        kvpChitsList.Add(new KeyValuePair<string, string>(
                            string.Format("{0}[{1}][Flight]", ResultsChits, chitsIndex), score.Flight.ToString()));

                        if (!line[flightNameCol].ToLower().Contains("flight"))
                        {
                            kvpChitsList.Add(new KeyValuePair<string, string>(
                                string.Format("{0}[{1}][FlightName]", ResultsChits, chitsIndex), "Flight " + score.Flight));
                        }
                        else
                        {
                            kvpChitsList.Add(new KeyValuePair<string, string>(
                                string.Format("{0}[{1}][FlightName]", ResultsChits, chitsIndex), line[flightNameCol]));
                        }

                        kvpChitsList.Add(new KeyValuePair<string, string>(
                            string.Format("{0}[{1}][TeamNumber]", ResultsChits, chitsIndex), score.TeamNumber.ToString()));

                        kvpChitsList.Add(new KeyValuePair<string, string>(
                            string.Format("{0}[{1}][Name]", ResultsChits, chitsIndex), name));

                        // TODO: do I have to merge the team results into a single entry by filling in the
                        // names of the other players?

                        kvpChitsList.Add(new KeyValuePair<string, string>(
                            string.Format("{0}[{1}][GHIN]", ResultsChits, chitsIndex), line[ghinCol]));

                        chitsIndex++;
                    }
                }
            }

            return chitsIndex;
        }

        private List<List<KeyValuePair<string, string>>> AddScoresEntries(string file)
        {
            if (string.IsNullOrEmpty(file))
            {
                return null;
            }
            string fullPath = Path.Combine(CSVFolderName, file);
            if (!File.Exists(fullPath))
            {
                throw new ApplicationException("File doesn't exist: " + fullPath);
            }

            List<List<KeyValuePair<string, string>>> masterList = new List<List<KeyValuePair<string, string>>>();
            List<KeyValuePair<string, string>> kvpList = null;

            List<Score> scoreList = new List<Score>();

            using (TextReader tr = new StreamReader(fullPath))
            {
                string[][] lines = CSVParser.Parse(tr);
                foreach (var line in lines)
                {
                    if (line.Length > 0)
                    {
                        if (line.Length < 23)
                        {
                            throw new ApplicationException(file + ": does not have 23 fields: " + string.Join(", ", line));
                        }

                        if(string.IsNullOrEmpty(line[18]) && string.IsNullOrEmpty(line[19]) && string.IsNullOrEmpty(line[13]))
                        {
                            // empty line
                            continue;
                        }

                        Score score = new Score();

                        // Stableford negative numbers start with "(" instead of "-"
                        bool negative = false;
                        if (line[18].StartsWith("("))
                        {
                            negative = true;
                            line[18] = line[18].TrimStart('(');
                        }

                        float scoreFloat;
                        if (!float.TryParse(line[18], out scoreFloat))
                        {
                            throw new ArgumentException(file + ": score round 1 must be an integer: " + line[18]);
                        }
                        if (negative) scoreFloat = -scoreFloat;
                        score.ScoreRound1 = (int)scoreFloat;

                        // Not all tournaments are 2 day.
                        score.ScoreRound2 = -1;
                        if (!string.IsNullOrEmpty(line[19]))
                        {
                            negative = false;
                            if (line[19].StartsWith("("))
                            {
                                negative = true;
                                line[19] = line[19].TrimStart('(');
                            }
                            if (!float.TryParse(line[19], out scoreFloat))
                            {
                                throw new ArgumentException(file + ": score round 2 must be an integer: " + line[19]);
                            }
                            if (negative) scoreFloat = -scoreFloat;
                            score.ScoreRound2 = (int)scoreFloat;
                        }

                        DateTime dt;
                        if (!DateTime.TryParse(line[2], out dt))
                        {
                            throw new ArgumentException(file + ": invalid date: " + lines[2]);
                        }
                        score.Date = dt;

                        // flight looks like FLT. 1 (0-19), or just "1"
                        int flight = 0;
                        string flightString = line[11];
                        int paren = flightString.IndexOf('(');
                        if (paren > 0)
                        {
                            flightString = flightString.Substring(0, paren).Trim();
                        }
                        for (int i = flightString.Length - 1; i >= 0; i--)
                        {
                            if ((flightString[i] >= '0') && (flightString[i] <= '9'))
                            {
                                flight = flightString[i] - '0';
                                break;
                            }
                        }
                        if (flight == 0)
                        {
                            throw new ArgumentException("Unable to determine flight in: " + line[11]);
                        }
                        score.Flight = flight;

                        int teamNumber;
                        if (!int.TryParse(line[12], out teamNumber))
                        {
                            throw new ArgumentException(file + ": team number must be an integer: " + line[12]);
                        }
                        score.TeamNumber = teamNumber;

                        score.Name1 = line[13];
                        score.Name2 = line[14];
                        score.Name3 = line[15];
                        score.Name4 = line[16];

                        negative = false;
                        if (line[22].StartsWith("("))
                        {
                            negative = true;
                            line[22] = line[22].TrimStart('(');
                        }
                        if (!float.TryParse(line[22], out scoreFloat))
                        {
                            throw new ArgumentException(file + ": score total must be an integer: " + line[22]);
                        }
                        if (negative) scoreFloat = -scoreFloat;
                        score.ScoreTotal = (int)scoreFloat;

                        scoreList.Add(score);
                    }
                }
            }

            // Check to see if any round 2 is filled in
            bool isRound1 = true;
            foreach (var score in scoreList)
            {
                if(score.ScoreRound2 > 0)
                {
                    isRound1 = false;
                    break;
                }
            }

            int index = 0;
            foreach (var score in scoreList)
            {
                // If both rounds are 0, then don't display it.  
                // If round 1 is 0 and round 2 was not provided, then it is a 1 day and don't display it.
                // If all of round 2 is 0, then this is day 1 of a 2 day tournament.
                // If one round is large (non-stableford) and the other is 0 and the scores are from day 2 of a 2 day tournament, then don't display it. 
                // If round 1 is 0, round 2 is valid, and not an eclectic, then the player was disqualified from round 1, but played round 2. (could be stableford)
                // For a stableford, 0 is a valid value.
                // Also, if the name field is empty, then skip it.
                if (((score.ScoreRound1 == 0) && (score.ScoreRound2 == 0)) ||
                    ((score.ScoreRound1 == 0) && (score.ScoreRound2 == -1)) ||
                    ((score.ScoreRound1 > 55) && (score.ScoreRound2 == 0) && !isRound1) ||
                    //((score.ScoreRound1 == 0) && (score.ScoreRound2 > 0) && !IsEclectic) ||
                    string.IsNullOrEmpty(score.Name1.Trim()) || (score.Name1.Trim() == ","))
                {
                    continue;
                }

                if ((index % 40) == 0)
                {
                    kvpList = new List<KeyValuePair<string, string>>();
                    masterList.Add(kvpList);
                    index = 0;
                }

                if (!isRound1 && IsEclectic)
                {
                    // don't use total as that is the sum of round 1 and round 2
                    score.ScoreTotal = score.ScoreRound2;
                }

                score.AddToList(kvpList, index);

                index++;
            }

            return masterList;
        }
        

        //private void MergeResultsPoolKvp(List<KeyValuePair<string, string>> mergedList, List<KeyValuePair<string, string>> list)
        //{
        //    int lastIndex = 0;
        //    foreach (var kvp in mergedList)
        //    {
        //        string key = string.Format("{0}[{1}][Flight]", ResultsPool, lastIndex);
        //        if (kvp.Key == key)
        //        {
        //            lastIndex++;
        //        }
        //    }

        //    if(lastIndex == 0)
        //    {
        //        mergedList.AddRange(list);
        //        return;
        //    }

        //    foreach (var kvp in list)
        //    {
        //        int startBracket = "ResultsPool[".Length;
        //        int endBracket = kvp.Key.IndexOf("][");
        //        string s = kvp.Key.Substring(startBracket, endBracket - startBracket);
        //        int oldIndex = int.Parse(s);

        //        KeyValuePair<string, string> newKvp;
        //        string newKey = kvp.Key.Replace(kvp.Key.Substring(0, endBracket), "ResultsPool[" + (oldIndex + lastIndex));
        //        newKvp = new KeyValuePair<string, string>(newKey, kvp.Value);
        //        mergedList.Add(newKvp);
        //    }
        //}

        private async Task SubmitCsv(object o)
        {

            if (string.IsNullOrEmpty(GgTournamentResultsCsvFileName))
            {
                MessageBox.Show("Please select file(s) to submit.", "Error");
                return;
            }

            _kvpChitsList = new List<KeyValuePair<string, string>>();
            int chitsIndex = 0;

            // Allow names for flight numbers 0 to 9
            string[] flightNames = new string[10];

            // The scores are a list of lists, so they can be uploaded in chunks
            _kvpScoresList = new List<List<KeyValuePair<string, string>>>();

            string[] fileNames = GgTournamentResultsCsvFileName.Split(',');
            foreach (var name in fileNames)
            {
                ReadGgResultsFile(name.Trim(), flightNames, _kvpScoresList, _kvpChitsList, ref chitsIndex);
            }

            var kvpFlightNames = new List<KeyValuePair<string, string>>();
            for (int i = 0; i < flightNames.Length; i++)
            {
                if (!string.IsNullOrEmpty(flightNames[i]))
                {
                    kvpFlightNames.Add(new KeyValuePair<string, string>(
                                    string.Format("{0}[{1}]", FlightNameTable, i), flightNames[i]));
                }
            }

            string submitted = string.Empty;

            // cancelled password input
            if (string.IsNullOrEmpty(Credentials.LoginPassword))
            {
                return;
            }

            //if (kvpPoolList.Count > 0)
            //{
            //    if (!await SubmitResultsCsv(kvpPoolList, "pool", true))
            //    {
            //        MessageBox.Show("Failed to submit pool results.  Did not try to upload chits or scores.");
            //        return;
            //    }

            //    if (!string.IsNullOrEmpty(submitted)) submitted += ", ";
            //    submitted += "pool";
            //}

            if (_kvpChitsList.Count > 0)
            {
                if (!await SubmitResultsCsv(_kvpChitsList, "chits", true))
                {
                    MessageBox.Show("Failed to submit chits results.  Did not try to upload scores.");
                    return;
                }

                if (!string.IsNullOrEmpty(submitted)) submitted += ", ";
                submitted += "chits";
            }

            if ((_kvpScoresList != null) && (_kvpScoresList.Count > 0))
            {
                foreach (var kvpScoresList in _kvpScoresList)
                {
                    if (!await SubmitResultsCsv(kvpScoresList, MatchPlay ? "match play scores" : "scores", kvpScoresList == _kvpScoresList[0]))
                    {
                        MessageBox.Show("Failed to submit scores results.");
                        return;
                    }
                }

                if (!string.IsNullOrEmpty(submitted)) submitted += ", ";
                submitted += "scores";
            }

            if (kvpFlightNames.Count > 0)
            {
                if (!await SubmitResultsCsv(kvpFlightNames, "FlightNames", true))
                {
                    MessageBox.Show("Failed to submit flight names.");
                }
            }

            if (!string.IsNullOrEmpty(submitted))
            {
                MessageBox.Show("Submitted results for " + submitted, "Upload Results");
            }
        }

        private async Task SubmitGgResultsLink(object o)
        {
            if (string.IsNullOrEmpty(GgTournamentResultsLink))
            {
                MessageBox.Show("Fill in the Golf Genius results link");
                return;
            }

            if (!GgTournamentResultsLink.ToLower().StartsWith("http"))
            {
                MessageBox.Show("Invalid Golf Genius results link: must contain \"http\" or \"https\" at the start of the link");
                return;
            }

            // cancelled password input
            if (string.IsNullOrEmpty(Credentials.LoginPassword))
            {
                return;
            }

            // This doesn't need to be a list since it has only 1 entry, but
            // make it a list to re-use the same code for submitting results.
            var kvpGgResultsLinkList = new List<KeyValuePair<string, string>>();

            kvpGgResultsLinkList.Add(new KeyValuePair<string, string>(string.Format("{0}[Link]", GolfGeniusResultsLink), GgTournamentResultsLink));

            if (!await SubmitResultsCsv(kvpGgResultsLinkList, GolfGeniusResultsLink, true))
            {
                MessageBox.Show("Failed to submit Golf Genius results link.");
                return;
            }

            MessageBox.Show("Submitted Golf Genius results link");
        }

        private async Task ClearCsv(object o)
        {
            string cleared = string.Empty;

            // cancelled password input
            if (string.IsNullOrEmpty(Credentials.LoginPassword))
            {
                return;
            }

            if (await ClearResults("scores"))
            {
                cleared = "scores";
            }

            if (await ClearResults("chits"))
            {
                if (!string.IsNullOrEmpty(cleared)) cleared += ", ";
                cleared += "chits";
            }

            //if (await ClearResults("pool"))
            //{
            //    if (!string.IsNullOrEmpty(cleared)) cleared += ", ";
            //    cleared += "pool";
            //}

            if (!string.IsNullOrEmpty(cleared))
            {
                MessageBox.Show("Cleared " + cleared + " results");
            }
            else
            {
                MessageBox.Show("None of the results were cleared");
            }
        }

        private async Task ClearGgResultsLink(object o)
        {
            string cleared = string.Empty;

            // cancelled password input
            if (string.IsNullOrEmpty(Credentials.LoginPassword))
            {
                return;
            }

            if (await ClearResults(GolfGeniusResultsLink))
            {
                cleared = "Golf Genius results link";
            }

            if (!string.IsNullOrEmpty(cleared))
            {
                MessageBox.Show("Cleared Golf Genius results link");
            }
            else
            {
                MessageBox.Show("Failed to clear Golf Genius results link");
            }
        }

        //private void CSVDay1PoolAdjust(object o)
        //{
        //    int poolIndex = int.Parse((string)o);

        //    if (UpdateEventWinnings(_csvDay1PoolKvp[poolIndex], ResultsPool))
        //    {
        //        CsvDay1PoolTotal[poolIndex] = "$" + GetWinningsTotal(_csvDay1PoolKvp[poolIndex], ResultsPool).ToString("F0") + " Day 1";
        //        SaveAdjustments(CsvDay1PoolFileName[poolIndex], _csvDay1PoolKvp[poolIndex]);
        //    }
        //}

        //private void CSVDay2PoolAdjust(object o)
        //{
        //    int poolIndex = int.Parse((string)o);

        //    if (UpdateEventWinnings(_csvDay2PoolKvp[poolIndex], ResultsPool))
        //    {
        //        CsvDay2PoolTotal[poolIndex] = "$" + GetWinningsTotal(_csvDay2PoolKvp[poolIndex], ResultsPool).ToString("F0") + " Day 2";
        //        SaveAdjustments(CsvDay2PoolFileName[poolIndex], _csvDay2PoolKvp[poolIndex]);
        //    }
        //}

        private void ChitsAdjust(object o)
        {
            if (UpdateEventWinnings(_kvpChitsList, ResultsChits))
            {
                ChitsTotal = "$" + GetWinningsTotal(_kvpChitsList, ResultsChits).ToString("F0") + " Total chits";
                SaveAdjustments(CsvChitsFileName, _kvpChitsList);
            }
        }

        private string GetAdjustedFileName(string csvFileName)
        {
            return Path.Combine(CSVFolderName, Path.ChangeExtension(csvFileName, ".adjusted"));
        }

        private void SaveAdjustments(string csvFileName, List<KeyValuePair<string, string>> kvpList)
        {
            string fullPath = GetAdjustedFileName(csvFileName);
            using (TextWriter tw = new StreamWriter(fullPath))
            {
                foreach (var keyValuePair in kvpList)
                {
                    tw.WriteLine(keyValuePair.Key + ",\"" + keyValuePair.Value + "\"");
                }
            }
        }

        private void LoadAdjustments(string csvFileName, List<KeyValuePair<string, string>> kvpList)
        {
            string fullPath = GetAdjustedFileName(csvFileName);

            if (File.Exists(fullPath))
            {
                using (TextReader tr = new StreamReader(fullPath))
                {
                    kvpList.Clear();
                    string[][] csvFileEntries = CSVParser.Parse(tr);

                    for (int row = 0; row < csvFileEntries.Length; row++)
                    {
                        if ((csvFileEntries[row] == null) || (csvFileEntries[row].Length == 0)) continue;

                        KeyValuePair<string, string> kvp = new KeyValuePair<string, string>(csvFileEntries[row][0], csvFileEntries[row][1]);
                        kvpList.Add(kvp);
                    }
                }
            }
        }

        private bool UpdateEventWinnings(List<KeyValuePair<string, string>> kvpList, string eventName)
        {
            // Convert key value pair list into list of EventWinningsWindow
            EventWinningsList = ConvertKvpToEventWinnings(kvpList, eventName);
            EventWinningsWindow eventWinningsWindow = new EventWinningsWindow();
            eventWinningsWindow.DataContext = this;
            eventWinningsWindow.Owner = App.Current.MainWindow;

            eventWinningsWindow.ShowDialog();
            if (eventWinningsWindow.DialogResult.HasValue && eventWinningsWindow.DialogResult.Value)
            {
                foreach(var pw in EventWinningsList)
                {
                    int winnings;
                    if(!int.TryParse(pw.Winnings.Trim(), out winnings))
                    {
                        MessageBox.Show("Invalid winnings for " + pw.Name + ": not an integer: '" + pw.Winnings + "'. Nothing was saved.");
                        return false;
                    }
                }

                // Replace all the entries in the list, so the ones with 0 winnings can be removed
                int index = 0;
                kvpList.Clear();
                foreach (var pw in EventWinningsList)
                {
                    int winnings = int.Parse(pw.Winnings.Trim());
                    if(winnings > 0)
                    {
                        kvpList.Add(new KeyValuePair<string, string>(string.Format("{0}[{1}][Winnings]", eventName, index), winnings.ToString(CultureInfo.InvariantCulture)));
                        kvpList.Add(new KeyValuePair<string, string>(string.Format("{0}[{1}][Flight]", eventName, index), pw.Flight));
                        kvpList.Add(new KeyValuePair<string, string>(string.Format("{0}[{1}][Date]", eventName, index), pw.Date));
                        kvpList.Add(new KeyValuePair<string, string>(string.Format("{0}[{1}][Place]", eventName, index), pw.Place));
                        kvpList.Add(new KeyValuePair<string, string>(string.Format("{0}[{1}][Score]", eventName, index), pw.Score));
                        kvpList.Add(new KeyValuePair<string, string>(string.Format("{0}[{1}][TeamNumber]", eventName, index), pw.TeamNumber));
                        kvpList.Add(new KeyValuePair<string, string>(string.Format("{0}[{1}][Name]", eventName, index), pw.Name));
                        kvpList.Add(new KeyValuePair<string, string>(string.Format("{0}[{1}][GHIN]", eventName, index), pw.GHIN));
                        if (string.CompareOrdinal(eventName, ResultsPool) == 0)
                        {
                            kvpList.Add(new KeyValuePair<string, string>(
                                string.Format("{0}[{1}][Hole]", eventName, index), pw.Hole));
                        }
                        if (string.CompareOrdinal(eventName, ResultsChits) == 0)
                        {
                            kvpList.Add(new KeyValuePair<string, string>(
                                string.Format("{0}[{1}][FlightName]", eventName, index), pw.FlightName));
                        }
                        index++;
                    }
                }

                return true;
            }

            return false;
        }

        private ObservableCollection<EventWinnings> ConvertKvpToEventWinnings(List<KeyValuePair<string, string>> kvp, string eventName)
        {
            ObservableCollection<EventWinnings> winnings = new ObservableCollection<EventWinnings>();

            // Convert the list to a dictionary for easier lookup
            Dictionary<string, string> winningsDict = new Dictionary<string, string>();
            foreach(var entry in kvp)
            {
                winningsDict.Add(entry.Key, entry.Value);
            }

            bool indexExists = true;
            for (int index = 0; indexExists; index++ )
            {
                string key = string.Format("{0}[{1}][Flight]", eventName, index);
                if (winningsDict.ContainsKey(key))
                {
                    EventWinnings eventWinnings = new EventWinnings();
                    eventWinnings.Index = index;
                    eventWinnings.Flight = winningsDict[string.Format("{0}[{1}][Flight]", eventName, index)];
                    eventWinnings.Date = winningsDict[string.Format("{0}[{1}][Date]", eventName, index)];
                    eventWinnings.Place = winningsDict[string.Format("{0}[{1}][Place]", eventName, index)];
                    eventWinnings.Score = winningsDict[string.Format("{0}[{1}][Score]", eventName, index)];
                    eventWinnings.Winnings = winningsDict[string.Format("{0}[{1}][Winnings]", eventName, index)];
                    eventWinnings.TeamNumber = winningsDict[string.Format("{0}[{1}][TeamNumber]", eventName, index)];
                    eventWinnings.Name = winningsDict[string.Format("{0}[{1}][Name]", eventName, index)];
                    eventWinnings.GHIN = winningsDict[string.Format("{0}[{1}][GHIN]", eventName, index)];

                    // Pool will have "Hole" entry
                    string hole = string.Format("{0}[{1}][Hole]", eventName, index);
                    eventWinnings.Hole = winningsDict.ContainsKey(hole) ? winningsDict[hole] : "0";

                    string flightName = string.Format("{0}[{1}][FlightName]", eventName, index);
                    eventWinnings.FlightName = winningsDict.ContainsKey(flightName) ? winningsDict[flightName] : "Flight " + eventWinnings.Flight;

                    // If skins, then the hole will be > 0
                    eventWinnings.PlaceOrHole = (int.Parse(eventWinnings.Hole) > 0) ? eventWinnings.Hole : eventWinnings.Place;

                    winnings.Add(eventWinnings);
                }
                else
                {
                    indexExists = false;
                }
            }

            return winnings;
        }
    }
}
