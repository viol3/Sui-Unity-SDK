// Ibe.cs
using MCL.BLS12_381.Net;
using Sui.Seal;
using System;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Sui.Seal
{
    public static class Ibe
    {
        // export const DST_POP: Uint8Array = new TextEncoder().encode('SUI-SEAL-IBE-BLS12381-POP-00');
        public static readonly byte[] DST_POP = Encoding.UTF8.GetBytes("SUI-SEAL-IBE-BLS12381-POP-00");

        // Bu, TypeScript'teki 'BonehFranklinBLS12381Services' sınıfına karşılık geliyor.
        // Şimdilik daha basit olması için statik bir sınıf olarak başlıyoruz.
        public static class BonehFranklin
        {
            public static IBEEncryptions EncryptBatched(
                G2[] publicKeys,
                byte[] id,
                Share[] shares,
                byte[] baseKey,
                int threshold,
                string[] objectIds
            )
            {
                if (publicKeys.Length == 0 || publicKeys.Length != shares.Length)
                {
                    throw new ArgumentException("Invalid public keys");
                }

                // 1. KEM (Key Encapsulation Mechanism) adımı
                // const [r, nonce, keys] = encapBatched(this.publicKeys, id);
                var (r, nonce, keys) = EncapBatched(publicKeys, id);

                // 2. DEM (Data Encapsulation Mechanism) adımı
                // Her bir payı (share) ilgili anahtarla XOR'layarak şifrele.
                var encryptedShares = new byte[shares.Length][];
                for (int i = 0; i < shares.Length; i++)
                {
                    var key = keys[i];
                    var share = shares[i];
                    var objectId = objectIds[i];

                    // xor(share, kdf(keys[i], nonce, id, this.objectIds[i], index))
                    var derivedKey = Kdf.KdfBytes(key, nonce, id, objectId, share.Index);
                    encryptedShares[i] = Utils.Xor(share.Data, derivedKey);
                }

                // 3. Rastgeleliği (randomness 'r') şifrele.
                // deriveKey(KeyPurpose.EncryptedRandomness, baseKey, ...)
                var randomnessKey = Kdf.DeriveKey(
                    Kdf.KeyPurpose.EncryptedRandomness,
                    baseKey,
                    encryptedShares,
                    threshold,
                    objectIds
                );

                // xor(randomnessKey, r.toBytes());
                var encryptedRandomness = Utils.Xor(randomnessKey, r.ToBytes());

                // Sonucu TypeScript'teki yapıyla aynı formatta döndür.
                return new IBEEncryptions
                {
                    BonehFranklinBLS12381 = new BonehFranklinBLS12381
                    {
                        nonce = nonce.ToBytes(),
                        encryptedShares = encryptedShares,
                        encryptedRandomness = encryptedRandomness
                    }
                };
            }

            public static byte[] DecryptShare(G2 nonce, G1 sk, byte[] encryptedShare, byte[] id, string objectId, int index)
            {
                var key = GT.Pairing(sk, nonce);
                var derivedKey = Kdf.KdfBytes(key, nonce, id, objectId, index);
                return Utils.Xor(encryptedShare, derivedKey);
            }
        }

        // Bu, TypeScript'teki dosyanın en altındaki yardımcı 'encapBatched' fonksiyonudur.
        // Geçen sefer başladığımız iskeleti şimdi tamamlıyoruz.
        private static (Fr r, G2 nonce, GT[] keys) EncapBatched(G2[] publicKeys, byte[] id)
        {
            if (publicKeys.Length == 0)
            {
                throw new ArgumentException("No public keys provided");
            }

            // const r = Scalar.random();
            var r = Fr.GetRandom();

            // const nonce = G2Element.generator().multiply(r);
            var nonce = G2.Generator * r;

            // const gid_r = hashToG1(id).multiply(r);
            var qId = Kdf.HashToG1(id);
            var gid_r = qId * r;

            // return [r, nonce, publicKeys.map((public_key) => gid_r.pairing(public_key))];
            var keys = publicKeys.Select(publicKey => GT.Pairing(gid_r, publicKey)).ToArray();

            return (r, nonce, keys);
        }

        // Bu da TypeScript'teki 'decap' fonksiyonu.
        private static GT Decap(G2 nonce, G1 usk)
        {
            // return usk.pairing(nonce);
            return GT.Pairing(usk, nonce);
        }
    }
}