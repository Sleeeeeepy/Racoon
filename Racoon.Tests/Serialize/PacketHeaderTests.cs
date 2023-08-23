namespace Racoon.Tests.Serialize
{
    public class PacketHeaderTests
    {
        [Fact]
        public void PacketHeaderSerializeTest()
        {
            var body = new PingPacket(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
            var packet = new PacketHeader(1, body, body.Length, Guid.NewGuid().ToByteArray());
            var buffer = new byte[100];
            var result = packet.Serialize(buffer, 0);

            var deserialized = PacketHeader.Deserialize(buffer, new PacketHeader());
            Assert.True(result);
            Assert.Equal(packet.Sequence, deserialized?.Sequence);
            Assert.Equal(packet.PacketType, deserialized?.PacketType);
            Assert.Equal(packet.TotalLength, deserialized?.TotalLength);
            Assert.Equal(packet.Identifier, deserialized?.Identifier);
            Assert.Equal(packet.Length, deserialized?.Length);
        }

        [Fact]
        public void PacketHeaderAbnormalBufferSize()
        {
            var body = new PingPacket(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
            var packet = new PacketHeader(1, body, body.Length, Guid.NewGuid().ToByteArray());
            var buffer = new byte[PacketHeader.HeaderSize + 1];
            var result = packet.Serialize(buffer, 0);

            var deserialized = PacketHeader.Deserialize(buffer, new PacketHeader());
            Assert.True(result);
            Assert.Equal(packet.Sequence, deserialized?.Sequence);
            Assert.Equal(packet.PacketType, deserialized?.PacketType);
            Assert.Equal(packet.TotalLength, deserialized?.TotalLength);
            Assert.Equal(packet.Identifier, deserialized?.Identifier);
            Assert.Equal(packet.Length, deserialized?.Length);
        }

        [Fact]
        public void PingPacketFitBufferSizeTest()
        {
            var body = new PingPacket(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
            var packet = new PacketHeader(1, body, body.Length, Guid.NewGuid().ToByteArray());
            var buffer = new byte[PacketHeader.HeaderSize];
            var result = packet.Serialize(buffer, 0);

            var deserialized = PacketHeader.Deserialize(buffer, new PacketHeader());
            Assert.True(result);
            Assert.Equal(packet.Sequence, deserialized?.Sequence);
            Assert.Equal(packet.PacketType, deserialized?.PacketType);
            Assert.Equal(packet.TotalLength, deserialized?.TotalLength);
            Assert.Equal(packet.Identifier, deserialized?.Identifier);
            Assert.Equal(packet.Length, deserialized?.Length);
        }
    }
}
