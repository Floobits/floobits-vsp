using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Security;
using System.IO;
using Floobits.Common.Interfaces;
using Floobits.Common.Protocol.Handlers;
using Floobits.Utilities;


namespace Floobits.Common
{
    public class API
    {
        public static int maxErrorReports = 3;
        private static int numSent = 0;

        public static T createFromJsonStream<T>(this Stream stream)
        {
            JsonSerializer serializer = new JsonSerializer();
            T data;
            using (StreamReader streamReader = new StreamReader(stream))
            {
                data = (T)serializer.Deserialize(streamReader, typeof(T));
            }
            return data;
        }

        private static HttpWebRequest genGetRequest(string url)
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.Method = "GET";
            return req;
        }

        private static HttpWebRequest genPostRequest(string url, object serialize_obj)
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.Method = "POST";

            using (var writer = new StreamWriter(req.GetRequestStream()))
            {
                writer.Write(JsonConvert.SerializeObject(serialize_obj));
                writer.Flush();
                writer.Close();
            }

            return req;
        }

        private static HttpWebRequest genPutRequest(string url, object serialize_obj)
        {
            HttpWebRequest req = genPostRequest(url, serialize_obj);
            req.Method = "PUT";
            return req;
        }

        public static bool createWorkspace(string host, string owner, string workspace, IContext context, bool notPublic)
        {
            string path = "/api/workspace";

            HttpWebRequest req = genPostRequest(string.Format("https://{0}{1}", host, path), new HTTPWorkspaceRequest(owner, workspace, notPublic));

            HttpWebResponse resp = apiRequest(req, context, host);

            switch (resp.StatusCode)
            {
                case HttpStatusCode.BadRequest:
                    // Todo: pick a new name or something
                    context.errorMessage("Invalid workspace name (a-zA-Z0-9).");
                    return false;
                case HttpStatusCode.PaymentRequired:
                    string details;
                    try
                    {
                        var reader = new StreamReader(resp.GetResponseStream());
                        dynamic obj = JObject.Parse(reader.ReadToEnd());
                        details = obj["detail"].asString();
                    }
                    catch (IOException e)
                    {
                        Flog.warn(e.ToString());
                        return false;
                    }
                    context.errorMessage(details);
                    return false;
                case HttpStatusCode.Conflict:
                    context.statusMessage("The workspace already exists so I am joining it.");
                    return false; // join here instead
                case HttpStatusCode.Created:
                    context.statusMessage("Workspace created.");
                    return true;
                case HttpStatusCode.Unauthorized:
                    Flog.log("Auth failed");
                    context.errorMessage("There is an invalid username or secret in your ~/.floorc and you were not able to authenticate.");
                    // We could open the file in the editor
                    return false;
                default:
                    try
                    {
                        Flog.warn(string.Format("Unknown error creating workspace:\n{0}", resp.ToString()));
                    }
                    catch (IOException e)
                    {
                        Flog.warn(e.ToString());
                    }
                    return false;
            }
        }

        public static bool updateWorkspace(FlooUrl f, HTTPWorkspaceRequest workspaceRequest, IContext context) {
            HttpWebRequest req = genPutRequest(string.Format("https://{0}/api/workspace/{1}/{2}", f.host, f.owner, f.workspace), workspaceRequest);
            HttpWebResponse resp;
            try {
                resp = apiRequest(req, context, f.host);
            } catch (Exception e) {
                context.errorMessage(String.Format("Could not create workspace: {0}", e.ToString()));
                return false;
            }

            Flog.log(resp.GetResponseStream().ToString());
            return (int)resp.StatusCode < 300;
        }

        public static HTTPWorkspaceRequest getWorkspace(FlooUrl f, IContext context) {

            HttpWebResponse resp;
            try {
                resp = getWorkspaceMethod(f, context);
            } catch (IOException e) {
                return null;
            }
            
            // Redirects aren't followed, so die here
            if ((int)resp.StatusCode >= 300) {
                PersistentJson.removeWorkspace(f);
                return null;
            }

            return createFromJsonStream<HTTPWorkspaceRequest>(resp.GetResponseStream());
        }

        public static bool workspaceExists(FlooUrl f, IContext context) {
            if (f == null) {
                return false;
            }
            HttpWebResponse resp;
            try {
                resp = getWorkspaceMethod(f, context);
            } catch (Exception e) {
                Flog.warn(e.ToString());
                return false;
            }

            if ((int)resp.StatusCode >= 300){
                PersistentJson.removeWorkspace(f);
                return false;
            }
            return true;
        }

        static private HttpWebResponse getWorkspaceMethod(FlooUrl f, IContext context)
        {
            return apiRequest(genGetRequest(String.Format("https://{0}{1}", f.host, String.Format("/api/workspace/{0}/{1}", f.owner, f.workspace))), context, f.host);
        }

	    static public HttpWebResponse apiRequest(HttpWebRequest method, IContext context, String host)
        {
            FloorcJson floorc = Settings.get();
            try
            {
                string username = floorc.auth[host]["username"];
                string secret = floorc.auth[host]["secret"]; ;
                method.Credentials = new NetworkCredential(username, secret);
            }
            finally
            {

            }
            method.PreAuthenticate = true;

            method.ContentType = "application/json; charset=utf-8";
            method.UserAgent = "Floo CSP";
            method.KeepAlive = false;

            Flog.info("Sending an API request");
            HttpWebResponse resp;
            try
            {
                resp = (HttpWebResponse)method.GetResponse();
            }
            catch (WebException we)
            {
                resp = (HttpWebResponse)we.Response;
            }

            return resp;
        }

        static public LinkedList<string> getOrgsCanAdmin(string host, IContext context) {
            HttpWebRequest req = genGetRequest(String.Format("https://{0}{1}", host, "/api/orgs/can/admin"));
            LinkedList<string> orgs = new LinkedList<string>();
            HttpWebResponse resp;
            try {
                resp = apiRequest(req, context, host);
            } catch (Exception e) {
                Flog.warn(e.ToString());
                return orgs;
            }

            if ((int)resp.StatusCode >= 400) {
                return orgs;
            }

            try {
                StreamReader reader = new StreamReader(resp.GetResponseStream());
                JToken tokens = JToken.Parse(reader.ReadToEnd());
    
                foreach (JToken token in tokens) {
                    orgs.AddLast(token.Value<string>("name"));
                }
            } catch (Exception e) {
                Flog.warn(e.ToString());
                context.errorMessage("Error getting Floobits organizations. Try again later or please contact support.");
            }

            return orgs;
        }

        static public void uploadCrash(BaseHandler baseHandler, IContext context, Exception throwable) {
#if LATER
            numSent++;
            if (numSent >= maxErrorReports) {
                Flog.warn(string.Format("Already sent {0} errors this session. Not sending any more.", numSent));
                if (throwable != null) Flog.warn(throwable.ToString());
                return;
            }

            try {
                Flog.warn("Uploading crash report: {0}", throwable.ToString());
                string owner = "";
                string workspace = "";
                string colabDir = "";
                string username = "";

                if (baseHandler != null) {
                    owner = baseHandler.getUrl().owner;
                    workspace = baseHandler.getUrl().workspace;
                    colabDir = context != null ? context.colabDir : "???";
                    username = baseHandler is FlooHandler ? ((FlooHandler) baseHandler).state.username : "???";
                }



                method = new PostMethod("/api/log");

                Gson gson = new Gson();
                CrashDump crashDump = new CrashDump(throwable, owner, workspace, colabDir, username);
                String json = gson.toJson(crashDump);
                method.setRequestEntity(new StringRequestEntity(json, "application/json", "UTF-8"));
                new Thread(new Runnable() {
                    @Override
                    public void run() {
                        try {
                            apiRequest(method, context, Constants.floobitsDomain);
                        } catch (Throwable e) {
                            if (context != null) {
                                context.errorMessage(String.format("Couldn't send crash report %s", e));
                            }

                        }
                    }
                }, "Floobits Crash Reporter").run();
            } catch (Exception e) {
                try {
                    context.errorMessage(String.format("Couldn't send crash report %s", e));
                } catch (Throwable ignored) {}
            }
#endif
        }

        static public void uploadCrash(IContext context, Exception throwable) {
            uploadCrash(context.handler, context, throwable);
        }
#if LATER
        static public void sendUserIssue(String description, String username) {
            final PostMethod method;
            method = new PostMethod("/api/log");
            Gson gson = new Gson();
            CrashDump crashDump = new CrashDump(String.format("User submitted an issue: %s", description), username);
            String json = gson.toJson(crashDump);
            try {
                method.setRequestEntity(new StringRequestEntity(json, "application/json", "UTF-8"));
            } catch (UnsupportedEncodingException e) {
                Flog.warn("Couldn't send a user issue.");
                return;
            }
            new Thread(new Runnable() {
                @Override
                public void run() {
                    try {
                        apiRequest(method, null, Constants.floobitsDomain);
                    } catch (Throwable e) {
                        Flog.errorMessage(String.format("Couldn't send crash report %s", e), null);
                    }
                }
            }, "Floobits User Issue Submitter").run();
        }

#endif

    }
}
