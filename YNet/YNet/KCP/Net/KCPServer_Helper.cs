// Author：yaoshuai
// Email：yscode@126.com
// Date：2021-11-27
// ------------------------------
using System.Net;

namespace YNet.KCP
{
    public partial class KCPServer<T> where T : KCPSession, new()
    {
        private void SendUdpMessage(byte[] bytes, IPEndPoint remotePoint)
        {
            if (udp != null && remotePoint != null && bytes != null && bytes.Length > 0)
            {
                udp.SendAsync(bytes, bytes.Length, remotePoint);
            }
        }
    }
}