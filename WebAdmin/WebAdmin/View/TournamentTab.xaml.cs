using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WebAdmin.View
{
    /// <summary>
    /// Interaction logic for TournamentTab.xaml
    /// </summary>
    public partial class TournamentTab : UserControl
    {
        private string _pre = "<!DOCTYPE html><html><body>";
        private string _post = "</body></html>";

        public TournamentTab()
        {
            InitializeComponent();
        }

        private void DescriptionListbox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ShowDescription();
        }

        private void ShowDescription()
        {
            TournamentDescription descr = DescriptionListbox.SelectedItem as TournamentDescription;

            if (descr != null)
            {
                WebBrowser.NavigateToString(_pre + descr.Description + _post);
            }
            else
            {
                WebBrowser.NavigateToString(_pre + _post);
            }
        }

        
    }
}
