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
    /// Interaction logic for ClosestToThePinWindow.xaml
    /// </summary>
    public partial class ClosestToThePinWindow : Window
    {
        public List<GHINEntry> GHINList { get; set; }
        public int GHIN { get; private set; }

        public ClosestToThePinWindow()
        {
            InitializeComponent();
            GHINList = new List<GHINEntry>();
            PlayerTextBox.Focus();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if ( (string.IsNullOrEmpty(PlayerTextBox.Text) ||
                  string.IsNullOrEmpty(DistanceTextBox.Text) ||
                  string.IsNullOrEmpty(BusinessesComboBox.Text)) &&
                !(string.IsNullOrEmpty(PlayerTextBox.Text) &&
                  string.IsNullOrEmpty(DistanceTextBox.Text) &&
                  string.IsNullOrEmpty(BusinessesComboBox.Text)))
            {
                MessageBox.Show("Please fill in all of the fields or clear all of the fields before saving");
                return;
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
            if(e.Key == Key.Return)
            {
                if(!string.IsNullOrEmpty(AutoCompleteFeedback.Text))
                {
                    PlayerTextBox.Text = AutoCompleteFeedback.Text;
                    AutoCompleteFeedback.Text = string.Empty;
                    e.Handled = true;
                }
            }
        }

        private void PlayerTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(PlayerTextBox.Text))
            {
                foreach (var player in GHINList)
                {
                    if (player.LastNameFirstName.StartsWith(PlayerTextBox.Text, StringComparison.InvariantCultureIgnoreCase))
                    {
                        AutoCompleteFeedback.Text = player.LastNameFirstName;
                        GHIN = player.GHIN;
                        return;
                    }
                }
            }
            AutoCompleteFeedback.Text = null;
            GHIN = 0;
        }

        private void PlayerTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(AutoCompleteFeedback.Text))
            {
                PlayerTextBox.Text = AutoCompleteFeedback.Text;
                AutoCompleteFeedback.Text = string.Empty;
            }
        }
    }
}
