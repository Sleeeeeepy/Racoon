using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Racoon.Core.Packet;

namespace Racoon.Core.Net
{
    public class DefaultPacketHandler : IPacketHandler
    {
        public void HandlePacket(ConnectionContext context, PacketBase header, IPacket body)
        {
            return;
        }
    }
}
