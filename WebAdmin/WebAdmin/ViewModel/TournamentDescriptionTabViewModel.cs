using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Net.Http;

namespace WebAdmin.ViewModel
{
    public class TournamentDescriptionTabViewModel : TabViewModelBase
    {
        #region Properties
        public override string Header { get { return "Tournament Descriptions"; } }

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
                    OnPropertyChanged("TournamentDescriptionSelected");
                    TournamentDescriptionSelectionChanged(value);
                }
            }
        }

        private TrulyObservableCollection<TournamentDescription> _tournamentDescriptionNames;
        public TrulyObservableCollection<TournamentDescription> TournamentDescriptionNames
        {
            get { return _tournamentDescriptionNames; }
            set { _tournamentDescriptionNames = value; OnPropertyChanged(); }
        }

        private TournamentDescription _tournamentDescription;
        public TournamentDescription TournamentDescription { get { return _tournamentDescription; } set { _tournamentDescription = value; OnPropertyChanged(); } }

        public bool TournamentDescriptionSelected { get { return TournamentDescriptionNameIndex >= 0; } }
        #endregion

        #region Commands
        public ICommand GetTournamentDescriptionCommand { get { return new ModelCommand(s => GetTournamentDescriptions(s)); } }
        public ICommand AddTournamentDescriptionCommand { get { return new ModelCommand(s => AddTournamentDescription(s)); } }
        public ICommand UpdateTournamentDescriptionCommand { get { return new ModelCommand(s => UpdateTournamentDescription(s)); } }
        public ICommand DeleteTournamentDescriptionCommand { get { return new ModelCommand(s => DeleteTournamentDescription(s)); } }
        #endregion

        public TournamentDescriptionTabViewModel()
        {
            TournamentDescriptionNames = new TrulyObservableCollection<WebAdmin.TournamentDescription>();
            TournamentDescription = new TournamentDescription();
            TournamentDescriptionNameIndex = -1;
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

        private async Task GetTournamentDescriptions(object o)
        {
            TournamentDescriptionNames.Clear();
            TournamentDescriptionNameIndex = -1;

            string responseString = await GetTournamentDescriptions();

            LoadTournamentDescriptionsFromWebResponse(responseString, TournamentDescriptionNames);
        }

        private void AddTournamentDescription(object o)
        {
            // cancelled password input
            if (string.IsNullOrEmpty(Credentials.LoginPassword))
            {
                return;
            }

            AddTournamentDescription(true);

            // TODO: pass in a delegate to execute below only if succeeded
            System.Windows.MessageBox.Show("Added tournament description");

            // select the tournament description
            SelectDescription(TournamentDescription.Name);
        }

        private void UpdateTournamentDescription(object o)
        {
            // cancelled password input
            if (string.IsNullOrEmpty(Credentials.LoginPassword))
            {
                return;
            }

            AddTournamentDescription(false);

            // TODO: pass in a delegate to execute below only if succeeded
            System.Windows.MessageBox.Show("Updated tournament description");

            // re-select the tournament description
            SelectDescription(TournamentDescription.Name);
        }

        private void SelectDescription(string selectedName)
        {
            TournamentDescriptionNameIndex = -1;
            TournamentDescription.Clear();

            for (int i = 0; i < TournamentDescriptionNames.Count; i++)
            {
                if (string.Compare(TournamentDescriptionNames[i].Name, selectedName, true) == 0)
                {
                    TournamentDescriptionNameIndex = i;
                    break;
                }
            }
        }

        private async Task AddTournamentDescription(bool add)
        {
            if (string.IsNullOrEmpty(TournamentDescription.Name))
            {
                MessageBox.Show("Please fill in the name of the tournament description");
                return;
            }

            if (string.IsNullOrEmpty(TournamentDescription.Description))
            {
                MessageBox.Show("Please fill in the description of the tournament");
                return;
            }

            if (add)
            {
                // Check for duplicates before adding
                foreach (var td in TournamentDescriptionNames)
                {
                    if (string.Compare(td.Name, TournamentDescription.Name, true) == 0)
                    {
                        MessageBox.Show("Unable to add: a tournament description already exists for " + TournamentDescription.Name);
                        return;
                    }
                }
            }
            else if (TournamentDescriptionNameIndex < 0)
            {
                System.Windows.MessageBox.Show("Please select a tournament description to update");
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

                values.Add(new KeyValuePair<string, string>("Action", add ? "Insert" : "Update"));
                if (!add)
                {
                    values.Add(new KeyValuePair<string, string>("TournamentDetailsKey",
                    TournamentDescriptionNames[TournamentDescriptionNameIndex].TournamentDescriptionKey.ToString()));
                }
                values.Add(new KeyValuePair<string, string>("TournamentDetailsName", TournamentDescription.Name));
                values.Add(new KeyValuePair<string, string>("TournamentDetailsDescription", TournamentDescription.Description));

                bool sent = await HttpSend(client, HtmlRequestType.Post, values, WebAddresses.ScriptFolder + WebAddresses.SubmitTournamentDescription);

                if (!sent)
                {
                    return;
                }

                // Update the list of tournament descriptions
                GetTournamentDescriptions(null);
            }
        }

        private async Task DeleteTournamentDescription(object o)
        {
            if (TournamentDescriptionNameIndex < 0)
            {
                System.Windows.MessageBox.Show("Please select a tournament description to delete");
                return;
            }

            if(System.Windows.MessageBox.Show("Are you sure you want to delete tournament description: " +  
                TournamentDescriptionNames[TournamentDescriptionNameIndex].Name + "?", "Confirm Delete", 
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
                values.Add(new KeyValuePair<string, string>("TournamentDetailsKey", 
                    TournamentDescriptionNames[TournamentDescriptionNameIndex].TournamentDescriptionKey.ToString()));

                bool sent = await HttpSend(client, HtmlRequestType.Post, values, WebAddresses.ScriptFolder + WebAddresses.SubmitTournamentDescription);

                if (!sent)
                {
                    return;
                }

                TournamentDescription.Clear();

                System.Windows.MessageBox.Show("Deleted tournament description");

                // Update the list of tournament descriptions
                GetTournamentDescriptions(null);

                TournamentDescriptionNameIndex = -1;
            }
        }
    }
}
