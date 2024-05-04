using Melange.core;
using Melange.core.entities;
using System.Xml.Linq;

BitcoinNode node = new BitcoinNode();
node.Start();

//Blockchain blockchain = new Blockchain(4, 50);
//Node node1 = new Node(blockchain);
//Node node2 = new Node(blockchain);

//Wallet alice = new Wallet();
//Wallet bob = new Wallet();

//Wallet miner1 = new Wallet();
//Wallet miner2 = new Wallet();

////node1.AddTransaction(alice.PublicKey, bob.PublicKey, 50, alice.GetPrivateKey());
////node1.AddTransaction(alice.PublicKey, bob.PublicKey, 30, alice.GetPrivateKey());
////node2.AddTransaction(alice.PublicKey, bob.PublicKey, 10, alice.GetPrivateKey());
////node2.AddTransaction(alice.PublicKey, bob.PublicKey, 20, alice.GetPrivateKey());


//while (true)
//{
//    node2.MinePendingTransactions(miner2.PublicKey);
//    node2.MineNewBlock(miner2.PublicKey);

//    //node1.MinePendingTransactions(miner1.PublicKey);
//    //node1.MineNewBlock(miner1.PublicKey);

//    //List<string> validators = new List<string> { miner1.PublicKey, miner2.PublicKey };
//    //node1.AddValidatorsToBlock(blockchain.Chain.Count - 1, validators);

//    Console.WriteLine("=====================================================");
//    Console.WriteLine("Blockchain on Node 1:");
//    node1.DisplayChain();

//    //Console.WriteLine("=====================================================");
//    //Console.WriteLine("Blockchain on Node 2:");
//    //node2.DisplayChain();
//    Console.WriteLine("=====================================================");

//    //Console.WriteLine($"Alice's balance: {blockchain.GetBalance(alice.PublicKey)}");
//    //Console.WriteLine($"Bob's balance: {blockchain.GetBalance(bob.PublicKey)}");
//    //Console.WriteLine($"Miner 1's melange: {blockchain.GetBalance(miner1.PublicKey)}");
//    Console.WriteLine($"Miner 2's melange: {blockchain.GetBalance(miner2.PublicKey)}");
//    Console.WriteLine("=====================================================");
//    Console.WriteLine($"Total Melange generated: {blockchain.GetTotalCoinsInBlockchain()} of {Blockchain.MAX_COINS}");
//    Console.WriteLine($"Total transactions: {blockchain.GetTotalTransactions()}");
//    Console.WriteLine($"Total blocks: {blockchain.GetLatestBlock().Index}");
//    Console.WriteLine("=====================================================");
//}


