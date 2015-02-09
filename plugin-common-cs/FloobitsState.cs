using System.Threading;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Floobits.Common.Protocol.Buf;
using Floobits.Common.Interfaces;
using Floobits.Common.Protocol;
using Floobits.Common.Protocol.Json.Send;
using Floobits.Utilities;

namespace Floobits.Common
{
    public class FloobitsState
    {
        public JObject lastHighlight;
        public bool following = false;
        private Timer pausedFollowing;
        public HashSet<string> perms = new HashSet<string>();
        private Dictionary<int, FlooUser> users = new Dictionary<int, FlooUser>();
        public Dictionary<int, Buf> bufs = new Dictionary<int, Buf>();
        public Dictionary<string, int> paths_to_ids = new Dictionary<string, int>();
        private int connectionId;

        public bool readOnly = false;
        public string username = "";
        protected HashSet<int> requests = new HashSet<int>();
        private IContext context;
        public FlooUrl url;

        public FloobitsState(IContext context, FlooUrl flooUrl)
        {
            this.context = context;
            url = flooUrl;
        }

        public bool can(string perm)
        {
            if (!context.isJoined())
                return false;

            if (!perms.Contains(perm))
            {
                Flog.info("we can't do that because perms");
                return false;
            }
            return true;
        }

        public void handleRoomInfo(RoomInfoResponse ri)
        {
            users = ri.users;
            context.setUsers(users);
            perms = new HashSet<string>(ri.perms);
            if (!can("patch"))
            {
                readOnly = true;
                context.statusMessage("You don't have permission to edit files in this workspace.  All documents will be set to read-only.");
            }
            connectionId = int.Parse(ri.user_id);
            Flog.info("Got roominfo with userId {0}", connectionId);

        }
        public void set_buf_path(Buf buf, string newPath)
        {
            paths_to_ids.Remove(buf.path);
            buf.path = newPath;
            paths_to_ids.Add(buf.path, buf.id.Value);
        }

        public Buf get_buf_by_path(string absPath)
        {
            string relPath = context.toProjectRelPath(absPath);
            if (relPath == null)
            {
                return null;
            }
            int id;
            if (paths_to_ids.TryGetValue(FilenameUtils.separatorsToUnix(relPath), out id))
            {
                return null;
            }
            return bufs[id];
        }

        public string getUsername(int userId)
        {
            FlooUser user;
            if (!users.TryGetValue(userId, out user))
            {
                return "";
            }
            return user.username;
        }

        /**
         * Get a user by their connection id (userId).
         * @param userId
         * @return null or the FlooUser object for the connection id.
         */
        public FlooUser getUser(int userId)
        {
            FlooUser u = null;
            users.TryGetValue(userId, out u);
            return u;
        }

        public void addUser(FlooUser flooser)
        {
            users.Add(flooser.user_id, flooser);
            context.statusMessage(string.Format("{0} joined the workspace on {1} ({2}).", flooser.username, flooser.platform, flooser.client));
            context.setUsers(users);
        }

        public void removeUser(int userId)
        {
            FlooUser u = getUser(userId);
            if (users.Remove(userId))
            {
                context.setUsers(users);
                context.statusMessage(string.Format("{0} left the workspace.", u.username));
            }
        }

        public int getMyConnectionId()
        {
            return connectionId;
        }

        public void changePermsForUser(int userId, string[] permissions)
        {
            FlooUser user = getUser(userId);
            if (user == null)
            {
                return;
            }
            List<string> givenPerms = new List<string>(permissions); // maybe use a hash set here?
            HashSet<string> translatedPermsSet = new HashSet<string>();
            Dictionary<string, string[]> permTypes = new Dictionary<string, string[]>();
            permTypes.Add("edit_room", new string[]{
                    "patch", "get_buf", "set_buf", "create_buf", "delete_buf", "rename_buf", "set_temp_data", "delete_temp_data",
                    "highlight", "msg", "datamsg", "create_term", "term_stdin", "delete_term", "update_term", "term_stdout", "saved"
            });
            permTypes.Add("view_room", new string[] { "get_buf", "ping", "pong" });
            permTypes.Add("request_perms", new string[] { "get_buf", "request_perms" });
            permTypes.Add("admin_room", new string[] { "kick", "pull_repo", "perms" });
            foreach (KeyValuePair<string, string[]> entry in permTypes)
            {
                if (givenPerms.Contains(entry.Key))
                {
                    translatedPermsSet.UnionWith(entry.Value);
                }
            }
            translatedPermsSet.CopyTo(user.perms);
        }

        public void shutdown()
        {
            bufs = null;
        }

        public bool getFollowing()
        {
            return following;
        }

        public void setFollowing(bool following)
        {
            this.pauseFollowing(false);
            this.following = following;
        }

        public bool getPausedFollowing()
        {
            return pausedFollowing != null;
        }

        public void pauseFollowing(bool pause)
        {
            /*
            Possible states:
                Not following, not paused.
                Following, not paused.
                Following, paused.

            Impossible state:
                Not following, paused. Paused means following, just not active. Anytime we set follow, we must cancel any pause.
             */
            if (this.pausedFollowing != null)
            {
                following = true;
                this.pausedFollowing.Dispose();
            }
            this.pausedFollowing = null;
            if (pause)
            {
                if (!following)
                {
                    return;
                }
                following = false;
                this.pausedFollowing = context.setTimeout(2000, delegate
                {
                    pauseFollowing(false);
                });
            }
        }
    }
}