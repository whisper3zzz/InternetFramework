using System.Net.Sockets;
using System.Text;

namespace TcpServer;

public class ClientSocket
{
    private static int CLIENT_BEGIN_ID = 1;
    public Socket Socket;
    public int ClientId;
    public bool Connected => this.Socket.Connected;

    public ClientSocket(Socket socket)
    {
        this.ClientId = CLIENT_BEGIN_ID;
        this.Socket = socket;
        ++CLIENT_BEGIN_ID;
    }

    public void Close()
    {
        if (Socket == null) return;
        Socket.Shutdown(SocketShutdown.Both);
        Socket.Close();
    }

    public void Send(string msg)
    {
        if (Socket == null) return;
        try
        {
            Socket.Send(Encoding.UTF8.GetBytes(msg));
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }

    public void Receive()
    {
        if (Socket == null) return;
        byte[] result = new byte[1024 * 1024];
        try
        {
            if (Socket.Available <= 0) return;
            var receiveNum = Socket.Receive(result);
            ThreadPool.QueueUserWorkItem(MsgHandle, Encoding.UTF8.GetString(result, 0, receiveNum));
            Console.WriteLine("接收到客户端消息：" + Encoding.UTF8.GetString(result, 0, receiveNum));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Close();
        }
    }

    private void MsgHandle(object obj)
    {
        string? msg = obj as string;
        Console.WriteLine("处理消息：" + msg);
    }
}