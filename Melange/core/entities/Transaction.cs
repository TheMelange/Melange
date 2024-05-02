using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Melange.core.entities.validators;

namespace Melange.core.entities
{
    public class Transaction
    {
        public int BlockIndex { get; set; }
        public string FromAddress { get; }
        public string ToAddress { get; }
        public double Amount { get; }
        public string Signature { get; }
        public double Fee { get; }
        public List<ValidatorEntity> Validators { get; set; }

        public Transaction(int blockIndex, string fromAddress, string toAddress, double amount, string signature, double fee = 0.000005)
        {
            BlockIndex = blockIndex;
            FromAddress = fromAddress;
            ToAddress = toAddress;
            Amount = amount;
            Signature = signature;
            Fee = fee;
            Validators = new List<ValidatorEntity>();
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
