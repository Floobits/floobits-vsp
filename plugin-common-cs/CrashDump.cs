using System;
using System.Collections.Generic;

namespace Floobits.Common
{
    [Serializable]
    public class CrashDump {
        public string owner;
        public string workspace;
        public string dir;
        public string subject;
        public string username;
        public static string userAgent;
        public Dictionary<string, string> message = new Dictionary<string, string>();
        private static string editor = "";

        public static void setUA(string userAgent, string editor) {
            CrashDump.userAgent = userAgent;
            CrashDump.editor = editor;
        }
        public CrashDump(Exception e, string owner, string workspace, string dir, string username) {
            this.owner = owner;
            this.workspace = workspace;
            this.dir = dir;
            this.username = username;
            message.Add("sendingAt", DateTime.Now.Subtract(DateTime.MinValue.AddYears(1969)).TotalMilliseconds.ToString());
            message.Add("stack", e.StackTrace);
            message.Add("description", e.Message);
            setContextInfo("{0} died{1}!");
        }

        public CrashDump(string description, string username) {
            this.username = username;
            message.Add("sendingAt", DateTime.Now.Subtract(DateTime.MinValue.AddYears(1969)).TotalMilliseconds.ToString());
            message.Add("description", description);
            setContextInfo("{0} submitted an issues{1}!");
        }

        protected void setContextInfo(string subjectText) {
            subject = String.Format(subjectText, editor, username != null ? " for " + username : "");
        }
    }
}
