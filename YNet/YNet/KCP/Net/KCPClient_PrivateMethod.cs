// Author：yaoshuai
// Email：yscode@126.com
// Date：2021-11-27
// ------------------------------
namespace YNet.KCP
{
    public abstract partial class KCPClient<T> where T : KCPSession, new()
    {
        private void BeginConnectState()
        {
            switch (CurrentNetworkState)
            {
                case NetworkState.NotConnect:
                    NetworkStateChanged(NetworkState.Connecting);
                    break;
                case NetworkState.Connecting:
                    break;
                case NetworkState.Connected:
                    break;
                case NetworkState.DisConnected:
                    NetworkStateChanged(NetworkState.Connecting);
                    break;
                case NetworkState.Timeout:
                    NetworkStateChanged(NetworkState.Connecting);
                    break;
                case NetworkState.Error:
                    NetworkStateChanged(NetworkState.Connecting);
                    break;
            }
        }
    }
}