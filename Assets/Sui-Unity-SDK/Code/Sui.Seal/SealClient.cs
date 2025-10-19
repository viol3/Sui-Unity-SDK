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
using UnityEditor.PackageManager;
using UnityEngine;

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

    // Sunucudan dönen JSON'daki "decryption_keys" dizisinin içindeki her bir elemanı temsil eder.
    public class DecryptionKeyItem
    {
        [JsonProperty("id")]
        public List<byte> Id { get; set; } // Bu, fullId'nin byte dizisi hali

        [JsonProperty("encrypted_key")]
        public List<string> EncryptedKey { get; set; } // Bu, G1 anahtarlarının Base64 string listesi
    }

    // Sunucudan dönen JSON'un en dıştaki yapısını temsil eder.
    public class FetchKeysResponse
    {
        [JsonProperty("decryption_keys")]
        public List<DecryptionKeyItem> DecryptionKeys { get; set; }
    }

    // Şifrelenmiş veriyi paketlemek için kullanılacak sınıf
    public class EncryptedObject : ISerializable
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

        public static ISerializable Deserialize(Deserialization deserializer)
        {
            var obj = new EncryptedObject();

            // 1. Version (u8)
            obj.version = deserializer.DeserializeU8().Value;

            // 2. packageId (AccountAddress)
            // AccountAddress'in kendi Deserialize metodunu çağırır.
            obj.packageId = (AccountAddress)AccountAddress.Deserialize(deserializer);

            // 3. id (vector<u8> -> byte[] -> hex string)
            // Önce vector<u8> olarak okunur, sonra hex string'e çevrilir.
            byte[] idBytes = deserializer.ToBytes();
            obj.id = Utils.ToHex(idBytes);

            // 4. services (vector of tuples)
            // Önce dizinin eleman sayısını oku.
            int servicesLen = deserializer.DeserializeUleb128();
            obj.services = new (AccountAddress objectId, int index)[servicesLen];
            for (int i = 0; i < servicesLen; i++)
            {
                // Sırayla her bir tuple elemanını oku.
                var objectId = (AccountAddress)AccountAddress.Deserialize(deserializer);
                var index = deserializer.DeserializeU8().Value;
                obj.services[i] = (objectId, index);
            }

            // 5. threshold (u8)
            obj.threshold = deserializer.DeserializeU8().Value;

            // 6. encryptedShares (enum)
            // IBEEncryptions'ın kendi Deserialize metodunu çağırır.
            obj.encryptedShares = (IBEEncryptions)IBEEncryptions.Deserialize(deserializer);

            // 7. ciphertext (enum)
            // Ciphertext'in kendi Deserialize metodunu çağırır.
            obj.ciphertext = (Ciphertext)Ciphertext.Deserialize(deserializer);

            return obj;
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

    public class FetchKeysRequest
    {
        // JSON'daki "ptb" alanına karşılık gelir
        [JsonProperty("ptb")]
        public string Ptb { get; set; } // Base64 formatında, ilk byte'ı atılmış txBytes

        // JSON'daki "enc_key" alanına karşılık gelir
        [JsonProperty("enc_key")]
        public string EncKeyPk { get; set; } // Base64 formatında

        // JSON'daki "enc_verification_key" alanına karşılık gelir
        [JsonProperty("enc_verification_key")]
        public string EncVerificationKey { get; set; } // Base64 formatında

        // JSON'daki "request_signature" alanına karşılık gelir
        [JsonProperty("request_signature")]
        public string RequestSignature { get; set; } // Base64 formatında

        // JSON'daki "certificate" alanına karşılık gelir
        [JsonProperty("certificate")]
        public Certificate Certificate { get; set; } // Certificate nesnesinin kendisi
    }

    public class SealClient
    {
        private readonly SuiClient suiClient;

        private readonly List<KeyServerConfig> serverConfigs;

        private readonly Dictionary<string, KeyServer> keyServers;
        private readonly Dictionary<string, G1> cachedKeys = new Dictionary<string, G1>();

        private readonly int totalWeight;

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
            this.totalWeight = options.ServerConfigs.Sum(c => c.weight);
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

            //for (int i = 0; i < ibeEncryptions.BonehFranklinBLS12381.encryptedShares.Length; i++)
            //{
            //    UnityEngine.Debug.Log($"[ENCRYPT] encryptedShares[{i}] (Hex): {Utils.ToHex(ibeEncryptions.BonehFranklinBLS12381.encryptedShares[i])}");
            //}
            //for (int i = 0; i < objectIds.Length; i++)
            //{
            //    UnityEngine.Debug.Log($"[ENCRYPT] objectIds[{i}]: {objectIds[i]}");
            //}

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

        private int GetWeight(string objectId)
        {
            return serverConfigs.FirstOrDefault(c => c.objectId == objectId)?.weight ?? 0;
        }

        public async Task FetchKeysNew(FetchKeysOptions options)
        {
            // 1. Threshold Kontrolü
            if (options.Threshold > totalWeight || options.Threshold < 1)
                throw new ArgumentException($"Invalid threshold {options.Threshold} for total weight {totalWeight}");

            // 2. Full ID'leri Hesapla
            //    Not: options.Ids'in null veya boş olmaması kontrolü eklenebilir.
            var fullIds = options.Ids.Select(id =>
                Utils.ToHex(Utils.Flatten(Utils.FromHex(options.SessionKey.GetPackageId()), Utils.FromHex(id)))
            ).Distinct().ToArray(); // Benzersiz ID'leri al

            if (fullIds.Length == 0) return; // İstenecek ID yoksa çık

            // 3. Önbelleği Kontrol Et ve Kalan Sunucuları Belirle
            int completedWeight = 0;
            var remainingServerObjectIds = new List<string>();
            int remainingServersWeight = 0; // Hata durumunda erken çıkmak için

            if (this.keyServers.Count == 0) await InitializeAsync(); // Henüz başlatılmadıysa başlat

            foreach (var objectId in keyServers.Keys)
            {
                // Bu sunucu, istenen TÜM fullId'ler için önbellekte anahtara sahip mi?
                if (fullIds.All(fullId => cachedKeys.ContainsKey($"{fullId}:{objectId}")))
                {
                    int weight = GetWeight(objectId);
                    completedWeight += weight;
                }
                else
                {
                    int weight = GetWeight(objectId);
                    remainingServerObjectIds.Add(objectId);
                    remainingServersWeight += weight;
                }
            }

            // 4. Yeterli Anahtar Önbellekteyse Erken Çık
            if (completedWeight >= options.Threshold)
            {
                UnityEngine.Debug.Log($"FetchKeys: Gerekli threshold ({options.Threshold}) önbellekten sağlandı ({completedWeight}). Sunucuya gidilmeyecek.");
                return;
            }

            UnityEngine.Debug.Log($"FetchKeys: Önbellekte {completedWeight}/{options.Threshold} ağırlık bulundu. {remainingServerObjectIds.Count} sunucudan anahtar istenecek.");

            // 5. İstek İçin Gerekli Verileri Hazırla
            var myCertificate = options.SessionKey.GetCertificate();
            // CreateRequestParams artık SignedRequestParams döndürüyor ve async olabilir
            var requestParams = options.SessionKey.CreateRequestParams(options.TxBytes);
            var errors = new List<Exception>();
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("Client-Sdk-Version", "0.8.6"); // Güncel tutulmalı
                httpClient.DefaultRequestHeaders.Add("Client-Sdk-Type", "typescript");
                // 6. Kalan Sunuculara SIRAYLA İstek Gönder
                foreach (var objectId in remainingServerObjectIds)
                {
                    // Her döngü başında yeterli anahtar toplandıysa çık (önceki sunucu tamamlamış olabilir)
                    if (completedWeight >= options.Threshold) break;

                    // Threshold artık ulaşılamaz durumdaysa erken çık (TS'deki finally bloğu mantığı)
                    if (remainingServersWeight < options.Threshold - completedWeight)
                    {
                        UnityEngine.Debug.LogWarning($"FetchKeys: Kalan sunucularla threshold'a ulaşılamıyor ({remainingServersWeight} < {options.Threshold - completedWeight}). İşlem durduruluyor.");
                        // Hata fırlatmak yerine sadece döngüden çıkabiliriz, son kontrol yapılacak.
                        break;
                    }

                    var server = keyServers[objectId];
                    string requestId = Guid.NewGuid().ToString();

                    try
                    {
                        UnityEngine.Debug.Log($"{server.url}/v1/fetch_key adresinden anahtar isteniyor (RequestId: {requestId})...");
                        
                        var requestBody = new FetchKeysRequest
                        {
                            Ptb = Convert.ToBase64String(options.TxBytes),
                            EncKeyPk = Convert.ToBase64String(requestParams.EncKeyPk),
                            EncVerificationKey = Convert.ToBase64String(requestParams.EncVerificationKey),
                            RequestSignature = requestParams.RequestSignature,
                            Certificate = myCertificate,
                        };
                        var jsonContent = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");

                        // İsteğe özel Request-Id ekle (DefaultHeaders yerine burada eklemek daha doğru olabilir)
                        using (var requestMessage = new HttpRequestMessage(HttpMethod.Post, server.url + "/v1/fetch_key"))
                        {
                            requestMessage.Headers.TryAddWithoutValidation("Request-Id", requestId);
                            // İsteğe bağlı API Key header'ları da burada eklenebilir (TS'deki gibi)
                            // var config = serverConfigs.FirstOrDefault(c=> c.objectId == objectId);
                            // if (config?.apiKeyName != null && config?.apiKey != null) { ... }
                            requestMessage.Content = jsonContent;

                            var httpResponse = await httpClient.SendAsync(requestMessage); // SendAsync kullan
                            string content = await httpResponse.Content.ReadAsStringAsync();

                            if (!httpResponse.IsSuccessStatusCode)
                            {
                                throw new HttpRequestException($"Sunucu {objectId} Hata Döndü ({httpResponse.StatusCode}) - RequestId: {requestId}: {content}");
                            }

                            var serverResponse = JsonConvert.DeserializeObject<FetchKeysResponse>(content);
                            if (serverResponse?.DecryptionKeys == null)
                            {
                                UnityEngine.Debug.LogWarning($"Sunucu {objectId} 'decryption_keys' dizisi döndürmedi.");
                                continue; // Bir sonraki sunucuya geç
                            }

                            bool serverCompletedThisRequest = true; // Bu sunucu istediğimiz tüm anahtarları verdi mi?
                            foreach (var keyItem in serverResponse.DecryptionKeys)
                            {
                                string fullId = Utils.ToHex(keyItem.Id.ToArray());
                                if (!fullIds.Contains(fullId)) continue;

                                // Bu fullId için zaten cache'de anahtar varsa tekrar işlem yapma
                                if (cachedKeys.ContainsKey($"{fullId}:{objectId}")) continue;

                                if (keyItem.EncryptedKey == null || keyItem.EncryptedKey.Count == 0)
                                {
                                    serverCompletedThisRequest = false; // Bu ID için anahtar gelmedi
                                    continue;
                                }

                                bool validKeyFoundForThisId = false;
                                foreach (var keyBase64 in keyItem.EncryptedKey)
                                {
                                    G1 partialKey;
                                    try
                                    {
                                        var keyBytes = Convert.FromBase64String(keyBase64);
                                        partialKey = G1.FromBytes(keyBytes);
                                    }
                                    catch(Exception ex)
                                    {
                                        UnityEngine.Debug.LogError("partial key error!! => " + ex.Message);
                                        continue; 
                                    } // Geçersiz anahtarı atla

                                    // ANAHTAR DOĞRULAMA
                                    var serverPublicKey = G2.FromBytes(server.pk);
                                    if (!Ibe.BonehFranklin.VerifyUserSecretKey(partialKey, fullId, serverPublicKey))
                                    {
                                        UnityEngine.Debug.LogWarning($"Sunucu {objectId} tarafından {fullId} için geçersiz anahtar alındı.");
                                        continue; // Bu anahtarı atla, listedeki diğerine bak
                                    }

                                    // Doğrulama BAŞARILI!
                                    AddKeyToCache(fullId, objectId, partialKey);
                                    UnityEngine.Debug.Log($"Anahtar (FullID: {fullId}) {server.objectId} sunucusundan alındı ve doğrulandı.");
                                    validKeyFoundForThisId = true;
                                    break; // Bu fullId için geçerli anahtar bulundu, iç döngüden çık
                                }

                                // Eğer iç döngü bittiğinde bu fullId için geçerli anahtar bulunamadıysa
                                if (!validKeyFoundForThisId)
                                {
                                    serverCompletedThisRequest = false;
                                }

                            } // foreach keyItem bitti

                            // Eğer bu sunucu istenen TÜM fullId'ler için GEÇERLİ anahtar verdiyse, ağırlığını ekle
                            if (serverCompletedThisRequest && fullIds.All(fullId => cachedKeys.ContainsKey($"{fullId}:{server.objectId}")))
                            {
                                int weight = GetWeight(objectId);
                                completedWeight += weight;
                                UnityEngine.Debug.Log($"Sunucu {objectId} tamamlandı. Toplam Ağırlık: {completedWeight}/{options.Threshold}");
                                // Yeterli anahtar toplandıysa dış döngüden çıkmak için bir sonraki iterasyonda break tetiklenecek.
                            }
                        } // using requestMessage bitti
                    } // try bitti
                    catch (Exception ex)
                    {
                        UnityEngine.Debug.LogError($"Sunucu {objectId} ile iletişimde hata (RequestId: {requestId}): {ex.Message}");
                        errors.Add(ex);
                    }
                    finally // TS'deki finally mantığı
                    {
                        int weight = GetWeight(objectId);
                        remainingServersWeight -= weight;
                    }

                } // foreach server bitti
            } // using httpClient bitti

            // 7. Son Threshold Kontrolü
            if (completedWeight < options.Threshold)
            {
                // Hataları birleştirerek fırlat (TS'deki toMajorityError gibi)
                var errorMessages = string.Join("; ", errors.Select(e => e.Message));
                throw new Exception($"Yeterli anahtar toplanamadı ({completedWeight}/{options.Threshold}). Hatalar: {errorMessages}");
            }
            UnityEngine.Debug.Log($"FetchKeys başarıyla tamamlandı. Toplam {completedWeight} ağırlıkta anahtar toplandı.");
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

                    var serverResponse = JsonConvert.DeserializeObject<FetchKeysResponse>(content);

                    if (serverResponse?.DecryptionKeys == null)
                    {
                        UnityEngine.Debug.LogWarning($"Sunucudan 'decryption_keys' dizisi alınamadı.");
                        continue;
                    }

                    // 2. 'decryption_keys' dizisindeki her bir eleman için dön
                    foreach (var keyItem in serverResponse.DecryptionKeys)
                    {
                        // 3. 'id' (byte dizisi) alanını hex string'e çevir
                        string fullId = Utils.ToHex(keyItem.Id.ToArray());
                        // 4. Bu fullId'nin, bizim istediğimiz ID'lerden biri olup olmadığını kontrol et
                        if (!fullIds.Contains(fullId))
                        {
                            UnityEngine.Debug.LogWarning($"Sunucu {server.objectId} istenmeyen bir anahtar döndürdü: {fullId}");
                            continue;
                        }

                        // 5. 'encrypted_key' dizisini al. 
                        //    TS koduna göre bu dizi, o sunucunun o 'fullId' için sahip olduğu
                        //    parça anahtarları içerir. (Eğer 'weight' > 1 ise birden fazla olabilir).
                        //    Şimdilik ilkini almamız yeterli.
                        if (keyItem.EncryptedKey == null || keyItem.EncryptedKey.Count == 0) continue;

                        var keyBase64 = keyItem.EncryptedKey[0]; // Listenin ilk anahtarını alıyoruz
                        var keyBytes = Convert.FromBase64String(keyBase64);
                        var partialKey = G1.FromBytes(keyBytes);

                        UnityEngine.Debug.Log($"Sunucu {server.objectId} için {keyItem.EncryptedKey.Count} adet anahtar döndü.");
                        if (keyItem.EncryptedKey.Count > 1)
                        {
                            var key1Bytes = Convert.FromBase64String(keyItem.EncryptedKey[0]);
                            var key2Bytes = Convert.FromBase64String(keyItem.EncryptedKey[1]);
                            bool areKeysIdentical = Utils.AreEqual(key1Bytes, key2Bytes);
                            UnityEngine.Debug.Log($"---> İkinci anahtar var! İlk anahtarla aynı mı? {areKeysIdentical}");
                            UnityEngine.Debug.Log($"---> Key 1 (Hex): {Utils.ToHex(key1Bytes)}");
                            UnityEngine.Debug.Log($"---> Key 2 (Hex): {Utils.ToHex(key2Bytes)}");
                        }


                        UnityEngine.Debug.Log($"---> Sunucudan alınan SK (objectId: {server.objectId}, fullId: {fullId}) doğrulanıyor...");
                        try
                        {
                            // a) Bu sunucunun public key'ini al (InitializeAsync'te çekmiştik)
                            if (!keyServers.TryGetValue(server.objectId, out var currentServerInfo))
                            {
                                UnityEngine.Debug.LogWarning($"Doğrulama için {server.objectId} sunucusunun bilgileri bulunamadı.");
                                continue;
                            }
                            var serverPublicKey = G2.FromBytes(currentServerInfo.pk);
                            
                            // b) qId'yi hesapla
                            var fullIdBytes = Utils.FromHex(fullId);
                            var qId = Kdf.HashToG1(fullIdBytes);
                            UnityEngine.Debug.Log("serverPublicKey Valid => " + serverPublicKey.IsValid());
                            UnityEngine.Debug.Log("qId Valid => " + qId.IsValid());
                            UnityEngine.Debug.Log("G2.Generator Valid => " + G2.Generator.IsValid());
                            UnityEngine.Debug.Log("Partial Key Valid => " + partialKey.IsValid());
                            // c) Denklemin sol tarafı: e(sk, G2.Generator)

                            var lhsPairing = GT.Pairing(partialKey, G2.Generator);
                            var lhsBytes = lhsPairing.ToBytes();

                            // d) Denklemin sağ tarafı: e(qId, publicKey)
                            var rhsPairing = GT.Pairing(qId, serverPublicKey);
                            var rhsBytes = rhsPairing.ToBytes();

                         
                            // e) Karşılaştır
                            if (lhsPairing == rhsPairing)
                            {
                                UnityEngine.Debug.Log($"<color=cyan>---> SK Doğrulaması BAŞARILI! Sunucu: {server.objectId}, FullID: {fullId}</color>");
                            }
                            else
                            {
                                UnityEngine.Debug.LogError($"<color=red>SK DOĞRULAMASI BAŞARISIZ! Sunucu: {server.objectId}, FullID: {fullId}. Pairing sonuçları eşleşmiyor.</color>");
                                UnityEngine.Debug.Log($"[SK VERIFY] LHS (e(sk, G2.Gen)) (Hex): {Utils.ToHex(lhsBytes)}");
                                UnityEngine.Debug.Log($"[SK VERIFY] RHS (e(qId, pk)) (Hex): {Utils.ToHex(rhsBytes)}");
                            }
                                
                        }
                        catch (Exception ex)
                        {
                            UnityEngine.Debug.LogError($"SK doğrulaması sırasında hata oluştu (Sunucu: {server.objectId}): {ex.Message}");
                        }

                        // 6. Anahtarı önbelleğe ekle
                        AddKeyToCache(fullId, server.objectId, partialKey);
                        UnityEngine.Debug.Log($"Anahtar (FullID: {fullId}) {server.objectId} sunucusundan alındı ve önbelleğe eklendi.");
                    }
                }
            }
        }

        public async Task<byte[]> Decrypt(DecryptOptions options)
        {
            Deserialization deserialization = new Deserialization(options.Data);
            var encryptedObject = (EncryptedObject)EncryptedObject.Deserialize(deserialization);
            await FetchKeysNew(
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
            G2 nonce = G2.FromBytes(ibeData.nonce);
            var fullIdBytes = Utils.FromHex(fullId);
            var qId = Kdf.HashToG1(fullIdBytes);
            UnityEngine.Debug.Log($"[QID HESAPLANDI] (Yer: Decrypt) qId (Hex): {Utils.ToHex(qId.ToBytes())}");
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
            var r = Fr.FromBytes(randomness);
            G2 secret = G2.Generator * r;
            string nonceStr = nonce.ToString();
            UnityEngine.Debug.Log(nonce.ToString());
            if (!secret.Equals(nonce))
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