using System;
using BufNS = Floobits.Common.Protocol.Buf;

namespace Floobits.Common.Protocol
{
    [Serializable]
    public class FlooPatch : Base
    {
        public string name = "patch";
        public int id;
        public int user_id;
        public string md5_after;
        public string md5_before;
        public string patch;

        // Deprecated
        public string path;
        public string username;


        public FlooPatch(){}

        public FlooPatch (string patch, string md5_before, BufNS.Buf buf) {
            this.path = buf.path;
            this.md5_before = md5_before;
            this.md5_after = buf.md5;
            this.id = buf.id.Value;
            this.patch = patch;
        }
    }
}
