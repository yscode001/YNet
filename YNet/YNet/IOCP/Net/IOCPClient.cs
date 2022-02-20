// Author：yaoshuai
// Email：yscode@126.com
// Date：2021-11-30
// ------------------------------
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace YNet.IOCP
{
    public abstract partial class IOCPClient<T> where T : IOCPSession, new()
    {
        public T ClientSession;

        private Socket skt;
        private readonly SocketAsyncEventArgs saea;

        public IOCPClient()
        {
            saea = new SocketAsyncEventArgs();
            saea.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
        }
        public void StartAsClient(string serverHost, int serverPort, bool disableLog = true)
        {
            IPAddress serverAddress = null;
            try
            {
                serverAddress = IPAddress.Parse(serverHost);
            }
            catch
            {
                try
                {
                    serverAddress = Dns.GetHostEntry(serverHost).AddressList.FirstOrDefault(m => m.AddressFamily == AddressFamily.InterNetwork);
                }
                catch { }
            }
            if (serverAddress == null)
            {
                NetworkStateChanged(NetworkState.Error);
                throw new Exception($"ServerHost：{serverHost}，解析失败");
            }

            YNetTool.IsDisableLog = disableLog;
            IPEndPoint ip = new IPEndPoint(serverAddress, serverPort);
            skt = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            saea.RemoteEndPoint = ip;

            YNetTool.ColorLog(YNetLogColor.Green, "启动客户端");

            StartConnect();
        }
        private void StartConnect()
        {
            BeginConnectState();
            bool suspend = skt.ConnectAsync(saea);
            if (suspend == false)
            {
                ProcessConnect();
            }
        }
        private void ProcessConnect()
        {
            ClientSession = new T();
            ClientSession.InitSessionAfterConnected(skt);
            NetworkStateChanged(NetworkState.Connected);
        }
        private void IO_Completed(object sender, SocketAsyncEventArgs saea)
        {
            switch (saea.LastOperation)
            {
                case SocketAsyncOperation.Connect:
                    ProcessConnect();
                    break;
                default:
                    YNetTool.Warn("The last operation completed on the socket was not a connect op.");
                    break;
            }
        }

        public void CloseClient()
        {
            if (ClientSession != null)
            {
                ClientSession.CloseSession();
                ClientSession = null;
            }
            if (skt != null)
            {
                skt = null;
            }
        }
    }
}