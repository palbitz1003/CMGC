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
    /// Interaction logic for ChangeFlightWindow.xaml
    /// </summary>
    public partial class ChangeFlightWindow : Window
    {
        public List<Player> PlayerList { get; set; }
        public Player Player { get; set; }
        public bool AllowGuest = false;
        public bool RequiresFlight = false;
        private bool ignoreTextChange;

        public ChangeFlightWindow()
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
                    FlightTextBox.Text = Player.Extra;
                    //AutoCompleteFeedback.Text = string.Empty;
                    ignoreTextChange = false;
                    e.Handled = true;
                }
            }
        }

        private void PlayerTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ignoreTextChange) return;

            if (!string.IsNullOrEmpty(PlayerTextBox.Text))
            {
                foreach (var player in PlayerList)
                {
                    if (player.Name.StartsWith(PlayerTextBox.Text, StringComparison.InvariantCultureIgnoreCase))
                    {
                        AutoCompleteFeedback.Text = ((player.TeeTime == null) ? "(Unassigned) " : "(" + player.TeeTime.StartTime + ") ") + player.Name;
                        Player = player;
                        FlightTextBox.Text = Player.Extra;
                        return;
                    }
                }
            }
            Player = null;
            FlightTextBox.Text = string.Empty;
            //AutoCompleteFeedback.Text = null;
        }

        private void PlayerTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (Player != null)
            {
                PlayerTextBox.Text = Player.Name;
                FlightTextBox.Text = Player.Extra;
                //AutoCompleteFeedback.Text = string.Empty;
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Player = null;
            this.Close();
        }

        private void ChangeButton_Click(object sender, RoutedEventArgs e)
        {
            if (AllowGuest)
            {
                if (string.IsNullOrEmpty(FlightTextBox.Text))
                {
                    MessageBox.Show("Please fill in the flight with M, G, or G - <tee box>");
                    return;
                }
                if ((string.Compare(Player.Extra, "M") == 0) && (string.Compare(FlightTextBox.Text, "M") != 0))
                {
                    MessageBox.Show("Can't change member away from flight M");
                    return;
                }
                if (Player.Extra.StartsWith("G") && !FlightTextBox.Text.StartsWith("G"))
                {
                    MessageBox.Show("Can't change guest away from flight G");
                    return;
                }
            }
            else if (RequiresFlight)
            {
                if (string.IsNullOrEmpty(FlightTextBox.Text))
                {
                    MessageBox.Show("Please fill in the flight with CH or F1-F5");
                    return;
                }
                switch (FlightTextBox.Text)
                {
                    case "CH":
                    case "F1":
                    case "F2":
                    case "F3":
                    case "F4":
                    case "F5":
                        break;
                    default:
                        MessageBox.Show("Please fill in the flight with CH or F1-F5");
                        return;
                }
            }

            Player.Extra = FlightTextBox.Text;
            DialogResult = true;
            this.Close();
        }
    }
}
