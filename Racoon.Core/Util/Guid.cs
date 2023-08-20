namespace Racoon.Core.Util;

public static class ByteUtil
{
    public static string ToHexString(this byte[] bytes)
    {
        if (bytes == null)
        {
            return "";
        }

        return BitConverter.ToString(bytes).Replace("-", "");
    }
}