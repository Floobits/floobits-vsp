using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Floobits.Common;
using Floobits.Common.Dmp;
using Floobits.Common.Interfaces;
using Floobits.Common.Protocol;
using Floobits.Utilities;
using DiffMatchPatch;

namespace Floobits.Common.Protocol.Buf
{
    public class TextBuf : BufTempl<string>
    {
        protected static FlooDmp dmp = new FlooDmp();

        public TextBuf(string path, int? id, string buf, string md5, IContext context, OutboundRequestHandler outbound) :
            base(path, id, buf, md5, context, outbound)
        {
            if (buf != null)
            {
                this.buf = Constants.NEW_LINE.Replace(buf, "\n");
            }
            this.encoding = Encoding.UTF8;
        }


        public override void read()
        {
            IDoc d = getVirtualDoc();
            if (d == null)
            {
                return;
            }
            this.buf = d.getText();
            this.md5 = DigestUtils.md5Hex(this.buf);
        }

        public override void write()
        {
            if (!isPopulated())
            {
                Flog.warn("Unable to write {0} because it's not populated yet.", path);
                return;
            }

            IDoc d = getVirtualDoc();
            if (d != null)
            {
                lock (context)
                {
                    try
                    {
                        context.setListener(false);
                        d.setReadOnly(false);
                        d.setText(buf);
                    }
                    finally
                    {
                        context.setListener(true);
                    }
                    return;
                }
            }

            Flog.warn("Tried to write to null document: {0}", path);

            IFile virtualFile = getOrCreateFile();
            try
            {

                byte[] bytes = Encoding.UTF8.AsSysEncoding().GetBytes(buf);
                virtualFile.setBytes(bytes);
            }
            catch (Exception e)
            {
                Flog.warn(e.ToString());
                context.errorMessage("The Floobits plugin was unable to write to a file.");
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public override void set(string s, string newMD5)
        {
            buf = s == null ? null : Constants.NEW_LINE.Replace(s, "\n");
            md5 = newMD5;
        }

        public override String serialize()
        {
            return buf;
        }

        public override void send_patch(IFile virtualFile)
        {
            IDoc d = context.iFactory.getDocument(virtualFile);
            if (d == null)
            {
                Flog.warn("Can't get document to read from disk for sending patch {0}", path);
                return;
            }
            send_patch(d.getText());
        }

        public void send_patch(string current)
        {
            string before_md5;
            string textPatch;
            string after_md5;

            string previous = buf;
            before_md5 = md5;
            after_md5 = DigestUtils.md5Hex(current);
            List<Patch> patches = dmp.patch_make(previous, current);
            textPatch = dmp.patch_toText(patches);

            set(current, after_md5);
            if (before_md5.Equals(after_md5))
            {
                Flog.log("Not patching {0} because no change.", path);
                return;
            }
            outbound.patch(textPatch, before_md5, this);
        }

        private void getBuf()
        {
            cancelTimeout();
            outbound.getBuf(id.Value);
        }

        private void setGetBufTimeout()
        {
            int buf_id = id.Value;
            cancelTimeout();
            timeout = context.setTimeout(2000, delegate
            {
                Flog.info("Sending get buf after timeout.");
                outbound.getBuf(buf_id);
            });
        }

        public override void patch(FlooPatch res)
        {
            TextBuf b = this;
            Flog.info("Got _on_patch");

            string oldText = buf;
            IFile virtualFile = b.getVirtualFile();
            if (virtualFile == null)
            {
                Flog.warn("VirtualFile is null, no idea what do do. Aborting everything {0}", this);
                getBuf();
                return;
            }
            IDoc d = context.iFactory.getDocument(virtualFile);
            if (d == null)
            {
                Flog.warn("Document not found for {0}", virtualFile);
                getBuf();
                return;
            }
            string viewText;
            if (!virtualFile.exists())
            {
                viewText = oldText;
            }
            else
            {
                viewText = d.getText();
                if (viewText.Equals(oldText))
                {
                    b.forced_patch = false;
                }
                else if (!b.forced_patch)
                {
                    b.forced_patch = true;
                    oldText = viewText;
                    b.send_patch(viewText);
                    Flog.warn("Sending force patch for {0}. this is dangerous!", b.path);
                }
            }

            b.cancelTimeout();

            string md5Before = DigestUtils.md5Hex(viewText);
            if (!md5Before.Equals(res.md5_before))
            {
                Flog.warn("starting md5s don't match for {0}. this is dangerous!", b.path);
            }

            List<Patch> patches = dmp.patch_fromText(res.patch);
            Object[] results = dmp.patch_apply((List<Patch>)patches, oldText);
            string patchedContents = (string)results[0];
            bool[] patchesClean = (bool[])results[1];
            FlooPatchPosition[] positions = (FlooPatchPosition[])results[2];

            foreach (bool clean in patchesClean)
            {
                if (!clean)
                {
                    Flog.log("Patch not clean for {0}. Sending get_buf and setting readonly.", d.getVirtualFile().getPath());
                    getBuf();
                    return;
                }
            }
            // XXX: If patchedContents have carriage returns this will be a problem:
            string md5After = DigestUtils.md5Hex(patchedContents);
            if (!md5After.Equals(res.md5_after))
            {
                Flog.info("MD5 after mismatch (ours {0} remote {1})", md5After, res.md5_after);
            }

            if (!d.makeWritable())
            {
                Flog.info("Document: {0} is not writable.", d.getVirtualFile().getPath());
                return;
            }

            string text = d.patch(positions);
            if (text == null)
            {
                getBuf();
                return;
            }

            string md5FromDoc = DigestUtils.md5Hex(text);
            if (!md5FromDoc.Equals(res.md5_after))
            {
                Flog.info("md5FromDoc mismatch (ours {0} remote {1})", md5FromDoc, res.md5_after);
                b.setGetBufTimeout();
            }

            b.set(text, md5FromDoc);
            Flog.log("Patched {0}", res.path);
        }
    }
}

