using Floobits.Common.Protocol;

namespace Floobits.Common.Protocol.Json.Receive
{
    public class RequestPerms : Base
    {
        public string name = "request_perms";
        public int user_id;
        public string[] perms;
    }
}

