using System;
using System.Collections.Generic;
using System.Text;

namespace LocalHandicap
{
    public class PlayerData
    {
        public PlayerData(string number)
        {
            GHINNumber = number;
        }

        public string GHINNumber;
        public float? LocalHandicap;
        public string Name;
        public float? GHINIndex;
        public float? Handicap;
        public List<Score> Scores = new List<Score>();
        public int IndividualTabIndex;
        public bool Excluded = false;
    }

    public class Score
    {
        public string PlayerNumber;
        public int GrossScore;
        public int ESScore;
        public float Differential;
        public float CourseRating;
        public int CourseSlope;
        public int Round;
        public string TodaysDateString;
        public string ClubPlayed;
        public DateTime DT;
        public bool UsedInLocalHandicap;
        public string SourceFile;
    }
}
