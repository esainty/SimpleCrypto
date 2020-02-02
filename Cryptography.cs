using System.Security.Cryptography;
using System.Text;
using System;

public class KeyPair {
    public byte[] publicKey;
    public byte[] privateKey;

    public KeyPair() {
        KeyPair newKeyPair = createKeyPair();
        this.publicKey = newKeyPair.publicKey;
        this.privateKey = newKeyPair.privateKey;
    }

    // public KeyPair(string publicKey, string privateKey) {
    //     this.publicKey = publicKey;
    //     this.privateKey = privateKey;
    // }

    public KeyPair(byte[] publicKey, byte[] privateKey) {
        this.publicKey = publicKey;
        this.privateKey = privateKey;
    }

    public static KeyPair createKeyPair() {
        using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(1024)) {
            return new KeyPair(rsa.ExportRSAPublicKey(), rsa.ExportRSAPrivateKey());
        };
    }
}

public static class Cryptography {
    public static byte[] SHA256HashBytes(byte[] toHash) {
        return SHA256.Create().ComputeHash(toHash);
    }
    
    public static string SHA256HashString(string toHash) {
        // Compute the hash as a byte array
        byte[] hash = SHA256.Create().ComputeHash(Encoding.Default.GetBytes(toHash));
        // Convert to binary string.
        return toBinaryString(hash);
    }

    public static string toBinaryString(byte[] bytes) {
        string binarystring = "";
        foreach (byte b in bytes) {
            binarystring += Convert.ToString(b, 2).PadLeft(8, '0');
        }
        return binarystring;
    }

    public static byte[] RSAEncryptData(byte[] data, string publicKey) {
        using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider()) {
            rsa.FromXmlString(publicKey);
            return rsa.Encrypt(data, true);
        }
    }

    public static byte[] RSAEncryptData(byte[] data, byte[] publicKey) {
        // *** TEST THIS
        using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider()) {
            int outBox;
            rsa.ImportRSAPublicKey(publicKey, out outBox);
            return rsa.Encrypt(data, true);
        }
    }

    public static byte[] RSADecryptData(byte[] data, string privateKey) {
        using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider()) {
            rsa.FromXmlString(privateKey);
            return rsa.Decrypt(data, true);
        }
    }

    public static byte[] RSASignHash(byte[] data, byte[] privateKey) {
        using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider()) {
            int outBox;
            rsa.ImportRSAPrivateKey(privateKey, out outBox);
            return rsa.SignHash(data, CryptoConfig.MapNameToOID("SHA256"));
        }
    }

    public static bool RSAVerifySignature(byte[] signature, byte[] originalHash, byte[] publicKey) {
        using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider()) {
            int outBox;
            rsa.ImportRSAPublicKey(publicKey, out outBox);
            return rsa.VerifyHash(originalHash, CryptoConfig.MapNameToOID("SHA256"), signature);
        }
    }
}