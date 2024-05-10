using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MelangeTestPeerToPeer.Core
{
    public class Transaction
    {
        [JsonProperty("BlockIndex")]
        public int BlockIndex { get; set; }

        [JsonProperty("FromAddress")]
        public string? FromAddress { get; }

        [JsonProperty("ToAddress")]
        public string ToAddress { get; }

        [JsonProperty("Amount")]
        public double Amount { get; }

        [JsonProperty("Signature")]
        public string? Signature { get; }

        [JsonProperty("Fee")]
        public double Fee { get; }

        public Transaction(int blockIndex, string fromAddress, string toAddress, double amount, string signature, double fee = 0.000005)
        {
            BlockIndex = blockIndex;
            FromAddress = fromAddress;
            ToAddress = toAddress;
            Amount = amount;
            Signature = signature;
            Fee = fee;
        }

        public bool IsValid()
        {
            if (FromAddress == null) return true; // Reward transaction
            if (string.IsNullOrEmpty(Signature)) return false;

            using (var ecdsa = ECDsa.Create())
            {
                ecdsa.ImportSubjectPublicKeyInfo(Convert.FromBase64String(FromAddress), out _);
                return ecdsa.VerifyData(Encoding.UTF8.GetBytes(ToAddress + Amount), Convert.FromBase64String(Signature), HashAlgorithmName.SHA256);
            }
        }
    }
}
