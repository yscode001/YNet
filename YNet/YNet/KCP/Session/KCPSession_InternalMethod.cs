// Author：yaoshuai
// Email：yscode@126.com
// Date：2021-11-26
// ------------------------------
using System;
using System.Net;
using System.Net.Sockets.Kcp;
using System.Threading;
using System.Threading.Tasks;

namespace YNet.KCP
{
    #region 内部赋值方法
    public abstract partial class KCPSession
    {
        internal void InitSessionAfterConnected(uint sessionID, Action<byte[], IPEndPoint> udpSender, IPEndPoint remoteIPPoint)
        {
            SessionID = sessionID;
            m_udpSender = udpSender;
            m_remoteIPPoint = remoteIPPoint;

            m_kcpHandle = new KCPHandle();
            m_kcp = new Kcp(sessionID, m_kcpHandle);
            m_kcp.NoDelay(1, 10, 2, 1);
            m_kcp.WndSize(64, 64);
            m_kcp.SetMtu(512);

            m_kcpHandle.SetupOutputAction((buffer) =>
            {
                // 发送数据
                m_udpSender(buffer.ToArray(), m_remoteIPPoint);
            });
            m_kcpHandle.SetupReceiveAction((buffer) =>
            {
                // 接收数据
                OnReceiveMessage(buffer);
            });

            m_cts = new CancellationTokenSource();
            m_ct = m_cts.Token;
            Task.Run(Update, m_ct);
        }

        internal void UpdateRemoteIPPoint(IPEndPoint remoteIPPoint)
        {
            if (remoteIPPoint == null || IPEndPointEqual(remoteIPPoint, m_remoteIPPoint)) { return; }
            m_remoteIPPoint = remoteIPPoint;
        }

        private bool IPEndPointEqual(IPEndPoint a, IPEndPoint b)
        {
            return a.Address.Equals(b.Address) && a.Port == b.Port;
        }

        private async void Update()
        {
            try
            {
                while (true)
                {
                    if (m_ct.IsCancellationRequested)
                    {
                        YNetTool.ColorLog(YNetLogColor.Cyan, "KCPSession Update 任务已取消");
                        break;
                    }
                    else
                    {
                        DateTime now = DateTime.UtcNow;
                        OnUpdate(now);
                        if (m_ct.IsCancellationRequested)
                        {
                            // 外界用的时候有可能在OnUpdate里面断开连接，所以这里再次判断
                            YNetTool.ColorLog(YNetLogColor.Cyan, "KCPSession Update 任务已取消");
                            break;
                        }
                        else
                        {
                            m_kcp.Update(now);
                            int len;
                            while ((len = m_kcp.PeekSize()) > 0)
                            {
                                var buffer = new byte[len];
                                if (m_kcp.Recv(buffer) >= 0)
                                {
                                    m_kcpHandle.ReceiveMessage(buffer);
                                }
                            }
                            await Task.Delay(KCPConfig.UpdateMsgTimeInterval);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                YNetTool.Warn("KCPSession Update 异常：{0}", e.ToString());
                UpdateException(e);
            }
        }

        internal void KCPInputData(byte[] buffer)
        {
            m_kcp.Input(buffer.AsSpan());
        }

        internal void CloseSession()
        {
            if (IsConnected)
            {
                m_cts.Cancel();

                OnSessionCloseAction?.Invoke(SessionID);
                OnSessionCloseAction = null;
                SessionID = 0;

                m_kcpHandle = null;
                m_kcp = null;
                m_cts = null;
            }
        }
    }
    #endregion
}