// Author：yaoshuai
// Email：yscode@126.com
// Date：2021-11-25
// ------------------------------
namespace YNet
{
    /// <summary>
    /// 网络状态
    /// </summary>
    public enum NetworkState
    {
        /// <summary>
        /// 未连接，默认状态
        /// </summary>
        NotConnect,

        /// <summary>
        /// 正在连接
        /// </summary>
        Connecting,

        /// <summary>
        /// 已连接
        /// </summary>
        Connected,

        /// <summary>
        /// 断开连接
        /// </summary>
        DisConnected,

        /// <summary>
        /// 连接超时
        /// </summary>
        Timeout,

        /// <summary>
        /// 连接出错
        /// </summary>
        Error,
    }
}