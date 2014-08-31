using System;

namespace Floobits.Common.Protocol
{
    [Serializable]
    public class FlooUser 
    {
        public string[] perms;
        public string client;
        public string platform;
        public int user_id;
        public string username;
        public string version;
    }
}
