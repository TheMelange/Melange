using Melange.core.entities.validators;
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

        // ##############################
        // # Public methods
        // ##############################
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

        public void ValidatePendingTransactions(string minerAddress)
        {
            // check if the transaction is valid
            foreach (var transaction in pendingTransactions)
            {
                var block = GetBlockById(transaction.BlockIndex);
                if (block == null)
                {
                    Console.WriteLine("Block not found.");
                    transaction.Validators.Add(new ValidatorEntity(minerAddress, false));
                    continue;
                }

                // check if it is a reward transaction
                if (transaction.FromAddress == null)
                {
                    // check if the miner is the same as the one who mined the block
                    // get the block based on the blockId
                    if(transaction.ToAddress == block.MinerAddress)
                    {
                        transaction.Validators.Add(new ValidatorEntity(minerAddress, true));
                    }
                    continue;
                }

                // check if the transaction is not already in the block
                if (block.Transactions.Contains(transaction))
                {
                    Console.WriteLine("Transaction already in the block.");
                    transaction.Validators.Add(new ValidatorEntity(minerAddress, false));
                    continue;
                }

                var transactionState = Wallet.VerifySignature(Wallet.ConvertTransactionToData(transaction), transaction.Signature, transaction.FromAddress);

                var newValidator = new ValidatorEntity(minerAddress, transactionState);
                transaction.Validators.Add(newValidator);

                // TODO: create a new method to add the valid transactions to the chain
                // TODO: and after the transactions to be addded to the block, add the rewards transaction
                double transactionFee = transaction.Fee;
                double expectedReward = transactionFee / transaction.Validators.Count;
                Transaction rewardTransaction = new Transaction(block.Index, null, minerAddress, expectedReward, null, 0);
                pendingTransactions.Add(rewardTransaction);

            }

            // add pending transactions to the latest block mined
            pendingTransactions.AddRange(mempool);
            Chain[GetLatestBlock().Index].Transactions.AddRange(pendingTransactions);
            // TODO: the pending transactions should be cleared after the block is mined
            mempool.Clear();
            pendingTransactions.Clear();
        }

        public Wallet CreateWallet()
        {
            Wallet wallet = new Wallet();
            wallets.Add(wallet);
            return wallet;
        }

        public void ValidatePendingBlock(Block block, string minerCheckerAddress, string minerCheckerSignature)
        {
            if (block.Validators.Count > 50)
            {
                Console.WriteLine($"50 validators already validated the new block");
                return;
            }

            // check if the miner is the same as the one who mined the block
            var checkMiner = block.MinerAddress == minerCheckerAddress;
            if (checkMiner)
            {
                Console.WriteLine("The miner cannot validate the block.");
                return;
            }

            // check if this block is already in the chain
            var blockInChain = GetBlockByHash(block.Hash);
            if (blockInChain != null)
            {
                Console.WriteLine("The block is already in the chain.");
                block.Validators.Add(new ValidatorEntity(minerCheckerAddress, false));
            }

            // check if the block is valid based on the PoW
            var isValidBlock = block.IsValid(difficulty);
            if (isValidBlock)
            {
                block.Validators.Add(new ValidatorEntity(minerCheckerAddress, true));
            }
            else
            {
                block.Validators.Add(new ValidatorEntity(minerCheckerAddress, false));
            }
        }
        public void MineNewBlock(string minerAddress)
        {
            Block block = new Block(Chain.Count, DateTime.Now, GetLatestBlock().Hash, pendingTransactions, new List<ValidatorEntity>(), minerAddress);
            block.MineBlock(difficulty);
            double totalCoinsInBlockchain = GetTotalCoinsInBlockchain();
            if (totalCoinsInBlockchain <= MAX_COINS)
            {
                // generate mining reward transaction
                Transaction rewardTransaction = new Transaction(block.Index, null, minerAddress, miningReward, null, 0);
                pendingTransactions.Add(rewardTransaction);

            }
            // TODO: the block should be added to the chain only if the block is valid (validators should validate the block)
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


        // ##############################
        // # Public Get methods
        // ##############################
        public double GetMiningReward()
        {
            return this.miningReward;
        }

        public Block GetLatestBlock()
        {
            return Chain[^1];
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

        public Block? GetBlockByHash(string hash)
        {
            return Chain.FirstOrDefault(b => b.Hash == hash);
        }
        public Block? GetBlockById(int id)
        {
            return Chain.FirstOrDefault(b => b.Index == id);
        }

        // ##############################
        // # Private methods
        // ##############################
        private void InitializeChain()
        {
            Console.WriteLine("Initializing chain...");
            Console.WriteLine("Genesis block...");
            Block genesisBlock = new Block(0, DateTime.Now, null, new List<Transaction>(), new List<ValidatorEntity>(), null);
            if (Chain.Count == 0)
                genesisBlock.PreviousHash = null;
            else
                genesisBlock.PreviousHash = GetLatestBlock().Hash;

            genesisBlock.MineBlock(difficulty);
            Chain.Add(genesisBlock);
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
    }
}
