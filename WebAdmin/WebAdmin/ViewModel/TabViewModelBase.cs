using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Net.Http;
using System.Web.Script.Serialization;
using WebAdmin.View;

namespace WebAdmin.ViewModel
{
    public class TabViewModelBase : INotifyPropertyChanged, ITabViewModel
    {
        public enum HtmlRequestType { Get, Post, Put, Delete }

        #region PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
        #endregion

        public virtual string Header { get { return "tab base"; } }

        public static Options Options { get; set; }
        public static Credentials Credentials { get; set; }
        public static StatusMsg Status { get; set; }

        public static List<GHINEntry> GHINEntries { get; set; }

        //public delegate void TournamentsUpdatedEventHandler(object sender, EventArgs e);
        //protected event TournamentsUpdatedEventHandler TournamentsUpdatedEvent

        public TabViewModelBase()
        {
            if(Credentials == null)
            {
                Credentials = new Credentials();
            }
        }

        protected async Task<bool> HttpSend(
            HttpClient client,
            HtmlRequestType requestType,
            List<KeyValuePair<string, string>> values,
            string url)
        {
            string responseString;

            using (new WaitCursor())
            {
                var content = new FormUrlEncodedContent(values);

                HttpResponseMessage response = null;

                switch(requestType)
                {
                    case HtmlRequestType.Get:
                        break;
                    case HtmlRequestType.Put:
                        break;
                    case HtmlRequestType.Post:
                        response = await client.PostAsync(url, content);
                        break;
                    case HtmlRequestType.Delete:
                        response = await client.DeleteAsync(url);
                        break;
                }

                responseString = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine(responseString);
            }

            if (responseString.StartsWith("Success", StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }
            else
            {
                TabViewModelBase.Credentials.CheckForInvalidPassword(responseString);
                Logging.Log(url, responseString);

                HtmlDisplayWindow displayWindow = new HtmlDisplayWindow();
                if (!string.IsNullOrEmpty(responseString))
                {
                    displayWindow.WebBrowser.NavigateToString(responseString);
                    displayWindow.Owner = App.Current.MainWindow;
                    displayWindow.ShowDialog();
                }
                else
                {
                    System.Windows.MessageBox.Show("Reponse was an empty string");
                }
            }

            return false;

        }

        protected async Task<string> GetTournamentNames()
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(WebAddresses.BaseAddress);

                var values = new List<KeyValuePair<string, string>>();

                var content = new FormUrlEncodedContent(values);

                string responseString = string.Empty;

                using (new WaitCursor())
                {
                    var response = await client.PostAsync(WebAddresses.ScriptFolder + WebAddresses.GetTournamentNames, content);
                    responseString = await response.Content.ReadAsStringAsync();
                }
                Logging.Log("GetTournamentNames", responseString);

                return responseString;

            }
        }

        protected void LoadTournamentNamesFromWebResponse(
            string webResponse, 
            TrulyObservableCollection<TournamentName> tournamentNames,
            bool loadAll)
        {
            tournamentNames.Clear();

            var jss = new JavaScriptSerializer();
            TournamentName[] names = jss.Deserialize<TournamentName[]>(webResponse);

            foreach (var tournamentName in names)
            {
                if (loadAll)
                {
                    tournamentNames.Add(tournamentName);
                }
                else if (tournamentName.StartDate.AddYears(1) > DateTime.Now)
                {
                    tournamentNames.Add(tournamentName);
                }
            }

            OnTournamentsUpdated();
        }

        protected virtual void OnTournamentsUpdated()
        {
        }

        protected async Task<string> GetTournamentDescriptions()
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(WebAddresses.BaseAddress);

                var values = new List<KeyValuePair<string, string>>();

                var content = new FormUrlEncodedContent(values);

                string responseString = string.Empty;

                using (new WaitCursor())
                {
                    var response = await client.PostAsync(WebAddresses.ScriptFolder + WebAddresses.GetTournamentDescriptions, content);
                    responseString = await response.Content.ReadAsStringAsync();
                }
                Logging.Log("GetTournamentDescriptions", responseString);

