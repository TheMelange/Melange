using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MelangeTestPeerToPeer.Protocols.Contracts
{
    public class CommunicationMessage
    {
        [JsonProperty("Node")]
        public NodeConnection Node { get; set; }
        [JsonProperty("InstructionCode")]
        public string InstructionCode { get; set; }
        [JsonProperty("Data")]
        public string? Data { get; set; }


        public CommunicationMessage()
        {
        }

        public CommunicationMessage(NodeConnection node, string instructionCode, string? data)
        {
            Node = new NodeConnection(node.NodeIPAddress, node.NodePort);
            InstructionCode = instructionCode;
            Data = data;
        }

        public CommunicationMessage(NodeConnection node, string instructionCode)
        {
            Node = node;
            InstructionCode = instructionCode;
        }

        public static byte[] Serialize(CommunicationMessage message)
        {
            string json = JsonConvert.SerializeObject(message);
            return Encoding.ASCII.GetBytes(json);
        }

        public static CommunicationMessage? Deserialize(byte[] data)
        {
            try
            {
                string json = Encoding.ASCII.GetString(data);
                return JsonConvert.DeserializeObject<CommunicationMessage>(json);
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error deserializing message.");
                Console.WriteLine(ex.Message);
                return null;
            }
        }
    }
}
