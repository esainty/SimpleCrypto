using System;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Web;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace SimpleCrypto {
    class Program {
        static async Task Main(string[] args) {
            // BlockchainManager bcManager = new BlockchainManager();
            // await bcManager.initialise();

            KeyPair keys = new KeyPair();
            CoinbaseTransaction coinbaseTx = generateCoinbaseTransaction(keys.publicKey);
            Transaction subsequentTx = generateSubsequentTransaction(coinbaseTx, keys.privateKey);
        }

        private static CoinbaseTransaction generateCoinbaseTransaction(byte[] publicKey) {
            CoinbaseTransaction coinbase = new CoinbaseTransaction(publicKey, 0);
            if (coinbase.isValidCoinbaseTransaction(0)) {
                return coinbase;
            } else {
                throw new InvalidDataException("Invalid coinbase transaction");
            }
        }

        public static Transaction generateSubsequentTransaction(CoinbaseTransaction previousTx, byte[] privateKey) {
            byte[] signature = Tx.calculateSignedTxId(previousTx.id, privateKey);
            TxIn[] ins = {new TxIn(previousTx.id, 0, signature)};
            byte[] newPublic = new KeyPair().publicKey;
            TxOut[] outs = {new TxOut(newPublic, 25)};
            Transaction testTransaction = new Transaction(ins, outs);
            if (testTransaction.isValidTransaction()) {
                return testTransaction;
            } else {
                throw new InvalidDataException("ooga booga");
            }
        }

        public static BlockData assembleBlockData(CoinbaseTransaction coinbaseTx, Transaction[] transactions) {
            return new BlockData(coinbaseTx, transactions, 0);
        }
    }
}
