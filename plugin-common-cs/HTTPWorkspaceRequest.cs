using System;
using System.Runtime.Serialization;
using System.Collections.Generic;

namespace Floobits.Common
{
    [Serializable()]
    public class HTTPWorkspaceRequest
    {
        public string owner;
        public string name;
        public Dictionary<string, string[]> perms = new Dictionary<string, string[]>();

        public HTTPWorkspaceRequest(string owner, string name, bool notPublic)
        {
            this.owner = owner;
            this.name = name;
            if (notPublic)
            {
                perms.Add("AnonymousUser", new String[] { });
            }
            else
            {
                perms.Add("AnonymousUser", new String[] { "view_room" });
            }
        }
    }
}
