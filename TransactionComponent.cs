using System;
using System.Linq;

namespace SimpleCrypto {
    public class TxIn {
        public byte[] txId;
        public int txOutIndex; 
        public byte[] signature;

        public TxIn(byte[] txId, int txOutIndex, byte[] signature) {
            this.txId = txId;
            this.txOutIndex = txOutIndex;
            this.signature = signature;
        }

        public bool isValidTxIn(UTxOut[] uTxOSet) {
            // Confirm TxIn is in the set of Unspent Transaction Outs (uTxOSet)
            UTxOut referencedUTxO = Array.Find(uTxOSet, (uTxO) => {
                return txId == uTxO.txId && txOutIndex == uTxO.txOutIndex;
            });
            if (referencedUTxO == null) {
                Console.WriteLine($"No UTxO exists for provided TxIn TxID: {txId}, TxOutIndex: {txOutIndex}");
                return false;
            }

            // Confirm that the public key stored in the unspent UTxO is the same one used to sign the transaction
            // ********* This might be completely wrong. Check what is signing what originally. *********
            byte[] address = referencedUTxO.address;
            bool validSignature = Cryptography.RSAVerifySignature(signature, txId, address);
            if (!validSignature) {
                Console.WriteLine("Verification failed: signature for TxIn does not match for Transaction");
            }
            return validSignature;
        }

        public static bool isValidTxInArray(TxIn[] txIns, UTxOut[] UTxOuts) {
            foreach (TxIn txIn in txIns) {
                if (!txIn.isValidTxIn(UTxOuts)) {
                    return false;
                }
            }
            return true;
        }
    }

    public class TxOut {
        // public key address
        public byte[] address;
        public int amount;

        public TxOut(byte[] address, int amount) {
            this.address = address;
            this.amount = amount;
        }

        public bool isValidTxOut() {
            if (address.Length > 140) {
                Console.WriteLine("Address must be valid RSA Public Key");
                return false;
            }
            return true;
        }

        public static bool isValidTxOutArray(TxOut[] txOuts, TxIn[] txIns, UTxOut[] uTxOSet) {
            foreach (TxOut txOut in txOuts) {
                if (!txOut.isValidTxOut()) {
                    return false;
                }
            }
            
            // Get combined total output value
            int totalTxOutValues = txOuts.Select(txOut => {
                return txOut.amount;
            }).Aggregate((amountA, amountB) => {
                return amountA + amountB;
            });

            // Get combined total input value
            int totalTxInValues = txIns.Select(txIn => {
                UTxOut referencedUTxO = Array.Find(uTxOSet, (uTxO) => {
                    return txIn.txId == uTxO.txId && txIn.txOutIndex == uTxO.txOutIndex;
                });
                return referencedUTxO.amount;
            }).Aggregate((amountA, amountB) => {
                return amountA + amountB;
            });

            // Ensure that inputs and outputs match
            if (totalTxOutValues != totalTxInValues) {
                Console.WriteLine($"Transaction Invalid. Total in values do not match total out values");
                return false;
            }

            return true;
        }
    }

    public class UTxOut {
        public byte[] txId;
        public int txOutIndex;
        public byte[] address;
        public int amount;

        public UTxOut(byte[] txId, int txOutIndex, byte[] address, int amount) {
            this.txId = txId;
            this.txOutIndex = txOutIndex;
            this.address = address;
            this.amount = amount;
        }

        public override bool Equals(object obj) {
            UTxOut uTxO = obj as UTxOut;

            return (uTxO != null
                && txId == uTxO.txId
                && txOutIndex == uTxO.txOutIndex); 
        }

        // I don't know about this at all
        public override int GetHashCode() {
            return (txId + txOutIndex.ToString()).GetHashCode();
        }

        public static UTxOut[] deriveUnspentTxOuts(Transaction[] transactions) {
            return transactions.Select((tx) => {
                return tx.txOuts.Select((TxOut output, int index) => {
                    return new UTxOut(tx.id, index, output.address, output.amount);
                }).ToArray();
            }).Aggregate((arrayA, arrayB) => {
                return arrayA.Concat(arrayB).ToArray();
            });
        }

        public static UTxOut[] deriveConsumedTxOuts(Transaction[] transactions) {
            // *** IS THIS THE BEST WAY TO HANDLE THIS?
            return transactions.Select((tx) => {
                return tx.txIns;
            }).Aggregate((arrayA, arrayB) => {
                return arrayA.Concat(arrayB).ToArray();
            }).Select((txin) => {
                return new UTxOut(txin.txId, txin.txOutIndex, new byte[0], 0); 
            }).ToArray();
        }

        public static UTxOut[] deriveRemainingUnspentTxOuts(UTxOut[] unspentTxOs, UTxOut[] consumedTxOs) {
            // var n = unspentTxOs.Where((uTxO) => {
            //     return consumedTxOs.All((cTxO) => {
            //         return (uTxO.txId != cTxO.txId || uTxO.txOutIndex != cTxO.txOutIndex);
            //     });
            // }).ToArray();

            // Compares UTxOs to CTxOs using overridden equality operator. 
            // LINQ Except method uses hash tables and is faster than nested comparisons. 
            return unspentTxOs.Except(consumedTxOs).ToArray();
        }
    }
}