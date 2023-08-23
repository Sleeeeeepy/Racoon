using System.Security.Cryptography;

namespace Racoon.Tests.Serialize;

public class NormalPacketTests
{
    private static RandomNumberGenerator random = RandomNumberGenerator.Create();

    [Fact]
    public void NormalPacketSerializeTest()
    {
        var randomBytes = new byte[512];
        random.GetBytes(randomBytes);

        var packet = new NormalPacket(randomBytes);
        var buffer = new byte[512];
        packet.Serialize(buffer, 0);

        Assert.Equal(packet.Payload, buffer);
    }
}

