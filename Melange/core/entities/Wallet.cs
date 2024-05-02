using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Melange.core.entities
{
    public class Wallet
    {
        public string PublicKey { get; }
        private string PrivateKey { get; }

        public Wallet()
        {
            using (var ecdsa = ECDsa.Create())
            {
                ecdsa.GenerateKey(ECCurve.NamedCurves.nistP256);
                PublicKey = Convert.ToBase64String(ecdsa.ExportSubjectPublicKeyInfo());
                PrivateKey = Convert.ToBase64String(ecdsa.ExportPkcs8PrivateKey());
            }
        }

        public string SignTransaction(string transactionData)
        {
            using (var ecdsa = ECDsa.Create())
            {
                ecdsa.ImportPkcs8PrivateKey(Convert.FromBase64String(PrivateKey), out _);
                return Convert.ToBase64String(ecdsa.SignData(Encoding.UTF8.GetBytes(transactionData), HashAlgorithmName.SHA256));
            }
        }

        public bool VerifySignature(string data, string signature, string publicKey)
        {
            using (var ecdsa = ECDsa.Create())
            {
                ecdsa.ImportSubjectPublicKeyInfo(Convert.FromBase64String(publicKey), out _);
                return ecdsa.VerifyData(Encoding.UTF8.GetBytes(data), Convert.FromBase64String(signature), HashAlgorithmName.SHA256);
            }
        }
        // todo: temporary method to get private key for testing purposes
        public string GetPrivateKey()
        {
            return PrivateKey;
        }   
    }
}
