using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Melange.core.entities
{
    public class Blockchain
    {
        public List<Block> Chain { get; private set; }
        private List<Transaction> pendingTransactions;
        private int difficulty;
        private double miningReward;
        private List<Transaction> mempool;
        private List<Wallet> wallets;
        public const int MAX_COINS = 1000000;
        public const double TARGET_BLOCK_TIME = 600; // 10 minutes

        public Blockchain(int difficulty, double miningReward)
        {
            Chain = new List<Block>();
            pendingTransactions = new List<Transaction>();
            this.difficulty = difficulty;
            this.miningReward = miningReward;
            mempool = new List<Transaction>();
            wallets = new List<Wallet>();
            InitializeChain();
        }

        public double GetMiningReward()
        {
            return this.miningReward;
        }

        private void InitializeChain()
        {
            Console.WriteLine("Initializing chain...");
            Console.WriteLine("Genesis block...");
            AddBlock(new Block(0, DateTime.Now, null, new List<Transaction>(), new List<string>()));
        }

        public Block GetLatestBlock()
        {
            return Chain[^1];
        }

        public void AddBlock(Block block)
        {
            if (Chain.Count == 0)
                block.PreviousHash = null;
            else
                block.PreviousHash = GetLatestBlock().Hash;

            block.MineBlock(difficulty);
            Chain.Add(block);
        }

        public void AddTransaction(Transaction transaction)
        {
            if (!transaction.IsValid())
            {
                Console.WriteLine("Transaction is not valid.");
                return;
            }
            mempool.Add(transaction);
        }

        public void MineNewBlock(string minerAddress)
        {
            Block block = new Block(Chain.Count, DateTime.Now, GetLatestBlock().Hash, pendingTransactions, new List<string>());
            block.MineBlock(difficulty);
            double totalCoinsInBlockchain = GetTotalCoinsInBlockchain();
            if (totalCoinsInBlockchain <= MAX_COINS)
            {
                // generate mining reward transaction
                Transaction rewardTransaction = new Transaction(null, minerAddress, miningReward, null, 0);
                pendingTransactions.Add(rewardTransaction);
            }
            Chain.Add(block);

            // adjust the difficulty
            if (Chain.Count % 2016 == 0)
            {
                Console.WriteLine("Adjusting difficulty...");
                AdjustDifficulty();
            }

            // adjust the mining reward
            if(Chain.Count % 210000 == 0)
            {
                Console.WriteLine("Adjusting mining reward...");
                AdjustHalving();
            }

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
                difficulty--;
            }
            else
            {
                difficulty++;
            }

            Console.WriteLine(" > New difficulty: " + difficulty);
        }

        private void AdjustHalving()
        {
            double totalCoinsInBlockchain = GetTotalCoinsInBlockchain();
            if (totalCoinsInBlockchain <= MAX_COINS)
            {
                miningReward = miningReward / 2;
            }
        }

        public void MinePendingTransactions(string minerAddress)
        {
            double totalFees = pendingTransactions.Sum(t => t.Fee);
            double totalReward = totalFees / Chain[GetLatestBlock().Index].Validators.Count;

            if(pendingTransactions.Count != 0)
            {
                Transaction rewardTransaction = new Transaction(null, minerAddress, totalReward, null, 0);
                pendingTransactions.Add(rewardTransaction);
                pendingTransactions.AddRange(mempool);
                // add pending transactions to the latest block mined
                Chain[GetLatestBlock().Index].Transactions.AddRange(pendingTransactions);

                mempool.Clear();
                pendingTransactions.Clear();
            }

            //Block block = new Block(Chain.Count, DateTime.Now, GetLatestBlock().Hash, pendingTransactions, new List<string>());
            //Chain.Add(block);
            //Console.WriteLine(" Reward Block : I:" + block.Index + " H:" + block.Hash + " PH:" + block.PreviousHash + " T:" + block.Timestamp + " VC:" + block.Validators.Count);
            //block.MineBlock(difficulty);
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

        public int GetTotalTransactions()
        {
            int totalTransactions = 0;
            foreach (var block in Chain)
            {
                totalTransactions += block.Transactions.Count;
            }
            return totalTransactions;
        }

        public double GetBalance(string address)
        {
            double balance = 0;
            foreach (var block in Chain)
            {
                foreach (var transaction in block.Transactions)
                {
                    if (transaction.FromAddress == address)
                    {
                        balance -= transaction.Amount;
                    }
                    if (transaction.ToAddress == address)
                    {
                        balance += transaction.Amount;
                    }
                }
            }
            return balance;
        }

        public Wallet CreateWallet()
        {
            Wallet wallet = new Wallet();
            wallets.Add(wallet);
            return wallet;
        }

        public void AddValidatorsToBlock(int blockIndex, List<string> validators)
        {
            if (blockIndex < 0 || blockIndex >= Chain.Count)
            {
                Console.WriteLine("Invalid block index.");
                return;
            }

            Chain[blockIndex].Validators.AddRange(validators);
        }
    }
}
