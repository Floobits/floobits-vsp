using System;
using System.Collections.Generic;
using Floobits.Common.Interfaces;
using Floobits.Common.Protocol.Buf;
using Floobits.Common.Protocol.Handlers;
using Floobits.Utilities;

namespace Floobits.Common
{
    public class EditorEventHandler
    {
        private IContext context;
        public FloobitsState state;
        private OutboundRequestHandler outbound;
        private InboundRequestHandler inbound;

        public EditorEventHandler(IContext context, FloobitsState state, OutboundRequestHandler outbound, InboundRequestHandler inbound)
        {
            this.context = context;
            this.state = state;
            this.outbound = outbound;
            this.inbound = inbound;
        }

        public void createFile(IFile virtualFile)
        {
            if (context.isIgnored(virtualFile))
            {
                return;
            }
            context.setTimeout(100, delegate
            {
                FlooHandler flooHandler = context.getFlooHandler();
                if (flooHandler == null)
                {
                    return;
                }
                flooHandler.editorEventHandler.upload(virtualFile);
            });
        }

        public void go()
        {
            context.listenToEditor(this);
        }

        public void rename(String path, String newPath)
        {
            if (!state.can("patch"))
            {
                return;
            }
            Flog.log("Renamed buf: {0} - {1}", path, newPath);
            Buf buf = state.get_buf_by_path(path);
            if (buf == null)
            {
                Flog.info("buf does not exist.");
                return;
            }
            string newRelativePath = context.toProjectRelPath(newPath);
            if (newRelativePath == null)
            {
                Flog.warn(String.Format("{0} was moved to {1}, deleting from workspace.", buf.path, newPath));
                outbound.deleteBuf(buf, true);
                return;
            }
            if (buf.path.Equals(newRelativePath))
            {
                Flog.info("rename handling workspace rename, aborting.");
                return;
            }
            outbound.renameBuf(buf, newRelativePath);
        }

        public void change(IFile file)
        {
            string filePath = file.getPath();
            if (!state.can("patch"))
            {
                return;
            }
            if (!context.isShared(filePath))
            {
                return;
            }
            state.pauseFollowing(true);
            Buf buf = state.get_buf_by_path(filePath);
            if (buf == null)
            {
                return;
            }
            lock (buf)
            {
                if (Buf.isBad(buf))
                {
                    Flog.info("buf isn't populated yet %s", file.getPath());
                    return;
                }
                buf.send_patch(file);
            }
        }

        public void changeSelection(string path, List<List<int>> textRanges, bool following)
        {
            Buf buf = state.get_buf_by_path(path);
            outbound.highlight(buf, textRanges, false, following);
        }

        public void save(string path)
        {
            Buf buf = state.get_buf_by_path(path);
            outbound.saveBuf(buf);
        }

        public void softDelete(HashSet<string> files)
        {
            if (!state.can("patch"))
            {
                return;
            }

            foreach (string path in files)
            {
                Buf buf = state.get_buf_by_path(path);
                if (buf == null)
                {
                    context.warnMessage(String.Format("The file, {0}, is not in the workspace.", path));
                    continue;
                }
                outbound.deleteBuf(buf, false);
            }
        }

        void delete(string path)
        {
            Buf buf = state.get_buf_by_path(path);
            if (buf == null)
            {
                return;
            }
            outbound.deleteBuf(buf, true);
        }

        public void deleteDirectory(List<string> filePaths)
        {
            if (!state.can("patch"))
            {
                return;
            }

            foreach (string filePath in filePaths)
            {
                delete(filePath);
            }
        }

        public void msg(String chatContents)
        {
            outbound.message(chatContents);
        }

        public void kick(int userId)
        {
            outbound.kick(userId);
        }

        public void changePerms(int userId, String[] perms)
        {
            outbound.setPerms("set", userId, perms);
        }

        public void upload(IFile virtualFile)
        {
            if (state.readOnly)
            {
                return;
            }
            if (!virtualFile.isValid())
            {
                return;
            }
            string path = virtualFile.getPath();
            Buf b = state.get_buf_by_path(path);
            if (b != null)
            {
                Flog.info("Already in workspace: {0}", path);
                return;
            }
            outbound.createBuf(virtualFile);
        }

        public bool follow()
        {
            bool mode = !state.getFollowing();
            state.setFollowing(mode);
            context.statusMessage(String.Format("{0} follow mode", mode ? "Enabling" : "Disabling")); ;
            if (mode && state.lastHighlight != null)
            {
                inbound._on_highlight(state.lastHighlight);
            }
            return mode;
        }

        public void summon(string path, int offset)
        {
            outbound.summon(path, offset);
        }

        public void sendEditRequest()
        {
            outbound.requestEdit();
        }

        public void clearHighlights()
        {
            context.iFactory.clearHighlights();
        }

        public void openChat()
        {
            Flog.info("Showing user window.");
            context.openChat();
        }
        public void openInBrowser()
        {
#if LATER
            if(!Desktop.isDesktopSupported()) {
                context.statusMessage("This version of java lacks to support to open your browser.");
                return;
            }
            try {
                Desktop.getDesktop().browse(new URI(state.url.toString()));
            } catch (IOException error) {
                Flog.warn(error);
            } catch (URISyntaxException error) {
                Flog.warn(error);
            }
#endif
        }

        public void beforeChange(IDoc doc)
        {
            IFile virtualFile = doc.getVirtualFile();
            string path = virtualFile.getPath();
            Buf bufByPath = state.get_buf_by_path(path);
            if (bufByPath == null)
            {
                return;
            }
            string msg;
            if (state.readOnly)
            {
                msg = "This document is readonly because you don't have edit permission in the workspace.";
            }
            else if (!bufByPath.isPopulated())
            {
                msg = "This document is temporarily readonly while we fetch a fresh copy.";
            }
            else
            {
                return;
            }
            context.statusMessage(msg);
            doc.setReadOnly(true);
            context.iFactory.readOnlyBufferIds.Add(bufByPath.path);
            string text = doc.getText();
            context.setTimeout(0, delegate
            {
                context.writeThread(delegate
                {
                    if (!state.readOnly && bufByPath.isPopulated())
                    {
                        return;
                    }
                    lock (context)
                    {
                        try
                        {
                            context.setListener(false);
                            IDoc d = context.iFactory.getDocument(virtualFile);
                            if (d == null)
                            {
                                return;
                            }
                            d.setReadOnly(false);
                            d.setText(text);
                            d.setReadOnly(true);
                        }
                        catch (Exception e)
                        {
                            Flog.warn(e.ToString());
                        }
                        finally
                        {
                            context.setListener(true);
                        }
                    }
                });
            });
        }
    }
}
