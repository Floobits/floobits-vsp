using System;
using Floobits.Common.Protocol;
using BufNS = Floobits.Common.Protocol.Buf;
using Floobits.Common;

namespace Floobits.Common.Protocol.Json.Receive
{
    [Serializable]
    public class CreateBuf : Base
    {
        public string buf;
        public string path;
        public string md5;
        public string encoding;

        public CreateBuf(BufNS.Buf buf)
        {
            this.path = FilenameUtils.separatorsToUnix(buf.path);
            this.buf = buf.serialize();
            this.md5 = buf.md5;
            this.encoding = buf.encoding.ToString();
        }

        protected override string getMessageName() { return "create_buf"; }
    }
}
