using System;
using System.Collections.Generic;

namespace Floobits.Common.Protocol.Json.Receive
{
    [Serializable]
    public class EditRequest {
        string name = "request_perms";
        public List<string> perms;

        public EditRequest (List<string> perms) {
            this.perms = perms;
        }
    }
}

