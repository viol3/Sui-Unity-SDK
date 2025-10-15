using MCL.BLS12_381.Net; // Kendi kripto kütüphanemiz
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Sui.Seal
{
    public class KeyServerConfig
    {
        public string objectId;
        public int weight;
        public string url;
    }

    public class SealClientOptions
    {
        public List<KeyServerConfig> ServerConfigs { get; set; }
        // suiClient, apiKey gibi kısımları şimdilik atlıyoruz.
    }

    public enum KemType { BonehFranklinBLS12381DemCCA = 0 }
    public enum DemType { AesGcm256 = 0, Hmac256Ctr = 1 }

    public class EncryptOptions
    {
        public KemType KemType { get; set; } = KemType.BonehFranklinBLS12381DemCCA;
        public DemType DemType { get; set; } = DemType.AesGcm256;
        public int Threshold { get; set; }
        public string PackageId { get; set; }
        public string Id { get; set; } // Bu artık byte[] değil, string
        public byte[] Data { get; set; }
        public byte[] Aad { get; set; } = new byte[0];
    }

    // Şifrelenmiş veriyi paketlemek için kullanılacak sınıf
    public class EncryptedObject
    {
        public int version = 0;
        public string packageId;
        public string id;
        public (string objectId, int index)[] services;
        public int threshold;
        public IBEEncryptions encryptedShares;
        public Ciphertext ciphertext;
    }

    public class SealClient
    {
        private readonly List<KeyServer> keyServers;
        private readonly List<KeyServerConfig> serverConfigs;

        private readonly Dictionary<string, G1> cachedKeys = new Dictionary<string, G1>();
        public SealClient(SealClientOptions options)
        {
            this.serverConfigs = options.ServerConfigs;
            var sk = Fr.GetRandom();
            // 2. Jeneratör noktasını (G) alıp bu gizli anahtarla çarpıyoruz. pk = sk * G
            var pk = G2.Generator * sk;
            this.keyServers = options.ServerConfigs.Select(config => new KeyServer
            {

                objectId = config.objectId,
                pk = pk.ToBytes() // Test için rastgele public key'ler
            }).ToList();
        }

        // Bu, TypeScript'teki 'encrypt.ts' içindeki ana 'encrypt' fonksiyonunun C# karşılığıdır.
        public async Task<(byte[] demKey, EncryptedObject encryptedObject)> Encrypt(EncryptOptions options)
        {
            // === Giriş kontrolleri ===
            if (options.Threshold <= 0 || options.Threshold > Utils.MAX_U8 ||
                keyServers.Count < options.Threshold || keyServers.Count > Utils.MAX_U8)
            {
                throw new ArgumentException("Invalid key servers or threshold");
            }

            // === 1. Adım: DEM Tipine Göre Şifreleme Girişi Oluştur ===
            IEncryptionInput encryptionInput;
            switch (options.DemType)
            {
                case DemType.AesGcm256:
                    encryptionInput = new AesGcm256(options.Data, options.Aad);
                    break;
                // case DemType.Hmac256Ctr:
                //     encryptionInput = new Hmac256Ctr(options.Data, options.Aad);
                //     break;
                default:
                    throw new NotSupportedException("Unsupported DEM type");
            }

            // === 2. Adım: Rastgele bir "base key" oluştur ===
            // const baseKey = await encryptionInput.generateKey();
            var baseKey = await encryptionInput.GenerateKey();

            // === 3. Adım: Base key'i Shamir ile paylara ayır ===
            // const shares = split(baseKey, threshold, keyServers.length);
            var shares = Shamir.Split(baseKey, options.Threshold, this.keyServers.Count);

            // === 4. Adım: Payları IBE ile şifrele ===
            // const fullId = createFullId(packageId, id);
            var fullId = Utils.ToHex(Utils.Flatten(Utils.FromHex(options.PackageId), Utils.FromHex(options.Id)));
            var fullIdBytes = Utils.FromHex(fullId);

            var publicKeys = this.keyServers.Select(ks => G2.FromBytes(ks.pk)).ToArray();
            var objectIds = this.keyServers.Select(ks => ks.objectId).ToArray();

            // const encryptedShares = encryptBatched(...)
            var ibeEncryptions = Ibe.BonehFranklin.EncryptBatched(
                publicKeys, fullIdBytes, shares, baseKey, options.Threshold, objectIds
            );
            // === 5. Adım: DEM anahtarını türet ===
            // const demKey = deriveKey(...)
            var demKey = Kdf.DeriveKey(
                Kdf.KeyPurpose.DEM,
                baseKey,
                ibeEncryptions.BonehFranklinBLS12381.encryptedShares,
                options.Threshold,
                objectIds
            );

            // === 6. Adım: Orijinal veriyi DEM anahtarı ile şifrele ===
            // const ciphertext = await encryptionInput.encrypt(demKey);
            var ciphertext = await encryptionInput.Encrypt(demKey);

            // === 7. Adım: Sonucu EncryptedObject olarak paketle ===
            var services = keyServers.Select((ks, i) => (ks.objectId, shares[i].Index)).ToArray();

            var encryptedObject = new EncryptedObject
            {
                version = 0,
                packageId = options.PackageId,
                id = options.Id,
                services = services,
                threshold = options.Threshold,
                encryptedShares = ibeEncryptions,
                ciphertext = ciphertext,
            };

            return (demKey, encryptedObject);
        }

        public async Task<byte[]> Decrypt(EncryptedObject encryptedObject)
        {
            if (encryptedObject.encryptedShares.BonehFranklinBLS12381 == null)
            {
                throw new NotSupportedException("Encryption mode not supported");
            }

            var fullId = Utils.ToHex(Utils.Flatten(Utils.FromHex(encryptedObject.packageId), Utils.FromHex(encryptedObject.id)));

            // Önbelleğimizde bu 'fullId' için anahtarı olan sunucuları bul
            var availableServices = encryptedObject.services
                .Where(s => cachedKeys.ContainsKey($"{fullId}:{s.objectId}"))
                .ToList();

            if (availableServices.Count < encryptedObject.threshold)
            {
                throw new Exception("Not enough shares to decrypt. Please fetch more keys.");
            }

            var ibeData = encryptedObject.encryptedShares.BonehFranklinBLS12381;
            var encryptedShares = ibeData.encryptedShares;
            var nonce = G2.FromBytes(ibeData.nonce);
            var fullIdBytes = Utils.FromHex(fullId);

            // 1. Adım: Mevcut anahtarlarla şifreli payları (shares) çöz
            var shares = availableServices.Select(service =>
            {
                var (objectId, index) = service;
                var sk = cachedKeys[$"{fullId}:{objectId}"];

                int serviceIndex = Array.FindIndex(encryptedObject.services, s => s.objectId == objectId && s.index == index);
                var encryptedShare = encryptedShares[serviceIndex];

                var shareData = Ibe.BonehFranklin.DecryptShare(nonce, sk, encryptedShare, fullIdBytes, objectId, index);
                return new Share { Index = index, Data = shareData };
            }).ToList();

            // 2. Adım: Çözülmüş payları birleştirerek "baseKey"i yeniden oluştur
            var baseKey = Shamir.Combine(shares.ToArray());

            // 3. Adım: "randomnessKey"i türet ve randomness'ı çöz
            var allObjectIds = encryptedObject.services.Select(s => s.objectId).ToArray();
            var randomnessKey = Kdf.DeriveKey(Kdf.KeyPurpose.EncryptedRandomness, baseKey, encryptedShares, encryptedObject.threshold, allObjectIds);
            var randomness = Utils.Xor(ibeData.encryptedRandomness, randomnessKey);

            // 4. Adım: Nonce'ı doğrula
            var r = Fr.FromBytes(randomness);
            if (!(G2.Generator * r).Equals(nonce))
            {
                throw new CryptographicException("Invalid nonce, decryption failed.");
            }

            // 5. Adım: DEM anahtarını türet
            var demKey = Kdf.DeriveKey(Kdf.KeyPurpose.DEM, baseKey, encryptedShares, encryptedObject.threshold, allObjectIds);

            // 6. Adım: Ana şifreli metni (ciphertext) DEM anahtarı ile çöz
            if (encryptedObject.ciphertext is Aes256GcmCiphertext aesCiphertext)
            {
                return await AesGcm256.Decrypt(demKey, aesCiphertext);
            }

            throw new NotSupportedException("Unsupported ciphertext type");
        }

        // Bu, TypeScript'teki 'fetchKeys' metodunun basitleştirilmiş bir simülasyonudur.
        // Test için, userSecretKey'leri doğrudan önbelleğe ekler.
        public void AddKeyToCache(string fullId, string objectId, G1 userSecretKey)
        {
            cachedKeys[$"{fullId}:{objectId}"] = userSecretKey;
        }
    }
}