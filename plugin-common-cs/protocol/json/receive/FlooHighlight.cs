using Floobits.Common.Protocol.Buf;
using Floobits.Common.Protocol;
using System.Collections.Generic;

namespace Floobits.Common.Protocol.Json.Receive
{
    public class FlooHighlight : Base
    {
        public string name = "highlight";
        public int id;
        public bool ping = false;
        public bool summon = false;
        public bool following = false;
        public List<List<int>> ranges;
        public int user_id;

        public FlooHighlight(Buf buf, List<List<int>> ranges, bool summon, bool following)
        {
            this.following = following;
            this.id = buf.id;
            if (summon != null)
            {
                this.summon = summon;
                this.ping = summon;
            }
            this.ranges = ranges;
        }
    }
}

