using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Racoon.Core.Packet;

namespace Racoon.Core.Util;

public static class SerializationHelper
{
    public static bool Serialize(byte[] buffer, PacketHeader header, IPacket body)
    {
        if (body is not ISerializable)
        {
            return false;
        }

        int packetLength = header.Length + body.Length;
        if (buffer.Length < packetLength)
        {
            return false;
        }

        bool result = true;
        result &= header.Serialize(buffer, 0);
        result &= ((ISerializable)body).Serialize(buffer, PacketHeader.HeaderSize);

        return result;
    }

    public static bool Deserialize<T>(byte[] buffer, PacketHeader header, T body) where T : IDeserializable<T>
    {
        if (buffer.Length < PacketHeader.HeaderSize)
        {
            return false;
        }

        int startIndex = 0;
        int endIndex = PacketHeader.HeaderSize;

        bool result = true;
        result &= PacketHeader.Deserialize(buffer, header) != null;

        int packetLength = PacketHeader.HeaderSize + header.Length;
        if (buffer.Length < packetLength)
        {
            return false;
        }

        startIndex = endIndex;
        endIndex = startIndex + header.Length;
        result &= T.Deserialize(buffer.AsSpan()[startIndex..endIndex], body) != null;

        return result;
    }
}
