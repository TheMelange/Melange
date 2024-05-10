using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Melange.Commands;
using Melange.Protocols.Contracts;
using Melange.Protocols;

namespace Melange
{
    public class MelangeNode
    {
        private int port;
        private TcpListener listener;
        private List<NodeConnection> connectedNodes;
        private NodeConnection selfNode;

        public MelangeNode(int port)
        {
            this.port = port;
            connectedNodes = new List<NodeConnection>();
        }

        private void Initialize()
        {
            listener = new TcpListener(IPAddress.Any, port);
            // display the IP address of the node and port
            this.selfNode = new NodeConnection(Dns.GetHostEntry(Dns.GetHostName()).AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork).ToString(), port);

            Console.WriteLine($"Node Public IP Address: {selfNode.NodeIPAddress}:{selfNode.NodePort}");
            Console.WriteLine($"Node Port: {port}");
            Console.WriteLine("Melange Node initialized successfully...");
        }


        public void Start()
        {
            Console.WriteLine("Melange Node started...");
            Initialize();
            listener.Start();
            Console.WriteLine("Type 'exit' to quit.");
            Console.WriteLine("Type a message to send to all connected nodes.");

            Thread receiveThread = new Thread(() => CommunicationProtocol.ReceiveMessages(listener, ref connectedNodes, ref selfNode));
            receiveThread.Start();

            while (true)
            {
                try
                {
                    string input = Console.ReadLine();
                    CommunicationProtocol.HandleCommandMessage(input, ref connectedNodes, ref selfNode);
                }
                catch
                {

                }

            }
        }
    }
}
