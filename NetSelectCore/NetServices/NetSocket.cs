
// ****************************************************
//     文件：NetSocket.cs
//     作者：积极向上小木木
//     邮箱: positivemumu@126.com
//     日期：2021/07/24 14:13
//     功能：异步网络核心类，负责启动、连接Socket并创建会话。
// *****************************************************

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using DetailLogTool;

namespace NetSelectCore
{
    public class NetSocket<T,K> 
        where T: NetSession<K>,new() 
        where K : NetData
    {
        private Socket socket;
        public T clientSession;

        private List<Socket> checkReadList = new List<Socket>();
        private Dictionary<Socket, T> clientSocketDic = new Dictionary<Socket, T>();
        
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
                DetailLog.ColorLog(LogColor.Blue,"Server Start Success!\nWaiting for Connecting......");
                while (true)
                {
                    ResetCheckRead();
                    try
                    {
                        //检查当前Socket，返回可读Socket数组
                        Socket.Select(checkReadList, null, null, 5000);
                    }
                    catch (SocketException e)
                    {
                        DetailLog.Error("StartReceiveError:" + e.Message);
                    }
                    
                    for (int i = checkReadList.Count-1; i >= 0; i--)
                    {
                        Socket s = checkReadList[i];
                        if (s == socket)
                        {
                            //监听Socket可读，有新的客户端连接到服务器
                            Socket client = socket.Accept();
                            T clientSession = new T();
                            clientSession.InitNetSession(client, () =>
                            {
                                if (clientSocketDic.ContainsKey(client))
                                    clientSocketDic.Remove(client);
                            }, false);
                            clientSocketDic.Add(client,clientSession);
                        }
                        else
                        {
                            //连接客户端的Socket可读，客户端向服务端发送消息
                            clientSocketDic[s].ServerReceiveData();
                        }
                    }
                    
                }
            }
            catch (Exception e)
            {
                DetailLog.Error("StartServerError:" + e.Message);
            }
        }
        
        private void ResetCheckRead()
        {
            checkReadList.Clear();
            checkReadList.Add(socket);
            foreach (Socket s in clientSocketDic.Keys)
            {
                checkReadList.Add(s);                
            }
        }
        
        # endregion

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
                DetailLog.Error("StartClientError:" + e.Message);
            }
        }

        private void ServerConnectCallBack(IAsyncResult ar)
        {
            try
            {
                socket.EndConnect(ar);
                clientSession = new T();
                clientSession.InitNetSession(socket, null, true);
            }
            catch (Exception e)
            {
                DetailLog.Error("ConnectServerError:" + e.Message);
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