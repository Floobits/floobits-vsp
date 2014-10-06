using System;
using Floobits.Common.Protocol;
using BufNS = Floobits.Common.Protocol.Buf;

namespace Floobits.Common.Protocol.Json.Receive
{
    [Serializable]
    public class SetBuf : Base
    {
        public string name = "set_buf";
        public int id;
        public string buf;
        public string md5;
        public string encoding;

        public SetBuf(BufNS.Buf buf)
        {
            this.md5 = buf.md5;
            this.id = buf.id.Value;
            this.encoding = buf.encoding.ToString();
        }
    }
}

