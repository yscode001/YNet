// Author：yaoshuai
// Email：yscode@126.com
// Date：2021-11-27
// ------------------------------
using System;
using YNet.KCP;
using YNet;

public class KCPClinetNet : KCPClient<KCPClientSession>
{
    protected override void OnNetworkStateChanged(NetworkState networkState)
    {
        Console.WriteLine($"网络状态改变：{networkState}");
    }
}