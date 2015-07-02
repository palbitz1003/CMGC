using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace WebAdmin
{
    public class Logging
    {
        private const string LogFileName = "Log.txt";
        private static object _lock = new object();

        public static void Init()
        {
            lock(_lock)
            {
                if(File.Exists(LogFileName))
                {
                    File.Delete(LogFileName);
                }
            }
        }
        public static void Log(string header, string s)
        {
            lock(_lock)
            {
                using(TextWriter tw = new StreamWriter(LogFileName, true))
                {
                    tw.Write(DateTime.Now);
                    tw.WriteLine("  ====== " + header + " ======");
                    tw.WriteLine(s);
                }
            }
        }

        public static void Log(string s)
        {
            lock (_lock)
            {
                using (TextWriter tw = new StreamWriter(LogFileName, true))
                {
                    tw.Write(DateTime.Now);
                    tw.WriteLine(s);
                }
            }
        }
    }
}
