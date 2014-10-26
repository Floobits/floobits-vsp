using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Floobits.Common
{
    [Serializable]
    public class Workspace
    {
        public string url;
        public string path;

        public Workspace()
        {

        }

        public Workspace(string url, string path)
        {
            this.url = url;
            this.path = path;
        }

        public void clean()
        {
            if (url.EndsWith("/"))
            {
                url = url.Substring(0, url.Length - 1);
            }
        }
    }
}
