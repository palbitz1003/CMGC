using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml.Serialization;
using System.IO;

namespace LocalHandicap
{
    public partial class LocalHandicapForm : Form
    {
        string _settingsFile;
        Settings _settings;
        bool _initializing;
        string _dbFolder;
        LocalHandicap _localHandicap = new LocalHandicap();
        GHINIndex _GHINIndex = new GHINIndex();
        System.Windows.Forms.Timer _timer = new Timer();
        ResultsUserControl _resultsUserControl;
        string _lastAddedVPTFile;

        [Serializable]
        public class Settings
        {
            private string _SCGAFile;
            private int _monthsOfLocalScores = 12;
            private int _maxLocalScores = 10;
            private string _lastVPTFileFolder;
            private string _lastSavedGHINFileFolder;
            private string _SCGAClubCode;
            private string _courseName;
            private string _courseSlope;
            private bool _upperCaseNames;
            private string _tee1;
            private string _tee2;
            private string _tee3;
            private string _courseRating1;
            private string _courseRating2;
            private string _courseRating3;

            public string SCGAFile
            {
                get { return _SCGAFile; }
                set { _SCGAFile = value; }
            }

            public string SCGAClubCode
            {
                get { return _SCGAClubCode; }
                set { _SCGAClubCode = value; }
            }

            public int MonthsOfLocalScores
            {
                get { return _monthsOfLocalScores; }
                set { _monthsOfLocalScores = value; }
            }

            public int MaxLocalScores
            {
                get { return _maxLocalScores; }
                set { _maxLocalScores = value; }
            }

            public string LastVPTFileFolder
            {
                get { return _lastVPTFileFolder; }
                set { _lastVPTFileFolder = value; }
            }

            public string LastSavedGHINFileFolder
            {
                get { return _lastSavedGHINFileFolder; }
                set { _lastSavedGHINFileFolder = value; }
            }

            public string CourseName
            {
                get { return _courseName; }
                set { _courseName = value; }
            }

            public string CourseSlope
            {
                get { return _courseSlope; }
                set { _courseSlope = value; }
            }

            public bool UpperCaseNames
            {
                get { return _upperCaseNames; }
                set { _upperCaseNames = value; }
            }

            public string Tee1
            {
                get { return _tee1; }
                set { _tee1 = value; }
            }

            public string Tee2
            {
                get { return _tee2; }
                set { _tee2 = value; }
            }

            public string Tee3
            {
                get { return _tee3; }
                set { _tee3 = value; }
            }

            public string CourseRating1
            {
                get { return _courseRating1; }
                set { _courseRating1 = value; }
            }

            public string CourseRating2
            {
                get { return _courseRating2; }
                set { _courseRating2 = value; }
            }

            public string CourseRating3
            {
                get { return _courseRating3; }
                set { _courseRating3 = value; }
            }
        }

        public LocalHandicapForm()
        {
            InitializeComponent();

            _initializing = true;
            _dbFolder = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "db");
            IncludeExclude.ExcludedFileFolder(_dbFolder);
            _settingsFile = Path.Combine(_dbFolder, "settings.xml");
            loadSettings();
            InitControls();
            UpdateDates();
            _initializing = false;

            // Poll for the files changing
            _timer.Interval = 1000;
            _timer.Tick += new EventHandler(UpdateIndexDate);
            _timer.Start();

            Application.ApplicationExit += new EventHandler(Application_ApplicationExit);
        }

        void Application_ApplicationExit(object sender, EventArgs e)
        {
            if (_resultsUserControl != null)
            {
                _settings.UpperCaseNames = _resultsUserControl.UpperCaseNames;
                SaveSettings();
            }
        }

        private void loadSettings()
        {
            if (File.Exists(_settingsFile))
            {
                using (FileStream fs = new FileStream(_settingsFile, FileMode.Open))
                {
                    XmlSerializer xs = new XmlSerializer(typeof(Settings));
                    try
                    {
                        _settings = (Settings)xs.Deserialize(fs);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Failed to read settings from " + _settingsFile + ": " + ex.Message);
                    }
                }
            }

            if (_settings == null)
            {
                _settings = new Settings();
            }
        }

        private void SaveSettings()
        {
            createDBFolder();

            if (!Directory.Exists(_dbFolder)) return;

            using (FileStream fs = new FileStream(_settingsFile, FileMode.Create))
            {
                XmlSerializer xs = new XmlSerializer(typeof(Settings));
                try
                {
                    xs.Serialize(fs, _settings);
                }
                catch(Exception ex)
                {
                    MessageBox.Show("Failed to save settings to " + _settingsFile + ": " + ex.Message);
                }
            }
        }

