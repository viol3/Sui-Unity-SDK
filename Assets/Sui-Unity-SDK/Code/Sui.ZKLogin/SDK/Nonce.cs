using System;
using System.Numerics;
using System.Security.Cryptography;
using Sui.Cryptography.Ed25519;

namespace Sui.ZKLogin.SDK
{
    public static class NonceGenerator
    {
        public const int NONCE_LENGTH = 27;

        private static BigInteger ToBigIntBE(byte[] bytes)
        {
            if (bytes.Length == 0)
                return BigInteger.Zero;

            // Convert to hex and then to BigInteger
            string hex = BitConverter.ToString(bytes).Replace("-", "");
            return BigInteger.Parse("0" + hex, System.Globalization.NumberStyles.HexNumber);
        }

        public static string GenerateRandomness()
        {
            byte[] randomBytes = new byte[16];
            // IRVIN: See the impact of using this. TypeScript uses `noble/hashes`
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(randomBytes);
            }
            return ToBigIntBE(randomBytes).ToString();
        }

        public static string GenerateNonce(PublicKey publicKey, int maxEpoch, string randomness)
        {
            return GenerateNonce(publicKey, maxEpoch, BigInteger.Parse(randomness));
        }

        public static string GenerateNonce(PublicKey publicKey, int maxEpoch, BigInteger randomness)
        {
            byte[] publicKeyBytes = publicKey.ToSuiBytes();
            BigInteger publicKeyBigInt = ToBigIntBE(publicKeyBytes);

            // Split public key into two 128-bit parts
            BigInteger eph_public_key_0 = publicKeyBigInt >> 128; // IRVIN: Same as publicKeyBytes / 2n ** 128n;
            BigInteger eph_public_key_1 = publicKeyBigInt & ((BigInteger.One << 128) - BigInteger.One); // IRVIN: Same as publicKeyBytes % 2n ** 128n;
            //BigInteger eph_public_key_0 = publicKeyBigInt / BigInteger.Pow(2, 128);
            //BigInteger eph_public_key_1 = publicKeyBigInt % BigInteger.Pow(2, 128);

            BigInteger bigNum = PoseidonHasher.PoseidonHash(new[] {
                eph_public_key_0,
                eph_public_key_1,
                new BigInteger(maxEpoch),
                randomness
            });

            byte[] Z = ToPaddedBigEndianBytes(bigNum, 20);
            string nonce = Base64UrlEncode(Z);

            if (nonce.Length != NONCE_LENGTH)
            {
                throw new Exception($"Length of nonce {nonce} ({nonce.Length}) is not equal to {NONCE_LENGTH}");
            }

            return nonce;
        }

        // Helper function to convert BigInteger to padded big-endian bytes
        private static byte[] ToPaddedBigEndianBytes(BigInteger value, int length)
        {
            byte[] bytes = value.ToByteArray();
            Array.Reverse(bytes); // Convert to big-endian

            if (bytes.Length > length)
            {
                throw new ArgumentException($"Value too large for {length} bytes");
            }

            byte[] paddedBytes = new byte[length];
            Array.Copy(bytes, 0, paddedBytes, length - bytes.Length, bytes.Length);
            return paddedBytes;
        }

        // Base64Url encoding implementation
        private static string Base64UrlEncode(byte[] input)
        {
            string base64 = Convert.ToBase64String(input);
            return base64
                .Replace('+', '-')
                .Replace('/', '_')
                .TrimEnd('=');
        }
    }
}