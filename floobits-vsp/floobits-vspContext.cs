using System;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using EnvDTE;
using EnvDTE80;
using Floobits.Common.Interfaces;

namespace Floobits.Context
{
    public class VSPContext : IContext
    {
        private DTE2 package_dte;
        private OutputWindow ow;
        private OutputWindowPane owP;

        public void Initialize(DTE2 dte)
        {
            package_dte = dte;
            // Create a tool window reference for the Output window
            // and window pane.
            ow = dte.ToolWindows.OutputWindow;
            // Add a new pane to the Output window.
            owP = ow.OutputWindowPanes.Add("Floobits");
        }

        public void outputWindowMessage(string message)
        {
            // Add a line of text to the new pane.
            owP.OutputString(message + "\r\n");
        }

        public void flashMessage(string message)
        {
            // Add a line of text to the new pane.
            owP.OutputString("FLASH : " + message + "\r\n");
        }
        public void warnMessage(string message)
        {
            // Add a line of text to the new pane.
            owP.OutputString("WARN  : " + message + "\r\n");
        }
        public void statusMessage(string message)
        {
            // Add a line of text to the new pane.
            owP.OutputString("STATUS: " + message + "\r\n");
        }
        public void errorMessage(string message)
        {
            // Add a line of text to the new pane.
            owP.OutputString("ERROR : " + message + "\r\n");
        }
    }
}