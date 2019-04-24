using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Ftp;
using Ftp.@enum;
using Ftp.@interface;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var ftpConnector = new FtpConnector("39.106.225.117", "cjj123", "cjj123");
            List<VisualFile> listRemoteFiles = ftpConnector.ListRemoteFiles();

        }
    }
}