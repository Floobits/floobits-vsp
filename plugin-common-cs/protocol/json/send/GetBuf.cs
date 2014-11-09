using System;
using Floobits.Common.Protocol;

namespace Floobits.Common.Protocol.Json.Send
{
    [Serializable]
    public class GetBuf : Base
    {
        public int id;

        public GetBuf(int buf_id)
        {
            this.id = buf_id;
        }

        protected override string getMessageName() { return "get_buf"; }
    }
}
