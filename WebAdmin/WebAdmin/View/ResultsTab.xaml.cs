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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using WebAdmin.ViewModel;

namespace WebAdmin.View
{
    /// <summary>
    /// Interaction logic for ResultsTab.xaml
    /// </summary>
    public partial class ResultsTab : UserControl
    {
        public ResultsTab()
        {
            InitializeComponent();
        }

        private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender != null)
            {
                DataGrid grid = sender as DataGrid;
                if (grid != null && grid.SelectedItems != null && grid.SelectedItems.Count == 1)
                {
                    List<GHINEntry> ghinList = new List<GHINEntry>();
                    try
                    {
                        ghinList = GHINEntry.LoadGHIN(TabViewModelBase.Options.GHINFileName);
                    }
                    catch
                    {
                        // no error if file doesn't exist
                    }

                    ClosestToThePin ctp = (ClosestToThePin)grid.SelectedItems[0];

                    ctp.Businesses = new List<string>();
                    if (File.Exists("closest to the pin.txt"))
                    {
                        using (TextReader tr = new StreamReader("closest to the pin.txt"))
                        {
                            string line;
                            while ((line = tr.ReadLine()) != null)
                            {
                                ctp.Businesses.Add(line.Trim());
                            }
                        }
                    }

                    ClosestToThePinWindow ctpw = new ClosestToThePinWindow();
                    ClosestToThePin ctpClone = (ClosestToThePin)ctp.Clone();
                    ctpw.DataContext = ctpClone;
                    ctpw.GHINList = ghinList;
                    ctpw.Owner = App.Current.MainWindow;
                    
                    // Default to $25
                    if (string.IsNullOrEmpty(ctpClone.Prize)) { ctpClone.Prize = "$25"; }

                    ctpw.ShowDialog();
                    if(ctpw.DialogResult.HasValue && ctpw.DialogResult.Value)
                    {
                        ctp.Player = ctpClone.Player;
                        ctp.Distance = ctpClone.Distance;
                        ctp.Prize = ctpClone.Prize;
                        ctp.Business = ctpClone.Business;
                        ctp.GHIN = ctpw.GHIN;
                    }
                }
            }
        }

        private class PoolFile
        {
            public string FileName { get; set; }
            public int Flight { get; set; }
            public string Day { get; set; }
            public int DayInteger { get; set; }
        }

        private void ResetTextBoxes()
        {
            CSVFolderTextBox.Text = string.Empty;
            CSVScoresTextBox.Text = string.Empty;
            CSVChitsTextBox.Text = string.Empty;

            Day1Flight1PoolTextBox.Text = string.Empty;
            Day1Flight2PoolTextBox.Text = string.Empty;
            Day1Flight3PoolTextBox.Text = string.Empty;
            Day1Flight4PoolTextBox.Text = string.Empty;

            Day2Flight1PoolTextBox.Text = string.Empty;
            Day2Flight2PoolTextBox.Text = string.Empty;
            Day2Flight3PoolTextBox.Text = string.Empty;
            Day2Flight4PoolTextBox.Text = string.Empty;
        }

        private void BrowseCSVFolderButton_Click(object sender, RoutedEventArgs e)
        {
            ResetTextBoxes();

            var fbd = new System.Windows.Forms.FolderBrowserDialog();
            //fbd.RootFolder = Environment.SpecialFolder.MyComputer;
            if(!string.IsNullOrEmpty(TabViewModelBase.Options.LastCSVResultFolder) &&
                Directory.Exists(TabViewModelBase.Options.LastCSVResultFolder))
            {
                fbd.SelectedPath = TabViewModelBase.Options.LastCSVResultFolder;
            }
            System.Windows.Forms.DialogResult result = fbd.ShowDialog();

            if (result != System.Windows.Forms.DialogResult.OK)
            {
                return;
            }

            string[] files = Directory.GetFiles(fbd.SelectedPath, "*.csv");
            if(files.Length == 0)
            {
                MessageBox.Show("No csv files found");
                return;
            }

            CSVFolderTextBox.Text = fbd.SelectedPath;
            TabViewModelBase.Options.LastCSVResultFolder = fbd.SelectedPath;

            List<PoolFile> poolFiles = new List<PoolFile>();
            int minDay = 100;

            foreach (string file in files)
            {
                string[] dayList1 = new string[] { 
                    "monday",  
                    "tuesday", 
                    "wednesday", 
                    "thursday", 
                    "friday", 
                    "saturday", 
                    "sunday", };
                string[] dayList2 = new string[] { 
                    "mon", 
                    "tues", 
                    "wed",
                    "thurs",
                    "fri",
                    "sat",
                    "sun"};
                string[] dayList3 = new string[] { 
                    "mon.", 
                    "tues.", 
                    "wed.",
                    "thurs.",
                    "fri.",
                    "sat.",
                    "sun."};

                string fileName = System.IO.Path.GetFileName(file).ToLower();

                if (fileName.Contains("scores") || fileName.Contains(" scoresexportn"))
                {
                    CSVScoresTextBox.Text = fileName;
                }
                else if (fileName.Contains("payout") || fileName.Contains("payoff"))
                {
                    CSVChitsTextBox.Text = fileName;
                }
                else if (fileName.Contains("pool"))
                {
                    PoolFile pf = new PoolFile() { FileName = fileName };
                    string[] fields = System.IO.Path.GetFileNameWithoutExtension(fileName).Split(' ');
                    pf.DayInteger = 100;

                    for (int i = 0; i < fields.Length; i++)
                    {
                        if (fields[i].ToLower() == "flight")
                        {
                            int flight;
                            if (((i + 1) < fields.Length) && int.TryParse(fields[i + 1], out flight))
                            {
                                pf.Flight = flight;
                            }
                            else
                            {
                                MessageBox.Show("File name contains 'flight' but it does not have a flight number: " + fileName + " (assuming flight 1)");
                                pf.Flight = 1;
                            }
                            i++;
                        }
                        else if (pf.DayInteger == 100)
                        {
                            for (int d = 0; d < dayList1.Length; d++)
                            {
                                if ((fields[i] == dayList1[d]) || (fields[i] == dayList2[d]) || (fields[i] == dayList3[d]))
                                {
                                    pf.Day = fields[i];
                                    pf.DayInteger = d;
                                    minDay = Math.Min(pf.DayInteger, minDay);
                                    break;
                                }
                            }
                        }
                    }

                    if (pf.DayInteger == 100)
                    {
                        MessageBox.Show("Unable to determine tournament day from file name: " + fileName + " (Ignoring file)");
                    }
                    else
                    {
                        poolFiles.Add(pf);
                    }
                }
            }

            foreach (var pf in poolFiles)
            {
                if (pf.DayInteger == minDay)
                {
                    switch (pf.Flight)
                    {
                        case 1:
                            Day1Flight1PoolTextBox.Text = pf.FileName;
                            break;
                        case 2:
                            Day1Flight2PoolTextBox.Text = pf.FileName;
                            break;
                        case 3:
                            Day1Flight3PoolTextBox.Text = pf.FileName;
                            break;
                        case 4:
                            Day1Flight4PoolTextBox.Text = pf.FileName;
                            break;
                        default:
                            throw new ApplicationException("Bad file name: " + pf.FileName + " (flight must be 1-4)");
                    }
                }
                else
                {
                    switch (pf.Flight)
                    {
                        case 1:
                            Day2Flight1PoolTextBox.Text = pf.FileName;
                            break;
                        case 2:
                            Day2Flight2PoolTextBox.Text = pf.FileName;
                            break;
                        case 3:
                            Day2Flight3PoolTextBox.Text = pf.FileName;
                            break;
                        case 4:
                            Day2Flight4PoolTextBox.Text = pf.FileName;
                            break;
                        default:
                            throw new ApplicationException("Bad file name: " + pf.FileName + " (flight must be 1-4)");
                    }
                }
            }

            // Create OpenFileDialog 
            //Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            //if (!string.IsNullOrEmpty(HTMLPoolTextBox.Text))
            //{
            //    dlg.InitialDirectory = System.IO.Path.GetDirectoryName(HTMLPoolTextBox.Text);
            //}

            //// Set filter for file extension and default file extension 
            ////dlg.DefaultExt = ".htm|.html";
            //dlg.Filter = "HTML Files (*.htm, *.html)|*.htm;*.html";

            //// Display OpenFileDialog by calling ShowDialog method 
            //Nullable<bool> result = dlg.ShowDialog();

            //// Get the selected file name and display in a TextBox 
            //if (result == true)
            //{
            //    HTMLPoolTextBox.Text = dlg.FileName;
            //}
        }
    }
}
