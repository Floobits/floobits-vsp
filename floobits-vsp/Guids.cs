// Guids.cs
// MUST match guids.h
using System;

namespace Floobits.floobits_vsp
{
    static class GuidList
    {
        public const string guidfloobits_vspPkgString = "0053e413-f941-4d28-990e-3b9a8c3eabd3";
        public const string guidfloobits_vspCmdSetString = "116bd6a1-d540-4ca9-8c08-2de0a3cd7799";
        public const string guidFlooChatWindowPersistanceString = "904bddd4-b9cf-446e-99f8-e50ff6684305";
        public const string guidFlooTreeWindowPersistanceString = "34cacc8d-f92a-41b7-bab0-22a7a015acbf";

        public static readonly Guid guidfloobits_vspCmdSet = new Guid(guidfloobits_vspCmdSetString);
    };
}