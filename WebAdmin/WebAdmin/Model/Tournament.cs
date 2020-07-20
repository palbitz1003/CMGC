using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.IO;

namespace WebAdmin
{
    public class Tournament : INotifyPropertyChanged
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

        private string _name;
        public string Name { get { return _name; } set { _name = value; OnPropertyChanged(); } }

        private string _year;
        public string Year { get { return _year; } set { _year = value; OnPropertyChanged(); } }

        private int _tournamentKey;
        public int TournamentKey { get { return _tournamentKey; } set { _tournamentKey = value; OnPropertyChanged(); } }

        private DateTime _startDate;
        public DateTime StartDate 
        { 
            get { return _startDate; } 
            set { 
                _startDate = value;
                OnPropertyChanged();

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
                OnPropertyChanged(); 
            } 
        }

        private DateTime _signupStartDate;
        public DateTime SignupStartDate { get { return _signupStartDate; } set { _signupStartDate = value; OnPropertyChanged(); } }

        private DateTime _signupEndDate;
        public DateTime SignupEndDate { get { return _signupEndDate; } set { _signupEndDate = value; OnPropertyChanged(); } }

        private DateTime _cancelEndDate;
        public DateTime CancelEndDate { get { return _cancelEndDate; } set { _cancelEndDate = value; OnPropertyChanged(); } }

        private bool _localHandicap;
        public bool LocalHandicap { get { return _localHandicap; } set { _localHandicap = value; OnPropertyChanged(); } }

        private bool _scgaTournament;
        public bool ScgaTournament { get { return _scgaTournament; } set { _scgaTournament = value; OnPropertyChanged(); } }

        public int TeamSize
        {
            get { return _teamSizeList[_teamSizeSelectedIndex]; }
            set
            {
                for (int i = 0; i < _teamSizeList.Count; i++)
                {
                    if(value == _teamSizeList[i])
                    {
                        _teamSizeSelectedIndex = i;
                        break;
                    }
                }
            }
        }

        private ObservableCollection<int> _teamSizeList;
        public ObservableCollection<int> TeamSizeList { get { return _teamSizeList; } set { _teamSizeList = value; OnPropertyChanged(); } }

        private int _teamSizeSelectedIndex;
        public int TeamSizeSelectedIndex { get { return _teamSizeSelectedIndex;} set { _teamSizeSelectedIndex = value; OnPropertyChanged();}}

        private int _tournamentDescriptionKey;
        public int TournamentDescriptionKey { get { return _tournamentDescriptionKey; } set { _tournamentDescriptionKey = value; OnPropertyChanged(); } }

        private int _cost;
        public int Cost { get { return _cost; } set { _cost = value; OnPropertyChanged(); } }

        private int _pool;
        public int Pool { get { return _pool; } set { _pool = value; OnPropertyChanged(); } }

        private string _chairmanName;
        public string ChairmanName { get { return _chairmanName; } set { _chairmanName = value; OnPropertyChanged(); } }

        private string _chairmanEmail;
        public string ChairmanEmail { get { return _chairmanEmail; } set { _chairmanEmail = value; OnPropertyChanged(); } }

        private string _chairmanPhone;
        public string ChairmanPhone { get { return _chairmanPhone; } set { _chairmanPhone = value; OnPropertyChanged(); } }

        private bool _sendEmail;
        public bool SendEmail { get { return _sendEmail; } set { _sendEmail = value; OnPropertyChanged(); } }

        private bool _requirePayment;
        public bool RequirePayment { get { return _requirePayment; } set { _requirePayment = value; OnPropertyChanged(); } }

        public bool StrokePlay
        {
            get { return !Eclectic && !Stableford && !MatchPlay && !AnnouncementOnly; }
            set
            {
                if (value)
                {
                    Eclectic = false;
                    Stableford = false;
                    MatchPlay = false;
                    AnnouncementOnly = false;
                }
            }
        }

        private bool _eclectic;
        public bool Eclectic { get { return _eclectic; } set { _eclectic = value; OnPropertyChanged(); OnPropertyChanged("StrokePlay"); } }

