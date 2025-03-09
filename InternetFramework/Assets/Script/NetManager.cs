using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using Utility.SingletonPatternSystem;

public class NetManager : MonoSingleton<NetManager>
{
    private Socket _socket;

    private Queue<BaseMessage> _msgQueue = new Queue<BaseMessage>();
    private Queue<BaseMessage> _receiveQueue = new Queue<BaseMessage>();
    private byte[] receiveBytes = new byte[1024 * 1024];
    private int _receiveNum;

    private bool _isConnected = false;
    // private Thread _sendThread;
    //
    // private Thread _receiveThread;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (_receiveQueue.Count > 0)
        {
            BaseMessage msg = _receiveQueue.Dequeue();
            if (msg is PlayerMessage)
            {
            }
        }
    }

    public void Connect(string ip, int port)
    {
        if (_socket != null) return;
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
        try
        {
            _socket.Connect(ipEndPoint);
            _isConnected = true;
            ThreadPool.QueueUserWorkItem(SendMsg);
            ThreadPool.QueueUserWorkItem(ReceiveMsg);
            // _sendThread = new Thread(SendMsg);
            // _sendThread.Start();
            // _receiveThread = new Thread(ReceiveMsg);
            // _receiveThread.Start();
        }
        catch (SocketException e)
        {
            if (e.ErrorCode == 10061)
            {
                Debug.Log("服务器拒绝连接");
            }
            else
            {
                Debug.Log("连接服务器失败,错误码：" + e.ErrorCode);
            }
        }
    }

    public void Send(BaseMessage msg)
    {
        _msgQueue.Enqueue(msg);
    }

    private void SendMsg(object msg)
    {
        while (!_isConnected)
        {
            if (_msgQueue.Count > 0)
            {
                _socket.Send(_msgQueue.Dequeue().Serialize2Bytes());
            }
        }
    }

    private void ReceiveMsg(object msg)
    {
        while (!_isConnected)
        {
            if (_socket.Available <= 0) continue;
            _receiveNum = _socket.Receive(receiveBytes);
            int msgId = BitConverter.ToInt32(receiveBytes, 0);
            BaseMessage msgObj = null;
            switch (msgId)
            {
                case 1001:
                    PlayerMessage playerMessage = new PlayerMessage();
                    playerMessage.DeserializeFromBytes(receiveBytes, 4);
                    msgObj = playerMessage;
                    break;
            }

            if (msgObj != null)
                _receiveQueue.Enqueue(msgObj);
        }
    }

    private void Close()
    {
        _socket.Shutdown(SocketShutdown.Both);
        _socket.Close();
        _isConnected = false;
        // _sendThread = null;
        // _receiveThread = null;
    }

    private void OnDestroy()
    {
        Close();
    }
}