using Codice.CM.Client.Differences;
using Codice.CM.Common.Serialization.Replication;
using MCL.BLS12_381.Net; // Kütüphanenin C# sınıflarını kullanabilmek için
using NBitcoin;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenDive.BCS;
using Sui.Accounts;
using Sui.Cryptography;
using Sui.Rpc;
using Sui.Rpc.Client;
using Sui.Rpc.Models;
using Sui.Seal;
using Sui.Transactions;
using Sui.Types;
using Sui.Utilities;
using System;            // Exception ve BitConverter kullanmak için
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.XR;

public class BlsTest : MonoBehaviour
{
    SuiClient _suiClient;
    Account _account;
    string _objectIdToDecrypt = "0x05c7af488251a84cf8f4b975b164c7fe75be251f5387e79c74693e0706ef2334";
    string _packageId = "0xf3dfe70b4916fecaecf7928bb8221031c28d5130c66e8fa7e645ce8785846f91";
    string _moduleName = "private_data";
    string _funcName = "store_entry";
    byte[] _policyId;
    byte[] _nonceBytes = { 179, 187, 103, 40, 166, 131, 240, 66, 249, 74, 252, 248, 94, 86, 237, 156, 126, 166, 204, 121, 87, 83, 242, 54, 142, 192, 68, 94, 192, 49, 245, 27 };
    async void Start()
    {
        _suiClient = new SuiClient(Constants.TestnetConnection);
        _account = new Account("0x8358b8f5a0850969194d0cd0e6e70dad2ec27b981669a8caf9fc566a17c9c115");
        Debug.Log("Sui Address => " + _account.SuiAddress());
        //byte[] nonce = Utils.GenerateNonce();
        _policyId = Sui.Seal.Utils.CreatePolicyId(_account.SuiAddress().KeyHex, _nonceBytes);
        RunAllTests();
        //TestSignPersonalMessage();
        //await DecryptTest();
        //TestBCSSerialization();
    }

