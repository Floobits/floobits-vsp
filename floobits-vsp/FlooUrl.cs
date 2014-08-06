using System;
using System.Net;

namespace Floobits
{
    public class FlooUrl
    {
        public string proto;
        public string host;
        public string owner;
        public string workspace;

        public int port;
        public bool secure;

        public FlooUrl(string url)
        {
            Uri u = new Uri(url);
            string path = u.AbsolutePath;
            string[] parts = path.Split('/');

            this.host = u.Host;
            this.owner = parts[1];
            this.workspace = parts[2];
            if (this.owner.Equals("r")) {
                this.owner = parts[2];
                this.workspace = parts[3];
            }
            this.port = u.Port;
            this.proto = u.Scheme;

            this.secure = !this.proto.Equals("http");

            if (this.port < 0) {
                if (this.secure) {
                    this.port = 3448;
                } else {
                    this.port = 3148;
                }
            }
        }

        public FlooUrl(string host, string owner, string workspace, int port, bool secure)
        {
            this.host = host;
            this.owner = owner;
            this.workspace = workspace;
            this.port = port < 0 ? 3448 : port;
            this.secure = secure;
            this.proto = secure ? "https" : "http";
        }

        public string toString() {
            string port = "";

            if (this.secure) {
                proto = "https";
                if (this.port != 3448) {
                    port = string.Format(":{0}", this.port);
                }
            } else {
                proto = "http";
                if (this.port != 3148) {
                    port = string.Format(":{0}", this.port);
                }
            }
            return string.Format("{0}://{1}{2}/{3}/{4}", proto, this.host, port, this.owner, this.workspace);
        }
    }   
};
