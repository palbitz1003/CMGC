using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WebAdmin.ViewModel;

namespace WebAdmin.View
{
    /// <summary>
    /// Interaction logic for AdminTab.xaml
    /// </summary>
    public partial class AdminTab : UserControl
    {

        private List<GHINEntry> _ghinList = null;
        private bool _changingPlayerName = false;
        private bool _changingPlayerGhin = false;

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
            GHINTextBox.Text = TabViewModelBase.Options.GHINFileName;
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

        /*
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
        */

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
                    try
                    {
                        _changingPlayerName = true;
                        PlayerNameTextBox.Text = AutoCompleteFeedback.Text;
                    }
                    finally
                    {
                        _changingPlayerName = false;
                    }

                    AutoCompleteFeedback.Text = string.Empty;
                    
                    e.Handled = true;
                }
                PlayerDuesTextBox.Text = string.Empty;
            }
        }

        private void PlayerNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _changingPlayerGhin = false;

            // Ignore if changed from event handler
            if (_changingPlayerName) return;

            LoadGHIN();

            PlayerDuesTextBox.Text = string.Empty;
            OnPropertyChanged("PlayerDues");

            if (!string.IsNullOrEmpty(PlayerNameTextBox.Text) && (_ghinList != null))
            {
                foreach (var player in _ghinList)
                {
                    if (player.LastNameFirstName.StartsWith(PlayerNameTextBox.Text, StringComparison.InvariantCultureIgnoreCase))
                    {
                        AutoCompleteFeedback.Text = player.LastNameFirstName;

                        try
                        {
                            _changingPlayerGhin = true;
                            PlayerGhinTextBox.Text = player.GHIN.ToString();
                            OnPropertyChanged("PlayerGhin");
                        }
                        finally
                        {
                            _changingPlayerGhin = false;
                        }

                        return;
                    }
                }
            }

            AutoCompleteFeedback.Text = null;
            try
            {
                _changingPlayerGhin = true;
                PlayerGhinTextBox.Text = string.Empty;
            }
            finally
            {
                _changingPlayerGhin = false;
            }
        }

        private void PlayerNameTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(AutoCompleteFeedback.Text))
            {
                try
                {
                    _changingPlayerName = true;
                    PlayerNameTextBox.Text = AutoCompleteFeedback.Text;
                }
                finally
                {
                    _changingPlayerName = false;
                }
                AutoCompleteFeedback.Text = string.Empty;
            }
            PlayerDuesTextBox.Text = string.Empty;
        }

        private void PlayerGhinTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _changingPlayerName = false;

            // Ignore if changed from event handler
            if (_changingPlayerGhin) return;

            LoadGHIN();

            PlayerDuesTextBox.Text = string.Empty;
            OnPropertyChanged("PlayerDues");

            if (!string.IsNullOrEmpty(PlayerGhinTextBox.Text) && (_ghinList != null))
            {
                int ghin;
                if (int.TryParse(PlayerGhinTextBox.Text.Trim(), out ghin))
                {
                    foreach (var player in _ghinList)
                    {

                        if (player.GHIN == ghin)
                        {
                            try
                            {
                                _changingPlayerName = true;
                                PlayerNameTextBox.Text = player.LastNameFirstName;
                                OnPropertyChanged("PlayerName");
                            }
                            finally
                            {
                                _changingPlayerName = false;
                            }
                            return;
                        }
                    }
                }
            }

            AutoCompleteFeedback.Text = null;
            try
            {
                _changingPlayerName = true;
                PlayerNameTextBox.Text = string.Empty;
            }
            finally
            {
                _changingPlayerName = false;
            }
        }
    }
}
