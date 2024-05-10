using Melange.Core;
using Melange.Protocols.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Melange.Commands
{
    public static class InitializeBlockchainCommand
    {

        public static void Execute(string input, BlockchainWrapper wrapper)
        {
            if (wrapper.SelfNode.Blockchain == null)
            {
                Console.WriteLine("Blockchain not initialized.");
                // check if blockchain is already initialized 
                if (wrapper.SelfNode.Blockchain == null)
                {
                    wrapper.SelfNode.Blockchain = new Blockchain(0, 50);
                }

                wrapper.SelfNode.Blockchain.ConnectedNodes = wrapper.ConnectedNodes;
                wrapper.SelfNode.Blockchain.InitializeChain("mineraddress");
            }
            else
            {

                if (Blockchain.Mining)
                {
                    Console.WriteLine("Mining already started.");
                }
                else
                {
                    wrapper.SelfNode.Blockchain.InitializeChain("mineraddress");

                }
            }
        }
    }
}
