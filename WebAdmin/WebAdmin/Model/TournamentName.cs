using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace WebAdmin
{
    public class TournamentName : INotifyPropertyChanged
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

        private DateTime _startDate;
        public DateTime StartDate { get { return _startDate; } set { _startDate = value; OnPropertyChanged(); } }

        private DateTime _endDate;
        public DateTime EndDate { get { return _endDate; } set { _endDate = value; OnPropertyChanged(); } }

        private DateTime _signupStartDate;
        public DateTime SignupStartDate { get { return _signupStartDate; } set { _signupStartDate = value; OnPropertyChanged(); } }

        private int _tournamentKey;
        public int TournamentKey { get { return _tournamentKey; } set { _tournamentKey = value; OnPropertyChanged(); } }

        private bool _isEclectic;
        public bool IsEclectic { get { return _isEclectic; } set { _isEclectic = value; OnPropertyChanged(); } }

        private bool _matchPlay;
        public bool MatchPlay { get { return _matchPlay; } set { _matchPlay = value; OnPropertyChanged(); } }

        public override string ToString()
        {
            return StartDate.ToShortDateString() + ": " + Name;
        }
    }
}
