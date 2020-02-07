using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace SimpleCrypto {
    public abstract class Tx {
        public const int COINBASE_AMOUNT = 50;
        public byte[] id {get; protected set;}
        public TxIn[] txIns;
        public TxOut[] txOuts;

        public abstract byte[] calculateTransactionId();

        protected byte[] _reduceInOut() {
            if (txIns == null || txOuts == null) {
                throw new NullReferenceException("TxIns/TxOuts have not been instantiated");
            }

            byte[] inputBytes = txIns.Select(input => {
                return input.txId.Concat(BitConverter.GetBytes(input.txOutIndex));
            }).Aggregate((inputA, inputB) => {
                return inputA.Concat(inputB);
            }).ToArray();

            byte[] outputBytes = txOuts.Select(output => {
                return output.address.Concat(BitConverter.GetBytes(output.amount));
            }).Aggregate((outputA, outputB) => {
                return outputA.Concat(outputB);
            }).ToArray();

            return inputBytes.Concat(outputBytes).ToArray();
        }

        public byte[] getBytes() {
            return id.Concat(_reduceInOut()).ToArray();
        }

        public static byte[] calculateSignedTxId(byte[] txId, byte[] privateKey) {
            return Cryptography.RSASignHash(txId, privateKey);
        }
    }

    public class Transaction : Tx {

        public Transaction(TxIn[] txIns, TxOut[] txOuts) {
            this.txIns = txIns;
            this.txOuts = txOuts;
            this.id = calculateTransactionId();
        }

        override public byte[] calculateTransactionId() {
            byte[] txData = _reduceInOut();
            return Cryptography.SHA256HashBytes(txData);
        }

        public bool isValidTransaction(UTxOut[] UTxOuts) {
            if (id == null || txIns == null || 
                txOuts == null || !id.SequenceEqual(calculateTransactionId()) ) {
                    Console.WriteLine("Basic structure of transaction is invalid");
                    return false;
                }
            if (!TxOut.isValidTxOutArray(txOuts, txIns, UTxOuts)) {
                Console.WriteLine("TxOuts array is invalid");
                return false;
            }
            if (!TxIn.isValidTxInArray(txIns, UTxOuts)) {
                Console.WriteLine("TxIns array is invalid");
                return false;
            }
            return true;
        }
    }

    public class CoinbaseTransaction : Tx {
        public int blockHeight;

        public CoinbaseTransaction(byte[] address, int blockHeight) {
            this.txIns = new TxIn[] {new TxIn(new byte[0], 0, new byte[0])};
            this.txOuts = new TxOut[] {new TxOut(address, COINBASE_AMOUNT)};
            this.blockHeight = blockHeight;
            this.id = calculateTransactionId();
        }

        override public byte[] calculateTransactionId() {
            byte[] txData = _reduceInOut().Concat(BitConverter.GetBytes(blockHeight)).ToArray();
            return Cryptography.SHA256HashBytes(txData);
        }

        public bool isValidCoinbaseTransaction(int currentBlockHeight) {
            if (!calculateTransactionId().SequenceEqual(id)) {
                Console.WriteLine("Invalid coinbase transaction ID");
                return false;
            }
            if (txIns.Length != 1) {
                Console.WriteLine("Coinbase transaction can only have one input");
                return false;
            }
            if (txOuts.Length != 1) {
                Console.WriteLine("Coinbase transaction can only have one output");
                return false;
            }
            if (txOuts[0].amount != COINBASE_AMOUNT) {
                Console.WriteLine("Invalid amount in coinbase transaction");
                return false;
            }
            return true;
        }

        new public byte[] getBytes() {
            return base.getBytes().Concat(BitConverter.GetBytes(blockHeight)).ToArray();
        }
    }
}