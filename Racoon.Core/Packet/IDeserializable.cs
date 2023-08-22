namespace Racoon.Core.Packet;

public interface IDeserializable<T>
{
    public static abstract T? Deserialize(ReadOnlySpan<byte> buffer, T packet); 
}