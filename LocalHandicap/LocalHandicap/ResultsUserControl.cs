using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace LocalHandicap
{
    public partial class ResultsUserControl : UserControl
    {
        public class TeeAndRating
        {
            public string Tee;
            public float Rating;

            public TeeAndRating(string tee, float rating)
            {
                Tee = tee;
                Rating = rating;
            }
        }

        //public event ShowDetailsHandler ShowDetails;
        List<DateTime> _dateList = null;
        SortedDictionary<string, PlayerData> _localHandicapDBByNumber;
        SortedDictionary<string, PlayerData> _localHandicapDBByName;
        int _longestName;
        string _courseSlope;
        string _courseName;
        List<TeeAndRating> _teeAndRatings;
        List<DetailsRecord> _details = new List<DetailsRecord>();
        Dictionary<string, TournamentData> _tournamentData = new Dictionary<string, TournamentData>();

        private const int NameField = 0;
        private const int SCGANumberField = 1;
        private const int LowerHandicapField = 2;
        private const int SCGAIndexField = 3;
        private const int LocalHandicapField = 4;

        public ResultsUserControl(List<DateTime> dateList,
            SortedDictionary<string, PlayerData> localHandicapDBByNumber,
            SortedDictionary<string, PlayerData> localHandicapDBByName,
            string courseSlope,
            string courseName,
            List<TeeAndRating> teeAndRatings,
            bool upperCaseNames)
        {
            InitializeComponent();
            _dateList = dateList;
            _dateList.Sort();
            _localHandicapDBByNumber = localHandicapDBByNumber;
            _localHandicapDBByName = localHandicapDBByName;
            _courseSlope = courseSlope;
            _courseName = courseName;
            _teeAndRatings = teeAndRatings;
            UpperCaseCheckBox.Checked = upperCaseNames;

            AddSummaryColumns();
            ResetScores();

            int extra = Math.Max(0, this.Width - HandicapDetailsListView.PreferredSize.Width);

            if ((HandicapDetailsListView.PreferredSize.Width + extra) > this.Width)
            {
                this.Width = Math.Max(this.Width, HandicapDetailsListView.PreferredSize.Width) + extra + 30;
            }

            if (SystemInformation.PrimaryMonitorMaximizedWindowSize.Width < this.Width)
            {
                this.Width = SystemInformation.PrimaryMonitorMaximizedWindowSize.Width;
            }

            HandicapDetailsListView.DoubleClick += new EventHandler(HandicapDetailsListView_DoubleClick);
            ResultsListView.DoubleClick += new EventHandler(ResultsListView_DoubleClick);
        }

        public bool UpperCaseNames { get { return UpperCaseCheckBox.Checked; } }

        void ResultsListView_DoubleClick(object sender, EventArgs e)
        {
            HandicapTab.SelectedIndex = 1;
            IndividualDetailsScrollBar.Value = ResultsListView.SelectedIndices[0];
        }

        void HandicapDetailsListView_DoubleClick(object sender, EventArgs e)
        {
            HandicapTab.SelectedIndex = 1;
            IndividualDetailsScrollBar.Value = HandicapDetailsListView.SelectedIndices[0];
        }

        private void AddLowHandicaps()
        {
            _longestName = 0;
            ResultsListView.Items.Clear();
            foreach (KeyValuePair<string, PlayerData> entry in _localHandicapDBByName)
            {
                if (entry.Value.Excluded)
                {
                    continue;
                }

                if (LocalHandicapExistsCheckbox.Checked && (entry.Value.LocalHandicap == null))
                {
                    continue;
                }

                if (LocalHandicapLowerCheckBox.Checked &&
                    ((entry.Value.LocalHandicap == null) ||
                        (entry.Value.LocalHandicap >= entry.Value.GHINIndex)))
                {
                    continue;
                }

                ListViewItem lvi = new ListViewItem(entry.Value.Name);
                ListViewItem.ListViewSubItem lvsi = new ListViewItem.ListViewSubItem(lvi, entry.Value.GHINNumber);
                lvi.SubItems.Add(lvsi);
                lvsi = new ListViewItem.ListViewSubItem(lvi, entry.Value.Handicap.ToString().Replace("-", "+"));
                lvi.SubItems.Add(lvsi);
                lvsi = new ListViewItem.ListViewSubItem(lvi, entry.Value.GHINIndex.ToString().Replace("-", "+"));
                lvi.SubItems.Add(lvsi);
                lvsi = new ListViewItem.ListViewSubItem(lvi, entry.Value.LocalHandicap.ToString().Replace("-", "+"));
                lvi.SubItems.Add(lvsi);
                ResultsListView.Items.Add(lvi);

                if (entry.Value.Name.Length > _longestName)
                {
                    _longestName = entry.Value.Name.Length;
                }
            }
        }

        private void DoneButton_Click(object sender, EventArgs e)
        {
            this.Visible = false;
            //this.Close();
        }

        private void ResultsListView_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            System.Diagnostics.Debug.Write("column clicked");
        }

        private void AddSummaryColumns()
        {
            System.Windows.Forms.ColumnHeader NameColumn = new System.Windows.Forms.ColumnHeader();
            System.Windows.Forms.ColumnHeader SCGANumberColumn = new System.Windows.Forms.ColumnHeader();
            System.Windows.Forms.ColumnHeader LocalHandicapColumn = new System.Windows.Forms.ColumnHeader();

            // 
            // NameColumn
            // 
            NameColumn.Text = "Name";
            NameColumn.Width = 160;
            // 
            // SCGANumberColumn
            // 
            SCGANumberColumn.Text = "Number";
            SCGANumberColumn.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            SCGANumberColumn.Width = 70;
            // 
            // LocalHandicapColumn
            // 
            LocalHandicapColumn.Text = "LOC";
            LocalHandicapColumn.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            LocalHandicapColumn.Width = 40;

            this.HandicapDetailsListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            NameColumn,
            SCGANumberColumn,
            LocalHandicapColumn});

            foreach (DateTime dt in _dateList)
            {
                System.Windows.Forms.ColumnHeader dateColumn = new System.Windows.Forms.ColumnHeader();
                string year = dt.Year.ToString();
                year = year.Replace("20", "");
                dateColumn.Text = dt.Month.ToString() + "/" + dt.Day.ToString() + "/" + year;
                dateColumn.Width = 65;
                dateColumn.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
                HandicapDetailsListView.Columns.Add(dateColumn);
            }
        }

        private void AddSummaryScores()
        {
            HandicapDetailsListView.Items.Clear();
            foreach (KeyValuePair<string, PlayerData> entry in _localHandicapDBByName)
            {
                if (entry.Value.Excluded)
                {
                    continue;
                }

                if (LocalHandicapExistsCheckbox.Checked && (entry.Value.LocalHandicap == null))
                {
                    continue;
                }

                if (LocalHandicapLowerCheckBox.Checked &&
                    ((entry.Value.LocalHandicap == null) ||
                        (entry.Value.LocalHandicap >= entry.Value.GHINIndex)))
                {
                    continue;
                }

                ListViewItem lvi = new ListViewItem(entry.Key);
                lvi.UseItemStyleForSubItems = false;
                ListViewItem.ListViewSubItem lvsi = new ListViewItem.ListViewSubItem(lvi, entry.Value.GHINNumber);
                lvi.SubItems.Add(lvsi);
                lvsi = new ListViewItem.ListViewSubItem(lvi, entry.Value.LocalHandicap.ToString().Replace("-", "+"));
                lvi.SubItems.Add(lvsi);
                foreach (DateTime dt in _dateList)
                {
                    bool found = false;
                    foreach (Score score in entry.Value.Scores)
                    {
                        if (score.DT == dt)
                        {
                            lvsi = new ListViewItem.ListViewSubItem(lvi, score.Differential.ToString().Replace("-", "+") + " (" + score.ESScore + ")");
                            if (score.UsedInLocalHandicap)
                            {
                                lvsi.BackColor = Color.Yellow;
                            }
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        lvsi = new ListViewItem.ListViewSubItem(lvi, "");
                    }
                    lvi.SubItems.Add(lvsi);
                }
                HandicapDetailsListView.Items.Add(lvi);
            }
        }

        private void AddIndividualDetails()
        {
            int index = 0;
            foreach (KeyValuePair<string, PlayerData> entry in _localHandicapDBByName)
            {
                entry.Value.IndividualTabIndex = -1;

                if (entry.Value.Excluded)
                {
                    continue;
                }

                if (LocalHandicapExistsCheckbox.Checked && (entry.Value.LocalHandicap == null))
                {
                    continue;
                }

                if (LocalHandicapLowerCheckBox.Checked &&
                    ((entry.Value.LocalHandicap == null) ||
                        (entry.Value.LocalHandicap >= entry.Value.GHINIndex)))
                {
                    continue;
                }

                entry.Value.IndividualTabIndex = index++;
            }

            IndividualDetailsScrollBar.Minimum = 0;
            IndividualDetailsScrollBar.Maximum = index - 2 + IndividualDetailsScrollBar.LargeChange;
        }

        private void ShowIndividualDetails(int index)
        {
            PlayerData pd = null;
            PlayerData pdLast = null;
            foreach (KeyValuePair<string, PlayerData> entry in _localHandicapDBByName)
            {
                if (entry.Value.IndividualTabIndex == index)
                {
                    pd = entry.Value;
                    break;
                }
                // If the scrollbar number is too high, just show the last entry
                if (!entry.Value.Excluded)
                {
                    pdLast = entry.Value;
                }
            }
            if (pd == null)
            {
                pd = pdLast;
            }
            ShowIndividualDetails(pd);
        }

        private void ShowIndividualDetails(PlayerData pd)
        {
            if (pd == null)
            {
                return;
            }

            PlayerNameLabel.Text = pd.Name;
            SCGANumberLabel.Text = pd.GHINNumber;
            SCGAIndexLabel.Text = "SCGA Index: " + pd.GHINIndex.ToString().Replace("-", "+");
            LocalHandicapLabel.Text = "Local Handicap: " + pd.LocalHandicap.ToString().Replace("-", "+");

            DetailsRecord dr = null;
            int top = DateLabel.Top;
            for (int i = 0; i < pd.Scores.Count; i++)
            {
                Score score = pd.Scores[i];

                while ((i + 1) > _details.Count)
                {
                    dr = new DetailsRecord(top + 20, this);
                    dr.AddToControl(DetailsPanel);
                    _details.Add(dr);
                }

                TournamentData td = null;
                if (_tournamentData.ContainsKey(score.SourceFile))
                {
                    td = _tournamentData[score.SourceFile];
                }
                else
                {
                    td = new TournamentData();
                    td.LoadTournamentData(Path.ChangeExtension(score.SourceFile, ".data"));
                    if (td.Name == null)
                    {
                        td.Name = Path.GetFileNameWithoutExtension(score.SourceFile);
                    }
                    _tournamentData.Add(score.SourceFile, td);
                }

                dr = _details[i];
                top = dr.Date.Top;

                dr.Date.Text = score.TodaysDateString;
                dr.Date.Width = dr.Date.PreferredWidth;

                dr.Tournament.Text = td.Name;
                if ((dr.Tournament.Left + dr.Tournament.PreferredWidth + 5) > dr.Tees.Left)
                {
                    dr.Tournament.Width = dr.Tees.Left - dr.Tournament.Left - 5;
                }
                else
                {
                    dr.Tournament.Width = dr.Tournament.PreferredWidth;
                }

                // Save the name of the tee with the closest course rating.
                // Test for "closest" instead of "exact" since course ratings
                // can change over time.
                float maxDifference = float.MaxValue;
                foreach (TeeAndRating tar in _teeAndRatings)
                {
                    if (Math.Abs(score.CourseRating - tar.Rating) < maxDifference)
                    {
                        dr.Tees.Text = tar.Tee;
                        maxDifference = Math.Abs(score.CourseRating - tar.Rating);
                    }
                }
                // If no course ratings are within 1, just show the course rating.
                // This assumes the course rating doesn't change by 1 over time.
                if (maxDifference > 1)
                {
                    dr.Tees.Text = score.CourseSlope.ToString();
                }
                
                if ((dr.Tees.Left + dr.Tees.PreferredWidth + 5) > dr.Score.Left)
                {
                    dr.Tees.Width = dr.Score.Left - dr.Tees.Left - 5;
                }
                else
                {
                    dr.Tees.Width = dr.Tees.PreferredWidth;
                }
                
                dr.Score.Text = score.GrossScore.ToString();
                dr.Score.Width = dr.Score.PreferredWidth;

                dr.AdjustedScore.Text = score.ESScore.ToString();
                dr.AdjustedScore.Width = dr.AdjustedScore.PreferredWidth;

                dr.Differential.Text = score.Differential.ToString().Replace("-", "+");
                dr.Differential.Width = dr.Differential.PreferredWidth;
                if (score.UsedInLocalHandicap)
                {
                    dr.Differential.BackColor = Color.Yellow;
                }
                else
                {
                    dr.Differential.BackColor = Color.Transparent;
                }
            }
            for (int i = pd.Scores.Count; i < _details.Count; i++)
            {
                dr = _details[i];
                dr.Date.Text = String.Empty;
                dr.Tournament.Text = string.Empty;
                dr.Tees.Text = string.Empty;
                dr.Score.Text = string.Empty;
                dr.AdjustedScore.Text = string.Empty;
                dr.Differential.Text = string.Empty;
                dr.Differential.BackColor = Color.Transparent;
            }
        }

        private void LocalHandicapCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            ResetScores();
        }

        private void LocalHandicapExistsCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            ResetScores();
        }

        private void ResetScores()
        {
            IncludeExclude.ReadExcludedList(_localHandicapDBByNumber, _localHandicapDBByName);
            AddLowHandicaps();
            AddSummaryScores();
            AddIndividualDetails();
            IndividualDetailsScrollBar.Value = 0;
            IndividualSearchTextBox.Text = string.Empty;
            ShowIndividualDetails(0);
        }

        private void SaveAsCSVButton_Click(object sender, EventArgs e)
        {
            string fileName = string.Empty;
            using (SaveFileDialog dlg = new SaveFileDialog())
            {
                dlg.AddExtension = true;
                dlg.DefaultExt = ".csv"; // Default file extension
                dlg.Filter = "CSV file (.csv)|*.csv"; // Filter files by extension
                dlg.ValidateNames = true;

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    fileName = dlg.FileName;
                }
                else
                {
                    return;
                }
            }

            try
            {
                using (TextWriter streamWriter = new StreamWriter(fileName))
                {
                    streamWriter.WriteLine("Last Name,First Name,SCGA Number,Lower of local/SCGA Handicap,SCGA Handicap,Local Handicap,Source");
                    foreach (ListViewItem lvi in ResultsListView.Items)
                    {
                        System.Diagnostics.Debug.Assert(lvi.SubItems.Count == 5, "invalid count of subitems -- no report saved");
                        string playerNumber = lvi.SubItems[SCGANumberField].Text;
                        while (playerNumber.StartsWith("0"))
                        {
                            playerNumber = playerNumber.TrimStart('0');
                        }
                        string localHandicap = (lvi.SubItems[LocalHandicapField].Text.Length == 0) ? lvi.SubItems[SCGAIndexField].Text : lvi.SubItems[LocalHandicapField].Text;
                        string source = (lvi.SubItems[LowerHandicapField].Text == lvi.SubItems[SCGAIndexField].Text) ? "SCGA" : "Local";

                        string name = lvi.SubItems[NameField].Text;
                        if (UpperCaseCheckBox.Checked)
                        {
                            name = name.ToUpper();
                        }

                        streamWriter.WriteLine(name.Replace(", ", ",") + "," +
                            playerNumber + "," +
                            lvi.SubItems[LowerHandicapField].Text + "," +
                            lvi.SubItems[SCGAIndexField].Text + "," +
                            localHandicap + "," +
                            source);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
            }
        }

        private void SaveAsTSVButton_Click(object sender, EventArgs e)
        {
            string fileName = string.Empty;
            using (SaveFileDialog dlg = new SaveFileDialog())
            {
                dlg.AddExtension = true;
                dlg.DefaultExt = ".txt"; // Default file extension
                dlg.Filter = "TXT file (.txt)|*.txt"; // Filter files by extension
                dlg.ValidateNames = true;

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    fileName = dlg.FileName;
                }
                else
                {
                    return;
                }
            }

            try
            {
                using (TextWriter streamWriter = new StreamWriter(fileName))
                {
                    streamWriter.WriteLine("Last Name\tFirst Name\tSCGA Number\tLower of local/SCGA Handicap\tSCGA Handicap\tLocal Handicap\tSource");
                    foreach (ListViewItem lvi in ResultsListView.Items)
                    {
                        System.Diagnostics.Debug.Assert(lvi.SubItems.Count == 5, "invalid count of subitems -- no report saved");
                        string playerNumber = lvi.SubItems[SCGANumberField].Text;
                        while (playerNumber.StartsWith("0"))
                        {
                            playerNumber = playerNumber.TrimStart('0');
                        }
                        string localHandicap = (lvi.SubItems[LocalHandicapField].Text.Length == 0) ? lvi.SubItems[SCGAIndexField].Text : lvi.SubItems[LocalHandicapField].Text;
                        string source = (lvi.SubItems[LowerHandicapField].Text == lvi.SubItems[SCGAIndexField].Text) ? "SCGA" : "Local";

                        string name = lvi.SubItems[NameField].Text;
                        if(UpperCaseCheckBox.Checked)
                        {
                            name = name.ToUpper();
                        }

                        streamWriter.WriteLine(name.Replace(", ", ",").Replace(",", "\t") + "\t" +
                            playerNumber + "\t" +
                            lvi.SubItems[LowerHandicapField].Text.Replace("-", "+") + "\t" +
                            lvi.SubItems[SCGAIndexField].Text.Replace("-", "+") + "\t" +
                            localHandicap.Replace("-", "+") + "\t" +
                            source);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
            }
        }

        private void WriteField(TextWriter tr, string field, int width)
        {
            // make sure string fits in width
            if (field.Length >= width)
            {
                field = field.Substring(0, width - 1);
            }

            tr.Write(field);

            for (int i = field.Length; i < width; i++)
            {
                tr.Write(" ");
            }
        }

        private void SaveAsTextButton_Click(object sender, EventArgs e)
        {
            string fileName = string.Empty;
            using (SaveFileDialog dlg = new SaveFileDialog())
            {
                dlg.AddExtension = true;
                dlg.DefaultExt = ".txt"; // Default file extension
                dlg.Filter = "Text file (.txt)|*.txt"; // Filter files by extension
                dlg.ValidateNames = true;
                dlg.FileName = PlayerNameLabel.Text;

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    fileName = dlg.FileName;
                }
                else
                {
                    return;
                }
            }

            using (TextWriter streamWriter = new StreamWriter(fileName))
            {
                if (HandicapTab.SelectedIndex == 1)
                {
                    // Individual details
                    if (UpperCaseCheckBox.Checked)
                    {
                        WriteField(streamWriter, PlayerNameLabel.Text.ToUpper(), 60);
                    }
                    else
                    {
                        WriteField(streamWriter, PlayerNameLabel.Text, 60);
                    }
                    WriteField(streamWriter, SCGAIndexLabel.Text, 30);
                    streamWriter.WriteLine();

                    WriteField(streamWriter, SCGANumberLabel.Text, 60);
                    WriteField(streamWriter, LocalHandicapLabel.Text, 30);
                    streamWriter.WriteLine();

                    streamWriter.WriteLine();
                    WriteField(streamWriter, DateLabel.Text, 10);
                    WriteField(streamWriter, TournamentLabel.Text, 40);
                    WriteField(streamWriter, TeesLabel.Text, 10);
                    WriteField(streamWriter, ScoreLabel.Text, 8);
                    WriteField(streamWriter, AdjustedScoreLabel.Text, 8);
                    WriteField(streamWriter, DifferentialLabel.Text, 8);
                    streamWriter.WriteLine();

                    for (int i = 0; i < _details.Count; i++)
                    {
                        if (_details[i].Date.Text != string.Empty)
                        {
                            WriteField(streamWriter, _details[i].Date.Text, 10);
                            WriteField(streamWriter, _details[i].Tournament.Text, 40);
                            WriteField(streamWriter, _details[i].Tees.Text, 10);
                            WriteField(streamWriter, _details[i].Score.Text, 8);
                            WriteField(streamWriter, _details[i].AdjustedScore.Text, 8);
                            if (_details[i].Differential.BackColor == Color.Yellow)
                            {
                                WriteField(streamWriter, _details[i].Differential.Text + "*", 8);
                            }
                            else
                            {
                                WriteField(streamWriter, _details[i].Differential.Text, 8);
                            }
                            streamWriter.WriteLine();
                        }
                    }

                }
                else
                {
                    // Local Event Handicaps
                    int spacing = 2;
                    WriteField(streamWriter, "Name", _longestName + spacing);

                    string scgaNumberHeader = "SCGA Number";
                    int scgaNumberWidth = scgaNumberHeader.Length + spacing;
                    WriteField(streamWriter, scgaNumberHeader, scgaNumberWidth);

                    string handicapHeader = "Handicap";
                    int handicapWidth = handicapHeader.Length + spacing;
                    WriteField(streamWriter, handicapHeader, handicapWidth);

                    string scgaHandicapHeader = "SCGA Hcp";
                    int scgaHandicapWidth = scgaHandicapHeader.Length + spacing;
                    WriteField(streamWriter, scgaHandicapHeader, scgaHandicapWidth);

                    string localHandicapHeader = "Local Hcp";
                    int localHandicapWidth = localHandicapHeader.Length + spacing;
                    WriteField(streamWriter, localHandicapHeader, localHandicapWidth);

                    string handicapSourceHeader = "Hcp Source";
                    streamWriter.WriteLine(handicapSourceHeader);

                    foreach (ListViewItem lvi in ResultsListView.Items)
                    {
                        System.Diagnostics.Debug.Assert(lvi.SubItems.Count == 5, "invalid count of subitems -- no report saved");

                        string localHandicap = (lvi.SubItems[LocalHandicapField].Text.Length == 0) ? lvi.SubItems[SCGAIndexField].Text : lvi.SubItems[LocalHandicapField].Text;
                        string source = (lvi.SubItems[LowerHandicapField].Text == lvi.SubItems[SCGAIndexField].Text) ? "SCGA" : "Local";

                        if (UpperCaseCheckBox.Checked)
                        {
                            WriteField(streamWriter, lvi.SubItems[NameField].Text.ToUpper(), _longestName + spacing);
                        }
                        else
                        {
                            WriteField(streamWriter, lvi.SubItems[NameField].Text, _longestName + spacing);
                        }
                        WriteField(streamWriter, lvi.SubItems[SCGANumberField].Text, scgaNumberWidth);
                        WriteField(streamWriter, lvi.SubItems[LowerHandicapField].Text, handicapWidth);
                        WriteField(streamWriter, lvi.SubItems[SCGAIndexField].Text, scgaHandicapWidth);
                        WriteField(streamWriter, lvi.SubItems[LocalHandicapField].Text, localHandicapWidth);
                        streamWriter.WriteLine(source);
                    }
                }
            }
        }

        private void SaveAsHTMLButton_Click(object sender, EventArgs e)
        {
            float slope = 0f;
            try
            {
                slope = float.Parse(_courseSlope);
            }
            catch
            {
            }

            if (slope == 0f)
            {
                MessageBox.Show("This page calculates the course handicap.  To calculate the course handicap, you must fill in the course slope in the main form.");
                return;
            }

            string fileName = string.Empty;
            using (SaveFileDialog dlg = new SaveFileDialog())
            {
                dlg.AddExtension = true;
                dlg.DefaultExt = ".html"; // Default file extension
                dlg.Filter = "HTML file (.html)|*.html"; // Filter files by extension
                dlg.ValidateNames = true;

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    fileName = dlg.FileName;
                }
                else
                {
                    return;
                }
            }

            using (TextWriter streamWriter = new StreamWriter(fileName))
            {
                MonthQuestion mq = new MonthQuestion();
                mq.ShowDialog();
                if (mq.DialogResult != DialogResult.OK)
                {
                    return;
                }
                string date = mq.Date.ToString("MMMM dd, yyyy");
                mq.Dispose();

                streamWriter.WriteLine("<html>");
                streamWriter.WriteLine("<head>");
                streamWriter.WriteLine("<p align=center><b>Local Handicap for " + _courseName + " (" + date + ")</b></p>");
                streamWriter.WriteLine("</head>");
                streamWriter.WriteLine("<body>");
                streamWriter.WriteLine("<p align=center><table border=0 cellpadding=0 cellspacing=0>");
                streamWriter.WriteLine("<col width=260>");
                streamWriter.WriteLine("<col width=130>");
                streamWriter.WriteLine("<col width=130>");
                streamWriter.WriteLine("<col width=150>");

                streamWriter.WriteLine("<tr height=20 style='height:15.0pt'>");
                streamWriter.WriteLine("<th align=left>Name</td>");
                streamWriter.WriteLine("<th align=right>SCGA Index</th>");
                streamWriter.WriteLine("<th align=right>Local Handicap</th>");
                streamWriter.WriteLine("<th align=right>Local Tournament Handicap</th></tr>");

                foreach (ListViewItem lvi in ResultsListView.Items)
                {
                    System.Diagnostics.Debug.Assert(lvi.SubItems.Count == 5, "invalid count of subitems -- no report saved");

                    streamWriter.Write("<tr height=20 style='height:15.0pt'><td>");
                    if (UpperCaseCheckBox.Checked)
                    {
                        streamWriter.Write(lvi.SubItems[NameField].Text.ToUpper());
                    }
                    else
                    {
                        streamWriter.Write(lvi.SubItems[NameField].Text);
                    }
                    streamWriter.Write("</td><td align=right>");
                    streamWriter.Write(lvi.SubItems[SCGAIndexField].Text);
                    streamWriter.Write("</td><td align=right>");
                    streamWriter.Write(lvi.SubItems[LocalHandicapField].Text);
                    streamWriter.Write("</td><td align=right>");

                    float? lowerHcp = null;
                    try
                    {
                        lowerHcp = float.Parse(lvi.SubItems[LowerHandicapField].Text.Replace("+", "-"));
                    }
                    catch
                    {
                    }

                    if (lowerHcp == null)
                    {
                        streamWriter.Write(" ");
                    }
                    else
                    {
                        float? courseHcpFloat = (lowerHcp * (slope / 113f));
                        int courseHcp = (int)Math.Round(courseHcpFloat ?? 0, MidpointRounding.AwayFromZero);
                        streamWriter.Write(courseHcp.ToString().Replace("-", "+"));
                    }
                    streamWriter.WriteLine("</td></tr>");
                }

                streamWriter.WriteLine("</table></p>");
                streamWriter.WriteLine("</body>");
                streamWriter.WriteLine("</html>");
            }
        }

        private void HandicapTab_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (HandicapTab.SelectedIndex == 0)
            {
                SaveAsCSVButton.Visible = true;
                SaveAsTSVButton.Visible = true;
                SaveAsHTMLButton.Visible = true;
                SaveAsTextButton.Visible = true;
                ExcludePlayersButton.Visible = true;
            }
            else
            {
                SaveAsCSVButton.Visible = false;
                SaveAsTSVButton.Visible = false;
                SaveAsHTMLButton.Visible = false;
                SaveAsTextButton.Visible = false;
                ExcludePlayersButton.Visible = false;

                if (HandicapTab.SelectedIndex == 1)
                {
                    SaveAsTextButton.Visible = true;
                }
            }
        }

        private void IndividualDetailsScrollBar_ValueChanged(object sender, EventArgs e)
        {
            ShowIndividualDetails(IndividualDetailsScrollBar.Value);
        }

        private void IndividualSearchTextBox_TextChanged(object sender, EventArgs e)
        {
            string name = IndividualSearchTextBox.Text.ToLower();
            foreach (KeyValuePair<string, PlayerData> entry in _localHandicapDBByName)
            {
                if (entry.Value.IndividualTabIndex != -1)
                {
                    if (entry.Value.Name.ToLower().StartsWith(name))
                    {
                       IndividualDetailsScrollBar.Value = entry.Value.IndividualTabIndex;
                        break;
                    }
                }
            }
        }

        public class DetailsRecord
        {
            public DetailsRecord(int top, ResultsUserControl results)
            {
                Date = new Label();
                Date.Location = new Point(results.DateLabel.Left, top);
                Date.TextAlign = ContentAlignment.MiddleLeft;
                Tournament = new Label();
                Tournament.Location = new Point(results.TournamentLabel.Left, top);
                Tournament.TextAlign = ContentAlignment.MiddleLeft;
                Tees = new Label();
                Tees.Location = new Point(results.TeesLabel.Left, top);
                Tees.TextAlign = ContentAlignment.MiddleLeft;
                Score = new Label();
                Score.Location = new Point(results.ScoreLabel.Left, top);
                Score.TextAlign = ContentAlignment.MiddleLeft;
                AdjustedScore = new Label();
                AdjustedScore.Location = new Point(results.AdjustedScoreLabel.Left, top);
                AdjustedScore.TextAlign = ContentAlignment.MiddleLeft;
                Differential = new Label();
                Differential.Location = new Point(results.DifferentialLabel.Left, top);
                Differential.TextAlign = ContentAlignment.MiddleRight;
            }

            public void AddToControl(Control control)
            {
                control.SuspendLayout();
                control.Controls.Add(Date);
                control.Controls.Add(Tournament);
                control.Controls.Add(Tees);
                control.Controls.Add(Score);
                control.Controls.Add(AdjustedScore);
                control.Controls.Add(Differential);
                control.ResumeLayout();
            }

            public Label Date;
            public Label Tournament;
            public Label Tees;
            public Label Score;
            public Label AdjustedScore;
            public Label Differential;
        }

        private void ResultsUserControl_Resize(object sender, EventArgs e)
        {
            if (this.Visible)
            {
                int space = (this.Width - DoneButton.Width - SaveAsCSVButton.Width - 
                    SaveAsTSVButton.Width - SaveAsHTMLButton.Width - SaveAsTextButton.Width) / 6;
                if (space < 0)
                {
                    space = 5;
                }
                SaveAsCSVButton.Left = space;
                SaveAsTSVButton.Left = SaveAsCSVButton.Right + space;
                SaveAsTextButton.Left = SaveAsTSVButton.Right + space;
                SaveAsHTMLButton.Left = SaveAsTextButton.Right + space;
                DoneButton.Left = SaveAsHTMLButton.Right + space;
                ExcludePlayersButton.Left = DoneButton.Left;
            }
        }

        private void ExcludePlayersButton_Click(object sender, EventArgs e)
        {
            IncludeExclude includeExclude = new IncludeExclude(_localHandicapDBByName);
            includeExclude.ShowDialog();
            if (includeExclude.DialogResult == DialogResult.OK)
            {
                // Re-load the UI
                ResetScores();
            }

            includeExclude.Dispose();
        }
    }
}
