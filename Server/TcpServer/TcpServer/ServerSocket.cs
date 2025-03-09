using System.Net;
using System.Net.Sockets;

namespace TcpServer;

public class ServerSocket
{
    public Socket Socket;
    public Dictionary<int, ClientSocket> ClientSockets = new Dictionary<int, ClientSocket>();
    private bool isClosed = false;

    public void Start(string ip, int port, int num)
    {
        Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse(ip), port);
        Socket.Bind(ipPoint);
        Socket.Listen(num);
        ThreadPool.QueueUserWorkItem(Accept);
        ThreadPool.QueueUserWorkItem(Receive);
    }

    public void Close()
    {
        foreach (var client in ClientSockets.Values)
        {
            client.Close();
        }

        ClientSockets.Clear();
        Socket.Shutdown(SocketShutdown.Both);
        Socket.Close();
    }

    private void Accept(object state)
    {
        while (!isClosed)
        {
            try
            {
                Socket clientSocket = Socket.Accept();
                ClientSocket client = new ClientSocket(clientSocket);
                ClientSockets.Add(client.ClientId, client);
            }
            catch (SocketException e)
            {
                Console.WriteLine("Accept error: " + e.Message);
            }
        }
    }

    private void Receive(object state)
    {
        while (!isClosed)
        {
            if (ClientSockets.Count <= 0) continue;
            foreach (var client in ClientSockets.Values)
            {
                client.Receive();
            }
        }
    }

    public void Broadcast(string msg)
    {
        foreach (var client in ClientSockets.Values)
        {
            client.Send(msg);
        }
    }
}