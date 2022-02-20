// Author：yaoshuai
// Email：yscode@126.com
// Date：2021-11-25
// ------------------------------
using System;
using System.Buffers;
using System.Net.Sockets.Kcp;

namespace YNet.KCP
{
    public class KCPHandle : IKcpCallback
    {
        #region 发送数据
        private event Action<Memory<byte>> OutputAction;
        public void SetupOutputAction(Action<Memory<byte>> outputAction)
        {
            OutputAction = outputAction;
        }
        public void Output(IMemoryOwner<byte> buffer, int avalidLength)
        {
            using (buffer)
            {
                OutputAction?.Invoke(buffer.Memory.Slice(0, avalidLength));
            }
        }
        #endregion

        #region 接收数据
        private event Action<byte[]> ReceiveAction;
        public void SetupReceiveAction(Action<byte[]> receiveAction)
        {
            ReceiveAction = receiveAction;
        }
        public void ReceiveMessage(byte[] buffer)
        {
            ReceiveAction?.Invoke(buffer);
        }
        #endregion
    }
}