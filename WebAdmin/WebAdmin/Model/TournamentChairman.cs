using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace WebAdmin
{
    public class TournamentChairman : INotifyPropertyChanged
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

        private string _email;
        public string Email { get { return _email; } set { _email = value; OnPropertyChanged("Email"); } }

        private string _phone;
        public string Phone { get { return _phone; } set { _phone = value; OnPropertyChanged("Phone"); } }

        public override string ToString()
        {
            return Name;
        }
    }
}
