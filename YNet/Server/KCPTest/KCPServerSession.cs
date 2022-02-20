// Author：yaoshuai
// Email：yscode@126.com
// Date：2021-11-27
// ------------------------------
using System;
using System.Text;
using YNet.KCP;

public class KCPServerSession : KCPSession
{
    protected override void OnReceiveMessage(byte[] bytes)
    {
        string msg = Encoding.UTF8.GetString(bytes);
        if (msg == "ping")
        {
            lastPingTime = DateTime.UtcNow;
            Console.WriteLine($"收到ping消息：{lastPingTime}");
            SendMessage(Encoding.UTF8.GetBytes("pong"));
        }
    }

    // 10秒内未收到ping，断开连接
    DateTime lastPingTime = DateTime.UtcNow;
    protected override void OnUpdate(DateTime now)
    {
        TimeSpan ts = DateTime.UtcNow - lastPingTime;
        if (ts.Seconds > 10)
        {
            Console.WriteLine($"断开连接：{DateTime.UtcNow}");
            ServerCloseSession();
        }
    }
}