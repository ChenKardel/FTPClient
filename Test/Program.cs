using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Ftp;
using Ftp.@enum;
//using FtpClient = Ftp.exp.FtpClient;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var ftpConnector1 = new FtpConnector("47.100.3.187", "cjj123", "cjj123");
            ftpConnector1.Connect();
            foreach (var remoteFile in ftpConnector1.ListRemoteFiles())
            {
                Debug.WriteLine(remoteFile);
            }
            foreach (var remoteFile in ftpConnector1.ListRemoteFiles())
            {
                Debug.WriteLine(remoteFile);
            }
        }
    }
}