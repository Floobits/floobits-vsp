using System;
using Floobits.Common.Protocol;

namespace Floobits.Common.Protocol.Json.Receive
{
    [Serializable]
    public class DeleteBuf : Base
    {
        public int id;
        public string name = "delete_buf";
        public bool unlink = false;

        public DeleteBuf(int id, bool unlink)
        {
            this.id = id;
            this.unlink = unlink;
        }
    }
}

