using Melange.Protocols.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Melange.Core
{
    public class BlockchainWrapper
    {
        public List<NodeConnection> ConnectedNodes { get; set; }
        public NodeConnection SelfNode { get; set; }

        public BlockchainWrapper(List<NodeConnection> connectedNodes, NodeConnection selfNode)
        {
            ConnectedNodes = connectedNodes;
            SelfNode = selfNode;
        }
    }

}
