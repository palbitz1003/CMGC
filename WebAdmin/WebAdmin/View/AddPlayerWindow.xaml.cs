using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace WebAdmin.View
{
    /// <summary>
    /// Interaction logic for AddPlayerWindow.xaml
    /// </summary>
    public partial class AddPlayerWindow : Window
    {

        public List<GHINEntry> GHINList { get; set; }
        public Player Player { get; set; }
        public bool RequiresFlight = false;

        private string _ghin;
        private string _email;

        public AddPlayerWindow()
        {
            InitializeComponent();
            GHINList = new List<GHINEntry>();
            PlayerTextBox.Focus();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(PlayerTextBox.Text))
            {
                MessageBox.Show("Please fill in the name before saving");
                return;
            }
            if (string.IsNullOrEmpty(Player.Extra))
            {
                // Only complain about a missing flight if flights are required
                if (RequiresFlight)
                {
                    MessageBox.Show("Please fill in the flight with CH or F1-F5");
                    return;
                }
            }
            else
            {
                // If Player.Extra is filled in, make sure the value is valid
                switch (Player.Extra)
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
            DialogResult = true;
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void PlayerTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                if (!string.IsNullOrEmpty(AutoCompleteFeedback.Text))
                {
                    PlayerTextBox.Text = AutoCompleteFeedback.Text;
                    Player.Name = AutoCompleteFeedback.Text;
                    Player.GHIN = _ghin;
                    Player.Email = _email;
                    AutoCompleteFeedback.Text = string.Empty;
                    e.Handled = true;
                }
            }
        }

        private void PlayerTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            foreach (var player in GHINList)
            {
                if (player.LastNameFirstName.StartsWith(PlayerTextBox.Text, StringComparison.InvariantCultureIgnoreCase))
                {
                    AutoCompleteFeedback.Text = player.LastNameFirstName;
                    _ghin = player.GHIN.ToString();
                    _email = player.Email;
                    return;
                }
            }
            AutoCompleteFeedback.Text = null;
            _ghin = string.Empty;
            _email = string.Empty;
        }

        private void PlayerTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(AutoCompleteFeedback.Text))
            {
                PlayerTextBox.Text = AutoCompleteFeedback.Text;
                Player.Name = AutoCompleteFeedback.Text;
                Player.GHIN = _ghin;
                Player.Email = _email;
                AutoCompleteFeedback.Text = string.Empty;
            }
        }
    }
}
