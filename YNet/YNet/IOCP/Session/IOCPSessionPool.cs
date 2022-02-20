// Author：yaoshuai
// Email：yscode@126.com
// Date：2021-11-25
// ------------------------------
using System.Collections.Generic;

namespace YNet.IOCP
{
    public class IOCPSessionPool<T> where T : IOCPSession, new()
    {
        private readonly Stack<T> stackPool;
        public int Size => stackPool.Count;

        public IOCPSessionPool(int capacity)
        {
            stackPool = new Stack<T>(capacity);
        }

        public T Pop()
        {
            lock (stackPool)
            {
                return stackPool.Pop();
            }
        }

        public void Push(T session)
        {
            if (session == null)
            {
                YNetTool.Error("放入IOCP池中的Session不能为空");
            }
            lock (stackPool)
            {
                stackPool.Push(session);
            }
        }
    }
}