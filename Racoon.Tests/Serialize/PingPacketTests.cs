namespace Racoon.Tests.Serialize;

public class PingPacketTests
{
    [Fact]
    public void PingPacketFitBufferSizeTest()
    {
        var packet = new PingPacket(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
        var buffer = new byte[8];
        var result = packet.Serialize(buffer, 0);
        var deserialized = PingPacket.Deserialize(buffer);

        Assert.Equal(packet.RequestTime, deserialized?.RequestTime);
    }

    [Fact]
    public void PingPacketAbnormalBufferSizeTest()
    {
        var packet = new PingPacket(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
        var buffer = new byte[7];
        var result = packet.Serialize(buffer, 0);
        var deserialized = PingPacket.Deserialize(buffer);

        Assert.False(result);
        Assert.Null(deserialized);
    }

    [Fact]
    public void PingPacketSerializeTest()
    {
        var packet = new PingPacket(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
        var buffer = new byte[100];
        var result = packet.Serialize(buffer, 0);
        var deserialized = PingPacket.Deserialize(buffer);

        Assert.Equal(packet.RequestTime, deserialized?.RequestTime);
    }
}

