using System;
using System.Numerics;
using System.Security.Cryptography;
using Sui.Cryptography.Ed25519;

namespace Sui.ZKLogin.SDK
{
    /// <summary>
    /// TODO: See if there are any issues with using RNGCryptoServiceProvider on mobile or WebGL
    /// TODO: See how TS implements this. Perhaps we can use a difference source of randomness.
    /// </summary>
    public static class NonceGenerator
    {
        public const int NONCE_LENGTH = 27;

        /// <summary>
        /// Converts a byte array into a BigInteger value,
        /// interpreting the bytes in big-endian order.
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static BigInteger ToBigIntBE(byte[] bytes)
        {
            if (bytes.Length == 0 || bytes == null)
                return BigInteger.Zero;

            // Convert to hex and then to BigInteger
            string hex = BitConverter.ToString(bytes).Replace("-", "");
            return BigInteger.Parse("0" + hex, System.Globalization.NumberStyles.HexNumber);
        }

        public static string GenerateRandomness()
        {
            byte[] randomBytes = new byte[16];
            // TODO: See the impact of using this. TypeScript uses `noble/hashes`
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(randomBytes);
            }
            return ToBigIntBE(randomBytes).ToString();
        }

        /// <summary>
        ///  An application-defined field embedded in the JWT payload, computed
        ///  as the hash of the ephemeral public key, JWT randomness,
        ///  and the maximum epoch (Sui's defined expiry epoch).
        ///
        /// Specifically, a zkLogin compatible nonce is required to passed in as
        /// <code>
        ///     nonce = ToBase64URL(
        ///         Poseidon_BN254([
        ///             ext_eph_pk_bigint / 2^128,
        ///             ext_eph_pk_bigint % 2^128,
        ///             max_epoch,
        ///             jwt_randomness
        ///         ]).to_bytes()[len - 20..]
        ///     )
        /// </code>
        /// where `ext_eph_pk_bigint` is the BigInt representation of ext_eph_pk.
        /// </summary>
        /// 
        /// <param name="publicKey">
        ///     The byte representation of an ephemeral public key (flag || eph_pk).
        ///     Size varies depending on the choice of the signature scheme
        ///     (denoted by the flag, defined in Signatures).
        /// </param>
        /// <param name="maxEpoch">
        ///     The epoch at which the JWT expires. This is u64 used in Sui, and
        ///     is fetched using the Sui Client.
        ///     Validity period of the ephemeral key pair. e.g. `26`
        /// </param>
        /// <param name="randomness">
        ///     Randomness generated.e.g. `91593735651025872471886891147594672981`
        /// </param>
        /// <returns>
        /// A nonce value computed from the parameter, and encoded as a Base64 string.
        /// e.g. `LSLuhEjHLSeRvyI26wfPQSjYNbc`
        /// </returns>
        public static string GenerateNonce(PublicKey publicKey, int maxEpoch, string randomness)
        {
            return GenerateNonce(publicKey, maxEpoch, BigInteger.Parse(randomness));
        }

        public static string GenerateNonce(PublicKey publicKey, int maxEpoch, BigInteger randomness)
        {
            byte[] publicKeyBytes = publicKey.ToSuiBytes();
            BigInteger publicKeyBigInt = ToBigIntBE(publicKeyBytes);

            // Split public key into two 128-bit parts
            //BigInteger eph_public_key_0 = publicKeyBigInt >> 128; // IRVIN: Same as publicKeyBytes / 2n ** 128n;
            //BigInteger eph_public_key_1 = publicKeyBigInt & ((BigInteger.One << 128) - BigInteger.One); // IRVIN: Same as publicKeyBytes % 2n ** 128n;
            BigInteger eph_public_key_0 = publicKeyBigInt / BigInteger.Pow(2, 128);
            BigInteger eph_public_key_1 = publicKeyBigInt % BigInteger.Pow(2, 128);

            BigInteger bigNum = PoseidonHasher.PoseidonHash(new[] {
                eph_public_key_0,
                eph_public_key_1,
                new BigInteger(maxEpoch),
                randomness
            });

            byte[] Z = ZKLogin.Utils.ToPaddedBigEndianBytes(bigNum, 20);
            string nonce = Base64UrlEncode(Z);

            if (nonce.Length != NONCE_LENGTH)
                throw new Exception($"Length of nonce {nonce} ({nonce.Length}) is not equal to {NONCE_LENGTH}");

            return nonce;
        }

        // Base64Url encoding implementation
        public static string Base64UrlEncode(byte[] input)
        {
            string base64 = Convert.ToBase64String(input);
            return base64
                .Replace('+', '-')
                .Replace('/', '_')
                .TrimEnd('=');
        }
    }
}