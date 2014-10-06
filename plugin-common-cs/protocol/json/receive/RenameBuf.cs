using Floobits.Common.Protocol;

namespace Floobits.Common.Protocol.Json.Receive
{
    public class RenameBuf : Base
    {
        public int id;
        public string name = "rename_buf";
        public string path;

        public RenameBuf(int id, string path)
        {
            this.id = id;
            // This should already have unix separators but just to make sure
            this.path = FilenameUtils.separatorsToUnix(path);
        }
    }
}

