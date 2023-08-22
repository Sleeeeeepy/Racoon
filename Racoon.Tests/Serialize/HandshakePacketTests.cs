using Racoon.Core.Util;

namespace Racoon.Tests.Serialize;

public class HandshakePacketTests
{
    [Fact]
    public void HandshakePacketSerializeTest()
    {
        byte[] publicKey = { 48, 129, 155, 48, 16, 6, 7, 42, 134, 72, 206, 61, 2, 1, 6, 5, 43, 129, 4, 0, 35, 3, 129, 134, 0, 4, 1, 199, 114, 138, 207, 52, 7, 159, 81, 132, 86, 195, 124, 30, 119, 140, 179, 169, 148, 68, 196, 109, 24, 205, 7, 199, 135, 238, 214, 4, 177, 115, 9, 42, 197, 1, 181, 83, 203, 2, 212, 146, 220, 77, 65, 52, 166, 25, 234, 93, 140, 16, 71, 71, 129, 153, 76, 166, 99, 145, 126, 233, 190, 31, 170, 12, 1, 191, 2, 103, 161, 206, 230, 29, 34, 230, 215, 79, 114, 100, 226, 131, 113, 39, 223, 56, 218, 49, 194, 129, 191, 188, 199, 202, 48, 25, 193, 123, 112, 17, 35, 225, 199, 55, 12, 93, 176, 17, 85, 251, 142, 162, 192, 116, 209, 234, 240, 131, 152, 50, 154, 111, 206, 129, 209, 193, 236, 89, 62, 164, 103, 255 };
        byte[] iv = { 131, 123, 239, 185, 222, 109, 240, 185, 164, 3, 148, 67, 175, 178, 155, 126 };
        HandshakePacket expected = new(publicKey, iv);

        byte[] buffer = new byte[512];
        expected.Serialize(buffer, 0);
        HandshakePacket? actual = HandshakePacket.Deserialize(buffer, new HandshakePacket());

        Assert.Equal(expected.KeyLength, actual?.KeyLength);
        Assert.Equal(expected.PublicKey, actual?.PublicKey);
        Assert.Equal(expected.InitializeVectorLength, actual?.InitializeVectorLength);
        Assert.Equal(expected.InitializeVector, actual?.InitializeVector);
    }

    [Fact]
    public void HandshakePacketAbnormalBufferSizeTest()
    {
        byte[] publicKey = { 48, 129, 155, 48, 16, 6, 7, 42, 134, 72, 206, 61, 2, 1, 6, 5, 43, 129, 4, 0, 35, 3, 129, 134, 0, 4, 1, 199, 114, 138, 207, 52, 7, 159, 81, 132, 86, 195, 124, 30, 119, 140, 179, 169, 148, 68, 196, 109, 24, 205, 7, 199, 135, 238, 214, 4, 177, 115, 9, 42, 197, 1, 181, 83, 203, 2, 212, 146, 220, 77, 65, 52, 166, 25, 234, 93, 140, 16, 71, 71, 129, 153, 76, 166, 99, 145, 126, 233, 190, 31, 170, 12, 1, 191, 2, 103, 161, 206, 230, 29, 34, 230, 215, 79, 114, 100, 226, 131, 113, 39, 223, 56, 218, 49, 194, 129, 191, 188, 199, 202, 48, 25, 193, 123, 112, 17, 35, 225, 199, 55, 12, 93, 176, 17, 85, 251, 142, 162, 192, 116, 209, 234, 240, 131, 152, 50, 154, 111, 206, 129, 209, 193, 236, 89, 62, 164, 103, 255 };
        byte[] iv = { 131, 123, 239, 185, 222, 109, 240, 185, 164, 3, 148, 67, 175, 178, 155, 126 };
        HandshakePacket original = new(publicKey, iv);
        byte[] buffer = new byte[publicKey.Length + iv.Length + 3];

        var result = original.Serialize(buffer, 0);
        HandshakePacket? actual = HandshakePacket.Deserialize(buffer, new HandshakePacket());

        Assert.False(result);
        Assert.Null(actual);
    }

    [Fact]
    public void HandshakePacketFitBufferSizeTest()
    {
        byte[] publicKey = { 48, 129, 155, 48, 16, 6, 7, 42, 134, 72, 206, 61, 2, 1, 6, 5, 43, 129, 4, 0, 35, 3, 129, 134, 0, 4, 1, 199, 114, 138, 207, 52, 7, 159, 81, 132, 86, 195, 124, 30, 119, 140, 179, 169, 148, 68, 196, 109, 24, 205, 7, 199, 135, 238, 214, 4, 177, 115, 9, 42, 197, 1, 181, 83, 203, 2, 212, 146, 220, 77, 65, 52, 166, 25, 234, 93, 140, 16, 71, 71, 129, 153, 76, 166, 99, 145, 126, 233, 190, 31, 170, 12, 1, 191, 2, 103, 161, 206, 230, 29, 34, 230, 215, 79, 114, 100, 226, 131, 113, 39, 223, 56, 218, 49, 194, 129, 191, 188, 199, 202, 48, 25, 193, 123, 112, 17, 35, 225, 199, 55, 12, 93, 176, 17, 85, 251, 142, 162, 192, 116, 209, 234, 240, 131, 152, 50, 154, 111, 206, 129, 209, 193, 236, 89, 62, 164, 103, 255 };
        byte[] iv = { 131, 123, 239, 185, 222, 109, 240, 185, 164, 3, 148, 67, 175, 178, 155, 126 };
        HandshakePacket expected = new(publicKey, iv);
        byte[] buffer = new byte[publicKey.Length + iv.Length + 4];

        var result = expected.Serialize(buffer, 0);
        HandshakePacket? actual = HandshakePacket.Deserialize(buffer, new HandshakePacket());

        Assert.Equal(expected.KeyLength, actual?.KeyLength);
        Assert.Equal(expected.PublicKey, actual?.PublicKey);
        Assert.Equal(expected.InitializeVectorLength, actual?.InitializeVectorLength);
        Assert.Equal(expected.InitializeVector, actual?.InitializeVector);
    }

