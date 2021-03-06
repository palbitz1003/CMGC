﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Net.Http;
using System.Windows.Input;
using WebAdmin.View;

namespace WebAdmin.ViewModel
{
    public class PaymentsTabViewModel : TabViewModelBase
    {

        #region Properties
        public override string Header { get { return "Payments"; } }

        private Visibility _getTournamentsVisible;
        public Visibility GetTournamentsVisible { get { return _getTournamentsVisible; } set { _getTournamentsVisible = value; OnPropertyChanged(); } }

        private Visibility _gotTournamentsVisible;
        public Visibility GotTournamentsVisible { get { return _gotTournamentsVisible; } set { _gotTournamentsVisible = value; OnPropertyChanged(); } }

        private float _paymentsMade;
        public float PaymentsMade { get { return _paymentsMade; } set { _paymentsMade = value; OnPropertyChanged(); } }

        private float _paymentsDue;
        public float PaymentsDue { get { return _paymentsDue; } set { _paymentsDue = value; OnPropertyChanged(); } }

        private bool _enableUploadToWebButton;

        public bool EnableUploadToWebButton
        {
            get { return _enableUploadToWebButton; }
            set
            {
                _enableUploadToWebButton = value;
                OnPropertyChanged();
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
                }
            }
        }

        private TrulyObservableCollection<TournamentName> _tournamentNames;
        public TrulyObservableCollection<TournamentName> TournamentNames
        {
            get { return _tournamentNames; }
            set { _tournamentNames = value; OnPropertyChanged(); }
        }

        private TrulyObservableCollection<TeeTimeRequest> _teeTimeRequests;
        public TrulyObservableCollection<TeeTimeRequest> TeeTimeRequests
        {
            get { return _teeTimeRequests; }
            set
            {
                _teeTimeRequests = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Commands
        public ICommand GetTournamentsCommand { get { return new ModelCommand(async s => await GetTournaments(s)); } }

        public ICommand LoadSignupsCommand { get { return new ModelCommand(async s => await LoadSignupsFromWeb(s)); } }

        public ICommand UploadSignupsCommand { get { return new ModelCommand(async s => await UploadToWeb(s)); } }
        #endregion

        public PaymentsTabViewModel()
        {
            TournamentNames = new TrulyObservableCollection<TournamentName>();
            TeeTimeRequests = new TrulyObservableCollection<TeeTimeRequest>();

            GetTournamentsVisible = Visibility.Visible;
            GotTournamentsVisible = Visibility.Collapsed;
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
                for (int i = TournamentNames.Count - 1; i >= 0; i--)
                {
                    if ((DateTime.Now >= TournamentNames[i].EndDate) && !TournamentNames[i].AnnouncementOnly)
                    {
                        TournamentNameIndex = i;
                        break;
                    }
                }
            }

            if (TournamentNames.Count > 0)
            {
                GetTournamentsVisible = Visibility.Collapsed;
                GotTournamentsVisible = Visibility.Visible;
            }
        }

        private async Task UploadToWeb(object o)
        {
            PaymentsDue = 0;
            PaymentsMade = 0;

            List<TeeTimeRequest> modifiedEntries = new List<TeeTimeRequest>();
            foreach (var teeTimeRequest in TeeTimeRequests)
            {
                if(!teeTimeRequest.Paid && teeTimeRequest.ModifiedPaid)
                {
                    modifiedEntries.Add(teeTimeRequest);
                    teeTimeRequest.PaymentMade = teeTimeRequest.PaymentDue;
                    teeTimeRequest.Paid = teeTimeRequest.ModifiedPaid;
                }
                if(teeTimeRequest.Paid && !teeTimeRequest.ModifiedPaid)
                {
                    modifiedEntries.Add(teeTimeRequest);
                    teeTimeRequest.PaymentMade = 0;
                    teeTimeRequest.Paid = teeTimeRequest.ModifiedPaid;
                }

                PaymentsDue += teeTimeRequest.PaymentDue;
                PaymentsMade += teeTimeRequest.PaymentMade;
            }

            if (modifiedEntries.Count == 0)
            {
                MessageBox.Show("No entries were changed");
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
                values.Add(new KeyValuePair<string, string>("Action", "UpdatePaymentMade"));

                values.Add(new KeyValuePair<string, string>("TournamentKey",
                        TournamentNames[TournamentNameIndex].TournamentKey.ToString(CultureInfo.InvariantCulture)));

                for (int i = 0; i < modifiedEntries.Count; i++)
                {
                    values.Add(new KeyValuePair<string, string>(
                        string.Format("Signup[{0}][SignupKey]", i),
                        modifiedEntries[i].SignupKey.ToString(CultureInfo.InvariantCulture)));

                    values.Add(new KeyValuePair<string, string>(
                        string.Format("Signup[{0}][PaymentMade]", i),
                        modifiedEntries[i].PaymentMade.ToString(CultureInfo.InvariantCulture)));
                }

                var content = new FormUrlEncodedContent(values);

                using (new WaitCursor())
                {
                    var response = await client.PostAsync(WebAddresses.ScriptFolder + WebAddresses.SubmitSignUps, content);
                    var responseString = await response.Content.ReadAsStringAsync();

                    if (responseString.StartsWith("Success", StringComparison.InvariantCultureIgnoreCase))
                    {
                        MessageBox.Show("Updated " + modifiedEntries.Count + " signups");
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

        private async Task LoadSignupsFromWeb(object o)
        {
            if (TournamentNames.Count == 0)
            {
                MessageBox.Show("You must select a touranment first");
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
                    List<TeeTimeRequest> ttr = LoadSignupsFromWebResponseJson(responseString);

                    PaymentsDue = 0;
                    PaymentsMade = 0;
                    TrulyObservableCollection<TeeTimeRequest> ttr2 = new TrulyObservableCollection<TeeTimeRequest>();
                    foreach (var teeTimeRequest in ttr)
                    {
                        ttr2.Add(teeTimeRequest);
                        PaymentsDue += teeTimeRequest.PaymentDue;
                        PaymentsMade += teeTimeRequest.PaymentMade;
                    }
                    TeeTimeRequests = TrulyObservableCollection<TeeTimeRequest>.Sort(ttr2, new PlayerOrderSort());
                    ttr2.CollectionChanged += TeeTimeRequests_CollectionChanged;
                }
            }
        }

        private class PlayerOrderSort : IComparer<TeeTimeRequest>
        {
            public int Compare(TeeTimeRequest x, TeeTimeRequest y)
            {
                return string.Compare(x.Players[0].Name, y.Players[0].Name);
            }
        }

        void TeeTimeRequests_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            foreach (var teeTimeRequest in TeeTimeRequests)
            {
                if(teeTimeRequest.Paid != teeTimeRequest.ModifiedPaid)
                {
                    EnableUploadToWebButton = true;
                    return;
                }
            }

            EnableUploadToWebButton = false;
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
