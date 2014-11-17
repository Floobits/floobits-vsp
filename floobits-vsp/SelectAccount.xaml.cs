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
    /// Interaction logic for SelectAccount.xaml
    /// </summary>
    public partial class SelectAccount : Window
    {
        public SelectAccount(string [] accounts)
        {
            InitializeComponent();
            this.account.ItemsSource = accounts;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.account.SelectedIndex = -1;
            this.Close();
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        public string getAccount()
        {
            string ret = null;
            if (this.account.SelectedItem != null)
            {
                ret = this.account.SelectedItem.ToString();
            }
            return ret;
        }
    }
}
