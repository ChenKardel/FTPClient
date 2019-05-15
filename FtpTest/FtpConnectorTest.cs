using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Ftp;
using Ftp.@enum;
using Ftp.@interface;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FtpTest
{
    [TestClass]
    public class FtpConnectorTest
    {

        [TestMethod]
        public void TestMethod1()
        {
            //
            // TODO:  在此处添加测试逻辑
            //
            var ftpConnector = new FtpConnector("47.100.3.187", "cjj123", "cjj123");
            ftpConnector.Connect();
//            ftpConnector.OpenDataSocket();
            foreach (var listRemoteFile in ftpConnector.ListRemoteFiles())
            {
                Debug.WriteLine(listRemoteFile);
            }
//            ftpConnector.CloseDataSocket();
//            ftpConnector.OpenDataSocket();
//            ftpConnector.ControlSocket.Send(EncodingUtf8("LIST\r\n"));
//            ftpConnector.CloseDataSocket();
            //            ftpConnector.ListRemoteFiles().ForEach((file => Debug.WriteLine(file)));
            //            ftpConnector.ListRemoteFiles().ForEach((file => Debug.WriteLine(file)));
        }
        private static byte[] EncodingUtf8(string s)
        {
            return Encoding.UTF8.GetBytes(s);
        }

        private string SocketReceive(Socket socket)
        {
            const int bufferSize = 256;
            string result = "";
            byte[] buffer = new byte[bufferSize];
            int bytes = 0;
            while (socket.Available > 0)
            {
                bytes = socket.Receive(buffer, bufferSize, 0);
                result += Encoding.UTF8.GetString(buffer, 0, bytes);

            }

            return result;
        }

        private string WaitReceive(Socket socket)
        {
            Wait(socket);
            return SocketReceive(socket);
        }

        private static void Wait(int millSeconds = 20)
        {
            Thread.Sleep(TimeSpan.FromMilliseconds(millSeconds));
        }

        /// <summary>
        /// 等待socket
        /// </summary>
        /// <param name="socket">等待socket</param>
        /// <param name="timeout">极限等到时间</param>
        private static void Wait(Socket socket, int timeout = 10000)
        {
            int t = 50;
            while (socket.Available == 0 && timeout > 0)
            {
                timeout -= t;
                Wait(t);
            }
        }

        [TestMethod]
        public void TestNonActiveMethod()
        {
//            var ftpConnector = new FtpConnector("39.106.225.117", "cjj123", "cjj123");
//          
            var hostname = "39.106.225.117";
            var username = "cjj123";
            var password = "cjj123";
            var port = 21;
            Socket cmdSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            cmdSocket.Connect(hostname, port);
            Debug.WriteLine(WaitReceive(cmdSocket));
            cmdSocket.Send(EncodingUtf8(Actions.User(username)));
            Debug.WriteLine(WaitReceive(cmdSocket));
            cmdSocket.Send(EncodingUtf8(Actions.Password(username)));
            Debug.WriteLine(WaitReceive(cmdSocket));


            cmdSocket.Send(EncodingUtf8(Actions.Passsive()));
            var retstr = WaitReceive(cmdSocket);
            var retArray = Regex.Split(retstr, ",");
            if (retArray[5][2] != ')')
            {
                //                retArray[5].Substring(0, 3);
                retstr = (retArray[5]).Substring(0, 3);
            }
            else
            {
                retstr = retArray[5].Substring(0, 2);
            }
            var dataPort = Convert.ToInt32(retArray[4]) * 256 + Convert.ToInt32(retstr);
            var dataSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            dataSocket.Connect(hostname, dataPort);
            cmdSocket.Send(EncodingUtf8("LIST\r\n"));
            Debug.WriteLine(WaitReceive(dataSocket));
            dataSocket.Close();
            cmdSocket.Send(EncodingUtf8("ABOR\r\n"));

            cmdSocket.Send(EncodingUtf8(Actions.Passsive()));
            retstr = WaitReceive(cmdSocket);
            Debug.WriteLine(retstr);
            retArray = Regex.Split(retstr, ",");
            if (retArray[5][2] != ')')
            {
                //                retArray[5].Substring(0, 3);
                retstr = (retArray[5]).Substring(0, 3);
            }
            else
            {
                retstr = retArray[5].Substring(0, 2);
            }
            dataPort = Convert.ToInt32(retArray[4]) * 256 + Convert.ToInt32(retstr);
            dataSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            dataSocket.Connect(hostname, dataPort);
            cmdSocket.Send(EncodingUtf8("LIST\r\n"));
            Debug.WriteLine(WaitReceive(dataSocket));
            dataSocket.Close();
            cmdSocket.Send(EncodingUtf8("ABOR\r\n"));
        }
    }
}
