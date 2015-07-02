using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.IO;

namespace WebAdmin
{
    public class Tournament : INotifyPropertyChanged
    {
        //private object _property;
        //public object Property { get { return _property; } set { _property = value; OnPropertyChanged("Property"); } }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        public enum TournamentTypes
        {
            Stroke,
            Eclectic,
            Stableford
        };

        public enum TournamentSubTypes
        {
            None,
            ScgaQualifier,
            SrClubChampionship
        };

        private string _name;
        public string Name { get { return _name; } set { _name = value; OnPropertyChanged("Name"); } }

        private string _year;
        public string Year { get { return _year; } set { _year = value; OnPropertyChanged("Year"); } }

        private int _tournamentKey;
        public int TournamentKey { get { return _tournamentKey; } set { _tournamentKey = value; OnPropertyChanged("TournamentKey"); } }

        private DateTime _startDate;
        public DateTime StartDate 
        { 
            get { return _startDate; } 
            set { 
                _startDate = value;
                OnPropertyChanged("StartDate");

                EndDate = value.Add(TimeSpan.FromDays(1));
                SignupStartDate = value.Subtract(TimeSpan.FromDays(28));
                SignupEndDate = value.Subtract(TimeSpan.FromDays(10));
                CancelEndDate = value.Subtract(TimeSpan.FromDays(1));
                } 
        }

        private DateTime _endDate;
        public DateTime EndDate 
        { 
            get { return _endDate; } 
            set { 
                _endDate = value;
                Cost = (_endDate == _startDate) ? 10 : 25;
                OnPropertyChanged("EndDate"); 
            } 
        }

        private DateTime _signupStartDate;
        public DateTime SignupStartDate { get { return _signupStartDate; } set { _signupStartDate = value; OnPropertyChanged("SignupStartDate"); } }

        private DateTime _signupEndDate;
        public DateTime SignupEndDate { get { return _signupEndDate; } set { _signupEndDate = value; OnPropertyChanged("SignupEndDate"); } }

        private DateTime _cancelEndDate;
        public DateTime CancelEndDate { get { return _cancelEndDate; } set { _cancelEndDate = value; OnPropertyChanged("CancelEndDate"); } }

        private bool _localHandicap;
        public bool LocalHandicap { get { return _localHandicap; } set { _localHandicap = value; OnPropertyChanged("LocalHandicap"); } }

        private bool _scgaTournament;
        public bool ScgaTournament { get { return _scgaTournament; } set { _scgaTournament = value; OnPropertyChanged("ScgaTournament"); } }

        

        public int TeamSize { get { return _teamSizeList[_teamSizeSelectedIndex]; }  }

        private ObservableCollection<int> _teamSizeList;
        public ObservableCollection<int> TeamSizeList { get { return _teamSizeList; } set { _teamSizeList = value; OnPropertyChanged("TeamSizeList"); } }

        private int _teamSizeSelectedIndex;
        public int TeamSizeSelectedIndex { get { return _teamSizeSelectedIndex;} set { _teamSizeSelectedIndex = value; OnPropertyChanged("TeamSizeSelectedIndex");}}

        private int _tournamentDescriptionKey;
        public int TournamentDescriptionKey { get { return _tournamentDescriptionKey; } set { _tournamentDescriptionKey = value; OnPropertyChanged("TournamentDescriptionKey"); } }

        private int _cost;
        public int Cost { get { return _cost; } set { _cost = value; OnPropertyChanged("Cost"); } }

        private int _pool;
        public int Pool { get { return _pool; } set { _pool = value; OnPropertyChanged("Pool"); } }

        private TournamentChairman _chairman = new TournamentChairman();
        public TournamentChairman Chairman { get { return _chairman; } set { _chairman = value; OnPropertyChanged("Chairman"); } }

        private bool _sendEmail;
        public bool SendEmail { get { return _sendEmail; } set { _sendEmail = value; OnPropertyChanged("SendEmail"); } }

        private bool _requirePayment;
        public bool RequirePayment { get { return _requirePayment; } set { _requirePayment = value; OnPropertyChanged("RequirePayment"); } }

        public bool Eclectic { get { return TournamentType == TournamentTypes.Eclectic; } }
        public bool Stableford { get { return TournamentType == TournamentTypes.Stableford; } }

        private TournamentTypes _tournamentType;
        public TournamentTypes TournamentType
        {
            get { return _tournamentType; }
            set { _tournamentType = value; OnPropertyChanged("TournamentType"); }
        }

