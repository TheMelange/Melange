using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MelangeTestPeerToPeer.Protocols.Contracts
{
    public class NodeConnection
    {
        [JsonProperty("NodeIPAddress")]
        public string NodeIPAddress;
        [JsonProperty("NodePort")]
        public int NodePort;

        public NodeConnection(string IPAddress, int Port)
        {
            this.NodeIPAddress = IPAddress;
            this.NodePort = Port;
        }

    }
}
