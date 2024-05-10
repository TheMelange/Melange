using Melange.Protocols.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Melange.Commands
{
    public static class ConnectCommand
    {
        public static void Execute(string input, List<NodeConnection> nodes, NodeConnection selfNode)
        {
            string[] parts = input.Split(' ');
            if (parts.Length == 2)
            {
                // split the ip address and port
                string[] addressParts = parts[1].Split(':');
                if (addressParts.Length == 2)
                {
                    string ipAddress = addressParts[0];
                    int port = int.Parse(addressParts[1]);
                    NodeConnection node = new NodeConnection(ipAddress, port);
                    // add the node to the list of connected nodes

                    // check if the node is already in the connected nodes list
                    if (nodes.Any(n => n.NodeIPAddress == node.NodeIPAddress && n.NodePort == node.NodePort))
                    {
                        Console.WriteLine($"Node {ipAddress}:{port} is already connected, not adding again.");
                        return;
                    }

                    if (node.NodeIPAddress == selfNode.NodeIPAddress && node.NodePort == selfNode.NodePort)
                    {
                        Console.WriteLine("Cannot connect to self.");
                        return;
                    }

                    nodes.Add(node);
                    Console.WriteLine($"Connected to node {ipAddress}:{port}");
                }
                else
                {
                    Console.WriteLine("Invalid connect command. Usage: connect ipaddress:port");
                }
            }
            else
            {
                Console.WriteLine("Invalid connect command. Usage: connect ipaddress:port");
            }
        }




    }
}
