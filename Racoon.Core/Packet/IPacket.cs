using Racoon.Core.Enums;

namespace Racoon.Core.Packet;

public interface IPacket
{
    public short Length { get; }
    public PacketType PacketType { get; }
}