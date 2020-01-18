using System;
using System.Linq;
using System.Text;

class Transaction {
    public string txId {get;}
    public txIn[] inputs;
    public txOut[] outputs;

    private string calculateTransactionId() {
        string inputString = inputs.Select(input => {
            return input.txOutId + input.txOutIndex;
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

    public static string calculateSignedTxId(int txInIndex, string privateKey) {
        // 
        // string data = calculateTransactionId();
        string data = "something encryptable";
        KeyPair keypair = new KeyPair();
        byte[] encrypted = Cryptography.RSAEncryptData(Encoding.Default.GetBytes(data), keypair.publicKey);
        string encryptedString = Encoding.Default.GetString(encrypted);
        byte[] decrypted = Cryptography.RSADecryptData(encrypted, keypair.privateKey);
        string decryptedString = Encoding.Default.GetString(decrypted);
        return encryptedString;
    }
}

class txIn {
    public string txOutId;
    public int txOutIndex; 
    public string signature;

    public txIn(string txOutId, int txOutIndex, string signature) {
        this.txOutId = txOutId;
        this.txOutIndex = txOutIndex;
        this.signature = signature;
    }
}

class txOut {
    // public key address
    public string address;
    public double amount;

    public txOut(string address, double amount) {
        this.address = address;
        this.amount = amount;
    }
}