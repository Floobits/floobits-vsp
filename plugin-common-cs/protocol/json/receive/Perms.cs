using System;
using Floobits.Common.Protocol;

namespace Floobits.Common.Protocol.Json.Receive
{
    [Serializable]
    public class Perms : Base
    {
        public string name = "perms";
        public int user_id;
        public string[] perms;
        public string action;
    }
}
