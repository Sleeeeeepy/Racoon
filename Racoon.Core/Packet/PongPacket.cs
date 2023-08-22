namespace Racoon.Core.Packet;

using System;
using Racoon.Core.Enums;

public class PongPacket : IPacket, ISerializable, IDeserializable<PongPacket>
{
    public DateTimeOffset ResponseTime { get; private set; }
    public RemoteHostStatus RemoteHostStatus { get; private set; }
    public short Length => sizeof(long) + 1;
    public PacketType PacketType => PacketType.Pong;

    public PongPacket() { }

    public PongPacket(RemoteHostStatus status) : this()
    {
        ResponseTime = DateTimeOffset.Now;
        this.RemoteHostStatus = status;
    }

    public PongPacket(long responseTime, RemoteHostStatus status) : this(status)
    {
        ResponseTime = DateTimeOffset.FromUnixTimeMilliseconds(responseTime);
    }

    public PongPacket(DateTimeOffset responseTime, RemoteHostStatus status) : this(status)
    {
        ResponseTime = responseTime;
    }

    public static PongPacket? Deserialize(ReadOnlySpan<byte> bytes, PongPacket packet)
    {
        try
        {
            int startIndex = 0;
            packet.ResponseTime = DateTimeOffset.FromUnixTimeMilliseconds(BitConverter.ToInt64(bytes));

            startIndex += 8;
            packet.RemoteHostStatus = (RemoteHostStatus)bytes[startIndex];
            
            return packet;
        } 
        catch (ArgumentOutOfRangeException)
        {
            return null;
        }
        catch (IndexOutOfRangeException)
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
            result &= BitConverter.TryWriteBytes(span.Slice(startIndex, 8), ResponseTime.ToUnixTimeMilliseconds());

            startIndex += 8;
            result &= span.Slice(startIndex, 1).Length > 0;
            span.Slice(startIndex, 1)[0] = (byte)RemoteHostStatus;

            return result;
        } 
        catch (ArgumentOutOfRangeException)
        {
            return false;
        }
    }
}
