using System.Security.Cryptography;

namespace Racoon.Core.Cryptography;

public class AesCryptography : ICryptography
{
    public byte[] InitializeVector { get; private set; }
    public byte[] Key { get; private set; }
    private Aes aes = Aes.Create();
    public AesCryptography()
    {
        aes.GenerateIV();
        aes.GenerateKey();
        this.InitializeVector = aes.IV;
        this.Key = aes.Key;
        AssertKeyLength(this.Key.Length);
        AssertInitializeVectorLength(this.InitializeVector.Length);
    }

    public AesCryptography(byte[] key)
    {
        aes.GenerateIV();
        this.InitializeVector = aes.IV;
        this.Key = key;
        AssertKeyLength(this.Key.Length);
        AssertInitializeVectorLength(this.InitializeVector.Length);
        aes.Key = key;
    }

    public AesCryptography(byte[] key, byte[] initializeVector)
    {
        this.InitializeVector = initializeVector;
        this.Key = key;
        AssertKeyLength(this.Key.Length);
        AssertInitializeVectorLength(this.InitializeVector.Length);
        aes.Key = key;
        aes.IV = initializeVector;
    }

    public byte[] Decrypt(byte[] data)
    {
        if (data == null)
        {
            throw new ArgumentNullException(nameof(data));
        }

        ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

        using MemoryStream memoryStream = new MemoryStream();
        using CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Write);
        cryptoStream.Write(data);
        cryptoStream.FlushFinalBlock();

        return memoryStream.ToArray();
    }

    public byte[] Encrypt(byte[] data)
    {
        if (data == null)
        {
            throw new ArgumentNullException(nameof(data));
        }

        ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

        using MemoryStream memoryStream = new();
        using CryptoStream cryptoStream = new(memoryStream, encryptor, CryptoStreamMode.Write);
        using (StreamWriter streamWriter = new(cryptoStream))
        {
            streamWriter.Write(data);
        }
        return memoryStream.ToArray();
    }

    private static void AssertKeyLength(int length)
    {
        if (length != 32)
        {
            throw new ArgumentException("An length of AES key must be 32");
        }
    }

    private static void AssertInitializeVectorLength(int length)
    {
        if (length != 16)
        {
            throw new ArgumentException("An length of initialize vector must be 16");
        }
    }
}