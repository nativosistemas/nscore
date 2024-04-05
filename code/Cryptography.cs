using System.Security.Cryptography;
using System.Text;
using DocumentFormat.OpenXml.Bibliography;

namespace nscore;
public class Cryptography
{
    public static string Ejemplo()
    {
        string original = "Here is some data to hash!";
        byte[] originalBytes = Encoding.UTF8.GetBytes(original);

        var hash = ComputeHash(originalBytes);
        var verify = VerifyHash(originalBytes, hash);

        Console.WriteLine("Text to Hash:      {0}", original);
        Console.WriteLine("Text hashed:       {0}", Convert.ToBase64String(hash));
        Console.WriteLine("Text Verify Result {0}", verify);
        return "";
    }
    public static byte[] ComputeHash(byte[] data)
    {
        using var sha256 = SHA256.Create();
        return sha256.ComputeHash(data);
    }

    public static bool VerifyHash(byte[] original, byte[] hash)
    {
        using var sha256 = SHA256.Create();
        var newHash = sha256.ComputeHash(original);
        return newHash.SequenceEqual(hash);
    }
}