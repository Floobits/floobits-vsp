using System;
using Floobits.Common.Interfaces;

namespace Floobits.Utilities
{
    public class Flog
    {
        private static IContext context;

        public static void Setup(IContext context)
        {
            Flog.context = context;
        }

        public static void log(string s, params object[] args)
        {
            context.statusMessage(String.Format(s, args));
        }
        public static void debug(string s, params object[] args)
        {
            context.statusMessage(String.Format(s, args));
        }
        public static void warn(string s, params object[] args)
        {
            context.warnMessage(String.Format(s, args));
        }
        public static void info(string s, params object[] args)
        {
            context.statusMessage(String.Format(s, args));
        }
    }
}