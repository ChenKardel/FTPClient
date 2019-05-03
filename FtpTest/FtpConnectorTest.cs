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
        public void TestInit()
        {
            var ftpConnector1 = new FtpConnector("39.106.225.117", "cjj123", "cjj123");
            Console.WriteLine(ftpConnector1.ListLocalFiles());
        }

        [TestMethod]
        public void TestVisualFileToString()
        {
            var visualFile = new VisualFile("hello.txt", VisualFile.FType.NormalFile, 1024, DateTime.Now);
            Debug.WriteLine(visualFile.ToString());
        }


    }
}
