using System;
using Floobits.Common.Interfaces;

namespace Floobits.Utilities
{
    public class Flog
    {
        private static IContext context;
        private static string logPath = String.Format("{0}/floobits.msg.log", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));

        public static void Setup(IContext context)
        {
            Flog.context = context;
            string[] lines = { "Floobits plugin started." };
            // WriteAllLines creates a file, writes a collection of strings to the file, 
            // and then closes the file.
            System.IO.File.WriteAllLines(logPath, lines);
        }

        public static void log(string s, params object[] args)
        {
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(logPath, true))
            {
                file.WriteLine(s);
            }
        }
        public static void debug(string s, params object[] args)
        {
            log(s, args);
        }
        public static void warn(string s, params object[] args)
        {
            log(s, args);
        }
        public static void info(string s, params object[] args)
        {
            log(s, args);
        }
    }
}