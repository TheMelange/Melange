using MelangeTestPeerToPeer.Commands;
using MelangeTestPeerToPeer.Protocols.Contracts;
using System;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace MelangeTestPeerToPeer.Protocols
{
    public static class CommunicationProtocol
    {
        public static void SendMessageToNode(NodeConnection targetNode, NodeConnection selfNode, string instructionCode, string? dataObj)
        {
            try
            {
                TcpClient client = new TcpClient();
                client.Connect(targetNode.NodeIPAddress, targetNode.NodePort);
                NetworkStream stream = client.GetStream();

                var communicationMessage = new CommunicationMessage(selfNode, instructionCode, dataObj);
                byte[] data = CommunicationMessage.Serialize(communicationMessage);
                stream.Write(data, 0, data.Length);

                stream.Close();
                client.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending message to node {targetNode.NodeIPAddress}:{targetNode.NodePort} {ex.Message}");
            }
        }

        public static void SendMessageToAllNodes(List<NodeConnection> connectedNodes, NodeConnection selfNode, string instructionCode, string? data)
        {
            if(connectedNodes.Count == 0)
            {
                Console.WriteLine("No nodes connected.");
                return;
            }

            foreach (NodeConnection connectedNode in connectedNodes)
            {
                SendMessageToNode(connectedNode, selfNode, instructionCode, data);
            }
            
            Console.WriteLine($"Sent message to all connected nodes: {data}");
        }

        public static void ReceiveMessages(TcpListener listener, ref List<NodeConnection> connectedNodes, NodeConnection selfNode)
        {
            while (true)
            {
                try
                {
                    TcpClient client = listener.AcceptTcpClient();
                    NetworkStream stream = client.GetStream();
                    
                    byte[] buffer = new byte[1024];
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    CommunicationMessage? receivedMessageObj = CommunicationMessage.Deserialize(buffer);

                    stream.Close();
                    client.Close();

                    // check if the node is already in the connected nodes list
                    if (receivedMessageObj != null && receivedMessageObj.Node != null && connectedNodes.FirstOrDefault(node => node.NodeIPAddress == receivedMessageObj.Node.NodeIPAddress && node.NodePort == receivedMessageObj.Node.NodePort) == null)
                    {
                        Console.WriteLine("Received message from new node: " + receivedMessageObj.Node.NodeIPAddress + ":" + receivedMessageObj.Node.NodePort + " Adding it to the connected nodes list");
                        ConnectCommand.Execute("connect " + receivedMessageObj.Node.NodeIPAddress + ":" + receivedMessageObj.Node.NodePort, connectedNodes, selfNode);
                    }

                    //todo: do something with the received message object
                    if(receivedMessageObj != null)
                    {
                        // check the message instruction code
                        if(receivedMessageObj.InstructionCode == "send")
                        {
                            Console.WriteLine($"Received message from {receivedMessageObj.Node.NodeIPAddress}:{receivedMessageObj.Node.NodePort} : {receivedMessageObj.Data}");
                        } else if( receivedMessageObj.InstructionCode == "ping")
                        {
                            CommunicationProtocol.SendMessageToNode(receivedMessageObj.Node, selfNode, "send", "pong");
                        } else if(receivedMessageObj.InstructionCode == "discovery_request")
                        {
                            // send the connected nodes to the node that requested it
                            string connectedNodesString = string.Join(",", connectedNodes.Select(n => n.NodeIPAddress + ":" + n.NodePort));
                            CommunicationProtocol.SendMessageToNode(receivedMessageObj.Node, selfNode, "discovery_response", connectedNodesString);

                        } else if(receivedMessageObj.InstructionCode == "discovery_response")
                        {
                            Console.WriteLine($"Received connected nodes from {receivedMessageObj.Node.NodeIPAddress}:{receivedMessageObj.Node.NodePort} : {receivedMessageObj.Data}");

                            string[] connectedNodesArray = receivedMessageObj.Data.Split(',');
                            foreach(string node in connectedNodesArray)
                            {
                               ConnectCommand.Execute("connect " + node, connectedNodes, selfNode);
                            }

                        }
                    }
                }
                catch
                {
                    Console.WriteLine("Error receiving message.");
                }
            }
        }

        public static void HandleCommandMessage(string command, List<NodeConnection> connectedNodes, NodeConnection selfNode)
        {
            if (command.StartsWith("connect"))
            {
                ConnectCommand.Execute(command, connectedNodes, selfNode);
            }
            else if (command.StartsWith("send"))
            {
                SendCommand.Execute(command, connectedNodes,selfNode);
            }
            else if(command.StartsWith("discovery"))
            {
                DiscoveryCommand.Execute(command, connectedNodes, selfNode);
            }
            else if (command.StartsWith("exit"))
            {
                Console.WriteLine("Exiting...");
            }
            else if(command.StartsWith("ping"))
            {
                PingCommand.Execute(command, connectedNodes, selfNode);
            }
            else
            {
                Console.WriteLine("Invalid command.");
            }
        }
    }
}
