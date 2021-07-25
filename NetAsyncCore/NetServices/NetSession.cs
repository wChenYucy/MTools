
// ****************************************************
//     文件：NetSession.cs
//     作者：积极向上小木木
//     邮箱: positivemumu@126.com
//     日期：2021/07/23 15:08
//     功能：网络会话类，每个Socket都有一个自己的会话类。
//          负责接受信息、处理信息（继承方法）、发送信息。
// *****************************************************

using System;
using System.Net.Sockets;
using DetailLogTool;

namespace NetAsyncCore
{
    public class NetSession<T> where T : NetData
    {
        protected Socket clientSocket;
        protected Action closeCallBack;

        #region ReceiveData

        public void ReceiveData(Socket socket, Action closeCB)
        {
            OnConnected();
            try
            {
                clientSocket = socket;
                closeCallBack = closeCB;
                NetPackage mPackage = new NetPackage();

                this.clientSocket.BeginReceive(
                    mPackage.HeadBuffer,
                    0,
                    mPackage.HeadLength,
                    SocketFlags.None,
                    new AsyncCallback(ReceiveHeadData),
                    mPackage);
            }
            catch (Exception e)
            {
                DetailLog.Error("StartRcvData:" + e.Message);
            }
        }

        private void ReceiveHeadData(IAsyncResult ar)
        {
            try
            {
                NetPackage mPackage = (NetPackage) ar.AsyncState;
                if (clientSocket.Available == 0)
                {
                    Clear();
                    return;
                }

                int len = clientSocket.EndReceive(ar);
                if (len > 0)
                {
                    mPackage.HeadIndex += len;
                    if (mPackage.HeadIndex < mPackage.HeadLength)
                    {
                        clientSocket.BeginReceive(
                            mPackage.HeadBuffer,
                            mPackage.HeadIndex,
                            mPackage.HeadLength - mPackage.HeadIndex,
                            SocketFlags.None,
                            ReceiveHeadData,
                            mPackage);
                    }
                    else
                    {
                        mPackage.InitBodyBuffer();
                        clientSocket.BeginReceive(mPackage.BodyBuffer,
                            0,
                            mPackage.BodyLength,
                            SocketFlags.None,
                            ReceiveBodyData,
                            mPackage);
                    }
                }
                else
                {
                    Clear();
                }
            }
            catch (Exception e)
            {
                DetailLog.Error("RcvHeadError:" + e.Message);
            }
        }
        
        private void ReceiveBodyData(IAsyncResult ar)
        {
            try
            {
                NetPackage mPackage = (NetPackage)ar.AsyncState;
                int len = clientSocket.EndReceive(ar);
                if (len > 0)
                {
                    mPackage.BodyIndex += len;
                    if (mPackage.BodyIndex < mPackage.BodyLength)
                    {
                        clientSocket.BeginReceive(mPackage.BodyBuffer,
                            mPackage.BodyIndex,
                            mPackage.BodyLength - mPackage.BodyIndex,
                            SocketFlags.None,
                            ReceiveBodyData,
                            mPackage);
                    }
                    else
                    {
                        //处理数据
                        T data = NetTool.Deserialize<T>(mPackage.BodyBuffer);
                        OnReceiveData(data);

                        mPackage.RestPackage();
                        clientSocket.BeginReceive(
                            mPackage.HeadBuffer,
                            0,
                            mPackage.HeadLength,
                            SocketFlags.None,
                            ReceiveHeadData,
                            mPackage);
                    }
                }
                else
                {
                    Clear();
                }
            }
            catch (Exception e)
            {
                DetailLog.Error("RcvBodyError:" + e.Message);
            }
        }

        #endregion

        #region SendData

        public void SendData(T data)
        {
            byte[] sendData = NetTool.PackNetData(data);
            //byte[] sendData = NetTool.AddHeadInfo(NetTool.Serialize<T>(data));
            SendData(sendData);
        }

        internal void SendData(byte[] data)
        {
            NetworkStream ns = null;
            try
            {
                ns = new NetworkStream(clientSocket);
                if (ns.CanWrite)
                {
                    ns.BeginWrite(data, 0, data.Length, new AsyncCallback(SendCallBack), ns);
                }
            }
            catch (Exception e)
            {
                DetailLog.Error("SndMsgError:" + e.Message);
            }
        }

        private void SendCallBack(IAsyncResult ar)
        {
            NetworkStream ns = (NetworkStream)ar.AsyncState;
            try
            {
                ns.EndWrite(ar);
                ns.Flush();
                ns.Close();
            }
            catch (Exception e)
            {
                DetailLog.Error("SndMsgError:" + e.Message);
            }
        }

        #endregion
        
        
        internal void Clear()
        {
            closeCallBack?.Invoke();
            OnDisconnected();
            clientSocket.Close();
        }
        

        protected virtual void OnConnected()
        {

        }

        protected virtual void OnReceiveData(T data)
        {

        }

        protected virtual void OnDisconnected()
        {

        }
        
    }
}