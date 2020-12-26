// ****************************************************
// 	文件：MSession.cs
// 	作者：积极向上小木木
// 	邮箱：positivemumu@126.com
// 	日期：2020/12/26 14:12:42
// 	功能：用于与服务器/客户端完成数据的接收与发送
// *****************************************************
using System;
using System.Net.Sockets;

namespace MNet
{
    public abstract class MSession<T> where T : MData
    {
        private Socket _socket;
        private Action _closeCB;

        #region Recevie

        public void ReceiveData(Socket socket, Action closeCB)
        {
            try
            {
                _socket = socket;
                _closeCB = closeCB;

                OnConnected();

                MPackage mPackage = new MPackage();

                this._socket.BeginReceive(
                    mPackage.HeadBuffer,
                    0,
                    mPackage.HeadLength,
                    SocketFlags.None,
                    new AsyncCallback(ReceiveHeadData),
                    mPackage);
            }
            catch (Exception e)
            {
                MTools.PrintLog("StartRcvData:" + e.Message, MLogLevels.Error);
            }
        }

        private void ReceiveHeadData(IAsyncResult ar)
        {
            try
            {
                MPackage mPackage = (MPackage)ar.AsyncState;
                if (_socket.Available == 0)
                {
                    OnDisconnected();
                    Clear();
                    return;
                }
                int len = _socket.EndReceive(ar);
                if (len > 0)
                {
                    mPackage.HeadIndex += len;
                    if (mPackage.HeadIndex < mPackage.HeadLength)
                    {
                        _socket.BeginReceive(
                            mPackage.HeadBuffer,
                            mPackage.HeadIndex,
                            mPackage.HeadLength - mPackage.HeadIndex,
                            SocketFlags.None,
                            new AsyncCallback(ReceiveHeadData),
                            mPackage);
                    }
                    else
                    {
                        mPackage.InitBodyBuffer();
                        _socket.BeginReceive(mPackage.BodyBuffer,
                            0,
                            mPackage.BodyLength,
                            SocketFlags.None,
                            new AsyncCallback(ReceiveBodyData),
                            mPackage);
                    }
                }
                else
                {
                    OnDisconnected();
                    Clear();
                }

            }
            catch (Exception e)
            {
                MTools.PrintLog("RcvHeadError:" + e.Message, MLogLevels.Error);
            }

        }

        private void ReceiveBodyData(IAsyncResult ar)
        {
            try
            {
                MPackage mPackage = (MPackage)ar.AsyncState;
                int len = _socket.EndReceive(ar);
                if (len > 0)
                {
                    mPackage.BodyIndex += len;
                    if (mPackage.BodyIndex < mPackage.BodyLength)
                    {
                        _socket.BeginReceive(mPackage.BodyBuffer,
                            mPackage.BodyIndex,
                            mPackage.BodyLength - mPackage.BodyIndex,
                            SocketFlags.None,
                            new AsyncCallback(ReceiveBodyData),
                            mPackage);
                    }
                    else
                    {
                        T data = MTools.Deserialize<T>(mPackage.BodyBuffer);
                        OnReceiveData(data);

                        mPackage.RestPackage();
                        _socket.BeginReceive(
                            mPackage.HeadBuffer,
                            0,
                            mPackage.HeadLength,
                            SocketFlags.None,
                            new AsyncCallback(ReceiveHeadData),
                            mPackage);
                    }
                }
                else
                {
                    OnDisconnected();
                    Clear();
                    return;
                }
            }
            catch (Exception e)
            {
                MTools.PrintLog("RcvBodyError:" + e.Message, MLogLevels.Error);
            }
        }
        #endregion

        #region Send

        public void SendData(T data)
        {
            byte[] sendData = MTools.AddHeadInfo(MTools.Serialize<T>(data));
            SendData(sendData);
        }

        public void SendData(byte[] data)
        {
            NetworkStream ns = null;
            try
            {
                ns = new NetworkStream(_socket);
                if (ns.CanWrite)
                {
                    ns.BeginWrite(data, 0, data.Length, new AsyncCallback(SendCB), ns);
                }
            }
            catch (Exception e)
            {
                MTools.PrintLog("SndMsgError:" + e.Message, MLogLevels.Error);
            }
        }

        private void SendCB(IAsyncResult ar)
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
                MTools.PrintLog("SndMsgError:" + e.Message, MLogLevels.Error);
            }
        }
        #endregion

        private void Clear()
        {
            if (_closeCB != null)
            {
                _closeCB();
            }
            _socket.Close();
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