    [Fact]
    public void HandshakePacketMaliciousKeyLengthTest()
    {
        byte[] publicKey = { 48, 129, 155, 48, 16, 6, 7, 42, 134, 72, 206, 61, 2, 1, 6, 5, 43, 129, 4, 0, 35, 3, 129, 134, 0, 4, 1, 199, 114, 138, 207, 52, 7, 159, 81, 132, 86, 195, 124, 30, 119, 140, 179, 169, 148, 68, 196, 109, 24, 205, 7, 199, 135, 238, 214, 4, 177, 115, 9, 42, 197, 1, 181, 83, 203, 2, 212, 146, 220, 77, 65, 52, 166, 25, 234, 93, 140, 16, 71, 71, 129, 153, 76, 166, 99, 145, 126, 233, 190, 31, 170, 12, 1, 191, 2, 103, 161, 206, 230, 29, 34, 230, 215, 79, 114, 100, 226, 131, 113, 39, 223, 56, 218, 49, 194, 129, 191, 188, 199, 202, 48, 25, 193, 123, 112, 17, 35, 225, 199, 55, 12, 93, 176, 17, 85, 251, 142, 162, 192, 116, 209, 234, 240, 131, 152, 50, 154, 111, 206, 129, 209, 193, 236, 89, 62, 164, 103, 255 };
        byte[] iv = { 131, 123, 239, 185, 222, 109, 240, 185, 164, 3, 148, 67, 175, 178, 155, 126 };
        HandshakePacket packet = new(publicKey, iv);
        byte[] buffer = new byte[4096];

        var result = packet.Serialize(buffer, 0);
        // Malicious user attacks
        buffer[0] = 0xFF;
        buffer[1] = 0x0F;
        HandshakePacket? actual = HandshakePacket.Deserialize(buffer, new HandshakePacket());

        Assert.Null(actual);
    }

    [Fact]
    public void HandshakePacketSerializeWithHeaderTest()
    {
        byte[] publicKey = { 48, 129, 155, 48, 16, 6, 7, 42, 134, 72, 206, 61, 2, 1, 6, 5, 43, 129, 4, 0, 35, 3, 129, 134, 0, 4, 1, 199, 114, 138, 207, 52, 7, 159, 81, 132, 86, 195, 124, 30, 119, 140, 179, 169, 148, 68, 196, 109, 24, 205, 7, 199, 135, 238, 214, 4, 177, 115, 9, 42, 197, 1, 181, 83, 203, 2, 212, 146, 220, 77, 65, 52, 166, 25, 234, 93, 140, 16, 71, 71, 129, 153, 76, 166, 99, 145, 126, 233, 190, 31, 170, 12, 1, 191, 2, 103, 161, 206, 230, 29, 34, 230, 215, 79, 114, 100, 226, 131, 113, 39, 223, 56, 218, 49, 194, 129, 191, 188, 199, 202, 48, 25, 193, 123, 112, 17, 35, 225, 199, 55, 12, 93, 176, 17, 85, 251, 142, 162, 192, 116, 209, 234, 240, 131, 152, 50, 154, 111, 206, 129, 209, 193, 236, 89, 62, 164, 103, 255 };
        byte[] iv = { 131, 123, 239, 185, 222, 109, 240, 185, 164, 3, 148, 67, 175, 178, 155, 126 };
        var body = new HandshakePacket(publicKey, iv);
        var header = new PacketHeader(0, body, body.Length, Guid.NewGuid().ToByteArray());

        var packetLength = header.Length + body.Length;
        var buffer = new byte[packetLength];

        SerializationHelper.Serialize(buffer, header, body);

        var deserializedHeader = new PacketHeader();
        var deserializedBody = new HandshakePacket();
        SerializationHelper.Deserialize(buffer, deserializedHeader, deserializedBody);

        Assert.Equal(header.Sequence, deserializedHeader?.Sequence);
        Assert.Equal(header.Length, deserializedHeader?.Length);
        Assert.Equal(header.TotalLength, deserializedHeader?.TotalLength);
        Assert.Equal(header.IsFragmented, deserializedHeader?.IsFragmented);
        Assert.Equal(header.Identifier, deserializedHeader?.Identifier);
        Assert.Equal(header.PacketType, deserializedHeader?.PacketType);

        Assert.Equal(body.KeyLength, deserializedBody?.KeyLength);
        Assert.Equal(body.InitializeVectorLength, deserializedBody?.InitializeVectorLength);
        Assert.Equal(body.PublicKey, deserializedBody?.PublicKey);
        Assert.Equal(body.InitializeVector, deserializedBody?.InitializeVector);

        Assert.Equal(publicKey, deserializedBody?.PublicKey);
        Assert.Equal(iv, deserializedBody?.InitializeVector);
    }
}