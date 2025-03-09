// See https://aka.ms/new-console-template for more information

using System.Net;
using System.Net.Sockets;
using System.Text;
using TcpServer;

#region Old

Socket socketTcp = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

List<Socket> clientSockets = new List<Socket>();
bool isClosed = false;
try
{
    IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8080);
    socketTcp.Bind(ipEndPoint);
}
catch (Exception e)
{
    Console.WriteLine(e.Message);
    throw;
}

socketTcp.Listen(10);

Thread acceptThread = new Thread(new ThreadStart(() =>
{
    while (true)
    {
        Socket socketClient = socketTcp.Accept();
        Console.WriteLine("客户端连接成功");
        clientSockets.Add(socketClient);
        socketClient.Send(Encoding.UTF8.GetBytes("Hello, I'm server"));
    }
}));
acceptThread.Start();
Thread receiveThread = new Thread(new ThreadStart(() =>
{
    byte[] result = new byte[1024 * 1024];
    while (true)
    {
        int i;
        for (i = 0; i < clientSockets.Count; i++)
        {
            var clientSocket = clientSockets[i];
            if (clientSocket.Available <= 0) continue;
            var receiveNum = clientSocket.Receive(result);
            ThreadPool.QueueUserWorkItem(HandleMessage,
                (clientSocket, Encoding.UTF8.GetString(result, 0, receiveNum)));
        }
    }
}));
receiveThread.Start();
while (true)
{
    string? input = Console.ReadLine();
    if (input == "exit")
    {
        foreach (Socket clientSocket in clientSockets)
        {
            clientSocket.Shutdown(SocketShutdown.Both);
            clientSocket.Close();
        }

        clientSockets.Clear();

        break;
    }
}
// Console.WriteLine("服务端监听结束，等待客户端连接");
// Socket socketClient = socketTcp.Accept();
// Console.WriteLine("客户端连接成功");
// socketClient.Send(Encoding.UTF8.GetBytes("Hello, I'm server"));
// byte[] result = new byte[1024];
// int receiveNum = socketClient.Receive(result);
// Console.WriteLine("接收到客户端{0}消息：{1}", socketClient.RemoteEndPoint?.ToString(),
//     Encoding.UTF8.GetString(result, 0, receiveNum));
// socketClient.Shutdown(SocketShutdown.Both);
// socketClient.Close();

Console.WriteLine("按任意键退出");
Console.ReadKey();

void HandleMessage(object? state)
{
    (Socket socketTcp, string str) info =
        ((Socket socketTcp, string str))(state ?? throw new ArgumentNullException(nameof(state)));
    Console.WriteLine(info.socketTcp.RemoteEndPoint?.ToString() + "：" + info.str);
}

#endregion

ServerSocket socket = new ServerSocket();
socket.Start("127.1.1.0", 8080, 10);