using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Racoon.Core.Correction;
using Racoon.Core.Packet;

namespace Racoon.Core.Util;

public class EncodeHelper
{
    public static int GetBlockSize(int size)
    {
        int numBlocks = size / BlockEncoder.BLOCK_SIZE + (size % BlockEncoder.BLOCK_SIZE != 0 ? 1 : 0);
        int fitSize = numBlocks * BlockEncoder.CODEWORD_SIZE;
        return fitSize;
    }

    public static int GetBlockSize(PacketBase header)
    {
        return PacketBase.HeaderSize + GetBlockSize(header.Length);
    }

    public static void EncodeWithoutHeader(byte[] buffer, PacketBase header)
    {
        BlockEncoder.Encode(buffer.AsSpan().Slice(PacketBase.HeaderSize, header.Length), buffer.AsSpan()[PacketBase.HeaderSize..]);
    }

    public static void DecodeWithoutHeader(byte[] buffer, PacketBase header)
    {
        BlockEncoder.Decode(buffer.AsSpan().Slice(PacketBase.HeaderSize, header.Length), buffer.AsSpan()[PacketBase.HeaderSize..]);
    }
}
