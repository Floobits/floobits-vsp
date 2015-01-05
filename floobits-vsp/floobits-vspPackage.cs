﻿using System;
using System.IO;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using System.Net;
using Microsoft.Win32;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Floobits.Common;
using Floobits.Utilities;
using EnvDTE;
using EnvDTE80;


namespace Floobits.floobits_vsp
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    ///
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the 
    /// IVsPackage interface and uses the registration attributes defined in the framework to 
    /// register itself and its components with the shell.
    /// </summary>
    // This attribute tells the PkgDef creation utility (CreatePkgDef.exe) that this class is
    // a package.
    [PackageRegistration(UseManagedResourcesOnly = true)]
    // This attribute is used to register the information needed to show this package
    // in the Help/About dialog of Visual Studio.
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    // This attribute is needed to let the shell know that this package exposes some menus.
    [ProvideMenuResource("Menus.ctmenu", 1)]
    // This attribute registers a tool window exposed by this package.
    [ProvideToolWindow(typeof(MyToolWindow))]
    [Guid(GuidList.guidfloobits_vspPkgString)]
    public sealed class floobits_vspPackage : Package
    {

        VSPContext context = new VSPContext();
        /// <summary>
        /// Default constructor of the package.
        /// Inside this method you can place any initialization code that does not require 
        /// any Visual Studio service because at this point the package object is created but 
        /// not sited yet inside Visual Studio environment. The place to do all the other 
        /// initialization is the Initialize method.
        /// </summary>
        public floobits_vspPackage()
        {
            Flog.Setup(context);
            Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this.ToString()));
            Directory.CreateDirectory(Floobits.Common.Constants.baseDir);
        }

        /// <summary>
        /// This function is called when the user clicks the menu item that shows the 
        /// tool window. See the Initialize method to see how the menu item is associated to 
        /// this function using the OleMenuCommandService service and the MenuCommand class.
        /// </summary>
        private void ShowToolWindow(object sender, EventArgs e)
        {
            // Get the instance number 0 of this tool window. This window is single instance so this instance
            // is actually the only one.
            // The last flag is set to true so that if the tool window does not exists it will be created.
            ToolWindowPane window = this.FindToolWindow(typeof(MyToolWindow), 0, true);
            if ((null == window) || (null == window.Frame))
            {
                throw new NotSupportedException(Resources.CanNotCreateWindow);
            }
            IVsWindowFrame windowFrame = (IVsWindowFrame)window.Frame;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());
        }


        /////////////////////////////////////////////////////////////////////////////
        // Overridden Package Implementation
        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            Debug.WriteLine (string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this.ToString()));
            base.Initialize();

            // Initialize the Floobits Context
            context.Initialize((DTE2)GetService(typeof(DTE)));

            // HTTP/SSL Setup
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            
            // Add our command handlers for menu (commands must exist in the .vsct file)
            OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if ( null != mcs )
            {
                // Create the command for the menu item.
                CommandID menuCommandID;
                MenuCommand menuItem;
                menuCommandID = new CommandID(GuidList.guidfloobits_vspCmdSet, (int)PkgCmdIDList.cmdidJoinWorkspace);
                menuItem = new MenuCommand(MenuItemJoinWorkspaceCallback, menuCommandID);
                mcs.AddCommand(menuItem);
                // Create the command for the menu item.
                menuCommandID = new CommandID(GuidList.guidfloobits_vspCmdSet, (int)PkgCmdIDList.cmdidJoinRecentWorkspace);
                menuItem = new MenuCommand(MenuItemJoinRecentWorkspaceCallback, menuCommandID);
                mcs.AddCommand(menuItem);
                // Create the command for the menu item.
                menuCommandID = new CommandID(GuidList.guidfloobits_vspCmdSet, (int)PkgCmdIDList.cmdidCreatePublicWorkspace);
                menuItem = new MenuCommand(MenuItemCreatePublicWorkspaceCallback, menuCommandID);
                mcs.AddCommand(menuItem);
                // Create the command for the menu item.
                menuCommandID = new CommandID(GuidList.guidfloobits_vspCmdSet, (int)PkgCmdIDList.cmdidCreatePrivateWorkspace);
                menuItem = new MenuCommand(MenuItemCreatePrivateWorkspaceCallback, menuCommandID);
                mcs.AddCommand(menuItem);
                // Create the command for the tool window
                CommandID toolwndCommandID = new CommandID(GuidList.guidfloobits_vspCmdSet, (int)PkgCmdIDList.cmdidFlooTool);
                MenuCommand menuToolWin = new MenuCommand(ShowToolWindow, toolwndCommandID);
                mcs.AddCommand( menuToolWin );
            }
        }
        #endregion

        private void MenuItemJoinWorkspaceCallback(object sender, EventArgs e)
        {
            // get current solution
            IVsSolution solution = (IVsSolution)Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(IVsSolution));
            string soldir, solfile, uso;
            int ret = solution.GetSolutionInfo(out soldir, out solfile, out uso);

            var d = new JoinWorkspaceURL(context, soldir);
            d.ShowDialog();
        }

        private void MenuItemJoinRecentWorkspaceCallback(object sender, EventArgs e)
        {
            var d = new JoinRecentWorkspace(context);
            d.ShowDialog();
        }

        private void MenuItemCreatePublicWorkspaceCallback(object sender, EventArgs e)
        {
            // get current solution
            IVsSolution solution = (IVsSolution)Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(IVsSolution));
            string soldir, solfile, uso;
            int ret = solution.GetSolutionInfo(out soldir, out solfile, out uso);
            context.shareProject(false, soldir);
        }

        private void MenuItemCreatePrivateWorkspaceCallback(object sender, EventArgs e)
        {
            context.shareProject(true, "horsey");
        }
    }
}
