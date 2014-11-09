using System;
using System.IO;
using System.Text;
using System.Threading;
using Floobits.Common;
using Floobits.Common.Interfaces;
using Floobits.Common.Protocol;
using Floobits.Utilities;

namespace Floobits.Common.Protocol.Buf
{
    public abstract class Buf
    {
        public string path;
        public int? id;  // can be null
        public volatile string md5;
        public Encoding encoding;

        abstract public void cancelTimeout();
        public static bool isBad(Buf b)
        {
            return (b == null || !b.isPopulated());
        }
        abstract public bool isPopulated();
        abstract public bool isBufNull();
        abstract public override string ToString();
        abstract public IFile createFile();
        abstract public void read();
        abstract public void write();
        abstract public void set(string s, string md5);
        abstract public void patch(FlooPatch res);
        abstract public void send_patch(IFile virtualFile);
        abstract public string serialize();

        public static Buf createBuf(string path, int id, Encoding enc, string md5, IContext context, OutboundRequestHandler outbound)
        {
            if (enc == Encoding.BASE64)
            {
                return new BinaryBuf(path, id, null, md5, context, outbound);
            }
            return new TextBuf(path, id, null, md5, context, outbound);
        }

        public static Buf createBuf(IFile virtualFile, IContext context, OutboundRequestHandler outbound)
        {
            try
            {
                byte[] originalBytes = virtualFile.getBytes();
                string encodedContents = Encoding.UTF8.AsSysEncoding().GetString(originalBytes);
                byte[] decodedContents = new byte[encodedContents.Length * sizeof(char)];
                System.Buffer.BlockCopy(encodedContents.ToCharArray(), 0, decodedContents, 0, decodedContents.Length);
                string filePath = context.toProjectRelPath(virtualFile.getPath());
                if (Array.Equals(decodedContents, originalBytes))
                {
                    IDoc doc = context.iFactory.getDocument(virtualFile);
                    string contents = doc == null ? encodedContents : doc.getText();
                    string md5 = DigestUtils.md5Hex(contents);
                    return new TextBuf(filePath, null, contents, md5, context, outbound);
                }
                else
                {
                    String md5 = DigestUtils.md5Hex(originalBytes);
                    return new BinaryBuf(filePath, null, originalBytes, md5, context, outbound);
                }
            }
            catch (IOException)
            {
                Flog.warn("Error getting virtual file contents in createBuf %s", virtualFile);
            }
            return null;
        }
    }


    public abstract class BufTempl<T> : Buf
    {
        public T buf;
        public Timer timeout;
        public bool forced_patch = false;
        protected IContext context;
        protected OutboundRequestHandler outbound;

        public BufTempl(string path, int? id, T buf, string md5, IContext context, OutboundRequestHandler outbound)
        {
            this.id = id;
            this.path = path;
            this.buf = buf;
            this.md5 = md5;
            this.context = context;
            this.outbound = outbound;
        }

        public override void cancelTimeout()
        {
            if (timeout != null)
            {
                Flog.log("canceling timeout for %s", path);
                timeout.Dispose();
                timeout = null;
            }
        }

        public override bool isPopulated()
        {
            return this.id != null && this.buf != null;
        }

        public override bool isBufNull()
        {
            return this.buf != null;
        }

        protected IFile getVirtualFile()
        {
            return context.iFactory.findFileByPath(context.absPath(this.path));
        }

        protected IFile getOrCreateFile()
        {
            return context.iFactory.getOrCreateFile(context.absPath(this.path));
        }

        protected IDoc getVirtualDoc()
        {
            IFile virtualFile = getVirtualFile();
            if (virtualFile == null)
            {
                Flog.warn("Can't get virtual file to read from disk {0}", this);
                return null;
            }

            return context.iFactory.getDocument(virtualFile);
        }

        public override string ToString()
        {
            return string.Format("id: {0} file: {1}", id, path);
        }

        public override IFile createFile()
        {
            string fn = context.absPath(path);
            string name = Path.GetFileName(fn);
            string parentPath = Path.GetFullPath(fn);
            IFile iFile = context.iFactory.createDirectories(parentPath);
            if (iFile == null)
            {
                context.errorMessage("The Floobits plugin was unable to create a file.");
                return null;
            }
            return iFile.makeFile(name);
        }
    }
}