        public bool ScgaQualifier { get { return TournamentSubType == TournamentSubTypes.ScgaQualifier; } }
        public bool SrClubChampionship { get { return TournamentSubType == TournamentSubTypes.SrClubChampionship; } }

        private TournamentSubTypes _tournamentSubType;
        public TournamentSubTypes TournamentSubType
        {
            get { return _tournamentSubType; }
            set { _tournamentSubType = value; OnPropertyChanged("TournamentSubType"); }
        }

        public Tournament()
        {
            TeamSizeList = new ObservableCollection<int>() { 1, 2, 4 };
            Reset();
        }

        public void Reset()
        {
            TournamentKey = -1;
            TeamSizeSelectedIndex = 0;
            Name = String.Empty;
            int thisYear = DateTime.Now.Year;
            int thisMonth = DateTime.Now.Month;
            StartDate = new DateTime(thisYear, thisMonth, 1);
            EndDate = new DateTime(thisYear, thisMonth, 1);
            SignupStartDate = new DateTime(thisYear, thisMonth, 1);
            SignupEndDate = new DateTime(thisYear, thisMonth, 1);
            CancelEndDate = new DateTime(thisYear, thisMonth, 1);
            Year = thisYear.ToString();
            LocalHandicap = false;
            ScgaTournament = false;
            TournamentDescriptionKey = -1;
            Cost = 25;
            Pool = 10;
            Chairman.Name = String.Empty;
            Chairman.Email = String.Empty;
            Chairman.Phone = String.Empty;
            TournamentType = TournamentTypes.Stroke;
            TournamentSubType = TournamentSubTypes.None;
            SendEmail = true;
            RequirePayment = true;
        }

        public List<KeyValuePair<string, string>> ToKeyValuePairs()
        {
            List<KeyValuePair<string, string>> values = new List<KeyValuePair<string, string>>();
            values.Add(new KeyValuePair<string, string>("TournamentKey", TournamentKey.ToString()));
            values.Add(new KeyValuePair<string, string>("Name", Name));
            values.Add(new KeyValuePair<string, string>("Year", Year));
            values.Add(new KeyValuePair<string, string>("StartDate", StartDate.ToString("yyyy-MM-dd")));
            values.Add(new KeyValuePair<string, string>("EndDate", EndDate.ToString("yyyy-MM-dd")));
            values.Add(new KeyValuePair<string, string>("SignupStartDate", SignupStartDate.ToString("yyyy-MM-dd")));
            values.Add(new KeyValuePair<string, string>("SignupEndDate", SignupEndDate.ToString("yyyy-MM-dd")));
            values.Add(new KeyValuePair<string, string>("CancelEndDate", CancelEndDate.ToString("yyyy-MM-dd")));
            values.Add(new KeyValuePair<string, string>("LocalHandicap", LocalHandicap ? "1" : "0"));
            values.Add(new KeyValuePair<string, string>("ScgaTournament", ScgaTournament ? "1" : "0"));
            values.Add(new KeyValuePair<string, string>("TeamSize", TeamSize.ToString()));
            values.Add(new KeyValuePair<string, string>("TournamentDescriptionKey", TournamentDescriptionKey.ToString()));
            values.Add(new KeyValuePair<string, string>("Cost", Cost.ToString()));
            values.Add(new KeyValuePair<string, string>("Pool", Pool.ToString()));
            values.Add(new KeyValuePair<string, string>("ChairmanName", Chairman.Name));
            values.Add(new KeyValuePair<string, string>("ChairmanEmail", Chairman.Email));
            values.Add(new KeyValuePair<string, string>("ChairmanPhone", Chairman.Phone));
            values.Add(new KeyValuePair<string, string>("Stableford", Stableford ? "1" : "0"));
            values.Add(new KeyValuePair<string, string>("Eclectic", Eclectic ? "1" : "0"));
            values.Add(new KeyValuePair<string, string>("SendEmail", SendEmail ? "1" : "0"));
            values.Add(new KeyValuePair<string, string>("RequirePayment", RequirePayment ? "1" : "0"));
            values.Add(new KeyValuePair<string, string>("SCGAQualifier", ScgaQualifier ? "1" : "0"));
            values.Add(new KeyValuePair<string, string>("SrClubChampionship", SrClubChampionship ? "1" : "0"));

            return values;
        }
    }
}
