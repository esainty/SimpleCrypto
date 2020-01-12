using System.Security.Cryptography;
using System.Text;
using System;

public static class Cryptography {
    public static string hashStringSHA256(string toHash) {
        // Compute the hash as a byte array
        byte[] hash = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(toHash));
        // Convert to binary string.
        string binarystring = "";
        foreach (byte b in hash) {
            binarystring += Convert.ToString(b, 2).PadLeft(8, '0');
        }
        return binarystring;
    }
}