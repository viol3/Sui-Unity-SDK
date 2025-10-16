using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using MCL.BLS12_381.Net;
using Org.BouncyCastle.Crypto.Digests;
using Sui.Accounts; // BouncyCastle'dan SHA3 için

namespace Sui.Seal
{
    public static class Kdf
    {
        // const DST: Uint8Array = new TextEncoder().encode('SUI-SEAL-IBE-BLS12381-00');
        public static readonly byte[] DST = Encoding.UTF8.GetBytes("SUI-SEAL-IBE-BLS12381-00");

        // const KDF_DST = new TextEncoder().encode('SUI-SEAL-IBE-BLS12381-H2-00');
        public static readonly byte[] KDF_DST = Encoding.UTF8.GetBytes("SUI-SEAL-IBE-BLS12381-H2-00");

        // const DERIVE_KEY_DST = new TextEncoder().encode('SUI-SEAL-IBE-BLS12381-H3-00');
        public static readonly byte[] DERIVE_KEY_DST = Encoding.UTF8.GetBytes("SUI-SEAL-IBE-BLS12381-H3-00");

        // export function hashToG1(id: Uint8Array): G1Element
        public static G1 HashToG1(byte[] id)
        {
            // return G1Element.hashToCurve(flatten([DST, id]));
            // Utils.Flatten'ı bir sonraki adımda yazacağız.
            byte[] concatenated = Utils.Flatten(DST, id);
            return G1.HashToCurve(concatenated);
        }

        // export function kdf(...)
        public static byte[] KdfBytes(GT element, G2 nonce, byte[] id, AccountAddress objectId, int index)
        {
            // if (index < 0 || index > MAX_U8)
            if (index < 0 || index > 255) // MAX_U8 = 255
            {
                throw new ArgumentOutOfRangeException(nameof(index), $"Invalid index {index}");
            }

            // const hash = sha3_256.create();
            var digest = new Sha3Digest(256);

            // hash.update(KDF_DST);
            digest.BlockUpdate(KDF_DST, 0, KDF_DST.Length);
            // hash.update(element.toBytes());
            var elementBytes = element.ToBytes();
            digest.BlockUpdate(elementBytes, 0, elementBytes.Length);
            // hash.update(nonce.toBytes());
            var nonceBytes = nonce.ToBytes();
            digest.BlockUpdate(nonceBytes, 0, nonceBytes.Length);
            // hash.update(hashToG1(id).toBytes());
            var hashToG1Bytes = HashToG1(id).ToBytes();
            digest.BlockUpdate(hashToG1Bytes, 0, hashToG1Bytes.Length);
            // hash.update(fromHex(objectId));
            var objectIdBytes = objectId.KeyBytes; // Bunu Utils.cs'de yazacağız
            digest.BlockUpdate(objectIdBytes, 0, objectIdBytes.Length);
            // hash.update(new Uint8Array([index]));
            digest.Update((byte)index);

            // return hash.digest();
            var result = new byte[digest.GetDigestSize()];
            digest.DoFinal(result, 0);
            return result;
        }

        // export enum KeyPurpose
        public enum KeyPurpose
        {
            EncryptedRandomness,
            DEM,
        }

        // function tag(purpose: KeyPurpose): Uint8Array
        private static byte[] GetTag(KeyPurpose purpose)
        {
            switch (purpose)
            {
                case KeyPurpose.EncryptedRandomness:
                    return new byte[] { 0 };
                case KeyPurpose.DEM:
                    return new byte[] { 1 };
                default:
                    throw new ArgumentException($"Invalid key purpose {purpose}", nameof(purpose));
            }
        }

        // export function deriveKey(...)
        public static byte[] DeriveKey(
            KeyPurpose purpose,
            byte[] baseKey,
            byte[][] encryptedShares,
            int threshold,
            AccountAddress[] keyServers)
        {
            if (threshold <= 0 || threshold > 255)
            {
                throw new ArgumentOutOfRangeException(nameof(threshold), $"Invalid threshold {threshold}");
            }

            var digest = new Sha3Digest(256);

            digest.BlockUpdate(DERIVE_KEY_DST, 0, DERIVE_KEY_DST.Length);
            digest.BlockUpdate(baseKey, 0, baseKey.Length);

            var tag = GetTag(purpose);
            digest.BlockUpdate(tag, 0, tag.Length);

            digest.Update((byte)threshold);

            foreach (var share in encryptedShares)
            {
                digest.BlockUpdate(share, 0, share.Length);
            }
            foreach (var keyServer in keyServers)
            {
                var keyServerBytes = keyServer.KeyBytes;
                digest.BlockUpdate(keyServerBytes, 0, keyServerBytes.Length);
            }

            var result = new byte[digest.GetDigestSize()];
            digest.DoFinal(result, 0);
            return result;
        }
    }
}