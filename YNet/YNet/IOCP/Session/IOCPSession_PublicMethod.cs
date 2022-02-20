// Author：yaoshuai
// Email：yscode@126.com
// Date：2021-11-30
// ------------------------------
using System.Net.Sockets;

namespace YNet.IOCP
{
    public abstract partial class IOCPSession
    {
        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public bool SendMessage(byte[] bytes)
        {
            if (bytes == null || bytes.Length <= 0)
            {
                YNetTool.Warn("待发送消息为空，无法发送");
                return false;
            }
            if (!IsConnected)
            {
                YNetTool.Warn("网络连接已断开，无法发送消息");
                return false;
            }
            if (isWrite)
            {
                cacheQue.Enqueue(bytes);
                return true;
            }
            isWrite = true;
            sendSAEA.SetBuffer(bytes, 0, bytes.Length);
            bool suspend = skt.SendAsync(sendSAEA);
            if (suspend == false)
            {
                ProcessSend();
            }
            return true;
        }

        private void ProcessSend()
        {
            if (sendSAEA.SocketError == SocketError.Success)
            {
                isWrite = false;
                if (cacheQue.Count > 0)
                {
                    byte[] item = cacheQue.Dequeue();
                    SendMessage(item);
                }
            }
            else
            {
                YNetTool.Error("消息发送失败：{0}", sendSAEA.SocketError.ToString());
                CloseSession();
            }
        }
    }
}