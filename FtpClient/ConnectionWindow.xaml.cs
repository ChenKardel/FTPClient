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
using Ftp;
using Ftp.@interface;
namespace FtpClient
{
    /// <summary>
    /// ConnectionWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ConnectionWindow : Window
    {
        private FtpConnector connector;
        public ConnectionWindow()
        {
            connector = new FtpConnector();
            InitializeComponent();
        }

        private void LinkBtn_OnClick(object sender, RoutedEventArgs e)
        {
            var hostname = FtpAddressTextBox.Text;
            var password = PwdBox.Password;
            var username = UsernameTextBox.Text;
            var success = connector.ConnectSave(hostname, connector.Port, username, password,2000);
            if (success)
            {
                MessageBox.Show("Success!");
                //todo
            }
            else
            {
                MessageBox.Show("Fail!");
                //todo
            }
        }
    }
}
