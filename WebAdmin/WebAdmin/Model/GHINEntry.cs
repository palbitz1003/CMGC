using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace WebAdmin
{
    public class GHINEntry
    {
        public string LastNameFirstName { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public int GHIN { get; set; }

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

            int nameColumn = 0;
            int ghinColumn = 1;

            for (int row = 0; row < csvFileEntries.Length; row++)
            {
                if ((csvFileEntries[row] == null) || (csvFileEntries[row].Length == 0)) continue;

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
    }
}
