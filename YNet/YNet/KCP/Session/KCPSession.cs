// Author：yaoshuai
// Email：yscode@126.com
// Date：2021-11-25
// ------------------------------
using System;
using System.Net;
using System.Net.Sockets.Kcp;
using System.Threading;

namespace YNet.KCP
{
    #region 属性
    public abstract partial class KCPSession
    {
        public uint SessionID { get; private set; } = 0;

        /// <summary>
        /// 会话关闭时调用
        /// </summary>
        internal Action<uint> OnSessionCloseAction;
        internal bool IsConnected => SessionID > 0;

        private IPEndPoint m_remoteIPPoint;
        private Action<byte[], IPEndPoint> m_udpSender;

        private KCPHandle m_kcpHandle;
        private Kcp m_kcp;

        private CancellationTokenSource m_cts;
        private CancellationToken m_ct;
    }
    #endregion
    #region 常规重写
    public abstract partial class KCPSession
    {
        public override string ToString()
        {
            return string.Format("SessionID：{0}", SessionID);
        }
        public override int GetHashCode()
        {
            return SessionID.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            if (obj is KCPSession)
            {
                return SessionID == (obj as KCPSession).SessionID;
            }
            return false;
        }
    }
    #endregion
}