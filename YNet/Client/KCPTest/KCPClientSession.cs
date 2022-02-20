// Author：yaoshuai
// Email：yscode@126.com
// Date：2021-11-27
// ------------------------------
using System;
using System.Text;
using YNet.KCP;

public class KCPClientSession : KCPSession
{
    internal DateTime sendPingTime = DateTime.UtcNow;
    internal DateTime lastPongTime = DateTime.UtcNow;

    protected override void OnReceiveMessage(byte[] messageBytes)
    {
        string message = Encoding.UTF8.GetString(messageBytes);
        if (message == "pong")
        {
            lastPongTime = DateTime.UtcNow;
            TimeSpan ts = lastPongTime - sendPingTime;
            Console.WriteLine($"客户端：收到消息：{message}，网络延时：{(ulong)ts.TotalMilliseconds}ms");
        }
        else
        {
            Console.WriteLine($"客户端：收到消息：{message}");
        }
    }

    protected override void OnUpdate(DateTime now)
    {
        TimeSpan span = now - lastPongTime;
        if (span.Seconds > 10)
        {
            // 断线重连
            KCPClientRoot.Instance.BeginConnect();
        }
    }
}