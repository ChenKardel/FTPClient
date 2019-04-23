using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Remoting.Channels;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Ftp.@enum;
using Ftp.@interface;
//using Action = System.Action;

namespace Ftp
{
    public class FtpConnector: IConnector
    {
        #region variables;
        private string _username;
        private string _password;
        private string _hostname;
        private int _port;
        private Socket _socket;
        private int _timeout;
        private bool _isAnonymous = true;
        #endregion

        #region getter&setter
        #endregion

        #region PublicFunction
        public FtpConnector(string hostname, string username="", string password="", int port =21, int timeout=2000)
        {
            if (username.Equals("") && password.Equals(""))
            {
                _username = username;
                _password = password;
                _isAnonymous = false;
            }
            _hostname = hostname;
            _port = port;
            _timeout = timeout;
            _socket = Connect(hostname, port, username, password,timeout);
        }

     
        public Socket Connect(string host, int port, int timeout)
        {
            return Connect(host, port, "", "", timeout);
        }

       
        public Socket Connect(string host, int port, string username, string password, int timeout)
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
            {
                ReceiveTimeout = timeout,
                SendTimeout = timeout
            };
            Regex regex = new Regex(@"[0-9]+\.[0-9]+\.[0-9]+\.[0-9]+");
            var isMatch = regex.IsMatch(host);
            IPAddress ipAddress = isMatch ? IPAddress.Parse(host) : Dns.GetHostAddresses(host)[0];
            socket.Connect(ipAddress, port);
            WaitReceive(socket);
            var userCmd = Actions.User(username);
            socket.Send(EncodingUtf8(userCmd));
            WaitReceive(socket);
            var pwdCmd = Actions.Password(password);
            socket.Send(EncodingUtf8(pwdCmd));
            WaitReceive(socket);
            socket.Send(EncodingUtf8(Actions.Passsive()));
            var receiveMsg = WaitReceive(socket);
            if (ReadCode(receiveMsg) == StateCode.EnterPassiveMode)
            {
                var leftPar = receiveMsg.IndexOf("(", StringComparison.Ordinal)+1;
                var rightPar = receiveMsg.IndexOf(")", StringComparison.Ordinal);

                var passive = receiveMsg.Substring(leftPar, rightPar - leftPar).Split(',');
                var server = passive[0] + "." + passive[1] + "." + passive[2] + "." + passive[3];
                port = (int.Parse(passive[4]) << 8) + int.Parse(passive[5]);

//
//                if (socket.Connected)
//                {
//                    socket.Close();
//                }
//                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
//                {
//                    ReceiveTimeout = timeout,
//                    SendTimeout = timeout
//                };
//                socket.Connect(server, port);
//                Debug.WriteLine(WaitReceive(socket));
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

        public List<VisualFile> ListRemoteFiles()
        {
            return ListRemoteFiles(".");
        }

        public List<VisualFile> ListRemoteFiles(string dirname)
        {
            Debug.WriteLine(Actions.NList("data"));
            _socket.Send(EncodingUtf8(Actions.NList("data")));
            Debug.WriteLine(WaitReceive(_socket));
            return null;
        }

        public List<VisualFile> ListLocalFiles()
        {
            throw new NotImplementedException();
        }

        public List<VisualFile> ListLocalFiles(string dirname)
        {
            throw new NotImplementedException();
        }

        public Socket Disconnect()
        {
            throw new NotImplementedException();
        }

        #endregion
        #region privateFunction

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

        private static void Wait(int millSeconds=20)
        {
            Thread.Sleep(TimeSpan.FromMilliseconds(millSeconds));
        }

        private static void Wait(Socket socket, int timeout=5000)
        {
            int t = 20;
            while (socket.Available == 0 && timeout > 0)
            {
                timeout -= 20;
                Wait(20);
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

        private byte[] EncodingUtf8(string s)
        {
            return Encoding.UTF8.GetBytes(s);
        }

        private string WaitReceive(Socket socket)
        {
            Wait(socket);
            return SocketReceive(socket);
        }
        #endregion
    }
}
