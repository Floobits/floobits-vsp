﻿using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;
using ErrorHandler = Microsoft.VisualStudio.ErrorHandler;
using Floobits.Common.Interfaces;
using Microsoft.VisualStudio.ComponentModelHost;

namespace Floobits.floobits_vsp
{
    /// <summary>
    /// This class implements the tool window exposed by this package and hosts a user control.
    ///
    /// In Visual Studio tool windows are composed of a frame (implemented by the shell) and a pane, 
    /// usually implemented by the package implementer.
    ///
    /// This class derives from the ToolWindowPane class provided from the MPF in order to use its 
    /// implementation of the IVsUIElementPane interface.
    /// </summary>
    [Guid(GuidList.guidFlooChatWindowPersistanceString)]
    public class FlooChatWindow : ToolWindowPane
    {
        // Chat Control
        public ChatControl control = null;

        /// <summary>
        /// Standard constructor for the tool window.
        /// </summary>
        public FlooChatWindow() :
            base(null)
        {
            // Set the window title reading it from the resources.
            this.Caption = Resources.ChatWindowTitle;
            // Set the image that will appear on the tab of the window frame
            // when docked with an other window
            // The resource ID correspond to the one defined in the resx file
            // while the Index is the offset in the bitmap strip. Each image in
            // the strip being 16x16.
            this.BitmapResourceID = 301;
            this.BitmapIndex = 1;

            // This is the user control hosted by the tool window; Note that, even if this class implements IDisposable,
            // we are not calling Dispose on this object. This is because ToolWindowPane calls Dispose on 
            // the object returned by the Content property.
            this.control = new ChatControl();
            base.Content = this.control;
        }

        /// <summary>
        /// This is called after our control has been created and sited.
        /// This is a good place to initialize the control with data gathered
        /// from Visual Studio services.
        /// </summary>
        public override void OnToolWindowCreated()
        {
            base.OnToolWindowCreated();
            control.SetContext((this.Package as floobits_vspPackage).GetVSPContext());

            // Register to the window events
            WindowStatus windowFrameEventsHandler = new WindowStatus(null, this.Frame as IVsWindowFrame);
            ErrorHandler.ThrowOnFailure(((IVsWindowFrame)this.Frame).SetProperty((int)__VSFPROPID.VSFPROPID_ViewHelper, (IVsWindowFrameNotify3)windowFrameEventsHandler));
            // Let our control have access to the window state
            control.CurrentState = windowFrameEventsHandler;
        }

    }
}
