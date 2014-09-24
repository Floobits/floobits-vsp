using System;
using Floobits.Common.Protocol;

namespace Floobits.Common.Protocol.Json.Send
{
    [Serializable]
    public class GetBuf : Base
    {
        public string name = "get_buf";
        public int id;

        public GetBuf(int buf_id)
        {
            this.id = buf_id;
        }
    }
}
