using System;
using System.Collections.Generic;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using EnvDTE;
using EnvDTE80;
using Floobits.Common;
using Floobits.Common.Protocol;
using Floobits.Common.Interfaces;

namespace Floobits.floobits_vsp
{
    public class VSPContext : IContext
    {
        private DTE2 package_dte;
        private OutputWindow ow;
        private OutputWindowPane owP;

        public VSPContext()
        {
            this.iFactory = new VSPFactory(this);
        }

        public void Initialize(DTE2 dte)
        {
            package_dte = dte;
            // Create a tool window reference for the Output window
            // and window pane.
            ow = dte.ToolWindows.OutputWindow;
            // Add a new pane to the Output window.
            owP = ow.OutputWindowPanes.Add("Floobits");
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

        }

        public override void chatErrorMessage(string message)
        {

        }

        public override void chat(string username, string msg, DateTime messageDate)
        {

        }

        public override void openChat()
        {

        }

        public override void listenToEditor(EditorEventHandler editorEventHandler)
        {

        }

        public override void setUsers(Dictionary<int, FlooUser> users)
        {

        }

        public override void setListener(bool b)
        {

        }

        public override void mainThread(Action runnable)
        {

        }

        public override void readThread(Action runnable)
        {

        }

        public override void writeThread(Action runnable)
        {

        }

        public override void dialog(string title, string body, RunLater<bool> runLater)
        {

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