        private bool _stableford;
        public bool Stableford { get { return _stableford; } set { _stableford = value; OnPropertyChanged(); OnPropertyChanged("StrokePlay"); } }

        private bool _matchPlay;
        public bool MatchPlay { get { return _matchPlay; } set { _matchPlay = value; OnPropertyChanged(); OnPropertyChanged("StrokePlay"); } }

        private bool _announcementOnly;
        public bool AnnouncementOnly { get { return _announcementOnly; } set { _announcementOnly = value; OnPropertyChanged(); OnPropertyChanged("StrokePlay"); } }

        public bool NonSpecificTournament
        {
            get { return !ScgaQualifier && !SrClubChampionship && !AllowNonMemberSignup && !MemberGuest; }
            set
            {
                if (value)
                {
                    ScgaQualifier = false;
                    SrClubChampionship = false;
                    AllowNonMemberSignup = false;
                    MemberGuest = false;
                }
            }
        }

        private bool _scgaQualifier;
        public bool ScgaQualifier { get { return _scgaQualifier; } set { _scgaQualifier = value; OnPropertyChanged(); } }

        private bool _srClubChampionship;
        public bool SrClubChampionship { get { return _srClubChampionship; } set { _srClubChampionship = value; OnPropertyChanged(); } }

        private bool _onlineSignUp;
        public bool OnlineSignUp { get { return _onlineSignUp; } set { _onlineSignUp = value; OnPropertyChanged(); } }

        private bool _allowNonMemberSignup;
        public bool AllowNonMemberSignup { get { return _allowNonMemberSignup; } set { _allowNonMemberSignup = value; OnPropertyChanged(); } }

        private bool _memberGuest;
        public bool MemberGuest { get { return _memberGuest; } set { _memberGuest = value; OnPropertyChanged();} }

        private int _maxSignups;
        public int MaxSignups { get { return _maxSignups; } set { _maxSignups = value; OnPropertyChanged(); } }

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
            ChairmanName = String.Empty;
            ChairmanEmail = String.Empty;
            ChairmanPhone = String.Empty;
            Eclectic = false;
            Stableford = false;
            ScgaQualifier = false;
            SrClubChampionship = false;
            AnnouncementOnly = false;
            SendEmail = true;
            RequirePayment = true;
            MemberGuest = false;
            MaxSignups = 0;
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
            values.Add(new KeyValuePair<string, string>("ChairmanName", ChairmanName));
            values.Add(new KeyValuePair<string, string>("ChairmanEmail", ChairmanEmail));
            values.Add(new KeyValuePair<string, string>("ChairmanPhone", ChairmanPhone));
            values.Add(new KeyValuePair<string, string>("Stableford", Stableford ? "1" : "0"));
            values.Add(new KeyValuePair<string, string>("Eclectic", Eclectic ? "1" : "0"));
            values.Add(new KeyValuePair<string, string>("AnnouncementOnly", AnnouncementOnly ? "1" : "0"));
            values.Add(new KeyValuePair<string, string>("SendEmail", SendEmail ? "1" : "0"));
            values.Add(new KeyValuePair<string, string>("RequirePayment", RequirePayment ? "1" : "0"));
            values.Add(new KeyValuePair<string, string>("SCGAQualifier", ScgaQualifier ? "1" : "0"));
            values.Add(new KeyValuePair<string, string>("SrClubChampionship", SrClubChampionship ? "1" : "0"));
            values.Add(new KeyValuePair<string, string>("OnlineSignUp", OnlineSignUp ? "1" : "0"));
            values.Add(new KeyValuePair<string, string>("MatchPlay", MatchPlay ? "1" : "0"));
            values.Add(new KeyValuePair<string, string>("AllowNonMemberSignup", AllowNonMemberSignup ? "1" : "0"));
            values.Add(new KeyValuePair<string, string>("MemberGuest", MemberGuest ? "1" : "0"));
            values.Add(new KeyValuePair<string, string>("MaxSignups", MaxSignups.ToString()));

            return values;
        }
    }
}
