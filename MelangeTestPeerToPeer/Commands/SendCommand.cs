using MelangeTestPeerToPeer.Protocols;
using MelangeTestPeerToPeer.Protocols.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MelangeTestPeerToPeer.Commands
{
    public static class SendCommand
    {
        public static void Execute(string input, List<NodeConnection> nodes, NodeConnection selfNode)
        {
            string[] parts = input.Split(' ');
            if (parts.Length >= 2)
            {
                string message = string.Join(" ", parts.Skip(1));
                CommunicationProtocol.SendMessageToAllNodes(nodes, selfNode, "send", message);
            }
            else
            {
                Console.WriteLine("Invalid send command. Usage: send message");
            }
        }
    }
}
