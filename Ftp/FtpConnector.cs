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
    public class FtpConnector: IConnector
    {
        #region variables;
        /// <summary>
        /// 用户名
        /// </summary>
        private string _username;
        /// <summary>
        /// 密码
        /// </summary>
        private string _password;
        /// <summary>
        /// host地址
        /// </summary>
        private string _hostname;
        /// <summary>
        /// 端口
        /// </summary>
        private int _port;
        /// <summary>
        /// 命令socket
        /// </summary>
        private Socket _mainSocket;
        /// <summary>
        /// 数据socket
        /// </summary>
        private Socket _dataSocket;
        /// <summary>
        /// 设置的timeout时间
        /// </summary>
        private int _timeout;
        /// <summary>
        /// 是否为匿名登陆
        /// </summary>
        private bool _isAnonymous = false;
        #endregion

        #region getter&setter
        #endregion

        #region PublicFunction
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="hostname">主机名</param>
        /// <param name="username">用户名，当用户名为""的时候，默认为匿名登陆</param>
        /// <param name="password">密码，当密码为""的时候，默认为匿名登陆</param>
        /// <param name="port">接口</param>
        /// <param name="timeout">超时</param>
        public FtpConnector(string hostname, string username="", string password="", int port =21, int timeout=2000)
        {
            if (username.Equals("") && password.Equals(""))
            {
                _username = username;
                _password = password;
                _isAnonymous = true;
            }
            
            _hostname = hostname;
            _port = port;
            _timeout = timeout;
            //连接
            _mainSocket = Connect(hostname, port, username, password,timeout);
        }

     /// <summary>
     /// 进行匿名连接
     /// </summary>
     /// <param name="host">主机名</param>
     /// <param name="port">端口</param>
     /// <param name="timeout">超时</param>
     /// <returns></returns>
        public Socket Connect(string host, int port, int timeout)
        {
            return Connect(host, port, "", "", timeout);
        }

       /// <summary>
       /// 进行连接
       /// </summary>
       /// <param name="host">主机名</param>
       /// <param name="port">端口</param>
       /// <param name="username">用户名</param>
       /// <param name="password">密码</param>
       /// <param name="timeout">超时</param>
       /// <returns></returns>
        public Socket Connect(string host, int port, string username, string password, int timeout)
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
            {
                ReceiveTimeout = timeout,
                SendTimeout = timeout
            };
            Regex regex = new Regex(@"[0-9]+\.[0-9]+\.[0-9]+\.[0-9]+");
            //判断是ip还是域名
            var isMatch = regex.IsMatch(host);
            IPAddress ipAddress = isMatch ? IPAddress.Parse(host) : Dns.GetHostAddresses(host)[0];
            socket.Connect(ipAddress, port);
            //输入密码和用户名
            //todo:考虑匿名连接
            if (!_isAnonymous)
            {
                WaitReceive(socket);
                socket.Send(EncodingUtf8(Actions.User(username)));
                WaitReceive(socket);
                socket.Send(EncodingUtf8(Actions.Password(password)));
                WaitReceive(socket);
            }
            //被动模式连接
            socket.Send(EncodingUtf8(Actions.Passsive()));
            var receiveMsg = WaitReceive(socket);
            //如果为被动连接模式
            if (ReadCode(receiveMsg) == StateCode.EnterPassiveMode)
            {
                var leftPar = receiveMsg.IndexOf("(", StringComparison.Ordinal)+1;
                var rightPar = receiveMsg.IndexOf(")", StringComparison.Ordinal);
                var passive = receiveMsg.Substring(leftPar, rightPar - leftPar).Split(',');
                //获取连接的host名
                var server = passive[0] + "." + passive[1] + "." + passive[2] + "." + passive[3];
                //获取连接的端口
                port = (int.Parse(passive[4]) << 8) + int.Parse(passive[5]);
                //关闭数据socket（无论是否开启过）
                CloseDataSocket();
                //数据socket重新连接
                _dataSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _dataSocket.Connect(server, port);
            }
            else
            {
                throw new NotImplementedException();
            }

            return socket;
        }

        public bool Download(string url)
        {
            throw new NotImplementedException();
        }

        public bool Upload(Socket socket, string filename)
        {
            throw new NotImplementedException();
        }

        public bool Upload(Socket socket, FileStream fileStream)
        {
            throw new NotImplementedException();
        }

        public bool ContinueUpload(Socket socket, string filename)
        {
            throw new NotImplementedException();
        }

        public bool ContinueUpload(Socket socket, FileStream fileStream)
        {
            throw new NotImplementedException();
        }

        public List<VisualFile> ListRemoteFiles(string dirname)
        {
            Debug.WriteLine(Actions.NList(dirname));
            _mainSocket.Send(EncodingUtf8(Actions.NList(dirname)));
            Debug.WriteLine(WaitReceive(_mainSocket));
            
            return null;
        }
        public List<VisualFile> ListRemoteFiles()
        {
            Debug.WriteLine(Actions.List());
            _mainSocket.Send(EncodingUtf8(Actions.List()));
            Debug.WriteLine(WaitReceive(_mainSocket));
            var waitReceive = WaitReceive(_dataSocket);
            Debug.WriteLine(waitReceive);
            var files = VisualFileConverter.ConvertTextToVisualFiles(waitReceive);
            foreach (var visualFile in files)
            {
                Debug.WriteLine(visualFile);
            }
            return files.ToList();
        }

        public List<VisualFile> ListLocalFiles()
        {
            return ListLocalFiles(".");
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
        /// <summary>
        /// 关闭数据socket
        /// </summary>
        public void CloseDataSocket()
        {
            if (_dataSocket != null && _dataSocket.Connected)
            {
                _dataSocket.Close();
            }

            _dataSocket = null;
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
            while(socket.Available > 0)
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
        private static void Wait(int millSeconds=20)
        {
            Thread.Sleep(TimeSpan.FromMilliseconds(millSeconds));
        }
        /// <summary>
        /// 等待socket
        /// </summary>
        /// <param name="socket">等待socket</param>
        /// <param name="timeout">极限等到时间</param>
        private static void Wait(Socket socket, int timeout=5000)
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
