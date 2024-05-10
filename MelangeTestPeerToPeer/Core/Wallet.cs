using NBitcoin;
using NBitcoin.DataEncoders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MelangeTestPeerToPeer.Core
{
    public class Wallet
    {
        public Mnemonic Mnemonic { get; private set; }
        public Key PrivateKey { get; private set; }
        public PubKey PublicKey { get; private set; }

        public bool LoginWithMnemonic(string mnemonicPhrase)
        {
            try
            {
                Mnemonic mnemonic = new Mnemonic(mnemonicPhrase);
                ExtKey masterKey = mnemonic.DeriveExtKey();
                Key privateKey = masterKey.PrivateKey;
                PubKey publicKey = privateKey.PubKey;

                return publicKey == this.PublicKey;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        public void CreateNewWallet(int wordCount = 12)
        {
            if (wordCount != 12 && wordCount != 24)
            {
                throw new ArgumentException("Word count must be 12 or 24.");
            }

            this.Mnemonic = new Mnemonic(Wordlist.English, (wordCount == 12) ? WordCount.Twelve : WordCount.TwentyFour);
            ExtKey masterKey = this.Mnemonic.DeriveExtKey();
            this.PrivateKey = masterKey.PrivateKey;
            this.PublicKey = this.PrivateKey.PubKey;

            Console.WriteLine($"Wallet created with the following mnemonic words: {this.Mnemonic}");
            Console.WriteLine($"Private key: {this.PrivateKey.GetWif(Network.Main)}");
            Console.WriteLine($"Public key: {this.PublicKey.GetAddress(ScriptPubKeyType.Legacy, Network.Main)}");
        }

        public string SignTransaction(string transactionData)
        {
            var message = new uint256(Encoders.Hex.DecodeData(transactionData)); // Assume transactionData is in hex format
            var signature = this.PrivateKey.SignCompact(message);
            return Encoders.Hex.EncodeData(signature.Signature) + signature.RecoveryId.ToString("X"); // Append recoveryId as a hexadecimal string
        }

        public bool VerifySignature(string transactionData, string signatureHex)
        {
            var message = new uint256(Encoders.Hex.DecodeData(transactionData)); // Assume transactionData is in hex format
            int recoveryId = Convert.ToInt32(signatureHex.Substring(signatureHex.Length - 1), 16); // Last character is recoveryId
            byte[] signatureBytes = Encoders.Hex.DecodeData(signatureHex.Substring(0, signatureHex.Length - 1));

            CompactSignature compactSig = new CompactSignature(recoveryId, signatureBytes);
            bool result = this.PublicKey.Verify(message, compactSig.Signature);
            return result;
        }

        public bool IsValidWallet()
        {
            return this.PrivateKey != null && this.PublicKey != null;
        }
    }
}
