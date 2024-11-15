using System;
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

        // Note: This method would depend on your PublicKey implementation
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
            if (bytes.Length == 0)
                return BigInteger.Zero;

            string hex = BitConverter.ToString(bytes).Replace("-", "");
            return BigInteger.Parse("0" + hex, System.Globalization.NumberStyles.HexNumber);
        }

        /// <summary>
        /// Hashes an ASCII string to a field element
        /// </summary>
        public static BigInteger HashASCIIStrToField(string str, int maxSize)
        {
            if (str.Length > maxSize)
            {
                throw new Exception($"String {str} is longer than {maxSize} chars");
            }

            // Pad the string with null characters
            // NOTE in the TypeScript implementation they pad it with `zeroes`
            var strPadded = str.PadRight(maxSize, '\0')
                              .Select(c => (byte)c)
                              .ToArray();

            int chunkSize = PACK_WIDTH / 8;
            var packed = ChunkArray(strPadded, chunkSize)
                .Select(chunk => BytesBEToBigInt(chunk))
                .ToArray();

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
    }
}