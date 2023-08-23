namespace Racoon.Core.Packet;

using System;
using Racoon.Core.Enums;

public class NormalPacket : IPacket, ISerializable, IDeserializable<NormalPacket>
{
    public byte[] Payload { get; private set; } = Array.Empty<byte>();
    public short Length => (short)Payload.Length;
    public PacketType PacketType => PacketType.Normal;

    public NormalPacket() { }

    public NormalPacket(byte[] payload)
    {
        Payload = new byte[payload.Length];
        Array.Copy(payload, Payload, Payload.Length);
    }

    public static NormalPacket Deserialize(ReadOnlySpan<byte> bytes, NormalPacket packet)
    {
        packet.Payload = bytes.ToArray();
        return packet;
    }

    public bool Serialize(byte[] buffer, int offset)
    {
        try
        {
            return Payload.AsSpan().TryCopyTo(buffer.AsSpan(offset));
        }
        catch (ArgumentOutOfRangeException)
        {
            return false;
        }
        catch (IndexOutOfRangeException)
        {
            return false;
        }
        // int maxLength = buffer.Length - (offset + PacketHeader.HeaderSize);
        // int copyLength = Math.Min(Payload.Length, maxLength);
        // Array.Copy(Payload, 0, buffer, offset + PacketHeader.HeaderSize, copyLength);
        // return true;
    }
}
