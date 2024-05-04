using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Melange.core
{
    public class NodeConnection
    {
        public string ipAddress;
        public int port;
    }

    public class BitcoinNode
    {
        private const int port = 8888;
        private TcpListener listener;
        private List<NodeConnection> connectedNodes;

        public BitcoinNode()
        {
            connectedNodes = new List<NodeConnection>();
        }

        private void Initialize()
        {
            listener = new TcpListener(IPAddress.Any, port);
            // display the IP address of the node and port
            Console.WriteLine($"Node Public IP Address: {Dns.GetHostEntry(Dns.GetHostName()).AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork)}");
            Console.WriteLine($"Node Port: {port}");
            Console.WriteLine("Bitcoin Node initialized successfully...");

        }


        public void Start()
        {
            Console.WriteLine("Bitcoin Node started...");
            Initialize();
            listener.Start();
            Console.WriteLine("Type 'exit' to quit.");
            Console.WriteLine("Type a message to send to all connected nodes.");

            Thread receiveThread = new Thread(ReceiveMessages);
            receiveThread.Start();

            while (true)
            {
                string input = Console.ReadLine();

                if (input.ToLower() == "exit")
                    break;

                // if the input starts with "connect", connect to the specified node 
                // e.g. "connect ipaddress:port"
                if (input.ToLower().StartsWith("connect"))
                {
                    
                }
                // if the input starts with "send", send the message to all connected nodes
                // e.g. "send Hello, world!"
                else if (input.ToLower().StartsWith("send"))
                {
                    string[] parts = input.Split(' ');
                    if (parts.Length >= 2)
                    {
                        string message = string.Join(" ", parts.Skip(1));
                        SendMessageToAllNodes(message);
                        Console.WriteLine($"Sent message to all connected nodes: {message}");
                    }
                    else
                    {
                        Console.WriteLine("Invalid send command. Usage: send message");
                    }
                }
                else
                {
                    Console.WriteLine("Invalid command.");
                }
            }
        }


        private void ReceiveMessages()
        {
            while (true)
            {
                TcpClient client = listener.AcceptTcpClient();
                NetworkStream stream = client.GetStream();

                byte[] buffer = new byte[1024];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string receivedMessage = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                Console.WriteLine($"Received message: {receivedMessage}");

                stream.Close();
                client.Close();
            }
        }

        private void SendMessageToAllNodes(string message)
        {
            foreach (NodeConnection connectedNode in connectedNodes)
            {
                try
                {
                    TcpClient client = new TcpClient();
                    client.Connect(connectedNode.ipAddress, connectedNode.port);

                    NetworkStream stream = client.GetStream();
                    byte[] data = Encoding.ASCII.GetBytes(message);
                    stream.Write(data, 0, data.Length);

                    stream.Close();
                    client.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error sending message to node {connectedNode.ipAddress}:{connectedNode.port} {ex.Message}");
                }
            }
        }

        public void ConnectToNode(NodeConnection node)
        {
            connectedNodes.Add(node);
        }
    }
}
