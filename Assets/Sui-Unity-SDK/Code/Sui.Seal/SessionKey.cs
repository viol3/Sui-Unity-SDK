using Chaos.NaCl;
using MCL.BLS12_381.Net; // Kendi kripto kütüphanemiz
using Sui.Cryptography.Ed25519;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
// Ed25519 ve ElGamal için uygun kütüphanelerin using'leri buraya eklenecek
// Örnek: using Chaos.NaCl; // Ed25519 için popüler bir kütüphane

namespace Sui.Seal
{
    // TypeScript'teki Certificate tipinin karşılığı
    public class Certificate
    {
        public string user { get; set; }
        public string session_vk { get; set; } // Base64
        public ulong creation_time { get; set; } // JS'deki Date.now() milisaniye, C#'ta ulong
        public int ttl_min { get; set; }
        public string signature { get; set; } // Base64
        public string mvr_name { get; set; }
    }

    public class SignedRequestParams
    {
        public byte[] EncKey { get; set; }
        public byte[] EncKeyPk { get; set; }
        public byte[] EncVerificationKey { get; set; }
        public string RequestSignature { get; set; } // Base64 formatında
    }

    public class SessionKey
    {
        private readonly string address;
        private readonly string packageId;
        private readonly string mvrName;
        private ulong creationTimeMs;
        private readonly int ttlMin;

        private PrivateKey sessionKey;

        private string personalMessageSignature;
        // private readonly ISigner signer; // ISigner arayüzü daha sonra tanımlanabilir.
        // private readonly ISuiClient suiClient; // ISuiClient arayüzü daha sonra tanımlanabilir.

        // Constructor'ı private yapıyoruz, çünkü nesne 'Create' metoduyla oluşturulacak.
        private SessionKey(string address, string packageId, int ttlMin, string mvrName = null)
        {
            // Girdi kontrolleri
            if (ttlMin > 30 || ttlMin < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(ttlMin), "Invalid TTL, must be between 1 and 30");
            }

            this.address = address;
            this.packageId = packageId;
            this.mvrName = mvrName;
            this.ttlMin = ttlMin;

            // Yeni bir Ed25519 anahtar çifti oluştur.
            this.sessionKey = new PrivateKey(); // Bu metodu yazmamız gerekecek.

            // C#'ta Date.now() karşılığı.
            this.creationTimeMs = (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }

        // static async create
        public static async Task<SessionKey> Create(string address, string packageId, int ttlMin, string mvrName = null)
        {
            // TypeScript'teki package versiyon kontrolünü şimdilik atlıyoruz.
            // await suiClient.core.getObject(...)

            // await/async yapısını korumak için Task.FromResult kullanıyoruz.
            return await Task.FromResult(new SessionKey(address, packageId, ttlMin, mvrName));
        }

        public bool IsExpired()
        {
            // Allow 10 seconds for clock skew
            return creationTimeMs + (ulong)(ttlMin * 60 * 1000) - 10000 < (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }

        public string GetPackageId() => packageId;

        public byte[] GetPersonalMessage()
        {
            var creationTimeUtc = DateTimeOffset.FromUnixTimeMilliseconds((long)creationTimeMs).ToString("yyyy-MM-dd HH:mm:ss") + " UTC";

            var sessionKeyPkBase64 = Convert.ToBase64String(this.sessionKey.PublicKey().KeyBytes); // Bu metodu yazmamız gerekecek

            string message = $"Accessing keys of package {this.packageId} for {this.ttlMin} mins from {creationTimeUtc}, session key {sessionKeyPkBase64}";
            return Encoding.UTF8.GetBytes(message);
        }

        public void SetPersonalMessageSignature(string personalMessageSignature)
        {
            this.personalMessageSignature = personalMessageSignature;
        }

        public Certificate GetCertificate()
        {
            return new Certificate
            {
                user = this.address,
                session_vk = Convert.ToBase64String(this.sessionKey.PublicKey().KeyBytes),
                creation_time = this.creationTimeMs,
                ttl_min = this.ttlMin,
                signature = this.personalMessageSignature,
                mvr_name = this.mvrName,
            };
        }

        public SignedRequestParams CreateRequestParams(byte[] txBytes)
        {
            if (this.IsExpired())
            {
                throw new Exception("Expired SessionKey");
            }

            // ElGamal anahtarlarını oluştur. Bu fonksiyonları ElGamal.cs'de yazacağız.
            var encKey = ElGamal.GenerateSecretKey();
            var encKeyPk = ElGamal.ToPublicKey(encKey);
            var encVerificationKey = ElGamal.ToVerificationKey(encKey);

            // İmzalanacak mesajı oluştur (BCS formatı şimdilik atlanmıştır).
            // RequestFormat.serialize(...)
            var msgToSign = Utils.Flatten(txBytes, encKeyPk, encVerificationKey);

            // Mesajı sessionKey ile imzala.
            var requestSignature = this.sessionKey.Sign(msgToSign); // Bu metodu yazmamız gerekecek

            // TypeScript'teki gibi anonim bir obje döndürüyoruz.
            // Bunu daha sonra güçlü tipli bir sınıfa dönüştürebiliriz.
            return new SignedRequestParams
            {
                EncKey = encKey,
                EncKeyPk = encKeyPk,
                EncVerificationKey = encVerificationKey,
                RequestSignature = Convert.ToBase64String(requestSignature.SignatureBytes),
            };
        }
    }

}