                return responseString;
            }
        }

        protected void LoadTournamentDescriptionsFromWebResponse(string responseString,
            TrulyObservableCollection<TournamentDescription> tournamentDescriptionNames)
        {
            string[] lines = responseString.Split('\n');
            string[][] csvParsedLines = CSVParser.Parse(lines);

            bool clearedDescriptions = false;
            int lineNumber = 0;
            foreach (var fields in csvParsedLines)
            {
                lineNumber++;
                if ((fields.Length == 0) || string.IsNullOrWhiteSpace(fields[0])) continue;

                if (!clearedDescriptions)
                {
                    clearedDescriptions = true;
                    
                }

                if (fields.Length < 3)
                {
                    // the description contains newlines, so the line number doesn't match up ...
                    throw new ArgumentException(string.Format("Website response: line {0}: contains fewer than 3 fields: {1}", lineNumber, lines[lineNumber - 1]));
                }

                var td = new TournamentDescription();

                int key;
                if (!int.TryParse(fields[0], out key))
                {
                    throw new ArgumentException(string.Format("Website response: line {0}: contains a bad key: {1}", lineNumber, fields[0]));
                }
                td.TournamentDescriptionKey = key;
                td.Name = fields[1];
                td.Description = fields[2].Replace("\r\r", "\r");

                tournamentDescriptionNames.Add(td);
            }
        }

        protected List<TeeTimeRequest> LoadSignupsFromWebResponse(string webResponse)
        {
            List<TeeTimeRequest> teeTimeRequests = new List<TeeTimeRequest>();

            string[] responseLines = webResponse.Split('\n');

            string[][] lines = CSVParser.Parse(responseLines);
            int lineNumber = 0;
            for (int lineIndex = 0, playerIndex = 0; lineIndex < lines.Length; lineIndex++, playerIndex++)
            {
                lineNumber++;
                string[] fields = lines[lineIndex];

                if (fields.Length == 0) continue;

                TeeTimeRequest teeTimeRequest = new TeeTimeRequest();

                teeTimeRequest.Preference = fields[0];

                float f;
                if (!float.TryParse(fields[1], out f))
                {
                    throw new ArgumentException(string.Format("Website response: line {0}: invalid payment made: {1}", lineNumber, responseLines[lineNumber - 1]));
                }
                teeTimeRequest.PaymentMade = f;

                if (!float.TryParse(fields[2], out f))
                {
                    throw new ArgumentException(string.Format("Website response: line {0}: invalid payment due: {1}", lineNumber, responseLines[lineNumber - 1]));
                }
                teeTimeRequest.PaymentDue = f;

                teeTimeRequest.PaymentDateTime = fields[3];
                teeTimeRequest.AccessCode = fields[4];
                teeTimeRequest.Paid = (teeTimeRequest.PaymentMade >= teeTimeRequest.PaymentDue);
                teeTimeRequest.ModifiedPaid = teeTimeRequest.Paid;  // for payments tab

                int signupKey;
                if(!int.TryParse(fields[5], out signupKey))
                {
                    throw new ArgumentException(string.Format("Website response: line {0}: invalid signup key: {1}", lineNumber, responseLines[lineNumber - 1]));
                }
                teeTimeRequest.SignupKey = signupKey;

                teeTimeRequest.PayerName = fields[6];
                teeTimeRequest.PayerEmail = fields[7];

                // The rest of the fields are Name/GHIN/Extra triples
                int playerPosition = 1;
                for (int i = 8; i < fields.Length; i += 3)
                {
                    if (string.IsNullOrWhiteSpace(fields[i]))
                    {
                        continue;
                    }

                    Player player = new Player();
                    player.Position = playerPosition;
                    playerPosition++;
                    player.Name = fields[i].Trim();
                    if (string.IsNullOrWhiteSpace(fields[i + 1]))
                    {
                        throw new ArgumentException(
                            string.Format("Website response: line {0}: missing GHIN number: {1}", lineNumber,
                                responseLines[lineNumber - 1]));
                    }
                    player.GHIN = fields[i + 1].Trim();
                    player.Extra = fields[i + 2].Trim();


                    teeTimeRequest.Players.Add(player);
                }

                if (teeTimeRequest.Players.Count == 0)
                {
                    throw new ArgumentException(string.Format("Website response: line {0} does not have any players: {1}", lineNumber, responseLines[lineNumber - 1]));
                }

                teeTimeRequests.Add(teeTimeRequest);
            }

            return teeTimeRequests;

        }
    }
}
