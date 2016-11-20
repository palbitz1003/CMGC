using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
using System.Windows.Threading;
using System.ComponentModel;
using System.Collections.ObjectModel;
using WebAdmin.ViewModel;

namespace WebAdmin
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window,  INotifyPropertyChanged
    {
        private const string OptionsFileName = "options.xml";
        public ObservableCollection<ITabViewModel> TabViewModels { get; set; }
        public ITabViewModel SelectedTabViewModel { get; set; }

        private StatusMsg _statusMsg;
        public StatusMsg StatusMsg { get { return _statusMsg; } set { _statusMsg = value; OnPropertyChanged(); } }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Dispatcher.CurrentDispatcher.UnhandledException += CurrentDispatcher_UnhandledException;

            Logging.Init();
            TabViewModelBase.Options = Options.Load(OptionsFileName);

            StatusMsg = new WebAdmin.StatusMsg();
            TabViewModelBase.Status = StatusMsg;
            StatusTextBlock.DataContext = this;

            WebAddresses.BaseAddress = "https://" +  TabViewModelBase.Options.WebSite;
            WebAddresses.ScriptFolder = string.IsNullOrEmpty(TabViewModelBase.Options.ScriptFolder)
                ? string.Empty
                : "/" + TabViewModelBase.Options.ScriptFolder;

            TabViewModels = new ObservableCollection<ITabViewModel>() { 
                new SignupTabViewModel(), 
                new PaymentsTabViewModel(),
                new ResultsTabViewModel(),
                new TournamentTabViewModel(),
                new TournamentDescriptionTabViewModel(),
                new AdminTabViewModel()
            };
            MainTabControl.DataContext = this;
            MainTabControl.SelectedIndex = 0;

            if (System.Diagnostics.Debugger.IsAttached)
            {
                if (TabViewModelBase.Options.WebSite.Contains("coronado"))
                {
                    MessageBox.Show("You are using the coronado web site ...");
                }
            }
            else if (TabViewModelBase.Options.WebSite.Contains("paulalbitz"))
            {
                MessageBox.Show("You are using the paulalbitz web site ...");
            }

            if(!string.IsNullOrEmpty(TabViewModelBase.Options.GHINFileName))
            {
                try
                {
                    TabViewModelBase.GHINEntries = GHINEntry.LoadGHIN(TabViewModelBase.Options.GHINFileName);
                }
                catch(FileNotFoundException)
                {
                    MessageBox.Show("GHIN file does not exist: " + TabViewModelBase.Options.GHINFileName + ". Auto-completion will not work");
                }
                catch(ArgumentException ex)
                {
                    MessageBox.Show("Failed to load roster file " + TabViewModelBase.Options.GHINFileName + ": " +
                                    ex.Message + ". Please go to the Admin tab and specify a valid roster file name. Without the roster file, auto-completion will not work.");
                }
                catch
                {
                    MessageBox.Show("Please go to the Admin tab and specify the roster file name. Without the roster file, auto-completion will not work.");
                }
            }
        }


        void CurrentDispatcher_UnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            Exception baseException = e.Exception.GetBaseException();

            string msg = e.Exception.Message + ((e.Exception != baseException) ? " caused by: " + baseException.Message : string.Empty);

            // The base exception message for this error is useless
            if ((e.Exception is System.Net.Http.HttpRequestException) && (e.Exception.InnerException != null))
            {
                msg = e.Exception.Message + " caused by: " + e.Exception.InnerException.Message;
            }

            Logging.Log("Exception: ", msg);
            Logging.Log("Stack Trace:", e.Exception.StackTrace);

            MessageBox.Show("Error: " + msg, "Error");
        }

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = (Exception)e.ExceptionObject;

            Exception baseException = exception.GetBaseException();

            string msg = exception.Message + ((exception != baseException) ? " caused by: " + baseException.Message : string.Empty);

            // The base exception message for this error is useless
            if ((exception is System.Net.Http.HttpRequestException) && (exception.InnerException != null))
            {
                msg = exception.Message + " caused by: " + exception.InnerException.Message;
            }

            Logging.Log("Exception: ", msg);
            Logging.Log("Stack Trace:", exception.StackTrace);

            MessageBox.Show("Error: " + msg, "Error");
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            TabViewModelBase.Options.Save(OptionsFileName);
        }
    }
}
