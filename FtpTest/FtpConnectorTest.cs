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
        [DataRow("/hello/world","..", '/')]
        [DataRow("/hello/world", "kardel", '/')]
        [DataRow("/", "kardel", '/')]
        [DataRow("/", "..", '/')]
        public void ChangeDir(string path, string newDir, char seperator = '/')
        {
            Debug.WriteLine("123");
            if (path != "/")
            {
                if (newDir == "..")
                {
                    var lastIndexOf = path.LastIndexOf(seperator);
                    Debug.WriteLine(path.Substring(0, lastIndexOf));
                }
                else
                {
                    Debug.WriteLine(path + seperator + newDir);
                }
            }
            else
            {
                if (newDir == "..")
                {
                    Debug.WriteLine("Error");
                }
                else
                {
                    Debug.WriteLine(path + newDir);
                }
            }

        }

    }
}
