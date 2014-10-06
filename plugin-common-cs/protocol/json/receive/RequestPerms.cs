using System;
using Floobits.Common.Protocol;

namespace Floobits.Common.Protocol.Json.Receive
{
    [Serializable]
    public class RequestPerms : Base
    {
        public string name = "request_perms";
        public int user_id;
        public string[] perms;
    }
}

