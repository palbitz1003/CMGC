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
using System.ComponentModel;
using WebAdmin.ViewModel;

namespace WebAdmin.View
{
    /// <summary>
    /// Interaction logic for SignupTab.xaml
    /// </summary>
    public partial class SignupTab : UserControl
    {
        INotifyPropertyChanged previous;

        public SignupTab()
        {
            InitializeComponent();
            previous = (INotifyPropertyChanged)this.DataContext;
            DataContextChanged += (sender, args) => SubscribeToTeeTimeRequestChanges((INotifyPropertyChanged)args.NewValue);
            SubscribeToTeeTimeRequestChanges(previous);
            PrevWaitlistTextBox.Text = TabViewModelBase.Options.SignupWaitListFileName;
            TODOListBox.IsVisibleChanged += TODOListBox_IsVisibleChanged;
        }

        private void TODOListBox_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                DisplayTodoListboxItems();
            }
        }

        // subscriber
        private void SubscribeToTeeTimeRequestChanges(INotifyPropertyChanged viewModel)
        {
            if (previous != null)
                previous.PropertyChanged -= TeeTimeRequestsChanged;
            previous = viewModel;
            if (viewModel != null)
                viewModel.PropertyChanged += TeeTimeRequestsChanged;
        }

        // event handler
        private void TeeTimeRequestsChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName.Equals("TeeTimeRequestsUnassigned"))
            {
                DisplayTodoListboxItems();
            }
        }

        private void DisplayTodoListboxItems()
        {
            TODOListBox.Items.Clear();
            foreach (var ttr in ((WebAdmin.ViewModel.SignupTabViewModel)DataContext).TeeTimeRequestsUnassigned)
            {
                var rtb = new RichTextBox();
                rtb.Height = 20;
                rtb.Width = 800; // TODOListBox.ActualWidth;
                rtb.BorderThickness = new Thickness(0);
                rtb.IsReadOnly = true;
                rtb.Focusable = false;
                AppendText(rtb, ttr.RequestedTimeWithoutPlayerList, Brushes.Black);

                for (int i = 0; i < ttr.Players.Count; i++)
                {
                    if (i > 0)
                    {
                        AppendText(rtb, " --- ", Brushes.Black);
                    }

                    if (ttr.Players[i].PreviouslyWaitlisted)
                    {
                        AppendText(rtb, ttr.Players[i].Name, Brushes.Blue);
                    }
                    else if ((ttr.Players[i].TeeTimeCount >= 0) && (ttr.Players[i].TeeTimeCount <= 2) && !ttr.Players[i].Extra.StartsWith("G"))
                    {
                        AppendText(rtb, ttr.Players[i].Name, Brushes.DarkRed);
                    }
                    else
                    {
                        AppendText(rtb, ttr.Players[i].Name, Brushes.Black);
                    }

                    if (!string.IsNullOrEmpty(ttr.Players[i].Extra))
                    {
                        AppendText(rtb, "(" + ttr.Players[i].Extra + ")", Brushes.Black);
                    }
                }

                TODOListBox.Items.Add(rtb);
            }
        }

        public void AppendText(RichTextBox box, string text, SolidColorBrush color)
        {
            TextRange rangeOfText1 = new TextRange(box.Document.ContentEnd, box.Document.ContentEnd);
            rangeOfText1.Text = text;
            rangeOfText1.ApplyPropertyValue(TextElement.ForegroundProperty, color);
        }

        private void OrderByRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (DataContext == null) return;

            if ((HistoricalRadioButton != null) && (HistoricalRadioButton.IsChecked != null) && (HistoricalRadioButton.IsChecked.Value == true))
            {
                ((WebAdmin.ViewModel.SignupTabViewModel)DataContext).OrderTeeTimeRequestsBy =
                    ViewModel.SignupTabViewModel.OrderTeeTimeRequestsByEnum.HistoricalTeeTimes;
            }
            else if ((LastTeeTimeRadioButton != null) && (LastTeeTimeRadioButton.IsChecked != null) && (LastTeeTimeRadioButton.IsChecked.Value == true))
            {
                ((WebAdmin.ViewModel.SignupTabViewModel)DataContext).OrderTeeTimeRequestsBy =
                    ViewModel.SignupTabViewModel.OrderTeeTimeRequestsByEnum.LastTeeTime;
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

        private void BrowsePrevWaitlistButton_Click(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog 
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            if (!string.IsNullOrEmpty(PrevWaitlistTextBox.Text))
            {
                string dir = System.IO.Path.GetDirectoryName(PrevWaitlistTextBox.Text);
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
                PrevWaitlistTextBox.Text = dlg.FileName;
                TabViewModelBase.Options.SignupWaitListFileName = dlg.FileName;
            }
        }

        private void ClearPrevWaitlistButton_Click(object sender, RoutedEventArgs e)
        {
            PrevWaitlistTextBox.Text = string.Empty;
            TabViewModelBase.Options.SignupWaitListFileName = string.Empty;
        }
    }
}
