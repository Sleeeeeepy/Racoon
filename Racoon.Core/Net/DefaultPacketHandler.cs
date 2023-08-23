using Racoon.Core.Packet;

namespace Racoon.Core.Net
{
    public class DefaultPacketHandler : IPacketHandler
    {
        public void HandlePacket(ConnectionContext context, PacketHeader header, IPacket body)
        {
            return;
        }
    }
}
