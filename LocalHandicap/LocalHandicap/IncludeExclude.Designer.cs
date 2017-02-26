namespace LocalHandicap
{
    partial class IncludeExclude
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
            this.IncludeExcludeListView = new System.Windows.Forms.ListView();
            this.playerColumnHeader = new System.Windows.Forms.ColumnHeader();
            this.DoneButton = new System.Windows.Forms.Button();
            this.IncludeExcludeLlabel = new System.Windows.Forms.Label();
            this.CancelButton2 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // IncludeExcludeListView
            // 
            this.IncludeExcludeListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.IncludeExcludeListView.CheckBoxes = true;
            this.IncludeExcludeListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.playerColumnHeader});
            this.IncludeExcludeListView.Location = new System.Drawing.Point(14, 40);
            this.IncludeExcludeListView.Name = "IncludeExcludeListView";
            this.IncludeExcludeListView.Size = new System.Drawing.Size(284, 358);
            this.IncludeExcludeListView.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.IncludeExcludeListView.TabIndex = 0;
            this.IncludeExcludeListView.UseCompatibleStateImageBehavior = false;
            this.IncludeExcludeListView.View = System.Windows.Forms.View.Details;
            // 
            // playerColumnHeader
            // 
            this.playerColumnHeader.Text = "Player";
            this.playerColumnHeader.Width = 250;
            // 
            // DoneButton
            // 
            this.DoneButton.Location = new System.Drawing.Point(160, 416);
            this.DoneButton.Name = "DoneButton";
            this.DoneButton.Size = new System.Drawing.Size(94, 26);
            this.DoneButton.TabIndex = 1;
            this.DoneButton.Text = "Done";
            this.DoneButton.UseVisualStyleBackColor = true;
            this.DoneButton.Click += new System.EventHandler(this.DoneButton_Click);
            // 
            // IncludeExcludeLlabel
            // 
            this.IncludeExcludeLlabel.AutoSize = true;
            this.IncludeExcludeLlabel.Location = new System.Drawing.Point(11, 10);
            this.IncludeExcludeLlabel.Name = "IncludeExcludeLlabel";
            this.IncludeExcludeLlabel.Size = new System.Drawing.Size(164, 17);
            this.IncludeExcludeLlabel.TabIndex = 2;
            this.IncludeExcludeLlabel.Text = "Exclude checked players";
            // 
            // CancelButton2
            // 
            this.CancelButton2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.CancelButton2.Location = new System.Drawing.Point(40, 416);
            this.CancelButton2.Name = "CancelButton2";
            this.CancelButton2.Size = new System.Drawing.Size(94, 26);
            this.CancelButton2.TabIndex = 1;
            this.CancelButton2.Text = "Cancel";
            this.CancelButton2.UseVisualStyleBackColor = true;
            this.CancelButton2.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // IncludeExclude
            // 
            this.AcceptButton = this.DoneButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(176)))), ((int)(((byte)(0)))));
            this.CancelButton = this.CancelButton2;
            this.ClientSize = new System.Drawing.Size(310, 454);
            this.Controls.Add(this.IncludeExcludeLlabel);
            this.Controls.Add(this.CancelButton2);
            this.Controls.Add(this.DoneButton);
            this.Controls.Add(this.IncludeExcludeListView);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "IncludeExclude";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Exclude";
            this.Activated += new System.EventHandler(this.IncludeExclude_Activated);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView IncludeExcludeListView;
        private System.Windows.Forms.Button DoneButton;
        private System.Windows.Forms.Label IncludeExcludeLlabel;
        private System.Windows.Forms.Button CancelButton2;
        private System.Windows.Forms.ColumnHeader playerColumnHeader;
    }
}