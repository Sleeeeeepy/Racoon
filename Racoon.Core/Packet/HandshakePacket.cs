namespace Racoon.Core.Packet;

using System;
using Racoon.Core.Enums;

public class HandshakePacket : IPacket, ISerializable, IDeserializable<HandshakePacket>
{
    public short KeyLength { get; private set; } = 0;
    public byte[] PublicKey { get; private set; } = Array.Empty<byte>();
    public short InitializeVectorLength { get; private set; } = 0;
    public byte[] InitializeVector { get; private set; } = Array.Empty<byte>();
    public short Length => (short)(KeyLength + InitializeVectorLength + 4);
    public PacketType PacketType => PacketType.ConnectionRequest;

    public HandshakePacket() { }

    public HandshakePacket(byte[] publicKey, byte[] initializeVector) : this()
    {
        this.PublicKey = publicKey;
        this.KeyLength = (short)publicKey.Length;
        this.InitializeVector = initializeVector;
        this.InitializeVectorLength = (short)initializeVector.Length;
    }

    public static HandshakePacket? Deserialize(ReadOnlySpan<byte> bytes, HandshakePacket packet)
    {
        try
        {
            int startIndex = 0;
            int endIndex = 2;
            packet.KeyLength = BitConverter.ToInt16(bytes[startIndex..endIndex]);

            startIndex = endIndex;
            endIndex = startIndex + packet.KeyLength;
            packet.PublicKey = bytes[startIndex..endIndex].ToArray();

            startIndex = endIndex;
            endIndex = startIndex + 2;
            packet.InitializeVectorLength = BitConverter.ToInt16(bytes[startIndex..endIndex]);

            startIndex = endIndex;
            endIndex = startIndex + packet.InitializeVectorLength;
            packet.InitializeVector = bytes[startIndex..endIndex].ToArray();

            return packet;
        }
        catch (ArgumentOutOfRangeException)
        {
            return null;
        }
    }

    public bool Serialize(byte[] buffer, int offset)
    {
        try
        {
            var result = true;
            var span = buffer.AsSpan(offset);
            int startIndex = 0;
            result &= BitConverter.TryWriteBytes(span.Slice(startIndex, 2), this.KeyLength);

            startIndex += 2;
            result &= PublicKey.AsSpan().TryCopyTo(span.Slice(startIndex, KeyLength));

            startIndex += KeyLength;
            result &= BitConverter.TryWriteBytes(span.Slice(startIndex, 2), this.InitializeVectorLength);

            startIndex += 2;
            result &= InitializeVector.AsSpan().TryCopyTo(span.Slice(startIndex, InitializeVectorLength));

            return result;
        }
        catch (ArgumentOutOfRangeException)
        {
            return false;
        }
    }
}
