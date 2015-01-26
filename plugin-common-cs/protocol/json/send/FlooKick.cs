using System;
using Floobits.Common.Protocol;

namespace Floobits.Common.Protocol.Json.Send
{
    [Serializable]
    public class FlooKick : Base
    {
        public int user_id;

        public FlooKick(int userId)
        {
            this.user_id = userId;
        }

        protected override string getMessageName() { return "kick"; }
    }
}

