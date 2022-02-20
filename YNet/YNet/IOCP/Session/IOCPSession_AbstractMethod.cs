// Author：yaoshuai
// Email：yscode@126.com
// Date：2021-11-30
// ------------------------------
namespace YNet.IOCP
{
    public abstract partial class IOCPSession
    {
        /// <summary>
        /// 当接收到消息时调用
        /// </summary>
        /// <param name="bytes">接收到消息的字节数组</param>
        protected abstract void OnReceiveMessage(byte[] bytes);
    }
}