using System.Security.Cryptography;
using System.Text;

public static class Cryptography {
    public static string hashStringSHA256(string toHash) {
        // Compute the hash as a byte array
        byte[] hash = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(toHash));
        // Then return byte array as Hex String;
        StringBuilder builder = new StringBuilder();
        for (int i = 0; i < hash.Length; i++) {
            // Add byte as 2-digit lowercase hex (x2)
            builder.Append(hash[i].ToString("x2"));
        }
        return builder.ToString();
    }
}