        private void InitControls()
        {
            _initializing = true;

            ActualInputPathLabel.Text = _settings.SCGAFile;

            if ((_settings.MonthsOfLocalScores < MonthsOfLocalScoresNumericUpDown.Minimum) ||
                (_settings.MonthsOfLocalScores > MonthsOfLocalScoresNumericUpDown.Maximum))
            {
                _settings.MonthsOfLocalScores = 12;
                SaveSettings();
            }
            if ((_settings.MaxLocalScores < NumberOfTournamentsNumericUpDown.Minimum) ||
                (_settings.MaxLocalScores > NumberOfTournamentsNumericUpDown.Maximum))
            {
                _settings.MaxLocalScores = 10;
                SaveSettings();
            }
            MonthsOfLocalScoresNumericUpDown.Value = _settings.MonthsOfLocalScores;
            NumberOfTournamentsNumericUpDown.Value = _settings.MaxLocalScores;
            SCGAClubCodeTextBox.Text = _settings.SCGAClubCode;
            CourseSlopeTextBox.Text = _settings.CourseSlope;
            CourseNameTextBox.Text = _settings.CourseName;
            TeeTextBox1.Text = _settings.Tee1;
            TeeTextBox2.Text = _settings.Tee2;
            TeeTextBox3.Text = _settings.Tee3;
            CourseRatingTextBox1.Text = _settings.CourseRating1;
            CourseRatingTextBox2.Text = _settings.CourseRating2;
            CourseRatingTextBox3.Text = _settings.CourseRating3;
            _initializing = false;
        }

        private void UpdateDates()
        {
            DeleteTournaments dt = new DeleteTournaments(_dbFolder);
            LocalHandicapDateLabel.Text = "Last tournament added: " + dt.LastTournamentDate;
            dt = null;

            UpdateIndexDate(null, null);
        }

        private void UpdateIndexDate(object sender, EventArgs e)
        {

            if (_settings.SCGAFile != null)
            {
                SCGAIndexDateLabel.Text = "Last SCGA Index update: ";
                FileInfo fi = new FileInfo(_settings.SCGAFile);
                SCGAIndexDateLabel.Text += fi.LastWriteTime.ToShortDateString();
            }
            else
            {
                SCGAIndexDateLabel.Text = string.Empty;
            }

        }

