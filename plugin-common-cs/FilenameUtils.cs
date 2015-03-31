using System;
using System.IO;

namespace Floobits.Common
{
    public class FilenameUtils
    {
        public static string separatorsToUnix(string path)
        {
            return path.Replace(Path.PathSeparator, Path.AltDirectorySeparatorChar);
        }

        public static string separatorsToWindows(string path)
        {
            return path.Replace(Path.AltDirectorySeparatorChar, Path.PathSeparator);
        }

        public static string normalize(string path)
        {
            return Path.GetFullPath(path)
                       .ToUpperInvariant();
        }

        public static string normalizeNoEndSeparator(string path)
        {
            return Path.GetFullPath(path)
                       .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                       .ToUpperInvariant();
        }

        public static string concat(params string[] paths)
        {
            return Path.Combine(paths);
        }
    }
}
