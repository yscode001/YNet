// Author：yaoshuai
// Email：yscode@126.com
// Date：2021-11-30
// ------------------------------
namespace YNet.IOCP
{
    public abstract partial class IOCPClient<T> where T : IOCPSession, new()
    {
        /// <summary>
        /// 网络状态改变
        /// </summary>
        /// <param name="networkState">当前网络状态</param>
        protected abstract void OnNetworkStateChanged(NetworkState networkState);
    }
}