using System;
using IO = System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using Forms = System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.VisualStudio.PlatformUI;
using Floobits.Common;
using Floobits.Common.Interfaces;

namespace Floobits.floobits_vsp
{
    /// <summary>
    /// Interaction logic for JoinWorkspaceURL.xaml
    /// </summary>
    public partial class JoinWorkspaceURL : DialogWindow
    {
        IContext context;

        public JoinWorkspaceURL(IContext context, string path)
        {
            InitializeComponent();
            this.context = context;
            this.url.Text = "https://floobits.com/";
            SetPath(path);
        }

        private void SetPath(string path)
        {
            if (IO.Directory.Exists(path))
            {
                this.PathLabel.Text = FilenameUtils.normalize(path);
            }
            else
            {
                if (IO.File.Exists(path))
                {
                    this.PathLabel.Text = FilenameUtils.normalize(IO.Path.GetDirectoryName(path));
                }
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Join_Click(object sender, RoutedEventArgs e)
        {
            FlooUrl floourl = new FlooUrl(url.Text);
            context.joinWorkspace(floourl, PathLabel.Text, false);
            this.Close();
        }

        private void PathDialog_Click(object sender, RoutedEventArgs e)
        {
            Forms.FolderBrowserDialog dialog = new Forms.FolderBrowserDialog();
            Forms.DialogResult result = dialog.ShowDialog();
            if (result == Forms.DialogResult.OK)
            {
                SetPath(dialog.SelectedPath);
            }
        }
    }
}
