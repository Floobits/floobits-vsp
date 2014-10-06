using System;
using Floobits.Common.Protocol;

namespace Floobits.Common.Protocol.Json.Send
{
    [Serializable]
    public class FlooMessage : Base
    {
        string name = "msg";
        string data;

        public FlooMessage(string chatContents)
        {
            data = chatContents;
        }
    }
}
