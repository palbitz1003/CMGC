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

        private string _preference;
        public string Preference { get { return _preference; } set { _preference = value; OnPropertyChanged(); } }

        public TrulyObservableCollection<Player> Players { get; set;}

        // Do not make TeeTime a property that notifies when it is changed. When you click on an unassigned 
        // tee time request, the event triggers an extra attempt to put the item on the assigned list.
        public TeeTime TeeTime { get; set; }

        public string PlayerList {
            get { return PlayersToString(); }
        }

        public string RequestedTimeAndPlayerList
        {
            get {
                if (_paid)
                {
                    return "paid: " + _blindDrawValue + " " + ToString();
                }
                else if (_waitlisted)
                {
                    return "wait: " + _blindDrawValue + " " + ToString();
                }
                else
                {
                    return "         " + _blindDrawValue + " " + ToString();
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

        public TeeTimeRequest()
        {
            Players = new TrulyObservableCollection<Player>();
        }

        public int GetHour()
        {
            if (string.IsNullOrEmpty(Preference)) return 100;
            if (Preference.StartsWith("None", false, System.Globalization.CultureInfo.InvariantCulture)) return 100;

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

            if(string.IsNullOrEmpty(hour)) return 100;

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
