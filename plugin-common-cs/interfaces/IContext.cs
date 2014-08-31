using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Floobits.Common.Interfaces
{
    public interface IContext
    {
        void flashMessage(string message);
        void warnMessage(string message);
        void statusMessage(string message);
        void errorMessage(string message);

        void shutdown();
    }
}
