namespace Racoon.Core.Packet;

using System;
using Racoon.Core.Enums;

public class PacketHeader : ISerializable, IDeserializable<PacketHeader>
{
    public int Sequence { get; protected set; }
    public PacketType PacketType { get; protected set; }
    public long TotalLength { get; protected set; }
    public bool IsFragmented { get; protected set; }
    public byte[] Identifier { get; protected set; } = Array.Empty<byte>();
    public short Length { get; protected set; }
    public static short HeaderSize = 32;

    public PacketHeader() { }

    public PacketHeader(int sequence, IPacket packet, long totalLength, byte[] identifier)
    {
        this.Sequence = sequence;
        this.PacketType = packet.PacketType;
        this.Length = packet.Length;
        this.TotalLength = totalLength;
        this.Identifier = identifier;
        this.IsFragmented = TotalLength == Length;
    }

    public PacketHeader(int sequence, PacketType packetType, long totalLength, bool isFragmented, byte[] identifier, short length)
    {
        Sequence = sequence;
        PacketType = packetType;
        TotalLength = totalLength;
        IsFragmented = isFragmented;
        Identifier = identifier;
        Length = length;
    }

    public static PacketHeader? Deserialize(ReadOnlySpan<byte> bytes, PacketHeader packet)
    {
        try
        {
            int startIndex = 0;
            int endIndex = startIndex + 4;
            packet.Sequence = BitConverter.ToInt32(bytes[startIndex..endIndex]);

            startIndex = endIndex;
            endIndex = startIndex + 1;
            packet.PacketType = (PacketType)bytes[startIndex];

            startIndex = endIndex;
            endIndex = startIndex + 8;
            packet.TotalLength = BitConverter.ToInt64(bytes[startIndex..endIndex]);

            startIndex = endIndex;
            endIndex = startIndex + 1;
            packet.IsFragmented = BitConverter.ToBoolean(bytes[startIndex..endIndex]);

            startIndex = endIndex;
            endIndex = startIndex + 16;
            packet.Identifier = bytes[startIndex..endIndex].ToArray();

            startIndex = endIndex;
            endIndex = startIndex + 2;
            packet.Length = BitConverter.ToInt16(bytes[startIndex..endIndex]);

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
            var result = true;
            var span = buffer.AsSpan(offset);
            int startIndex = 0;
            result &= BitConverter.TryWriteBytes(span.Slice(startIndex, 4), this.Sequence);

            startIndex += 4;
            buffer[startIndex] = (byte)this.PacketType;

            startIndex += 1;
            result &= BitConverter.TryWriteBytes(span.Slice(startIndex, 8), this.TotalLength);

            startIndex += 8;
            result &= BitConverter.TryWriteBytes(span.Slice(startIndex, 1), this.IsFragmented);

            startIndex += 1;
            result &= this.Identifier.AsSpan().TryCopyTo(span.Slice(startIndex, 16));

            startIndex += 16;
            result &= BitConverter.TryWriteBytes(span.Slice(startIndex, 2), this.Length);

            return result;
        }
        catch (ArgumentOutOfRangeException)
        {
            return false;
        }
        catch (IndexOutOfRangeException)
        {
            return false;
        }
    }
}
