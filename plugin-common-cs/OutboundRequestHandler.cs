using System.Runtime.CompilerServices;
using System.Collections.Generic;
using Floobits.Common.Protocol.Buf;
using Floobits.Common.Interfaces;
using Floobits.Common.Protocol;
using Floobits.Common.Protocol.FlooPatch;
using Floobits.Common.Protocol.Json.Receive;
using Floobits.Common.Protocol.Json.Send;
using Floobits.Utilities;

namespace Floobits.Common
{
    public class OutboundRequestHandler
    {
        private IContext context;
        private FloobitsState state;
        private Connection conn;
        protected int requestId = 0;

        public OutboundRequestHandler(IContext context, FloobitsState state, Connection conn)
        {
            this.context = context;
            this.state = state;
            this.conn = conn;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private void syncSetNullBuf(Buf buf)
        {
            buf.set(null, null);
        }

        public void getBuf(int buf_id)
        {
            Buf buf;
            if (!state.bufs.TryGetValue(buf_id, out buf))
            {
                return;
            }
            syncSetNullBuf(buf);
            conn.write(new GetBuf(buf_id));
        }

        public void patch(string textPatch, string before_md5, TextBuf b)
        {
            if (!state.can("patch"))
            {
                return;
            }
            if (Buf.isBad(b))
            {
                Flog.info("Not sending patch. Buf isn't populated yet {0}", b != null ? b.path : "?");
                return;
            }
            Flog.log("Sending patch for {0}", b.path);
            FlooPatch req = new FlooPatch(textPatch, before_md5, b);
            conn.write(req);
        }

        public void createBuf(IFile virtualFile)
        {
            Buf buf = Buf.createBuf(virtualFile, context, this);
            if (buf == null)
            {
                return;
            }
            if (!state.can("patch"))
            {
                return;
            }
            conn.write(new CreateBuf(buf));
        }

        public void deleteBuf(Buf buf, bool unlink)
        {
            if (!state.can("patch"))
            {
                return;
            }
            buf.cancelTimeout();
            conn.write(new DeleteBuf(buf.id.Value, unlink));
        }

        public void saveBuf(Buf b)
        {
            if (Buf.isBad(b))
            {
                Flog.info("Not sending save. Buf isn't populated yet {0}", b != null ? b.path : "?");
                return;
            }
            if (!state.can("patch"))
            {
                return;
            }
            conn.write(new SaveBuf(b.id.Value));
        }

        public void setBuf(Buf b)
        {
            if (!state.can("patch"))
            {
                return;
            }
            b.cancelTimeout();
            conn.write(new SetBuf(b));
        }

        public void renameBuf(Buf b, string newRelativePath)
        {
            if (!state.can("patch"))
            {
                return;
            }
            b.cancelTimeout();
            state.set_buf_path(b, newRelativePath);
            conn.write(new RenameBuf(b.id.Value, newRelativePath));
        }

        public void highlight(Buf b, List<List<int>> textRanges, bool summon, bool following)
        {
            if (!state.can("highlight"))
            {
                return;
            }
            if (Buf.isBad(b))
            {
                Flog.info("Not sending highlight. Buf isn't populated yet {0}", b != null ? b.path : "?");
                return;
            }
            conn.write(new FlooHighlight(b, textRanges, summon, following));
        }

        public void summon(string current, int offset)
        {
            if (!state.can("patch"))
            {
                return;
            }
            Buf buf = state.get_buf_by_path(current);
            if (Buf.isBad(buf))
            {
                context.errorMessage(string.Format("The file {0} is not shared!", current));
                return;
            }
            List<List<int>> ranges = new List<List<int>>();
            ranges.Add(new List<int> { offset, offset });
            conn.write(new FlooHighlight(buf, ranges, true, state.stalking));
        }

        public void requestEdit()
        {
            if (!state.can("request_perms"))
            {
                context.errorMessage("You are not allowed to ask for edit permissions.");
                return;
            }
            conn.write(new EditRequest(new List<string> { "edit_room" }));
        }

        public void message(string chatContents)
        {
            conn.write(new FlooMessage(chatContents));
        }

        public void kick(int userId)
        {
            if (!state.can("kick"))
            {
                return;
            }
            conn.write(new FlooKick(userId));
        }

        public void pong()
        {
            conn.write(new Pong());
        }

        public void setPerms(string action, int userId, string[] perms)
        {
            if (!state.can("kick"))
            {
                return;
            }
            state.changePermsForUser(userId, perms);
            conn.write(new PermsChange(action, userId, perms));
        }
    }
}
