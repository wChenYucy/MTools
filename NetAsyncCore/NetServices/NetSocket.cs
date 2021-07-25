// ****************************************************
//     文件：NetSocket.cs
//     作者：积极向上小木木
//     邮箱: positivemumu@126.com
//     日期：2021/07/23 15:02
//     功能：异步网络核心类，负责启动、连接Socket并创建会话。
// *****************************************************

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using DetailLogTool;

namespace NetAsyncCore
{
    public class NetSocket<T,K> 
        where T: NetSession<K>,new() 
        where K : NetData
    {
        private Socket socket = null;
        
        public T clientSession = null;
        private List<T> clientSessionList = new List<T>();


        public NetSocket()
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        #region Server
        
        public void StartServer(string ip, int port)
        {
            StartServer(IPAddress.Parse(ip),port);
        }
        
        public void StartServer(IPAddress ip,int port)
        {
            StartServer(new IPEndPoint(ip, port));
        }

        public void StartServer(IPEndPoint ipEndPoint)
        {
            DetailLog.InitSettings();
            try
            {
                socket.Bind(ipEndPoint);
                socket.Listen(50000);
                socket.BeginAccept(ClientConnectCallBack, socket);
                DetailLog.ColorLog(LogColor.Blue,"Server Start Success!\nWaiting for Connecting......");
            }
            catch (Exception e)
            {
                DetailLog.Error("StartServer:" + e.Message);
            }
        }
        private void ClientConnectCallBack(IAsyncResult ar)
        {
            try
            {
                Socket clientSocket = socket.EndAccept(ar);
                T session = new T();
                clientSessionList.Add(session);
                session.ReceiveData(clientSocket, () =>
                {
                    if (clientSessionList.Contains(session))
                        clientSessionList.Remove(session);
                });
            }
            catch (Exception e)
            {
                DetailLog.Error("ConnectClient:" + e.Message);
            }
            socket.BeginAccept(ClientConnectCallBack, socket);
        }
        
        #endregion
        
        #region Client
        public void StartClient(string ip, int port)
        {
            StartClient(IPAddress.Parse(ip),port);
        }
        
        public void StartClient(IPAddress ip,int port)
        {
            StartClient(new IPEndPoint(ip, port));
        }
        
        public void StartClient(IPEndPoint ipEndPoint)
        {
            DetailLog.InitSettings();
            try
            {
                socket.BeginConnect(ipEndPoint, ServerConnectCallBack, socket);
                DetailLog.ColorLog(LogColor.Blue,"Client Start Success!\nConnecting To Server......");
            }
            catch (Exception e)
            {
                DetailLog.Error("StartClient:" + e.Message);
            }
        }

        private void ServerConnectCallBack(IAsyncResult ar)
        {
            try
            {
                socket.EndConnect(ar);
                clientSession = new T();
                clientSession.ReceiveData(socket, null);
            }
            catch (Exception e)
            {
                DetailLog.Error("ConnectServer:" + e.Message);
            }
        }
        #endregion

        public void Close(T netSession)
        {
            netSession.Clear();
        }

        public void Close()
        {
            clientSession.Clear();
        }
        
    }
}