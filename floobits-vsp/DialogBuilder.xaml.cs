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

namespace Floobits.floobits_vsp
{
    /// <summary>
    /// Interaction logic for DialogBuilder.xaml
    /// </summary>
    public partial class DialogBuilder : DialogWindow
    {
        RunLater<bool> runLater;

        public DialogBuilder(string title, string body, RunLater<bool> runLater)
        {
            InitializeComponent();
            this.Title = title;
            this.Body.Text = body;
            this.runLater = runLater;
        }

        static public void build(string title, string body, RunLater<bool> runLater)
        {
            DialogBuilder d = new DialogBuilder(title, body, runLater);
            d.ShowDialog();
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            runLater.run(true);
            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            runLater.run(false);
            this.Close();
        }
    }
}
