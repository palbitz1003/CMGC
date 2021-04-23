using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WebAdmin
{
    public class TeeTime : INotifyPropertyChanged
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

        private string _startTime;
        public string StartTime { get { return _startTime; } set { _startTime = value; OnPropertyChanged(); } }

        public TrulyObservableCollection<Player> Players = new TrulyObservableCollection<Player>();

        // For some reason, adding directly to the Players list doesn't trigger an event the UI is looking for
        public void AddPlayer(Player player)
        {
            Players.Add(player);
            OnPropertyChanged("Players");
        }

        public void RemovePlayer(Player player)
        {
            Players.Remove(player);
            OnPropertyChanged("Players");
        }

        public void ClearPlayers()
        {
            Players.Clear();
            OnPropertyChanged("Players");
        }

        public void PlayersChanged()
        {
            OnPropertyChanged("Players");
        }

        public override string ToString()
        {
            string s = StartTime + " ";
            for (int i = 0; i < Players.Count; i++ )
            {
                if (i != 0) s += " --- ";
                s += Players[i].Name;
                if (!string.IsNullOrEmpty(Players[i].Extra))
                {
                    switch (Players[i].Extra.ToLower())
                    {
                        case "flight1":
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
    }
}
