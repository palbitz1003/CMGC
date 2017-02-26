using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace LocalHandicap
{
    public partial class IncludeExclude : Form
    {
        SortedDictionary<string, PlayerData> _localHandicapDBByName;
        private static string ExcludedFileName = "Excluded.csv";
        private int _previousCount;

        public static void ExcludedFileFolder(string folder)
        {
            ExcludedFileName = Path.Combine(folder, Path.GetFileName(ExcludedFileName));
        }

        public IncludeExclude(SortedDictionary<string, PlayerData> localHandicapDBByName)
        {
            InitializeComponent();

            _localHandicapDBByName = localHandicapDBByName;

            CreateExcludedList();
        }

        /// <summary>
        /// Create the list view items to display
        /// </summary>
        private void CreateExcludedList()
        {
            int excludedCount = 0;
            foreach (KeyValuePair<string, PlayerData> entry in _localHandicapDBByName)
            {
                ListViewItem lvi = new ListViewItem(entry.Value.Name);
                lvi.Tag = entry.Value;

                if (entry.Value.Excluded)
                {
                    excludedCount++;
                    lvi.Checked = true;
                }
                IncludeExcludeListView.Items.Add(lvi);
            }

            _previousCount = excludedCount;
            if (excludedCount > 0)
            {
                IncludeExcludeLlabel.Text = excludedCount + " players excluded";
            }
            else
            {
                IncludeExcludeLlabel.Text = "Select players to exclude";
            }
        }

        private void DoneButton_Click(object sender, EventArgs e)
        {
            // Clear all the player data records
            foreach (KeyValuePair<string, PlayerData> entry in _localHandicapDBByName)
            {
                entry.Value.Excluded = false;
            }

            // Take the excluded list and apply it to the player data
            ListView.CheckedListViewItemCollection selectedItems = IncludeExcludeListView.CheckedItems;
            foreach (ListViewItem lvi in selectedItems)
            {
                PlayerData playerData = (PlayerData)lvi.Tag;
                playerData.Excluded = true;
            }

            // Save the list to a file
            WriteExcludedList(_localHandicapDBByName);

            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }

        /// <summary>
        /// Save the excluded players to a file
        /// </summary>
        /// <param name="localHandicapDBByName"></param>
        public static void WriteExcludedList(SortedDictionary<string, PlayerData> localHandicapDBByName)
        {
            try
            {
                using (TextWriter tw = new StreamWriter(ExcludedFileName))
                {
                    foreach (KeyValuePair<string, PlayerData> entry in localHandicapDBByName)
                    {
                        if (entry.Value.Excluded)
                        {
                            tw.WriteLine(entry.Value.GHINNumber + "," + entry.Value.Name.Replace(", ", ","));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error writing " + ExcludedFileName + ":" + ex.Message);
            }
        }

        /// <summary>
        /// Read the excluded list and apply the exclusions to the player data
        /// </summary>
        /// <param name="localHandicapDBByNumber"></param>
        /// <param name="localHandicapDBByName"></param>
        public static void ReadExcludedList(SortedDictionary<string, PlayerData> localHandicapDBByNumber,
            SortedDictionary<string, PlayerData> localHandicapDBByName)
        {
            if (!File.Exists(ExcludedFileName))
            {
                return;
            }

            try
            {
                bool needToWrite = false;

                using (TextReader tr = new StreamReader(ExcludedFileName))
                {
                    string line;
                    while ((line = tr.ReadLine()) != null)
                    {
                        string[] fields = line.Split(',');

                        if (localHandicapDBByNumber.ContainsKey(fields[0]))
                        {
                            localHandicapDBByNumber[fields[0]].Excluded = true;
                        }
                        else
                        {
                            // If some people were in the excluded list and there is
                            // no data for them, write out the excluded list again,
                            // which will drop them from the list.
                            //
                            // Disable his feature to avoid clearing the list if the
                            // player list is temporarily empty.
                            //needToWrite = true;
                        }
                    }
                }

                if (needToWrite)
                {
                    WriteExcludedList(localHandicapDBByName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error reading " + ExcludedFileName + ":" + ex.Message);
            }
        }

        private void IncludeExcludeListView_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            if (_previousCount == IncludeExcludeListView.CheckedItems.Count)
            {
                return;
            }

            _previousCount = IncludeExcludeListView.CheckedItems.Count;
            if (_previousCount > 0)
            {
                IncludeExcludeLlabel.Text = _previousCount + " players excluded";
            }
            else
            {
                IncludeExcludeLlabel.Text = "Select players to exclude";
            }
        }

        private void IncludeExclude_Activated(object sender, EventArgs e)
        {
            this.IncludeExcludeListView.ItemChecked -= new System.Windows.Forms.ItemCheckedEventHandler(this.IncludeExcludeListView_ItemChecked);

            this.IncludeExcludeListView.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.IncludeExcludeListView_ItemChecked);
        }
    }
}