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
    public partial class DeleteTournaments : Form
    {
        private string _dbFolder;
        private string _lastTournamentFileName;
        private string _lastTournamentDate;

        public DeleteTournaments(string dbfolder)
        {
            InitializeComponent();
            _dbFolder = dbfolder;
            InitFiles();
        }

        private void InitFiles()
        {
            FileListView.Items.Clear();
            if(Directory.Exists(_dbFolder))
            {
                string[] files = Directory.GetFiles(_dbFolder, "*.vpt");

                foreach (string file in files)
                {
                    TournamentData td = new TournamentData();
                    string tdFile = Path.ChangeExtension(file, ".data");
                    td.LoadTournamentData(tdFile);

                    ListViewItem lvi = new ListViewItem(getDate(file));
                    lvi.Tag = file;

                    string tournamentName = td.Name == null ? Path.GetFileNameWithoutExtension(file) : td.Name;
                    ListViewItem.ListViewSubItem lvsi = new ListViewItem.ListViewSubItem(lvi, tournamentName);
                    lvi.SubItems.Add(lvsi);
                    FileListView.Items.Add(lvi);
                }
            }

            if (_lastTournamentDate == null)
            {
                _lastTournamentDate = "no tournaments in database";
            }
            if (_lastTournamentFileName == null)
            {
                _lastTournamentFileName = string.Empty;
            }
        }

        private string getDate(string file)
        {
            try
            {
                using (FileStream fs = new FileStream(file, FileMode.Open))
                {
                    TextReader tr = new StreamReader(fs);
                    string line = tr.ReadLine();
                    line = tr.ReadLine();
                    string[] fields = line.Split(',');

                    if (fields.Length < 11)
                    {
                        return "?";
                    }

                    // remove the # on both sides
                    string vpDate = fields[10].Replace("#", "");
                    vpDate = vpDate.Replace("\"", "");

                    if (string.Compare(vpDate, _lastTournamentDate) > 0)
                    {
                        _lastTournamentDate = vpDate;
                        _lastTournamentFileName = file;
                    }

                    return vpDate;
                }
            }
            catch
            {
                return "?";
            }
        }

        private void DoneButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void DeleteButton_Click(object sender, EventArgs e)
        {
            ListView.CheckedListViewItemCollection selectedItems = FileListView.CheckedItems;
            foreach (ListViewItem lvi in selectedItems)
            {
                string file = (string)lvi.Tag;
                    //Path.ChangeExtension(Path.Combine(_dbFolder, lvi.SubItems[1].Text), ".vpt");
                if (File.Exists(file))
                {
                    File.Delete(file);
                    string dataFile = Path.ChangeExtension(file, ".data");
                    if(File.Exists(dataFile))
                    {
                        File.Delete(dataFile);
                    }
                }
                else
                {
                    MessageBox.Show("File not found: " + file);
                }
            }
            InitFiles();
        }

        public string LastTournamentDate
        {
            get 
            {
                string[] fields = _lastTournamentDate.Split('-');
                if (fields.Length == 3)
                {
                    try
                    {
                        DateTime dt = new DateTime(int.Parse(fields[0]), int.Parse(fields[1]), int.Parse(fields[2]));
                        return dt.ToShortDateString();
                    }
                    catch
                    {
                    }
                }

                return _lastTournamentDate; 
            }
        }

        public string LastTournamentFileName
        {
            get { return _lastTournamentFileName; }
        }
    }
}