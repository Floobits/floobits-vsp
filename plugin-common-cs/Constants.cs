using System.Text.RegularExpressions;

namespace Floobits.Common
{
    public class Constants {
        static public string baseDir = FilenameUtils.concat(System.getProperty("user.home"), "floobits");
        static public string shareDir = FilenameUtils.concat(baseDir, "share");
        static public string version = "0.11";
        static public string pluginVersion = "0.01";
        static public string OUTBOUND_FILTER_PROXY_HOST = "proxy.floobits.com";
        static public int OUTBOUND_FILTER_PROXY_PORT = 443;
        static public string floobitsDomain = "floobits.com";
        static public string defaultHost = "floobits.com";
        static public int defaultPort = 3448;
        static public Regex NEW_LINE = new Regex("\\r\\n?", RegexOptions.Compiled);
        static public int TOO_MANY_BIG_DIRS = 50;
    }
}

