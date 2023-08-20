using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Racoon.Tests.Serialize
{
    public class PacketHeaderTests
    {
        [Fact]
        public void PacketHeaderSerializeTest()
        {
            var body = new PingPacket(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
            var packet = new PacketBase(1, body, body.Length, Guid.NewGuid().ToByteArray());
            var buffer = new byte[PacketBase.HeaderSize];
            var result = packet.Serialize(buffer, 0);
            Assert.True(result);
        }
    }
}
