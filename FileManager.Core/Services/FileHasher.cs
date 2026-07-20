using System.Security.Cryptography;

namespace FileManager.Core.Services;

public class FileHasher
{
    public string Calculate(string path)
    {
        using var stream = File.OpenRead(path);

        using var sha256 = SHA256.Create();

        var hash = sha256.ComputeHash(stream);

        return Convert.ToHexString(hash);
    }
}