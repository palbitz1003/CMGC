using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Security.RightsManagement;
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

        private Visibility _getTournamentsVisible;
        public Visibility GetTournamentsVisible { get { return _getTournamentsVisible; } set { _getTournamentsVisible = value; OnPropertyChanged("GetTournamentsVisible"); } }

        private Visibility _gotTournamentsVisible;
        public Visibility GotTournamentsVisible { get { return _gotTournamentsVisible; } set { _gotTournamentsVisible = value; OnPropertyChanged("GotTournamentsVisible"); } }

        private bool _updateTournamentAllowed;
        public bool UpdateTournamentsAllowed { get { return _updateTournamentAllowed; } set { _updateTournamentAllowed = value; OnPropertyChanged("UpdateTournamentsAllowed"); } }

        private Visibility _is2DayTournament;
        public Visibility Is2DayTournament { get { return _is2DayTournament; } set { _is2DayTournament = value; OnPropertyChanged("Is2DayTournament"); } }

        private int _tournamentNameIndex;
        public int TournamentNameIndex
        {
            get { return _tournamentNameIndex; }
            set
            {
                if (_tournamentNameIndex != value)
                {
                    _tournamentNameIndex = value;
                    OnPropertyChanged("TournamentNameIndex");
                    if (value >= 0)
                    {
                        UpdateTournamentsAllowed = true;
                        Is2DayTournament = (TournamentNames[value].StartDate != TournamentNames[value].EndDate) 
                                                    ? Visibility.Visible : Visibility.Collapsed;
                        IsEclectic = TournamentNames[value].IsEclectic;
                        IsMatchPlay = TournamentNames[value].IsMatchPlay;
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
            set { _tournamentNames = value; OnPropertyChanged("TournamentNames"); }
        }

        private TrulyObservableCollection<ClosestToThePin> _closestToThePinsDay1;
        public TrulyObservableCollection<ClosestToThePin> ClosestToThePinsDay1
        {
            get { return _closestToThePinsDay1; }
            set { _closestToThePinsDay1 = value; OnPropertyChanged("ClosestToThePinsDay1"); }
        }

        private TrulyObservableCollection<ClosestToThePin> _closestToThePinsDay2;
        public TrulyObservableCollection<ClosestToThePin> ClosestToThePinsDay2
        {
            get { return _closestToThePinsDay2; }
            set { _closestToThePinsDay2 = value; OnPropertyChanged("ClosestToThePinsDay2"); }
        }

        private string _htmlScoresFileName;
        public string HtmlScoresFileName { get { return _htmlScoresFileName; } set { _htmlScoresFileName = value; OnPropertyChanged("HtmlScoresFileName"); } }

        private string _htmlChitsFileName;
        public string HtmlChitsFileName { get { return _htmlChitsFileName; } set { _htmlChitsFileName = value; OnPropertyChanged("HtmlChitsFileName"); } }

        private string _htmlPoolFileName;
        public string HtmlPoolFileName { get { return _htmlPoolFileName; } set { _htmlPoolFileName = value; OnPropertyChanged("HtmlPoolFileName"); } }

        private string _csvFolderName;
        public string CSVFolderName {  get { return _csvFolderName;} set { _csvFolderName = value; OnPropertyChanged("CSVFolderName");} }

        private string _csvScoresFileName;
        public string CsvScoresFileName { get { return _csvScoresFileName; } set { _csvScoresFileName = value; OnPropertyChanged("CsvScoresFileName"); } }

        private string _csvChitsFileName;
        public string CsvChitsFileName { get { return _csvChitsFileName; } set { _csvChitsFileName = value; OnPropertyChanged("CsvChitsFileName"); } }

        private List<KeyValuePair<string, string>>[] _csvDay1PoolKvp;

        private ObservableCollection<string> _csvDay1PoolFileName;
        public ObservableCollection<string> CsvDay1PoolFileName
        {
            get { return _csvDay1PoolFileName; }
            set { _csvDay1PoolFileName = value; OnPropertyChanged("CsvDay1PoolFileName"); }
        }

        private List<KeyValuePair<string, string>>[] _csvDay2PoolKvp;

        private ObservableCollection<string> _csvDay2PoolFileName;
        public ObservableCollection<string> CsvDay2PoolFileName
        {
            get { return _csvDay2PoolFileName; }
            set { _csvDay2PoolFileName = value; OnPropertyChanged("CsvDay2PoolFileName"); }
        }

        private ObservableCollection<string> _csvDay1PoolTotal;
        public ObservableCollection<string> CsvDay1PoolTotal
        {
            get { return _csvDay1PoolTotal; }
            set { _csvDay1PoolTotal = value; OnPropertyChanged("CsvDay1PoolTotal"); }
        }

        private ObservableCollection<string> _csvDay2PoolTotal;
        public ObservableCollection<string> CsvDay2PoolTotal
        {
            get { return _csvDay2PoolTotal; }
            set { _csvDay2PoolTotal = value; OnPropertyChanged("CsvDay2PoolTotal"); }
        }

        private bool _csvExpanded;
        public bool CsvExpanded { get { return _csvExpanded; } set { _csvExpanded = value; OnPropertyChanged("CsvExpanded"); } }

        private int _poolTotal;
        public int PoolTotal { get { return _poolTotal; } set { _poolTotal = value; OnPropertyChanged("PoolTotal"); } }

        private int _chitsTotal;
        public int ChitsTotal { get { return _chitsTotal; } set { _chitsTotal = value; OnPropertyChanged("ChitsTotal"); } }

        private ObservableCollection<PoolWinnings> _poolWinnings;
        public ObservableCollection<PoolWinnings> PoolWinningsList { get { return _poolWinnings; } set { _poolWinnings = value; OnPropertyChanged("PoolWinningsList"); } }

        private bool _isEclectic;
        public bool IsEclectic { get { return _isEclectic; } set { _isEclectic = value; OnPropertyChanged("IsEclectic"); ResetFileNames(); } }

        private bool _isMatchPlay;
        public bool IsMatchPlay { get { return _isMatchPlay; } set { _isMatchPlay = value; OnPropertyChanged("IsMatchPlay"); } }

        #region Commands
        public ICommand GetTournamentsCommand { get { return new ModelCommand(s => GetTournaments(s)); } }
        public ICommand SubmitClosestToThePinCommand { get { return new ModelCommand(s => SubmitClosestToThePin(s)); } }
        public ICommand ClearClosestToThePinCommand { get { return new ModelCommand(s => ClearClosestToThePin(s)); } }

        public ICommand SubmitHtmlScoresCommand { get { return new ModelCommand(s => SubmitHtmlScores(s)); } }
        public ICommand SubmitHtmlChitsCommand { get { return new ModelCommand(s => SubmitHtmlChits(s)); } }
        public ICommand SubmitHtmlPoolCommand { get { return new ModelCommand(s => SubmitHtmlPool(s)); } }

        public ICommand ClearHtmlScoresCommand { get { return new ModelCommand(s => ClearHtmlScores(s)); } }
        public ICommand ClearHtmlChitsCommand { get { return new ModelCommand(s => ClearHtmlChits(s)); } }
        public ICommand ClearHtmlPoolCommand { get { return new ModelCommand(s => ClearHtmlPool(s)); } }

        public ICommand SubmitCsvCommand { get { return new ModelCommand(s => SubmitCsv(s)); } }
        public ICommand ClearCsvCommand { get { return new ModelCommand(s => ClearCsv(s)); } }

        public ICommand CSVDay1PoolAdjustCommand { get { return new ModelCommand(s => CSVDay1PoolAdjust(s)); } }
        public ICommand CSVDay2PoolAdjustCommand { get { return new ModelCommand(s => CSVDay2PoolAdjust(s)); } }
        #endregion

        public ResultsTabViewModel()
        {
            TournamentNames = new TrulyObservableCollection<TournamentName>();
            GetTournamentsVisible = Visibility.Visible;
            GotTournamentsVisible = Visibility.Collapsed;
            Is2DayTournament = Visibility.Collapsed;
            TournamentNameIndex = -1;
            CsvExpanded = true;

            CreateEmptyClosestToThePin();
            CsvDay1PoolFileName = new ObservableCollection<string>();
            CsvDay2PoolFileName = new ObservableCollection<string>();
            CsvDay1PoolTotal = new ObservableCollection<string>();
            CsvDay2PoolTotal = new ObservableCollection<string>();

            for(int i = 0; i < 4; i++)
            {
                CsvDay1PoolFileName.Add(string.Empty);
                CsvDay2PoolFileName.Add(string.Empty);

                CsvDay1PoolTotal.Add("$0 Day 1");
                CsvDay2PoolTotal.Add("$0 Day 2");
            }

            _csvDay1PoolKvp = new List<KeyValuePair<string, string>>[4];
            _csvDay2PoolKvp = new List<KeyValuePair<string, string>>[4];

            CsvDay1PoolFileName.CollectionChanged += CsvDay1PoolFileName_CollectionChanged;
            CsvDay2PoolFileName.CollectionChanged += CsvDay2PoolFileName_CollectionChanged;
        }

        private void ResetFileNames()
        {
            CSVFolderName = string.Empty;
            CsvScoresFileName = string.Empty;
            CsvChitsFileName = string.Empty;

            if (CsvDay1PoolFileName != null)
            {
                for (int i = 0; i < CsvDay1PoolFileName.Count; i++)
                {
                    CsvDay1PoolFileName[i] = string.Empty;
                    CsvDay2PoolFileName[i] = string.Empty;
                }
            }
        }

        void CsvDay1PoolFileName_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            int fileIndex = e.NewStartingIndex;
            _csvDay1PoolKvp[fileIndex] = new List<KeyValuePair<string, string>>();

            if (!string.IsNullOrEmpty(CsvDay1PoolFileName[fileIndex]))
            {
                AddPoolEntries(CsvDay1PoolFileName[fileIndex], fileIndex + 1, 0, _csvDay1PoolKvp[fileIndex]);
                LoadAdjustments(CsvDay1PoolFileName[fileIndex], _csvDay1PoolKvp[fileIndex]);
            }
            CsvDay1PoolTotal[fileIndex] = "$" + GetWinningsTotal(_csvDay1PoolKvp[fileIndex]).ToString("F0") + " Day 1";
        }

        void CsvDay2PoolFileName_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            int fileIndex = e.NewStartingIndex;
            _csvDay2PoolKvp[fileIndex] = new List<KeyValuePair<string, string>>();

            if (!string.IsNullOrEmpty(CsvDay2PoolFileName[fileIndex]))
            {
                AddPoolEntries(CsvDay2PoolFileName[fileIndex], fileIndex + 1, 1, _csvDay2PoolKvp[fileIndex]);
                LoadAdjustments(CsvDay2PoolFileName[fileIndex], _csvDay2PoolKvp[fileIndex]);
            }
            CsvDay2PoolTotal[fileIndex] = "$" + GetWinningsTotal(_csvDay2PoolKvp[fileIndex]).ToString("F0") + " Day 2";

        }

        private float GetWinningsTotal(List<KeyValuePair<string, string>> kvpPoolList)
        {
            float winnings = 0;
            bool indexFound = true;
            for (int index = 0; (index < kvpPoolList.Count) && indexFound; index++)
            {
                string key = string.Format("ResultsPool[{0}][Winnings]", index);

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

        private async void GetTournaments(object o)
        {
            string responseString = await GetTournamentNames();

            LoadTournamentNamesFromWebResponse(responseString, TournamentNames, false);
        }

        protected override void OnTournamentsUpdated()
        {
            TournamentNameIndex = -1;

            // Grab the last tournament that has completed
            for (int i = TournamentNames.Count - 1; i >= 0; i--)
            {
                if (DateTime.Now >= TournamentNames[i].EndDate)
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

        private async void LoadClosestToThePinFromWeb()
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

        private async void SubmitClosestToThePin(object o)
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

        private async void ClearClosestToThePin(object o)
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

        private async void SubmitResultsHtml(string file, string result)
        {
            if (string.IsNullOrEmpty(file))
            {
                MessageBox.Show("Please fill in the name of the " + result + " file", "Error");
                return;
            }

            if (!File.Exists(file))
            {
                MessageBox.Show("File does not exist: " + file, "Error");
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

                using (var multipartFormDataContent = new MultipartFormDataContent())
                {
                    var values = new[]
                            {
                                new KeyValuePair<string, string>("Login", Credentials.LoginName),
                                new KeyValuePair<string, string>("Password", Credentials.LoginPassword),
                                new KeyValuePair<string, string>("TournamentKey", TournamentNames[TournamentNameIndex].TournamentKey.ToString()),
                                new KeyValuePair<string, string>("Action", "Submit"),
                                new KeyValuePair<string, string>("Result", result)
                            };

                    foreach (var keyValuePair in values)
                    {
                        multipartFormDataContent.Add(new StringContent(keyValuePair.Value),
                            String.Format("\"{0}\"", keyValuePair.Key));
                    }

                    multipartFormDataContent.Add(new ByteArrayContent(File.ReadAllBytes(file)),
                        '"' + "file" + '"',
                        '"' + file + '"');

                    var requestUri = WebAddresses.ScriptFolder + WebAddresses.SubmitResultsHtml;

                    string responseString;
                    using (new WaitCursor())
                    {
                        var response = client.PostAsync(requestUri, multipartFormDataContent).Result;
                        responseString = await response.Content.ReadAsStringAsync();
                        System.Diagnostics.Debug.WriteLine(responseString);
                    }

                    if (responseString.StartsWith("Success", StringComparison.InvariantCultureIgnoreCase))
                    {
                        MessageBox.Show("Submitted " + result + " results");
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
        }

        private void SubmitHtmlScores(object o)
        {
            SubmitResultsHtml(HtmlScoresFileName, "scores");
        }

        private  void SubmitHtmlChits(object o)
        {
            SubmitResultsHtml(HtmlChitsFileName, "chits");
        }

        private  void SubmitHtmlPool(object o)
        {
            SubmitResultsHtml(HtmlPoolFileName, "pool");
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

        private async void ClearHtmlScores(object o)
        {
            if(await ClearResults("scores"))
            {
                MessageBox.Show("Cleared scores results");
            }
        }

        private async void ClearHtmlChits(object o)
        {
            if (await ClearResults("chits"))
            {
                MessageBox.Show("Cleared chits results");
            }
        }

        private async void ClearHtmlPool(object o)
        {
            if (await ClearResults("pool"))
            {
                MessageBox.Show("Cleared pool results");
            }
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

        private void AddPoolEntries(string file, int flight, int day, List<KeyValuePair<string, string>> kvpList)
        {
            string fullPath = Path.Combine(CSVFolderName, file);
            if(!File.Exists(fullPath))
            {
                throw new ApplicationException("File doesn't exist: " + fullPath);
            }

            int index = 0;
            foreach(var kvp in kvpList)
            {
                string key = string.Format("ResultsPool[{0}][Flight]", index);
                if(kvp.Key == key)
                {
                    index++;
                }
            }
            using(TextReader tr = new StreamReader(fullPath))
            {
                string[][] lines = CSVParser.Parse(tr);
                foreach(var line in lines)
                {
                    if (line.Length > 0)
                    {
                        if ((line.Length != 6) && (line.Length != 15))
                        {
                            throw new ApplicationException(file + ": does not have 6 fields (total score) or 15 fields (skins): " + string.Join(", ", line));
                        }

                        if (line.Length == 6)
                        {
                            kvpList.Add(new KeyValuePair<string, string>(
                                string.Format("ResultsPool[{0}][Flight]", index), flight.ToString()));

                            // since these are not skins, set the hole number to 0
                            kvpList.Add(new KeyValuePair<string, string>(
                                string.Format("ResultsPool[{0}][Hole]", index), "0"));

                            DateTime dt;
                            if (TournamentNameIndex >= 0)
                            {
                                dt = TournamentNames[TournamentNameIndex].StartDate.AddDays(day);
                            }
                            else if (!DateTime.TryParse(line[0], out dt))
                            {
                                throw new ArgumentException(file + ": invalid date: " + lines[0]);
                            }

                            kvpList.Add(new KeyValuePair<string, string>(
                                string.Format("ResultsPool[{0}][Date]", index), dt.ToString("yyyy-MM-dd")));

                            kvpList.Add(new KeyValuePair<string, string>(
                                string.Format("ResultsPool[{0}][Place]", index), line[2]));

                            kvpList.Add(new KeyValuePair<string, string>(
                                string.Format("ResultsPool[{0}][Score]", index), line[4]));

                            float winnings;
                            if (!float.TryParse(line[5], out winnings))
                            {
                                throw new ArgumentException(file + ": winnings must be a decimal number: " + line[5]);
                            }
                            // round to a multiple of 5
                            int w = ((int)((winnings + 2.5f) / 5f)) * 5;

                            kvpList.Add(new KeyValuePair<string, string>(
                                string.Format("ResultsPool[{0}][Winnings]", index), w.ToString()));

                            // field 3 looks like "(1 )  Albitz, Paul , 9079663"
                            int paren = line[3].IndexOf(')');
                            if (paren < 0)
                            {
                                throw new ArgumentException(file + ": expected team number within parentheses: " + line[3]);
                            }
                            string team = line[3].Substring(0, paren);
                            team = team.Trim('(');
                            string rest = line[3].Substring(paren + 1);
                            string[] fields = rest.Split(',');

                            if (fields.Length != 3)
                            {
                                throw new ArgumentException(file + ": expected 3 fields for last name, first name, GHIN: " + rest);
                            }

                            kvpList.Add(new KeyValuePair<string, string>(
                                string.Format("ResultsPool[{0}][TeamNumber]", index), team.Trim()));

                            kvpList.Add(new KeyValuePair<string, string>(
                                string.Format("ResultsPool[{0}][Name]", index), fields[0].Trim() + ", " + fields[1].Trim()));

                            kvpList.Add(new KeyValuePair<string, string>(
                                string.Format("ResultsPool[{0}][GHIN]", index), fields[2].Trim()));
                        }

                        else // skins files have 15 fields
                        {
                            kvpList.Add(new KeyValuePair<string, string>(
                                    string.Format("ResultsPool[{0}][Flight]", index), flight.ToString()));

                            // with skins, set the Place field to 0
                            kvpList.Add(new KeyValuePair<string, string>(
                                string.Format("ResultsPool[{0}][Place]", index), "0"));

                            DateTime dt;
                            if (TournamentNameIndex >= 0)
                            {
                                dt = TournamentNames[TournamentNameIndex].StartDate.AddDays(day);
                            }
                            else if (!DateTime.TryParse(line[1], out dt))
                            {
                                throw new ArgumentException(file + ": invalid date: " + lines[1]);
                            }

                            kvpList.Add(new KeyValuePair<string, string>(
                                string.Format("ResultsPool[{0}][Date]", index), dt.ToString("yyyy-MM-dd")));

                            kvpList.Add(new KeyValuePair<string, string>(
                                string.Format("ResultsPool[{0}][Hole]", index), line[9]));

                            kvpList.Add(new KeyValuePair<string, string>(
                                string.Format("ResultsPool[{0}][Score]", index), line[10]));

                            float winnings;
                            if (!float.TryParse(line[11], out winnings))
                            {
                                throw new ArgumentException(file + ": winnings must be a decimal number: " + line[11]);
                            }
                            // round to a multiple of 5
                            int w = ((int)((winnings + 2.5f) / 5f)) * 5;

                            kvpList.Add(new KeyValuePair<string, string>(
                                string.Format("ResultsPool[{0}][Winnings]", index), w.ToString()));

                            kvpList.Add(new KeyValuePair<string, string>(
                                string.Format("ResultsPool[{0}][TeamNumber]", index), "0"));

                            kvpList.Add(new KeyValuePair<string, string>(
                                string.Format("ResultsPool[{0}][Name]", index), line[8]));

                            GHINEntry gi = GHINEntry.FindName(GHINEntries, line[8]);
                            int ghinNumber = 0;

                            if (gi != null)
                            {
                                ghinNumber = gi.GHIN;

                            }

                            kvpList.Add(new KeyValuePair<string, string>(
                                string.Format("ResultsPool[{0}][GHIN]", index), ghinNumber.ToString()));
                        }
                        index++;
                    }
                }
            }
        }

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

                        DateTime dt;
                        if (!DateTime.TryParse(line[0], out dt))
                        {
                            throw new ArgumentException(file + ": invalid date: " + line[0]);
                        }

                        kvpList.Add(new KeyValuePair<string, string>(
                            string.Format("ResultsChits[{0}][Date]", index), dt.ToString("yyyy-MM-dd")));

                        kvpList.Add(new KeyValuePair<string, string>(
                            string.Format("ResultsChits[{0}][Place]", index), line[3]));

                        kvpList.Add(new KeyValuePair<string, string>(
                            string.Format("ResultsChits[{0}][Score]", index), line[5]));

                        float winnings;
                        if (!float.TryParse(line[6], out winnings))
                        {
                            throw new ArgumentException(file + ": winnings must be a decimal number: " + line[6]);
                        }
                        // round to a multiple of 5
                        int w = (int)Math.Round(winnings);

                        kvpList.Add(new KeyValuePair<string, string>(
                            string.Format("ResultsChits[{0}][Winnings]", index), w.ToString()));

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
                                string.Format("ResultsChits[{0}][FlightName]", index), "Flight " + currentFlight));
                        }
                        else
                        {
                            kvpList.Add(new KeyValuePair<string, string>(
                                string.Format("ResultsChits[{0}][FlightName]", index), currentFlight));
                        }

                        kvpList.Add(new KeyValuePair<string, string>(
                            string.Format("ResultsChits[{0}][Flight]", index), currentFlightIndex.ToString()));

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
                            string.Format("ResultsChits[{0}][TeamNumber]", index), team.Trim()));

                        kvpList.Add(new KeyValuePair<string, string>(
                            string.Format("ResultsChits[{0}][Name]", index), fields[0].Trim() + ", " + fields[1].Trim()));

                        kvpList.Add(new KeyValuePair<string, string>(
                            string.Format("ResultsChits[{0}][GHIN]", index), fields[2].Trim()));

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
                            string.Format("MatchPlayResultsScores[{0}][Round]", index), line[0]));
                        kvpList.Add(new KeyValuePair<string, string>(
                            string.Format("MatchPlayResultsScores[{0}][MatchNumber]", index), line[1]));
                        kvpList.Add(new KeyValuePair<string, string>(
                            string.Format("MatchPlayResultsScores[{0}][Player1]", index), line[2]));
                        kvpList.Add(new KeyValuePair<string, string>(
                            string.Format("MatchPlayResultsScores[{0}][Player2]", index), line[3]));
                        index++;
                    }
                }
            }

            return masterList;
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
        

        private void MergeKvp(List<KeyValuePair<string, string>> mergedList, List<KeyValuePair<string, string>> list)
        {
            int lastIndex = 0;
            foreach (var kvp in mergedList)
            {
                string key = string.Format("ResultsPool[{0}][Flight]", lastIndex);
                if (kvp.Key == key)
                {
                    lastIndex++;
                }
            }

            if(lastIndex == 0)
            {
                mergedList.AddRange(list);
                return;
            }

            foreach (var kvp in list)
            {
                int startBracket = "ResultsPool[".Length;
                int endBracket = kvp.Key.IndexOf("][");
                string s = kvp.Key.Substring(startBracket, endBracket - startBracket);
                int oldIndex = int.Parse(s);

                KeyValuePair<string, string> newKvp;
                string newKey = kvp.Key.Replace(kvp.Key.Substring(0, endBracket), "ResultsPool[" + (oldIndex + lastIndex));
                newKvp = new KeyValuePair<string, string>(newKey, kvp.Value);
                mergedList.Add(newKvp);
            }
        }

        private async void SubmitCsv(object o)
        {
            List<KeyValuePair<string, string>> kvpPoolList = new List<KeyValuePair<string, string>>();

            foreach (var kvp in _csvDay1PoolKvp)
            {
                if (kvp != null)
                {
                    MergeKvp(kvpPoolList, kvp);
                }
            }

            foreach (var kvp in _csvDay2PoolKvp)
            {
                if (kvp != null)
                {
                    MergeKvp(kvpPoolList, kvp);
                }
            }

            List<KeyValuePair<string, string>> kvpChitsList = new List<KeyValuePair<string, string>>();
            AddChitsEntries(CsvChitsFileName, kvpChitsList);

            List<List<KeyValuePair<string, string>>> scoresListList = IsMatchPlay 
                ? AddMatchPlayEntries(CsvScoresFileName) 
                : AddScoresEntries(CsvScoresFileName);

            string submitted = string.Empty;

            // cancelled password input
            if (string.IsNullOrEmpty(Credentials.LoginPassword))
            {
                return;
            }

            if (kvpPoolList.Count > 0)
            {
                if (!await SubmitResultsCsv(kvpPoolList, "pool", true))
                {
                    MessageBox.Show("Failed to submit pool results.  Did not try to upload chits or scores.");
                    return;
                }

                if (!string.IsNullOrEmpty(submitted)) submitted += ", ";
                submitted += "pool";
            }

            if (kvpChitsList.Count > 0)
            {
                if (!await SubmitResultsCsv(kvpChitsList, "chits", true))
                {
                    MessageBox.Show("Failed to submit chits results.  Did not try to upload scores.");
                    return;
                }

                if (!string.IsNullOrEmpty(submitted)) submitted += ", ";
                submitted += "chits";
            }

            if (scoresListList != null)
            {
                foreach (var kvpScoresList in scoresListList)
                {
                    if (!await SubmitResultsCsv(kvpScoresList, IsMatchPlay ? "match play scores" : "scores", kvpScoresList == scoresListList[0]))
                    {
                        MessageBox.Show("Failed to submit scores results.");
                        return;
                    }
                }

                if (!string.IsNullOrEmpty(submitted)) submitted += ", ";
                submitted += "scores";
            }

            MessageBox.Show("Submitted results for " + submitted);
        }

        private async void ClearCsv(object o)
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

            if (await ClearResults("pool"))
            {
                if (!string.IsNullOrEmpty(cleared)) cleared += ", ";
                cleared += "pool";
            }

            if (!string.IsNullOrEmpty(cleared))
            {
                MessageBox.Show("Cleared " + cleared + " results");
            }
            else
            {
                MessageBox.Show("None of the results were cleared");
            }
        }

        private void CSVDay1PoolAdjust(object o)
        {
            int poolIndex = int.Parse((string)o);

            if (UpdatePool(_csvDay1PoolKvp[poolIndex]))
            {
                CsvDay1PoolTotal[poolIndex] = "$" + GetWinningsTotal(_csvDay1PoolKvp[poolIndex]).ToString("F0") + " Day 1";
                SaveAdjustments(CsvDay1PoolFileName[poolIndex], _csvDay1PoolKvp[poolIndex]);
            }
        }

        private void CSVDay2PoolAdjust(object o)
        {
            int poolIndex = int.Parse((string)o);

            if (UpdatePool(_csvDay2PoolKvp[poolIndex]))
            {
                CsvDay2PoolTotal[poolIndex] = "$" + GetWinningsTotal(_csvDay2PoolKvp[poolIndex]).ToString("F0") + " Day 2";
                SaveAdjustments(CsvDay2PoolFileName[poolIndex], _csvDay2PoolKvp[poolIndex]);
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

        private bool UpdatePool(List<KeyValuePair<string, string>> kvpList)
        {
            // Convert key value pair list into list of PoolWinningsWindow
            PoolWinningsList = ConvertKvpToPoolWinnings(kvpList);
            PoolWinningsWindow pww = new PoolWinningsWindow();
            pww.DataContext = this;

            pww.ShowDialog();
            if (pww.DialogResult.HasValue && pww.DialogResult.Value)
            {
                foreach(var pw in PoolWinningsList)
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
                foreach (var pw in PoolWinningsList)
                {
                    int winnings = int.Parse(pw.Winnings.Trim());
                    if(winnings > 0)
                    {
                        kvpList.Add(new KeyValuePair<string, string>(string.Format("ResultsPool[{0}][Winnings]", index), winnings.ToString()));
                        kvpList.Add(new KeyValuePair<string, string>(string.Format("ResultsPool[{0}][Flight]", index), pw.Flight));
                        kvpList.Add(new KeyValuePair<string, string>(string.Format("ResultsPool[{0}][Date]", index), pw.Date));
                        kvpList.Add(new KeyValuePair<string, string>(string.Format("ResultsPool[{0}][Place]", index), pw.Place));
                        kvpList.Add(new KeyValuePair<string, string>(string.Format("ResultsPool[{0}][Score]", index), pw.Score));
                        kvpList.Add(new KeyValuePair<string, string>(string.Format("ResultsPool[{0}][TeamNumber]", index), pw.TeamNumber));
                        kvpList.Add(new KeyValuePair<string, string>(string.Format("ResultsPool[{0}][Name]", index), pw.Name));
                        kvpList.Add(new KeyValuePair<string, string>(string.Format("ResultsPool[{0}][GHIN]", index), pw.GHIN));
                        kvpList.Add(new KeyValuePair<string, string>(string.Format("ResultsPool[{0}][Hole]", index), pw.Hole));
                        index++;
                    }
                }

                return true;
            }

            return false;
        }

        private ObservableCollection<PoolWinnings> ConvertKvpToPoolWinnings(List<KeyValuePair<string, string>> kvp)
        {
            ObservableCollection<PoolWinnings> winnings = new ObservableCollection<PoolWinnings>();

            // Convert the list to a dictionary for easier lookup
            Dictionary<string, string> winningsDict = new Dictionary<string, string>();
            foreach(var entry in kvp)
            {
                winningsDict.Add(entry.Key, entry.Value);
            }

            bool indexExists = true;
            for (int index = 0; indexExists; index++ )
            {
                string key = string.Format("ResultsPool[{0}][Flight]", index);
                if (winningsDict.ContainsKey(key))
                {
                    PoolWinnings pw = new PoolWinnings();
                    pw.Index = index;
                    pw.Flight = winningsDict[string.Format("ResultsPool[{0}][Flight]", index)];
                    pw.Date = winningsDict[string.Format("ResultsPool[{0}][Date]", index)];
                    pw.Place = winningsDict[string.Format("ResultsPool[{0}][Place]", index)];
                    pw.Score = winningsDict[string.Format("ResultsPool[{0}][Score]", index)];
                    pw.Winnings = winningsDict[string.Format("ResultsPool[{0}][Winnings]", index)];
                    pw.TeamNumber = winningsDict[string.Format("ResultsPool[{0}][TeamNumber]", index)];
                    pw.Name = winningsDict[string.Format("ResultsPool[{0}][Name]", index)];
                    pw.GHIN = winningsDict[string.Format("ResultsPool[{0}][GHIN]", index)];
                    pw.Hole = winningsDict[string.Format("ResultsPool[{0}][Hole]", index)];

                    // If skins, then the hole will be > 0
                    pw.PlaceOrHole = (int.Parse(pw.Hole) > 0) ? pw.Hole : pw.Place;


                    winnings.Add(pw);
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
