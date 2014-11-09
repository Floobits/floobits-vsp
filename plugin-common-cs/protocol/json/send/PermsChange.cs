﻿using System;

namespace Floobits.Common.Protocol.Json.Send
{
    [Serializable]
    public class PermsChange : InitialBase
    {
        string action;
        int user_id;
        string[] perms;

        public PermsChange(string action, int userId, string[] perms) {
            this.action = action;
            this.user_id = userId;
            this.perms = perms;
        }

        protected override string getMessageName() { return "perms"; }
    }
}
