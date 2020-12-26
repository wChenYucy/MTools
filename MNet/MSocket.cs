// ****************************************************
// 	文件：MSocket.cs
// 	作者：积极向上小木木
// 	邮箱：positivemumu@126.com
// 	日期：2020/12/26 14:13:34
// 	功能：Socket核心类
// *****************************************************
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace MNet
{
    public class MSocket<T, K>
        where T : MSession<K>, new()
        where K : MData
    {
        public T session = null;

        private Socket _socket = null;
        private List<T> _sessionList = new List<T>();
        private int _backlog = 10;

        public MSocket()
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        #region Server

        public void ServerStart(string ip, int port)
        {
            try
            {
                _socket.Bind(new IPEndPoint(IPAddress.Parse(ip), port));
                _socket.Listen(_backlog);
                _socket.BeginAccept(new AsyncCallback(ClientConnectCallBack), _socket);
                MTools.PrintLog("\nServer Start Success!\nWaiting for Connecting......", MLogLevels.Information);
            }
            catch (Exception e)
            {
                MTools.PrintLog(e.Message, MLogLevels.Error);
            }
        }

        private void ClientConnectCallBack(IAsyncResult ar)
        {
            try
            {
                Socket clientSocket = _socket.EndAccept(ar);
                T session = new T();
                _sessionList.Add(session);
                session.ReceiveData(clientSocket, () =>
                {
                    if (_sessionList.Contains(session))
                        _sessionList.Remove(session);
                });
            }
            catch (Exception e)
            {
                MTools.PrintLog(e.Message, MLogLevels.Error);
            }
            _socket.BeginAccept(new AsyncCallback(ClientConnectCallBack), _socket);
        }
        #endregion

        #region Client
        public void ClientStart(string ip, int port)
        {
            try
            {
                _socket.BeginConnect(new IPEndPoint(IPAddress.Parse(ip), port), new AsyncCallback(ServerConnectCB), _socket);
                MTools.PrintLog("\nClient Start Success!\nConnecting To Server......", MLogLevels.Information);
            }
            catch (Exception e)
            {
                MTools.PrintLog(e.Message, MLogLevels.Error);
            }
        }

        void ServerConnectCB(IAsyncResult ar)
        {
            try
            {
                _socket.EndConnect(ar);
                session = new T();
                session.ReceiveData(_socket, null);
            }
            catch (Exception e)
            {
                MTools.PrintLog(e.Message, MLogLevels.Error);
            }
        }
        #endregion

        public void Close()
        {
            if (_socket != null)
            {
                _socket.Close();
            }
        }

        public List<T> GetSesstionList()
        {
            return _sessionList;
        }

        public void SetLogState(bool showLog = true)
        {
            if (!showLog)
            {
                MTools.showLog = false;
            }
        }

        public void SetPrintLogMethod(Action<string, int> logcallback = null)
        {
            if (logcallback != null)
            {
                MTools.logCallBack = logcallback;
            }
        }
    }
}