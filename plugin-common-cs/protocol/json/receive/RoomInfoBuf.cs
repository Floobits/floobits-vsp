using System;
using Floobits.Common.Protocol.Base;

namespace Floobits.Common.Protocol.Json.Receive
{
    [Serializable]
    public class RoomInfoBuf : Base
    {
        public int id;
        public string md5;
        public string path;
        public string encoding;
    }
}

