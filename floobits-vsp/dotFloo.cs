using Newtonsoft.Json.Linq;
using System;
using System.Data;
using System.IO;

// Classes for reading .floo* files

namespace Floobits
{
    public class dotFlooFile
    {

    }

    // Reads a user's .floorc file
    public class dotFloorcFile
    {
        public dynamic Contents;
        public dotFloorcFile()
        {
            Contents = JObject.Parse(
                File.ReadAllText(Environment.ExpandEnvironmentVariables(@"%HOMEDRIVE%%HOMEPATH%\.floorc.json"))); 
        }
    }
}

