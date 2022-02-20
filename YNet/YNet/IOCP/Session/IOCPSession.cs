// Author：yaoshuai
// Email：yscode@126.com
// Date：2021-11-25
// ------------------------------
using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace YNet.IOCP
{
    public abstract partial class IOCPSession
    {
        internal int ServerSessionID = 0;

        internal bool IsConnected { get; private set; } = false;
        internal Action<int> OnSessionCloseAction;

        private readonly SocketAsyncEventArgs receiveSAEA;
        private readonly SocketAsyncEventArgs sendSAEA;

        private Socket skt;
        private List<byte> readList = new List<byte>();
        private readonly Queue<byte[]> cacheQue = new Queue<byte[]>();
        private bool isWrite = false;

        public IOCPSession()
        {
            receiveSAEA = new SocketAsyncEventArgs();
            sendSAEA = new SocketAsyncEventArgs();
            receiveSAEA.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
            sendSAEA.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
            receiveSAEA.SetBuffer(new byte[2048], 0, 2048);
        }

        internal void InitSessionAfterConnected(Socket skt)
        {
            this.skt = skt;
            IsConnected = true;
            StartAsyncReceive();
        }

        private void StartAsyncReceive()
        {
            bool suspend = skt.ReceiveAsync(receiveSAEA);
            if (suspend == false)
            {
                ProcessReceive();
            }
        }
        private void ProcessReceive()
        {
            if (receiveSAEA.BytesTransferred > 0 && receiveSAEA.SocketError == SocketError.Success)
            {
                byte[] bytes = new byte[receiveSAEA.BytesTransferred];
                Buffer.BlockCopy(receiveSAEA.Buffer, 0, bytes, 0, receiveSAEA.BytesTransferred);
                readList.AddRange(bytes);
                ProcessByteList();
                StartAsyncReceive();
            }
            else
            {
                YNetTool.Warn("SessionID：{0} Close：{1}", ServerSessionID, receiveSAEA.SocketError.ToString());
                CloseSession();
            }
        }
        private void ProcessByteList()
        {
            byte[] buff = YNetTool.SplitLogicBytes(ref readList);
            if (buff != null)
            {
                OnReceiveMessage(buff);
                ProcessByteList();
            }
        }


        private void IO_Completed(object sender, SocketAsyncEventArgs saea)
        {
            switch (saea.LastOperation)
            {
                case SocketAsyncOperation.Receive:
                    ProcessReceive();
                    break;
                case SocketAsyncOperation.Send:
                    ProcessSend();
                    break;
                default:
                    YNetTool.Warn("The last operation completed on the socket was not a receive or send op.");
                    break;
            }
        }

        public void CloseSession()
        {
            if (skt != null)
            {
                IsConnected = false;

                OnSessionCloseAction?.Invoke(ServerSessionID);

                readList.Clear();
                cacheQue.Clear();
                isWrite = false;

                try
                {
                    skt.Shutdown(SocketShutdown.Send);
                }
                catch (Exception e)
                {
                    YNetTool.Error("Shutdown Socket Error:{0}", e.ToString());
                }
                finally
                {
                    skt.Close();
                    skt = null;
                }
            }
        }
    }
}