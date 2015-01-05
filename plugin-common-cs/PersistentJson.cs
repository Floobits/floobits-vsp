using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Floobits.Utilities;

namespace Floobits.Common
{
    public class PersistentJson
    {
        public Dictionary<string, Dictionary<string, Workspace>> workspaces = new Dictionary<string, Dictionary<string, Workspace>>();
        public bool auto_generated_account = false;
        public bool disable_account_creation = false;
        public LinkedList<Workspace> recent_workspaces = new LinkedList<Workspace>();

        public static void removeWorkspace(FlooUrl flooUrl)
        {
            PersistentJson persistentJson = getInstance();
            Dictionary<string, Workspace> workspaces;
            if (persistentJson.workspaces.TryGetValue(flooUrl.owner, out workspaces))
            {
                workspaces.Remove(flooUrl.workspace);
            }
            Uri uri = new Uri(flooUrl.ToString());

            LinkedListNode<Workspace> recent_workspace = persistentJson.recent_workspaces.First;
            while (recent_workspace != null)
            {
                Uri normalizedURL = new Uri(recent_workspace.Value.url);
                bool del = (uri.AbsolutePath.Equals(normalizedURL.AbsolutePath) && uri.Host.Equals(normalizedURL.Host));
                LinkedListNode<Workspace> old = recent_workspace;
                recent_workspace = old.Next;
                if (del)
                {
                    persistentJson.recent_workspaces.Remove(old);
                }
            }
            persistentJson.save();
        }

        public void addWorkspace(FlooUrl flooUrl, string path)
        {
            Dictionary<string, Workspace> workspaces;
            if (!this.workspaces.TryGetValue(flooUrl.owner, out workspaces))
            {
                workspaces = new Dictionary<string, Workspace>();
                this.workspaces[flooUrl.owner] = workspaces;
            }
            Workspace workspace;
            if (!workspaces.TryGetValue(flooUrl.workspace, out workspace))
            {
                workspace = new Workspace(flooUrl.ToString(), path);
                workspaces[flooUrl.workspace] = workspace;
            }
            else
            {
                workspace.path = path;
                workspace.url = flooUrl.ToString();
            }
            this.recent_workspaces.AddFirst(workspace);
            HashSet<String> seen = new HashSet<String>();
            LinkedList<Workspace> unique = new LinkedList<Workspace>();
            foreach (Workspace w in this.recent_workspaces)
            {
                w.clean();
                if (seen.Contains(w.url))
                {
                    continue;
                }
                seen.Add(w.url);
                unique.AddLast(w);
            }
            this.recent_workspaces = unique;
        }

        public void save()
        {
            string s = JsonConvert.SerializeObject(this, Formatting.Indented);
            try
            {
                File.WriteAllText(getFile(), s, Encoding.UTF8);
            }
            catch (Exception e)
            {
                Flog.warn(e.ToString());
            }
        }

        public static string getFile()
        {
            return FilenameUtils.concat(Constants.baseDir, "persistent.json");
        }

        public static PersistentJson getInstance()
        {
            string s;
            string defaultJSON = "{}";
            try
            {
                s = File.ReadAllText(getFile(), Encoding.UTF8);
            }
            catch (Exception)
            {
                Flog.info("Couldn't find persistent.json");
                s = defaultJSON;
            }
            PersistentJson pj;
            try
            {
                pj = JsonConvert.DeserializeObject<PersistentJson>(s);
            }
            catch (Exception)
            {
                Flog.warn("Bad JSON in persistent json");
                pj = JsonConvert.DeserializeObject<PersistentJson>(s);
            }
            return pj;
        }
    }
}
