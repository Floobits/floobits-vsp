using System;
using IO = System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel.Composition;
using Floobits.Common;
using Floobits.Common.Interfaces;

namespace Floobits.floobits_vsp
{
    /// <summary>
    /// Interaction logic for FlooTreeControl.xaml
    /// </summary>
    public partial class FlooTreeControl : UserControl
    {
        [Import(typeof(VSPContextContainer))]
        internal VSPContextContainer ContextContainer = null;

        CancellationTokenSource cts;
        IO.FileSystemWatcher fw;

        public FlooTreeControl()
        {
            InitializeComponent();
        }

        private WindowStatus currentState = null;
        /// <summary>
        /// This is the object that will keep track of the state of the IVsWindowFrame
        /// that is hosting this control. The pane should set this property once
        /// the frame is created to enable us to stay up to date.
        /// </summary>
        public WindowStatus CurrentState
        {
            get { return currentState; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                currentState = value;
                // Subscribe to the change notification so we can update our UI
                currentState.StatusChange += new EventHandler<EventArgs>(this.RefreshValues);
                // Update the display now
                this.RefreshValues(this, null);
            }
        }

        /// <summary>
        /// This method is the call back for state changes events
        /// </summary>
        /// <param name="sender">Event senders</param>
        /// <param name="arguments">Event arguments</param>
        private void RefreshValues(object sender, EventArgs arguments)
        {
            this.Width = currentState.Width;
            this.Height = currentState.Height;

            if (this.Width > this.TreeView.Margin.Top + this.TreeView.Margin.Bottom)
            {
                this.TreeView.Width = this.Width - this.TreeView.Margin.Top - this.TreeView.Margin.Bottom;
            }
            if (this.Height > this.TreeView.Margin.Right + this.TreeView.Margin.Left)
            {
                this.TreeView.Height = this.Height - this.TreeView.Margin.Right - this.TreeView.Margin.Left;
            }

            this.InvalidateVisual();
        }

        public void WatchPath(string path)
        {
            Label.Visibility = Visibility.Hidden;
            TreeView.Visibility = Visibility.Visible;

            // Create the token source.
            //cts = new CancellationTokenSource();
            //fw = new IO.FileSystemWatcher(FilenameUtils.normalize(path));

            TreeView.Items.Clear();
            WalkDirectoryTree(TreeView.Items, new System.IO.DirectoryInfo(path));            
        }

        public void StopWatching()
        {
            TreeView.Visibility = Visibility.Hidden;
            Label.Visibility = Visibility.Visible;

        }

        private void WalkDirectoryTree(ItemCollection items, IO.DirectoryInfo root)
        {
            IO.FileInfo[] files = null;
            IO.DirectoryInfo[] subDirs = null;

            // First, process all the files directly under this folder 
            try
            {
                files = root.GetFiles("*.*");
            }
            // This is thrown if even one of the files requires permissions greater 
            // than the application provides. 
            catch (UnauthorizedAccessException e)
            {
                // This code just writes out the message and continues to recurse. 
                // You may decide to do something different here. For example, you 
                // can try to elevate your privileges and access the file again.
                //log.Add(e.Message);
            }

            catch (IO.DirectoryNotFoundException e)
            {
                //Console.WriteLine(e.Message);
            }

            if (files != null)
            {
                foreach (IO.FileInfo fi in files)
                {
                    // In this example, we only access the existing FileInfo object. If we 
                    // want to open, delete or modify the file, then 
                    // a try-catch block is required here to handle the case 
                    // where the file has been deleted since the call to TraverseTree().
                    items.Add(IO.Path.GetFileName(fi.FullName));
                }

                // Now find all the subdirectories under this directory.
                subDirs = root.GetDirectories();

                foreach (IO.DirectoryInfo dirInfo in subDirs)
                {
                    TreeViewItem item = new TreeViewItem();
                    item.Header = IO.Path.GetFileName(dirInfo.Name);
                    items.Add(item);
                    // Recursive call for each subdirectory.
                    WalkDirectoryTree(item.Items, dirInfo);
                }
            }
        }
    }
}
