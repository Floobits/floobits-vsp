using System;
using SysTasks = System.Threading.Tasks;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using EnvDTE;
using EnvDTE80;
using Floobits.Common;
using Floobits.Common.Protocol;
using Floobits.Common.Interfaces;

namespace Floobits.floobits_vsp
{
    public interface VSPContextContainer
    {
        IContext GetIContext();
        VSPContext GetVSPContext();
        VSPFactory GetVSPFactory();
        void Initialize(Package package, DTE2 dte);
    }

    [Export(typeof(VSPContextContainer))]
    public class VSPContext : IContext, VSPContextContainer
    {
        private DTE2 package_dte;
        private Package package;
        private OutputWindow ow;
        private OutputWindowPane owP;
        private FlooChatWindow chat_window;
        private FlooTreeWindow tree_window;

        public VSPContext()
        {
            this.iFactory = new VSPFactory(this);
        }

        public void Initialize(Package package, DTE2 dte)
        {
            this.package = package;
            package_dte = dte;
            // Create a tool window reference for the Output window
            // and window pane.
            ow = dte.ToolWindows.OutputWindow;
            // Create a tool window reference for the chat and tree window
            chat_window = (FlooChatWindow)package.FindToolWindow(typeof(FlooChatWindow), 0, true);
            tree_window = (FlooTreeWindow)package.FindToolWindow(typeof(FlooTreeWindow), 0, true);
            // Add a new pane to the Output window.
            owP = ow.OutputWindowPanes.Add("Floobits");

            tree_window.control.TreeView.Items.Add("Horse");
        }

        public IContext GetIContext()
        {
            return this;
        }

        public VSPContext GetVSPContext()
        {
            return this;
        }

        public VSPFactory GetVSPFactory()
        {
            return this.iFactory as VSPFactory;
        }

        public void outputWindowMessage(string message)
        {
            // Add a line of text to the new pane.
            owP.OutputString(message + "\r\n");
        }

        public override void flashMessage(string message)
        {
            // Add a line of text to the new pane.
            owP.OutputString("FLASH : " + message + "\r\n");
        }
        public override void warnMessage(string message)
        {
            // Add a line of text to the new pane.
            owP.OutputString("WARN  : " + message + "\r\n");
        }
        public override void statusMessage(string message)
        {
            // Add a line of text to the new pane.
            owP.OutputString("STATUS: " + message + "\r\n");
        }
        public override void errorMessage(string message)
        {
            // Add a line of text to the new pane.
            owP.OutputString("ERROR : " + message + "\r\n");
        }

        protected override void shareProjectDialog(string name, LinkedList<string> orgs, string host, bool _private_, string projectPath)
        {
            var d = new ShareProject(this, name, orgs, host, _private_, projectPath);
            d.ShowDialog();
        }

        protected override string selectAccount(string[] keys)
        {
            var d = new SelectAccount(keys);
            d.ShowDialog();
            return d.getAccount();
        }

        public override Object getActualContext()
        {
            return null;
        }

        public override void loadChatManager()
        {

        }

        public override void chatStatusMessage(string message)
        {
            mainThread(delegate
                {
                    chat_window.control.ChatDialog.ContentEnd.InsertTextInRun("STATUS: " + message);
                    chat_window.control.ChatDialog.ContentEnd.InsertLineBreak();
                }
            );
        }

        public override void chatErrorMessage(string message)
        {
            mainThread(delegate
                {
                    chat_window.control.ChatDialog.ContentEnd.InsertTextInRun("ERROR: " + message);
                    chat_window.control.ChatDialog.ContentEnd.InsertLineBreak();
                }
            );
        }

        public override void chat(string username, string msg, DateTime messageDate)
        {
            mainThread(delegate
                {
                    chat_window.control.ChatDialog.ContentEnd.InsertTextInRun(username + ": " + msg);
                    chat_window.control.ChatDialog.ContentEnd.InsertLineBreak();
                }
            );
        }

        public override void openChat()
        {
            IVsWindowFrame windowFrame = (IVsWindowFrame)chat_window.Frame;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());
        }

        public override void listenToEditor(EditorEventHandler editorEventHandler)
        {
            // Setup Floobits Tree Window
            tree_window.control.WatchPath(colabDir);
        }

        public override void setUsers(Dictionary<int, FlooUser> users)
        {

        }

        public override void setListener(bool b)
        {

        }

        public override void mainThread(Action runnable)
        {
            /* Runs in the main UI Thread */
            ThreadHelper.Generic.Invoke(runnable);
        }

        public override void readThread(Action runnable)
        {
            SysTasks.Task.Run(runnable);
        }

        public override void writeThread(Action runnable)
        {
            SysTasks.Task.Run(runnable);
        }

        public override void dialog(string title, string body, RunLater<bool> runLater)
        {
            DialogBuilder.build(title, body, runLater);
        }

        public override void dialogDisconnect(int tooMuch, int howMany)
        {

        }

        public override void dialogPermsRequest(string username, RunLater<string> perms)
        {

        }

        public override bool dialogTooBig(LinkedList<Ignore> tooBigIgnores)
        {
            return false;
        }

        public override void dialogResolveConflicts(Action stompLocal, Action stompRemote, bool readOnly, Action flee, string[] conflictedPathsArray)
        {

        }
    }
}