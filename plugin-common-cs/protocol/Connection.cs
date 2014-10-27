using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.Linq;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Floobits.Common;
using Floobits.Common.Protocol.Handlers;
using Floobits.Common.Interfaces;
using Floobits.Utilities;
using Floobits.Common.Client;


namespace Floobits.Common.Protocol
{
    public class Connection
    {
        private BaseHandler handler;
        private IContext context;

        private int MAX_RETRIES = 20;
        private int INITIAL_RECONNECT_DELAY = 500;
        private int CONNECT_TIMEOUT_MILLIS = 15000;
        protected volatile int retries;
        protected int delay;
        protected AsynchronousClient channel;

        public Connection(BaseHandler handler) {
            this.handler = handler;
            this.context = handler.context;
            this.retries = MAX_RETRIES;
            this.delay = INITIAL_RECONNECT_DELAY;
        }

        public void setRetries(int retries) {
            this.retries = retries;
        }
        public void start() {
            connect();
        }

        public void write(Object obj) {
            if (channel == null) {
                Flog.warn("not writing because not connected");
                return;
            }
            string data = JsonConvert.SerializeObject(obj);
            channel.Send(data + "\n");
        }

        protected void _connect() {
            retries -= 1;

            FlooUrl flooUrl = handler.getUrl();
            try {
                channel = new AsynchronousClient(
                    flooUrl.host, flooUrl.port, CONNECT_TIMEOUT_MILLIS,
                    Flog.debug, reader);
                channel.Connect();
                if (!channel.isConnected())
                {
                    channel = null;
                    context.errorMessage("Can not connect to floobits!");
                    context.shutdown();
                }
            }   catch (Exception e) {
                Flog.warn(e.ToString());
                reconnect();
            }
        }

        protected void connect() {
            if (retries <= 0) {
                Flog.warn("I give up connecting.");
                return;
            }
            if (channel == null) {
                _connect();
                return;
            }
            try {
                channel.Shutdown();
            } catch (Exception e) {
                Flog.warn(e.ToString());
                reconnect();
            }
        }

        protected void reconnect() {
            if (retries <= 0) {
                Flog.log("Giving up!");
                context.shutdown();
                return;
            }
            delay = (int)Math.Min(10000, Math.Round((float) 1.5 * delay));
            Flog.log("Connection lost. Reconnecting in %sms", delay);
            context.setTimeout(delay, delegate {
                Flog.log("Attempting to reconnect.");
                connect();
            });
        }

        public void shutdown() {
            retries = -1;
            if (channel != null) {
                try {
                    channel.Shutdown();
                } catch (Exception e) {
                    Flog.warn(e.ToString());
                }
                channel = null;
            }
        }

        public void channelActive()
        {
            Flog.log("Connected to %s", channel);
            handler.on_connect();
        }

        public void reader(string msg)
        {
            JObject obj = JObject.Parse(msg);
            JToken name;
            if (!obj.TryGetValue("name", out name)) {
                Flog.warn("No name for receive, ignoring");
                return;
            }
            string requestName = name.Value<string>();
            retries = MAX_RETRIES;
            delay = INITIAL_RECONNECT_DELAY;
            handler.on_data(requestName, obj);
        }

        public void channelUnregistered() {
            Flog.log("Disconnected from %s", channel);
            reconnect();
        }
    }
}