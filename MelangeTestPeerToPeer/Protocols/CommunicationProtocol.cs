using MelangeTestPeerToPeer.Commands;
using MelangeTestPeerToPeer.Core;
using MelangeTestPeerToPeer.Protocols.Contracts;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
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

        public static string ConvertDataObjectToString(object dataObj)
        {
            try
            {
                var response = JsonConvert.SerializeObject(dataObj);
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error serializing object: {ex.Message}");
                return null;
            }
        }

        public static T ConvertDataStringToObject<T>(string dataString) where T : class
        {
            if (string.IsNullOrEmpty(dataString))
            {
                throw new ArgumentException("Input data string is null or empty.", nameof(dataString));
            }

            try
            {
                //Console.WriteLine("Current Data string: " + dataString);
                T result = JsonConvert.DeserializeObject<T>(dataString);
                if (result == null)
                    throw new InvalidOperationException("Deserialization returned null.");

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deserializing string to object: {ex.Message}");
                throw;  // Re-throw to handle higher up in the call stack
            }
        }



        public static void ReceiveMessages(TcpListener listener, ref List<NodeConnection> connectedNodes, ref NodeConnection selfNode)
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

                    if(receivedMessageObj != null)
                    {
                        if(receivedMessageObj.InstructionCode == "send")
                        {
                            Console.WriteLine($"Received message from {receivedMessageObj.Node.NodeIPAddress}:{receivedMessageObj.Node.NodePort} : {receivedMessageObj.Data}");
                        } else if( receivedMessageObj.InstructionCode == "ping")
                        {
                            CommunicationProtocol.SendMessageToNode(receivedMessageObj.Node, selfNode, "send", "pong");
                        } else if(receivedMessageObj.InstructionCode == "discovery_request")
                        {
                            string connectedNodesString = string.Join(",", connectedNodes.Select(n => n.NodeIPAddress + ":" + n.NodePort));
                            CommunicationProtocol.SendMessageToNode(receivedMessageObj.Node, selfNode, "discovery_response", connectedNodesString);

                        } else if(receivedMessageObj.InstructionCode == "discovery_response")
                        {
                            string[] connectedNodesArray = receivedMessageObj.Data.Split(',');
                            foreach(string node in connectedNodesArray)
                            {
                               ConnectCommand.Execute("connect " + node, connectedNodes, selfNode);
                            }
                        } else if(receivedMessageObj.InstructionCode == "sync_request")
                        {
                            if(selfNode.Blockchain == null)
                            {
                                CommunicationProtocol.SendMessageToNode(receivedMessageObj.Node, selfNode, "sync_response", "no_genesis_block");
                            }
                            else
                            {
                                if(receivedMessageObj.Data == "genesis_block")
                                {
                                    var genesisBlock = selfNode.Blockchain.GetGenesisBlock();

                                    if(genesisBlock == null)
                                    {
                                        Console.WriteLine("Genesis block not found to send to " + receivedMessageObj.Node.NodeIPAddress + ":" + receivedMessageObj.Node.NodePort);
                                    }
                                    else
                                    {
                                        Console.WriteLine("Genesis block sent to " + receivedMessageObj.Node.NodeIPAddress + ":" + receivedMessageObj.Node.NodePort);

                                        CommunicationProtocol.SendMessageToNode(receivedMessageObj.Node, selfNode, "sync_response", CommunicationProtocol.ConvertDataObjectToString(genesisBlock));
                                    }
                                }
                                else
                                {
                                    //! check based on the block index
                                    if(int.TryParse(receivedMessageObj.Data, out int blockIndex))
                                    {
                                        // convert block to Json string with NewtsoneSoft.Json
                                        try
                                        {
                                            Console.WriteLine("Block requested: " + blockIndex + " from " + receivedMessageObj.Node.NodeIPAddress + ":" + receivedMessageObj.Node.NodePort);

                                            var block = selfNode.Blockchain.GetBlockByIndex(blockIndex);


                                            if(block == null)
                                            {
                                                Console.WriteLine($"Block {blockIndex} not found in the blockchain.");
                                                CommunicationProtocol.SendMessageToNode(receivedMessageObj.Node, selfNode, "sync_response", "no_block_found");
                                            }
                                            else
                                            {
                                                CommunicationProtocol.SendMessageToNode(receivedMessageObj.Node, selfNode, "sync_response", CommunicationProtocol.ConvertDataObjectToString(block));
                                            }
                                        }
                                        catch(Exception ex)
                                        {
                                            Console.WriteLine("Error sending block response to node.");
                                            Console.WriteLine(ex.Message);
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("Invalid block index received from node.");
                                    }
                                }

                            }

                        } else if(receivedMessageObj.InstructionCode == "sync_response")
                        {
                            if(receivedMessageObj.Data == "no_genesis_block")
                            {
                                Console.WriteLine($"Node {receivedMessageObj.Node.NodeIPAddress}:{receivedMessageObj.Node.NodePort} does not have a genesis block.");
                                MelangeSyncCommand.ForceDefineSyncing(false);
                                Console.WriteLine("Sync completed.");
                            }
                            else if (receivedMessageObj.Data == "no_block_found")
                            {
                                Console.WriteLine($"Current Node don't have the block requested to send to " + receivedMessageObj.Node.NodeIPAddress + ":" + receivedMessageObj.Node.NodePort);
                                MelangeSyncCommand.ForceDefineSyncing(false);
                                Console.WriteLine("Sync completed at block: " + (selfNode.Blockchain != null ? selfNode.Blockchain?.Chain.Count() : "0"));
                            }
                            else
                            {
                                try
                                {
                                    Block? block = CommunicationProtocol.ConvertDataStringToObject<Block>(receivedMessageObj.Data);
                                    if (block == null)
                                    {
                                        Console.WriteLine("Deserialization failed to produce a block object.");
                                    }
                                    else
                                    {
                                        Console.WriteLine($"Block received from {receivedMessageObj.Node.NodeIPAddress}:{receivedMessageObj.Node.NodePort} : {block.Index}");
                                        Console.WriteLine("Adding block to blockchain...");

                                        if(selfNode.Blockchain == null)
                                        {
                                            selfNode.Blockchain = new Blockchain(0, 50);
                                        }

                                        selfNode.Blockchain.AddBlockToChain(block);
                                        CommunicationProtocol.SendMessageToNode(receivedMessageObj.Node, selfNode, "sync_request", (block.Index + 1).ToString());
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine("Error parsing block received from node.");
                                    Console.WriteLine(ex.Message);
                                }

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
        public static void HandleCommandMessage(string command, ref List<NodeConnection> connectedNodes, ref NodeConnection selfNode)
        {
            command = command.ToLower().Trim();
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
            else if (command.StartsWith("mining:start"))
            {
                BlockchainWrapper wrapper = new BlockchainWrapper(connectedNodes, selfNode);
                Thread receiveThread = new Thread(() => InitializeBlockchainCommand.Execute(command, wrapper));
                receiveThread.Start();
            }
            else if (command.StartsWith("status:currency")){
                var totalCoins = selfNode.Blockchain.GetTotalCoinsInBlockchain();
                Console.WriteLine("Total coins in blockchain: " + totalCoins);
            }
            else if (command.StartsWith("status:block"))
            {
                var totalBlocks = selfNode.Blockchain.Chain.Count -1;
                Console.WriteLine("Total blocks in blockchain: " + totalBlocks);
            }
            else if (command.StartsWith("status:difficulty"))
            {
                var difficulty = selfNode.Blockchain.GetDifficulty();
                Console.WriteLine("Current difficulty: " + difficulty);
            }
            else if (command.StartsWith("mining:stop"))
            {
                Blockchain.Mining = false;
                Console.WriteLine("Mining stopped.");
            }
            else if (command.StartsWith("wallet:create"))
            {
                CreateWalletCommand.Execute(command);
            }
            else if (command.StartsWith("sync"))
            {
                MelangeSyncCommand.Execute(command, connectedNodes, selfNode);
            }
            else
            {
                Console.WriteLine("Invalid command.");
            }
        }
    }
}
