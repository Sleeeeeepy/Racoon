namespace Racoon.Core.Packet;

using System;
using Racoon.Core.Enums;

public class PingPacket : IPacket, ISerializable, IDeserializable<PingPacket>
{
    public DateTimeOffset RequestTime { get; private set; }
    public short Length => 8;
    public PacketType PacketType => PacketType.Ping;

    public PingPacket() { }

    public PingPacket(DateTimeOffset requestTime) : this()
    {
        RequestTime = requestTime;
    }

    public PingPacket(long requestTime) : this()
    {
        RequestTime = DateTimeOffset.FromUnixTimeMilliseconds(requestTime);
    }

    public static PingPacket? Deserialize(ReadOnlySpan<byte> bytes, PingPacket packet)
    {
        try
        {
            packet.RequestTime = DateTimeOffset.FromUnixTimeMilliseconds(BitConverter.ToInt64(bytes));
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
            bool result = true;
            var span = buffer.AsSpan(offset);
            var startIndex = 0;
            result &= BitConverter.TryWriteBytes(span.Slice(startIndex, 8), RequestTime.ToUnixTimeMilliseconds());

            return result;
        }
        catch (ArgumentOutOfRangeException)
        {
            return false;
        }
    }
}
