
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Messaging;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Ftp.entity;
using Ftp.@enum;
using Ftp.@interface;
//using Action = System.Action;

namespace Ftp
{
    public class FtpConnector : IConnector
    {
        #region variables;
        public string Username { get; set; }
        public string Password { get; set; }
        public string Hostname { get; set; }
        public int Port { get; set; }
        public int TimeOutLimit { get; set; }
        private Socket _controlSocket;
        private bool _isAnonymous = false;
        public bool IsConnected { get; } = false;
        public FtpMode Mode { get; set; } = FtpMode.Passive;
        #endregion
        #region PublicFunction

        public enum FtpMode
        {
            Passive,
            Port
        }
        public FtpConnector(string hostname, string username = "", string password = "", int port = 21, int timeout = 2000)
        {
            if (username.Equals("") && password.Equals(""))
            {
                _isAnonymous = true;
            }
            else
            {
                _isAnonymous = false;
            }
            Username = username;
            Password = password;
            Hostname = hostname;
            Port = port;
            TimeOutLimit = timeout;
            //连接

        }

        public void Connect()
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
            {
                ReceiveTimeout = TimeOutLimit,
                SendTimeout = TimeOutLimit
            };
            Regex regex = new Regex(@"[0-9]+\.[0-9]+\.[0-9]+\.[0-9]+");
            //判断是ip还是域名
            var isMatch = regex.IsMatch(Hostname);
            IPAddress ipAddress = isMatch ? IPAddress.Parse(Hostname) : Dns.GetHostAddresses(Hostname)[0];
            socket.Connect(ipAddress, Port);
            if (!_isAnonymous)
            {
                WaitReceive(socket);
                socket.Send(EncodingUtf8(Actions.User(Username)));
                WaitReceive(socket);
                socket.Send(EncodingUtf8(Actions.Password(Password)));
                WaitReceive(socket);
                _controlSocket = socket;
            }
            else
            {
                throw new NotImplementedException();
            }

        }

        //指定下载地址时
        public bool Download(string remoteAddress, string localAddress)
        {
            var dataSocket = GetDataSocket();
            Debug.WriteLine(Actions.Retr(remoteAddress));
            _controlSocket.Send(EncodingUtf8(Actions.Retr(remoteAddress)));

            var waitReceive = WaitReceive(dataSocket);
            Debug.WriteLine(waitReceive);
            var statecode = WaitReceive(_controlSocket);
            Debug.WriteLine(statecode);
            if (File.Exists(localAddress))
                File.Delete(localAddress);
            FileStream fs = new FileStream(localAddress, FileMode.Create);
            byte[] data = System.Text.Encoding.Default.GetBytes(waitReceive);
            if (data == null)
            {
                return false;
            }
            fs.Write(data, 0, data.Length);
            fs.Flush();
            fs.Close();
            //根据statecode判断是否成功
            if (statecode.Substring(0, 3).Equals("125"))
            {
                return true;
            }
            else
            {
                return false;
            }
            CloseDataSocket(dataSocket);

        }
        //默认下载目录为当前目录
        public bool Download(string remoteAddress)
        {
            return Download(remoteAddress, ".");

        }

