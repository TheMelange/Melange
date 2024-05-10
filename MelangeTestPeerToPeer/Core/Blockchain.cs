using MelangeTestPeerToPeer.Protocols.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MelangeTestPeerToPeer.Core
{
    public class Blockchain
    {
        public static bool Mining { get; set; }
        public const int MAX_COINS = 1000000;
        public const double TARGET_BLOCK_TIME = 600; // 10 minutes

        public List<Block> Chain { get; private set; }
        private int Difficulty { get; set; } = 4;
        private double MiningReward { get; set; } = 50;
        private List<Transaction> PendingTransactions { get; set; }
        public List<NodeConnection> ConnectedNodes { get; set; }

        public int GetDifficulty()
        {
            return Difficulty;
        }
        public Blockchain(int difficulty, double miningReward)
        {
            Difficulty = difficulty;
            MiningReward = miningReward;
            Chain = new List<Block>();
            PendingTransactions = new List<Transaction>();
            ConnectedNodes = new List<NodeConnection>();
        }
        public Block GetLatestBlock()
        {
            if(Chain.Count == 0)
            {
                return null;
            }
            return Chain[^1];
        }
        public void InitializeChain(string miningAddress)
        {
            // check for node connections
            /*if (ConnectedNodes.Count() == 0)
            {
                Console.WriteLine("No connected nodes found.");
                Console.WriteLine("Please connect to a node to start mining.");
                return;
            }*/
            Blockchain.Mining = true;
            MineNewBlock(miningAddress);
        }
        private void MineNewBlock(string miningAddress)
        {
            Console.WriteLine("Starting to mine blocks ... ");
            Console.WriteLine("Mining Status: " + Mining);
            Console.WriteLine("Current Mining Difficulty: " + Difficulty);

            while(Blockchain.Mining == true)
            {
                //Console.WriteLine("Constructing Block");
                Block block = new Block(Chain.Count, DateTime.Now, GetLatestBlock()?.Hash ?? null, new List<Transaction>());
                //Console.WriteLine("Mining block: " + block.Index);
                block.MineBlock(Difficulty);
                double totalCoinsInBlockchain = GetTotalCoinsInBlockchain();
                //Console.WriteLine("Total coins in blockchain: " + totalCoinsInBlockchain);
                if (totalCoinsInBlockchain <= MAX_COINS)
                {
                    Transaction rewardTransaction = new Transaction(block.Index, null, miningAddress, MiningReward, null, 0);
                    block.Transactions.Add(rewardTransaction);
                    //PendingTransactions.Add(rewardTransaction);
                    //Console.WriteLine("Mining reward added to pending transactions.");
                }
                AddBlockToChain(block);
            }
               
        }

        public void AddBlockToChain(Block block)
        {
            //if (block.IsValid(this.Difficulty))

            // check if the block index already exists
            if (Chain.Count > block.Index)
            {
                Chain[block.Index] = block;
            }
            else {
                Chain.Add(block);
            }



            // adjust the difficulty
            if (Chain.Count % 2016 == 0)
                {
                    Console.WriteLine("Adjusting difficulty...");
                    AdjustDifficulty();
                }

                // adjust the mining reward
                if (Chain.Count % 210000 == 0)
                {
                    Console.WriteLine("Adjusting mining reward...");
                    AdjustHalving();
                }

                //Console.WriteLine(" > Block added to chain.");
            //}
            //else
            //{
              //  Console.WriteLine(" > Block is not valid. Discarding...");
            //}

           
        }



        public Block GetGenesisBlock()
        {
            if(Chain.Count == 0)
            {
                return null;
            }
            return Chain[0];
        }
        public Block? GetBlockByIndex(int index)
        {
            if(index < 0 || index >= Chain.Count)
            {
                return null;
            }
            return Chain[index];
        }
        private double CalculateAverageMiningTime()
        {
            if (Chain.Count <= 2016)
            {
                return TARGET_BLOCK_TIME;
            }

            TimeSpan totalMiningTime = TimeSpan.Zero;
            for (int i = Chain.Count - 1; i >= Chain.Count - 2016; i--)
            {
                totalMiningTime += Chain[i].Timestamp - Chain[i - 1].Timestamp;
            }

            return totalMiningTime.TotalSeconds / 2016;
        }
        private void AdjustDifficulty()
        {
            double averageMiningTime = CalculateAverageMiningTime();

            if (averageMiningTime > TARGET_BLOCK_TIME)
            {
                Difficulty--;
            }
            else
            {
                Difficulty++;
            }

            Console.WriteLine(" > New difficulty: " + Difficulty);
        }


        public double GetTotalCoinsInBlockchain()
        {
            double totalCoins = 0;
            foreach (var block in Chain)
            {
                foreach (var transaction in block.Transactions)
                {
                    if (transaction.FromAddress == null)
                    {
                        totalCoins += transaction.Amount;
                    }
                }
            }
            return totalCoins;
        }

        private void AdjustHalving()
        {
            double totalCoinsInBlockchain = GetTotalCoinsInBlockchain();
            if (totalCoinsInBlockchain <= MAX_COINS)
            {
                MiningReward = MiningReward / 2;
            }
        }
    }
}
