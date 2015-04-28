using System;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Floobits.Common.Protocol;
using Floobits.Common.Protocol.Handlers;
using Floobits.Utilities;

namespace Floobits.Common.Interfaces
{
    public abstract class IContext
    {
        public EditorScheduler editor;
        private ReaderWriterLockSlim rwlock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        public string colabDir;
        public volatile BaseHandler handler;
        public DateTime lastChatMessage;
        public IFactory iFactory;
        protected Ignore ignoreTree;

        public IContext()
        {
            editor = new EditorScheduler(this);
        }

#if LATER
        public bool addGroup(Bootstrap b) {
            bool b1 = false;
            try {
                rwlock.EnterReadLock();
                if (loopGroup != null) {
                    b.group(loopGroup);
                    b1 = true;
                }
            } finally {
                rwlock.ExitReadLock();
            }
            return b1;
            return true;
        }
#endif

        public Timer setTimeout(int time, TimerCallback runnable)
        {
            return new Timer(runnable, null, time, Timeout.Infinite);
        }

        private bool changePerms(FlooUrl flooUrl, string[] newPerms)
        {
            HTTPWorkspaceRequest workspace = API.getWorkspace(flooUrl, this);
            if (workspace == null)
            {
                return false;
            }

            string[] anonPerms = workspace.perms["AnonymousUser"];
            if (anonPerms == null)
            {
                anonPerms = new String[] { };
            }
            Array.Sort(anonPerms);
            Array.Sort(newPerms);
            if (!Array.Equals(anonPerms, newPerms))
            {
                workspace.perms["AnonymousUser"] = newPerms;
                return API.updateWorkspace(flooUrl, workspace, this);
            }
            return true;
        }

        public void shareProject(bool _private_, string projectPath)
        {
            FlooUrl flooUrl = DotFloo.read(projectPath);

            string[] newPerms = _private_ ? new string[] { } : new string[] { "view_room" };

            if (flooUrl != null && changePerms(flooUrl, newPerms))
            {
                joinWorkspace(flooUrl, projectPath, true);
                return;
            }

            PersistentJson persistentJson = PersistentJson.getInstance();
            foreach (Dictionary<string, Workspace> i in persistentJson.workspaces.Values)
            {
                foreach (Workspace w in i.Values)
                {
                    if (Utils.isSamePath(w.path, projectPath))
                    {
                        try
                        {
                            flooUrl = new FlooUrl(w.url);
                        }
                        catch (UriFormatException e)
                        {
                            Flog.warn(e.ToString());
                            continue;
                        }
                        if (changePerms(flooUrl, newPerms))
                        {
                            joinWorkspace(flooUrl, w.path, true);
                            return;
                        }
                    }
                }
            }

            string host;
            FloorcJson floorcJson;
            try
            {
                floorcJson = Settings.get();
            }
            catch (Exception)
            {
                Flog.log("Invalid .floorc.json");
                statusMessage("Invalid .floorc.json");
                return;
            }
            int size = floorcJson.auth.Count;
            if (size <= 0)
            {
                Flog.log("No credentials.");
                return;
            }
            string[] keys = floorcJson.auth.Keys.ToArray<string>();
            if (keys.Length == 1)
            {
                host = keys[0];
            }
            else
            {
                host = selectAccount(keys);
            }

            if (host == null)
            {
                return;
            }

            string owner = floorcJson.auth[host]["username"];
            string name = Path.GetFileName(projectPath);
            LinkedList<string> orgs = API.getOrgsCanAdmin(host, this);
            orgs.AddFirst(owner);
            shareProjectDialog(name, orgs, host, _private_, projectPath);
        }

        protected abstract void shareProjectDialog(string name, LinkedList<string> orgs, string host, bool _private_, string projectPath);

        public void joinWorkspace(FlooUrl flooUrl, string path, bool upload)
        {
            Flog.log("Joining workspace.");
            FloorcJson floorcJson = null;
            try
            {
                floorcJson = Settings.get();
            }
            catch (Exception)
            {
                statusMessage("Invalid JSON in your .floorc.json.");
            }

            Dictionary<string, string> auth = null;
            if (floorcJson != null)
            {
                floorcJson.auth.TryGetValue(flooUrl.host, out auth);
            }
            if (auth == null)
            {
#if LATER
                setupHandler(new LinkEditorHandler(this, flooUrl.host, new Runnable() {
                    @Override
                    public void run() {
                        joinWorkspace(flooUrl, path, upload);
                    }
                }));
#endif
                return;
            }

            if (!API.workspaceExists(flooUrl, this))
            {
                errorMessage(String.Format("The workspace {0} does not exist!", flooUrl.ToString()));
                return;
            }

            if (setupHandler(new FlooHandler(this, flooUrl, upload, path, auth)))
            {
                setListener(true);
                return;
            }

            string title = String.Format("Really leave {0}?", handler.url.workspace);
            string body = String.Format("Leave {0} and join {1} ?", handler.url.ToString(), handler.url.ToString());

            dialog(title, body, new RunLaterAction<bool>(delegate(bool join)
            {
                if (!join)
                {
                    return;
                }
                shutdown();
                joinWorkspace(flooUrl, path, upload);
            }));
        }

