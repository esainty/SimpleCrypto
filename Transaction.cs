using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

public class Transaction {
    public string id {get; private set;}
    public TxIn[] txIns;
    public TxOut[] txOuts;

    public Transaction(TxIn[] txIns, TxOut[] txOuts) {
        this.txIns = txIns;
        this.txOuts = txOuts;
        calculateTransactionId();
    }

    private void calculateTransactionId() {
        if (txIns == null || txOuts == null) {
            throw new NullReferenceException("TxIns/TxOuts have not been instantiated");
        }
        id = calculateTransactionId(txIns, txOuts);
    }

    private static string calculateTransactionId(TxIn[] inputs, TxOut[] outputs) {
        string inputString = inputs.Select(input => {
            return input.txId + input.txOutIndex;
        }).Aggregate((stringA, stringB) => {
            return stringA + stringB;
        });

        string outputString = outputs.Select(output => {
            return output.address + output.amount;
        }).Aggregate((outputA, outputB) => {
            return outputA + outputB;
        });

        return Cryptography.SHA256HashString(inputString + outputString);
    }

    public static string calculateSignedTxId(string txId, string privateKey) {
        return Cryptography.RSASignHash(Encoding.Default.GetBytes(txId), privateKey).ToString();
    }

    public static bool isValidTransaction(Transaction tx) {
        return (tx.id != null &&
                tx.txIns != null &&
                tx.txOuts != null &&
                tx.id == calculateTransactionId(tx.txIns, tx.txOuts) 
            );
    }

    public static bool validateTxIn(TxIn txIn, Transaction tx, UTxOut[] uTxOSet) {
        // Confirm TxIn is in the set of Unspent Transaction Outs (uTxOSet)
        UTxOut referencedUTxO = Array.Find(uTxOSet, (uTxO) => {
            return txIn.txId == uTxO.txId && txIn.txOutIndex == uTxO.txOutIndex;
        });
        if (referencedUTxO == null) {
            Console.WriteLine($"No UTxO exists for provided TxIn TxID: {txIn.txId}, TxOutIndex: {txIn.txOutIndex}");
            return false;
        }

        // Confirm that the public key stored in the unspent UTxO is the same one used to sign the transaction
        // ********* This might be completely wrong. Check what is signing what originally. *********
        string address = referencedUTxO.address;
        bool validSignature = Cryptography.RSAVerifyHash(Encoding.Default.GetBytes(txIn.signature), Encoding.Default.GetBytes(tx.id), address);
        if (!validSignature) {
            Console.WriteLine("Verification failed: signature for TxIn does not match for Transaction");
        }
        return validSignature;
    }

    public static bool validateTxOuts(TxOut[] txOuts, TxIn[] txIns, UTxOut[] uTxOSet) {
        double totalTxOutValues = txOuts.Select(txOut => {
            return txOut.amount;
        }).Aggregate((amountA, amountB) => {
            return amountA + amountB;
        });

        double totalTxInValues = txIns.Select(txIn => {
            UTxOut referencedUTxO = Array.Find(uTxOSet, (uTxO) => {
                return txIn.txId == uTxO.txId && txIn.txOutIndex == uTxO.txOutIndex;
            });
            return referencedUTxO.amount;
        }).Aggregate((amountA, amountB) => {
            return amountA + amountB;
        });

        if (totalTxOutValues != totalTxInValues) {
            Console.WriteLine($"Transaction Invalid. Total in values do not match total out values");
            return false;
        }

        return true;
    }
}

public class TxIn {
    public string txId;
    public int txOutIndex; 
    public string signature;

    public TxIn(string txOutId, int txOutIndex, string signature) {
        this.txId = txOutId;
        this.txOutIndex = txOutIndex;
        this.signature = signature;
    }
}

public class TxOut {
    // public key address
    public string address;
    public double amount;

    public TxOut(string address, double amount) {
        this.address = address;
        this.amount = amount;
    }
}

public class UTxOut {
    public string txId;
    public int txOutIndex;
    public string address;
    public double amount;

    public UTxOut(string txId, int txOutIndex, string address, double amount) {
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
        return transactions.Select((tx) => {
            return tx.txIns;
        }).Aggregate((arrayA, arrayB) => {
            return arrayA.Concat(arrayB).ToArray();
        }).Select((txin) => {
            return new UTxOut(txin.txId, txin.txOutIndex, "", 0); 
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