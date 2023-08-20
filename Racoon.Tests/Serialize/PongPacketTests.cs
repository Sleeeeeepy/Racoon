namespace Racoon.Tests.Serialize;
public class PongPacketTests
{
    // CAUTION:
    // DateTimeOffset and UnixTimeMillisecond differ in precision.
    [Fact]
    public void PongPacketFitBufferSizeTest()
    {
        var packet = new PongPacket(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), RemoteHostStatus.Ready);
        var buffer = new byte[9];
        var result = packet.Serialize(buffer, 0);
        var deserialized = PongPacket.Deserialize(buffer, new PongPacket());

        Assert.Equal(packet.ResponseTime, deserialized?.ResponseTime);
        Assert.Equal(packet.RemoteHostStatus, deserialized?.RemoteHostStatus);
    }

    [Fact]
    public void PongPacketAbnormalBufferSizeTest()
    {
        var packet = new PongPacket(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), RemoteHostStatus.Busy);
        var buffer = new byte[8];
        var result = packet.Serialize(buffer, 0);
        var deserialized = PongPacket.Deserialize(buffer, new PongPacket());

        Assert.False(result);
        Assert.Null(deserialized);
    }

    [Fact]
    public void PongPacketSerializeTest()
    {
        var packet = new PongPacket(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), RemoteHostStatus.Ready);
        var buffer = new byte[1000];
        var result = packet.Serialize(buffer, 0);
        var deserialized = PongPacket.Deserialize(buffer, new PongPacket());

        Assert.Equal(packet.ResponseTime, deserialized?.ResponseTime);
        Assert.Equal(packet.RemoteHostStatus, deserialized?.RemoteHostStatus);
    }
}

