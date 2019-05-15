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
using Ftp.error;
using Ftp.exp;
using Ftp.@interface;

//using Action = System.Action;

namespace Ftp
{
    public class FtpConnector : IConnector
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
        public int Port { get; }

        /// <summary>
        /// 命令socket
        /// </summary>
        public Socket ControlSocket { get; private set; }

        /// <summary>
        /// 数据socket
        /// </summary>
        public Socket DataSocket { get; private set; }

        public Socket ListerSocket { get; private set; }
        /// <summary>
        /// 设置的timeout时间
        /// </summary>
        private int _timeout;

        /// <summary>
        /// 是否为匿名登陆
        /// </summary>
        private bool _isAnonymous = false;

        /// <summary>
        /// 
        /// </summary>
        public bool isConnected { get; set; }

        private bool _passiveMode;

        private Socket _listeningSocket;
        private string ftpSystem;
        #endregion

        #region getter&setter

        #endregion

        #region PublicFunction

        public FtpConnector()
        {
            this._hostname = "";
            this._password = "";
            this._username = "";
            this.Port = 21;
            isConnected = false;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="hostname">主机名</param>
        /// <param name="username">用户名，当用户名为""的时候，默认为匿名登陆</param>
        /// <param name="password">密码，当密码为""的时候，默认为匿名登陆</param>
        /// <param name="port">接口</param>
        /// <param name="timeout">超时</param>
        public FtpConnector(string hostname, string username = "", string password = "", int port = 21,
            int timeout = 2000, bool passiveMode = true)
        {
            if (username.Equals("") && password.Equals(""))
            {
                _username = username;
                _password = password;
                _isAnonymous = true;
            }

            isConnected = false;
            _hostname = hostname;
            Port = port;
            _timeout = timeout;
            _passiveMode = passiveMode;
            _username = username;
            _password = password;
            //连接
        }

        /// <summary>
        /// 进行匿名连接
        /// </summary>
        /// <param name="host">主机名</param>
        /// <param name="port">端口</param>
        /// <param name="timeout">超时</param>
        /// <returns></returns>
        public bool Connect(string host, int port, int timeout)
        {
            if (_isAnonymous)
            {
                return Connect(host, port, "", "", timeout);
            }
            else
            {
                return Connect(this._hostname, this.Port, this._username, this._password, this._timeout);
            }
        }

        public bool Connect()
        {
            return Connect(this._hostname, this.Port, this._timeout);
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
        public bool Connect(string host, int port, string username, string password, int timeout)
        {
            this.isConnected = true;
            if (ControlSocket != null && ControlSocket.Connected)
            {
                return true;
            }

            ControlSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
            {
                ReceiveTimeout = timeout,
                SendTimeout = timeout
            };
            Regex regex = new Regex(@"[0-9]+\.[0-9]+\.[0-9]+\.[0-9]+");
            //判断是ip还是域名
            var isMatch = regex.IsMatch(host);
            IPAddress ipAddress = isMatch ? IPAddress.Parse(host) : Dns.GetHostAddresses(host)[0];
            ControlSocket.Connect(ipAddress, port);
            //输入密码和用户名
            //todo:考虑匿名连接
            if (!_isAnonymous)
            {
                var buf = WaitReceive(ControlSocket);
                Debug.WriteLine(buf);
                ExecCommand(Actions.User(username));
                buf = WaitReceive(ControlSocket);
                Debug.WriteLine(buf);
                Debug.WriteLine(Actions.User(password));
                ExecCommand(Actions.Password(password));
                buf = WaitReceive(ControlSocket);
                Debug.WriteLine(buf);
                ExecCommand(Actions.System());
//                ControlSocket.Send(EncodingUtf8(Actions.System()));
                buf = WaitReceive(ControlSocket);
                this.ftpSystem = buf;
                Debug.WriteLine(buf);
            }
     
            return true;
        }

        public void OpenDataSocket()
        {
            //被动模式连接
            ControlSocket.Send(EncodingUtf8(Actions.Passsive()));
            var receiveMsg = WaitReceive(ControlSocket);
            Debug.WriteLine("PASV response: " + receiveMsg);
            //如果为被动连接模式
            
            if (ReadCode(receiveMsg) == StateCode.EnterPassiveMode)
            {
                this.DataSocket.Connect(ParsePassiveIp(receiveMsg));
            }
            else
            {
                //fail
                throw new NotImplementedException();
            }

            isConnected = true;
        }

        private IPEndPoint ParsePassiveIp(string receiveMsg)
        {
            var leftPar = receiveMsg.IndexOf("(", StringComparison.Ordinal) + 1;
            var rightPar = receiveMsg.IndexOf(")", StringComparison.Ordinal);
            var passive = receiveMsg.Substring(leftPar, rightPar - leftPar).Split(',');
            //获取连接的host名
            var server = passive[0] + "." + passive[1] + "." + passive[2] + "." + passive[3];
            //获取连接的端口
            var port = (int.Parse(passive[4]) << 8) + int.Parse(passive[5]);
            //关闭数据socket（无论是否开启过）
            return new IPEndPoint(IPAddress.Parse(server), port);
        }


        public Socket GetPasvDataSocket()
        {
            var dataSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            if (!this.isConnected)
            {
                this.Connect();
            }
            this.ExecCommand(Actions.Passsive());
            var receiveMsg = WaitReceive(ControlSocket);
           
            if (ReadCode(receiveMsg) == StateCode.EnterPassiveMode)
            {
                var dataSocketIpAddress = ParsePassiveIp(receiveMsg);
                dataSocket.Connect(dataSocketIpAddress);
                return dataSocket;
            }

            return null;
        }
        //指定下载地址时
        public bool Download(string remoteAddress, string localAddress)
        {
            if (isConnected == false)
            {
                return false;
            }

            Debug.WriteLine(Actions.Retr(remoteAddress));
            ControlSocket.Send(EncodingUtf8(Actions.Retr(remoteAddress)));

            var waitReceive = WaitReceive(DataSocket);
            Debug.WriteLine(waitReceive);
            var statecode = WaitReceive(ControlSocket);
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
            return statecode.Substring(0, 3).Equals("125");
        }

        //默认下载目录为当前目录
        public bool Download(string remoteAddress)
        {
            return Download(remoteAddress, ".");
        }

        //

        public bool Upload(string remoteAddress, string localAddress)
        {
            if (isConnected == false)
            {
                return false;
            }

            FileStream fs = new FileStream(localAddress, FileMode.Open);
            byte[] data = new byte[0];
            if (fs != null)
            {
                BinaryReader r = new BinaryReader(fs);
                r.BaseStream.Seek(0, SeekOrigin.Begin); //将文件指针设置到文件开
                data = r.ReadBytes((int) r.BaseStream.Length);
                fs.Close();
            }
            else
            {
                return false;
            }

            DataSocket.Send(data);
            var waitReceive = WaitReceive(DataSocket);

            ControlSocket.Send(EncodingUtf8(Actions.Stor(remoteAddress)));

            Debug.WriteLine(waitReceive);
            var statecode = WaitReceive(ControlSocket);
            Debug.WriteLine(statecode);

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

        public List<VisualFile> ListRemoteFiles(string dirname)
        {
            if (!isConnected)
            {
                throw new NoConnectionException();
            }

            var dataSocket = GetPasvDataSocket();
            dataSocket.Send(EncodingUtf8(Actions.List()));
            var waitReceive = WaitReceive(ControlSocket);
            Debug.WriteLine("WaitReceive:" + waitReceive);
            Debug.WriteLine("-: " + WaitReceive(dataSocket));
            var files = VisualFileConverter.ConvertTextToVisualFiles(waitReceive);
            foreach (var visualFile in files)
            {
                Debug.WriteLine(visualFile);
            }

            return null;
        }

        public List<VisualFile> ListRemoteFiles()
        {
            if (!isConnected)
            {
                throw new NoConnectionException();
            }

            var dataSocket = GetPasvDataSocket();
            dataSocket.Send(EncodingUtf8(Actions.List()));
            var waitReceive = WaitReceive(ControlSocket);
            Debug.WriteLine("WaitReceive:" + waitReceive);
            Debug.WriteLine("-: " + WaitReceive(dataSocket));
            var files = VisualFileConverter.ConvertTextToVisualFiles(waitReceive);
            foreach (var visualFile in files)
            {
                Debug.WriteLine(visualFile);
            }

            return null;
        }

        public List<VisualFile> ListLocalFiles()
        {
            return ListLocalFiles(".");
        }

        public List<VisualFile> ListLocalFiles(string dirname)
        {
            var normalFiles = Directory.GetFiles(dirname)
                .Select((s => new VisualFile(new FileInfo(s), VisualFile.FType.NormalFile)));
            var directories = Directory.GetDirectories(dirname)
                .Select((s => new VisualFile(new FileInfo(s), VisualFile.FType.Directory)));
            return normalFiles.Concat(directories).ToList();
        }

        public void Close()
        {
            if (ControlSocket != null && ControlSocket.Connected)
            {
                ExecCommand(Actions.Quit());
            }
        }

        public void CloseControlSocket()
        {
            if (this.ControlSocket != null)
            {
                this.ControlSocket.Close();
                this.ControlSocket = null;
            }
        }

        /// <summary>
        /// 关闭数据socket
        /// </summary>
        public void CloseDataSocket()
        {
            isConnected = false;
            ControlSocket.Send(EncodingUtf8(Actions.Abort()));
            if (DataSocket != null && DataSocket.Connected)
            {
                DataSocket.Close();
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
            int intCount = 0;
            StringBuilder result = new StringBuilder();
            byte[] buffer = new byte[bufferSize];
            int bytes = 0;
            //todo: replace it
            while (socket.Available > 0)
            {
            bytes = socket.Receive(buffer, bufferSize, 0);
            result.Append(Encoding.UTF8.GetString(buffer, 0, bytes));
            }
//            while (true)
//            {
//                bytes = socket.Receive(buffer, buffer.Length, 0);
//                result.Append(Encoding.ASCII.GetString(buffer, 0, bytes));
//                if (bytes < buffer.Length)
//                {
//                    intCount++;
//                    Thread.Sleep(10);
//                    if (intCount >= 2)
//                    {
//                        break;
//                    }
//                }
//            }
            return result.ToString();
        }

        private void ConnectDataSocket()
        {
            if (DataSocket != null) // 已链接
                return;

            try
            {
                DataSocket = _listeningSocket.Accept(); // Accept is blocking
                _listeningSocket.Close();
                _listeningSocket = null;

                if (DataSocket == null)
                {
                    throw new Exception("Winsock error: " +
                                        Convert.ToString(System.Runtime.InteropServices.Marshal.GetLastWin32Error()));
                }
            }
            catch (Exception ex)
            {
//                errormessage += "Failed to connect for data transfer: " + ex.Message;
            }
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
        private static void Wait(Socket socket, int timeout = 10000)
        {
            int t = 50;
            while (socket.Available == 0 && timeout > 0)
            {
                timeout -= t;
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

        private static byte[] EncodingUtf8(string s)
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

        private string ExecCommand(string cmd)
        {            
           
            this.ControlSocket.Send(EncodingUtf8(cmd));
            return "";
        }
        
        private string ReadCmdReply()
        {
            var waitReceive = WaitReceive(ControlSocket);
            return waitReceive;
        }

        private string GetBack(string dir)
        {
            var count = dir.Count((c) => c == '/');
            string d = "..";
            for (int i = 0; i < count; i++)
            {
                d += "/..";
            }

            return d;
        }

        #endregion
    }
}