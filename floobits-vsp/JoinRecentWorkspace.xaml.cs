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
    /// Interaction logic for JoinRecentWorkspace.xaml
    /// </summary>
    public partial class JoinRecentWorkspace : DialogWindow
    {
        IContext context; 
        public JoinRecentWorkspace(IContext context)
        {
            this.context = context;
            PersistentJson p = PersistentJson.getInstance();

            InitializeComponent();

            list.SelectionMode = SelectionMode.Single;
            list.ItemsSource = p.recent_workspaces;            
        }

        private void Join_Click(object sender, RoutedEventArgs e)
        {
            Workspace w = (Workspace)list.SelectedItem;
            context.joinWorkspace(new FlooUrl(w.url), w.path, false);
            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
