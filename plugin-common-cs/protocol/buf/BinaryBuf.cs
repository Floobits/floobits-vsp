using System;
using System.Runtime.CompilerServices;
using Floobits.Common;
using Floobits.Common.Protocol;
using Floobits.Common.Protocol.Handlers;
using Floobits.Common.Interfaces;
using Floobits.Utilities;

namespace Floobits.Common.Protocol.Buf
{
    public class BinaryBuf : BufTempl<byte[]>
    {

        public BinaryBuf(string path, int? id, byte[] buf, string md5, IContext context, OutboundRequestHandler outbound) :
            base(path, id, buf, md5, context, outbound)
        {
            this.encoding = Encoding.BASE64;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public override void read()
        {
            IFile virtualFile = getVirtualFile();
            if (virtualFile == null)
            {
                Flog.warn("Couldn't get virtual file in readFromDisk {0}", this);
                return;
            }

            byte[] bytes = virtualFile.getBytes();
            if (bytes == null)
            {
                Flog.warn("Could not get byte array contents for file {0}", this);
                return;
            }
            buf = bytes;
            md5 = DigestUtils.md5Hex(bytes);
        }

        public override void write()
        {
            context.writeThread(delegate
            {
                if (!isPopulated())
                {
                    Flog.warn("Unable to write {0} because it's not populated yet.", path);
                    return;
                }
                IFile virtualFile = getOrCreateFile();
                if (virtualFile == null)
                {
                    context.errorMessage("Unable to write file. virtualFile is null.");
                    return;
                }
                FlooHandler flooHandler = context.getFlooHandler();
                if (flooHandler == null)
                {
                    return;
                }
                lock (context)
                {
                    try
                    {
                        context.setListener(false);
                        if (!virtualFile.setBytes(buf))
                        {
                            Flog.warn("Writing binary content to disk failed. {0}", path);
                        }
                    }
                    finally
                    {
                        context.setListener(true);
                    }
                }
            });
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public override void set(string s, string md5)
        {
            if (s == null)
            {
                buf = new byte[] { };
            }
            else
            {
                buf = Convert.FromBase64String(s);
            }
            this.md5 = md5;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void set(byte[] s, string md5)
        {
            buf = s;
            this.md5 = md5;
        }

        public override string serialize()
        {
            return Convert.ToBase64String(buf);
        }

        public override void patch(FlooPatch res)
        {
            FlooHandler flooHandler = context.getFlooHandler();
            if (flooHandler == null)
            {
                return;
            }
            flooHandler.outbound.getBuf(this.id.Value);
            set((byte[])null, null);
        }

        public override void send_patch(IFile virtualFile)
        {
            FlooHandler flooHandler = context.getFlooHandler();
            if (flooHandler == null)
            {
                return;
            }
            byte[] contents = virtualFile.getBytes();
            if (contents == null)
            {
                Flog.warn("Couldn't read contents of binary file. {0}", virtualFile);
                return;
            }
            string after_md5 = DigestUtils.md5Hex(contents);
            if (md5.Equals(after_md5))
            {
                Flog.debug("Binary file change event but no change in md5 {0}", virtualFile);
                return;
            }
            set(contents, after_md5);
            flooHandler.outbound.setBuf(this);
        }
    }
}

