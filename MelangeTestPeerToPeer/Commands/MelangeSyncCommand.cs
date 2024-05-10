using MelangeTestPeerToPeer.Protocols;
using MelangeTestPeerToPeer.Protocols.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MelangeTestPeerToPeer.Commands
{
    public static class MelangeSyncCommand
    {
        private static bool IsSyncing { get; set; } = false;

        public static void Execute(string input, List<NodeConnection> nodes, NodeConnection selfNode)
        {
            if (MelangeSyncCommand.IsMelangeSyncing())
            {
                Console.WriteLine("Already syncing.");
                return;
            }

            if(nodes.Count == 0)
            {
                Console.WriteLine("No nodes connected.");
                return;
            }

            // ask one of the connected nodes to sync with us (the node that responds will be the one we sync with)
            MelangeSyncCommand.ForceDefineSyncing(true);
            Console.WriteLine("Syncing...");

            // check if input has a specific block to sync with (e.g. "sync 1")
            if (input.Contains(" "))
            {
                string[] inputArray = input.Split(" ");
                string blockIndex = inputArray[1];
                CommunicationProtocol.SendMessageToNode(nodes.First(), selfNode, "sync_request", blockIndex);
            }
            else
            {
                // send sync request to the first node
                CommunicationProtocol.SendMessageToNode(nodes.First(), selfNode, "sync_request", "genesis_block");
            }
        }

        public static void ForceDefineSyncing(bool isSyncing)
        {
            IsSyncing = isSyncing;
        }

        public static bool IsMelangeSyncing()
        {
            return MelangeSyncCommand.IsSyncing;
        }
    }
}
