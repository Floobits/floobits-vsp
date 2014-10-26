using Newtonsoft.Json.Linq;
using Floobits.Common;
using Floobits.Common.Interfaces;
using Floobits.Common.Protocol;
using Floobits.Common.Protocol.Buf;
using Floobits.Common.Protocol.Json.Send;
using Floobits.Common.Protocol.Json.Receive;
using Floobits.Utilities;
using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;

namespace Floobits.Common
{
    public class InboundRequestHandler {
        private IContext context;
        private FloobitsState state;
        private OutboundRequestHandler outbound;
        private bool shouldUpload;
        private StatusMessageThrottler fileAddedMessageThrottler;
        private StatusMessageThrottler fileRemovedMessageThrottler;
        private EditorScheduler editor;

        enum Events {
            room_info, get_buf, patch, highlight, saved, join, part, create_buf, ack,
            request_perms, msg, rename_buf, term_stdin, term_stdout, delete_buf, perms, ping
        }
        public InboundRequestHandler(IContext context, FloobitsState state, OutboundRequestHandler outbound, bool shouldUpload) {
            this.context = context;
            editor = context.editor;
            this.state = state;
            this.outbound = outbound;
            this.shouldUpload = shouldUpload;
            fileAddedMessageThrottler = new StatusMessageThrottler(context,
                    "{0} files were added to the workspace.");
            fileRemovedMessageThrottler = new StatusMessageThrottler(context,
                    "{0} files were removed from the workspace.");
        }

        private class ConflictWork
        {
            private IContext context;
            private OutboundRequestHandler outbound;
            private LinkedList<Buf> conflicts;
            private LinkedList<Buf> missing;

            public ConflictWork(IContext context, OutboundRequestHandler outbound, LinkedList<Buf> conflicts, LinkedList<Buf> missing)
            {
                this.context = context;
                this.outbound = outbound;
                this.conflicts = conflicts;
                this.missing = missing;
            }
            public void stompLocalWork() {
                foreach (Buf buf in conflicts) {
                    outbound.getBuf(buf.id.Value);
                }
                foreach (Buf buf in missing) {
                    outbound.getBuf(buf.id.Value);
                }
            }
            public void stompRemoteWork() {
                foreach (Buf buf in conflicts) {
                    outbound.setBuf(buf);
                }
                foreach (Buf buf in missing) {
                    outbound.deleteBuf(buf, false);
                }
            }
            public void fleeWork() {
                context.shutdown();
            }
        }

        private void initialManageConflicts(RoomInfoResponse ri) {
            LinkedList<Buf> conflicts = new LinkedList<Buf>();
            LinkedList<Buf> missing = new LinkedList<Buf>();
            LinkedList<string> conflictedPaths = new LinkedList<string>();
            foreach (KeyValuePair<int, RoomInfoBuf> entry in ri.bufs) {
                int buf_id = entry.Key;
                RoomInfoBuf b = entry.Value;
                Buf buf = Buf.createBuf(b.path, b.id, Encoding.from(b.encoding), b.md5, context, outbound);
                state.bufs.Add(buf_id, buf);
                state.paths_to_ids.Add(b.path, b.id);
                buf.read();
                if (buf.isBufNull()) {
                    if (buf.path.Equals("FLOOBITS_README.md") && buf.id == 1) {
                        outbound.getBuf(buf.id.Value);
                        continue;
                    }
                    missing.AddLast(buf);
                    conflictedPaths.AddLast(buf.path);
                    continue;
                }
                if (!b.md5.Equals(buf.md5)) {
                    conflicts.AddLast(buf);
                    conflictedPaths.AddLast(buf.path);
                }
            }

            if (conflictedPaths.Count <= 0) {
                return;
            }

            string[] conflictedPathsArray;
            conflictedPaths.CopyTo(conflictedPathsArray, 0);
            ConflictWork conflictwork = new ConflictWork(context, outbound, conflicts, missing);
            ThreadStart stompLocal = new ThreadStart(conflictwork.stompLocalWork);
            ThreadStart stompRemote = new ThreadStart(conflictwork.stompRemoteWork);
            ThreadStart flee = new ThreadStart(conflictwork.fleeWork);
            context.dialogResolveConflicts(stompLocal, stompRemote, state.readOnly, flee, conflictedPathsArray);
        }

