// Author：yaoshuai
// Email：yscode@126.com
// Date：2021-11-27
// ------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace YNet.KCP
{
    public partial class KCPServer<T> where T : KCPSession, new()
    {
        private UdpClient udp;

        private readonly CancellationTokenSource cts;
        private readonly CancellationToken ct;

        public KCPServer()
        {
            cts = new CancellationTokenSource();
            ct = cts.Token;
        }
    }
    public partial class KCPServer<T> where T : KCPSession, new()
    {
        private Dictionary<uint, T> ServerSessionDict = null;

        public void StartAsServer(string serverHost, int serverPort, bool disableLog = true)
        {
            YNetTool.IsDisableLog = disableLog;
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

            ServerSessionDict = new Dictionary<uint, T>();
            udp = new UdpClient(new IPEndPoint(serverAddress, serverPort));
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                udp.Client.IOControl((IOControlCode)(-1744830452), new byte[] { 0, 0, 0, 0 }, null);
            }
            YNetTool.ColorLog(YNetLogColor.Green, "启动服务器");
            Task.Run(ServerReciveMessage, ct);
        }

        private async void ServerReciveMessage()
        {
            UdpReceiveResult result;
            while (true)
            {
                try
                {
                    if (ct.IsCancellationRequested)
                    {
                        YNetTool.ColorLog(YNetLogColor.Cyan, "服务器 ServerReciveMessage 任务 已取消");
                        break;
                    }
                    result = await udp.ReceiveAsync();
                    uint sessionID = BitConverter.ToUInt32(result.Buffer, 0);
                    if (sessionID == 0)
                    {
                        // 生成SessionID，发送给客户端，建立连接
                        sessionID = GenerateUniqueSessionID();
                        byte[] sid_bytes = BitConverter.GetBytes(sessionID);
                        byte[] conv_bytes = new byte[8];
                        Array.Copy(sid_bytes, 0, conv_bytes, 4, 4);
                        SendUdpMessage(conv_bytes, result.RemoteEndPoint);
                    }
                    else
                    {
                        // 业务数据
                        if (ServerSessionDict.TryGetValue(sessionID, out T session))
                        {
                            session = ServerSessionDict[sessionID];
                            // 更新RemoteEndPoint，因为客户端换网后，对应的公网IP会变
                            session.UpdateRemoteIPPoint(result.RemoteEndPoint);
                        }
                        else
                        {
                            session = new T();
                            session.InitSessionAfterConnected(sessionID, SendUdpMessage, result.RemoteEndPoint);
                            session.OnSessionCloseAction = OnServerSessionClose;
                            lock (ServerSessionDict)
                            {
                                ServerSessionDict.Add(sessionID, session);
                            }
                        }
                        session.KCPInputData(result.Buffer);
                    }
                }
                catch (Exception e)
                {
                    YNetTool.Warn("服务器 ServerReciveMessage 任务 异常：{0}", e.ToString());
                }
            }
        }

        /// <summary>
        /// 服务器存的Session关闭时调用
        /// </summary>
        /// <param name="sessionID"></param>
        private void OnServerSessionClose(uint sessionID)
        {
            if (ServerSessionDict.ContainsKey(sessionID))
            {
                lock (ServerSessionDict)
                {
                    ServerSessionDict.Remove(sessionID);
                    YNetTool.Warn("服务器Session关闭，移除 Session：{0}", sessionID);
                }
            }
            else
            {
                YNetTool.Warn("服务器Session关闭，但移除 Session：{0} 失败，未找到此Session", sessionID);
            }
        }

        /// <summary>
        /// 关闭服务器
        /// </summary>
        /// <param name="operate"></param>
        public void CloseServer()
        {
            foreach (var item in ServerSessionDict)
            {
                item.Value.CloseSession();
            }
            ServerSessionDict = null;
            if (udp != null)
            {
                udp.Close();
                udp = null;
                cts.Cancel();
            }
        }

        /// <summary>
        /// 关闭某个会话
        /// </summary>
        /// <param name="sessionID"></param>
        public void CloseSession(uint sessionID)
        {
            if (ServerSessionDict.ContainsKey(sessionID))
            {
                ServerSessionDict[sessionID].CloseSession();
            }
        }

        /// <summary>
        /// 广播消息
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="sessionIDArray"></param>
        public void BroadCastMessage(byte[] bytes, uint[] sessionIDArray = null)
        {
            if (sessionIDArray != null && sessionIDArray.Length > 0)
            {
                foreach (var item in ServerSessionDict)
                {
                    if (sessionIDArray.Contains(item.Key))
                    {
                        item.Value.SendMessage(bytes);
                    }
                }
            }
            else
            {
                foreach (var item in ServerSessionDict)
                {
                    item.Value.SendMessage(bytes);
                }
            }
        }

        #region 根据客户端的连接生成对应的SessionID
        private uint ClientSessionIDInServer = uint.MinValue;
        private uint GenerateUniqueSessionID()
        {
            lock (ServerSessionDict)
            {
                while (true)
                {
                    ClientSessionIDInServer += 1;
                    if (ClientSessionIDInServer == uint.MaxValue)
                    {
                        ClientSessionIDInServer = 1;
                    }
                    if (!ServerSessionDict.ContainsKey(ClientSessionIDInServer))
                    {
                        break;
                    }
                }
            }
            return ClientSessionIDInServer;
        }
        #endregion
    }
}