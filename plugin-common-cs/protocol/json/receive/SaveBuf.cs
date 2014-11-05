using System;

namespace Floobits.Common.Protocol.Json.Receive
{
    [Serializable]
    public class SaveBuf : Base
    {
        public int id;
        public string name = "saved";

        public SaveBuf(int id)
        {
            this.id = id;
        }
    }
}