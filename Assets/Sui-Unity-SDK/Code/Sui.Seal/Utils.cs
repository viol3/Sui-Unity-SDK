using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Sui.Seal
{
    public static class Utils
    {
        // export const MAX_U8 = 255;
        public const int MAX_U8 = 255;

        // export function xor(a: Uint8Array, b: Uint8Array): Uint8Array
        public static byte[] Xor(byte[] a, byte[] b)
        {
            if (a.Length != b.Length)
            {
                throw new ArgumentException("Input arrays must have the same length.");
            }
            byte[] result = new byte[a.Length];
            for (int i = 0; i < a.Length; i++)
            {
                result[i] = (byte)(a[i] ^ b[i]);
            }
            return result;
        }

        // export function flatten(arrays: Uint8Array[]): Uint8Array
        public static byte[] Flatten(params byte[][] arrays)
        {
            int length = 0;
            foreach (var arr in arrays)
            {
                length += arr.Length;
            }
            var result = new byte[length];
            int offset = 0;
            foreach (var array in arrays)
            {
                Buffer.BlockCopy(array, 0, result, offset, array.Length);
                offset += array.Length;
            }
            return result;
        }

        // Bu, TypeScript'teki '@mysten/bcs' kütüphanesinden gelen 'fromHex' fonksiyonunun karşılığıdır.
        public static byte[] FromHex(string hex)
        {
            hex = hex.StartsWith("0x") ? hex.Substring(2) : hex;
            if (hex.Length % 2 != 0)
            {
                throw new FormatException("The hex string must have an even number of characters.");
            }

            byte[] bytes = new byte[hex.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                string currentHex = hex.Substring(i * 2, 2);
                bytes[i] = byte.Parse(currentHex, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            }
            return bytes;
        }

        // Bu da 'toHex' fonksiyonunun karşılığı.
        public static string ToHex(byte[] bytes)
        {
            return BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
        }

        // export function equals(a: Uint8Array, b: Uint8Array): boolean
        public static bool AreEqual(byte[] a, byte[] b)
        {
            // Linq'daki SequenceEqual, bu işi en performanslı şekilde yapar.
            return a.SequenceEqual(b);
        }

        public static byte[] CreatePolicyId(string suiAddress, byte[] nonce)
        {
            // const creatorBytes = fromHex(suiAddress.replace(/^0x/, ""));
            string addressHex = suiAddress.StartsWith("0x") ? suiAddress.Substring(2) : suiAddress;
            byte[] creatorBytes = FromHex(addressHex);

            // const policyId = new Uint8Array([...creatorBytes, ...nonce]);
            // Bu, iki diziyi birleştirmekle aynıdır. Flatten metodumuzu kullanıyoruz.
            byte[] policyId = Flatten(creatorBytes, nonce);

            return policyId;
        }

        public static byte[] GenerateNonce(int length = 32)
        {
            // Belirtilen uzunlukta boş bir byte dizisi oluştur.
            var nonce = new byte[length];

            // 1. Kriptografik olarak güvenli bir rastgele sayı üreteci nesnesi oluştur.
            using (var rng = RandomNumberGenerator.Create())
            {
                // 2. Bu nesnenin 'GetBytes' metodunu kullanarak diziyi doldur.
                rng.GetBytes(nonce);
            }

            return nonce;
        }
    }

    // export class Version
    public class Version
    {
        public int Major { get; }
        public int Minor { get; }
        public int Patch { get; }

        public Version(string version)
        {
            var parts = version.Split('.');
            if (parts.Length != 3 ||
                !int.TryParse(parts[0], out var major) ||
                !int.TryParse(parts[1], out var minor) ||
                !int.TryParse(parts[2], out var patch))
            {
                throw new ArgumentException($"Invalid version format: {version}");
            }
            Major = major;
            Minor = minor;
            Patch = patch;
        }

        // older_than(other: Version): boolean
        public bool IsOlderThan(Version other)
        {
            if (this.Major != other.Major)
            {
                return this.Major < other.Major;
            }
            if (this.Minor != other.Minor)
            {
                return this.Minor < other.Minor;
            }
            return this.Patch < other.Patch;
        }
    }
}