        private void initialUpload(RoomInfoResponse ri) {
            context.statusMessage("Overwriting remote files and uploading new ones.");
            context.flashMessage("Overwriting remote files and uploading new ones.");

            Ignore ignoreTree = context.getIgnoreTree();
            List<Ignore> allIgnores = new List<Ignore>();
            LinkedList<Ignore> tempIgnores = new LinkedList<Ignore>();
            tempIgnores.AddLast(ignoreTree);
            int size = 0;
            Ignore ignore;
            while (tempIgnores.Count > 0) {
                ignore = tempIgnores.Last.Value;
                tempIgnores.RemoveLast();
                size += ignore.size;
                allIgnores.Add(ignore);
                foreach (Ignore ig in ignore.children.Values) {
                    tempIgnores.AddLast(ig);
                }
            }
            LinkedList<Ignore> tooBigIgnores = new LinkedList<Ignore>();
            allIgnores.Sort();

            while (size > ri.max_size) {
                Ignore ig = allIgnores[0];
                allIgnores.RemoveAt(0);
                size -= ig.size;
                tooBigIgnores.AddLast(ig);
            }
            if (tooBigIgnores.Count > 0) {
                if (tooBigIgnores.Count > Constants.TOO_MANY_BIG_DIRS) {
                    context.dialogDisconnect(ri.max_size/1000, tooBigIgnores.Count);
                    return;
                }
                bool shouldContinue;

                shouldContinue = context.dialogTooBig(tooBigIgnores);

                if (!shouldContinue) {
                    context.shutdown();
                    return;
                }
            }

            HashSet<String> paths = new HashSet<String>();
            foreach (Ignore ig in allIgnores) {
                foreach (IFile virtualFile in ig.files)
                    paths.Add(context.toProjectRelPath(virtualFile.getPath()));
            }
            foreach (KeyValuePair<int, RoomInfoBuf> entry in ri.bufs) {
                int buf_id = entry.Key;
                RoomInfoBuf b = entry.Value;
                Buf buf = Buf.createBuf(b.path, b.id, Encoding.from(b.encoding), b.md5, context, outbound);
                state.bufs.Add(buf_id, buf);
                state.paths_to_ids.Add(b.path, b.id);
                if (!paths.Contains(buf.path)) {
                    outbound.deleteBuf(buf, false);
                    continue;
                }
                paths.Remove(buf.path);
                buf.read();
                if (buf.isBufNull()) {
                    outbound.getBuf(buf.id.Value);
                    continue;
                }
                if (b.md5.Equals(buf.md5)) {
                    continue;
                }
                outbound.setBuf(buf);
            }


            foreach (string path in paths) {
                IFile fileByPath = context.iFactory.findFileByPath(context.absPath(path));
                if (fileByPath == null || !fileByPath.isValid()) {
                    Flog.warn(string.Format("path is no longer a valid virtual file"));
                    continue;
                }
                outbound.createBuf(fileByPath);
            }
            string flooignore = FilenameUtils.concat(context.colabDir, ".flooignore");

            try {
                List<string> strings = new List<String>();
                if (File.Exists(flooignore)) {
                    strings.AddRange(File.ReadAllLines(flooignore));
                }

                foreach (Ignore ig in tooBigIgnores) {
                    string rule = "/" + context.toProjectRelPath(ig.stringPath);
                    if (!rule.EndsWith("/")) {
                        rule += "/";
                    }
                    rule += "*";
                    strings.Add(rule);
                }
                context.setListener(false);
                File.WriteAllLines(flooignore, strings);
                IFile fileByIoFile = context.iFactory.findFileByIoFile(f);
                if (fileByIoFile != null) {
                    fileByIoFile.refresh();
                    ignoreTree.addRules(fileByIoFile);
                }
            } catch (IOException e) {
                Flog.warn(e.ToString());
            } finally {
                context.setListener(true);
            }
            shouldUpload = false;
        }
        void _on_rename_buf(JObject jsonObject) {
            string name = jsonObject.GetValue("old_path").ToString();
            string oldPath = context.absPath(name);
            string newPath = context.absPath(jsonObject.GetValue("path").ToString());

            Buf buf = state.get_buf_by_path(oldPath);
            if (buf == null) {
                if (state.get_buf_by_path(newPath) == null) {
                    Flog.warn("Rename oldPath and newPath don't exist. %s %s", oldPath, newPath);
                } else {
                    Flog.info("We probably rename this, nothing to rename.");
                }
                return;
            }
            editor.queue(buf, new RunLaterAction<Buf>(delegate (Buf dbuf) {
                IFile foundFile = context.iFactory.findFileByPath(oldPath);
                if (foundFile == null) {
                    Flog.warn("File we want to move was not found %s %s.", oldPath, newPath);
                    return;
                }
                string newRelativePath = context.toProjectRelPath(newPath);
                if (newRelativePath == null) {
                    context.errorMessage("A file is now outside the workspace.");
                    return;
                }
                state.set_buf_path(dbuf, newRelativePath);

                string newFileName = Path.GetFileName(newPath);
                // Rename file

                if (foundFile.rename(null, newFileName)) {
                    return;
                }

                // Move file
                string newParentDirectoryPath = Path.GetDirectoryName(newPath);
                string oldParentDirectoryPath = Path.GetDirectoryName(oldPath);
                if (newParentDirectoryPath.Equals(oldParentDirectoryPath)) {
                    Flog.warn("Only rename file, don't need to move {0} {1}", oldPath, newPath);
                    return;
                }
                IFile directory = context.iFactory.createDirectories(newParentDirectoryPath);
                if (directory == null) {
                    return;
                }

                foundFile.move(null, directory);
            }));
        }

