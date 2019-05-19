using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;

namespace Ftp.@interface
{
    public interface IConnector
    {
        void Connect();
        bool Download(string url,string localAddress);
        bool Download(string url);
        Socket GetDataSocket();
        bool Upload(string remoteAddress, string localAddress);
        bool Upload(FileStream fileStream);
        bool ContinueUpload(string filename);
        bool ContinueUpload(FileStream fileStream);
        List<VisualFile> ListRemoteFiles();
        void ChangeLocalDir(string dirname);
        List<VisualFile> ListLocalFiles();
        void ChangeRemoteDir(string dirname);
        void Close();
        void CloseDataSocket(Socket dataSocket);
    }
}
