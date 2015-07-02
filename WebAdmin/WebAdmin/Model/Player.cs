﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace WebAdmin
{
    public class Player : INotifyPropertyChanged
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

        private string _GHIN;
        public string GHIN { get { return _GHIN; } set { _GHIN = value; OnPropertyChanged("GHIN"); } }

        private string _handicap;
        public string Handicap { get { return _handicap; } set { _handicap = value; OnPropertyChanged("Handicap"); } }

        private string _name;
        public string Name { get { return _name; } set { _name = value; OnPropertyChanged("Name"); } }

        private string _extra;
        public string Extra { get { return _extra; } set { _extra = value; OnPropertyChanged("Extra"); } }

        //public TeeTimeRequest TeeTimeRequest { get; set; }
        public TeeTime TeeTime { get; set; }
        public int Position { get; set; }

        public override string ToString()
        {
            return Name + " (" + GHIN + ")";
        }
    }
}
