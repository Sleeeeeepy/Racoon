namespace Racoon.Core.Cryptography;

public interface ICryptography
{
    byte[] Encrypt(byte[] data);
    byte[] Decrypt(byte[] data);
}