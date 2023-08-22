using ReedSolomon;

namespace Racoon.Core.Correction;

public class BlockEncoder
{
    private BlockEncoder() { }
    internal const int CODEWORD_SIZE = Rs8.BlockLength;
    internal const int BLOCK_SIZE = Rs8.DataLength;

    public static void Encode(ReadOnlySpan<byte> data, Span<byte> result)
    {
        if (data.IsEmpty)
        {
            result.Clear();
        }

        int numberOfBlocks = (data.Length + BLOCK_SIZE - 1) / BLOCK_SIZE;
        for (int i = 0; i < numberOfBlocks - 1; i++)
        {
            ReadOnlySpan<byte> block = data.Slice(i * BLOCK_SIZE, BLOCK_SIZE);
            block.CopyTo(result[(i * CODEWORD_SIZE)..]);
            Rs8.Encode(result[(i * CODEWORD_SIZE)..]);
        }

        ReadOnlySpan<byte> lastBlock = data[((numberOfBlocks - 1) * BLOCK_SIZE)..];
        if (lastBlock.Length > 0)
        {
            lastBlock.CopyTo(result[((numberOfBlocks - 1) * CODEWORD_SIZE)..]);
            Rs8.Encode(result[((numberOfBlocks - 1) * CODEWORD_SIZE)..]);
        }
    }

    public static void Decode(Span<byte> codewords, Span<byte> result)
    {
        int numberOfBlocks = codewords.Length / CODEWORD_SIZE;
        if (numberOfBlocks <= 0 || codewords.Length % CODEWORD_SIZE != 0)
        {
            result.Clear();
            return;
        }

        for (int i = 0; i < numberOfBlocks; i++)
        {
            Span<byte> block = codewords.Slice(i * CODEWORD_SIZE, CODEWORD_SIZE);
            Rs8.Decode(block, Span<int>.Empty);
            block[..BLOCK_SIZE].CopyTo(result[(i * BLOCK_SIZE)..]);
        }
    }

    public static Span<byte> Encode(Span<byte> data)
    {
        if (data.IsEmpty)
        {
            return new byte[CODEWORD_SIZE];
        }
        int numberOfBlocks = (data.Length + BLOCK_SIZE - 1) / BLOCK_SIZE;
        var encodedData = new byte[numberOfBlocks * CODEWORD_SIZE].AsSpan();
        for (int i = 0; i < numberOfBlocks - 1; i++)
        {
            Span<byte> block = data.Slice(i * BLOCK_SIZE, BLOCK_SIZE);
            block.CopyTo(encodedData[(i * CODEWORD_SIZE)..]);
            Rs8.Encode(encodedData[(i * CODEWORD_SIZE)..]);
        }

        Span<byte> lastBlock = data[((numberOfBlocks - 1) * BLOCK_SIZE)..];
        if (lastBlock.Length > 0)
        {
            lastBlock.CopyTo(encodedData[((numberOfBlocks - 1) * CODEWORD_SIZE)..]);
            Rs8.Encode(encodedData[((numberOfBlocks - 1) * CODEWORD_SIZE)..]);
        }

        return encodedData;
    }

    public static Span<byte> Decode(Span<byte> codewords)
    {
        int numberOfBlocks = codewords.Length / CODEWORD_SIZE;
        if (numberOfBlocks <= 0 || codewords.Length % CODEWORD_SIZE != 0)
        {
            return new byte[BLOCK_SIZE];
        }

        var decodedData = new byte[numberOfBlocks * BLOCK_SIZE].AsSpan();
        for (int i = 0; i < numberOfBlocks; i++)
        {
            Span<byte> block = codewords.Slice(i * CODEWORD_SIZE, CODEWORD_SIZE);
            Rs8.Decode(block, Span<int>.Empty);
            block[..BLOCK_SIZE].CopyTo(decodedData[(i * BLOCK_SIZE)..]);
        }

        return decodedData;
    }
}