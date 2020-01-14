using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.IO;



namespace LocalHandicap
{
    class GHINIndex
    {
        int errorCount;
        private SortedDictionary<string, Index> _GHINIndexDBByNumber = new SortedDictionary<string, Index>();
        private List<Index> _GHINIndexDBByName = new List<Index>();

        public List<Index> DBByName { get { return _GHINIndexDBByName; } }

        public GHINIndex()
        {
        }

        public class Index
        {
            public string GHINNumber;
            public string Name;
            public float? CurrentIndex;
            //public float? LowIndex;
        }

        public void reset()
        {
            _GHINIndexDBByName.Clear();
            _GHINIndexDBByNumber.Clear();
        }

        public bool ExcelFile(string filename)
        {
            ReadExcelFile readExcelFile = new ReadExcelFile(filename);
            if ((readExcelFile.ExcelContents == null) && (readExcelFile.ExcelContents.Count == 0))
            {
                return false;
            }

            if (String.Compare(Path.GetExtension(filename), ".xls", true) != 0)
            {
                MessageBox.Show("Expected .xls file, but file does not end in .xls: " + filename);
                return false;
            }

            string csvFile = filename.Replace(".xls", ".csv");

            if (File.Exists(csvFile))
            {
                File.Delete(csvFile);
            }

            using (TextWriter tw = new StreamWriter(csvFile))
            {
                foreach (string s in readExcelFile.ExcelContents)
                {
                    tw.WriteLine(s);
                }
            }

            for (int i = 1; i <= readExcelFile.ExcelContents.Count; i++)
            {
                addCSVLine(readExcelFile.ExcelContents[i - 1], 1);
            }

            return true;
        }

        public bool CSVFile(string fileName)
        {
            try
            {
                errorCount = 0;
                if ((fileName == null) || (fileName.Trim().Length == 0))
                {
                    MessageBox.Show("Click the Browse button to fill in the file path");
                    return false;
                }

                if (!File.Exists(fileName))
                {
                    MessageBox.Show("File does not exist: " + fileName);
                    return false;
                }

                if (String.Compare(Path.GetExtension(fileName), ".xls", true) == 0)
                {
                    return ExcelFile(fileName);
                }

                bool textFile = String.Compare(Path.GetExtension(fileName), ".txt", true) == 0;

                int lineNumber = 0;
                using (TextReader streamReader = new StreamReader(fileName))
                {
                    Cursor oldCursor = Cursor.Current;
                    Cursor.Current = Cursors.WaitCursor;

                    string line;
                    while ((line = streamReader.ReadLine()) != null)
                    {
                        if (textFile)
                        {
                            addTextLine(line, lineNumber + 1);
                        }
                        else
                        {
                            addCSVLine(line, lineNumber + 1);
                        }
                        lineNumber++;
                    }
                    Cursor.Current = oldCursor;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
            return true;
        }

        public void addTextLine(string line, int lineCount)
        {
            string[] fields = line.Split('|');
            string error = string.Empty;

            if ((fields.Length >= 4) && (string.Compare(fields[0], "ghin #", true) != 0))
            {
                Index index = new Index();
                index.GHINNumber = fields[0];
                index.Name = fields[1];
                if (string.Compare(fields[2].Trim(), "nh", true) == 0)
                {
                    index.CurrentIndex = null;
                }
                else
                {
                    try
                    {
                        fields[2] = fields[2].Replace("R", "");  // some handicaps are of the form: 11.8R
                        index.CurrentIndex = float.Parse(fields[2]);
                    }
                    catch
                    {
                        error = "Line " + lineCount + " :invalid handicap " + fields[2]; 
                    }
                }

                // Add leading zeroes if they are not included
                while (index.GHINNumber.Length < 7)
                {
                    index.GHINNumber = "0" + index.GHINNumber;
                }

                if (error == string.Empty)
                {
                    addIndex(index);
                }
            }

            if (error != string.Empty)
            {
                errorCount++;

                if (errorCount <= 5)
                {
                    MessageBox.Show(error);
                }

                if (errorCount == 5)
                {
                    MessageBox.Show("5 errors reported.  Any more errors will be quietly ignored.");
                }
            }
        }

        public void addCSVLine(string line, int lineCount)
        {
            string[] fields = line.Split(',');
            for (int i = 0; i < fields.Length; i++)
            {
                fields[i] = fields[i].Replace("\"", "");
            }
            string error = string.Empty;

            if (fields.Length == 3)
            {
                string[] newFields = new string[4];
                newFields[0] = fields[0];
                newFields[1] = string.Empty;
                newFields[2] = fields[1];
                newFields[3] = fields[2];
                fields = newFields;
            }

            if (fields.Length == 4)
            {
                Index index = new Index();
                index.GHINNumber = fields[2];

                index.Name = fields[0].Trim() + ", " + fields[1].Trim();

                if (string.Compare(fields[3].Trim(), "nh", true) == 0)
                {
                    index.CurrentIndex = null;
                }
                else
                {
                    try
                    {
                        // The handicap may be followed by characters.  Here is the character list:
                        //
                        // L = Local handicap 
                        // M = Handicap modified by the Handicap Committee
                        // N = Nine-hole Handicap Index
                        // NL = Local nine-hole handicap
                        // R = Handicap automatically reduced for exceptional tournament performance
                        // SL = Short Course Handicap
                        // WD = Handicap withdrawn by the Handicap Committee
                        //
                        // Allow those ending in R, L, or M

                        fields[3] = fields[3].Replace("R", "");  
                        fields[3] = fields[3].Replace("L", "");  
                        fields[3] = fields[3].Replace("M", "");  
                        index.CurrentIndex = float.Parse(fields[3]);
                        if(fields[3].StartsWith("+"))
                        {
                            index.CurrentIndex = -index.CurrentIndex;
                        }
                    }
                    catch
                    {
                        error = "Line " + lineCount + " :invalid handicap " + fields[3];
                    }
                }

                // Add leading zeroes if they are not included
                while (index.GHINNumber.Length < 7)
                {
                    index.GHINNumber = "0" + index.GHINNumber;
                }

                if (error == string.Empty)
                {
                    addIndex(index);
                }
            }

            if (error != string.Empty)
            {
                errorCount++;

                if (errorCount <= 5)
                {
                    MessageBox.Show(error);
                }

                if (errorCount == 5)
                {
                    MessageBox.Show("5 errors reported.  Any more errors will be quietly ignored.");
                }
            }
        }

        private void addIndex(Index index)
        {
            if (_GHINIndexDBByNumber.ContainsKey(index.GHINNumber))
            {
                // duplicate
            }
            else
            {
                _GHINIndexDBByNumber.Add(index.GHINNumber, index);
                _GHINIndexDBByName.Add(index);
            }
        }
    }
}
