using MCL.BLS12_381.Net; // Kendi kripto kütüphanemiz
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenDive.BCS;
using Sui.Accounts;
using Sui.Rpc.Client;
using Sui.Rpc.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
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
        public SuiClient SuiClient { get; set; } // SuiClient nesnesini buraya ekliyoruz.
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
        public AccountAddress PackageId { get; set; }
        public string Id { get; set; } // Bu artık byte[] değil, string
        public byte[] Data { get; set; }
        public byte[] Aad { get; set; } = new byte[0];
    }

    // Şifrelenmiş veriyi paketlemek için kullanılacak sınıf
    public class EncryptedObject
    {
        public int version = 0;
        public AccountAddress packageId;
        public string id;
        public (AccountAddress objectId, int index)[] services;
        public int threshold;
        public IBEEncryptions encryptedShares;
        public Ciphertext ciphertext;

        public void Serialize(Serialization serializer)
        {
            serializer.Serialize((byte)this.version);

            serializer.Serialize(this.packageId);
            byte[] idBytes = Utils.FromHex(this.id);

            // 2. Bu byte dizisini 'Bytes' wrapper'ı ile serialize ediyoruz.
            // Bu, başına otomatik olarak ULEB128 uzunluk ön eki ekleyecektir.
            serializer.Serialize(new Bytes(idBytes));       // String

            // (string, int) tuple dizisini serialize et
            serializer.SerializeU32AsUleb128((uint)this.services.Length);
            foreach (var service in this.services)
            {
                // Artık 'service.objectId' bir AccountAddress nesnesi.
                // Serializer, bu nesnenin kendi Serialize metodunu çağırarak
                // onu doğru şekilde (32 byte, uzunluksuz) yazacak.
                serializer.Serialize(service.objectId);
                serializer.SerializeU8((byte)service.index);
            }

            serializer.SerializeU8((byte)this.threshold);

            this.encryptedShares.Serialize(serializer);
            this.ciphertext.Serialize(serializer);
        }
    }

    public class DecryptOptions
    {
        public byte[] Data { get; set; } // Sadece şifreli veri
        public SessionKey SessionKey { get; set; }
        public byte[] TxBytes { get; set; }
    }

    public class FetchKeysOptions
    {
        public string[] Ids { get; set; }
        public byte[] TxBytes { get; set; }
        public SessionKey SessionKey { get; set; }
        public int Threshold { get; set; }
    }

    public class SealClient
    {
        private readonly SuiClient suiClient;

        private readonly List<KeyServerConfig> serverConfigs;

        private readonly Dictionary<string, KeyServer> keyServers;
        private readonly Dictionary<string, G1> cachedKeys = new Dictionary<string, G1>();

        private const ulong EXPECTED_SERVER_VERSION = 1;
        public SealClient(SealClientOptions options)
        {
            if (options.SuiClient == null)
            {
                throw new ArgumentNullException(nameof(options.SuiClient), "SuiClient cannot be null.");
            }
            this.suiClient = options.SuiClient;
            this.serverConfigs = options.ServerConfigs;
            this.keyServers = new Dictionary<string, KeyServer>();
        }

        public async Task InitializeAsync()
        {
            var objectIds = serverConfigs.Select(c => c.objectId).ToList();
            var objectIdAddresses = objectIds.Select(id => new AccountAddress(id)).ToList();
            // SUI SDK'nızdaki 'GetObject' veya 'MultiGetObjects' metodunu kullanın.
            // Burada MultiGetObjects varsayılmıştır.
            var objectResponses = await suiClient.MultiGetObjectsAsync(
                objectIdAddresses,
                new ObjectDataOptions { ShowContent = true }
            );

            foreach (var response in objectResponses.Result)
            {
                if (response.Error != null)
                {
                    // Hata yönetimi
                    continue;
                }
                
                var moveObject = (ParsedMoveObject)response.Data.Content.ParsedData;
                // NOT: 'fields' içindeki 'url' ve 'pk' alan adlarının, Move objesindeki
                // alan adlarıyla eşleştiğinden emin olun.
                var fields = moveObject.Fields;

                var firstVersion = Convert.ToUInt64(fields["first_version"]);
                var lastVersion = Convert.ToUInt64(fields["last_version"]);

                if (EXPECTED_SERVER_VERSION < firstVersion || EXPECTED_SERVER_VERSION > lastVersion)
                {
                    throw new Exception($"Key server {response.Data.ObjectID} version mismatch. Expected {EXPECTED_SERVER_VERSION}, but server supports {firstVersion}-{lastVersion}.");
                }

                var fieldNameInput = new DynamicFieldNameInput("u64", EXPECTED_SERVER_VERSION.ToString());

                var versionedResponse = await suiClient.GetDynamicFieldObjectAsync(
                    response.Data.ObjectID,
                    fieldNameInput
                );

                if (versionedResponse.Error != null)
                {
                    continue;
                }

                var topLevelFields = ((ParsedMoveObject)versionedResponse.Result.Data.Content.ParsedData).Fields;

                JToken valueToken = topLevelFields["value"];
                var valueJObject = (JObject)valueToken;
                // Bu 'value' objesinin içindeki 'fields' anahtarını alıyoruz.
                if (valueJObject.TryGetValue("fields", out var fieldsToken))
                {
                    var fieldsJObject = (JObject)fieldsToken;
                    // Artık en içteki fields objesindeyiz ve url/pk'ya erişebiliriz.
                    var url = fieldsJObject["url"]?.ToString();

                    byte[] pkBytes = null;
                    var pkArray = fieldsJObject["pk"] as JArray;
                    if (pkArray != null)
                    {
                        // 'pk' bir JArray, içindeki her bir sayıyı byte'a çeviriyoruz.
                        pkBytes = pkArray.Select(jv => (byte)jv).ToArray();
                    }

                    // --- PK FORMAT KONTROLÜ ---
                    // Eğer 'pk' base64 string olarak geliyorsa, üstteki satır yerine bunu kullan:
                    // var pkBase64String = fieldsJObject["pk"]?.ToString();
                    // var pkBytes = Convert.FromBase64String(pkBase64String);

                    if (!string.IsNullOrEmpty(url) && pkBytes != null)
                    {
                        var keyServer = new KeyServer
                        {
                            objectId = response.Data.ObjectID.ToString(),
                            url = url,
                            pk = pkBytes // veya pkBytes
                        };
                        this.keyServers[keyServer.objectId] = keyServer;
                    }
                }

            }
        }

        // Bu, TypeScript'teki 'encrypt.ts' içindeki ana 'encrypt' fonksiyonunun C# karşılığıdır.
        public async Task<(byte[] demKey, EncryptedObject encryptedObject)> Encrypt(EncryptOptions options)
        {
            if (keyServers.Count == 0)
            {
                throw new InvalidOperationException("SealClient is not initialized. Call InitializeAsync() first.");
            }

            // DÜZELTME 2: Dictionary'deki tüm sunucuları almak için .Values.ToList() kullanıyoruz.
            var activeKeyServers = this.keyServers.Values.ToList();

            if (options.Threshold <= 0 || options.Threshold > Utils.MAX_U8 ||
                activeKeyServers.Count < options.Threshold || activeKeyServers.Count > Utils.MAX_U8)
            {
                throw new ArgumentException("Invalid key servers or threshold");
            }

            IEncryptionInput encryptionInput = new AesGcm256(options.Data, options.Aad);
            var baseKey = await encryptionInput.GenerateKey();
            var shares = Shamir.Split(baseKey, options.Threshold, activeKeyServers.Count);

            var fullId = Utils.ToHex(Utils.Flatten(options.PackageId.KeyBytes, Utils.FromHex(options.Id)));
            var fullIdBytes = Utils.FromHex(fullId);

            var publicKeys = activeKeyServers.Select(ks => G2.FromBytes(ks.pk)).ToArray();
            var objectIds = activeKeyServers.Select(ks => new AccountAddress(ks.objectId)).ToArray();

            var ibeEncryptions = Ibe.BonehFranklin.EncryptBatched(
                publicKeys, fullIdBytes, shares, baseKey, options.Threshold, objectIds
            );

            var demKey = Kdf.DeriveKey(
                Kdf.KeyPurpose.DEM,
                baseKey,
                ibeEncryptions.BonehFranklinBLS12381.encryptedShares,
                options.Threshold,
                objectIds
            );

            var ciphertext = await encryptionInput.Encrypt(demKey);

            var services = activeKeyServers.Select((ks, i) => (new AccountAddress(ks.objectId), shares[i].Index)).ToArray();

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

        public async Task FetchKeys(FetchKeysOptions options)
        {
            if (options.Threshold > serverConfigs.Count || options.Threshold < 1) throw new ArgumentException("...");

            var fullIds = options.Ids.Select(id => Utils.ToHex(Utils.Flatten(Utils.FromHex(options.SessionKey.GetPackageId()), Utils.FromHex(id)))).ToArray();

            // Hangi sunuculardan anahtar istememiz gerektiğini bul
            var keyServersToFetchFrom = keyServers.Values.Where(ks =>
                !fullIds.All(fullId => cachedKeys.ContainsKey($"{fullId}:{ks.objectId}"))
            ).ToList();

            if (keyServersToFetchFrom.Count == 0)
            {
                UnityEngine.Debug.Log("Gerekli tüm anahtarlar zaten önbellekte.");
                return;
            }

            var myCertificate = options.SessionKey.GetCertificate();
            var requestParams = options.SessionKey.CreateRequestParams(options.TxBytes); // Bu metodu async yapmamız gerekebilir

            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("Client-Sdk-Version", "0.8.6");
                httpClient.DefaultRequestHeaders.Add("Client-Sdk-Type", "typescript");
                httpClient.DefaultRequestHeaders.Add("Request-Id", Guid.NewGuid().ToString());
                foreach (var server in keyServersToFetchFrom)
                {
                    UnityEngine.Debug.Log($"{server.url} adresinden anahtar isteniyor...");

                    // Sunucuya gönderilecek isteğin gövdesini (body) oluştur.
                    // Bu yapı, sunucunun beklediği JSON formatıyla eşleşmelidir.
                    var requestBody = new
                    {
                        ptb = Convert.ToBase64String(options.TxBytes),
                        enc_key = Convert.ToBase64String(requestParams.EncKeyPk),
                        enc_verification_key = Convert.ToBase64String(requestParams.EncVerificationKey),
                        request_signature = requestParams.RequestSignature,
                        certificate = myCertificate,
                    };

                    var jsonContent = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
                    UnityEngine.Debug.Log(JsonConvert.SerializeObject(requestBody));
                    // HTTP POST isteğini yap
                    var httpResponse = await httpClient.PostAsync(server.url + "/v1/fetch_key", jsonContent);
                    string content = await httpResponse.Content.ReadAsStringAsync();
                    UnityEngine.Debug.Log(content);
                    httpResponse.EnsureSuccessStatusCode();

                    var jsonResponse = await httpResponse.Content.ReadAsStringAsync();
                    // Gelen cevabı parse et (Bu kısım sunucunun cevabına göre değişir)
                    var keyResponse = JObject.Parse(jsonResponse);

                    foreach (var fullId in fullIds)
                    {
                        if (keyResponse.TryGetValue(fullId, out var keyToken))
                        {
                            var keyBytes = Convert.FromBase64String(keyToken.ToString());
                            var partialKey = G1.FromBytes(keyBytes);

                            // Gelen anahtarın geçerliliğini doğrula (verifyUserSecretKey)
                            // ...

                            // Anahtarı önbelleğe ekle
                            AddKeyToCache(fullId, server.objectId, partialKey);
                            UnityEngine.Debug.Log($"Anahtar {server.objectId} sunucusundan alındı ve önbelleğe eklendi.");
                        }
                    }
                }
            }
        }

        public async Task<byte[]> Decrypt(DecryptOptions options)
        {
            var encryptedObject = JsonConvert.DeserializeObject<EncryptedObject>(Encoding.UTF8.GetString(options.Data), new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects });
            await FetchKeys(
                new FetchKeysOptions()
                {
                    Ids= new[] { encryptedObject.id },
                    TxBytes = options.TxBytes,
                    SessionKey = options.SessionKey,
                    Threshold = encryptedObject.threshold
                }
            );
            if (encryptedObject.encryptedShares.BonehFranklinBLS12381 == null)
            {
                throw new NotSupportedException("Encryption mode not supported");
            }

            var fullId = Utils.ToHex(Utils.Flatten(encryptedObject.packageId.KeyBytes, Utils.FromHex(encryptedObject.id)));

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