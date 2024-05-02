using Melange.core.entities.validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Melange.core.entities
{
    public class Node
    {
        public Blockchain Blockchain { get; private set; }

        public Node(Blockchain blockchain)
        {
            Blockchain = blockchain;
        }

        public void MinePendingTransactions(string minerAddress)
        {
            Blockchain.ValidatePendingTransactions(minerAddress);
        }
        public void MineNewBlock(string minerAddress)
        {
            Blockchain.MineNewBlock(minerAddress);
        }

        public void DisplayChain()
        {
            foreach (var block in Blockchain.Chain)
            {
                Console.WriteLine($"Index: {block.Index}");
                Console.WriteLine($"Previous Hash: {block.PreviousHash}");
                Console.WriteLine($"Hash: {block.Hash}");
                Console.WriteLine("Transactions:");
                if(block.Transactions.Count > 0)
                {
                    foreach (var transaction in block.Transactions)
                    {
                        Console.WriteLine($"    > FROM:{transaction.FromAddress} | TO:{transaction.ToAddress} | A:{transaction.Amount} | FEE:{transaction.Fee}");
                    }
                }
                Console.WriteLine("Validators:");
                if(block.Validators.Count > 0)
                {
                    foreach (var validator in block.Validators)
                    {
                        Console.WriteLine($"    > {validator}");
                    }
                }
                Console.WriteLine($"Timestamp: {block.Timestamp}");
                Console.WriteLine();
            }
        }
    }
}
