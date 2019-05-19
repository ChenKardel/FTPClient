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
using FtpClient.controller;

namespace FtpClient
{
    /// <summary>
    /// ConnectionWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ConnectionWindow : Window
    {
        private FtpConnector _connector;
        public ConnectionWindow()
        {
            InitializeComponent();
            _connector = new FtpConnector();
            this.FtpAddressTextBox.Text = "47.100.3.187";
            this.UsernameTextBox.Text = "cjj123";
            this.PwdBox.Password = "cjj123";
        }

        private void LinkBtn_OnClick(object sender, RoutedEventArgs e)
        {
            var password = this.PwdBox.Password;
            var username = this.UsernameTextBox.Text;
            var host = this.FtpAddressTextBox.Text;
            _connector.Password = password;
            _connector.Username = username;
            _connector.Hostname = host;
            _connector.Connect();
            RefreshRemoteFiles();
            RefreshLocalFiles();
        }

        private void RefreshLocalFiles()
        {
            var localFiles = _connector.ListLocalFiles();
            foreach (var localFile in localFiles)
            {
                this.LocalFileListBox.Items.Add(new FileItem(localFile));
            }
        }

        private void RefreshRemoteFiles()
        {
            var remoteFiles = _connector.ListRemoteFiles();
            foreach (var remoteFile in remoteFiles)
            {
                this.RemoteFileListBox.Items.Add(new FileItem(remoteFile));
            }
        }
    }
}
