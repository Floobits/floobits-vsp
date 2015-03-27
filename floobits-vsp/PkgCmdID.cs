// PkgCmdID.cs
// MUST match PkgCmdID.h
using System;

namespace Floobits.floobits_vsp
{
    static class PkgCmdIDList
    {
        public const uint cmdidJoinWorkspace = 0x200;
        public const uint cmdidJoinRecentWorkspace = 0x201;
        public const uint cmdidCreatePublicWorkspace = 0x202;
        public const uint cmdidCreatePrivateWorkspace = 0x203;
        public const uint cmdidFlooChat = 0x101;
        public const uint cmdidFlooTree = 0x102;
    };
}