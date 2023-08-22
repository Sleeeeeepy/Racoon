﻿using Racoon.Core.Packet;

namespace Racoon.Core.Net
{
    public interface IPacketHandler
    {
        public void HandlePacket(ConnectionContext context, PacketBase header, IPacket body);
        public static IPacketHandler GetDefaultPacketHandler()
        {
            return new DefaultPacketHandler();
        }
    }
}