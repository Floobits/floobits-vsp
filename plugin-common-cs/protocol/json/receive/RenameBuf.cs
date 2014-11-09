using System;
using Floobits.Common.Protocol;

namespace Floobits.Common.Protocol.Json.Receive
{
    [Serializable]
    public class RenameBuf : Base
    {
        public int id;
        public string path;

        public RenameBuf(int id, string path)
        {
            this.id = id;
            // This should already have unix separators but just to make sure
            this.path = FilenameUtils.separatorsToUnix(path);
        }

        protected override string getMessageName() { return "rename_buf"; }
    }
}

