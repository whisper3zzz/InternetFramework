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
    // private byte[] receiveBytes = new byte[1024 * 1024];
    // private int _receiveNum;

    [Header("处理分包时缓存的字节数组和长度")] private byte[] _cacheBytes = new byte[1024 * 1024];
    private int _cacheLength = 0;

    private bool _isConnected = false;
    // private Thread _sendThread;
    //
    // private Thread _receiveThread;


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
            byte[] receiveBytes = new byte[1024 * 1024];
            int receiveNum = _socket.Receive(receiveBytes);
            HandleReceiveMsg(receiveBytes, receiveNum);


            //     int msgId = BitConverter.ToInt32(receiveBytes, 0);
            //     BaseMessage msgObj = null;
            //     switch (msgId)
            //     {
            //         case 1001:
            //             PlayerMessage playerMessage = new PlayerMessage();
            //             playerMessage.DeserializeFromBytes(receiveBytes, 4);
            //             msgObj = playerMessage;
            //             break;
            //     }
            //
            //     if (msgObj != null)
            //         _receiveQueue.Enqueue(msgObj);
            // }
        }
    }

    private void HandleReceiveMsg(byte[] bytes, int receiveNum)
    {
        int msgId = 0;
        int msgLength = 0;
        int currentIndex = 0;
        bytes.CopyTo(_cacheBytes, _cacheLength);
        _cacheLength += receiveNum;
        while (true)
        {
            msgLength = -1;
            if (_cacheLength >= 8)
            {
                msgId = BitConverter.ToInt32(_cacheBytes, currentIndex);
                currentIndex += 4;
                msgLength = BitConverter.ToInt32(_cacheBytes, currentIndex);
                currentIndex += 4;
            }

            if (_cacheLength - 8 >= msgLength && msgLength != -1)
            {
                BaseMessage msgObj = null;
                switch (msgId)
                {
                    case 1001:
                        PlayerMessage playerMessage = new PlayerMessage();
                        playerMessage.DeserializeFromBytes(_cacheBytes, currentIndex);
                        msgObj = playerMessage;
                        break;
                }

                if (msgObj != null)
                    _receiveQueue.Enqueue(msgObj);
                currentIndex += msgLength;
                if (currentIndex == _cacheLength)
                {
                    _cacheLength = 0;
                    break;
                }
            }
            else
            {
                //解析了ID和长度，但是数据不够，没有解析完整Msg
                if (msgLength != -1)
                {
                    currentIndex -= 8;
                }

                Array.Copy(_cacheBytes, currentIndex, _cacheBytes, 0, _cacheLength - currentIndex);
                _cacheLength -= currentIndex;
                break;
            }
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