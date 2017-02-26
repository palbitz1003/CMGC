namespace LocalHandicap
{
    partial class TournamentDataForm
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
            this.TournamentNameLabel = new System.Windows.Forms.Label();
            this.TournamentNameTextBox = new System.Windows.Forms.TextBox();
            this.DoneButton = new System.Windows.Forms.Button();
            this.TournamentFileLabel = new System.Windows.Forms.Label();
            this.TournamentFileValueLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // TournamentNameLabel
            // 
            this.TournamentNameLabel.AutoSize = true;
            this.TournamentNameLabel.Location = new System.Drawing.Point(16, 34);
            this.TournamentNameLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.TournamentNameLabel.Name = "TournamentNameLabel";
            this.TournamentNameLabel.Size = new System.Drawing.Size(98, 13);
            this.TournamentNameLabel.TabIndex = 0;
            this.TournamentNameLabel.Text = "Tournament Name:";
            // 
            // TournamentNameTextBox
            // 
            this.TournamentNameTextBox.Location = new System.Drawing.Point(118, 32);
            this.TournamentNameTextBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.TournamentNameTextBox.Name = "TournamentNameTextBox";
            this.TournamentNameTextBox.Size = new System.Drawing.Size(243, 20);
            this.TournamentNameTextBox.TabIndex = 2;
            // 
            // DoneButton
            // 
            this.DoneButton.Location = new System.Drawing.Point(146, 80);
            this.DoneButton.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.DoneButton.Name = "DoneButton";
            this.DoneButton.Size = new System.Drawing.Size(86, 24);
            this.DoneButton.TabIndex = 4;
            this.DoneButton.Text = "Done";
            this.DoneButton.UseVisualStyleBackColor = true;
            this.DoneButton.Click += new System.EventHandler(this.DoneButton_Click);
            // 
            // TournamentFileLabel
            // 
            this.TournamentFileLabel.AutoSize = true;
            this.TournamentFileLabel.Location = new System.Drawing.Point(16, 12);
            this.TournamentFileLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.TournamentFileLabel.Name = "TournamentFileLabel";
            this.TournamentFileLabel.Size = new System.Drawing.Size(86, 13);
            this.TournamentFileLabel.TabIndex = 5;
            this.TournamentFileLabel.Text = "Tournament File:";
            // 
            // TournamentFileValueLabel
            // 
            this.TournamentFileValueLabel.AutoSize = true;
            this.TournamentFileValueLabel.Location = new System.Drawing.Point(116, 12);
            this.TournamentFileValueLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.TournamentFileValueLabel.Name = "TournamentFileValueLabel";
            this.TournamentFileValueLabel.Size = new System.Drawing.Size(35, 13);
            this.TournamentFileValueLabel.TabIndex = 6;
            this.TournamentFileValueLabel.Text = "label1";
            // 
            // TournamentDataForm
            // 
            this.AcceptButton = this.DoneButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(176)))), ((int)(((byte)(0)))));
            this.ClientSize = new System.Drawing.Size(377, 131);
            this.Controls.Add(this.TournamentFileValueLabel);
            this.Controls.Add(this.TournamentFileLabel);
            this.Controls.Add(this.DoneButton);
            this.Controls.Add(this.TournamentNameTextBox);
            this.Controls.Add(this.TournamentNameLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "TournamentDataForm";
            this.ShowInTaskbar = false;
            this.Text = "Tournament Details";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label TournamentNameLabel;
        private System.Windows.Forms.Button DoneButton;
        public System.Windows.Forms.TextBox TournamentNameTextBox;
        private System.Windows.Forms.Label TournamentFileLabel;
        public System.Windows.Forms.Label TournamentFileValueLabel;
    }
}