        void _on_request_perms(JObject obj) {
            Flog.log("got perms receive %s", obj);
            RequestPerms requestPerms = obj.ToObject<RequestPerms>();
            int userId = requestPerms.user_id;
            FlooUser u = state.getUser(userId);
            if (u == null) {
                Flog.info("Unknown user for id %s. Not handling request_perms event.", userId);
                return;
            }
            context.dialogPermsRequest(u.username, new RunLaterAction<string>(delegate (string action) {
                outbound.setPerms(action, userId, new string[]{"edit_room"});
            }));
        }

        void _on_join(JObject obj) {
            FlooUser u = obj.ToObject<FlooUser>();
            state.addUser(u);
        }

        void _on_part(JObject obj) {
            JToken id = obj.SelectToken("user_id");
            if (id == null){
                return;
            }
            int userId = id.Value<int>();
            state.removeUser(userId);
            context.iFactory.removeHighlightsForUser(userId);
        }

        void _on_delete_buf(JObject obj) {
            DeleteBuf deleteBuf = obj.ToObject<DeleteBuf>();
            Buf buf;
            if (!state.bufs.TryGetValue(deleteBuf.id, out buf)) {
                Flog.warn(string.Format("Tried to delete a buf that doesn't exist: {0}", deleteBuf.id));
                return;
            }
            editor.queue(buf, new RunLaterAction<Buf>(delegate(Buf dbuf) {
                dbuf.cancelTimeout();
                if (state.bufs != null) {
                    state.bufs.Remove(deleteBuf.id);
                    state.paths_to_ids.Remove(dbuf.path);
                }
                if (!deleteBuf.unlink) {
                    fileRemovedMessageThrottler.statusMessage(string.Format("Removed the file, {0}, from the workspace.", dbuf.path));
                    return;
                }
                String absPath = context.absPath(dbuf.path);
                IFile fileByPath = context.iFactory.findFileByPath(absPath);

                if (fileByPath == null) {
                    return;
                }

                fileByPath.delete(this);
            }));
        }

        void _on_msg(JObject jsonObject){
            string msg = jsonObject.SelectToken("data").ToString();
            string username = jsonObject.SelectToken("username").ToString();
            double time = jsonObject.SelectToken("time").ToObject<double>();
            DateTime messageDate;
            if (time == null) {
                messageDate = DateTime.Now;
            } else {
                // fixme
            }

            context.chat(username, msg, messageDate);
        }

        void _on_term_stdout(JObject jsonObject) {}
        void _on_term_stdin(JObject jsonObject) {}

        void _on_ping(JObject jsonObject) {
            outbound.pong();
        }

        public void _on_highlight(JObject obj) {
            FlooHighlight res = obj.ToObject<FlooHighlight>();
            bool force = !res.following && (res.ping || (res.summon == null ? false : res.summon));
            state.lastHighlight = obj;
            Buf buf = this.state.bufs[res.id];
            editor.queue(buf, new RunLaterAction<Buf>(delegate(Buf dbuf) {
                IDoc iDoc = context.iFactory.getDocument(dbuf.path);
                if (iDoc == null) {
                    return;
                }
                string username = state.getUsername(res.user_id);
                iDoc.applyHighlight(dbuf.path, res.user_id, username, state.getFollowing(), force, res.ranges);
            }));
        }

        void _on_saved(JObject obj) {
            int id = obj["id"].ToObject<int>();
            Buf buf = this.state.bufs[id];
            editor.queue(buf, new RunLaterAction<Buf>(delegate(Buf dbuf) {
                IDoc document = context.iFactory.getDocument(dbuf.path);
                if (document == null) {
                    return;
                }
                document.save();
            }));
        }

