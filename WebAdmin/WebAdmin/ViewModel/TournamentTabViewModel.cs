using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http;
using System.Net;
using System.Windows.Input;
using System.Windows;
using System.IO;
using WebAdmin;
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
        public ICommand GetTournamentsCommand { get { return new ModelCommand(s => GetTournaments(s)); } }
        public ICommand AddTournamentsCommand { get { return new ModelCommand(s => AddTournament(s)); } }
        public ICommand UpdateTournamentsCommand { get { return new ModelCommand(s => UpdateTournament(s)); } }
        public ICommand DeleteTournamentsCommand { get { return new ModelCommand(s => DeleteTournament(s)); } }
        public ICommand GetTournamentDescriptionCommand { get { return new ModelCommand(s => GetTournamentDescriptions(s)); } }

        #endregion

        public TournamentTabViewModel()
        {
            Tournament = new WebAdmin.Tournament();
            _allTournamentNames = new TrulyObservableCollection<TournamentName>();

            TournamentDescriptionNames = new TrulyObservableCollection<WebAdmin.TournamentDescription>();
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
                        TournamentChairmen.Add(new TournamentChairman() { Name = fields[0], Email = fields[1], Phone = fields[2] });
                    }
                }
            }
        }

        private void UpdateChairmanFields()
        {
            if(TournamentChairmenSelectedIndex >= 0)
            {
                Tournament.Chairman.Name = TournamentChairmen[TournamentChairmenSelectedIndex].Name;
                Tournament.Chairman.Email = TournamentChairmen[TournamentChairmenSelectedIndex].Email;
                Tournament.Chairman.Phone = TournamentChairmen[TournamentChairmenSelectedIndex].Phone;
            }
        }

        private async void GetTournaments(object o)
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
                    if (tournamentName.StartDate.AddYears(1) > DateTime.Now)
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

        private async void AddTournament(object o)
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
                    TabViewModelBase.Credentials.CheckForInvalidPassword(responseString);
                    Logging.Log(WebAddresses.ScriptFolder + WebAddresses.SubmitTournament, responseString);

                    HtmlDisplayWindow displayWindow = new HtmlDisplayWindow();
                    displayWindow.WebBrowser.NavigateToString(responseString);
                    displayWindow.Owner = App.Current.MainWindow;
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

        private async void UpdateTournament(object o)
        {
            if ((Tournament.TournamentKey < 0) || string.IsNullOrEmpty(Tournament.Name))
            {
                System.Windows.MessageBox.Show("Please select a tournament to update");
                return;
            }

            // cancelled password input
            if (string.IsNullOrEmpty(Credentials.LoginPassword))
            {
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
                    TabViewModelBase.Credentials.CheckForInvalidPassword(responseString);
                    Logging.Log(WebAddresses.ScriptFolder + WebAddresses.SubmitTournament, responseString);

                    HtmlDisplayWindow displayWindow = new HtmlDisplayWindow();
                    displayWindow.WebBrowser.NavigateToString(responseString);
                    displayWindow.Owner = App.Current.MainWindow;
                    displayWindow.ShowDialog();
                }
                else
                {
                    MessageBox.Show("Tournament updated");
                }
            }
        }

        private async void DeleteTournament(object o)
        {
            if ((Tournament.TournamentKey < 0) || string.IsNullOrEmpty(Tournament.Name))
            {
                System.Windows.MessageBox.Show("Please select a tournament to delete");
                return;
            }

            if(System.Windows.MessageBox.Show("Are you sure you want to delete tournament : " +  
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

                System.Windows.MessageBox.Show("Deleted tournament");

                // Update the list of tournament descriptions
                GetTournaments(null);

                TournamentDescriptionNameIndex = -1;
            }
        }

        private async void TournamentSelectionChanged(int selectedIndex)
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
                    //MessageBox.Show(responseString);

                    LoadTournamentFromWebResponse(responseString);
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
            string[] lines = webResponse.Split('\n');
            int lineNumber = 0;
            foreach (string line in lines)
            {
                lineNumber++;
                if (string.IsNullOrWhiteSpace(line)) continue;

                string[] fields = line.Split(',');
                if (fields.Length < 20)
                {
                    throw new ArgumentException(string.Format("Website response: line {0}: expected at least 20 fields, got {1} {2}", lineNumber, fields.Length, line));
                }

                int key;
                if (!int.TryParse(fields[0], out key))
                {
                    throw new ArgumentException(string.Format("Website response: line {0}: key field is not an integer {1}", lineNumber, line));
                }
                Tournament.TournamentKey = key;
                Tournament.Name = fields[1];
                Tournament.Year = fields[2];

                DateTime dt;
                if (!DateTime.TryParse(fields[3], out dt))
                {
                    throw new ArgumentException(string.Format("Website response: line {0}: start date field (4) is not a date {1}", lineNumber, line));
                }
                Tournament.StartDate = dt;

                if (!DateTime.TryParse(fields[4], out dt))
                {
                    throw new ArgumentException(string.Format("Website response: line {0}: end date field (5) is not a date {1}", lineNumber, line));
                }
                Tournament.EndDate = dt;

                if (!DateTime.TryParse(fields[5], out dt))
                {
                    throw new ArgumentException(string.Format("Website response: line {0}: signup start date field (6) is not a date {1}", lineNumber, line));
                }
                Tournament.SignupStartDate = dt;

                if (!DateTime.TryParse(fields[6], out dt))
                {
                    throw new ArgumentException(string.Format("Website response: line {0}: signup end date field (7) is not a date {1}", lineNumber, line));
                }
                Tournament.SignupEndDate = dt;

                if (!DateTime.TryParse(fields[7], out dt))
                {
                    throw new ArgumentException(string.Format("Website response: line {0}: cancel date field (8) is not a date {1}", lineNumber, line));
                }
                Tournament.CancelEndDate = dt;

                int i;
                if (!int.TryParse(fields[8], out i))
                {
                    throw new ArgumentException(string.Format("Website response: line {0}: local handicap field (9) is not a bool {1}", lineNumber, line));
                }
                Tournament.LocalHandicap = (i != 0);

                if (!int.TryParse(fields[9], out i))
                {
                    throw new ArgumentException(string.Format("Website response: line {0}: scga field (10) is not a bool {1}", lineNumber, line));
                }
                Tournament.ScgaTournament = (i != 0);

                if (!int.TryParse(fields[10], out i))
                {
                    throw new ArgumentException(string.Format("Website response: line {0}: team size field (11) is not an integer {1}", lineNumber, line));
                }
                for (int index = 0; index < Tournament.TeamSizeList.Count; index++)
                {
                    if (i == Tournament.TeamSizeList[index])
                    {
                        Tournament.TeamSizeSelectedIndex = index;
                    }
                }

                if(!int.TryParse(fields[11], out i))
                {
                    throw new ArgumentException(string.Format("Website response: line {0}: tournament description key field is not an integer {1}", lineNumber, line));
                }
                Tournament.TournamentDescriptionKey = i;

                if(!int.TryParse(fields[12], out i))
                {
                    throw new ArgumentException(string.Format("Website response: line {0}: cost field is not an integer {1}", lineNumber, line));
                }
                Tournament.Cost = i;

                if (!int.TryParse(fields[13], out i))
                {
                    throw new ArgumentException(string.Format("Website response: line {0}: pool field is not an integer {1}", lineNumber, line));
                }
                Tournament.Pool = i;

                Tournament.Chairman.Name = fields[14];
                Tournament.Chairman.Email = fields[15];
                Tournament.Chairman.Phone = fields[16];

                Tournament.TournamentType = Tournament.TournamentTypes.Stroke;
                if (!int.TryParse(fields[17], out i))
                {
                    throw new ArgumentException(string.Format("Website response: line {0}: stableford field (18) is not a bool {1}", lineNumber, line));
                }
                if (i != 0) Tournament.TournamentType = Tournament.TournamentTypes.Stableford;

                if (!int.TryParse(fields[18], out i))
                {
                    throw new ArgumentException(string.Format("Website response: line {0}: eclectic field (19) is not a bool {1}", lineNumber, line));
                }
                if (i != 0) Tournament.TournamentType = Tournament.TournamentTypes.Eclectic;

                if (!int.TryParse(fields[19], out i))
                {
                    throw new ArgumentException(string.Format("Website response: line {0}: send email field (20) is not a bool {1}", lineNumber, line));
                }
                Tournament.SendEmail = (i != 0);

                if (!int.TryParse(fields[20], out i))
                {
                    throw new ArgumentException(string.Format("Website response: line {0}: require payment field (21) is not a bool {1}", lineNumber, line));
                }
                Tournament.RequirePayment = (i != 0);

                Tournament.TournamentSubType = Tournament.TournamentSubTypes.None;
                if (fields.Length > 21)
                {
                    if (!int.TryParse(fields[21], out i))
                    {
                        throw new ArgumentException(
                            string.Format("Website response: line {0}: require payment field (22) is not a bool {1}",
                                lineNumber, line));
                    }
                    if (i != 0) Tournament.TournamentSubType = Tournament.TournamentSubTypes.ScgaQualifier;
                }

                if (fields.Length > 22)
                {
                    if (!int.TryParse(fields[22], out i))
                    {
                        throw new ArgumentException(
                            string.Format("Website response: line {0}: require payment field (23) is not a bool {1}",
                                lineNumber, line));
                    }
                    if (i != 0) Tournament.TournamentSubType = Tournament.TournamentSubTypes.SrClubChampionship;
                }

                break;
            }
        }

        private void GetTournamentDescriptions(object o)
        {
            GetTournamentDescriptions(true);
        }

        private async void GetTournamentDescriptions(bool defaultIndex)
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