        public void createAccount()
        {
#if LATER
            if (setupHandler(new CreateAccountHandler(this, Constants.defaultHost))) {
                return;
            }
            statusMessage("You already have an account and are connected with it.");
            shutdown();
#endif
        }

        public void linkEditor()
        {
#if LATER
            if (setupHandler(new LinkEditorHandler(this, Constants.defaultHost))) {
                return;
            }
            statusMessage("You already have an account and are connected with it.");
            shutdown();
#endif
        }

        private bool setupHandler(BaseHandler handler)
        {
            if (isJoined())
            {
                return false;
            }

            rwlock.EnterWriteLock();
            this.handler = handler;
            //loopGroup = new NioEventLoopGroup();
            rwlock.ExitWriteLock();
            handler.go();
            return true;
        }

        public bool isJoined()
        {
            return handler != null && handler.isJoined;
        }

        public FlooHandler getFlooHandler()
        {
            return handler as FlooHandler;
        }

        public void setColabDir(string colabDir)
        {
            this.colabDir = colabDir;
            Ignore.writeDefaultIgnores(this);
            refreshIgnores();
        }

        public void refreshIgnores()
        {
#if LATER
            IFile path = iFactory.findFileByPath(colabDir);
            ignoreTree = Ignore.BuildIgnore(path);
#endif
        }

        public String absPath(String path)
        {
            return Utils.absPath(colabDir, path);
        }

        public Boolean isShared(String path)
        {
            return Utils.isShared(path, colabDir);
        }

        public String toProjectRelPath(String path)
        {
            return Utils.toProjectRelPath(path, colabDir);
        }

        public Boolean isIgnored(IFile f)
        {
            String path = f.getPath();

            if (!isShared(path))
            {
                Flog.log("Ignoring %s because it isn't shared.", path);
                return true;
            }

            return ignoreTree.isIgnored(f, path, toProjectRelPath(path), f.isDirectory());
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void shutdown()
        {
            rwlock.EnterWriteLock();
            try
            {
                if (handler != null)
                {
                    handler.shutdown();
                    editor.shutdown();
                    statusMessage("Disconnecting.");
                    handler = null;
                }
                if (iFactory != null)
                {
                    iFactory.clearReadOnlyState();
                }
#if LATER
                if (loopGroup != null) {
                    try {
                        loopGroup.shutdownGracefully(0, 500, TimeUnit.MILLISECONDS);
                    } catch (Throwable e) {
                        Flog.warn(e);
                    } finally {
                        loopGroup = null;
                    }
                }
#endif
                ignoreTree = null;
            }
            finally
            {
                rwlock.ExitWriteLock();
            }
        }

        protected abstract string selectAccount(string[] keys);
        public Ignore getIgnoreTree()
        {
            return ignoreTree;
        }
        public abstract Object getActualContext();
        public abstract void loadChatManager();
        public abstract void flashMessage(string message);
        public abstract void warnMessage(string message);
        public abstract void statusMessage(string message);
        public abstract void errorMessage(string message);
        public abstract void chatStatusMessage(string message);
        public abstract void chatErrorMessage(string message);
        public abstract void chat(string username, string msg, DateTime messageDate);
        public abstract void openChat();
        public abstract void listenToEditor(EditorEventHandler editorEventHandler);
        public abstract void setUsers(Dictionary<int, FlooUser> users);
        public abstract void setListener(bool b);
        public abstract void mainThread(Action runnable);
        public abstract void readThread(Action runnable);
        public abstract void writeThread(Action runnable);
        public abstract void dialog(string title, string body, RunLater<bool> runLater);
        public abstract void dialogDisconnect(int tooMuch, int howMany);
        public abstract void dialogPermsRequest(string username, RunLater<string> perms);
        public abstract bool dialogTooBig(LinkedList<Ignore> tooBigIgnores);
        public abstract void dialogResolveConflicts(Action stompLocal, Action stompRemote, bool readOnly, Action flee, string[] conflictedPathsArray);
    }
}
