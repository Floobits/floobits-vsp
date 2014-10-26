using System.IO;

namespace Floobits.Common
{
    class FilenameUtils
    {
        public static string separatorsToUnix(string path)
        {
            return path.Replace('\\', '/');
        }

        public static string concat(params string[] paths)
        {
            return Path.Combine(paths);
        }
    }
}
