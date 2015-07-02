using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace WebAdmin
{
    public class ClosestToThePin : INotifyPropertyChanged, ICloneable
    {
        #region PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
        #endregion

        public Object Clone()
        {
            return this.MemberwiseClone();
        }

        private DateTime _date;
        public DateTime Date { get { return _date; } set { _date = value; OnPropertyChanged("Date"); } }

        private int _hole;
        public int Hole { get { return _hole; } set { _hole = value; OnPropertyChanged("Hole"); } }

        private string _player;
        public string Player { get { return _player; } set { _player = value; OnPropertyChanged("Player"); } }

        private int _GHIN;
        public int GHIN { get { return _GHIN; } set { _GHIN = value; OnPropertyChanged("GHIN"); } }

        private string _distance;
        public string Distance {  get { return _distance; } set {_distance = value; OnPropertyChanged("Distance");}}

        private string _prize;
        public string Prize { get { return _prize; } set { _prize = value; OnPropertyChanged("Prize"); } }

        private List<string> _businesses;
        public List<string> Businesses
        {
            get { return _businesses; }
            set { _businesses = value; OnPropertyChanged("Businesses");  }
        }

        private string _business;
        public string Business 
        { 
            get { return _business; }
            set { _business = value; OnPropertyChanged("Business"); }
        }
    }
}
