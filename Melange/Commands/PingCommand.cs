using Melange.Protocols;
using Melange.Protocols.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Melange.Commands
{
    public static class PingCommand
    {
        public static void Execute(string command, List<NodeConnection> nodes, NodeConnection selfNode)
        {
            if(nodes == null || nodes.Count == 0)
            {
                Console.WriteLine("No nodes connected.");
                return;
            }

            // check if the command contains an IP address and port after the ping command
            string[] parts = command.Split(' ');

            // check if the command contains an IP address and port after the ping command
            if (parts.Length == 2)
            {
                string[] addressParts = parts[1].Split(':');
                if (addressParts.Length == 2)
                {
                    string ipAddress = addressParts[0];
                    int port = int.Parse(addressParts[1]);
                    NodeConnection node = new NodeConnection(ipAddress, port);
                    // send a ping message to the specified node

                    CommunicationProtocol.SendMessageToNode(node, selfNode, "ping", null);
                }
                else
                {
                    Console.WriteLine("Invalid ping command. Usage: ping ipaddress:port");
                }
            }
            else
            {
                // send a ping message to all connected nodes
                CommunicationProtocol.SendMessageToAllNodes(nodes, selfNode, "ping", null);
            }
        }
    }
}
