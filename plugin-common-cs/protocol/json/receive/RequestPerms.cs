using System;
using Floobits.Common.Protocol;

namespace Floobits.Common.Protocol.Json.Receive
{
    [Serializable]
    public class RequestPerms : Base
    {
        public int user_id;
        public string[] perms;

        protected override string getMessageName() { return "request_perms"; }
    }
}

