using System;
using System.Numerics;
using System.Text;
using Blake2Fast;
using OpenDive.BCS;
using OpenDive.Utils.Jwt;
using Sui.Cryptography;
using UnityEngine;

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

        //private static readonly char[] HexChars = "0123456789abcdef".ToCharArray();

        //private static string BytesToHex(byte[] bytes)
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
        /// Computes a ZkLogin address from an address seed and issuer.
        /// </summary>
        public static string ComputeZkLoginAddressFromSeed(
            BigInteger seed,
            string iss,
            bool legacyAddress = true
            )
        {
            byte[] addressSeedBytesBigEndian = legacyAddress
                ? Utils.ToBigEndianBytes(seed, 32)
                : Utils.ToPaddedBigEndianBytes(seed, 32);

            if (iss == "accounts.google.com")
            {
                iss = "https://accounts.google.com";
            }
            Debug.Log("ISS: .... " + iss);
            byte[] addressParamBytes = Encoding.UTF8.GetBytes(iss); // WORKS
            Debug.Log(string.Join(", ", addressParamBytes));
            byte[] tmp = new byte[2 + addressSeedBytesBigEndian.Length + addressParamBytes.Length];

            tmp[0] = SignatureSchemeToFlag.ZkLogin;
            tmp[1] = (byte)addressParamBytes.Length;
            Buffer.BlockCopy(addressParamBytes, 0, tmp, 2, addressParamBytes.Length);
            Buffer.BlockCopy(addressSeedBytesBigEndian, 0, tmp, 2 + addressParamBytes.Length, addressSeedBytesBigEndian.Length);

            byte[] hash = Blake2b.ComputeHash(32, tmp);

            // Convert to hex and normalize
            Debug.Log(" BLAKE2HASH LENGTH: " + hash.Length + " HASH: " + hash.ToString());

            Accounts.AccountAddress address = new Accounts.AccountAddress(hash);
            //return address.ToString();
            return address.KeyHex;

            //return Utils.BytesToHex(hash);
            //return Utils.BytesToHex(hash).Substring(0, 32 * 2);
        }

        /// <summary>
        /// Performs length checks on JWT components.
        /// </summary>
        public static void LengthChecks(string jwt)
        {
            string[] parts = jwt.Split('.');
            if (parts.Length < 2)
                throw new ArgumentException("Invalid JWT format");

            string header = parts[0];
            string payload = parts[1];

            if (header.Length > MAX_HEADER_LEN_B64)
                throw new Exception("Header is too long");

            long L = (header.Length + 1 + payload.Length) * 8;
            long K = (512 + 448 - ((L % 512) + 1)) % 512;
            long padded_unsigned_jwt_len = (L + 1 + K + 64) / 8;

            if (padded_unsigned_jwt_len > MAX_PADDED_UNSIGNED_JWT_LEN)
                throw new Exception("JWT is too long");
        }

        // <summary>
        /// Converts a JWT to a ZkLogin address.
        /// </summary>
        public static string JwtToAddress(string jwt, string userSalt, bool legacyAddress = false)
        {
            LengthChecks(jwt);

            JWT decodedJWT = JWTDecoder.DecodeJWT(jwt);

            return JwtToAddress(decodedJWT, userSalt, legacyAddress);
        }

        public static string JwtToAddress(JWT jwt, string userSalt, bool legacyAddress = false)
        {
            JWTPayload payload = jwt.Payload;

            if (string.IsNullOrEmpty(payload.Sub)
                || string.IsNullOrEmpty(payload.Iss)
                || string.IsNullOrEmpty(payload.Aud))
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
                Iss = payload.Iss,
                LegacyAddress = legacyAddress
            });
        }

        public static string ComputeZkLoginAddress(ZkLoginAddressOptions options)
        {
            //var seed = BigInteger.Parse("6320033262267591434792934173917659296377585423834478563244217901081449450162");

            var seed = Utils.GenAddressSeed(
                options.UserSalt,
                options.ClaimName,
                options.ClaimValue,
                options.Aud
            );

            Debug.Log("USER SALT: " + options.UserSalt
                + "\nCLAIM NAME: " + options.ClaimName
                + "\nCLAIM VALUE: " + options.ClaimValue
                + "\nAUD: " + options.Aud);

            Debug.Log("ADDRESS SEED: " + seed);

            return ComputeZkLoginAddressFromSeed(
                seed,
                options.Iss,
                options.LegacyAddress
            );
        }

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="userSalt"></param>
        ///// <param name="claimName"></param>
        ///// <param name="claimValue"></param>
        ///// <param name="aud"></param>
        ///// <returns></returns>
        //private static BigInteger GenAddressSeed(string userSalt, string claimName, string claimValue, string aud)
        //{
        //    using var sha256 = SHA256.Create();
        //    var saltBytes = Encoding.UTF8.GetBytes(userSalt);
        //    var claimBytes = Encoding.UTF8.GetBytes($"{claimName}:{claimValue}:{aud}");

        //    var combined = new byte[saltBytes.Length + claimBytes.Length];
        //    Buffer.BlockCopy(saltBytes, 0, combined, 0, saltBytes.Length);
        //    Buffer.BlockCopy(claimBytes, 0, combined, saltBytes.Length, claimBytes.Length);

        //    var hash = sha256.ComputeHash(combined);
        //    return new BigInteger(hash);
        //}

        /// <summary>
        /// Computes a ZkLogin address from an address seed and issuer.
        /// </summary>
        //private static string ComputeZkLoginAddressFromSeed(BigInteger seed, string iss)
        //{
        //    using var sha256 = SHA256.Create();
        //    var issBytes = Encoding.UTF8.GetBytes(iss);
        //    var seedBytes = seed.ToByteArray();

        //    var combined = new byte[seedBytes.Length + issBytes.Length];
        //    Buffer.BlockCopy(seedBytes, 0, combined, 0, seedBytes.Length);
        //    Buffer.BlockCopy(issBytes, 0, combined, seedBytes.Length, issBytes.Length);

        //    var hash = sha256.ComputeHash(combined);
        //    return BytesToHex(hash);
        //}
    }

    public class ZkLoginAddressOptions
    {
        public string ClaimName { get; set; }
        public string ClaimValue { get; set; }
        public string UserSalt { get; set; }
        public string Iss { get; set; }
        public string Aud { get; set; }
        public bool LegacyAddress { get; set; }
    }
}