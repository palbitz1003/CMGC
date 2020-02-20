using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace WebAdmin
{
    public class Player : INotifyPropertyChanged
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

        private string _GHIN;
        public string GHIN { get { return _GHIN; } set { _GHIN = value; OnPropertyChanged(); } }

        private string _handicap;
        public string Handicap { get { return _handicap; } set { _handicap = value; OnPropertyChanged(); } }

        private string _name;
        public string Name { get { return _name; } set { _name = value; OnPropertyChanged(); } }

        private string _extra;
        public string Extra { get { return _extra; } set { _extra = value; OnPropertyChanged(); } }

        //public TeeTimeRequest TeeTimeRequest { get; set; }
        public TeeTime TeeTime { get; set; }
        public int Position { get; set; }
        public string Email { get; set; }

        public override string ToString()
        {
            return Name + " (" + GHIN + ")";
        }
    }
}