    private void TestBCSSerialization()
    {
        Debug.Log("--------- C# SERİLEŞTİRME TESTİ ---------");

        // 1. TypeScript'teki ile AYNI SABİT nesneyi oluşturuyoruz.
        var testEncryptedObject = new EncryptedObject
        {
            version = 0,
            packageId = new AccountAddress("0xf3dfe70b4916fecaecf7928bb8221031c28d5130c66e8fa7e645ce8785846f91"),
            id = "0xdeffff",
            services = new[] { (new AccountAddress("0x73d05d62c18d9374e3ea529e8e0ed6161da1a141a94d3f76ae3fe4e99356db75"), 1), (new AccountAddress("0xf5d14a81a982144ae441cd7d64b09027f116a468bd36e7eca494f750591623c8"), 2) },
            threshold = 2,
            encryptedShares = new IBEEncryptions
            {
                BonehFranklinBLS12381 = new BonehFranklinBLS12381
                {
                    nonce = new byte[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                    encryptedShares = new[] { new byte[] { 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2 }, new byte[] { 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3 } },
                    encryptedRandomness = new byte[] { 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4 }
                }
            },
            ciphertext = new Aes256GcmCiphertext
            {
                blob = new byte[] { 11, 12, 13, 14, 15, 16, 17 },
                aad = new byte[] { 14, 15 }
            }
        };

        // Her alan için ayrı bir serializer kullanarak logluyoruz
        var s = new Serialization();
        s.SerializeU8((byte)testEncryptedObject.version);
        Debug.Log($"[C#] version (u8): {Sui.Seal.Utils.ToHex(s.GetBytes())}");

        s = new Serialization();
        s.Serialize(testEncryptedObject.packageId);
        Debug.Log($"[C#] packageId (string): {Sui.Seal.Utils.ToHex(s.GetBytes())}");

        s = new Serialization();
        s.Serialize(testEncryptedObject.id);
        Debug.Log($"[C#] id (string): {Sui.Seal.Utils.ToHex(s.GetBytes())}");

        s = new Serialization();
        s.SerializeU32AsUleb128((uint)testEncryptedObject.services.Length);
        foreach (var service in testEncryptedObject.services)
        {
            s.Serialize(service.objectId);
            s.SerializeU8((byte)service.index);
        }
        Debug.Log($"[C#] services (vector<(string, u8)>): {Sui.Seal.Utils.ToHex(s.GetBytes())}");

        s = new Serialization();
        s.SerializeU8((byte)testEncryptedObject.threshold);
        Debug.Log($"[C#] threshold (u8): {Sui.Seal.Utils.ToHex(s.GetBytes())}");

        s = new Serialization();
        testEncryptedObject.encryptedShares.Serialize(s);
        Debug.Log($"[C#] encryptedShares (enum): {Sui.Seal.Utils.ToHex(s.GetBytes())}");

        s = new Serialization();
        testEncryptedObject.ciphertext.Serialize(s);
        Debug.Log($"[C#] ciphertext (enum): {Sui.Seal.Utils.ToHex(s.GetBytes())}");

        // Son olarak, objenin tamamını serialize edelim
        s = new Serialization();
        testEncryptedObject.Serialize(s);
        Debug.Log($"[C#] TAMAMI: {Sui.Seal.Utils.ToHex(s.GetBytes())}");
    }

    private async void RunAllTests()
    {
        Debug.Log("====== TÜM TESTLER BAŞLATILIYOR (PLATFORM: WINDOWS/EDITOR) ======");

        //Debug.Log("--- G1/G2 FromBytes Roundtrip Test ---");
        //try
        //{
        //    // Ensure ETH mode is set as it would be during Decrypt
        //    // If you made it permanently on at Start(), you don't need this line.
        //    // MCL_Imports.mclBn_setETHserialization(1); // Or 0, depending on your setup

        //    // Test G1
        //    var originalG1 = G1.Generator * Fr.GetRandom(); // Create a random G1 point
        //    var g1Bytes = originalG1.ToBytes();
        //    var reconstructedG1 = G1.FromBytes(g1Bytes);
        //    if (!originalG1.Equals(reconstructedG1))
        //    {
        //        Debug.LogError("G1 FromBytes Roundtrip FAILED!");
        //        Debug.Log($"Original G1: {Sui.Seal.Utils.ToHex(g1Bytes)}");
        //        // It might be useful to log the internal struct values if possible
        //    }
        //    else
        //    {
        //        Debug.Log("G1 FromBytes Roundtrip SUCCESSFUL.");
        //    }

        //    // Test G2
        //    var originalG2 = G2.Generator * Fr.GetRandom(); // Create a random G2 point
        //    var g2Bytes = originalG2.ToBytes();
        //    var reconstructedG2 = G2.FromBytes(g2Bytes);
        //    if (originalG2 == reconstructedG2)
        //    {
        //        Debug.Log("G2 FromBytes Roundtrip SUCCESSFUL.");
        //    }
        //    else
        //    {
        //        Debug.LogError("G2 FromBytes Roundtrip FAILED!");
        //        Debug.Log($"Original G2: {Sui.Seal.Utils.ToHex(g2Bytes)}");
        //    }
        //    Debug.Log("--- Roundtrip Test End ---");
        //}
        //catch (Exception ex)
        //{
        //    Debug.LogError($"Roundtrip Test Exception: {ex}");
        //}
        //finally
        //{
        //    // Ensure ETH mode is back to what Decrypt expects if you changed it
        //    // MCL_Imports.mclBn_setETHserialization(0); // Or 1
        //}
        //TestIsolatedPairingConsistency();
        EncryptedObject encryptedObject = await TestSealClientEncrypt();


        //var serializer = new Serialization();
        //encryptedObject.Serialize(serializer);
        //byte[] encryptedBytes = serializer.GetBytes();
        //Debug.Log($"[C#] TAMAMI: {Sui.Seal.Utils.ToHex(encryptedBytes)}");
        //Debug.Log(encryptedBytes.Length);
        //TransactionBlock tx_block = new TransactionBlock();
        //tx_block.AddMoveCallTx
        //(
        //    SuiMoveNormalizedStructType.FromStr($"{_packageId}::{_moduleName}::{_funcName}"),
        //    new SerializableTypeTag[] { },
        //    new TransactionArgument[]
        //    {
        //        tx_block.AddPure(new OpenDive.BCS.Bytes(_nonceBytes)),
        //        tx_block.AddPure(new OpenDive.BCS.Bytes(encryptedBytes))
        //    }
        //);
        //await _suiClient.SignAndExecuteTransactionBlockAsync(tx_block, _account);
        //Debug.Log("====== TÜM TESTLER TAMAMLANDI ======");
        await DecryptTest(encryptedObject);
    }

    private void TestIsolatedPairingConsistency()
    {
        Debug.Log("--- İzole Pairing Tutarlılık Testi Başlatılıyor ---");
        try
        {
            // ETH modunun Decrypt sırasında olduğu gibi ayarlandığından emin ol
            // (Eğer Start'ta kalıcı açtıysan bu satıra gerek yok)
            // MCL_Imports.mclBn_setETHserialization(1); // VEYA 0

            // 1. Sabit Girdiler Oluştur
            var msk = new Fr(); msk.SetInt(123); // Sabit master secret key
            var r = new Fr(); r.SetInt(456);     // Sabit randomness
            var id = Encoding.UTF8.GetBytes("test-id-for-pairing");

            // 2. Gerekli Noktaları Hesapla
            var publicKey = G2.Generator * msk;         // G2
            var qId = Kdf.HashToG1(id);                 // G1
            var gid_r = qId * r;                        // G1
            var nonce = G2.Generator * r;               // G2
            var sk = qId * msk;                         // G1 (fetchKeys simülasyonu)

            Debug.Log($"[ISOLATED] msk: {msk}");
            Debug.Log($"[ISOLATED] r: {r}");
            Debug.Log($"[ISOLATED] publicKey (Hex): {Sui.Seal.Utils.ToHex(publicKey.ToBytes())}");
            Debug.Log($"[ISOLATED] qId (Hex): {Sui.Seal.Utils.ToHex(qId.ToBytes())}");
            Debug.Log($"[ISOLATED] gid_r (Hex): {Sui.Seal.Utils.ToHex(gid_r.ToBytes())}");
            Debug.Log($"[ISOLATED] nonce (Hex): {Sui.Seal.Utils.ToHex(nonce.ToBytes())}");
            Debug.Log($"[ISOLATED] sk (Hex): {Sui.Seal.Utils.ToHex(sk.ToBytes())}");

            // 3. İki Farklı Pairing İşlemini Yap
            Debug.Log("Pairing 1 (Encrypt gibi): GT.Pairing(gid_r, publicKey)");
            var pairingResultEncrypt = GT.Pairing(gid_r, publicKey);
            var encryptBytes = pairingResultEncrypt.ToBytes();

            Debug.Log("Pairing 2 (Decrypt gibi): GT.Pairing(sk, nonce)");
            var pairingResultDecrypt = GT.Pairing(sk, nonce);
            var decryptBytes = pairingResultDecrypt.ToBytes();

            // 4. Sonuçları Karşılaştır
            Debug.Log($"[ISOLATED] Pairing Encrypt Sonucu (Hex): {Sui.Seal.Utils.ToHex(encryptBytes)}");
            Debug.Log($"[ISOLATED] Pairing Decrypt Sonucu (Hex): {Sui.Seal.Utils.ToHex(decryptBytes)}");

            if (Sui.Seal.Utils.AreEqual(encryptBytes, decryptBytes))
            {
                Debug.Log("<color=green>İzole Pairing Tutarlılık Testi: BAŞARILI! Sonuçlar aynı.</color>");
            }
            else
            {
                Debug.LogError("<color=red>İzole Pairing Tutarlılık Testi: BAŞARISIZ! Sonuçlar farklı.</color>");
            }
        }
        catch (Exception e)
        {
            Debug.LogError("<color=red>--- İzole Pairing Testi BAŞARISIZ (Exception) ---</color>");
            Debug.LogException(e);
        }
        finally
        {
            // ETH modunu eski haline getir (eğer başta değiştirdiysen)
            // MCL_Imports.mclBn_setETHserialization(0); // VEYA 1
        }
    }

    private void TestSignPersonalMessage()
    {
        string correctSignature = "AOpTGvPzr2Enz5Dq+lUbZIQo0GOJon3Hx7PRL4K/57TqVnLrV9hoGSytoNydhXz9Y2Jb5WMLyDgCZx4HGJsh/QRYO2MQSeQ/0N31rNHbB0ecbpO7Wh+9Q+7m7tk2KiRtDA==";
        byte[] correctSignatureBytes = Convert.FromBase64String(correctSignature);
        
        SignatureWithBytes signatureWithBytes = _account.SignPersonalMessage(Encoding.UTF8.GetBytes("aaaaa"));
        Debug.Log("Correct => " + Sui.Seal.Utils.ToHex(correctSignatureBytes));
        Debug.Log("Actual => " + Sui.Seal.Utils.ToHex(Convert.FromBase64String(signatureWithBytes.Signature)));
    }
    private async Task DecryptTest(EncryptedObject encryptedObject)
    {
        var serverConfigs = new List<KeyServerConfig>
        {
            new KeyServerConfig { objectId = "0x73d05d62c18d9374e3ea529e8e0ed6161da1a141a94d3f76ae3fe4e99356db75", weight = 1 },
            new KeyServerConfig { objectId = "0xf5d14a81a982144ae441cd7d64b09027f116a468bd36e7eca494f750591623c8", weight = 1 },
        };
        var options = new SealClientOptions { ServerConfigs = serverConfigs, SuiClient = _suiClient };

        // 2. SealClient'ı başlat
        var client = new SealClient(options);
        await client.InitializeAsync();
        var sessionKey = await SessionKey.Create(_account.SuiAddress().ToHex(), _packageId, 10);
        SignatureWithBytes signatureWithBytes = _account.SignPersonalMessage(sessionKey.GetPersonalMessage());
        sessionKey.SetPersonalMessageSignature(signatureWithBytes.Signature);

        Serialization serialization = new Serialization();
        // ... (encryptedObjectBytes'ı Move objesinin field'ından çekme) ...
        encryptedObject.Serialize(serialization);
        var encryptedBytes = serialization.GetBytes();
        // === 3. TRANSACTION OLUŞTURMA ===
        // decrypt.ts örneğindeki gibi, şifre çözmek için de bir transaction gerekir.
        var tx_block = new TransactionBlock();

        tx_block.AddMoveCallTx
        (
            SuiMoveNormalizedStructType.FromStr($"{_packageId}::{_moduleName}::seal_approve"),
            new SerializableTypeTag[] { },
            new TransactionArgument[]
            {
                tx_block.AddPure(new OpenDive.BCS.Bytes(_policyId)),
                tx_block.AddObjectInput(_objectIdToDecrypt)
            }
        );
        //tx_block.SetSender(_account);
        var txBytes = await tx_block.Build(new BuildOptions(_suiClient, null, true, null));
        var txBytesForServer = txBytes.Skip(1).ToArray();

        var decryptOptions = new DecryptOptions
        {
            Data = encryptedBytes,
            SessionKey = sessionKey,
            TxBytes = txBytesForServer
        };
        byte[] decryptedMessageBytes = await client.Decrypt(decryptOptions);
        string decryptedMessage = Encoding.UTF8.GetString(decryptedMessageBytes);
        Debug.Log("Veri başarıyla çözüldü.");
        Debug.Log(decryptedMessage);
        Debug.Log("<color=green>SealClient Entegrasyon Testi: BAŞARILI</color>");
    }


    private async Task DecryptTest()
    {
        var serverConfigs = new List<KeyServerConfig>
        {
            new KeyServerConfig { objectId = "0x73d05d62c18d9374e3ea529e8e0ed6161da1a141a94d3f76ae3fe4e99356db75", weight = 1 },
            new KeyServerConfig { objectId = "0xf5d14a81a982144ae441cd7d64b09027f116a468bd36e7eca494f750591623c8", weight = 1 },
        };
        var options = new SealClientOptions { ServerConfigs = serverConfigs, SuiClient = _suiClient };

        // 2. SealClient'ı başlat
        var client = new SealClient(options);
        Debug.Log("SealClient başarıyla oluşturuldu.");
        await client.InitializeAsync();
        var sessionKey = await SessionKey.Create(_account.SuiAddress().ToHex(), _packageId, 10);
        SignatureWithBytes signatureWithBytes = _account.SignPersonalMessage(sessionKey.GetPersonalMessage());
        sessionKey.SetPersonalMessageSignature(signatureWithBytes.Signature);

        var response = await _suiClient.GetObjectAsync(new AccountAddress(_objectIdToDecrypt), new ObjectDataOptions() { ShowContent = true});
        var moveObject = (ParsedMoveObject)response.Result.Data.Content.ParsedData;
        // NOT: 'fields' içindeki 'url' ve 'pk' alan adlarının, Move objesindeki
        // alan adlarıyla eşleştiğinden emin olun.
        var fields = moveObject.Fields;
        // ... (encryptedObjectBytes'ı Move objesinin field'ından çekme) ...
        var encryptedBytes = (moveObject.Fields["data"] as JArray).Select(jv => (byte)jv).ToArray(); ; // Bu, EncryptedObject'in serialize edilmiş hali

        // === 3. TRANSACTION OLUŞTURMA ===
        // decrypt.ts örneğindeki gibi, şifre çözmek için de bir transaction gerekir.
        var tx_block = new TransactionBlock();

        tx_block.AddMoveCallTx
        (
            SuiMoveNormalizedStructType.FromStr($"{_packageId}::{_moduleName}::seal_approve"),
            new SerializableTypeTag[] { },
            new TransactionArgument[]
            {
                tx_block.AddPure(new OpenDive.BCS.Bytes(_policyId)),
                tx_block.AddObjectInput(_objectIdToDecrypt)
            }
        );
        //tx_block.SetSender(_account);
        var txBytes = await tx_block.Build(new BuildOptions(_suiClient, null, true, null));
        var txBytesForServer = txBytes.Skip(1).ToArray();
        Debug.Log("--------- C# DEBUG BAŞLANGIÇ ---------");
        Debug.Log($"C# txBytes UZUNLUK: {txBytesForServer.Length}");
        Debug.Log($"C# txBytes (HEX): {Sui.Seal.Utils.ToHex(txBytesForServer)}");
        Debug.Log("--------- C# DEBUG BİTİŞ ---------");

        var decryptOptions = new DecryptOptions
        {
            Data = encryptedBytes,
            SessionKey = sessionKey,
            TxBytes = txBytesForServer
            //TxBytes = txBytes
        };
        byte[] decryptedMessageBytes = await client.Decrypt(decryptOptions);
        string decryptedMessage = Encoding.UTF8.GetString(decryptedMessageBytes);
        Debug.Log("Veri başarıyla çözüldü.");
        Debug.Log(decryptedMessage);
        Debug.Log("<color=green>SealClient Entegrasyon Testi: BAŞARILI</color>");
    }

    private async Task<EncryptedObject> TestSealClientEncrypt()
    {
        // 1. Test için konfigürasyon oluştur
        var serverConfigs = new List<KeyServerConfig>
            {
                new KeyServerConfig { objectId = "0x73d05d62c18d9374e3ea529e8e0ed6161da1a141a94d3f76ae3fe4e99356db75", weight = 1 },
                new KeyServerConfig { objectId = "0xf5d14a81a982144ae441cd7d64b09027f116a468bd36e7eca494f750591623c8", weight = 1 },
            };
        var options = new SealClientOptions { ServerConfigs = serverConfigs, SuiClient = _suiClient };

        // 2. SealClient'ı başlat
        var client = new SealClient(options);
        Debug.Log("SealClient başarıyla oluşturuldu.");
        await client.InitializeAsync();

        //var sessionKey = await SessionKey.Create(_account.SuiAddress().ToHex(), _packageId, 10);

        // 3. Şifrelenecek veriyi hazırla
        string originalMessage = "myspecialmessage";
        int threshold = 2;
        var encryptOptions = new EncryptOptions
        {
            Threshold = threshold,
            PackageId = new AccountAddress(_packageId), // Hex string
            Id = Sui.Seal.Utils.ToHex(_policyId),         // Hex string
            Data = Encoding.UTF8.GetBytes(originalMessage)
        };

        // 4. Şifreleme (seal) işlemini AWAIT ile gerçekleştir
        var (key, encryptedData) = await client.Encrypt(encryptOptions);
        return encryptedData;
    }

    private async Task TestSealClient()
    {
        Debug.Log("--- SealClient Entegrasyon Testi Başlatılıyor ---");
        try
        {
            // 1. Test için konfigürasyon oluştur
            var serverConfigs = new List<KeyServerConfig>
            {
                new KeyServerConfig { objectId = "0x73d05d62c18d9374e3ea529e8e0ed6161da1a141a94d3f76ae3fe4e99356db75", weight = 1 },
                new KeyServerConfig { objectId = "0xf5d14a81a982144ae441cd7d64b09027f116a468bd36e7eca494f750591623c8", weight = 1 },
            };
            var options = new SealClientOptions { ServerConfigs = serverConfigs, SuiClient = _suiClient };

            // 2. SealClient'ı başlat
            var client = new SealClient(options);
            Debug.Log("SealClient başarıyla oluşturuldu.");
            await client.InitializeAsync();

            var sessionKey = await SessionKey.Create(_account.SuiAddress().ToHex(), _packageId, 10);

            // 3. Şifrelenecek veriyi hazırla
            string originalMessage = "myspecialmessage";
            int threshold = 2;
            var encryptOptions = new EncryptOptions
            {
                Threshold = threshold,
                PackageId = new AccountAddress(_packageId), // Hex string
                Id = Sui.Seal.Utils.ToHex(_policyId),         // Hex string
                Data = Encoding.UTF8.GetBytes(originalMessage)
            };

            // 4. Şifreleme (seal) işlemini AWAIT ile gerçekleştir
            var (key, encryptedData) = await client.Encrypt(encryptOptions);

            if (key == null || key.Length != 32 || encryptedData == null)
            {
                throw new Exception("Encrypt metodu beklenen çıktıyı vermedi.");
            }
            Debug.Log("SealClient.Encrypt başarıyla çalıştı.");
            Debug.Log($"Oluşturulan Simetrik Anahtar (Hex): {Sui.Seal.Utils.ToHex(key)}");
            Debug.Log($"Şifrelenmiş Nonce (Hex): {Sui.Seal.Utils.ToHex(encryptedData.encryptedShares.BonehFranklinBLS12381.nonce)}");

            Debug.Log("Şifreyi çözmek için sunuculardan parça anahtarlar alınıyor (simülasyon)...");
            string serializedEncryptedObject = JsonConvert.SerializeObject(encryptedData);
            byte[] encryptedBytes = Encoding.UTF8.GetBytes(serializedEncryptedObject);

            var decryptOptions = new DecryptOptions
            {
                Data = encryptedBytes,
                SessionKey = sessionKey,
                TxBytes = new byte[] { 1, 2, 3 } // Test için sahte işlem verisi
            };
            byte[] decryptedMessageBytes = await client.Decrypt(decryptOptions);
            string decryptedMessage = Encoding.UTF8.GetString(decryptedMessageBytes);
            Debug.Log("Veri başarıyla çözüldü.");

            // === 5. DOĞRULAMA (VERIFICATION) ===
            if (originalMessage != decryptedMessage)
            {
                throw new Exception($"Şifre çözme başarısız! Orijinal: '{originalMessage}', Çözülen: '{decryptedMessage}'");
            }
            

            Debug.Log("<color=green>SealClient Entegrasyon Testi: BAŞARILI</color>");
        }
        catch (Exception e)
        {
            Debug.LogError("<color=red>--- SealClient Entegrasyon Testi BAŞARISIZ ---</color>");
            Debug.LogException(e);
        }
    }

    private void TestUtils()
    {
        Debug.Log("--- Utils Testi Başlatılıyor ---");
        try
        {
            // FromHex ve ToHex testi
            string hex = "deadbeef";
            byte[] bytes = Sui.Seal.Utils.FromHex(hex);
            if (Sui.Seal.Utils.ToHex(bytes) != hex) throw new Exception("FromHex/ToHex başarısız.");
            Debug.Log("<color=green>Utils.FromHex/ToHex: BAŞARILI</color>");

            // Xor testi
            byte[] a = { 1, 2, 3 };
            byte[] b = { 4, 5, 6 };
            byte[] expectedXor = { 5, 7, 5 }; // 1^4=5, 2^5=7, 3^6=5
            if (!Sui.Seal.Utils.AreEqual(Sui.Seal.Utils.Xor(a, b), expectedXor)) throw new Exception("Utils.Xor başarısız.");
            Debug.Log("<color=green>Utils.Xor: BAŞARILI</color>");

            // Flatten testi
            byte[] c = { 7, 8 };
            byte[] expectedFlatten = { 1, 2, 3, 7, 8 };
            if (!Sui.Seal.Utils.AreEqual(Sui.Seal.Utils.Flatten(a, c), expectedFlatten)) throw new Exception("Utils.Flatten başarısız.");
            Debug.Log("<color=green>Utils.Flatten: BAŞARILI</color>");
        }
        catch (Exception e)
        {
            Debug.LogError("<color=red>--- Utils Testi BAŞARISIZ ---</color>");
            Debug.LogException(e);
        }
    }

    private void TestShamir()
    {
        Debug.Log("--- Shamir Testi Başlatılıyor ---");
        try
        {
            byte[] secret = Encoding.UTF8.GetBytes("Bu çok gizli bir sır!");
            int threshold = 3;
            int total = 5;

            // 1. Sırrı 5 parçaya böl (3 tanesi birleştirmek için yeterli)
            Share[] shares = Shamir.Split(secret, threshold, total);
            Debug.Log($"Shamir.Split: Sır {shares.Length} parçaya bölündü.");

            // 2. Parçalardan 3 tanesini (threshold kadar) alıp birleştirmeyi dene
            Share[] sharesToCombine = new Share[] { shares[0], shares[2], shares[4] };
            byte[] combinedSecret = Shamir.Combine(sharesToCombine);
            Debug.Log("Shamir.Combine: 3 parça birleştirildi.");

            // 3. Sonucu kontrol et
            if (!Sui.Seal.Utils.AreEqual(secret, combinedSecret))
            {
                throw new Exception("Birleştirilen sır, orijinal sır ile eşleşmiyor!");
            }

            Debug.Log("<color=green>Shamir.Split/Combine: BAŞARILI</color>");
        }
        catch (Exception e)
        {
            Debug.LogError("<color=red>--- Shamir Testi BAŞARISIZ ---</color>");
            Debug.LogException(e);
        }
    }

    void MCLInitTest()
    {
        Debug.Log("---------- BLS KÜTÜPHANE TESTİ BAŞLATILIYOR (Düzeltilmiş Versiyon) ----------");

        try
        {
            // Test 1: Bir Fr (Scalar) nesnesi oluşturup serileştirme
            // Bu satır çalıştığı anda, kütüphane arka planda kendini otomatik olarak başlatacaktır.
            var fr = new Fr();
            fr.SetInt(5678);

            // ToBytes() metodunu çağırarak C++ tarafında serileştirme yapılmasını sağlıyoruz.
            byte[] frBytes = fr.ToBytes();

            if (frBytes.Length == Fr.ByteSize) // Fr.ByteSize = 32
            {
                // Görsel doğrulama için byte dizisini hex string olarak yazdıralım.
                string hexValue = BitConverter.ToString(frBytes).Replace("-", "");
                Debug.Log($"<color=green>Fr (Scalar) testi BAŞARILI. Byte uzunluğu: {frBytes.Length}. Değer (Hex): {hexValue}</color>");
            }
            else
            {
                throw new Exception($"Fr testi başarısız. Beklenen byte uzunluğu {Fr.ByteSize} ama gelen {frBytes.Length}");
            }

            // Test 2: Bir G1 (Eğri Noktası) nesnesi oluşturup serileştirme
            var g1 = G1.Generator;
            byte[] g1Bytes = g1.ToBytes();

            if (g1Bytes.Length == G1.ByteSize) // G1.ByteSize = 48
            {
                string hexValue = BitConverter.ToString(g1Bytes).Replace("-", "");
                Debug.Log($"<color=green>G1 (Point) testi BAŞARILI. Byte uzunluğu: {g1Bytes.Length}. Değer (Hex): {hexValue}</color>");
            }
            else
            {
                throw new Exception($"G1 testi başarısız. Beklenen byte uzunluğu {G1.ByteSize} ama gelen {g1Bytes.Length}");
            }

            Debug.Log("<color=green><b>>>> TÜM TESTLER BAŞARILI! BLS KÜTÜPHANESİ DÜZGÜN ÇALIŞIYOR. <<<</b></color>");
        }
        catch (DllNotFoundException e)
        {
            Debug.LogError($"<color=red><b>>>> BLS TESTİ BAŞARISIZ OLDU: Native DLL bulunamadı! <<<</b></color>");
            Debug.LogError($"Hata Mesajı: '{e.Message}'. Lütfen 'mclbn384_256.dll' dosyasının 'Assets/Plugins/x86_64/' klasöründe olduğundan emin olun.");
            Debug.LogException(e);
        }
        catch (Exception e)
        {
            Debug.LogError($"<color=red><b>>>> BLS TESTİ BAŞARISIZ OLDU: Genel bir hata oluştu. <<<</b></color>");
            Debug.LogException(e);
        }

        Debug.Log("---------- BLS KÜTÜPHANE TESTİ TAMAMLANDI ----------");
    }

    private async Task TestDemRoundtrip()
    {
        Debug.Log("--- Adım 2: DEM (AesGcm & HmacCtr) Test Ediliyor ---");

        // Test için kullanılacak örnek veriler
        byte[] originalPlaintext = Encoding.UTF8.GetBytes("Bu gizli bir mesajdır!");
        byte[] associatedData = Encoding.UTF8.GetBytes("Ek doğrulama verisi");

        // --- AesGcm256 Testi ---
        Debug.Log("AES-GCM-256 Round-trip testi başlatılıyor...");
        var aesEncryptor = new AesGcm256(originalPlaintext, associatedData);
        byte[] aesKey = await aesEncryptor.GenerateKey();

        Ciphertext aesCiphertextObject = await aesEncryptor.Encrypt(aesKey);
        Aes256GcmCiphertext aesCiphertext = aesCiphertextObject as Aes256GcmCiphertext;
        if (aesCiphertext == null) throw new Exception("AES Encrypt metodu yanlış tipte ciphertext döndürdü.");

        byte[] decryptedAes = await AesGcm256.Decrypt(aesKey, aesCiphertext);

        if (!originalPlaintext.SequenceEqual(decryptedAes))
        {
            throw new Exception("AES-GCM-256 round-trip testi BAŞARISIZ! Orjinal veri ile çözülen veri eşleşmiyor.");
        }
        Debug.Log("<color=green>AES-GCM-256 Round-trip testi BAŞARILI!</color>");


        // --- Hmac256Ctr Testi ---
        Debug.Log("HMAC-256-CTR Round-trip testi başlatılıyor...");
        var hmacEncryptor = new Hmac256Ctr(originalPlaintext, associatedData);
        byte[] hmacKey = await hmacEncryptor.GenerateKey();

        Ciphertext hmacCiphertextObject = await hmacEncryptor.Encrypt(hmacKey);
        Hmac256CtrCiphertext hmacCiphertext = hmacCiphertextObject as Hmac256CtrCiphertext;
        if (hmacCiphertext == null) throw new Exception("HMAC Encrypt metodu yanlış tipte ciphertext döndürdü.");

        byte[] decryptedHmac = await Hmac256Ctr.Decrypt(hmacKey, hmacCiphertext);

        if (!originalPlaintext.SequenceEqual(decryptedHmac))
        {
            throw new Exception("HMAC-256-CTR round-trip testi BAŞARISIZ! Orjinal veri ile çözülen veri eşleşmiyor.");
        }
        Debug.Log("<color=green>HMAC-256-CTR Round-trip testi BAŞARILI!</color>");
    }
}