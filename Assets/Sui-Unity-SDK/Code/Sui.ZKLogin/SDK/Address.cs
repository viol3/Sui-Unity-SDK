using System;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using OpenDive.Utils.Jwt;

namespace Sui.ZKLogin.SDK
{
    /// <summary>
    /// A utility class used to compute A Sui address from the:
    /// UserSalt, and JWT token values (ClaimName, ClaimValue, Aud, ISS).
    /// TODO: See how this is implemented / used in ZK Login TS. Can the SDK and outer accout `address` class be reconciled?
    /// </summary>
    public static class Address
    {
        public const int MAX_HEADER_LEN_B64 = 248;
        public const int MAX_PADDED_UNSIGNED_JWT_LEN = 64 * 25;

        private static readonly char[] HexChars = "0123456789abcdef".ToCharArray();

        private static string BytesToHex(byte[] bytes)
        {
            char[] hex = new char[bytes.Length * 2 + 2];
            hex[0] = '0';
            hex[1] = 'x';

            for (int i = 0; i < bytes.Length; i++)
            {
                hex[i * 2 + 2] = HexChars[bytes[i] >> 4];
                hex[i * 2 + 3] = HexChars[bytes[i] & 0xF];
            }
            return new string(hex);
        }

        public static void LengthChecks(string jwt)
        {
            var parts = jwt.Split(".");
            string header = parts[0];

            if (header.Length > MAX_HEADER_LEN_B64)
                throw new ArgumentException("Header is too long");

            int L = (header.Length + 1 + parts[1].Length) * 8;
            int K = (512 + 448 - ((L % 512) + 1)) % 512;
            int paddedUnsignedJwtLen = (L + 1 + K + 64) / 8;

            if (paddedUnsignedJwtLen > MAX_PADDED_UNSIGNED_JWT_LEN)
                throw new ArgumentException("JWT is too long");
        }

        public static string JwtToAddress(string jwt, string userSalt)
        {
            LengthChecks(jwt);

            JWT decodedJWT = JWTDecoder.DecodeJWT(jwt);
            JWTPayload payload = decodedJWT.Payload;

            if (string.IsNullOrEmpty(payload.Sub) || string.IsNullOrEmpty(payload.Iss) || string.IsNullOrEmpty(payload.Aud))
                throw new ArgumentException("Missing jwt data");

            // Check if Aud is an array by checking if it contains a comma
            // This is a simple way to detect multiple audience values
            if (payload.Aud.Contains(","))
                throw new ArgumentException("Not supported aud. Aud is an array, string was expected.");

            return ComputeZkLoginAddress(new ZkLoginAddressOptions
            {
                UserSalt = userSalt,
                ClaimName = "sub",
                ClaimValue = payload.Sub,
                Aud = payload.Aud,
                Iss = payload.Iss
            });
        }

        public static string ComputeZkLoginAddress(ZkLoginAddressOptions options)
        {
            var seed = GenAddressSeed(options.UserSalt, options.ClaimName, options.ClaimValue, options.Aud);
            return ComputeZkLoginAddressFromSeed(seed, options.Iss);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userSalt"></param>
        /// <param name="claimName"></param>
        /// <param name="claimValue"></param>
        /// <param name="aud"></param>
        /// <returns></returns>
        private static BigInteger GenAddressSeed(string userSalt, string claimName, string claimValue, string aud)
        {
            using var sha256 = SHA256.Create();
            var saltBytes = Encoding.UTF8.GetBytes(userSalt);
            var claimBytes = Encoding.UTF8.GetBytes($"{claimName}:{claimValue}:{aud}");

            var combined = new byte[saltBytes.Length + claimBytes.Length];
            Buffer.BlockCopy(saltBytes, 0, combined, 0, saltBytes.Length);
            Buffer.BlockCopy(claimBytes, 0, combined, saltBytes.Length, claimBytes.Length);

            var hash = sha256.ComputeHash(combined);
            return new BigInteger(hash);
        }

        private static string ComputeZkLoginAddressFromSeed(BigInteger seed, string iss)
        {
            using var sha256 = SHA256.Create();
            var issBytes = Encoding.UTF8.GetBytes(iss);
            var seedBytes = seed.ToByteArray();

            var combined = new byte[seedBytes.Length + issBytes.Length];
            Buffer.BlockCopy(seedBytes, 0, combined, 0, seedBytes.Length);
            Buffer.BlockCopy(issBytes, 0, combined, seedBytes.Length, issBytes.Length);

            var hash = sha256.ComputeHash(combined);
            return BytesToHex(hash);
        }
    }

    public class ZkLoginAddressOptions
    {
        public string ClaimName { get; set; }
        public string ClaimValue { get; set; }
        public string UserSalt { get; set; }
        public string Iss { get; set; }
        public string Aud { get; set; }
    }
}