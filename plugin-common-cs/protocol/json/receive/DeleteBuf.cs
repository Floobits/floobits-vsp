using System;
using Floobits.Common.Protocol;

namespace Floobits.Common.Protocol.Json.Receive
{
    [Serializable]
    public class DeleteBuf : Base
    {
        public int id;
        public bool unlink = false;

        public DeleteBuf(int id, bool unlink)
        {
            this.id = id;
            this.unlink = unlink;
        }

        protected override string getMessageName() { return "delete_buf"; }
    }
}

