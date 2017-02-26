namespace LocalHandicap
{
    partial class MonthQuestion
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
            this.RadioButton1st = new System.Windows.Forms.RadioButton();
            this.RadioButton15th = new System.Windows.Forms.RadioButton();
            this.DayGroupBox = new System.Windows.Forms.GroupBox();
            this.MonthGroupBox = new System.Windows.Forms.GroupBox();
            this.ThisMonthRadioButton = new System.Windows.Forms.RadioButton();
            this.NextMonthRadioButton = new System.Windows.Forms.RadioButton();
            this.DoneButton = new System.Windows.Forms.Button();
            this.DayGroupBox.SuspendLayout();
            this.MonthGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // RadioButton1st
            // 
            this.RadioButton1st.AutoSize = true;
            this.RadioButton1st.Location = new System.Drawing.Point(18, 19);
            this.RadioButton1st.Name = "RadioButton1st";
            this.RadioButton1st.Size = new System.Drawing.Size(39, 17);
            this.RadioButton1st.TabIndex = 3;
            this.RadioButton1st.TabStop = true;
            this.RadioButton1st.Text = "1st";
            this.RadioButton1st.UseVisualStyleBackColor = true;
            // 
            // RadioButton15th
            // 
            this.RadioButton15th.AutoSize = true;
            this.RadioButton15th.Location = new System.Drawing.Point(18, 42);
            this.RadioButton15th.Name = "RadioButton15th";
            this.RadioButton15th.Size = new System.Drawing.Size(46, 17);
            this.RadioButton15th.TabIndex = 4;
            this.RadioButton15th.TabStop = true;
            this.RadioButton15th.Text = "15th";
            this.RadioButton15th.UseVisualStyleBackColor = true;
            // 
            // DayGroupBox
            // 
            this.DayGroupBox.Controls.Add(this.RadioButton1st);
            this.DayGroupBox.Controls.Add(this.RadioButton15th);
            this.DayGroupBox.Location = new System.Drawing.Point(30, 96);
            this.DayGroupBox.Name = "DayGroupBox";
            this.DayGroupBox.Size = new System.Drawing.Size(145, 76);
            this.DayGroupBox.TabIndex = 5;
            this.DayGroupBox.TabStop = false;
            this.DayGroupBox.Text = "Day";
            // 
            // MonthGroupBox
            // 
            this.MonthGroupBox.Controls.Add(this.ThisMonthRadioButton);
            this.MonthGroupBox.Controls.Add(this.NextMonthRadioButton);
            this.MonthGroupBox.Location = new System.Drawing.Point(30, 14);
            this.MonthGroupBox.Name = "MonthGroupBox";
            this.MonthGroupBox.Size = new System.Drawing.Size(145, 76);
            this.MonthGroupBox.TabIndex = 6;
            this.MonthGroupBox.TabStop = false;
            this.MonthGroupBox.Text = "Month";
            // 
            // ThisMonthRadioButton
            // 
            this.ThisMonthRadioButton.AutoSize = true;
            this.ThisMonthRadioButton.Location = new System.Drawing.Point(18, 19);
            this.ThisMonthRadioButton.Name = "ThisMonthRadioButton";
            this.ThisMonthRadioButton.Size = new System.Drawing.Size(14, 13);
            this.ThisMonthRadioButton.TabIndex = 3;
            this.ThisMonthRadioButton.TabStop = true;
            this.ThisMonthRadioButton.UseVisualStyleBackColor = true;
            // 
            // NextMonthRadioButton
            // 
            this.NextMonthRadioButton.AutoSize = true;
            this.NextMonthRadioButton.Location = new System.Drawing.Point(18, 42);
            this.NextMonthRadioButton.Name = "NextMonthRadioButton";
            this.NextMonthRadioButton.Size = new System.Drawing.Size(14, 13);
            this.NextMonthRadioButton.TabIndex = 4;
            this.NextMonthRadioButton.TabStop = true;
            this.NextMonthRadioButton.UseVisualStyleBackColor = true;
            // 
            // DoneButton
            // 
            this.DoneButton.Location = new System.Drawing.Point(62, 189);
            this.DoneButton.Name = "DoneButton";
            this.DoneButton.Size = new System.Drawing.Size(75, 23);
            this.DoneButton.TabIndex = 7;
            this.DoneButton.Text = "Done";
            this.DoneButton.UseVisualStyleBackColor = true;
            this.DoneButton.Click += new System.EventHandler(this.DoneButton_Click);
            // 
            // MonthQuestion
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(176)))), ((int)(((byte)(0)))));
            this.ClientSize = new System.Drawing.Size(205, 226);
            this.Controls.Add(this.DoneButton);
            this.Controls.Add(this.MonthGroupBox);
            this.Controls.Add(this.DayGroupBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MonthQuestion";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Chose Date";
            this.DayGroupBox.ResumeLayout(false);
            this.DayGroupBox.PerformLayout();
            this.MonthGroupBox.ResumeLayout(false);
            this.MonthGroupBox.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RadioButton RadioButton1st;
        private System.Windows.Forms.RadioButton RadioButton15th;
        private System.Windows.Forms.GroupBox DayGroupBox;
        private System.Windows.Forms.GroupBox MonthGroupBox;
        private System.Windows.Forms.RadioButton ThisMonthRadioButton;
        private System.Windows.Forms.RadioButton NextMonthRadioButton;
        private System.Windows.Forms.Button DoneButton;
    }
}