        private void InputPathBrowseButton_Click(object sender, EventArgs e)
        {
            DialogResult result;
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.CheckFileExists = true;

                if (_settings.SCGAFile == null)
                {
                    dlg.InitialDirectory = Path.GetDirectoryName(Application.ExecutablePath);
                }
                else
                {
                    dlg.InitialDirectory = Path.GetDirectoryName(_settings.SCGAFile);
                }
                dlg.DefaultExt = ".txt"; // Default file extension
                dlg.Filter = "Excel, csv, or text|*.xls;*.csv;*.txt"; // Filter files by extension

                // Show open file dialog box
                result = dlg.ShowDialog();

                // Process open file dialog box results
                if (result == DialogResult.OK)
                {
                    _settings.SCGAFile = dlg.FileName;
                    SaveSettings();
                    InitControls();
                }

            }
            UpdateDates();
        }

        private void SCGAInputTypeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_initializing) return;

            SaveSettings();
            InitControls();
            UpdateDates();
        }

        private void MonthsOfLocalScoresNumericUpDown_ValueChanged(object sender, EventArgs e)
        {
            _settings.MonthsOfLocalScores = (int)MonthsOfLocalScoresNumericUpDown.Value;
            SaveSettings();
        }

        private void CalculateLocalHandicap()
        {
            createDBFolder();

            if (!Directory.Exists(_dbFolder)) return;

            DirectoryInfo di = new DirectoryInfo(_dbFolder);
            FileInfo[] fiList = di.GetFiles("*.vpt");

            if (fiList.Length == 0)
            {
                MessageBox.Show("You need to add local tournaments before a handicap can be calculated.");
                return;
            }
            else
            {
                _GHINIndex.reset();

                if (!_GHINIndex.CSVFile(_settings.SCGAFile))
                {
                    return;
                }

                DateTime oldestDate = DateTime.Now.AddMonths(-(int)MonthsOfLocalScoresNumericUpDown.Value);
                _localHandicap.reset();
                foreach (FileInfo fi in fiList)
                {
                    _localHandicap.addScores(fi, oldestDate);
                }
                _localHandicap.Calculate(_settings.MaxLocalScores);

                foreach (KeyValuePair<string, GHINIndex.Index> GHINEntry in _GHINIndex.DBByName)
                {
                    float? localHandicap = _localHandicap.GetLocalHandicap(GHINEntry.Value.GHINNumber);

                    float? handicap = GHINEntry.Value.CurrentIndex;

                    if(handicap == null)
                    {
                        handicap = localHandicap;
                    }
                    else if((localHandicap != null) && (localHandicap < handicap))
                    {
                        handicap = localHandicap;
                    }

                    if (handicap != null)
                    {
                        if (_localHandicap.LocalHandicapDBByNumber.ContainsKey(GHINEntry.Value.GHINNumber))
                        {
                            PlayerData playerData =
                                _localHandicap.LocalHandicapDBByNumber[GHINEntry.Value.GHINNumber];
                            playerData.Name = GHINEntry.Value.Name;
                            playerData.GHINIndex = GHINEntry.Value.CurrentIndex;
                            playerData.Handicap = handicap;

                            _localHandicap.LocalHandicapDBByName.Add(GHINEntry.Value.Name, playerData);
                        }
                        else
                        {
                            PlayerData playerData = new PlayerData(GHINEntry.Value.GHINNumber);
                            playerData.Name = GHINEntry.Value.Name;
                            playerData.GHINIndex = GHINEntry.Value.CurrentIndex;
                            playerData.Handicap = handicap;
                            _localHandicap.LocalHandicapDBByName.Add(GHINEntry.Value.Name, playerData);
                            _localHandicap.LocalHandicapDBByNumber.Add(GHINEntry.Value.GHINNumber, playerData);
                        }
                    }
                }

                if (_resultsUserControl != null)
                {
                    this.Controls.Remove(_resultsUserControl);
                    _resultsUserControl.Dispose();
                }

                List<ResultsUserControl.TeeAndRating> tarList = new List<ResultsUserControl.TeeAndRating>();
                if (!string.IsNullOrEmpty(_settings.Tee1) && !string.IsNullOrEmpty(_settings.CourseRating1))
                    tarList.Add(new ResultsUserControl.TeeAndRating(_settings.Tee1, float.Parse(_settings.CourseRating1)));
                if (!string.IsNullOrEmpty(_settings.Tee2) && !string.IsNullOrEmpty(_settings.CourseRating2))
                    tarList.Add(new ResultsUserControl.TeeAndRating(_settings.Tee2, float.Parse(_settings.CourseRating2)));
                if (!string.IsNullOrEmpty(_settings.Tee3) && !string.IsNullOrEmpty(_settings.CourseRating3))
                    tarList.Add(new ResultsUserControl.TeeAndRating(_settings.Tee3, float.Parse(_settings.CourseRating3)));

                _resultsUserControl = new
                    ResultsUserControl(_localHandicap.DateList,
                        _localHandicap.LocalHandicapDBByNumber,
                        _localHandicap.LocalHandicapDBByName,
                        _settings.CourseSlope,
                        _settings.CourseName,
                        tarList,
                        _settings.UpperCaseNames);
                _resultsUserControl.Dock = DockStyle.Fill;
                this.Controls.Add(_resultsUserControl);
                _resultsUserControl.BringToFront();
            }
        }

        private void createDBFolder()
        {
            if (!Directory.Exists(_dbFolder))
            {
                try
                {
                    Directory.CreateDirectory(_dbFolder);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to create database folder " + _dbFolder + ": " + ex.Message);
                }
            }
        }

        private void CalculateHandicapButton_Click(object sender, EventArgs e)
        {
            CalculateLocalHandicap();
        }

        private void AddTournamentButton_Click(object sender, EventArgs e)
        {
            createDBFolder();

            if (!Directory.Exists(_dbFolder)) return;

            DialogResult result;
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                if (_settings.LastVPTFileFolder == null)
                {
                    dlg.InitialDirectory = Path.GetDirectoryName(Application.ExecutablePath);
                }
                else
                {
                    dlg.InitialDirectory = _settings.LastVPTFileFolder;
                }
                dlg.DefaultExt = ".vpt"; // Default file extension
                dlg.Filter = "Vision Perfect file (.vpt)|*.vpt"; // Filter files by extension
                dlg.Multiselect = true;

                // Show open file dialog box
                result = dlg.ShowDialog();

                // Process open file dialog box results
                if (result == DialogResult.OK)
                {
                    bool updated = false;
                    bool added = false;
                    foreach (string fileName in dlg.FileNames)
                    {
                        string target = Path.Combine(_dbFolder, Path.GetFileName(fileName));
                        try
                        {
                            if (File.Exists(target))
                            {
                                _lastAddedVPTFile = fileName;

                                FileInfo fi1 = new FileInfo(target);
                                FileInfo fi2 = new FileInfo(fileName);
                                if ((fi1.Length == fi2.Length) && (fi1.LastWriteTime == fi2.LastWriteTime))
                                {
                                    MessageBox.Show(Path.GetFileName(fileName) + " is already in the database.");
                                }
                                else
                                {
                                    if (MessageBox.Show(Path.GetFileName(fileName) + " is already in the database.  Update scores?", "File Exists", MessageBoxButtons.YesNo) == DialogResult.Yes)
                                    {
                                        File.Copy(fileName, target, true);
                                        updated = true;
                                    }
                                }
                            }
                            else
                            {
                                added = true;
                                File.Copy(fileName, target, true);
                            }
                            
                            _settings.LastVPTFileFolder = Path.GetDirectoryName(fileName);
                            SaveSettings();

                            TournamentDataForm tdf = new TournamentDataForm();
                            TournamentData td = new TournamentData();
                            string tdFile = Path.ChangeExtension(target, ".data");
                            td.LoadTournamentData(tdFile);
                            tdf.TournamentNameTextBox.Text = td.Name;
                            tdf.TournamentFileValueLabel.Text = Path.GetFileName(fileName);
                            tdf.ShowDialog();

                            td.Name = tdf.TournamentNameTextBox.Text;
                            tdf.Dispose();

                            td.SaveTournamentData(tdFile);

                            if (updated)
                            {
                                MessageBox.Show(Path.GetFileName(fileName) + " updated");
                            }
                            else if(added)
                            {
                                MessageBox.Show(Path.GetFileName(fileName) + " added");
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Failed to copy " + fileName + " to " + target + ": " + ex.Message);
                        }
                    }
                }
            }

            UpdateDates();
        }

        private void DeleteTournamentButton_Click(object sender, EventArgs e)
        {
            DeleteTournaments dt = new DeleteTournaments(_dbFolder);
            dt.ShowDialog();
            UpdateDates();
        }

        private void SCGAClubCodeTextBox_TextChanged(object sender, EventArgs e)
        {
            if (_initializing)
            {
                return;
            }

            _settings.SCGAClubCode = SCGAClubCodeTextBox.Text;
            SaveSettings();
        }

        private void CourseNameTextBox_TextChanged(object sender, EventArgs e)
        {
            if (_initializing)
            {
                return;
            }

            _settings.CourseName = CourseNameTextBox.Text;
            SaveSettings();
        }

        private void CourseSlopeTextBox_TextChanged(object sender, EventArgs e)
        {
            if (_initializing)
            {
                return;
            }

            _settings.CourseSlope = CourseSlopeTextBox.Text;
            SaveSettings();
        }

        private void TeeTextBox1_TextChanged(object sender, EventArgs e)
        {
            if (_initializing)
            {
                return;
            }

            _settings.Tee1 = TeeTextBox1.Text;
            SaveSettings();
        }

        private void CourseRatingTextBox1_TextChanged(object sender, EventArgs e)
        {
            if (_initializing)
            {
                return;
            }

            _settings.CourseRating1 = CourseRatingTextBox1.Text;
            SaveSettings();
        }

        private void TeeTextBox2_TextChanged(object sender, EventArgs e)
        {
            if (_initializing)
            {
                return;
            }

            _settings.Tee2 = TeeTextBox2.Text;
            SaveSettings();
        }

        private void CourseRatingTextBox2_TextChanged(object sender, EventArgs e)
        {
            if (_initializing)
            {
                return;
            }

            _settings.CourseRating2 = CourseRatingTextBox2.Text;
            SaveSettings();
        }

        private void TeeTextBox3_TextChanged(object sender, EventArgs e)
        {
            if (_initializing)
            {
                return;
            }

            _settings.Tee3 = TeeTextBox3.Text;
            SaveSettings();
        }

        private void CourseRatingTextBox3_TextChanged(object sender, EventArgs e)
        {
            if (_initializing)
            {
                return;
            }

            _settings.CourseRating3 = CourseRatingTextBox3.Text;
            SaveSettings();
        }

        private void CourseSlopeTextBox_Validating(object sender, CancelEventArgs e)
        {
            e.Cancel = false;

            if (_initializing)
            {
                return;
            }

            if (CourseSlopeTextBox.Text.Trim().Length == 0)
            {
                return;
            }
            try
            {
                float.Parse(CourseSlopeTextBox.Text);
            }
            catch
            {
                MessageBox.Show("Invalid course slope");
                CourseSlopeTextBox.Text = "";
                e.Cancel = true;
            }
        }

        private void CourseRatingTextBox1_Validating(object sender, CancelEventArgs e)
        {
            e.Cancel = false;

            if (_initializing)
            {
                return;
            }

            if (CourseRatingTextBox1.Text.Trim().Length == 0)
            {
                return;
            }
            try
            {
                float.Parse(CourseRatingTextBox1.Text);
            }
            catch
            {
                MessageBox.Show("Invalid course rating: " + CourseRatingTextBox1.Text);
                CourseRatingTextBox1.Text = "";
                e.Cancel = true;
            }
        }

        private void CourseRatingTextBox2_Validating(object sender, CancelEventArgs e)
        {
            e.Cancel = false;

            if (_initializing)
            {
                return;
            }

            if (CourseRatingTextBox2.Text.Trim().Length == 0)
            {
                return;
            }
            try
            {
                float.Parse(CourseRatingTextBox2.Text);
            }
            catch
            {
                MessageBox.Show("Invalid course rating: " + CourseRatingTextBox2.Text);
                CourseRatingTextBox2.Text = "";
                e.Cancel = true;
            }
        }

        private void CourseRatingTextBox3_Validating(object sender, CancelEventArgs e)
        {
            e.Cancel = false;

            if (_initializing)
            {
                return;
            }

            if (CourseRatingTextBox3.Text.Trim().Length == 0)
            {
                return;
            }
            try
            {
                float.Parse(CourseRatingTextBox3.Text);
            }
            catch
            {
                MessageBox.Show("Invalid course rating: " + CourseRatingTextBox3.Text);
                CourseRatingTextBox3.Text = "";
                e.Cancel = true;
            }
        }

        private void NumberOfTournamentsNumericUpDown_ValueChanged(object sender, EventArgs e)
        {
            _settings.MaxLocalScores = (int)NumberOfTournamentsNumericUpDown.Value;
            SaveSettings();
        }

        private void LocalHandicapForm_Resize(object sender, EventArgs e)
        {
            if ((ActualInputPathLabel != null) && (ActualInputPathLabel.Parent != null))
            {
                ActualInputPathLabel.MaximumSize = 
                    new Size(ActualInputPathLabel.Parent.Width - ActualInputPathLabel.Left - 5, 
                    ActualInputPathLabel.Height);
            }

            if ((CalculateHandicapButton != null) && CalculateHandicapButton.Visible)
            {
                CalculateHandicapButton.Left = (this.Width - CalculateHandicapButton.Width) / 2;
            }
        }

        private void ConvertForGHINButton_Click(object sender, EventArgs e)
        {
            DialogResult result;
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                if (_lastAddedVPTFile != null)
                {
                    dlg.InitialDirectory = Path.GetDirectoryName(_lastAddedVPTFile);
                    dlg.FileName = Path.GetFileName(_lastAddedVPTFile);
                }
                else if (_settings.LastVPTFileFolder == null)
                {
                    dlg.InitialDirectory = Path.GetDirectoryName(Application.ExecutablePath);
                }
                else
                {
                    dlg.InitialDirectory = _settings.LastVPTFileFolder;
                }
                dlg.DefaultExt = ".vpt"; // Default file extension
                dlg.Filter = "Vision Perfect file (.vpt)|*.vpt"; // Filter files by extension
                dlg.Multiselect = false;

                // Show open file dialog box
                result = dlg.ShowDialog();

                // Process open file dialog box results
                if (result == DialogResult.OK)
                {

                    using (SaveFileDialog sfdlg = new SaveFileDialog())
                    {
                        if (_settings.LastSavedGHINFileFolder == null)
                        {
                            sfdlg.InitialDirectory = Path.GetDirectoryName(Application.ExecutablePath);
                        }
                        else
                        {
                            sfdlg.InitialDirectory = _settings.LastSavedGHINFileFolder;
                        }
                        string saveFile = Path.GetFileNameWithoutExtension(Path.GetFileName(dlg.FileName));
                        saveFile += "_ForGHIN.csv";

                        sfdlg.FileName = saveFile;

                        result = sfdlg.ShowDialog();

                        if (result == DialogResult.OK)
                        {
                            _settings.LastSavedGHINFileFolder = Path.GetDirectoryName(sfdlg.FileName);
                            SaveSettings();

                            FileInfo fi = new FileInfo(dlg.FileName);
                            _localHandicap.convertToGHIN(fi, sfdlg.FileName);
                        }
                    }
                }
            }
        }

        
    }
}