        public Socket GetDataSocket()
        {
            //被动模式连接
            if (Mode == FtpMode.Passive)
            {
                _controlSocket.Send(EncodingUtf8(Actions.Passsive()));
                var receiveMsg = WaitReceive(_controlSocket);
                //如果为被动连接模式
                if (ReadCode(receiveMsg) == StateCode.PassiveMode227)
                {
                    var ipEndPoint = ParsePassiveIp(receiveMsg);
                    var dataSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    dataSocket.Connect(ipEndPoint);
                    return dataSocket;
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        private static IPEndPoint ParsePassiveIp(string passiveIpString)
        {
            var leftPar = passiveIpString.IndexOf("(", StringComparison.Ordinal) + 1;
            var rightPar = passiveIpString.IndexOf(")", StringComparison.Ordinal);
            var passive = passiveIpString.Substring(leftPar, rightPar - leftPar).Split(',');
            //获取连接的host名
            var dataSocketServer = passive[0] + "." + passive[1] + "." + passive[2] + "." + passive[3];
            //获取连接的端口
            var dataSocketPort = (int.Parse(passive[4]) << 8) + int.Parse(passive[5]);
            return new IPEndPoint(IPAddress.Parse(dataSocketServer), dataSocketPort);
        }

        //

        public bool Upload(string remoteAddress, string localAddress)
        {
            var dataSocket = GetDataSocket();
            FileStream fs = new FileStream(localAddress, FileMode.Open);
            byte[] data = new byte[0];
            if (fs != null)
            {
                BinaryReader r = new BinaryReader(fs);
                r.BaseStream.Seek(0, SeekOrigin.Begin);    //将文件指针设置到文件开
                data = r.ReadBytes((int)r.BaseStream.Length);
                fs.Close();

            }
            else
            {
                return false;
            }

            dataSocket.Send(data);
            var waitReceive = WaitReceive(dataSocket);

            _controlSocket.Send(EncodingUtf8(Actions.Stor(remoteAddress)));

            Debug.WriteLine(waitReceive);
            var statecode = WaitReceive(_controlSocket);
            Debug.WriteLine(statecode);
            CloseDataSocket(dataSocket);
            return true;
        }

        public bool Upload(FileStream fileStream)
        {
            throw new NotImplementedException();
        }



        public bool ContinueUpload(string filename)
        {
            throw new NotImplementedException();
        }

        public bool ContinueUpload(FileStream fileStream)
        {
            throw new NotImplementedException();
        }

        public List<VisualFile> ListRemoteFiles()
        {
            var dataSocket = GetDataSocket();
            Debug.WriteLine(Actions.List());
            _controlSocket.Send(EncodingUtf8(Actions.List()));
            Debug.WriteLine(WaitReceive(_controlSocket));
            var waitReceive = WaitReceive(dataSocket);
            Debug.WriteLine(waitReceive);
            var files = VisualFileConverter.ConvertTextToVisualFiles(waitReceive);
            CloseDataSocket(dataSocket);
            return files.ToList();
        }
       
        public void ChangeLocalDir(string dirname)
        {
            throw new NotImplementedException();
        }

        public List<VisualFile> ListLocalFiles()
        {
            return ListLocalFiles(".");
        }

        public void ChangeRemoteDir(string dirname)
        {
            _controlSocket.Send(EncodingUtf8(Actions.ChangeCwd(dirname)));
            var waitReceive = WaitReceive(_controlSocket);
            Debug.WriteLine(waitReceive);
            if (ReadCode(waitReceive) != StateCode.ChangeDir250)
            {
                
            }
        }

        public List<VisualFile> ListLocalFiles(string dirname)
        {
            var normalFiles = Directory.GetFiles(dirname).
                Select((s => new VisualFile(new FileInfo(s), VisualFile.FType.NormalFile)));
            var directories = Directory.GetDirectories(dirname)
                .Select((s => new VisualFile(new FileInfo(s), VisualFile.FType.Directory)));
            return normalFiles.Concat(directories).ToList();
        }

        public void Close()
        {
            throw new NotImplementedException();
        }

        public void CloseDataSocket(Socket dataSocket)
        {
            if (dataSocket == null) return;
            if (dataSocket.Connected)
            {
                dataSocket.Close();
            }
        }

        #endregion
        #region privateFunction
        /// <summary>
        /// 从socket中读取数据
        /// </summary>
        /// <param name="socket">源socket</param>
        /// <returns>读取的数据</returns>
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
        /// <summary>
        /// 进行数据响应等待
        /// </summary>
        /// <param name="millSeconds">等待时间</param>
        private static void Wait(int millSeconds = 20)
        {
            Thread.Sleep(TimeSpan.FromMilliseconds(millSeconds));
        }
        /// <summary>
        /// 等待socket
        /// </summary>
        /// <param name="socket">等待socket</param>
        /// <param name="timeout">极限等到时间</param>
        private static void Wait(Socket socket, int timeout = 5000)
        {
            int t = 20;
            while (socket.Available == 0 && timeout > 0)
            {
                timeout -= 20;
                Wait(t);
            }
        }
        /// <summary>
        /// 解析并且读取code
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        private StateCode ReadCode(string msg)
        {
            Debug.WriteLine(msg);
            Regex regex = new Regex(@"[0-9]{3}");
            var match = regex.Match(msg);
            Enum.TryParse(match.Value, out StateCode stateCode);
            return stateCode;
        }

        private byte[] EncodingUtf8(string s)
        {
            return Encoding.UTF8.GetBytes(s);
        }
        /// <summary>
        /// 等待，并接收数据
        /// </summary>
        /// <param name="socket"></param>
        /// <returns></returns>
        private string WaitReceive(Socket socket)
        {
            Wait(socket);
            return SocketReceive(socket);
        }
        #endregion
    }
}