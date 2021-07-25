
// ****************************************************
//     文件：NetSession.cs
//     作者：积极向上小木木
//     邮箱: positivemumu@126.com
//     日期：2021/07/24 14:13
//     功能：信息存储类，用于存储接受到的网络信息。
//          方便处理类进行处理
// *****************************************************

using System;
using System.Diagnostics;
using System.Net.Sockets;
using DetailLogTool;

namespace NetSelectCore
{
    public class NetSession<T> where T : NetData
    {
        protected Socket clientSocket;
        private Action closeCallBack;

        private NetPackage package;

        public void InitNetSession(Socket socket,Action closeCB,bool isClient)
        {
            clientSocket = socket;
            closeCallBack = closeCB;
            package = new NetPackage();
            OnConnected();
            if (isClient)
            {
                clientSocket.BeginReceive(package.Bytes,package.WriteIndex,package.Remain,0, ClientReceiveData, socket);
            }
        }

        #region ReceiveData

        public void ServerReceiveData()
        {
            int count = 0;
            if (package.Remain <= 0)
            {
                ProcessData();
                package.CheckAndMoveBytes();
                while (package.Remain<=0)
                {
                    int expandSize = package.Length < NetPackage.DEFAULT_SIZE
                        ? NetPackage.DEFAULT_SIZE
                        : package.Length;
                    package.Resize(expandSize * 2);
                }
            }
            try
            {
                count = clientSocket.Receive(package.Bytes, package.WriteIndex, package.Remain, 0);
            }
            catch (SocketException e)
            {
                DetailLog.Error("ServerReceiveError:" + e.Message);
                Clear();
                return;
            }

            if (count <= 0)
            {
                Clear();
                return;
            }
            //根据协议内容处理消息，处理结果返回客户端
            package.WriteIndex += count;
            ProcessData();
            package.CheckAndMoveBytes();
        }

        public void ClientReceiveData(IAsyncResult ar)
        {
            try
            {
                Socket socket = (Socket)ar.AsyncState;
                int count = socket.EndReceive(ar);
                if (count <= 0) 
                {
                    Clear();
                    return;
                }
                package.WriteIndex += count;
                ProcessData();
                if (package.Remain < 8) 
                {
                    package.MoveBytes();
                    package.Resize(package.Length * 2);
                }
                socket.BeginReceive(package.Bytes, package.WriteIndex, package.Remain, 0, ClientReceiveData, socket);
            }
            catch (SocketException e)
            {
                DetailLog.Error("ClientReceiveError:" + e.Message);
                Clear();
            }
        }
        private void ProcessData()
        {
            if (package.Length <= 4 || package.ReadIndex < 0)
            {
                return;
            }
            int dataLength = BitConverter.ToInt32(package.Bytes, package.ReadIndex);
            if (package.Length < dataLength + 4)
            {
                return;
            }
            package.ReadIndex += 4;
            try
            {
                T data= NetTool.Deserialize<T>(package.Bytes,package.ReadIndex,dataLength);
                OnReceiveData(data);
            }
            catch (Exception e)
            {
                DetailLog.Error("ProcessDataError:" + e.Message);
                return;
            }
            package.ReadIndex += dataLength;
            package.CheckAndMoveBytes();
            
            if (package.Length > 4)
            {
                ProcessData();
            }
            
        }

        #endregion

        #region SendData

        public void SendData(T data)
        {
            byte[] sendData = NetTool.PackNetData(data);
            SendData(sendData);
        }

        internal void SendData(byte[] data)
        {
            try
            {
                clientSocket.BeginSend(data, 0, data.Length, 0, null, null);
            }
            catch (Exception e)
            {
                DetailLog.Error("SendMessageError:" + e.Message);
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