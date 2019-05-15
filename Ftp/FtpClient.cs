using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Ftp.entity;
using Ftp.@enum;
using Ftp.@interface;

namespace Ftp
{
    enum Mode
    {
        Passive, 
        Active
    }
    public class FtpClient
    {
        public string RemoteSystem { get; }
        private bool _isConnected;
        public string User { get; }
        public string Password { get; }
        public string Url { get; }
        private Mode _mode;
        public int Port { get; set; }
        private Socket _controlSocket;

        public FtpClient(string url, int port,string user, string password)
        {
            Url = url;
            User = user;
            Password = password;
            _isConnected = false;
            Port = port;
        }

        public FtpClient(string url, string user, string password)
        {
            this.Url = url;
            this.User = user;
            this.Password = password;
            this._isConnected = false;
            this.Port = 21;
        }

        public void Connect()
        {
            CloseControlSocket();
            string buf;
            _controlSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
//            _controlSocket.Connect(this.Url, this.Port);
            
            if (User == "")
            {
                //匿名
                throw new NotImplementedException();
            }
            else
            {
                _controlSocket.Connect(Url, Port);
                buf = WaitReceive(_controlSocket);
                Debug.WriteLine("BUF:" + buf);
                if (ReadCode(buf) == StateCode.NewUserReady)
                {
                    ExecCommand(Actions.User(this.User));
                    buf = WaitReceive(_controlSocket);
                    Debug.WriteLine(buf);
                    ExecCommand(Actions.Password(this.Password));
                    buf = WaitReceive(_controlSocket);
                    Debug.WriteLine(buf);
                    _isConnected = true;
                }
            }
        }

        public void DisConnect()
        {
            if (_controlSocket != null && this._isConnected)
            {
                ExecCommand(Actions.Quit());
            }
            CloseControlSocket();
        }

        public Socket GetDataSocket()
        {
            Socket dataSocket = null;
            if (this._mode == Mode.Passive)
            {
                dataSocket = this.GetPasvDataSocket();
            }
            else
            {
                throw new NotImplementedException();
            }

            return dataSocket;
        }


        private Socket GetPasvDataSocket()
        {
            string buf = "";
            var dataSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            
            this.ExecCommand(Actions.Passsive());
            buf = WaitReceive(_controlSocket);
            Debug.WriteLine(buf);
            if (ReadCode(buf) == StateCode.EnterPassiveMode)
            {
                var dataSocketIpAddress = ParsePassiveIp(buf);
                Debug.WriteLine(dataSocketIpAddress);
                dataSocket.Connect(dataSocketIpAddress);
                return dataSocket;
            }

            return null;
        }
        private void CloseControlSocket()
        {
            if (this._controlSocket != null)
            {
                this._controlSocket.Close();
                this._controlSocket = null;
            }

            this._isConnected = false;
        }

        public List<VisualFile> GetRemoteFiles()
        {
            string cmdBuf;
            string dataBuf;
            int curSize = 0;
            int intCount = 0;
            Socket dataSocket = GetDataSocket();

            dataBuf = WaitReceive(dataSocket);
            Debug.WriteLine(dataBuf);
            Console.WriteLine(dataBuf);

            return null;

        }
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
            return result.ToString();

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
        private static void Wait(int millSeconds = 20)
        {
            Thread.Sleep(TimeSpan.FromMilliseconds(millSeconds));
        }
        
        private static void Wait(Socket socket, int timeout = 10000)
        {
            int t = 50;
            while (socket.Available == 0 && timeout > 0)
            {
                timeout -= t;
                Wait(t);
            }
        }

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

        private string WaitReceive(Socket socket)
        {
            Wait(socket);
            return SocketReceive(socket);
        }

        private string ExecCommand(string cmd)
        {

            this._controlSocket.Send(EncodingUtf8(cmd));
            return "";
        }

        private string ReadCmdReply()
        {
            var waitReceive = WaitReceive(_controlSocket);
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
    }
}
