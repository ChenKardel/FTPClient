using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Ftp;
using Ftp.@enum;
namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var ftpConnector = new FtpConnector("10.132.91.24","ftpuser", "alksdj1029a");
            ftpConnector.ListRemoteFiles();
//            Regex regex = new Regex(@"[0-9]{3}");
//            var match = regex.Match("220 (vsFTPd 3.0.3)");
//            Debug.WriteLine(int.Parse(match.Value));
//            Enum.TryParse("227", out StateCode stateCode);
//            Debug.WriteLine(stateCode);
        }
    }
}
