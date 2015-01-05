using Newtonsoft.Json.Linq;
using Floobits.Common;
using Floobits.Common.Interfaces;
using Floobits.Common.Protocol;
using Floobits.Common.Protocol.Json.Send;
using Floobits.Utilities;
using System;
using System.Collections.Generic;

namespace Floobits.Common.Protocol.Handlers
{
    public class FlooHandler : BaseHandler
    {
        private Dictionary<string, string> auth;
        private bool shouldUpload;
        public FloobitsState state;
        InboundRequestHandler inbound;
        public EditorEventHandler editorEventHandler;

        public FlooHandler(IContext context, FlooUrl flooUrl, bool shouldUpload, string path, Dictionary<string, string> auth)
            : base(context)
        {
            this.auth = auth;
            this.shouldUpload = shouldUpload;
            context.setColabDir(Utils.unFuckPath(path));
            url = flooUrl;
            state = new FloobitsState(context, flooUrl);
            state.username = auth["username"];
        }

        public override void go()
        {
            base.go();
            Flog.log("joining workspace {0}", url);
            conn = new Connection(this);
            outbound = new OutboundRequestHandler(context, state, conn);
            inbound = new InboundRequestHandler(context, state, outbound, shouldUpload);
            editorEventHandler = new EditorEventHandler(context, state, outbound, inbound);
            //        if (ProjectRootManager.getInstance(context.project).getProjectSdk() == null) {
            //            Flog.warn("No SDK detected.");
            //        }
            PersistentJson persistentJson = PersistentJson.getInstance();
            persistentJson.addWorkspace(url, context.colabDir);
            persistentJson.save();
            conn.start();
            editorEventHandler.go();
        }

        public override void on_connect()
        {
            context.editor.reset();
            context.statusMessage(string.Format("Connecting to {0}.", url));

            string username = null, api_key = null, secret = null;
            auth.TryGetValue("username", out username);
            auth.TryGetValue("api_key", out api_key);
            auth.TryGetValue("secret", out secret);

            conn.write(new FlooAuth(username, api_key, secret, url.owner, url.workspace));
        }

        protected override void _on_data(String name, JObject obj)
        {
            Flog.debug("Calling {0}", name);
            try
            {
                inbound.on_data(name, obj);
            }
            catch (Exception e)
            {
                Flog.warn(string.Format("on_data error \n\n{0}", e.ToString()));
                API.uploadCrash(this, context, e);
            }
        }

        public override void shutdown()
        {
            base.shutdown();
            context.statusMessage(string.Format("Leaving workspace: {0}.", url.ToString()));
            state.shutdown();
        }
    }
}