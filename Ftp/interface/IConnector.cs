using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;

namespace Ftp.@interface
{
    interface IConnector
    {
        Socket Connect(string host, int port, int timeout);
        Socket Connect(string host, int port, string username, string password,int timeout);
        bool Download(string url);
        bool Upload(Socket socket, string filename);
        bool Upload(Socket socket, FileStream fileStream);
        bool ContinueUpload(Socket socket, string filename);
        bool ContinueUpload(Socket socket, FileStream fileStream);
        List<VisualFile> ListRemoteFiles();
        List<VisualFile> ListRemoteFiles(string dirname);
        List<VisualFile> ListLocalFiles();
        List<VisualFile> ListLocalFiles(string dirname);
        Socket Disconnect();
    }
}
