using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Sui.Cryptography.Ed25519;

namespace Sui.ZKLogin.SDK
{
    public static class Utils
    {
        private const int MAX_KEY_CLAIM_NAME_LENGTH = 32;
        private const int MAX_KEY_CLAIM_VALUE_LENGTH = 115;
        private const int MAX_AUD_VALUE_LENGTH = 145;
        private const int PACK_WIDTH = 248;

        // Cache for common calculations
        private static readonly Dictionary<int, int> _chunkSizeCache = new Dictionary<int, int>();

        /// <summary>
        /// TODO: Note: This method would depend on your PublicKey implementation
        /// Converts a public key to its extended ephemeral form in Sui format.
        /// </summary>
        /// <param name="publicKey">The public key to convert.</param>
        /// <returns>The public key in Sui format as a string.</returns>
        public static string GetExtendedEphemeralPublicKey(PublicKey publicKey)
        {
            return publicKey.ToSuiPublicKey();
        }

        /// <summary>
        /// Splits an array into chunks of size chunkSize. If the array is not evenly
        /// divisible by chunkSize, the first chunk will be smaller than chunkSize.
        /// </summary>
        public static T[][] ChunkArray<T>(T[] array, int chunkSize)
        {
            var reversed = array.Reverse().ToArray();
            int chunksCount = (int)Math.Ceiling(array.Length / (double)chunkSize);
            var chunks = new T[chunksCount][];

            for (int i = 0; i < chunksCount; i++)
            {
                chunks[i] = reversed.Skip(i * chunkSize)
                                  .Take(chunkSize)
                                  .Reverse()
                                  .ToArray();
            }

            Array.Reverse(chunks);
            return chunks;
        }

        private static BigInteger BytesBEToBigInt(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
                return BigInteger.Zero;

            string hex = BitConverter.ToString(bytes).Replace("-", "");
            return BigInteger.Parse("0" + hex, System.Globalization.NumberStyles.HexNumber);
        }

        /// <summary>
        /// Hashes an ASCII string to a field element
        /// TODO: This is the original TypeScript implementation. Need to benchmark.
        /// </summary>
        //public static BigInteger HashASCIIStrToField(string str, int maxSize)
        //{
        //    if (string.IsNullOrEmpty(str))
        //    {
        //        throw new ArgumentNullException(nameof(str));
        //    }

        //    if (str.Length > maxSize)
        //    {
        //        throw new ArgumentException($"String {str} is longer than {maxSize} chars");
        //    }

        //    // Pad the string with null characters
        //    // NOTE in the TypeScript implementation they pad it with `zeroes`
        //    var strPadded = str.PadRight(maxSize, '\0')
        //                      .Select(c => (byte)c)
        //                      .ToArray();

        //    int chunkSize = PACK_WIDTH / 8;
        //    var packed = ChunkArray(strPadded, chunkSize)
        //        .Select(chunk => BytesBEToBigInt(chunk))
        //        .ToArray();

        //    return PoseidonHasher.PoseidonHash(packed);
        //}

        public static BigInteger HashASCIIStrToField(string str, int maxSize)
        {
            if (string.IsNullOrEmpty(str))
            {
                throw new ArgumentNullException(nameof(str));
            }

            if (str.Length > maxSize)
            {
                throw new ArgumentException($"String {str} is longer than {maxSize} chars");
            }

            // Convert and pad in a single operation
            byte[] strPadded = new byte[maxSize];
            for (int i = 0; i < str.Length; i++)
            {
                strPadded[i] = (byte)str[i];
            }
            // Rest of array is already zeroed by default

            int chunkSize = PACK_WIDTH / 8;

            // Use cached chunk count if available
            if (!_chunkSizeCache.TryGetValue(maxSize, out int numChunks))
            {
                numChunks = (int)Math.Ceiling(maxSize / (double)chunkSize);
                _chunkSizeCache[maxSize] = numChunks;
            }

            var packed = new BigInteger[numChunks];
            for (int i = 0; i < numChunks; i++)
            {
                int start = maxSize - (i + 1) * chunkSize;
                int length = Math.Min(chunkSize, maxSize - i * chunkSize);
                if (start < 0)
                {
                    start = 0;
                    length = maxSize - i * chunkSize;
                }
                byte[] chunk = new byte[length];
                Array.Copy(strPadded, start, chunk, 0, length);
                packed[numChunks - 1 - i] = BytesBEToBigInt(chunk);
            }

            return PoseidonHasher.PoseidonHash(packed);
        }

        public static BigInteger GenAddressSeed(
            string salt,
            string name,
            string value,
            string aud,
            int maxNameLength = MAX_KEY_CLAIM_NAME_LENGTH,
            int maxValueLength = MAX_KEY_CLAIM_VALUE_LENGTH,
            int maxAudLength = MAX_AUD_VALUE_LENGTH)
        {
            var saltBigInt = BigInteger.Parse(salt);

            return PoseidonHasher.PoseidonHash(new[]
            {
                HashASCIIStrToField(name, maxNameLength),
                HashASCIIStrToField(value, maxValueLength),
                HashASCIIStrToField(aud, maxAudLength),
                PoseidonHasher.PoseidonHash(new[] { saltBigInt })
            });
        }

        // Overload for when salt is already a BigInteger
        public static BigInteger GenAddressSeed(
            BigInteger salt,
            string name,
            string value,
            string aud,
            int maxNameLength = MAX_KEY_CLAIM_NAME_LENGTH,
            int maxValueLength = MAX_KEY_CLAIM_VALUE_LENGTH,
            int maxAudLength = MAX_AUD_VALUE_LENGTH)
        {
            return PoseidonHasher.PoseidonHash(new[]
            {
                HashASCIIStrToField(name, maxNameLength),
                HashASCIIStrToField(value, maxValueLength),
                HashASCIIStrToField(aud, maxAudLength),
                PoseidonHasher.PoseidonHash(new[] { salt })
            });
        }

        private static readonly char[] HexChars = "0123456789abcdef".ToCharArray();

        /// <summary>
        /// Convert byte array to hex string.
        /// Implementation of bytesToHex from '@noble/hashes/utils';
        /// Example bytesToHex(Uint8Array.from([0xca, 0xfe, 0x01, 0x23])) // 'cafe0123'
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        /// TODO Incorrect implementation
        //public static string BytesToHex(byte[] bytes)
        //{
        //    char[] hex = new char[bytes.Length * 2 + 2];
        //    hex[0] = '0';
        //    hex[1] = 'x';

        //    for (int i = 0; i < bytes.Length; i++)
        //    {
        //        hex[i * 2 + 2] = HexChars[bytes[i] >> 4];
        //        hex[i * 2 + 3] = HexChars[bytes[i] & 0xF];
        //    }
        //    return new string(hex);
        //}

        /// <summary>
        /// Converts a byte array to a hexadecimal string using a pre-computed lookup table.
        /// </summary>
        /// <param name="bytes">The byte array to convert.</param>
        /// <returns>The hexadecimal representation of the byte array.</returns>
        /// <example>
        /// byte[] data = new byte[] { 0xca, 0xfe, 0x01, 0x23 };
        /// string hex = ByteUtils.BytesToHex(data); // Returns "cafe0123"
        /// </example>
        public static string BytesToHex(byte[] bytes)
        {
            return BitConverter.ToString(bytes).Replace("-", "").ToLower();
        }
    }
}