namespace Floobits.Common
{
    class FilenameUtils
    {
        public static string separatorsToUnix(string path)
        {
            return path.Replace('\\', '/');
        }
    }
}
