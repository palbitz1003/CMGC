using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WebAdmin.View
{
    /// <summary>
    /// Interaction logic for RemovePlayerWindow.xaml
    /// </summary>
    public partial class RemovePlayerWindow : Window
    {
        public List<Player> PlayerList { get; set; }
        public Player Player { get; set; }

        private bool ignoreTextChange;

        public RemovePlayerWindow()
        {
            InitializeComponent();
            PlayerList = new List<Player>();
            PlayerTextBox.Focus();
            Player = null;
            ignoreTextChange = false;
        }

        private void PlayerTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                if (Player != null)
                {
                    ignoreTextChange = true;
                    PlayerTextBox.Text = Player.Name;
                    ignoreTextChange = false;
                    e.Handled = true;
                }
            }
        }

        private void PlayerTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ignoreTextChange) return;

            foreach (var player in PlayerList)
            {
                if (player.Name.StartsWith(PlayerTextBox.Text, StringComparison.InvariantCultureIgnoreCase))
                {
                    AutoCompleteFeedback.Text = ((player.TeeTime == null) ? "(Unassigned) " : "(" + player.TeeTime.StartTime + ") ") + player.Name;
                    Player = player;
                    return;
                }
            }
            Player = null;
            AutoCompleteFeedback.Text = null;
        }

        private void PlayerTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            /*
            if (!string.IsNullOrEmpty(AutoCompleteFeedback.Text))
            {
                PlayerTextBox.Text = AutoCompleteFeedback.Text;
                
                Player.Name = AutoCompleteFeedback.Text;
                Player.GHIN = _ghin;
                Player.Email = _email;
                
                AutoCompleteFeedback.Text = string.Empty;
            }
            */
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Player = null;
            this.Close();
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            this.Close();
        }
    }
}
