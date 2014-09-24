using System;
using Floobits.Common;
using Floobits.Common.Protocol;

namespace Floobits.Common.Protocol.Json.Send
{
    public abstract class InitialBase : Base
    {
        public string platform = Environment.OSVersion.ToString();
        public string version = Constants.version;
        public string client = "C# client"; //ApplicationImpl.getClientName();
    }
}

