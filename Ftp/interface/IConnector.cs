using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;

namespace Ftp.@interface
{
    public interface IConnector
    {
        Socket Connect(string host, int port, int timeout);
        Socket Connect(string host, int port, string username, string password,int timeout);
        bool Download(string url,string localAddress);
        bool Download(string url);

        bool Upload(string remoteAddress, string localAddress);
        bool Upload(FileStream fileStream);
        bool ContinueUpload(string filename);
        bool ContinueUpload(FileStream fileStream);
        List<VisualFile> ListRemoteFiles();
        List<VisualFile> ListRemoteFiles(string dirname);
        List<VisualFile> ListLocalFiles();
        List<VisualFile> ListLocalFiles(string dirname);
        void Close();
        void CloseDataSocket();
    }
}
