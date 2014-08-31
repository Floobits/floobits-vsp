using System;
using System.Collections.Generic;
using Floobits.Common.Protocol;
using Floobits.Common.Protocol.Json.Receive;

namespace Floobits.Common.Protocol.Json.Send
{
    [Serializable]
    public class RoomInfoResponse : Base
    {
        public string[] anon_perms;
        public int max_size;
        public string owner;
        public string[] perms;
        public string room_name;
        public bool secret;
        public Dictionary<int, FlooUser> users;
        public Dictionary<int, RoomInfoBuf> bufs;
        public string user_id;
    }
}
