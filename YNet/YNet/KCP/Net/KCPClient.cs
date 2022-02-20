// Author：yaoshuai
// Email：yscode@126.com
// Date：2021-11-27
// ------------------------------
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace YNet.KCP
{
    public abstract partial class KCPClient<T> where T : KCPSession, new()
    {
        private UdpClient udp;
        private IPEndPoint remotePoint;

        private readonly CancellationTokenSource cts;
        private readonly CancellationToken ct;

        public KCPClient()
        {
            cts = new CancellationTokenSource();
            ct = cts.Token;
        }
    }

    public abstract partial class KCPClient<T> where T : KCPSession, new()
    {
        public T ClientSession;

        /// <summary>
        /// 启动客户端
        /// </summary>
        /// <param name="serverHost"></param>
        /// <param name="serverPort"></param>
        /// <param name="kcpConvID">0表示服务器分配，大于0表示客户端指定</param>
        /// <param name="disableLog"></param>
        public void StartAsClient(string serverHost, int serverPort, uint kcpConvID = 0, bool disableLog = true)
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
                NetworkStateChanged(NetworkState.Error);
                throw new Exception($"ServerHost：{serverHost}，解析失败");
            }

            remotePoint = new IPEndPoint(serverAddress, serverPort);
            udp = new UdpClient(0); // 0表示客户端端口由系统分配
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                udp.Client.IOControl((IOControlCode)(-1744830452), new byte[] { 0, 0, 0, 0 }, null);
            }
            YNetTool.ColorLog(YNetLogColor.Green, "启动客户端");
            Task.Run(ClientReciveMessage, ct);

            if (kcpConvID > 0)
            {
                // 初始化Session
                ClientSession = new T();
                ClientSession.InitSessionAfterConnected(kcpConvID, SendUdpMessage, remotePoint);
                ClientSession.OnSessionCloseAction = OnClientSessionClose;

                NetworkStateChanged(NetworkState.Connected);
            }
        }

        /// <summary>
        /// 连接服务器
        /// </summary>
        /// <param name="interval">间隔多少毫秒连一次</param>
        /// <param name="maxintervalSum">最多连接多少毫秒</param>
        /// <returns></returns>
        public Task<bool> ConnectServer(int interval, int maxintervalSum = 10000)
        {
            BeginConnectState();
            SendUdpMessage(new byte[4], remotePoint);
            int checkTimes = 0;
            Task<bool> task = Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(interval);
                    if (ClientSession != null && ClientSession.IsConnected)
                    {
                        checkTimes = 0;
                        return true;
                    }
                    checkTimes += interval;
                    if (checkTimes > maxintervalSum)
                    {
                        NetworkStateChanged(NetworkState.Timeout);
                        return false;
                    }
                }
            });
            return task;
        }

        private async void ClientReciveMessage()
        {
            UdpReceiveResult result;
            while (true)
            {
                try
                {
                    if (ct.IsCancellationRequested)
                    {
                        YNetTool.ColorLog(YNetLogColor.Cyan, "客户端 ClientReciveMessage 任务 已取消");
                        break;
                    }
                    else
                    {
                        result = await udp.ReceiveAsync();
                        if (Equals(remotePoint, result.RemoteEndPoint))
                        {
                            uint sessionID = BitConverter.ToUInt32(result.Buffer, 0);
                            if (sessionID == 0)
                            {
                                if (ClientSession != null && ClientSession.IsConnected)
                                {
                                    // 已经建立连接，初始化完成了，收到了多个sessionID，直接丢弃。
                                    YNetTool.Warn("客户端初始化已完成，无需重复初始化");
                                }
                                else
                                {
                                    // 未初始化，收到服务器分配的sessionID数据，初始化一个客户端session
                                    sessionID = BitConverter.ToUInt32(result.Buffer, 4);
                                    YNetTool.ColorLog(YNetLogColor.Green, "客户端收到SessionID：{0}，即将进行初始化", sessionID);

                                    // 初始化Session
                                    ClientSession = new T();
                                    ClientSession.InitSessionAfterConnected(sessionID, SendUdpMessage, remotePoint);
                                    ClientSession.OnSessionCloseAction = OnClientSessionClose;

                                    NetworkStateChanged(NetworkState.Connected);
                                }
                            }
                            else
                            {
                                // 处理业务逻辑
                                if (ClientSession != null && ClientSession.IsConnected)
                                {
                                    ClientSession.KCPInputData(result.Buffer);
                                }
                                else
                                {
                                    // 没初始化且sessionID != 0，数据消息提前到了，直接丢弃消息，直到初
                                    // 始化完成，kcp重传再开始处理。
                                    YNetTool.Warn("客户端未进行初始化，无法处理业务数据");
                                }
                            }
                        }
                        else
                        {
                            YNetTool.Warn("客户端 - 服务器IP 与 此条消息来自的服务器IP不一致");
                        }
                    }
                }
                catch (Exception e)
                {
                    YNetTool.Warn("客户端 ClientReciveMessage 任务 异常：{0}", e.ToString());
                }
            }
        }

        /// <summary>
        /// 客户端Session关闭时调用
        /// </summary>
        /// <param name="sessionID"></param>
        private void OnClientSessionClose(uint sessionID)
        {
            cts.Cancel();
            if (udp != null)
            {
                udp.Close();
                udp = null;
            }
            YNetTool.Warn("客户端Session关闭，SessionID：{0}", sessionID);
        }

        /// <summary>
        /// 关闭客户端
        /// </summary>
        public void CloseClient()
        {
            if (IsConnected && ClientSession != null && ClientSession.IsConnected)
            {
                ClientSession.CloseSession();
            }
            NetworkStateChanged(NetworkState.DisConnected);
        }
    }
}