// Author：yaoshuai
// Email：yscode@126.com
// Date：2021-11-26
// ------------------------------
using System;

namespace YNet.KCP
{
    public abstract partial class KCPSession
    {
        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="bytes"></param>
        public void SendMessage(byte[] bytes)
        {
            if (bytes == null || bytes.Length <= 0)
            {
                YNetTool.Warn("待发送消息为空，无法发送");
                return;
            }
            if (!IsConnected)
            {
                YNetTool.Warn("网络连接已断开，无法发送消息");
            }
            m_kcp.Send(bytes);
        }

        /// <summary>
        /// 关闭会话，只有Server可以调用，Client不可以调用
        /// </summary>
        protected void ServerCloseSession()
        {
            CloseSession();
        }
    }
}