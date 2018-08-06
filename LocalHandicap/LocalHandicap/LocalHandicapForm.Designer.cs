namespace LocalHandicap
{
    partial class LocalHandicapForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.InputPathLabel = new System.Windows.Forms.Label();
            this.ActualInputPathLabel = new System.Windows.Forms.Label();
            this.InputPathBrowseButton = new System.Windows.Forms.Button();
            this.SCGAIndexGroupBox = new System.Windows.Forms.GroupBox();
            this.SCGAIndexDateLabel = new System.Windows.Forms.Label();
            this.SCGAClubCodeTextBox = new System.Windows.Forms.TextBox();
            this.SCGAClubCodeLabel = new System.Windows.Forms.Label();
            this.LocalHandicapGroupBox = new System.Windows.Forms.GroupBox();
            this.ConvertForGHINButton = new System.Windows.Forms.Button();
            this.LocalHandicapDateLabel = new System.Windows.Forms.Label();
            this.NumberOfTournamentsMonthsLabel = new System.Windows.Forms.Label();
            this.LocalHandicapMonthsLabel = new System.Windows.Forms.Label();
            this.DeleteTournamentButton = new System.Windows.Forms.Button();
            this.AddTournamentButton = new System.Windows.Forms.Button();
            this.NumberOfTournamentsNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.MonthsOfLocalScoresNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.MaxTournamentsLabel1 = new System.Windows.Forms.Label();
            this.LocalHandicapDateRangeLabel = new System.Windows.Forms.Label();
            this.CalculateHandicapButton = new System.Windows.Forms.Button();
            this.CourseInfoGroupBox1 = new System.Windows.Forms.GroupBox();
            this.CourseRatingTextBox3 = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.TeeTextBox3 = new System.Windows.Forms.TextBox();
            this.CourseRatingTextBox2 = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.TeeTextBox2 = new System.Windows.Forms.TextBox();
            this.CourseRatingTextBox1 = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.TeeTextBox1 = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.UsedByLabel = new System.Windows.Forms.Label();
            this.CourseSlopeTextBox = new System.Windows.Forms.TextBox();
            this.CourseNameTextBox = new System.Windows.Forms.TextBox();
            this.CourseSlopeLabel = new System.Windows.Forms.Label();
            this.CourseNameLabel = new System.Windows.Forms.Label();
            this.VersionLabel = new System.Windows.Forms.Label();
            this.SCGAIndexGroupBox.SuspendLayout();
            this.LocalHandicapGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NumberOfTournamentsNumericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.MonthsOfLocalScoresNumericUpDown)).BeginInit();
            this.CourseInfoGroupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // InputPathLabel
            // 
            this.InputPathLabel.AutoSize = true;
            this.InputPathLabel.Location = new System.Drawing.Point(16, 55);
            this.InputPathLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.InputPathLabel.Name = "InputPathLabel";
            this.InputPathLabel.Size = new System.Drawing.Size(32, 13);
            this.InputPathLabel.TabIndex = 3;
            this.InputPathLabel.Text = "Path:";
            this.InputPathLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // ActualInputPathLabel
            // 
            this.ActualInputPathLabel.AutoEllipsis = true;
            this.ActualInputPathLabel.AutoSize = true;
            this.ActualInputPathLabel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(176)))), ((int)(((byte)(0)))));
            this.ActualInputPathLabel.Location = new System.Drawing.Point(52, 54);
            this.ActualInputPathLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.ActualInputPathLabel.MaximumSize = new System.Drawing.Size(462, 14);
            this.ActualInputPathLabel.MinimumSize = new System.Drawing.Size(462, 14);
            this.ActualInputPathLabel.Name = "ActualInputPathLabel";
            this.ActualInputPathLabel.Size = new System.Drawing.Size(462, 14);
            this.ActualInputPathLabel.TabIndex = 4;
            this.ActualInputPathLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // InputPathBrowseButton
            // 
            this.InputPathBrowseButton.Location = new System.Drawing.Point(19, 19);
            this.InputPathBrowseButton.Margin = new System.Windows.Forms.Padding(2);
            this.InputPathBrowseButton.Name = "InputPathBrowseButton";
            this.InputPathBrowseButton.Size = new System.Drawing.Size(64, 24);
            this.InputPathBrowseButton.TabIndex = 5;
            this.InputPathBrowseButton.Text = "Browse ...";
            this.InputPathBrowseButton.UseVisualStyleBackColor = true;
            this.InputPathBrowseButton.Click += new System.EventHandler(this.InputPathBrowseButton_Click);
            // 
            // SCGAIndexGroupBox
            // 
            this.SCGAIndexGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.SCGAIndexGroupBox.Controls.Add(this.SCGAIndexDateLabel);
            this.SCGAIndexGroupBox.Controls.Add(this.SCGAClubCodeTextBox);
            this.SCGAIndexGroupBox.Controls.Add(this.SCGAClubCodeLabel);
            this.SCGAIndexGroupBox.Controls.Add(this.ActualInputPathLabel);
            this.SCGAIndexGroupBox.Controls.Add(this.InputPathBrowseButton);
            this.SCGAIndexGroupBox.Controls.Add(this.InputPathLabel);
            this.SCGAIndexGroupBox.ForeColor = System.Drawing.Color.Black;
            this.SCGAIndexGroupBox.Location = new System.Drawing.Point(19, 126);
            this.SCGAIndexGroupBox.Margin = new System.Windows.Forms.Padding(2);
            this.SCGAIndexGroupBox.Name = "SCGAIndexGroupBox";
            this.SCGAIndexGroupBox.Padding = new System.Windows.Forms.Padding(2);
            this.SCGAIndexGroupBox.Size = new System.Drawing.Size(565, 100);
            this.SCGAIndexGroupBox.TabIndex = 6;
            this.SCGAIndexGroupBox.TabStop = false;
            this.SCGAIndexGroupBox.Text = "SCGA Index";
            // 
            // SCGAIndexDateLabel
            // 
            this.SCGAIndexDateLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.SCGAIndexDateLabel.AutoSize = true;
            this.SCGAIndexDateLabel.Location = new System.Drawing.Point(304, 25);
            this.SCGAIndexDateLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.SCGAIndexDateLabel.Name = "SCGAIndexDateLabel";
            this.SCGAIndexDateLabel.Size = new System.Drawing.Size(31, 13);
            this.SCGAIndexDateLabel.TabIndex = 8;
            this.SCGAIndexDateLabel.Text = "        ";
            // 
            // SCGAClubCodeTextBox
            // 
            this.SCGAClubCodeTextBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.SCGAClubCodeTextBox.Location = new System.Drawing.Point(237, 22);
            this.SCGAClubCodeTextBox.Margin = new System.Windows.Forms.Padding(2);
            this.SCGAClubCodeTextBox.Name = "SCGAClubCodeTextBox";
            this.SCGAClubCodeTextBox.Size = new System.Drawing.Size(43, 20);
            this.SCGAClubCodeTextBox.TabIndex = 7;
            this.SCGAClubCodeTextBox.Visible = false;
            this.SCGAClubCodeTextBox.TextChanged += new System.EventHandler(this.SCGAClubCodeTextBox_TextChanged);
            // 
            // SCGAClubCodeLabel
            // 
            this.SCGAClubCodeLabel.AutoSize = true;
            this.SCGAClubCodeLabel.Location = new System.Drawing.Point(150, 25);
            this.SCGAClubCodeLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.SCGAClubCodeLabel.Name = "SCGAClubCodeLabel";
            this.SCGAClubCodeLabel.Size = new System.Drawing.Size(91, 13);
            this.SCGAClubCodeLabel.TabIndex = 6;
            this.SCGAClubCodeLabel.Text = "SCGA Club Code:";
            this.SCGAClubCodeLabel.Visible = false;
            // 
            // LocalHandicapGroupBox
            // 
            this.LocalHandicapGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.LocalHandicapGroupBox.Controls.Add(this.ConvertForGHINButton);
            this.LocalHandicapGroupBox.Controls.Add(this.LocalHandicapDateLabel);
            this.LocalHandicapGroupBox.Controls.Add(this.NumberOfTournamentsMonthsLabel);
            this.LocalHandicapGroupBox.Controls.Add(this.LocalHandicapMonthsLabel);
            this.LocalHandicapGroupBox.Controls.Add(this.DeleteTournamentButton);
            this.LocalHandicapGroupBox.Controls.Add(this.AddTournamentButton);
            this.LocalHandicapGroupBox.Controls.Add(this.NumberOfTournamentsNumericUpDown);
            this.LocalHandicapGroupBox.Controls.Add(this.MonthsOfLocalScoresNumericUpDown);
            this.LocalHandicapGroupBox.Controls.Add(this.MaxTournamentsLabel1);
            this.LocalHandicapGroupBox.Controls.Add(this.LocalHandicapDateRangeLabel);
            this.LocalHandicapGroupBox.ForeColor = System.Drawing.Color.Black;
            this.LocalHandicapGroupBox.Location = new System.Drawing.Point(21, 231);
            this.LocalHandicapGroupBox.Margin = new System.Windows.Forms.Padding(2);
            this.LocalHandicapGroupBox.Name = "LocalHandicapGroupBox";
            this.LocalHandicapGroupBox.Padding = new System.Windows.Forms.Padding(2);
            this.LocalHandicapGroupBox.Size = new System.Drawing.Size(562, 108);
            this.LocalHandicapGroupBox.TabIndex = 7;
            this.LocalHandicapGroupBox.TabStop = false;
            this.LocalHandicapGroupBox.Text = "Local Handicap";
            // 
            // ConvertForGHINButton
            // 
            this.ConvertForGHINButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ConvertForGHINButton.Location = new System.Drawing.Point(424, 76);
            this.ConvertForGHINButton.Margin = new System.Windows.Forms.Padding(2);
            this.ConvertForGHINButton.Name = "ConvertForGHINButton";
            this.ConvertForGHINButton.Size = new System.Drawing.Size(132, 24);
            this.ConvertForGHINButton.TabIndex = 7;
            this.ConvertForGHINButton.Text = "Convert VPT for GHIN";
            this.ConvertForGHINButton.UseVisualStyleBackColor = true;
            this.ConvertForGHINButton.Click += new System.EventHandler(this.ConvertForGHINButton_Click);
            // 
            // LocalHandicapDateLabel
            // 
            this.LocalHandicapDateLabel.AutoSize = true;
            this.LocalHandicapDateLabel.Location = new System.Drawing.Point(14, 21);
            this.LocalHandicapDateLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.LocalHandicapDateLabel.Name = "LocalHandicapDateLabel";
            this.LocalHandicapDateLabel.Size = new System.Drawing.Size(35, 13);
            this.LocalHandicapDateLabel.TabIndex = 6;
            this.LocalHandicapDateLabel.Text = "label1";
            // 
            // NumberOfTournamentsMonthsLabel
            // 
            this.NumberOfTournamentsMonthsLabel.AutoSize = true;
            this.NumberOfTournamentsMonthsLabel.Location = new System.Drawing.Point(105, 79);
            this.NumberOfTournamentsMonthsLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.NumberOfTournamentsMonthsLabel.Name = "NumberOfTournamentsMonthsLabel";
            this.NumberOfTournamentsMonthsLabel.Size = new System.Drawing.Size(94, 13);
            this.NumberOfTournamentsMonthsLabel.TabIndex = 2;
            this.NumberOfTournamentsMonthsLabel.Text = "tournament scores";
            // 
            // LocalHandicapMonthsLabel
            // 
            this.LocalHandicapMonthsLabel.AutoSize = true;
            this.LocalHandicapMonthsLabel.Location = new System.Drawing.Point(105, 49);
            this.LocalHandicapMonthsLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.LocalHandicapMonthsLabel.Name = "LocalHandicapMonthsLabel";
            this.LocalHandicapMonthsLabel.Size = new System.Drawing.Size(41, 13);
            this.LocalHandicapMonthsLabel.TabIndex = 2;
            this.LocalHandicapMonthsLabel.Text = "months";
            // 
            // DeleteTournamentButton
            // 
            this.DeleteTournamentButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.DeleteTournamentButton.Location = new System.Drawing.Point(424, 46);
            this.DeleteTournamentButton.Margin = new System.Windows.Forms.Padding(2);
            this.DeleteTournamentButton.Name = "DeleteTournamentButton";
            this.DeleteTournamentButton.Size = new System.Drawing.Size(132, 24);
            this.DeleteTournamentButton.TabIndex = 5;
            this.DeleteTournamentButton.Text = "Delete Tournaments";
            this.DeleteTournamentButton.UseVisualStyleBackColor = true;
            this.DeleteTournamentButton.Click += new System.EventHandler(this.DeleteTournamentButton_Click);
            // 
            // AddTournamentButton
            // 
            this.AddTournamentButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.AddTournamentButton.Location = new System.Drawing.Point(424, 16);
            this.AddTournamentButton.Margin = new System.Windows.Forms.Padding(2);
            this.AddTournamentButton.Name = "AddTournamentButton";
            this.AddTournamentButton.Size = new System.Drawing.Size(132, 24);
            this.AddTournamentButton.TabIndex = 5;
            this.AddTournamentButton.Text = "Add Tournament";
            this.AddTournamentButton.UseVisualStyleBackColor = true;
            this.AddTournamentButton.Click += new System.EventHandler(this.AddTournamentButton_Click);
            // 
            // NumberOfTournamentsNumericUpDown
            // 
            this.NumberOfTournamentsNumericUpDown.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.NumberOfTournamentsNumericUpDown.Location = new System.Drawing.Point(68, 77);
            this.NumberOfTournamentsNumericUpDown.Margin = new System.Windows.Forms.Padding(2);
            this.NumberOfTournamentsNumericUpDown.Maximum = new decimal(new int[] {
            20,
            0,
            0,
            0});
            this.NumberOfTournamentsNumericUpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.NumberOfTournamentsNumericUpDown.Name = "NumberOfTournamentsNumericUpDown";
            this.NumberOfTournamentsNumericUpDown.Size = new System.Drawing.Size(33, 20);
            this.NumberOfTournamentsNumericUpDown.TabIndex = 1;
            this.NumberOfTournamentsNumericUpDown.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.NumberOfTournamentsNumericUpDown.ValueChanged += new System.EventHandler(this.NumberOfTournamentsNumericUpDown_ValueChanged);
            // 
            // MonthsOfLocalScoresNumericUpDown
            // 
            this.MonthsOfLocalScoresNumericUpDown.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.MonthsOfLocalScoresNumericUpDown.Location = new System.Drawing.Point(68, 47);
            this.MonthsOfLocalScoresNumericUpDown.Margin = new System.Windows.Forms.Padding(2);
            this.MonthsOfLocalScoresNumericUpDown.Maximum = new decimal(new int[] {
            24,
            0,
            0,
            0});
            this.MonthsOfLocalScoresNumericUpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.MonthsOfLocalScoresNumericUpDown.Name = "MonthsOfLocalScoresNumericUpDown";
            this.MonthsOfLocalScoresNumericUpDown.Size = new System.Drawing.Size(33, 20);
            this.MonthsOfLocalScoresNumericUpDown.TabIndex = 1;
            this.MonthsOfLocalScoresNumericUpDown.Value = new decimal(new int[] {
            12,
            0,
            0,
            0});
            this.MonthsOfLocalScoresNumericUpDown.ValueChanged += new System.EventHandler(this.MonthsOfLocalScoresNumericUpDown_ValueChanged);
            // 
            // MaxTournamentsLabel1
            // 
            this.MaxTournamentsLabel1.AutoSize = true;
            this.MaxTournamentsLabel1.Location = new System.Drawing.Point(14, 79);
            this.MaxTournamentsLabel1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.MaxTournamentsLabel1.Name = "MaxTournamentsLabel1";
            this.MaxTournamentsLabel1.Size = new System.Drawing.Size(53, 13);
            this.MaxTournamentsLabel1.TabIndex = 0;
            this.MaxTournamentsLabel1.Text = "Use up to";
            // 
            // LocalHandicapDateRangeLabel
            // 
            this.LocalHandicapDateRangeLabel.AutoSize = true;
            this.LocalHandicapDateRangeLabel.Location = new System.Drawing.Point(14, 49);
            this.LocalHandicapDateRangeLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.LocalHandicapDateRangeLabel.Name = "LocalHandicapDateRangeLabel";
            this.LocalHandicapDateRangeLabel.Size = new System.Drawing.Size(45, 13);
            this.LocalHandicapDateRangeLabel.TabIndex = 0;
            this.LocalHandicapDateRangeLabel.Text = "Use last";
            // 
            // CalculateHandicapButton
            // 
            this.CalculateHandicapButton.Location = new System.Drawing.Point(222, 359);
            this.CalculateHandicapButton.Margin = new System.Windows.Forms.Padding(2);
            this.CalculateHandicapButton.Name = "CalculateHandicapButton";
            this.CalculateHandicapButton.Size = new System.Drawing.Size(120, 24);
            this.CalculateHandicapButton.TabIndex = 8;
            this.CalculateHandicapButton.Text = "Calculate Handicaps";
            this.CalculateHandicapButton.UseVisualStyleBackColor = true;
            this.CalculateHandicapButton.Click += new System.EventHandler(this.CalculateHandicapButton_Click);
            // 
            // CourseInfoGroupBox1
            // 
            this.CourseInfoGroupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.CourseInfoGroupBox1.Controls.Add(this.CourseRatingTextBox3);
            this.CourseInfoGroupBox1.Controls.Add(this.label4);
            this.CourseInfoGroupBox1.Controls.Add(this.TeeTextBox3);
            this.CourseInfoGroupBox1.Controls.Add(this.CourseRatingTextBox2);
            this.CourseInfoGroupBox1.Controls.Add(this.label3);
            this.CourseInfoGroupBox1.Controls.Add(this.TeeTextBox2);
            this.CourseInfoGroupBox1.Controls.Add(this.CourseRatingTextBox1);
            this.CourseInfoGroupBox1.Controls.Add(this.label2);
            this.CourseInfoGroupBox1.Controls.Add(this.TeeTextBox1);
            this.CourseInfoGroupBox1.Controls.Add(this.label1);
            this.CourseInfoGroupBox1.Controls.Add(this.UsedByLabel);
            this.CourseInfoGroupBox1.Controls.Add(this.CourseSlopeTextBox);
            this.CourseInfoGroupBox1.Controls.Add(this.CourseNameTextBox);
            this.CourseInfoGroupBox1.Controls.Add(this.CourseSlopeLabel);
            this.CourseInfoGroupBox1.Controls.Add(this.CourseNameLabel);
            this.CourseInfoGroupBox1.ForeColor = System.Drawing.Color.Black;
            this.CourseInfoGroupBox1.Location = new System.Drawing.Point(21, 22);
            this.CourseInfoGroupBox1.Margin = new System.Windows.Forms.Padding(2);
            this.CourseInfoGroupBox1.Name = "CourseInfoGroupBox1";
            this.CourseInfoGroupBox1.Padding = new System.Windows.Forms.Padding(2);
            this.CourseInfoGroupBox1.Size = new System.Drawing.Size(562, 100);
            this.CourseInfoGroupBox1.TabIndex = 9;
            this.CourseInfoGroupBox1.TabStop = false;
            this.CourseInfoGroupBox1.Text = "Course Information";
            // 
            // CourseRatingTextBox3
            // 
            this.CourseRatingTextBox3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.CourseRatingTextBox3.Location = new System.Drawing.Point(499, 68);
            this.CourseRatingTextBox3.Margin = new System.Windows.Forms.Padding(2);
            this.CourseRatingTextBox3.Name = "CourseRatingTextBox3";
            this.CourseRatingTextBox3.Size = new System.Drawing.Size(55, 20);
            this.CourseRatingTextBox3.TabIndex = 14;
            this.CourseRatingTextBox3.TextChanged += new System.EventHandler(this.CourseRatingTextBox3_TextChanged);
            this.CourseRatingTextBox3.Validating += new System.ComponentModel.CancelEventHandler(this.CourseRatingTextBox3_Validating);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(483, 71);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(12, 13);
            this.label4.TabIndex = 13;
            this.label4.Text = "/";
            // 
            // TeeTextBox3
            // 
            this.TeeTextBox3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.TeeTextBox3.Location = new System.Drawing.Point(424, 68);
            this.TeeTextBox3.Margin = new System.Windows.Forms.Padding(2);
            this.TeeTextBox3.Name = "TeeTextBox3";
            this.TeeTextBox3.Size = new System.Drawing.Size(55, 20);
            this.TeeTextBox3.TabIndex = 12;
            this.TeeTextBox3.TextChanged += new System.EventHandler(this.TeeTextBox3_TextChanged);
            // 
            // CourseRatingTextBox2
            // 
            this.CourseRatingTextBox2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.CourseRatingTextBox2.Location = new System.Drawing.Point(329, 68);
            this.CourseRatingTextBox2.Margin = new System.Windows.Forms.Padding(2);
            this.CourseRatingTextBox2.Name = "CourseRatingTextBox2";
            this.CourseRatingTextBox2.Size = new System.Drawing.Size(55, 20);
            this.CourseRatingTextBox2.TabIndex = 11;
            this.CourseRatingTextBox2.TextChanged += new System.EventHandler(this.CourseRatingTextBox2_TextChanged);
            this.CourseRatingTextBox2.Validating += new System.ComponentModel.CancelEventHandler(this.CourseRatingTextBox2_Validating);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(313, 71);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(12, 13);
            this.label3.TabIndex = 10;
            this.label3.Text = "/";
            // 
            // TeeTextBox2
            // 
            this.TeeTextBox2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.TeeTextBox2.Location = new System.Drawing.Point(254, 68);
            this.TeeTextBox2.Margin = new System.Windows.Forms.Padding(2);
            this.TeeTextBox2.Name = "TeeTextBox2";
            this.TeeTextBox2.Size = new System.Drawing.Size(55, 20);
            this.TeeTextBox2.TabIndex = 9;
            this.TeeTextBox2.TextChanged += new System.EventHandler(this.TeeTextBox2_TextChanged);
            // 
            // CourseRatingTextBox1
            // 
            this.CourseRatingTextBox1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.CourseRatingTextBox1.Location = new System.Drawing.Point(166, 68);
            this.CourseRatingTextBox1.Margin = new System.Windows.Forms.Padding(2);
            this.CourseRatingTextBox1.Name = "CourseRatingTextBox1";
            this.CourseRatingTextBox1.Size = new System.Drawing.Size(55, 20);
            this.CourseRatingTextBox1.TabIndex = 8;
            this.CourseRatingTextBox1.TextChanged += new System.EventHandler(this.CourseRatingTextBox1_TextChanged);
            this.CourseRatingTextBox1.Validating += new System.ComponentModel.CancelEventHandler(this.CourseRatingTextBox1_Validating);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(150, 71);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(12, 13);
            this.label2.TabIndex = 7;
            this.label2.Text = "/";
            // 
            // TeeTextBox1
            // 
            this.TeeTextBox1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.TeeTextBox1.Location = new System.Drawing.Point(91, 68);
            this.TeeTextBox1.Margin = new System.Windows.Forms.Padding(2);
            this.TeeTextBox1.Name = "TeeTextBox1";
            this.TeeTextBox1.Size = new System.Drawing.Size(55, 20);
            this.TeeTextBox1.TabIndex = 6;
            this.TeeTextBox1.TextChanged += new System.EventHandler(this.TeeTextBox1_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(14, 71);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(71, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Tee / Rating:";
            // 
            // UsedByLabel
            // 
            this.UsedByLabel.AutoSize = true;
            this.UsedByLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.UsedByLabel.Location = new System.Drawing.Point(148, 46);
            this.UsedByLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.UsedByLabel.Name = "UsedByLabel";
            this.UsedByLabel.Size = new System.Drawing.Size(137, 13);
            this.UsedByLabel.TabIndex = 4;
            this.UsedByLabel.Text = "(Used by \"Save as HTML\")";
            this.UsedByLabel.Visible = false;
            // 
            // CourseSlopeTextBox
            // 
            this.CourseSlopeTextBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.CourseSlopeTextBox.Location = new System.Drawing.Point(91, 44);
            this.CourseSlopeTextBox.Margin = new System.Windows.Forms.Padding(2);
            this.CourseSlopeTextBox.Name = "CourseSlopeTextBox";
            this.CourseSlopeTextBox.Size = new System.Drawing.Size(38, 20);
            this.CourseSlopeTextBox.TabIndex = 3;
            this.CourseSlopeTextBox.TextChanged += new System.EventHandler(this.CourseSlopeTextBox_TextChanged);
            this.CourseSlopeTextBox.Validating += new System.ComponentModel.CancelEventHandler(this.CourseSlopeTextBox_Validating);
            // 
            // CourseNameTextBox
            // 
            this.CourseNameTextBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.CourseNameTextBox.Location = new System.Drawing.Point(92, 20);
            this.CourseNameTextBox.Margin = new System.Windows.Forms.Padding(2);
            this.CourseNameTextBox.Name = "CourseNameTextBox";
            this.CourseNameTextBox.Size = new System.Drawing.Size(198, 20);
            this.CourseNameTextBox.TabIndex = 2;
            this.CourseNameTextBox.TextChanged += new System.EventHandler(this.CourseNameTextBox_TextChanged);
            // 
            // CourseSlopeLabel
            // 
            this.CourseSlopeLabel.AutoSize = true;
            this.CourseSlopeLabel.Location = new System.Drawing.Point(14, 46);
            this.CourseSlopeLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.CourseSlopeLabel.Name = "CourseSlopeLabel";
            this.CourseSlopeLabel.Size = new System.Drawing.Size(73, 13);
            this.CourseSlopeLabel.TabIndex = 1;
            this.CourseSlopeLabel.Text = "Course Slope:";
            this.CourseSlopeLabel.Visible = false;
            // 
            // CourseNameLabel
            // 
            this.CourseNameLabel.AutoSize = true;
            this.CourseNameLabel.Location = new System.Drawing.Point(14, 22);
            this.CourseNameLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.CourseNameLabel.Name = "CourseNameLabel";
            this.CourseNameLabel.Size = new System.Drawing.Size(74, 13);
            this.CourseNameLabel.TabIndex = 0;
            this.CourseNameLabel.Text = "Course Name:";
            // 
            // VersionLabel
            // 
            this.VersionLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.VersionLabel.AutoSize = true;
            this.VersionLabel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(176)))), ((int)(((byte)(0)))));
            this.VersionLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(0)));
            this.VersionLabel.Location = new System.Drawing.Point(542, 403);
            this.VersionLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.VersionLabel.Name = "VersionLabel";
            this.VersionLabel.Size = new System.Drawing.Size(52, 12);
            this.VersionLabel.TabIndex = 10;
            this.VersionLabel.Text = "Version 2.3";
            // 
            // LocalHandicapForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(176)))), ((int)(((byte)(0)))));
            this.ClientSize = new System.Drawing.Size(605, 424);
            this.Controls.Add(this.VersionLabel);
            this.Controls.Add(this.CourseInfoGroupBox1);
            this.Controls.Add(this.CalculateHandicapButton);
            this.Controls.Add(this.LocalHandicapGroupBox);
            this.Controls.Add(this.SCGAIndexGroupBox);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MinimumSize = new System.Drawing.Size(573, 462);
            this.Name = "LocalHandicapForm";
            this.Text = "Local Handicap";
            this.Resize += new System.EventHandler(this.LocalHandicapForm_Resize);
            this.SCGAIndexGroupBox.ResumeLayout(false);
            this.SCGAIndexGroupBox.PerformLayout();
            this.LocalHandicapGroupBox.ResumeLayout(false);
            this.LocalHandicapGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NumberOfTournamentsNumericUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.MonthsOfLocalScoresNumericUpDown)).EndInit();
            this.CourseInfoGroupBox1.ResumeLayout(false);
            this.CourseInfoGroupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label InputPathLabel;
        private System.Windows.Forms.Label ActualInputPathLabel;
        private System.Windows.Forms.Button InputPathBrowseButton;
        private System.Windows.Forms.GroupBox SCGAIndexGroupBox;
        private System.Windows.Forms.GroupBox LocalHandicapGroupBox;
        private System.Windows.Forms.Label LocalHandicapMonthsLabel;
        private System.Windows.Forms.NumericUpDown MonthsOfLocalScoresNumericUpDown;
        private System.Windows.Forms.Label LocalHandicapDateRangeLabel;
        private System.Windows.Forms.Button AddTournamentButton;
        private System.Windows.Forms.Button CalculateHandicapButton;
        private System.Windows.Forms.Button DeleteTournamentButton;
        private System.Windows.Forms.GroupBox CourseInfoGroupBox1;
        private System.Windows.Forms.TextBox SCGAClubCodeTextBox;
        private System.Windows.Forms.Label SCGAClubCodeLabel;
        private System.Windows.Forms.TextBox CourseSlopeTextBox;
        private System.Windows.Forms.TextBox CourseNameTextBox;
        private System.Windows.Forms.Label CourseSlopeLabel;
        private System.Windows.Forms.Label CourseNameLabel;
        private System.Windows.Forms.Label VersionLabel;
        private System.Windows.Forms.Label UsedByLabel;
        private System.Windows.Forms.Label NumberOfTournamentsMonthsLabel;
        private System.Windows.Forms.NumericUpDown NumberOfTournamentsNumericUpDown;
        private System.Windows.Forms.Label MaxTournamentsLabel1;
        private System.Windows.Forms.Label SCGAIndexDateLabel;
        private System.Windows.Forms.Label LocalHandicapDateLabel;
        private System.Windows.Forms.Button ConvertForGHINButton;
        private System.Windows.Forms.TextBox CourseRatingTextBox3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox TeeTextBox3;
        private System.Windows.Forms.TextBox CourseRatingTextBox2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox TeeTextBox2;
        private System.Windows.Forms.TextBox CourseRatingTextBox1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox TeeTextBox1;
        private System.Windows.Forms.Label label1;
    }
}

