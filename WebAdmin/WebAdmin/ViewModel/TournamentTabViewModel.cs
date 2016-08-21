using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Input;
using System.Windows;
using System.IO;
using WebAdmin.View;

namespace WebAdmin.ViewModel
{
    public class TournamentTabViewModel : TabViewModelBase
    {

        #region Properties

        public override string Header
        {
            get { return "Tournament"; }
        }

        private bool _showAllTournamentNames;
        public bool ShowAllTournamentNames
        {
            get { return _showAllTournamentNames; }
            set 
            { 
                _showAllTournamentNames = value; 
                OnPropertyChanged();
                ShowTournamentNames();
            }
        }

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
                    TournamentSelectionChanged(value);
                }
            }
        }

        private TrulyObservableCollection<TournamentName> _allTournamentNames;
        private TrulyObservableCollection<TournamentName> _tournamentNames;

        public TrulyObservableCollection<TournamentName> TournamentNames
        {
            get { return _tournamentNames; }
            set
            {
                _tournamentNames = value;
                OnPropertyChanged();
            }
        }

        private Tournament _tournament;

        public Tournament Tournament
        {
            get { return _tournament; }
            set
            {
                _tournament = value;
                OnPropertyChanged();
            }
        }

        private int _tournamentDescriptionNameIndex;

        public int TournamentDescriptionNameIndex
        {
            get { return _tournamentDescriptionNameIndex; }
            set
            {
                if (_tournamentDescriptionNameIndex != value)
                {
                    _tournamentDescriptionNameIndex = value;
                    OnPropertyChanged();
                    TournamentDescriptionSelectionChanged(value);
                }
            }
        }

        private TrulyObservableCollection<TournamentDescription> _tournamentDescriptionNames;

        public TrulyObservableCollection<TournamentDescription> TournamentDescriptionNames
        {
            get { return _tournamentDescriptionNames; }
            set
            {
                _tournamentDescriptionNames = value;
                OnPropertyChanged();
            }
        }

        private TournamentDescription _tournamentDescription;

        public TournamentDescription TournamentDescription
        {
            get { return _tournamentDescription; }
            set
            {
                _tournamentDescription = value;
                OnPropertyChanged();
            }
        }

        private TrulyObservableCollection<TournamentChairman> _tournamentChairmen;

        public TrulyObservableCollection<TournamentChairman> TournamentChairmen
        {
            get { return _tournamentChairmen; }
            set
            {
                _tournamentChairmen = value;
                OnPropertyChanged();
            }
        }

        private int _tournamentChairmenSelectedIndex;
        public int TournamentChairmenSelectedIndex
        {
            get { return _tournamentChairmenSelectedIndex; }
            set
            {
                _tournamentChairmenSelectedIndex = value;
                UpdateChairmanFields();
                OnPropertyChanged();
            }
        }

        #endregion

        #region Commands
        public ICommand GetTournamentsCommand { get { return new ModelCommand(async s => await GetTournaments(s)); } }
        public ICommand AddTournamentsCommand { get { return new ModelCommand(async s => await AddTournament(s)); } }
        public ICommand UpdateTournamentsCommand { get { return new ModelCommand(async s => await UpdateTournament(s)); } }
        public ICommand DeleteTournamentsCommand { get { return new ModelCommand(async s => await DeleteTournament(s)); } }
        public ICommand GetTournamentDescriptionCommand { get { return new ModelCommand(s => GetTournamentDescriptions(s)); } }

        #endregion

        public TournamentTabViewModel()
        {
            Tournament = new Tournament();
            _allTournamentNames = new TrulyObservableCollection<TournamentName>();

            TournamentDescriptionNames = new TrulyObservableCollection<TournamentDescription>();
            TournamentDescription = new TournamentDescription();
            TournamentDescriptionNameIndex = -1;

            TournamentChairmen = new TrulyObservableCollection<TournamentChairman>();
            TournamentChairmenSelectedIndex = -1;

            ReadTournamentChairmen();
        }

        private void ReadTournamentChairmen()
        {
            string fileName = "TournamentChairmen.csv";
            if(!File.Exists(fileName))
            {
                return;
            }

            using(TextReader tr = new StreamReader(fileName))
            {
                string line;
                while((line = tr.ReadLine()) != null)
                {
                    string[] fields = line.Split(',');
                    if(fields.Length != 0)
                    {
                        if(fields.Length < 3)
                        {
                            throw new ArgumentException(fileName + ": this line does not have 3 fields: " + line);
                        }
                        TournamentChairmen.Add(new TournamentChairman { Name = fields[0], Email = fields[1], Phone = fields[2] });
                    }
                }
            }
        }

        private void UpdateChairmanFields()
        {
            if(TournamentChairmenSelectedIndex >= 0)
            {
                Tournament.ChairmanName = TournamentChairmen[TournamentChairmenSelectedIndex].Name;
                Tournament.ChairmanEmail = TournamentChairmen[TournamentChairmenSelectedIndex].Email;
                Tournament.ChairmanPhone = TournamentChairmen[TournamentChairmenSelectedIndex].Phone;
            }
        }

        private async Task GetTournaments(object o)
        {
            string responseString = await GetTournamentNames();

            LoadTournamentNamesFromWebResponse(responseString, _allTournamentNames, true);

            ShowTournamentNames();
        }

        private void ShowTournamentNames()
        {
            if (ShowAllTournamentNames)
            {
                TournamentNames = _allTournamentNames;
            }
            else
            {
                TournamentNames = new TrulyObservableCollection<TournamentName>();

                foreach (var tournamentName in _allTournamentNames)
                {
                    if (tournamentName.StartDate.AddMonths(3) > DateTime.Now)
                    {
                        TournamentNames.Add(tournamentName);
                    }
                }
            }
        }

        protected override void OnTournamentsUpdated()
        {
            TournamentNameIndex = -1;
            TournamentChairmenSelectedIndex = -1;
        }

        private async Task AddTournament(object o)
        {
            if(string.IsNullOrEmpty(Tournament.Name))
            {
                MessageBox.Show("Please fill in the tournament name");
                return;
            }

            // cancelled password input
            if (string.IsNullOrEmpty(Credentials.LoginPassword))
            {
                return;
            }

            foreach(var t in TournamentNames)
            {
                if((string.Compare(t.Name, Tournament.Name, true) == 0) && (t.StartDate == Tournament.StartDate))
                {
                    MessageBox.Show(Tournament.Name + " on " + Tournament.StartDate.ToShortDateString() + " matches an existing tournament.  Did you mean to update instead?");
                    return;
                }
            }

            if(Tournament.ScgaQualifier && Tournament.TeamSize != 2)
            {
                MessageBox.Show("SCGA qualifier requires team size to be set to 2.");
                return;
            }

            Tournament.TournamentDescriptionKey = TournamentDescription.TournamentDescriptionKey;
            bool success = false;
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(WebAddresses.BaseAddress);

                var values = Tournament.ToKeyValuePairs();

                values.Add(new KeyValuePair<string, string>("Login", Credentials.LoginName));
                values.Add(new KeyValuePair<string, string>("Password", Credentials.LoginPassword));

                values.Add(new KeyValuePair<string, string>("Action", "Insert"));

                var content = new FormUrlEncodedContent(values);

                string responseString;
                using (new WaitCursor())
                {
                    var response = await client.PostAsync(WebAddresses.ScriptFolder + WebAddresses.SubmitTournament, content);
                    responseString = await response.Content.ReadAsStringAsync();
                }

                if (!responseString.StartsWith("Success", StringComparison.InvariantCultureIgnoreCase))
                {
                    Credentials.CheckForInvalidPassword(responseString);
                    Logging.Log(WebAddresses.ScriptFolder + WebAddresses.SubmitTournament, responseString);

                    HtmlDisplayWindow displayWindow = new HtmlDisplayWindow();
                    displayWindow.WebBrowser.NavigateToString(responseString);
                    displayWindow.Owner = Application.Current.MainWindow;
                    displayWindow.ShowDialog();
                }
                else
                {
                    MessageBox.Show("Tournament added");
                    success = true;
                }
            }

            if (success)
            {
                GetTournaments(null);
                Tournament.Reset();
            }
        }

        private async Task UpdateTournament(object o)
        {
            if ((Tournament.TournamentKey < 0) || string.IsNullOrEmpty(Tournament.Name))
            {
                MessageBox.Show("Please select a tournament to update");
                return;
            }

            // cancelled password input
            if (string.IsNullOrEmpty(Credentials.LoginPassword))
            {
                return;
            }

            if (Tournament.ScgaQualifier && Tournament.TeamSize != 2)
            {
                MessageBox.Show("SCGA qualifier requires team size to be set to 2.");
                return;
            }

            if(Tournament.MemberGuest && Tournament.TeamSize != 2)
            {
                MessageBox.Show("Member/guest requires team size to be set to 2.");
                return;
            }

            Tournament.TournamentDescriptionKey = TournamentDescription.TournamentDescriptionKey;

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(WebAddresses.BaseAddress);

                var values = Tournament.ToKeyValuePairs();

                values.Add(new KeyValuePair<string, string>("Login", Credentials.LoginName));
                values.Add(new KeyValuePair<string, string>("Password", Credentials.LoginPassword));

                values.Add(new KeyValuePair<string, string>("Action", "Update"));

                var content = new FormUrlEncodedContent(values);

                string responseString;
                using (new WaitCursor())
                {
                    var response = await client.PostAsync(WebAddresses.ScriptFolder + WebAddresses.SubmitTournament, content);
                    responseString = await response.Content.ReadAsStringAsync();
                }

                if (!responseString.StartsWith("Success", StringComparison.InvariantCultureIgnoreCase))
                {
                    Credentials.CheckForInvalidPassword(responseString);
                    Logging.Log(WebAddresses.ScriptFolder + WebAddresses.SubmitTournament, responseString);

                    HtmlDisplayWindow displayWindow = new HtmlDisplayWindow();
                    displayWindow.WebBrowser.NavigateToString(responseString);
                    displayWindow.Owner = Application.Current.MainWindow;
                    displayWindow.ShowDialog();
                }
                else
                {
                    MessageBox.Show("Tournament updated");
                }
            }
        }

        private async Task DeleteTournament(object o)
        {
            if ((Tournament.TournamentKey < 0) || string.IsNullOrEmpty(Tournament.Name))
            {
                MessageBox.Show("Please select a tournament to delete");
                return;
            }

            if(MessageBox.Show("Are you sure you want to delete tournament : " +  
                Tournament.Name + "?", "Confirm Delete", 
                MessageBoxButton.YesNo) != MessageBoxResult.Yes)
            {
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

                values.Add(new KeyValuePair<string, string>("Action", "Delete"));
                values.Add(new KeyValuePair<string, string>("TournamentKey", 
                    Tournament.TournamentKey.ToString()));

                bool sent = await HttpSend(client, HtmlRequestType.Post, values, WebAddresses.ScriptFolder + WebAddresses.SubmitTournament);

                if (!sent)
                {
                    return;
                }

                Tournament.Reset();
                TournamentDescription.Clear();

                MessageBox.Show("Deleted tournament");

                // Update the list of tournament descriptions
                GetTournaments(null);

                TournamentDescriptionNameIndex = -1;
            }
        }

        private async Task TournamentSelectionChanged(int selectedIndex)
        {
            if (selectedIndex < 0)
            {
                return;
            }

            if (TournamentNames.Count == 0)
            {
                return;
            }

            TournamentName tournamentName = TournamentNames[selectedIndex];

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(WebAddresses.BaseAddress);

                using (new WaitCursor())
                {
                    var values = new List<KeyValuePair<string, string>>();

                    values.Add(new KeyValuePair<string, string>("TournamentKey", tournamentName.TournamentKey.ToString()));

                    var content = new FormUrlEncodedContent(values);

                    var response = await client.PostAsync(WebAddresses.ScriptFolder + WebAddresses.GetTournament, content);
                    var responseString = await response.Content.ReadAsStringAsync();

                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        LoadTournamentFromWebResponse(responseString);
                    }
                    else
                    {
                        Logging.Log(WebAddresses.ScriptFolder + WebAddresses.GetTournament, responseString);

                        HtmlDisplayWindow displayWindow = new HtmlDisplayWindow();
                        if (!string.IsNullOrEmpty(responseString))
                        {
                            displayWindow.WebBrowser.NavigateToString(responseString);
                            displayWindow.Owner = App.Current.MainWindow;
                            displayWindow.ShowDialog();
                        }
                    }
                }
            }

            if (TournamentDescriptionNames.Count == 0)
            {
                GetTournamentDescriptions(false);
            }

            SelectCurrentDescription();

            TournamentChairmenSelectedIndex = -1;
        }

        private void SelectCurrentDescription()
        {
            TournamentDescriptionNameIndex = -1;

            // Select the description if the key has been set
            if (Tournament.TournamentDescriptionKey > 0)
            {
                for (int i = 0; i < TournamentDescriptionNames.Count; i++)
                {
                    if (TournamentDescriptionNames[i].TournamentDescriptionKey == Tournament.TournamentDescriptionKey)
                    {
                        TournamentDescriptionNameIndex = i;
                        break;
                    }
                }
            }
        }

        public void LoadTournamentFromWebResponse(string webResponse)
        {
            var jss = new JavaScriptSerializer();
            Tournament = jss.Deserialize<Tournament>(webResponse);
        }

        private void GetTournamentDescriptions(object o)
        {
            GetTournamentDescriptions(true);
        }

        private async Task GetTournamentDescriptions(bool defaultIndex)
        {
            TournamentDescriptionNames.Clear();
            TournamentDescriptionNameIndex = -1;

            string responseString = await GetTournamentDescriptions();

            LoadTournamentDescriptionsFromWebResponse(responseString, TournamentDescriptionNames);

            SelectCurrentDescription();

            if (defaultIndex && (TournamentDescriptionNameIndex == -1) && (TournamentDescriptionNames.Count > 0))
            {
                TournamentDescriptionNameIndex = 0;
            }
        }

        private void TournamentDescriptionSelectionChanged(int index)
        {
            if ((index >= 0) && (index < TournamentDescriptionNames.Count))
            {
                TournamentDescription.TournamentDescriptionKey = TournamentDescriptionNames[index].TournamentDescriptionKey;
                TournamentDescription.Name = TournamentDescriptionNames[index].Name;
                TournamentDescription.Description = TournamentDescriptionNames[index].Description;
            }
        }
    }
}
