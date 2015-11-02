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
    public class TournamentDescription : INotifyPropertyChanged
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

        private int _tournamentDescriptionKey;
        public int TournamentDescriptionKey { get { return _tournamentDescriptionKey; } set { _tournamentDescriptionKey = value; OnPropertyChanged(); } }

        private string _name;
        public string Name { get { return _name; } set { _name = value; OnPropertyChanged(); } }

        private string _description;
        public string Description { get { return _description; } set { _description = value; OnPropertyChanged(); } }

        public override string ToString()
        {
            return Name;
        }

        public void Clear()
        {
            TournamentDescriptionKey = -1;
            Name = string.Empty;
            Description = string.Empty;
        }
    }
}
