// Ibe.cs
using MCL.BLS12_381.Net;
using Sui.Accounts;
using Sui.Seal;
using System;
using System.Linq;
using System.Security.Cryptography;
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
                AccountAddress[] objectIds
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
                    Debug.Log($"[ENCRYPT-PAIRING-INPUT] publicKey[{i}] (Hex): {Utils.ToHex(publicKeys[i].ToBytes())}");
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

                // d) VerifyNonce'ı çağır
                bool isNonceValid = VerifyNonce(nonce, r.ToBytes());

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

            public static byte[] DecryptShare(G2 nonce, G1 sk, byte[] encryptedShare, byte[] id, AccountAddress objectId, int index)
            {
                var key = GT.Pairing(sk, nonce);
                UnityEngine.Debug.Log("[DECRYPT] key => " + Utils.ToHex(key.ToBytes()));
                var derivedKey = Kdf.KdfBytes(key, nonce, id, objectId, index);
                return Utils.Xor(encryptedShare, derivedKey);
            }
        }

        private static Fr DecodeRandomness(byte[] bytes, bool useBE)
        {
            if (useBE)
            {
                // MCL kütüphanesinin varsayılanı Big-Endian'dır.
                return Fr.FromBytes(bytes);
            }
            else
            {
                // Little-Endian için, byte dizisini ters çevirip FromBytes'a vermeliyiz.
                var reversedBytes = bytes.Reverse().ToArray();
                return Fr.FromBytes(reversedBytes);
            }
        }

        /// <summary>
        /// Şifre çözme sırasında elde edilen 'nonce'ın geçerliliğini doğrular.
        /// 'randomness'tan türetilen noktanın, orijinal 'nonce' ile eşleşip eşleşmediğini kontrol eder.
        /// </summary>
        /// <param name="nonce">Doğrulanacak olan G2 noktası.</param>
        /// <param name="randomness">Çözülmüş 'randomness' verisi.</param>
        /// <param name="useBE">Randomness'ın nasıl yorumlanacağını belirtir.</param>
        /// <returns>Doğrulama başarılıysa true, değilse false.</returns>
        /// <exception cref="CryptographicException">'randomness' geçersizse fırlatılır.</exception>
        public static bool VerifyNonce(G2 nonce, byte[] randomness, bool useBE = true)
        {
            try
            {
                // 1. 'randomness'ı skalara (r) dönüştür.
                var r = DecodeRandomness(randomness, useBE);
                // 2. Jeneratör noktasını bu skalarla çarp (G2.Generator * r).
                var calculatedNonce = G2.Generator * r;

                // 3. Hesaplanan noktanın, orijinal nonce ile eşleşip eşleşmediğini kontrol et.
                return calculatedNonce.Equals(nonce);
            }
            catch (Exception ex)
            {
                // Fr.FromBytes geçersiz bir byte dizisi alırsa hata fırlatabilir.
                // Bu durumu yakalayıp daha anlamlı bir hata mesajı veriyoruz.
                throw new CryptographicException("Invalid randomness, could not decode scalar.", ex);
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
            //var r = Fr.FromBytes(new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18, 0x19, 0x20, 0x21, 0x22, 0x23, 0x24, 0x25, 0x26, 0x27, 0x28, 0x29, 0x30, 0x31, 0x32 });
            // const nonce = G2Element.generator().multiply(r);
            var nonce = G2.Generator * r;

            // const gid_r = hashToG1(id).multiply(r);
            var qId = Kdf.HashToG1(id);
            var gid_r = qId * r;
            Debug.Log($"[ENCRYPT-PAIRING-INPUT] gid_r (Hex): {Utils.ToHex(gid_r.ToBytes())}");
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