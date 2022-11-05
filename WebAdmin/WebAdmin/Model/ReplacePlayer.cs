using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WebAdmin
{
    public class ReplacePlayer : INotifyPropertyChanged
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

        public Player Remove;
        public Player Add;
    }
}
