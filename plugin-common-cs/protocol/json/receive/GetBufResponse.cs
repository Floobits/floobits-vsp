using System;
using Floobits.Common.Protocol;

namespace Floobits.Common.Protocol.Json.Receive
{
    [Serializable]
    public class GetBufResponse : Base
    {
        public int id;
        public string path;
        public string buf;
        public string encoding;
        public string md5;
    }
}

