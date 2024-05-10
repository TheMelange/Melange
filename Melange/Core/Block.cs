using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Melange.Core
{
    public class Block
    {
        [JsonProperty("Index")]
        public int Index { get; set; }

        [JsonProperty("Timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonProperty("PreviousHash")]
        public string? PreviousHash { get; set; }

        [JsonProperty("Hash")]
        public string Hash { get; set; }

        [JsonProperty("Transactions")]
        public List<Transaction> Transactions { get; set; }

        [JsonProperty("Nonce")]
        public int Nonce { get; set; }

        public bool IsValid(int difficulty)
        {
            if (Hash == null || !Hash.StartsWith(new string('0', difficulty)))
            {
                return false;
            }

            if (Index <= 0)
            {
                return false;
            }

            if (Index > 0 && string.IsNullOrEmpty(PreviousHash))
            {
                return false;
            }

            if (Index > 0 && !PreviousHash.Equals(PreviousHash))
            {
                return false;
            }

            if (Timestamp > DateTime.Now)
            {
                return false;
            }

            string calculatedHash = CalculateHash();
            if (!Hash.Equals(calculatedHash))
            {
                return false;
            }

            return true;
        }

        public string CalculateHash()
        {
            SHA256 sha256 = SHA256.Create();
            byte[] inputBytes = Encoding.ASCII.GetBytes($"{Index}-{Timestamp}-{PreviousHash ?? ""}-{Nonce}");
            byte[] outputBytes = sha256.ComputeHash(inputBytes);
            return Convert.ToBase64String(outputBytes);
        }

        public Block(int index, DateTime timestamp, string previousHash, List<Transaction> transactions)
        {
            Index = index;
            Timestamp = timestamp;
            PreviousHash = previousHash;
            Transactions = transactions;
            Hash = CalculateHash();
            Nonce = 0;
        }

        public void MineBlock(int difficulty)
        {
            string prefix = new string('0', difficulty);
            while (Hash.Substring(0, difficulty) != prefix)
            {
                Nonce++;
                Hash = CalculateHash();
            }
            //Console.WriteLine($"Block mined: {Hash}");
        }

    }
}
