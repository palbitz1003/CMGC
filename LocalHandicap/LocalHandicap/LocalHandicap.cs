using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace LocalHandicap
{
    class LocalHandicap
    {
        private Dictionary<string, int> _duplicateDetection = new Dictionary<string,int>();
        private SortedDictionary<string, PlayerData> _localHandicapDBByNumber = new SortedDictionary<string, PlayerData>();
        private SortedDictionary<string, PlayerData> _localHandicapDBByName = new SortedDictionary<string, PlayerData>();
        private List<DateTime> _dateList = new List<DateTime>();
        private SortScoresByDifferential _sortScoresByDifferential = new SortScoresByDifferential();
        private SortScoresByDate _sortScoresByDate = new SortScoresByDate();
        private int errors = 0;

        public LocalHandicap()
        {
        }

        public void reset()
        {
            _localHandicapDBByNumber.Clear();
            _localHandicapDBByName.Clear();
            _dateList.Clear();
        }

        private float truncateToTenths(float number)
        {
            float f = number * 10.0f;
            int x = (int)f;
            return (float)x / 10.0f;
        }

        private class SortScoresByDifferential : IComparer<Score>
        {
            public int Compare(Score score1, Score score2)
            {
                if (score1.Differential == score2.Differential)
                {
                    return 0;
                }
                if (score1.Differential > score2.Differential)
                {
                    return 1;
                }
                return -1;
            }
        }

        private class SortScoresByDate : IComparer<Score>
        {
            public int Compare(Score score1, Score score2)
            {
                if (score1.DT < score2.DT)
                {
                    return -1;
                }
                else if (score1.DT > score2.DT)
                {
                    return 1;
                }
                return 0;
            }
        }

        public void Calculate(int maxLocalScores)
        {
            errors = 0;
            foreach (KeyValuePair<string, PlayerData> keyValuePlayerData in _localHandicapDBByNumber)
            {
                keyValuePlayerData.Value.Scores.Sort(_sortScoresByDate);
                while (keyValuePlayerData.Value.Scores.Count > maxLocalScores)
                {
                    keyValuePlayerData.Value.Scores.RemoveAt(0);
                }
                keyValuePlayerData.Value.Scores.Sort(_sortScoresByDifferential);
                int bestRounds = (keyValuePlayerData.Value.Scores.Count + 1) / 2;
                float sum = 0.0f;
                for (int i = 0; i < keyValuePlayerData.Value.Scores.Count; i++)
                {
                    if (i < bestRounds)
                    {
                        sum += keyValuePlayerData.Value.Scores[i].Differential;
                        keyValuePlayerData.Value.Scores[i].UsedInLocalHandicap = true;
                    }
                    else
                    {
                        keyValuePlayerData.Value.Scores[i].UsedInLocalHandicap = false;
                    }
                }
                keyValuePlayerData.Value.LocalHandicap = truncateToTenths((sum / (float)bestRounds) * 0.96f);
                keyValuePlayerData.Value.Scores.Sort(_sortScoresByDate);
            }
        }

        public float? GetLocalHandicap(string SCGANumber)
        {
            if (_localHandicapDBByNumber.ContainsKey(SCGANumber))
            {
                return _localHandicapDBByNumber[SCGANumber].LocalHandicap;
            }
            return null;
        }

        public PlayerData GetPlayerData(string SCGANumber)
        {
            if (_localHandicapDBByNumber.ContainsKey(SCGANumber))
            {
                return _localHandicapDBByNumber[SCGANumber];
            }
            return null;
        }

        public void addScores(FileInfo fi, DateTime oldestDate)
        {
            try
            {
                int lineNumber = 0;
                _duplicateDetection.Clear();
                using (TextReader streamReader = new StreamReader(fi.FullName))
                {
                    Cursor oldCursor = Cursor.Current;
                    Cursor.Current = Cursors.WaitCursor;

                    string line;
                    while ((line = streamReader.ReadLine()) != null)
                    {
                        if (lineNumber == 0)
                        {
                            if (!checkLine0(line))
                            {
                                break;
                            }
                        }
                        else
                        {
                            if(_duplicateDetection.ContainsKey(line))
                            {
                                logError(fi.Name + ": Line " + (lineNumber + 1) + " is a duplicate of line " +
                                    _duplicateDetection[line] + ".  Skipping ...");
                            }
                            else
                            {
                                _duplicateDetection.Add(line, lineNumber + 1);
                                addScore(fi.FullName, line, lineNumber + 1, oldestDate);
                            }
                        }
                        lineNumber++;
                    }
                    Cursor.Current = oldCursor;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error in " + fi.FullName + ": " + ex.Message);
            }
        }

        public void convertToGHIN(FileInfo fi, string GHINFileName)
        {
            try
            {
                int lineNumber = 0;
                _duplicateDetection.Clear();
                using (TextWriter streamWriter = new StreamWriter(GHINFileName))
                {
                    using (TextReader streamReader = new StreamReader(fi.FullName))
                    {
                        Cursor oldCursor = Cursor.Current;
                        Cursor.Current = Cursors.WaitCursor;

                        string line;
                        while ((line = streamReader.ReadLine()) != null)
                        {
                            if (lineNumber == 0)
                            {
                                if (!checkLine0(line))
                                {
                                    break;
                                }
                            }
                            else
                            {
                                if (_duplicateDetection.ContainsKey(line))
                                {
                                    logError(fi.Name + ": Line " + (lineNumber + 1) + " is a duplicate of line " +
                                        _duplicateDetection[line] + ".  Skipping ...");
                                }
                                else
                                {
                                    _duplicateDetection.Add(line, lineNumber + 1);
                                    string GHINLine = convertToGHIN(fi.FullName, line, lineNumber + 1);
                                    if (GHINLine != null)
                                    {
                                        streamWriter.WriteLine(GHINLine);
                                    }
                                }
                            }
                            lineNumber++;
                        }
                        Cursor.Current = oldCursor;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error in " + fi.FullName + ": " + ex.Message);
            }
        }

        private Score ParseLine(string fileName, string line, int lineNumber)
        {
            string[] fields = line.Split(',');

            if (fields.Length == 1)
            {
                return null;  //empty line
            }
            if (fields.Length < 11)
            {
                logError("line " + lineNumber + ": record does not contain 11 fields: " + line);
                return null;
            }

            try
            {
                Score score = new Score();
                score.SourceFile = fileName;
                score.PlayerNumber = fields[1].Replace("\"", "");
                score.GrossScore = int.Parse(fields[2]);
                score.ESScore = int.Parse(fields[3]);
                score.CourseRating = float.Parse(fields[4]);
                score.CourseSlope = int.Parse(fields[5]);
                score.Round = int.Parse(fields[7]);
                score.ClubPlayed = fields[0].Replace("\"", "");

                if (score.ClubPlayed.Length <= 1)
                {
                    logError(fileName + ": Bad club played (length too short) on line " + lineNumber + ": " + line);
                    return null;
                }

                if (score.PlayerNumber.Length <= 1)
                {
                    logError(fileName + ": Bad player number (length too short) on line " + lineNumber + ": " + line);
                    return null;
                }

                // Add leading zeros to get the length to 6
                while (score.PlayerNumber.Length < 6)
                {
                    score.PlayerNumber = "0" + score.PlayerNumber;
                }

                if (score.PlayerNumber.Length < 7)
                {
                    score.PlayerNumber = "0" + score.PlayerNumber;
                }

                if (fields[6] == "\"9\"")
                {
                    // ignore 9 hole scores
                    return null;
                }

                // remove the # on both sides
                string vpDate = fields[10].Replace("#", "");
                vpDate = vpDate.Replace("\"", "");

                //////////////////////////////
                // deal with the date
                //////////////////////////////
                string[] dateFields = vpDate.Split('-');
                if (dateFields.Length == 3)
                {
                    try
                    {
                        score.DT = new DateTime(int.Parse(dateFields[0]), int.Parse(dateFields[1]), int.Parse(dateFields[2]));

                        // If this is round 2 (or more), add a day (or more) to the date
                        if (score.Round > 1)
                        {
                            score.DT = score.DT.AddDays((double)(score.Round - 1));
                        }

                        score.TodaysDateString = score.DT.ToString("MM-dd-yy");
                    }
                    catch
                    {
                        logError(fileName + ": Invalid date: " + vpDate + " line " + lineNumber + ": " + line);
                        return null;
                    }
                }

                // test that score is in the legitimate range
                if (score.ESScore < 19 || (score.ESScore > 166))
                {
                    //logError(fileName + ": Score outside of allowed range (19-166) on line " + lineNumber + ": " + line);
                    return null;
                }

                return score;
            }
            catch(Exception ex)
            {
                MessageBox.Show(fileName + ": error on line " + lineNumber.ToString() + ": " + line + ": " + ex.Message);
                return null;
            }
        }

        public void addScore(string fileName, string line, int lineNumber, DateTime oldestDate)
        {
            Score score = ParseLine(fileName, line, lineNumber);
            if(score == null)
            {
                return;
            }

            try
            {
                if (score.DT < oldestDate)
                {
                    // outside of date range
                    return;
                }

                score.Differential = (((float)score.ESScore - (float)score.CourseRating) * (float)113) / (float)score.CourseSlope;
                if (score.Differential > 0f)
                {
                    score.Differential = truncateToTenths(score.Differential + 0.05f);
                }
                else
                {
                    score.Differential = truncateToTenths(score.Differential - 0.05f);
                }

                if(!_localHandicapDBByNumber.ContainsKey(score.PlayerNumber))
                {
                    PlayerData playerData = new PlayerData(score.PlayerNumber);
                    playerData.Scores.Add(score);
                    _localHandicapDBByNumber.Add(score.PlayerNumber, playerData);

                    if(!_dateList.Contains(score.DT))
                    {
                        _dateList.Add(score.DT);
                    }
                }
                else
                {
                    PlayerData playerData = _localHandicapDBByNumber[score.PlayerNumber];
                    bool duplicate = false;
                    foreach (Score existingScore in playerData.Scores)
                    {
                        if (existingScore.DT == score.DT)
                        {
                            logError("Error: 2 scores with the same date in different files. Player: " + 
                                score.PlayerNumber + "   Date: " + score.DT.ToShortDateString() + "   Files: " +
                                Path.GetFileName(score.SourceFile) + ", "  + Path.GetFileName(existingScore.SourceFile));
                            duplicate = true;
                            break;
                        }
                    }

                    if(!duplicate)
                    {
                        playerData.Scores.Add(score);

                        if (!_dateList.Contains(score.DT))
                        {
                            _dateList.Add(score.DT);
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(fileName + ": error on line " + lineNumber.ToString() + ": " + line + ": " + ex.Message);
            }
        }

        public string convertToGHIN(string fileName, string line, int lineNumber)
        {
            Score score = ParseLine(fileName, line, lineNumber);
            if (score == null)
            {
                return null;
            }

            string[] fields = line.Split(',');
            string newDate = score.DT.ToString("#yyyy-MM-dd#");
            string GHINLine = String.Format("{1}{0}\"{2}\"{0}{3}{0}{4}{0}{5}{0}{6}{0}{7}{0}{8}", ",",
                fields[0], score.PlayerNumber, fields[3], fields[4], fields[5], fields[6],
                (fields[8] == "\" \"") ? "\"H\"" : fields[8], newDate);

            return GHINLine;
        }

        private bool checkLine0(string line)
        {
            string[] fields = line.Split(',');

            string errorMessage = "";
            try
            {
                if (fields[0].Replace("\"", "") != "Club Number")
                {
                    errorMessage = "Line 1 field 1 is not \"Club Number\"";
                }
                else if (fields[1].Replace("\"", "") != "Player Number")
                {
                    errorMessage = "Line 1 field 2 is not \"Player Number\"";
                }
                else if (fields[2].Replace("\"", "") != "Gross Score")
                {
                    errorMessage = "Line 1 field 3 is not \"Gross Score\"";
                }
                else if (fields[3].Replace("\"", "") != "ES Score")
                {
                    errorMessage = "Line 1 field 4 is not \"ES Score\"";
                }
                else if (fields[4].Replace("\"", "") != "Rating")
                {
                    errorMessage = "Line 1 field 5 is not \"Rating\"";
                }
                else if (fields[5].Replace("\"", "") != "Slope")
                {
                    errorMessage = "Line 1 field 6 is not \"Slope\"";
                }
                else if (fields[6].Replace("\"", "") != "Holes")
                {
                    errorMessage = "Line 1 field 7 is not \"Holes\"";
                }
                else if (fields[7].Replace("\"", "") != "Round")
                {
                    errorMessage = "Line 1 field 8 is not \"Round\"";
                }
                else if (fields[8].Replace("\"", "") != "Score Type")
                {
                    errorMessage = "Line 1 field 9 is not \"Score Type\"";
                }
                else if (fields[9].Replace("\"", "") != "Club Played")
                {
                    errorMessage = "Line 1 field 10 is not \"Club Played\"";
                }
                else if (fields[10].Replace("\"", "") != "Score Date")
                {
                    errorMessage = "Line 1 field 11 is not \"Score Date\"";
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }

            if (errorMessage.Length > 0)
            {
                MessageBox.Show(errorMessage, Application.ProductName);
                return false;
            }
            return true;
        }

        private void logError(string error)
        {
            if (errors < 5)
            {
                MessageBox.Show(error);
            }

            if (errors == 5)
            {
                MessageBox.Show("Only first 5 errors are displayed.  The rest of the errors will be handled but not displayed.");
            }

            errors++;
        }

        public List<DateTime> DateList
        {
            get { return _dateList; }
        }

        public SortedDictionary<string, PlayerData> LocalHandicapDBByNumber
        {
            get { return _localHandicapDBByNumber; }
        }

        public SortedDictionary<string, PlayerData> LocalHandicapDBByName
        {
            get { return _localHandicapDBByName; }
        }
    }
}
