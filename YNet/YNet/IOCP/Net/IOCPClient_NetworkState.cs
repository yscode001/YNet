// Author：yaoshuai
// Email：yscode@126.com
// Date：2021-11-30
// ------------------------------
namespace YNet.IOCP
{
    public abstract partial class IOCPClient<T> where T : IOCPSession, new()
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