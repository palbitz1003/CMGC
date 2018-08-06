namespace LocalHandicap
{
    partial class ResultsUserControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.ResultsListView = new System.Windows.Forms.ListView();
            this.NameColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.SCGANumberColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.HandicapColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.SCGAIndexColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.LocalHandicapColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.DoneButton = new System.Windows.Forms.Button();
            this.HandicapTab = new System.Windows.Forms.TabControl();
            this.LowHandicapTab = new System.Windows.Forms.TabPage();
            this.DetailsTab = new System.Windows.Forms.TabPage();
            this.DetailsPanel = new System.Windows.Forms.Panel();
            this.DifferentialLabel = new System.Windows.Forms.Label();
            this.AdjustedScoreLabel = new System.Windows.Forms.Label();
            this.ScoreLabel = new System.Windows.Forms.Label();
            this.TeesLabel = new System.Windows.Forms.Label();
            this.TournamentLabel = new System.Windows.Forms.Label();
            this.DateLabel = new System.Windows.Forms.Label();
            this.LocalHandicapLabel = new System.Windows.Forms.Label();
            this.SCGAIndexLabel = new System.Windows.Forms.Label();
            this.SCGANumberLabel = new System.Windows.Forms.Label();
            this.PlayerNameLabel = new System.Windows.Forms.Label();
            this.SearchPanel = new System.Windows.Forms.Panel();
            this.IndividualSearchTextBox = new System.Windows.Forms.TextBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.LastNameLabel = new System.Windows.Forms.Label();
            this.IndividualDetailsScrollBar = new System.Windows.Forms.VScrollBar();
            this.SummaryTab = new System.Windows.Forms.TabPage();
            this.HandicapDetailsListView = new System.Windows.Forms.ListView();
            this.LocalHandicapLowerCheckBox = new System.Windows.Forms.CheckBox();
            this.SaveAsHTMLButton = new System.Windows.Forms.Button();
            this.SaveAsCSVButton = new System.Windows.Forms.Button();
            this.SaveAsTextButton = new System.Windows.Forms.Button();
            this.LocalHandicapExistsCheckbox = new System.Windows.Forms.CheckBox();
            this.SaveAsTSVButton = new System.Windows.Forms.Button();
            this.ExcludePlayersButton = new System.Windows.Forms.Button();
            this.UpperCaseCheckBox = new System.Windows.Forms.CheckBox();
            this.HandicapTab.SuspendLayout();
            this.LowHandicapTab.SuspendLayout();
            this.DetailsTab.SuspendLayout();
            this.DetailsPanel.SuspendLayout();
            this.SearchPanel.SuspendLayout();
            this.SummaryTab.SuspendLayout();
            this.SuspendLayout();
            // 
            // ResultsListView
            // 
            this.ResultsListView.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.ResultsListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.NameColumn,
            this.SCGANumberColumn,
            this.HandicapColumn,
            this.SCGAIndexColumn,
            this.LocalHandicapColumn});
            this.ResultsListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ResultsListView.FullRowSelect = true;
            this.ResultsListView.Location = new System.Drawing.Point(2, 2);
            this.ResultsListView.Margin = new System.Windows.Forms.Padding(2);
            this.ResultsListView.MultiSelect = false;
            this.ResultsListView.Name = "ResultsListView";
            this.ResultsListView.Size = new System.Drawing.Size(519, 271);
            this.ResultsListView.TabIndex = 0;
            this.ResultsListView.UseCompatibleStateImageBehavior = false;
            this.ResultsListView.View = System.Windows.Forms.View.Details;
            this.ResultsListView.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.ResultsListView_ColumnClick);
            // 
            // NameColumn
            // 
            this.NameColumn.Text = "Name";
            this.NameColumn.Width = 160;
            // 
            // SCGANumberColumn
            // 
            this.SCGANumberColumn.Text = "Number";
            this.SCGANumberColumn.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.SCGANumberColumn.Width = 70;
            // 
            // HandicapColumn
            // 
            this.HandicapColumn.Text = "Local Event Index";
            this.HandicapColumn.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.HandicapColumn.Width = 100;
            // 
            // SCGAIndexColumn
            // 
            this.SCGAIndexColumn.Text = "SCGA Index";
            this.SCGAIndexColumn.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.SCGAIndexColumn.Width = 88;
            // 
            // LocalHandicapColumn
            // 
            this.LocalHandicapColumn.Text = "Local Handicap";
            this.LocalHandicapColumn.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.LocalHandicapColumn.Width = 108;
            // 
            // DoneButton
            // 
            this.DoneButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.DoneButton.Location = new System.Drawing.Point(446, 381);
            this.DoneButton.Margin = new System.Windows.Forms.Padding(2);
            this.DoneButton.Name = "DoneButton";
            this.DoneButton.Size = new System.Drawing.Size(96, 24);
            this.DoneButton.TabIndex = 1;
            this.DoneButton.Text = "Done";
            this.DoneButton.UseVisualStyleBackColor = true;
            this.DoneButton.Click += new System.EventHandler(this.DoneButton_Click);
            // 
            // HandicapTab
            // 
            this.HandicapTab.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.HandicapTab.Controls.Add(this.LowHandicapTab);
            this.HandicapTab.Controls.Add(this.DetailsTab);
            this.HandicapTab.Controls.Add(this.SummaryTab);
            this.HandicapTab.Location = new System.Drawing.Point(9, 10);
            this.HandicapTab.Margin = new System.Windows.Forms.Padding(2);
            this.HandicapTab.Name = "HandicapTab";
            this.HandicapTab.SelectedIndex = 0;
            this.HandicapTab.Size = new System.Drawing.Size(531, 301);
            this.HandicapTab.TabIndex = 2;
            this.HandicapTab.SelectedIndexChanged += new System.EventHandler(this.HandicapTab_SelectedIndexChanged);
            // 
            // LowHandicapTab
            // 
            this.LowHandicapTab.Controls.Add(this.ResultsListView);
            this.LowHandicapTab.Location = new System.Drawing.Point(4, 22);
            this.LowHandicapTab.Margin = new System.Windows.Forms.Padding(2);
            this.LowHandicapTab.Name = "LowHandicapTab";
            this.LowHandicapTab.Padding = new System.Windows.Forms.Padding(2);
            this.LowHandicapTab.Size = new System.Drawing.Size(523, 275);
            this.LowHandicapTab.TabIndex = 0;
            this.LowHandicapTab.Text = "Local Event Hcps";
            this.LowHandicapTab.UseVisualStyleBackColor = true;
            // 
            // DetailsTab
            // 
            this.DetailsTab.Controls.Add(this.DetailsPanel);
            this.DetailsTab.Controls.Add(this.SearchPanel);
            this.DetailsTab.Controls.Add(this.IndividualDetailsScrollBar);
            this.DetailsTab.Location = new System.Drawing.Point(4, 22);
            this.DetailsTab.Margin = new System.Windows.Forms.Padding(2);
            this.DetailsTab.Name = "DetailsTab";
            this.DetailsTab.Size = new System.Drawing.Size(523, 275);
            this.DetailsTab.TabIndex = 2;
            this.DetailsTab.Text = "Individual Details";
            this.DetailsTab.UseVisualStyleBackColor = true;
            // 
            // DetailsPanel
            // 
            this.DetailsPanel.Controls.Add(this.DifferentialLabel);
            this.DetailsPanel.Controls.Add(this.AdjustedScoreLabel);
            this.DetailsPanel.Controls.Add(this.ScoreLabel);
            this.DetailsPanel.Controls.Add(this.TeesLabel);
            this.DetailsPanel.Controls.Add(this.TournamentLabel);
            this.DetailsPanel.Controls.Add(this.DateLabel);
            this.DetailsPanel.Controls.Add(this.LocalHandicapLabel);
            this.DetailsPanel.Controls.Add(this.SCGAIndexLabel);
            this.DetailsPanel.Controls.Add(this.SCGANumberLabel);
            this.DetailsPanel.Controls.Add(this.PlayerNameLabel);
            this.DetailsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DetailsPanel.Location = new System.Drawing.Point(0, 0);
            this.DetailsPanel.Margin = new System.Windows.Forms.Padding(2);
            this.DetailsPanel.Name = "DetailsPanel";
            this.DetailsPanel.Size = new System.Drawing.Size(502, 252);
            this.DetailsPanel.TabIndex = 2;
            // 
            // DifferentialLabel
            // 
            this.DifferentialLabel.AutoSize = true;
            this.DifferentialLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.DifferentialLabel.Location = new System.Drawing.Point(428, 57);
            this.DifferentialLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.DifferentialLabel.Name = "DifferentialLabel";
            this.DifferentialLabel.Size = new System.Drawing.Size(27, 13);
            this.DifferentialLabel.TabIndex = 8;
            this.DifferentialLabel.Text = "Diff";
            // 
            // AdjustedScoreLabel
            // 
            this.AdjustedScoreLabel.AutoSize = true;
            this.AdjustedScoreLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.AdjustedScoreLabel.Location = new System.Drawing.Point(375, 57);
            this.AdjustedScoreLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.AdjustedScoreLabel.Name = "AdjustedScoreLabel";
            this.AdjustedScoreLabel.Size = new System.Drawing.Size(25, 13);
            this.AdjustedScoreLabel.TabIndex = 7;
            this.AdjustedScoreLabel.Text = "Adj";
            // 
            // ScoreLabel
            // 
            this.ScoreLabel.AutoSize = true;
            this.ScoreLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ScoreLabel.Location = new System.Drawing.Point(322, 57);
            this.ScoreLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.ScoreLabel.Name = "ScoreLabel";
            this.ScoreLabel.Size = new System.Drawing.Size(40, 13);
            this.ScoreLabel.TabIndex = 6;
            this.ScoreLabel.Text = "Score";
            // 
            // TeesLabel
            // 
            this.TeesLabel.AutoSize = true;
            this.TeesLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TeesLabel.Location = new System.Drawing.Point(270, 57);
            this.TeesLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.TeesLabel.Name = "TeesLabel";
            this.TeesLabel.Size = new System.Drawing.Size(35, 13);
            this.TeesLabel.TabIndex = 5;
            this.TeesLabel.Text = "Tees";
            // 
            // TournamentLabel
            // 
            this.TournamentLabel.AutoSize = true;
            this.TournamentLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TournamentLabel.Location = new System.Drawing.Point(72, 57);
            this.TournamentLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.TournamentLabel.Name = "TournamentLabel";
            this.TournamentLabel.Size = new System.Drawing.Size(74, 13);
            this.TournamentLabel.TabIndex = 4;
            this.TournamentLabel.Text = "Tournament";
            // 
            // DateLabel
            // 
            this.DateLabel.AutoSize = true;
            this.DateLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.DateLabel.Location = new System.Drawing.Point(8, 57);
            this.DateLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.DateLabel.Name = "DateLabel";
            this.DateLabel.Size = new System.Drawing.Size(34, 13);
            this.DateLabel.TabIndex = 3;
            this.DateLabel.Text = "Date";
            // 
            // LocalHandicapLabel
            // 
            this.LocalHandicapLabel.AutoSize = true;
            this.LocalHandicapLabel.Location = new System.Drawing.Point(345, 24);
            this.LocalHandicapLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.LocalHandicapLabel.Name = "LocalHandicapLabel";
            this.LocalHandicapLabel.Size = new System.Drawing.Size(82, 13);
            this.LocalHandicapLabel.TabIndex = 2;
            this.LocalHandicapLabel.Text = "Local Handicap";
            // 
            // SCGAIndexLabel
            // 
            this.SCGAIndexLabel.AutoSize = true;
            this.SCGAIndexLabel.Location = new System.Drawing.Point(345, 8);
            this.SCGAIndexLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.SCGAIndexLabel.Name = "SCGAIndexLabel";
            this.SCGAIndexLabel.Size = new System.Drawing.Size(65, 13);
            this.SCGAIndexLabel.TabIndex = 2;
            this.SCGAIndexLabel.Text = "SCGA Index";
            // 
            // SCGANumberLabel
            // 
            this.SCGANumberLabel.AutoSize = true;
            this.SCGANumberLabel.Location = new System.Drawing.Point(8, 24);
            this.SCGANumberLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.SCGANumberLabel.Name = "SCGANumberLabel";
            this.SCGANumberLabel.Size = new System.Drawing.Size(44, 13);
            this.SCGANumberLabel.TabIndex = 1;
            this.SCGANumberLabel.Text = "Number";
            // 
            // PlayerNameLabel
            // 
            this.PlayerNameLabel.AutoSize = true;
            this.PlayerNameLabel.Location = new System.Drawing.Point(8, 8);
            this.PlayerNameLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.PlayerNameLabel.Name = "PlayerNameLabel";
            this.PlayerNameLabel.Size = new System.Drawing.Size(35, 13);
            this.PlayerNameLabel.TabIndex = 0;
            this.PlayerNameLabel.Text = "Name";
            // 
            // SearchPanel
            // 
            this.SearchPanel.Controls.Add(this.IndividualSearchTextBox);
            this.SearchPanel.Controls.Add(this.textBox1);
            this.SearchPanel.Controls.Add(this.label1);
            this.SearchPanel.Controls.Add(this.LastNameLabel);
            this.SearchPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.SearchPanel.Location = new System.Drawing.Point(0, 252);
            this.SearchPanel.Margin = new System.Windows.Forms.Padding(2);
            this.SearchPanel.Name = "SearchPanel";
            this.SearchPanel.Size = new System.Drawing.Size(502, 23);
            this.SearchPanel.TabIndex = 1;
            // 
            // IndividualSearchTextBox
            // 
            this.IndividualSearchTextBox.Location = new System.Drawing.Point(118, 2);
            this.IndividualSearchTextBox.Margin = new System.Windows.Forms.Padding(2);
            this.IndividualSearchTextBox.Name = "IndividualSearchTextBox";
            this.IndividualSearchTextBox.Size = new System.Drawing.Size(188, 20);
            this.IndividualSearchTextBox.TabIndex = 1;
            this.IndividualSearchTextBox.TextChanged += new System.EventHandler(this.IndividualSearchTextBox_TextChanged);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(118, 2);
            this.textBox1.Margin = new System.Windows.Forms.Padding(2);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(188, 20);
            this.textBox1.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 4);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(113, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Search for Last Name:";
            // 
            // LastNameLabel
            // 
            this.LastNameLabel.AutoSize = true;
            this.LastNameLabel.Location = new System.Drawing.Point(9, 6);
            this.LastNameLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.LastNameLabel.Name = "LastNameLabel";
            this.LastNameLabel.Size = new System.Drawing.Size(61, 13);
            this.LastNameLabel.TabIndex = 0;
            this.LastNameLabel.Text = "Last Name:";
            // 
            // IndividualDetailsScrollBar
            // 
            this.IndividualDetailsScrollBar.Dock = System.Windows.Forms.DockStyle.Right;
            this.IndividualDetailsScrollBar.LargeChange = 5;
            this.IndividualDetailsScrollBar.Location = new System.Drawing.Point(502, 0);
            this.IndividualDetailsScrollBar.Name = "IndividualDetailsScrollBar";
            this.IndividualDetailsScrollBar.Size = new System.Drawing.Size(21, 275);
            this.IndividualDetailsScrollBar.TabIndex = 0;
            this.IndividualDetailsScrollBar.ValueChanged += new System.EventHandler(this.IndividualDetailsScrollBar_ValueChanged);
            // 
            // SummaryTab
            // 
            this.SummaryTab.Controls.Add(this.HandicapDetailsListView);
            this.SummaryTab.Location = new System.Drawing.Point(4, 22);
            this.SummaryTab.Margin = new System.Windows.Forms.Padding(2);
            this.SummaryTab.Name = "SummaryTab";
            this.SummaryTab.Padding = new System.Windows.Forms.Padding(2);
            this.SummaryTab.Size = new System.Drawing.Size(523, 275);
            this.SummaryTab.TabIndex = 1;
            this.SummaryTab.Text = "Summary";
            this.SummaryTab.UseVisualStyleBackColor = true;
            // 
            // HandicapDetailsListView
            // 
            this.HandicapDetailsListView.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.HandicapDetailsListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.HandicapDetailsListView.FullRowSelect = true;
            this.HandicapDetailsListView.Location = new System.Drawing.Point(2, 2);
            this.HandicapDetailsListView.Margin = new System.Windows.Forms.Padding(2);
            this.HandicapDetailsListView.MultiSelect = false;
            this.HandicapDetailsListView.Name = "HandicapDetailsListView";
            this.HandicapDetailsListView.Size = new System.Drawing.Size(519, 271);
            this.HandicapDetailsListView.TabIndex = 0;
            this.HandicapDetailsListView.UseCompatibleStateImageBehavior = false;
            this.HandicapDetailsListView.View = System.Windows.Forms.View.Details;
            // 
            // LocalHandicapLowerCheckBox
            // 
            this.LocalHandicapLowerCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.LocalHandicapLowerCheckBox.AutoSize = true;
            this.LocalHandicapLowerCheckBox.Location = new System.Drawing.Point(14, 339);
            this.LocalHandicapLowerCheckBox.Margin = new System.Windows.Forms.Padding(2);
            this.LocalHandicapLowerCheckBox.Name = "LocalHandicapLowerCheckBox";
            this.LocalHandicapLowerCheckBox.Size = new System.Drawing.Size(315, 17);
            this.LocalHandicapLowerCheckBox.TabIndex = 3;
            this.LocalHandicapLowerCheckBox.Text = "Show only if player\'s local handicap is lower than SCGA index";
            this.LocalHandicapLowerCheckBox.UseVisualStyleBackColor = true;
            this.LocalHandicapLowerCheckBox.CheckedChanged += new System.EventHandler(this.LocalHandicapCheckBox_CheckedChanged);
            // 
            // SaveAsHTMLButton
            // 
            this.SaveAsHTMLButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.SaveAsHTMLButton.Location = new System.Drawing.Point(338, 381);
            this.SaveAsHTMLButton.Margin = new System.Windows.Forms.Padding(2);
            this.SaveAsHTMLButton.Name = "SaveAsHTMLButton";
            this.SaveAsHTMLButton.Size = new System.Drawing.Size(96, 24);
            this.SaveAsHTMLButton.TabIndex = 1;
            this.SaveAsHTMLButton.Text = "Save as HTML";
            this.SaveAsHTMLButton.UseVisualStyleBackColor = true;
            this.SaveAsHTMLButton.Click += new System.EventHandler(this.SaveAsHTMLButton_Click);
            // 
            // SaveAsCSVButton
            // 
            this.SaveAsCSVButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.SaveAsCSVButton.Location = new System.Drawing.Point(14, 381);
            this.SaveAsCSVButton.Margin = new System.Windows.Forms.Padding(2);
            this.SaveAsCSVButton.Name = "SaveAsCSVButton";
            this.SaveAsCSVButton.Size = new System.Drawing.Size(96, 24);
            this.SaveAsCSVButton.TabIndex = 1;
            this.SaveAsCSVButton.Text = "Save as CSV";
            this.SaveAsCSVButton.UseVisualStyleBackColor = true;
            this.SaveAsCSVButton.Click += new System.EventHandler(this.SaveAsCSVButton_Click);
            // 
            // SaveAsTextButton
            // 
            this.SaveAsTextButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.SaveAsTextButton.Location = new System.Drawing.Point(230, 381);
            this.SaveAsTextButton.Margin = new System.Windows.Forms.Padding(2);
            this.SaveAsTextButton.Name = "SaveAsTextButton";
            this.SaveAsTextButton.Size = new System.Drawing.Size(96, 24);
            this.SaveAsTextButton.TabIndex = 1;
            this.SaveAsTextButton.Text = "Save as Text";
            this.SaveAsTextButton.UseVisualStyleBackColor = true;
            this.SaveAsTextButton.Click += new System.EventHandler(this.SaveAsTextButton_Click);
            // 
            // LocalHandicapExistsCheckbox
            // 
            this.LocalHandicapExistsCheckbox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.LocalHandicapExistsCheckbox.AutoSize = true;
            this.LocalHandicapExistsCheckbox.Location = new System.Drawing.Point(14, 320);
            this.LocalHandicapExistsCheckbox.Margin = new System.Windows.Forms.Padding(2);
            this.LocalHandicapExistsCheckbox.Name = "LocalHandicapExistsCheckbox";
            this.LocalHandicapExistsCheckbox.Size = new System.Drawing.Size(215, 17);
            this.LocalHandicapExistsCheckbox.TabIndex = 3;
            this.LocalHandicapExistsCheckbox.Text = "Show only if player has a local handicap";
            this.LocalHandicapExistsCheckbox.UseVisualStyleBackColor = true;
            this.LocalHandicapExistsCheckbox.CheckedChanged += new System.EventHandler(this.LocalHandicapExistsCheckbox_CheckedChanged);
            // 
            // SaveAsTSVButton
            // 
            this.SaveAsTSVButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.SaveAsTSVButton.Location = new System.Drawing.Point(122, 381);
            this.SaveAsTSVButton.Margin = new System.Windows.Forms.Padding(2);
            this.SaveAsTSVButton.Name = "SaveAsTSVButton";
            this.SaveAsTSVButton.Size = new System.Drawing.Size(96, 24);
            this.SaveAsTSVButton.TabIndex = 1;
            this.SaveAsTSVButton.Text = "Save as Tab Sep";
            this.SaveAsTSVButton.UseVisualStyleBackColor = true;
            this.SaveAsTSVButton.Click += new System.EventHandler(this.SaveAsTSVButton_Click);
            // 
            // ExcludePlayersButton
            // 
            this.ExcludePlayersButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.ExcludePlayersButton.Location = new System.Drawing.Point(446, 352);
            this.ExcludePlayersButton.Margin = new System.Windows.Forms.Padding(2);
            this.ExcludePlayersButton.Name = "ExcludePlayersButton";
            this.ExcludePlayersButton.Size = new System.Drawing.Size(96, 24);
            this.ExcludePlayersButton.TabIndex = 1;
            this.ExcludePlayersButton.Text = "Exclude Players";
            this.ExcludePlayersButton.UseVisualStyleBackColor = true;
            this.ExcludePlayersButton.Click += new System.EventHandler(this.ExcludePlayersButton_Click);
            // 
            // UpperCaseCheckBox
            // 
            this.UpperCaseCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.UpperCaseCheckBox.AutoSize = true;
            this.UpperCaseCheckBox.Location = new System.Drawing.Point(14, 358);
            this.UpperCaseCheckBox.Name = "UpperCaseCheckBox";
            this.UpperCaseCheckBox.Size = new System.Drawing.Size(178, 17);
            this.UpperCaseCheckBox.TabIndex = 4;
            this.UpperCaseCheckBox.Text = "Upper case names when saving";
            this.UpperCaseCheckBox.UseVisualStyleBackColor = true;
            // 
            // ResultsUserControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(176)))), ((int)(((byte)(0)))));
            this.Controls.Add(this.UpperCaseCheckBox);
            this.Controls.Add(this.LocalHandicapExistsCheckbox);
            this.Controls.Add(this.LocalHandicapLowerCheckBox);
            this.Controls.Add(this.HandicapTab);
            this.Controls.Add(this.SaveAsTextButton);
            this.Controls.Add(this.SaveAsTSVButton);
            this.Controls.Add(this.SaveAsCSVButton);
            this.Controls.Add(this.SaveAsHTMLButton);
            this.Controls.Add(this.ExcludePlayersButton);
            this.Controls.Add(this.DoneButton);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MinimumSize = new System.Drawing.Size(555, 422);
            this.Name = "ResultsUserControl";
            this.Size = new System.Drawing.Size(555, 422);
            this.Resize += new System.EventHandler(this.ResultsUserControl_Resize);
            this.HandicapTab.ResumeLayout(false);
            this.LowHandicapTab.ResumeLayout(false);
            this.DetailsTab.ResumeLayout(false);
            this.DetailsPanel.ResumeLayout(false);
            this.DetailsPanel.PerformLayout();
            this.SearchPanel.ResumeLayout(false);
            this.SearchPanel.PerformLayout();
            this.SummaryTab.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView ResultsListView;
        private System.Windows.Forms.ColumnHeader NameColumn;
        private System.Windows.Forms.ColumnHeader SCGANumberColumn;
        private System.Windows.Forms.ColumnHeader HandicapColumn;
        private System.Windows.Forms.ColumnHeader SCGAIndexColumn;
        private System.Windows.Forms.ColumnHeader LocalHandicapColumn;
        private System.Windows.Forms.Button DoneButton;
        private System.Windows.Forms.TabControl HandicapTab;
        private System.Windows.Forms.TabPage LowHandicapTab;
        private System.Windows.Forms.TabPage SummaryTab;
        private System.Windows.Forms.ListView HandicapDetailsListView;
        private System.Windows.Forms.CheckBox LocalHandicapLowerCheckBox;
        private System.Windows.Forms.Button SaveAsHTMLButton;
        private System.Windows.Forms.Button SaveAsCSVButton;
        private System.Windows.Forms.Button SaveAsTextButton;
        private System.Windows.Forms.CheckBox LocalHandicapExistsCheckbox;
        private System.Windows.Forms.TabPage DetailsTab;
        private System.Windows.Forms.Panel DetailsPanel;
        private System.Windows.Forms.Panel SearchPanel;
        private System.Windows.Forms.Label LastNameLabel;
        private System.Windows.Forms.VScrollBar IndividualDetailsScrollBar;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.TextBox IndividualSearchTextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label PlayerNameLabel;
        private System.Windows.Forms.Label LocalHandicapLabel;
        private System.Windows.Forms.Label SCGAIndexLabel;
        private System.Windows.Forms.Label SCGANumberLabel;
        public System.Windows.Forms.Label ScoreLabel;
        public System.Windows.Forms.Label TeesLabel;
        public System.Windows.Forms.Label TournamentLabel;
        public System.Windows.Forms.Label DateLabel;
        public System.Windows.Forms.Label AdjustedScoreLabel;
        public System.Windows.Forms.Label DifferentialLabel;
        private System.Windows.Forms.Button SaveAsTSVButton;
        private System.Windows.Forms.Button ExcludePlayersButton;
        private System.Windows.Forms.CheckBox UpperCaseCheckBox;
    }
}
