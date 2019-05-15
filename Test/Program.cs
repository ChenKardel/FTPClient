using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Ftp.@enum;
//using Ftp;
using Ftp.exp;
using Ftp.exp;
using Ftp.@interface;
using FtpClient = Ftp.FtpClient;
//using FtpClient = Ftp.exp.FtpClient;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
//            var ftpClient = new FtpClient("47.100.3.187", "cjj123", "cjj123");
//            var files = ftpClient.GetCurDirList(FtpFileType.All);
//            foreach (var ftpFileInfo in files)
//            {
//                Debug.WriteLine(ftpFileInfo.FileName);
//            }
//            Debug.WriteLine(ftpClient.PasvMode);
//            var files2 = ftpClient.GetCurDirList(FtpFileType.All);
//            foreach (var ftpFileInfo in files2)
//            {
//                Debug.WriteLine(ftpFileInfo.FileName);
//            }
            var ftpClient = new FtpClient("47.100.3.187", "cjj123", "cjj123");
            ftpClient.Connect();
            ftpClient.GetRemoteFiles();
        }
    }
}