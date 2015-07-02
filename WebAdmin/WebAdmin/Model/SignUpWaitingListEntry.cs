using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebAdmin
{
    public class SignUpWaitingListEntry
    {
        public int Position { get; set; }
        public int GHIN1 { get; set; }
        public string Name1 { get; set; }
        public int GHIN2 { get; set; }
        public string Name2 { get; set; }
        public int GHIN3 { get; set; }
        public string Name3 { get; set; }
        public int GHIN4 { get; set; }
        public string Name4 { get; set; }

        public SignUpWaitingListEntry()
        {
            Position = 0;
            GHIN1 = 0;
            Name1 = string.Empty;
            GHIN2 = 0;
            Name2 = string.Empty;
            GHIN3 = 0;
            Name3 = string.Empty;
            GHIN4 = 0;
            Name4 = string.Empty;
        }
    }
}
