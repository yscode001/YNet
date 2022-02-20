// Author：yaoshuai
// Email：yscode@126.com
// Date：2021-11-27
// ------------------------------
namespace YNet.KCP
{
    public abstract partial class KCPClient<T> where T : KCPSession, new()
    {
        /// <summary>
        /// 网络状态改变
        /// </summary>
        /// <param name="networkState">当前网络状态</param>
        protected abstract void OnNetworkStateChanged(NetworkState networkState);
    }
}