// Author：yaoshuai
// Email：yscode@126.com
// Date：2021-11-30
// ------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace YNet.IOCP
{
    public class IOCPServer<T> where T : IOCPSession, new()
    {
        private Socket skt;
        private readonly SocketAsyncEventArgs saea;

        private int curConnCount = 0;
        public int backlog = 100;
        private Semaphore acceptSeamaphore;
        private IOCPSessionPool<T> sessionPool;
        private List<T> sessionList;

        public IOCPServer()
        {
            saea = new SocketAsyncEventArgs();
            saea.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
        }
        public void StartAsServer(string serverHost, int serverPort, int maxConnCount, bool disableLog = true)
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
                throw new Exception($"ServerHost：{serverHost}，解析失败");
            }
            YNetTool.IsDisableLog = disableLog;

            curConnCount = 0;
            acceptSeamaphore = new Semaphore(maxConnCount, maxConnCount);
            sessionPool = new IOCPSessionPool<T>(maxConnCount);
            for (int i = 0; i < maxConnCount; i++)
            {
                T token = new T
                {
                    ServerSessionID = i
                };
                sessionPool.Push(token);
            }
            sessionList = new List<T>();

            IPEndPoint ip = new IPEndPoint(serverAddress, serverPort);
            skt = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            skt.Bind(ip);
            skt.Listen(backlog);

            YNetTool.ColorLog(YNetLogColor.Green, "启动服务器");

            StartAccept();
        }
        private void StartAccept()
        {
            saea.AcceptSocket = null;
            acceptSeamaphore.WaitOne();
            bool suspend = skt.AcceptAsync(saea);
            if (suspend == false)
            {
                ProcessAccept();
            }
        }
        private void ProcessAccept()
        {
            Interlocked.Increment(ref curConnCount);
            T session = sessionPool.Pop();
            lock (sessionList)
            {
                sessionList.Add(session);
            }
            session.InitSessionAfterConnected(saea.AcceptSocket);
            session.OnSessionCloseAction = OnSessionClose;
            StartAccept();
        }
        private void OnSessionClose(int tokenID)
        {
            int index = -1;
            for (int i = 0; i < sessionList.Count; i++)
            {
                if (sessionList[i].ServerSessionID == tokenID)
                {
                    index = i;
                    break;
                }
            }
            if (index != -1)
            {
                sessionPool.Push(sessionList[index]);
                lock (sessionList)
                {
                    sessionList.RemoveAt(index);
                }
                Interlocked.Decrement(ref curConnCount);
                acceptSeamaphore.Release();
            }
            else
            {
                YNetTool.Error("Token:{0} cannot find in server tokenLst.", tokenID);
            }
        }
        public void CloseServer()
        {
            for (int i = 0; i < sessionList.Count; i++)
            {
                sessionList[i].CloseSession();
            }
            sessionList = null;
            if (skt != null)
            {
                skt.Close();
                skt = null;
            }
        }
        public List<T> GetSessionList()
        {
            return sessionList;
        }

        private void IO_Completed(object sender, SocketAsyncEventArgs saea)
        {
            switch (saea.LastOperation)
            {
                case SocketAsyncOperation.Accept:
                    ProcessAccept();
                    break;
                default:
                    YNetTool.Warn("The last operation completed on the socket was not a accept op.");
                    break;
            }
        }
    }
}