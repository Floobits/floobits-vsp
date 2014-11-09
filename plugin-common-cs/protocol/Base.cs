using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Floobits.Common.Protocol
{
    [Serializable]
    public abstract class Base
    {
        public string name;
        protected Base()
        {
            this.name = getMessageName();
        }
        protected virtual string getMessageName() { return null; }
    }
}