        void _on_create_buf(JObject obj) {
            GetBufResponse res = obj.ToObject<GetBufResponse>();
            Buf buf;
            if (res.encoding.Equals(Encoding.BASE64.ToString())) {
                buf = new BinaryBuf(res.path, res.id, Convert.FromBase64String(res.buf), res.md5, context, outbound);
            } else {
                buf = new TextBuf(res.path, res.id, res.buf, res.md5, context, outbound);
            }
            editor.queue(buf, new RunLaterAction<Buf>(delegate(Buf dbuf) {
                if (state.bufs == null) {
                    return;
                }
                state.bufs.Add(buf.id.Value, dbuf);
                state.paths_to_ids.Add(dbuf.path, dbuf.id.Value);
                dbuf.write();
                fileAddedMessageThrottler.statusMessage(String.Format("Added the file, {0}, to the workspace.", dbuf.path));
            }));
        }

        void _on_perms(JObject obj) {
            Perms res = obj.ToObject<Perms>();

            bool previousState = state.can("patch");
            if (res.user_id != state.getMyConnectionId()) {
                return;
            }
            HashSet<string> perms = new HashSet<string>(res.perms);
            if (res.action.Equals("add")) {
                state.perms.UnionWith(perms);
            } else if (res.action.Equals("set")) {
                state.perms.Clear();
                state.perms.UnionWith(perms);
            } else if (res.action.Equals("remove")) {
                state.perms.ExceptWith(perms);
            }
            state.readOnly = !state.can("patch");
            if (state.can("patch") != previousState) {
                if (state.can("patch")) {
                    context.statusMessage("You state.can now edit this workspace.");
                    context.iFactory.clearReadOnlyState();
                } else {
                    context.errorMessage("You state.can no longer edit this workspace.");
                }
            }
        }

        void _on_patch(JObject obj) {
            FlooPatch res = obj.ToObject<FlooPatch>();
            Buf buf = this.state.bufs[res.id];
            editor.queue(buf, new RunLaterAction<Buf>(delegate(Buf dbuf) {
                if (dbuf.isBufNull()) {
                    Flog.warn("no buffer");
                    outbound.getBuf(res.id);
                    return;
                }

                if (res.patch.Length == 0) {
                    Flog.warn("wtf? no patches to apply. server is being stupid");
                    return;
                }
                dbuf.patch(res);
            }));
        }

        void _on_room_info(JObject obj) {
            context.readThread(delegate {
                try {
                    RoomInfoResponse ri = obj.ToObject<RoomInfoResponse>();
                    state.handleRoomInfo(ri);

                    context.statusMessage(String.Format("You successfully joined {0} ", state.url.toString()));
                    context.openChat();

                    DotFloo.write(context.colabDir, state.url.toString());

                    if (shouldUpload) {
                        if (!state.readOnly) {
                            initialUpload(ri);
                            return;
                        }
                        context.statusMessage("You don't have permission to update remote files.");
                    }
                    initialManageConflicts(ri);
                } catch (Exception e) {
                    API.uploadCrash(context, e);
                    context.errorMessage("There was a critical error in the plugin " + e.ToString());
                    context.shutdown();
                }
            });
        }

        void _on_get_buf(JObject obj) {
            GetBufResponse res = obj.ToObject<GetBufResponse>();
            Buf b = state.bufs[res.id];
            editor.queue(b, new RunLaterAction<Buf>(delegate(Buf dbuf) {
                dbuf.set(res.buf, res.md5);
                dbuf.write();
                Flog.info("on get buffed. %s", dbuf.path);
            }));
        }

        public void on_data(string name, JObject obj) {
            Events ev;

            try {
                ev = (Events)Enum.Parse(typeof(Events), name);
            } catch (ArgumentException e) {
                Flog.log("No enum for %s", name);
                return;
            }
            switch (ev) {
                case Events.room_info:
                    _on_room_info(obj);
                    break;
                case Events.get_buf:
                    _on_get_buf(obj);
                    break;
                case Events.patch:
                    _on_patch(obj);
                    break;
                case Events.highlight:
                    _on_highlight(obj);
                    break;
                case Events.saved:
                    _on_saved(obj);
                    break;
                case Events.join:
                    _on_join(obj);
                    break;
                case Events.part:
                    _on_part(obj);
                    break;
                case Events.create_buf:
                    _on_create_buf(obj);
                    break;
                case Events.request_perms:
                    _on_request_perms(obj);
                    break;
                case Events.msg:
                    _on_msg(obj);
                    break;
                case Events.rename_buf:
                    _on_rename_buf(obj);
                    break;
                case Events.term_stdin:
                    _on_term_stdin(obj);
                    break;
                case Events.term_stdout:
                    _on_term_stdout(obj);
                    break;
                case Events.delete_buf:
                    _on_delete_buf(obj);
                    break;
                case Events.perms:
                    _on_perms(obj);
                    break;
                case Events.ping:
                    _on_ping(obj);
                    break;
                case Events.ack:
                    break;
                default:
                    Flog.log("No handler for %s", name);
                    break;
            }
        }
    }
}
