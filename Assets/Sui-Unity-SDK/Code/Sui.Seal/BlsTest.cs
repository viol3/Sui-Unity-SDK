using MCL.BLS12_381.Net; // Kütüphanenin C# sınıflarını kullanabilmek için
using System;            // Exception ve BitConverter kullanmak için
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class BlsTest : MonoBehaviour
{
    async void Start()
    {
        await TestDemRoundtrip();
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