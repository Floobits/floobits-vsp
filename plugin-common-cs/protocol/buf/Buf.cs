using System.IO;
using System.Text;
using Floobits.Common;
using Floobits.Common.Interfaces;
using Floobits.Common.Protocol;
using Floobits.Utilities;

namespace Floobits.Common.Protocol
{
    public interface Buf
    {
        public string path;
        public int id;
        public volatile string md5;
        public Encoding encoding;

        public void cancelTimeout();
        public static bool isBad(Buf b);
        public bool isPopulated();
        public bool isBufNull();
        public override string ToString();
        public IFile createFile();
        public void read();
        public void write();
        public void set(string s, string md5);
        public void patch(FlooPatch res);
        public void send_patch(IFile virtualFile);
        public string serialize();
        public static Buf createBuf(string path, int id, Encoding enc, string md5, IContext context, OutboundRequestHandler outbound);
        public static Buf createBuf(IFile virtualFile, IContext context, OutboundRequestHandler outbound);
    }
    
    
    public abstract class BufTempl<T>
    {
        public string path;
        public int id;
        public volatile string md5;
        public volatile T buf;
        public Encoding encoding;
        public ScheduledFuture timeout;
        public bool forced_patch = false;
        protected IContext context;
        protected OutboundRequestHandler outbound;

        public BufTempl(string path, int id, T buf, string md5, IContext context, OutboundRequestHandler outbound)
        {
            this.id = id;
            this.path = path;
            this.buf = buf;
            this.md5 = md5;
            this.context = context;
            this.outbound = outbound;
        }

        public void cancelTimeout()
        {
            if (timeout != null)
            {
                Flog.log("canceling timeout for %s", path);
                timeout.cancel(false);
                timeout = null;
            }
        }
        public static bool isBad(Buf b)
        {
            return (b == null || !b.isPopulated());
        }

        public bool isPopulated()
        {
            return this.id != null && this.buf != null;
        }

        public bool isBufNull()
        {
            return this.buf != null;
        }

        protected IFile getVirtualFile()
        {
            return context.iFactory.findFileByPath(context.absPath(this.path));
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

        public IFile createFile()
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
                string encodedContents = new string(originalBytes, UTF8);
                byte[] decodedContents = encodedContents.getBytes();
                String filePath = context.toProjectRelPath(virtualFile.getPath());
                if (Arrays.equals(decodedContents, originalBytes))
                {
                    IDoc doc = context.iFactory.getDocument(virtualFile);
                    String contents = doc == null ? encodedContents : doc.getText();
                    String md5 = DigestUtils.md5Hex(contents);
                    return new TextBuf(filePath, null, contents, md5, context, outbound);
                }
                else
                {
                    String md5 = DigestUtils.md5Hex(originalBytes);
                    return new BinaryBuf(filePath, null, originalBytes, md5, context, outbound);
                }
            }
            catch (IOException e)
            {
                Flog.warn("Error getting virtual file contents in createBuf %s", virtualFile);
            }
            return null;
        }
    }
}
