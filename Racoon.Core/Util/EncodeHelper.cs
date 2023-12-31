﻿using Racoon.Core.Correction;
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

    public static int GetBlockSize(PacketHeader header)
    {
        return PacketHeader.HeaderSize + GetBlockSize(header.Length);
    }

    public static void EncodeWithoutHeader(byte[] buffer, PacketHeader header)
    {
        BlockEncoder.Encode(buffer.AsSpan().Slice(PacketHeader.HeaderSize, header.Length), buffer.AsSpan()[PacketHeader.HeaderSize..]);
    }

    public static void DecodeWithoutHeader(byte[] buffer, PacketHeader header)
    {
        BlockEncoder.Decode(buffer.AsSpan().Slice(PacketHeader.HeaderSize, header.Length), buffer.AsSpan()[PacketHeader.HeaderSize..]);
    }
}
