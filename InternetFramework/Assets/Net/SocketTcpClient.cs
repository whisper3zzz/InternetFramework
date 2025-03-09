using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

namespace Net
{
    public class SocketTcpClient : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8080);
            try
            {
                socket.Connect(ipEndPoint);
            }
            catch (SocketException e)
            {
                if (e.ErrorCode == 10061)
                {
                    print("服务器拒绝连接");
                }
                else
                {
                    print("连接服务器失败,错误码：" + e.ErrorCode);
                }
            }

            byte[] bytes = new byte[1024];
            int receiveNum = socket.Receive(bytes);
            print("接收到服务器消息：" + Encoding.UTF8.GetString(bytes, 0, receiveNum));
            socket.Send(Encoding.UTF8.GetBytes("Hello, I'm client"));
            
            
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
        }

        // Update is called once per frame
        void Update()
        {
        }
    }
}