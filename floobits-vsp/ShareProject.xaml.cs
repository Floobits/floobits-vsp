using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.VisualStudio.PlatformUI;
using Floobits.Common;
using Floobits.Common.Interfaces;

namespace Floobits.floobits_vsp
{
    /// <summary>
    /// Interaction logic for ShareProject.xaml
    /// </summary>
    public partial class ShareProject : DialogWindow
    {
        IContext context;
        string host;
        bool _private;
        string path;

        public ShareProject(IContext context, string name, LinkedList<string> orgs, string host, bool _private, string path)
        {
            InitializeComponent();
            this.context = context;
            this.name.Text = name;
            this._private = _private;
            this.path = path;
            this.host = host;

            if (this._private)
            {
                this.Title = string.Concat(this.Title, " Privately");
            }

            this.owner.ItemsSource = orgs;
            this.owner.SelectedIndex = 0;
        }

        private void Create_Click(object sender, RoutedEventArgs e)
        {
            Object item = owner.SelectedItem;
            API.createWorkspace(host, item.ToString(), name.Text, context, _private);
            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

    }
}
