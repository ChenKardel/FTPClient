using System;
using System.Diagnostics;
using Ftp;
using Ftp.@interface;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FtpTest
{
    [TestClass]
    public class FtpConnectorTest
    {
        [TestMethod]
        public void TestConnection()
        {
            var ftpConnector1 = new FtpConnector("47.100.3.187", "cjj123", "cjj123");
            ftpConnector1.Connect();
            Console.WriteLine(ftpConnector1.ListLocalFiles());
        }

        [TestMethod]
        public void TestListRemoteFiles()
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
