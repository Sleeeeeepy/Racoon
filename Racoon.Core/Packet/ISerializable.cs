namespace Racoon.Core.Packet;

public interface ISerializable
{
    public bool Serialize(byte[] buffer, int offset);
}
