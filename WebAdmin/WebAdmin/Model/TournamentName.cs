using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace WebAdmin
{
    public class TournamentName : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        private string _name;
        public string Name { get { return _name; } set { _name = value; OnPropertyChanged("Name"); } }

        private DateTime _startDate;
        public DateTime StartDate { get { return _startDate; } set { _startDate = value; OnPropertyChanged("StartDate"); } }

        private DateTime _endDate;
        public DateTime EndDate { get { return _endDate; } set { _endDate = value; OnPropertyChanged("EndDate"); } }

        private DateTime _signupStartDate;
        public DateTime SignupStartDate { get { return _signupStartDate; } set { _signupStartDate = value; OnPropertyChanged("SignupStartDate"); } }

        private int _tournamentKey;
        public int TournamentKey { get { return _tournamentKey; } set { _tournamentKey = value; OnPropertyChanged("TournamentKey"); } }

        private bool _isEclectic;
        public bool IsEclectic { get { return _isEclectic; } set { _isEclectic = value; OnPropertyChanged("IsEclectic"); } }

        private bool _isMatchPlay;
        public bool IsMatchPlay { get { return _isMatchPlay; } set { _isMatchPlay = value; OnPropertyChanged("IsMatchPlay"); } }

        public override string ToString()
        {
            return StartDate.ToShortDateString() + ": " + Name;
        }
    }
}
