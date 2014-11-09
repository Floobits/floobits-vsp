using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Data;
using System.IO;
using System.Text;
using System.Collections.Generic;
using Floobits.Utilities;
using Floobits.Common.Interfaces;

namespace Floobits.Common
{
    public class Settings
    {
        public static string floorcJsonPath = Environment.ExpandEnvironmentVariables(@"%HOMEDRIVE%%HOMEPATH%\.floorc.json");

        public static FloorcJson get()  {
            string str;
            try {
                str = File.ReadAllText(floorcJsonPath, Encoding.UTF8);
            } catch (Exception e) {
                Flog.log(String.Format("floorc.json read exception {0}", e.ToString()));
                return null;
            }
            try {
                return JsonConvert.DeserializeObject<FloorcJson>(str);
            } catch (Exception) {
               throw new Exception("Invalid JSON.");
           }
        }

        public static void write(IContext context, dynamic floorcJson) {
            StreamWriter file;
            
            try {
                file = new StreamWriter(floorcJsonPath, true);
            } catch (Exception e) {
                Flog.warn(e.ToString());
                context.errorMessage("Can't write ~/.floorc.json");
                return;
            }

            try {
                file.Write(JsonConvert.SerializeObject(floorcJson));
            } catch (Exception e) {
                Flog.warn(e.ToString());
                context.errorMessage("Can't write new ~/.floorc.json");
            }
        }

        public static bool isAuthComplete(Dictionary<string, string> settings)
        {
            return (settings.ContainsKey("secret") && (settings.ContainsKey("username") || settings.ContainsKey("api_key")));
        }

        public static Boolean canFloobits() {
            Dictionary<string, Dictionary<string, string>> auth;
            try {
                auth = get().auth;
            } catch (Exception) {
                return false;
            }
            if (auth == null) {
                return false;
            }
            foreach (string host in auth.Keys) {
                if (isAuthComplete(auth[host])) {
                    return true;
                }
            }
            return false;
        }
    }
}