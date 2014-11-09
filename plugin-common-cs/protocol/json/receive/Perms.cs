using System;
using Floobits.Common.Protocol;

namespace Floobits.Common.Protocol.Json.Receive
{
    [Serializable]
    public class Perms : Base
    {
        public int user_id;
        public string[] perms;
        public string action;

        protected override string getMessageName() { return "perms"; }
    }
}
