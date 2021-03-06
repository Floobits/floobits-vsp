﻿using System;
using Floobits.Common.Protocol;

namespace Floobits.Common.Protocol.Json.Send
{
    [Serializable]
    public class FlooMessage : Base
    {
        public string data;

        public FlooMessage(string chatContents)
        {
            data = chatContents;
        }

        protected override string getMessageName() { return "msg"; }
    }
}
