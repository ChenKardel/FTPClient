using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            this.LocalFileListBox.Items.Clear();
            if (!(_connector.LocalPath == "/" || _connector.LocalPath == @"\"))
            {
                var fileItem = new FileItem(new VisualFile("..", VisualFile.FType.Directory, 0, DateTime.Now));
                fileItem.MouseDoubleClick += LocalFileItem_OnMouseDoubleClick;
                this.LocalFileListBox.Items.Add(fileItem);
            }
            var localFiles = _connector.ListLocalFiles();
            foreach (var localFile in localFiles)
            {
                var fileItem = new FileItem(localFile);
                fileItem.MouseDoubleClick += LocalFileItem_OnMouseDoubleClick;
                this.LocalFileListBox.Items.Add(fileItem);
            }
        }

        private void LocalFileItem_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            VisualFile file = ((FileItem)sender).File;
            if (file.FileType == VisualFile.FType.Directory)
            {
                _connector.ChangeLocalDir(file.Filename);
                RefreshLocalFiles();
            }
        }

        private void RefreshRemoteFiles()
        {
            this.RemoteFileListBox.Items.Clear();
            if (!(_connector.RemotePath == "/" || _connector.RemotePath == @"\"))
            {
                var fileItem = new FileItem(new VisualFile("..", VisualFile.FType.Directory, 0, DateTime.Now));
                fileItem.MouseDoubleClick += RemoteFileItem_OnMouseDoubleClick;
                this.RemoteFileListBox.Items.Add(fileItem);
            }
            var remoteFiles = _connector.ListRemoteFiles();
            foreach (var remoteFile in remoteFiles)
            {
                var fileItem = new FileItem(remoteFile);
                fileItem.MouseDoubleClick += RemoteFileItem_OnMouseDoubleClick;
                this.RemoteFileListBox.Items.Add(fileItem);
            }
        }

        private void RemoteFileItem_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            VisualFile file = ((FileItem)sender).File;
            if (file.FileType == VisualFile.FType.Directory)
            {
                _connector.ChangeRemoteDir(file.Filename);
                RefreshRemoteFiles();
            }
        }

        private void DownloadBtn_OnClick(object sender, RoutedEventArgs e)
        {
            var selectedFile = ((FileItem) RemoteFileListBox.SelectedItem).File;
            //            var remoteFilename = _connector.RemotePath + "/" + selectedFile.Filename;
            var remoteFilename = _connector.RemotePath == "/" ? _connector.RemotePath + selectedFile.Filename : _connector.RemotePath + "/" + selectedFile.Filename;

            var localFilename = _connector.LocalPath + System.IO.Path.DirectorySeparatorChar + selectedFile.Filename;
            Debug.WriteLine("remoteFilename: " + remoteFilename);
            Debug.WriteLine("localFilename: " + localFilename);

            bool isDownload = _connector.Download(remoteFilename, localFilename);
            if (isDownload)
            {
                RefreshLocalFiles();
                var collection = LocalFileListBox.Items.SourceCollection;
                FileItem item = null;
                foreach (FileItem fileItem in collection)
                {
                    if (fileItem.File.Filename == selectedFile.Filename)
                    {
                        item = fileItem;
                        break;
                    }
                }
                LocalFileListBox.Items.MoveCurrentTo(item);
            }
            else
            {
                
            }

        }

        private void UploadBtn_OnClick(object sender, RoutedEventArgs e)
        {
            var selectedFile = ((FileItem)LocalFileListBox.SelectedItem).File;
            var remoteFilename = _connector.RemotePath == "/" ? _connector.RemotePath + selectedFile.Filename : _connector.RemotePath + "/" + selectedFile.Filename;
            var localFilename = _connector.LocalPath + System.IO.Path.DirectorySeparatorChar + selectedFile.Filename;
            Debug.WriteLine("remoteFilename: " + remoteFilename);
            Debug.WriteLine("localFilename: " + localFilename);
            bool isUpload = _connector.Upload(remoteFilename, localFilename);
            if (isUpload)
            {
                RefreshRemoteFiles();
                var collection = RemoteFileListBox.Items.SourceCollection;
                FileItem item = null;
                foreach (FileItem fileItem in collection)
                {
                    if (fileItem.File.Filename == selectedFile.Filename)
                    {
                        item = fileItem;
                        break;
                    }
                }
                RemoteFileListBox.Items.MoveCurrentTo(item);
            }
            else
            {

            }

        }
    }
}
