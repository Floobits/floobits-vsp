using System;
using System.Text.RegularExpressions;

namespace Floobits.Common
{
    public class Constants {
        static public string baseDir = baseDirInit();
        static public string shareDir = FilenameUtils.concat(baseDir, "share");
        static public string version = "0.11";
        static public string pluginVersion = "0.01";
        static public string OUTBOUND_FILTER_PROXY_HOST = "proxy.floobits.com";
        static public int OUTBOUND_FILTER_PROXY_PORT = 443;
        static public string floobitsDomain = "floobits.com";
        static public string defaultHost = "floobits.com";
        static public int defaultPort = 3448;
        static public int insecurePort = 3148;
        static public Regex NEW_LINE = new Regex("\\r\\n?", RegexOptions.Compiled);
        static public int TOO_MANY_BIG_DIRS = 50;

        static public string baseDirInit()
        {
            return FilenameUtils.concat(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "floobits");
        }
    }
}

