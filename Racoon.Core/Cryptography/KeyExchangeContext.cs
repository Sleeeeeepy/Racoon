using System.Security.Cryptography;

namespace Racoon.Core.Cryptography;

public class KeyExchangeContext
{
    private readonly byte[] privateKey = Array.Empty<byte>();
    private byte[] sharedKey = Array.Empty<byte>();
    public byte[] PublicKey { get; } = Array.Empty<byte>();
    public byte[] OtherPublicKey { get; private set; } = Array.Empty<byte>();
    public byte[] SharedKey
    {
        get
        {
            if (sharedKey is null || sharedKey.Length == 0)
            {
                throw new InvalidOperationException("shared key does not exist. call ReceiveKey(byte[]) first.");
            }
            return sharedKey;
        }

        private set
        {
            sharedKey = value;
        }
    }

    public KeyExchangeContext()
    {
        using ECDiffieHellman diffieHellman = ECDiffieHellman.Create();
        this.privateKey = diffieHellman.ExportECPrivateKey();
        this.PublicKey = diffieHellman.ExportSubjectPublicKeyInfo();
    }

    public KeyExchangeContext(byte[] privateKey)
    {
        if (privateKey == null || privateKey.Length != 128)
        {
            using ECDiffieHellman diffieHellman = ECDiffieHellman.Create();
            this.privateKey = diffieHellman.ExportECPrivateKey();
            this.PublicKey = diffieHellman.ExportSubjectPublicKeyInfo();
            return;
        }

        this.privateKey = privateKey;
    }

    public void ReceiveKey(byte[] otherPublicKey)
    {
        OtherPublicKey = otherPublicKey;
        using ECDiffieHellman local_private = ECDiffieHellman.Create();
        local_private.ImportECPrivateKey(privateKey, out _);

        using ECDiffieHellman remote_public = ECDiffieHellman.Create();
        remote_public.ImportSubjectPublicKeyInfo(OtherPublicKey, out _);

        SharedKey = local_private.DeriveKeyMaterial(remote_public.PublicKey);
    }
}
