namespace Racoon.Core.Packet;

using System;
using System.Security.Cryptography.X509Certificates;
using Racoon.Core.Enums;

public class PacketBase : ISerializable
{
    public int Sequence { get; protected set; }
    public PacketType PacketType { get; protected set; }
    public long TotalLength { get; protected set; }
    public bool IsFragmented { get; protected set; }
    public byte[] Identifier { get; protected set; } = Array.Empty<byte>();
    public short Length { get; protected set; }
    public static short HeaderSize = 32;

    public PacketBase() { }

    public PacketBase(int sequence, IPacket packet, long totalLength, byte[] identifier)
    {
        this.Sequence = sequence;
        this.PacketType = packet.PacketType;
        this.Length = packet.Length;
        this.TotalLength = totalLength;
        this.Identifier = identifier;
        this.IsFragmented = TotalLength == Length;
    }

    public static PacketBase? Deserialize(ReadOnlySpan<byte> bytes, PacketBase packet)
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

public class NormalPacket : IPacket, ISerializable
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
        // int maxLength = buffer.Length - (offset + PacketBase.HeaderSize);
        // int copyLength = Math.Min(Payload.Length, maxLength);
        // Array.Copy(Payload, 0, buffer, offset + PacketBase.HeaderSize, copyLength);
        // return true;
    }
}

public class HandshakePacket : IPacket, ISerializable
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

public class PingPacket : IPacket, ISerializable
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

public class PongPacket : IPacket, ISerializable
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
