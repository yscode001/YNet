// Author：yaoshuai
// Email：yscode@126.com
// Date：2021-11-27
// ------------------------------
namespace YNet.KCP
{
    public abstract partial class KCPClient<T> where T : KCPSession, new()
    {
        public bool IsConnected => CurrentNetworkState == NetworkState.Connected;
        public NetworkState CurrentNetworkState { get; private set; } = NetworkState.NotConnect;

        public void NetworkStateChanged(NetworkState networkState)
        {
            if (CurrentNetworkState == networkState) { return; }
            CurrentNetworkState = networkState;
            OnNetworkStateChanged(networkState);
        }
    }
}