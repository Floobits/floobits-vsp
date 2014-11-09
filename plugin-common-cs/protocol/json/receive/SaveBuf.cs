using System;

namespace Floobits.Common.Protocol.Json.Receive
{
    [Serializable]
    public class SaveBuf : Base
    {
        public int id;

        public SaveBuf(int id)
        {
            this.id = id;
        }

        protected override string getMessageName() { return "saved"; }
    }
}