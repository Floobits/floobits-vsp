using Newtonsoft.Json.Linq;
using Floobits.Common.Protocol;
using Floobits.Common;
using Floobits.Common.Interfaces;
using Floobits.Utilities;


namespace Floobits.Common.Protocol.Handlers
{
    abstract public class BaseHandler
    {
        public FlooUrl url;
        public bool isJoined = false;
        protected Connection conn;
        public IContext context;
        public OutboundRequestHandler outbound;

        public BaseHandler(IContext context)
        {
            this.context = context;
        }

        void _on_error(JObject jsonObject)
        {
            string reason = jsonObject.GetValue("msg").ToString();
            reason = string.Format("Floobits Error: {0}", reason);
            Flog.warn(reason);
            if (jsonObject["flash"] != null && jsonObject["flash"].Value<bool>())
            {
                context.errorMessage(reason);
                context.flashMessage(reason);
            }
        }

        void _on_disconnect(JObject jsonObject)
        {
            string reason = jsonObject.GetValue("reason").ToString();
            if (reason != null)
            {
                context.errorMessage(string.Format("You have been disconnected from the workspace because {0}", reason));
                context.flashMessage("You have been disconnected from the workspace.");
            }
            else
            {
                context.statusMessage("You have left the workspace");
            }
            context.shutdown();
        }

        protected abstract void _on_data(string name, JObject obj);

        public void on_data(string name, JObject obj)
        {
            if (name.Equals("error"))
            {
                _on_error(obj);
                return;
            }
            if (name.Equals("disconnect"))
            {
                _on_disconnect(obj);
                return;
            }
            _on_data(name, obj);
        }

        public abstract void on_connect();

        public FlooUrl getUrl()
        {
            return url;
        }

        public void go()
        {
            isJoined = true;
        }

        public void shutdown()
        {
            if (conn != null)
            {
                conn.shutdown();
                conn = null;
            }
            isJoined = false;
        }
    }
}
