using System;
using System.Collections.Generic;
using System.IO;

namespace WebAdmin
{
    public class GHINEntry
    {
        public string LastNameFirstName { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public int GHIN { get; set; }
        public string Email { get; set; }
        public DateTime Birthday { get; set; }
        public string MembershipType { get; set; }
        public string SignupPriority { get; set; }
        public string Tee { get; set; }

        public static List<GHINEntry> LoadGHIN(string ghinFileName)
        {
            if (!File.Exists(ghinFileName))
            {
                throw new FileNotFoundException("File does not exist: " + ghinFileName);
            }
            List<GHINEntry> entries = new List<GHINEntry>();
            string[][] csvFileEntries;
            using (TextReader tr = new StreamReader(ghinFileName))
            {
                csvFileEntries = CSVParser.Parse(tr);
            }

            int nameColumn = -1;
            int ghinColumn = -1;
            int emailColumn = -1;
            int birthdayColumn = -1;
            int membershipTypeColumn = -1;
            int signupPriorityColumn = -1;
            int teeColumn = -1;

            for(int col = 0; col < csvFileEntries[0].GetLength(0); col++)
            {
                if (string.Compare(csvFileEntries[0][col], "Name", true) == 0) nameColumn = col;
                if (string.Compare(csvFileEntries[0][col], "GHIN #", true) == 0) ghinColumn = col;
                if (string.Compare(csvFileEntries[0][col], "Email Address", true) == 0) emailColumn = col;
                if (string.Compare(csvFileEntries[0][col], "DOB", true) == 0) birthdayColumn = col;
                if (string.Compare(csvFileEntries[0][col], "Type", true) == 0) membershipTypeColumn = col;
                if (string.Compare(csvFileEntries[0][col], "Signup Priority", true) == 0) signupPriorityColumn = col;
                if (string.Compare(csvFileEntries[0][col], "Tee", true) == 0) teeColumn = col;
            }

            if(nameColumn == -1) throw new ArgumentException("Failed to find column named \"Name\"");
            if (ghinColumn == -1) throw new ArgumentException("Failed to find column named \"GHIN #\"");
            if (emailColumn == -1) throw new ArgumentException("Failed to find column named \"Email Address\"");
            if (birthdayColumn == -1) throw new ArgumentException("Failed to find column named \"DOB\"");
            if (membershipTypeColumn == -1) throw new ArgumentException("Failed to find column named \"Type\"");
            if (signupPriorityColumn == -1) throw new ArgumentException("Failed to find column named \"Signup Priority\"");
            if (teeColumn == -1) throw new ArgumentException("Failed to find column named \"Tee\"");

            for (int row = 1; row < csvFileEntries.Length; row++)
            {
                if ((csvFileEntries[row] == null) || (csvFileEntries[row].Length == 0)) continue;
                if (string.IsNullOrEmpty(csvFileEntries[row][ghinColumn])) continue;
                if(csvFileEntries[row].GetLength(0) <= membershipTypeColumn)
                {
                    throw new ArgumentException(string.Format("Not enough columns of data in row {0}. Expected at least {1} rows", row + 1, membershipTypeColumn));
                }

                GHINEntry ghinEntry = new GHINEntry();
                ghinEntry.LastNameFirstName = csvFileEntries[row][nameColumn];

                string[] nameFields = csvFileEntries[row][nameColumn].Split(',');
                ghinEntry.LastName = nameFields[0].Trim();
                if (nameFields.Length > 1)
                {
                    ghinEntry.FirstName = nameFields[1].Trim();
                }

                int ghinNumber;
                if (!int.TryParse(csvFileEntries[row][ghinColumn], out ghinNumber))
                {
                    throw new ArgumentException(string.Format("Invalid GHIN number on row {0}: '{1}'", row + 1, csvFileEntries[row][ghinColumn]));
                }

                ghinEntry.GHIN = ghinNumber;
                ghinEntry.Email = csvFileEntries[row][emailColumn].Trim();
                ghinEntry.MembershipType = csvFileEntries[row][membershipTypeColumn].Trim();
                ghinEntry.SignupPriority = csvFileEntries[row][signupPriorityColumn].Trim();
                ghinEntry.Tee = csvFileEntries[row][teeColumn].Trim();
                if (string.IsNullOrEmpty(ghinEntry.Tee))
                {
                    ghinEntry.Tee = "W";
                }

                DateTime dt = default(DateTime);
                if (!string.IsNullOrEmpty(csvFileEntries[row][birthdayColumn]))
                {
                    if (!DateTime.TryParse(csvFileEntries[row][birthdayColumn], out dt))
                    {
                        throw new ArgumentException(string.Format("Invalid birthdate on row {0}: '{1}'", row + 1, csvFileEntries[row][birthdayColumn]));
                    }
                }
                ghinEntry.Birthday = dt;

                entries.Add(ghinEntry);
            }

            return entries;
        }

        public static GHINEntry FindName(List<GHINEntry> GHINList, string name)
        {
            if (string.IsNullOrEmpty(name) || (GHINList == null)) return null;

            string[] fields = name.Split(',');

            if (fields.Length < 2) return null;

            string lastName = fields[0].Trim();
            string firstName = fields[1].Trim();

            foreach(var entry in GHINList)
            {
                if(string.Compare(entry.LastName, lastName, true) == 0)
                {
                    if(string.Compare(entry.FirstName, firstName, true) == 0)
                    {
                        return entry;
                    }
                }
            }

            return null;
        }

        public static GHINEntry FindGHIN(List<GHINEntry> GHINList, int number)
        {

            if (GHINList == null) return null;

            foreach (var entry in GHINList)
            {
                if (entry.GHIN == number)
                {
                        return entry;
                }
            }

            return null;
        }
    }
}
