using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebAdmin
{
    public class PlayerTeeTimeHistory : IComparable<PlayerTeeTimeHistory>
    {
        public string Name;
        public string GHIN;
        public DateTime?[] TeeTimes;

        // Default sort is by name;
        public int CompareTo(PlayerTeeTimeHistory compareName)
        {
            return Name.CompareTo(compareName.Name);
        }
    }
}
