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
        /// 驱动方法
        /// </summary>
        /// <param name="now"></param>
        protected abstract void OnUpdate(DateTime now);

        /// <summary>
        /// 当接收到消息时调用
        /// </summary>
        /// <param name="bytes">接收到消息的字节数组</param>
        protected abstract void OnReceiveMessage(byte[] bytes);

        /// <summary>
        /// SessionUpdate时出现异常
        /// </summary>
        /// <param name="ex"></param>
        protected abstract void UpdateException(Exception ex);
    }
}