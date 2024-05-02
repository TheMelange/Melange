﻿using Melange.core.entities.validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Melange.core.entities
{
    public class Block
    {
        public int Index { get; set; }
        public DateTime Timestamp { get; set; }
        public string PreviousHash { get; set; }
        public string Hash { get; set; }
        public List<Transaction> Transactions { get; set; }
        public int Nonce { get; set; }
        public List<ValidatorEntity> Validators { get; }
        public String MinerAddress { get; set; }


        public Block(int index, DateTime timestamp, string previousHash, List<Transaction> transactions, List<ValidatorEntity> validators, string minerAddress)
        {
            Index = index;
            Timestamp = timestamp;
            PreviousHash = previousHash;
            Transactions = transactions;
            Hash = CalculateHash();
            Nonce = 0;
            Validators = validators;
            MinerAddress = minerAddress;
        }

        public string CalculateHash()
        {
            SHA256 sha256 = SHA256.Create();
            byte[] inputBytes = Encoding.ASCII.GetBytes($"{Index}-{Timestamp}-{PreviousHash ?? ""}-{string.Join(",", Transactions)}-{Nonce}");
            byte[] outputBytes = sha256.ComputeHash(inputBytes);
            return Convert.ToBase64String(outputBytes);
        }

        public void MineBlock(int difficulty)
        {
            string prefix = new string('0', difficulty);
            while (Hash.Substring(0, difficulty) != prefix)
            {
                Nonce++;
                Hash = CalculateHash();
            }
            Console.WriteLine($"Block mined: {Hash}");
        }

        public bool HasValidTransactions()
        {
            foreach (var transaction in Transactions)
            {
                if (!transaction.IsValid())
                {
                    return false;
                }
            }
            return true;
        }

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
    }

}
