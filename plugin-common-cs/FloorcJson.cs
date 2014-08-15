using Newtonsoft.Json;
using System;
using System.Data;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Floobits.Utilities;

namespace Floobits.Common
{
    [Serializable()]
    public class FloorcJson
    {
        public Dictionary<string, Dictionary<string, string>> auth;
        public bool debug;
        public string share_dir;
        public int MAX_ERROR_REPORTS;

        public static FloorcJson getFloorcJsonFromSettings () {
            FloorcJson floorcJson = null;
            try {
                floorcJson = Settings.get();
            } catch (Exception e) {
                Flog.warn(e.ToString());
            }
            if (floorcJson == null) {
                floorcJson = new FloorcJson();
            }
            if (floorcJson.auth == null) {
                floorcJson.auth = new Dictionary<string, Dictionary<string, string>>();
            }
            return floorcJson;
        }
    }
}
