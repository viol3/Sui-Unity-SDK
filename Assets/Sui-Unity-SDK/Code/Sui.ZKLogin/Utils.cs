using System;
using System.Linq;
using System.Numerics;

namespace Sui.ZKLogin
{
    /// <summary>
    /// Utility functions used within ZK Login implementation.
    /// TODO: See if we have implemented this in the core Utils class. Does it make sense to move it there? (I think so).
    /// </summary>
    public static class Utils
    {

        /// <summary>
        /// Finds the index of the first non-zero byte in a byte array.
        /// </summary>
        /// <param name="bytes">The byte array to search</param>
        /// <returns>The index of the first non-zero byte, or -1 if all bytes are zero</returns>
        public static int FindFirstNonZeroIndex(byte[] bytes)
        {
            for (int i = 0; i < bytes.Length; i++)
            {
                if (bytes[i] != 0)
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Converts a BigInteger to a byte array padded to specified width.
        /// </summary>
        /// <param name="num">The number to convert</param>
        /// <param name="width">The desired width in bytes</param>
        /// <returns>Byte array padded to specified width</returns>
        public static byte[] ToPaddedBigEndianBytes(this BigInteger num, int width)
        {
            // Convert to hex string and pad
            string hex = num.ToString("X");
            hex = hex.PadLeft(width * 2, '0');

            // Take only the last width*2 characters to match desired byte length
            hex = hex.Substring(Math.Max(0, hex.Length - width * 2));

            return Utilities.Utils.HexStringToByteArray(hex);
        }

        /// <summary>
        /// Converts a BigInteger to a big-endian byte array, removing leading zeros but ensuring at least one byte is returned.
        /// </summary>
        /// <param name="num">The number to convert</param>
        /// <param name="width">The maximum width in bytes</param>
        /// <returns>A byte array representing the number with minimal leading zeros</returns>
        public static byte[] ToBigEndianBytes(this BigInteger num, int width)
        {
            byte[] bytes = ToPaddedBigEndianBytes(num, width);

            int firstNonZeroIndex = FindFirstNonZeroIndex(bytes);

            if (firstNonZeroIndex == -1)
            {
                return new byte[] { 0 };
            }

            return bytes.Skip(firstNonZeroIndex).ToArray();
        }

    }

}