using System.Security.Cryptography;
using System.Text;
using System;

public class KeyPair {
    public string publicKey;
    public string privateKey;

    public KeyPair() {
        KeyPair newKeyPair = createKeyPair();
        this.publicKey = newKeyPair.publicKey;
        this.privateKey = newKeyPair.privateKey;
    }

    public KeyPair(string publicKey, string privateKey) {
        this.publicKey = publicKey;
        this.privateKey = privateKey;
    }

    public static KeyPair createKeyPair() {
        using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(1024)) {
            return new KeyPair(rsa.ToXmlString(false), rsa.ToXmlString(true));
        };
    }
}

public static class Cryptography {
    public static string SHA256HashString(string toHash) {
        // Compute the hash as a byte array
        byte[] hash = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(toHash));
        // Convert to binary string.
        string binarystring = "";
        foreach (byte b in hash) {
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

    public static byte[] RSADecryptData(byte[] data, string privateKey) {
        using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider()) {
            rsa.FromXmlString(privateKey);
            return rsa.Decrypt(data, true);
        }
    }

    public static byte[] RSASignHash(byte[] data, string privateKey) {
        using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider()) {
            rsa.FromXmlString(privateKey);
            return rsa.SignHash(data, CryptoConfig.MapNameToOID("SHA256"));
        }
    }

    public static bool RSAVerifyHash(byte[] data, byte[] hashedOriginal, string publicKey) {
        using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider()) {
            rsa.FromXmlString(publicKey);
            return rsa.VerifyHash(hashedOriginal, CryptoConfig.MapNameToOID("SHA256"), data);
        }
    }
}