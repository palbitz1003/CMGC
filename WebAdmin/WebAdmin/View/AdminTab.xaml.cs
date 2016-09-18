using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WebAdmin.ViewModel;

namespace WebAdmin.View
{
    /// <summary>
    /// Interaction logic for AdminTab.xaml
    /// </summary>
    public partial class AdminTab : UserControl
    {

        private List<GHINEntry> _ghinList = null;

        #region PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
        #endregion

        public AdminTab()
        {
            InitializeComponent();

            WebSiteTextBox.Text = TabViewModelBase.Options.WebSite;
            ScriptFolderTextBox.Text = TabViewModelBase.Options.ScriptFolder;
            WaitListTextBox.Text = TabViewModelBase.Options.WaitListFileName;
            RosterTextBox.Text = TabViewModelBase.Options.RosterFileName;
            GHINTextBox.Text = TabViewModelBase.Options.GHINFileName;
            LocalHandicapTextBox.Text = TabViewModelBase.Options.LocalHandicapFileName;
        }

        private void BrowseWaitListButton_Click(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog 
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            if (!string.IsNullOrEmpty(WaitListTextBox.Text))
            {
                string dir = System.IO.Path.GetDirectoryName(WaitListTextBox.Text);
                if (Directory.Exists(dir))
                {
                    dlg.InitialDirectory = dir;
                }
            }

            // Set filter for file extension and default file extension 
            dlg.DefaultExt = ".csv";
            dlg.Filter = "CSV Files (*.csv)|*.csv";

            // Display OpenFileDialog by calling ShowDialog method 
            Nullable<bool> result = dlg.ShowDialog();

            // Get the selected file name and display in a TextBox 
            if (result == true)
            {
                WaitListTextBox.Text = dlg.FileName;
                TabViewModelBase.Options.WaitListFileName = dlg.FileName;
            }
        }

        private void BrowseRosterButton_Click(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog 
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            if (!string.IsNullOrEmpty(RosterTextBox.Text))
            {
                string dir = System.IO.Path.GetDirectoryName(RosterTextBox.Text);
                if (Directory.Exists(dir))
                {
                    dlg.InitialDirectory = dir;
                }
            }

            // Set filter for file extension and default file extension 
            dlg.DefaultExt = ".csv";
            dlg.Filter = "CSV Files (*.csv)|*.csv";

            // Display OpenFileDialog by calling ShowDialog method 
            Nullable<bool> result = dlg.ShowDialog();

            // Get the selected file name and display in a TextBox 
            if (result == true)
            {
                RosterTextBox.Text = dlg.FileName;
                TabViewModelBase.Options.RosterFileName = dlg.FileName;
            }
        }

        private void BrowseGHINButton_Click(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog 
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            if (!string.IsNullOrEmpty(GHINTextBox.Text))
            {
                string dir = System.IO.Path.GetDirectoryName(GHINTextBox.Text);
                if (Directory.Exists(dir))
                {
                    dlg.InitialDirectory = dir;
                }
            }

            // Set filter for file extension and default file extension 
            dlg.DefaultExt = ".csv";
            dlg.Filter = "CSV Files (*.csv)|*.csv";

            // Display OpenFileDialog by calling ShowDialog method 
            Nullable<bool> result = dlg.ShowDialog();

            // Get the selected file name and display in a TextBox 
            if (result == true)
            {
                GHINTextBox.Text = dlg.FileName;
                TabViewModelBase.Options.GHINFileName = dlg.FileName;

                TabViewModelBase.GHINEntries = GHINEntry.LoadGHIN(TabViewModelBase.Options.GHINFileName);
            }
        }

        private void BrowseLocalHandicapButton_Click(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog 
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            if (!string.IsNullOrEmpty(LocalHandicapTextBox.Text))
            {
                string dir = System.IO.Path.GetDirectoryName(LocalHandicapTextBox.Text);
                if (Directory.Exists(dir))
                {
                    dlg.InitialDirectory = dir;
                }
            }

            // Set filter for file extension and default file extension 
            dlg.DefaultExt = ".csv";
            dlg.Filter = "CSV Files (*.csv)|*.csv";

            // Display OpenFileDialog by calling ShowDialog method 
            Nullable<bool> result = dlg.ShowDialog();

            // Get the selected file name and display in a TextBox 
            if (result == true)
            {
                LocalHandicapTextBox.Text = dlg.FileName;
                TabViewModelBase.Options.LocalHandicapFileName = dlg.FileName;

                if (File.Exists(dlg.FileName))
                {
                    DateTime d = File.GetLastWriteTime(dlg.FileName);
                    LocalHandicapDatePicker.SelectedDate = new DateTime(d.Year, d.Month, (d.Day >= 15) ? 15 : 1);
                }
            }
        }

        private void WebSiteTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            TabViewModelBase.Options.WebSite = WebSiteTextBox.Text;

            WebAddresses.BaseAddress = "https://" + TabViewModelBase.Options.WebSite;
        }

        private void ScriptFolderTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            TabViewModelBase.Options.ScriptFolder = ScriptFolderTextBox.Text;

            WebAddresses.ScriptFolder = string.IsNullOrEmpty(TabViewModelBase.Options.ScriptFolder)
                ? string.Empty
                : "/" + TabViewModelBase.Options.ScriptFolder;
        }

        private void LoadGHIN()
        {
            try
            {
                if ((_ghinList == null) && 
                    !string.IsNullOrEmpty(TabViewModelBase.Options.GHINFileName) && 
                    File.Exists(TabViewModelBase.Options.GHINFileName))
                {
                    _ghinList = GHINEntry.LoadGHIN(TabViewModelBase.Options.GHINFileName);
                }
            }
            catch
            {
                // no error
            }
        }

        private void PlayerNameTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                if (!string.IsNullOrEmpty(AutoCompleteFeedback.Text))
                {
                    PlayerNameTextBox.Text = AutoCompleteFeedback.Text;
                    AutoCompleteFeedback.Text = string.Empty;
                    e.Handled = true;
                }
                PlayerDuesTextBox.Text = string.Empty;
            }
        }

        private void PlayerNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadGHIN();

            if (!string.IsNullOrEmpty(PlayerNameTextBox.Text) && (_ghinList != null))
            {
                foreach (var player in _ghinList)
                {
                    if (player.LastNameFirstName.StartsWith(PlayerNameTextBox.Text, StringComparison.InvariantCultureIgnoreCase))
                    {
                        AutoCompleteFeedback.Text = player.LastNameFirstName;
                        PlayerGhinTextBox.Text = player.GHIN.ToString();
                        OnPropertyChanged("PlayerGHIN");
                        return;
                    }
                }
            }

            AutoCompleteFeedback.Text = null;
            PlayerGhinTextBox.Text = string.Empty;
            PlayerDuesTextBox.Text = string.Empty;
        }

        private void PlayerNameTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(AutoCompleteFeedback.Text))
            {
                PlayerNameTextBox.Text = AutoCompleteFeedback.Text;
                AutoCompleteFeedback.Text = string.Empty;
            }
            PlayerDuesTextBox.Text = string.Empty;
        }
    }
}
