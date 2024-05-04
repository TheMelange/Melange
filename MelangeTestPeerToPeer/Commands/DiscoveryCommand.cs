using MelangeTestPeerToPeer.Protocols;
using MelangeTestPeerToPeer.Protocols.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MelangeTestPeerToPeer.Commands
{
    public static class DiscoveryCommand
    {

        public static void Execute(string input, List<NodeConnection> nodes, NodeConnection selfNode)
        {
            // check if the input contains the ip:port after the discovery
            // e.g. discovery ip:port

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
                    
                    // send a message to the node to ask for its connected nodes
                    CommunicationProtocol.SendMessageToNode(node, selfNode, "discovery_request", null);
                }
                else
                {
                    Console.WriteLine("Invalid discovery command. Usage: discovery ipaddress:port");
                }
            }
            else
            {
                Console.WriteLine("Invalid discovery command. Usage: discovery ipaddress:port");
            }
        }

    }
}
