using Racoon.Core.Enums;

namespace Racoon.Core.Packet;

public interface IPacket
{
    public short Length { get; }
    public PacketType PacketType { get; }
}

public interface ISerializable
{
    public bool Serialize(byte[] buffer, int offset);
}