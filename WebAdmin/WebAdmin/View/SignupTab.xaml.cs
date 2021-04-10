using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
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
    /// Interaction logic for SignupTab.xaml
    /// </summary>
    public partial class SignupTab : UserControl
    {
        public SignupTab()
        {
            InitializeComponent();
        }

        private void OrderByRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (DataContext == null) return;

            if ((HistoricalRadioButton != null) && (HistoricalRadioButton.IsChecked != null) && (HistoricalRadioButton.IsChecked.Value == true))
            {
                ((WebAdmin.ViewModel.SignupTabViewModel)DataContext).OrderTeeTimeRequestsBy =
                    ViewModel.SignupTabViewModel.OrderTeeTimeRequestsByEnum.HistoricalTeeTimes;
            }
            else if ((BlindDrawRadioButton != null) && (BlindDrawRadioButton.IsChecked != null) && (BlindDrawRadioButton.IsChecked.Value == true))
            {
                ((WebAdmin.ViewModel.SignupTabViewModel)DataContext).OrderTeeTimeRequestsBy =
                    ViewModel.SignupTabViewModel.OrderTeeTimeRequestsByEnum.BlindDraw;
            }
            if ((RequestedTimeRadioButton != null) && (RequestedTimeRadioButton.IsChecked != null) && (RequestedTimeRadioButton.IsChecked.Value == true))
            {
                ((WebAdmin.ViewModel.SignupTabViewModel)DataContext).OrderTeeTimeRequestsBy =
                    ViewModel.SignupTabViewModel.OrderTeeTimeRequestsByEnum.RequestedTime;
            }
        }
    }
}
