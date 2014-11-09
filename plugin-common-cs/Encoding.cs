using System;
using SysEncoding = System.Text.Encoding;

namespace Floobits.Common
{
    public class Encoding
    {
        private string enc;

        Encoding(string enc)
        {
            this.enc = enc;
        }

        public override string ToString()
        {
            return this.enc;
        }

        public static Encoding from(string str)
        {
            return new Encoding(str);
        }

        public static implicit operator SysEncoding(Encoding e)
        {
            switch (e.enc)
            {
                case "utf8":
                    return SysEncoding.UTF8;
                case "base64":
                    return SysEncoding.ASCII;
                default:
                    throw (new NotSupportedException());
            }
        }

        public SysEncoding AsSysEncoding()
        {
            return this;
        }

        static public Encoding UTF8
        {
            get { return from("utf8"); }
        }
        static public Encoding BASE64
        {
            get { return from("base64"); }
        }
    }
}
