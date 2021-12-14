using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace WebAdmin
{
    public class TeeTimeRequest : INotifyPropertyChanged, ICloneable
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        private System.Windows.Media.Brush _foregroundColor = System.Windows.Media.Brushes.Black;
        public System.Windows.Media.Brush ForegroundColor
        {
            get { return _foregroundColor; }
            set
            {
                _foregroundColor = value;
                OnPropertyChanged("ForegroundColor");
            }
        }

        private string _preference;
        public string Preference { get { return _preference; } set { _preference = value; OnPropertyChanged(); } }

        public TrulyObservableCollection<Player> Players { get; set;}

        // For some reason, adding directly to the Players list doesn't trigger an event the UI is looking for
        public void AddPlayer(Player player)
        {
            Players.Add(player);
            OnPropertyChanged("Players");
        }

        public void RemovePlayer(Player player)
        {
            Players.Remove(player);
            OnPropertyChanged("Players");
        }

        public void ReplacePlayer(Player playerToRemove, Player playerToAdd)
        {
            Players[Players.IndexOf(playerToRemove)] = playerToAdd;
            OnPropertyChanged("Players");
        }

        public void ClearPlayers()
        {
            Players.Clear();
            OnPropertyChanged("Players");
        }

        // Do not make TeeTime a property that notifies when it is changed. When you click on an unassigned 
        // tee time request, the event triggers an extra attempt to put the item on the assigned list.
        public TeeTime TeeTime { get; set; }

        public string PlayerList {
            get { return PlayersToString(); }
        }

        public string RequestedTimeAndPlayerList
        {
            get {
                string extraData;
                if (ShowBlindDrawValue)
                {
                    extraData = _blindDrawValue.ToString();
                }
                else if (ShowLastTeeTime)
                {
                    extraData = _lastTeetime;
                }
                else
                {
                    extraData = TimeSpan.FromSeconds(StartTimeAverageInSeconds).ToString(@"hh\:mm") + "/" + TeeTimeCount.ToString("D2");
                }

                if (_waitlisted)
                {
                    return "wait: " + extraData + " " + ToString();
                }
                else if (_paid)
                {
                    return "paid: " + extraData + " "  + ToString();
                }
                else
                {
                    return "         " + extraData + " " + ToString();
                }
            }
        }

        private bool _waitlisted = false;
        public bool Waitlisted { get { return _waitlisted; } set { _waitlisted = value; OnPropertyChanged(); } }

        private float _paymentDue;
        public float PaymentDue { get { return _paymentDue; } set { _paymentDue = value; OnPropertyChanged(); } }

        private float _paymentMade;
        public float PaymentMade { get { return _paymentMade; } set { _paymentMade = value; OnPropertyChanged(); } }

        private string _paymentDateTime;
        public string PaymentDateTime { get { return _paymentDateTime; } set { _paymentDateTime = value; OnPropertyChanged(); } }

        private string _accessCode;
        public string AccessCode { get { return _accessCode; } set { _accessCode = value; OnPropertyChanged(); } }

        private bool _paid;
        public bool Paid { get { return _paid; } set { _paid = value; OnPropertyChanged(); } }

        private bool _modifiedPaid;
        public bool ModifiedPaid { get { return _modifiedPaid; } set { _modifiedPaid = value; OnPropertyChanged(); } }

        private int _signupKey;
        public int SignupKey { get { return _signupKey; } set { _signupKey = value; OnPropertyChanged(); } }

        private string _payerName;
        public string PayerName { get { return _payerName; } set { _payerName = value; OnPropertyChanged(); } }

        private string _payerEmail;
        public string PayerEmail { get { return _payerEmail; } set { _payerEmail = value; OnPropertyChanged(); } }

        private int _blindDrawValue = 0;
        public int BlindDrawValue { get { return _blindDrawValue; } set { _blindDrawValue = value; OnPropertyChanged(); } }

        private string _lastTeetime = "00:00";
        public string LastTeeTime { get { return _lastTeetime; } set { _lastTeetime = value; OnPropertyChanged(); } }

        public bool ShowBlindDrawValue = false;
        public bool ShowLastTeeTime = false;

        // These values are not shown in the UI, so no need for property changed events;
        public double StartTimeAverageInSeconds = 0;
        public double StartTimeStandardDeviationInSeconds = 0;
        public int TeeTimeCount = 0;

        public TeeTimeRequest()
        {
            Players = new TrulyObservableCollection<Player>();
        }

        public int GetHour()
        {
            // put the no preferences first
            if (string.IsNullOrEmpty(Preference)) return 1;
            if (Preference.StartsWith("None", false, System.Globalization.CultureInfo.InvariantCulture)) return 1;

            string hour = string.Empty;
            for (int i = 0; i < _preference.Length; i++)
            {
                if ((_preference[i] >= '0') && (_preference[i] <= '9'))
                {
                    hour += _preference[i];
                }
                else
                {
                    break;
                }
            }

            return int.Parse(hour);
        }

        public override string ToString()
        {
            string s = string.Empty;
            if(TeeTime != null)
            {
                s = TeeTime.StartTime + " ";
            }
            else if(!string.IsNullOrEmpty(Preference))
            {
                s = "(" + Preference + ") ";
            }

            s += PlayersToString();

            return s;
        }

        private string PlayersToString()
        {
            string s = string.Empty;

            for (int i = 0; i < Players.Count; i++)
            {
                if (i > 0)
                {
                    s += " --- ";
                }
                s += Players[i].Name;
                if (!string.IsNullOrEmpty(Players[i].Extra))
                {
                    switch (Players[i].Extra.ToLower())
                    {
                        case "flight1" :
                            s += " (F1)";
                            break;
                        case "flight2":
                            s += " (F2)";
                            break;
                        default:
                            s += "(" + Players[i].Extra + ")";
                            break;
                    }
                }
            }

            return s;
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
