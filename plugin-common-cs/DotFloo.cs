using Newtonsoft.Json;
using System;
using System.Data;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Floobits.Utilities;

// Classes for reading .floo* files

namespace Floobits.Common
{
    public class DotFloo
    {
        [Serializable()]
        private class DotFlooJson
        {
            public string url;
            public Dictionary<string, string> hooks;
        }
        public static string path(string base_dir)
        {
            return Path.Combine(base_dir, ".floo");
        }

        private static DotFlooJson parse(string base_dir)
        {
            string floo;

            try
            {
                floo = File.ReadAllText(path(base_dir), Encoding.UTF8);
            }
            catch (Exception e)
            {
                Flog.debug(String.Format("no floo file {0} read exception {1]", path(base_dir), e.ToString()));
                return null;
            }

            try
            {
                return JsonConvert.DeserializeObject<DotFlooJson>(floo);
            }
            catch (Exception e)
            {
                Flog.warn(e.ToString());
            }
            return null;
        }

        public static FlooUrl read(string base_dir)
        {
            DotFlooJson dotFlooJson = parse(base_dir);
            if (dotFlooJson == null)
                return null;
            try
            {
                return new FlooUrl(dotFlooJson.url);
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public static void write(string base_dir, string url)
        {
            string filename = path(base_dir);
            DotFlooJson dotFlooJson = parse(base_dir);
            if (dotFlooJson == null)
            {
                Flog.warn("DotFloo isn't json.");
                if (File.Exists(filename))
                {
                    return;
                }
                dotFlooJson = new DotFlooJson();
            }

            string json = JsonConvert.SerializeObject(dotFlooJson, Formatting.Indented);

            try
            {
                File.WriteAllText(path(base_dir), json, Encoding.UTF8);
            }
            catch (Exception e)
            {
                Flog.warn(e.ToString());
            }
        }
    }
}

