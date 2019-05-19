using System;
using System.Diagnostics;
using System.IO;
using Ftp;
using Ftp.@interface;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FtpTest
{
    [TestClass]
    public class FtpConnectorTest
    {
        [TestMethod]
        [DataRow("47.100.3.187", "cjj123", "cjj123")]
        [DataRow("47.100.4.187", "cjj123", "cjj123")]
        [DataRow("47.100.3.187", "cjj124", "cjj123")]
        [DataRow("47.100.3.187", "cjj123", "cjj124")]
        public void TestConnection(string host, string username, string password)
        {
            var ftpConnector1 = new FtpConnector(host, username, password);
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

        
        [DataTestMethod]
        
        public void ChangeDir()
        {

            Debug.WriteLine(System.IO.Path.DirectorySeparatorChar);

        }

    }
}
