using OpenDive.BCS;
// BouncyCastle için GEREKLİ using direktifleri
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Macs;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

// Bu namespace'i kendi projenize göre düzenleyin.
// namespace Sui.Seal.Models 
// {

namespace Sui.Seal
{
    public abstract class Ciphertext : ISerializable
    {
        public abstract void Serialize(Serialization serializer);

        public static ISerializable Deserialize(Deserialization deserializer)
        {
            // Önce varyant indeksini oku.
            var variantIndex = deserializer.DeserializeU8().Value;
            switch (variantIndex)
            {
                case 0:
                    return Aes256GcmCiphertext.Deserialize(deserializer);
                // case 1:
                //     return Hmac256CtrCiphertext.Deserialize(deserializer);
                default:
                    throw new Exception("Unknown Ciphertext variant");
            }
        }
    }

    public class Aes256GcmCiphertext : Ciphertext
    {
        public byte[] blob { get; set; }
        public byte[]? aad { get; set; }

        public override void Serialize(Serialization serializer)
        {
            // 1. Enum varyant indeksini yaz (Aes256Gcm için 0)
            serializer.SerializeU8(0);

            // 2. Struct'ın alanlarını sırayla serialize et
            serializer.Serialize(new Bytes(this.blob));

            // bcs.option(bcs.vector(bcs.u8())) serileştirmesi:
            if (this.aad != null)
            {
                serializer.SerializeU8(1); // Some(1)
                serializer.Serialize(new Bytes(this.aad));
            }
            else
            {
                serializer.SerializeU8(0); // None(0)
            }
        }

        public static new ISerializable Deserialize(Deserialization deserializer)
        {
            var obj = new Aes256GcmCiphertext();


            obj.blob = deserializer.ToBytes();

            var hasAad = deserializer.DeserializeU8().Value; // Option'ı oku (0 veya 1)
            if (hasAad == 1)
            {
                obj.aad = deserializer.ToBytes();
            }
            return obj;
        }
    }

    public class Hmac256CtrCiphertext : Ciphertext
    {
        public byte[] blob { get; set; }
        public byte[] mac { get; set; }
        public byte[]? aad { get; set; }

        public override void Serialize(Serialization serializer)
        {
            // 1. Enum varyant indeksini yaz (Hmac256Ctr için 1)
            serializer.SerializeU32AsUleb128(1);

            // 2. Struct'ın alanlarını sırayla serialize et
            serializer.Serialize(new Bytes(this.blob));

            // bcs.option(bcs.vector(bcs.u8())) serileştirmesi:
            if (this.aad != null)
            {
                serializer.SerializeU8(1); // Some(1)
                serializer.Serialize(new Bytes(this.aad));
            }
            else
            {
                serializer.SerializeU8(0); // None(0)
            }
        }
    }

    public interface IEncryptionInput
    {
        byte[] Plaintext { get; }
        byte[] Aad { get; }
        Task<Ciphertext> Encrypt(byte[] key);
        Task<byte[]> GenerateKey();
    }

    public static class DemUtils
    {
        public static readonly byte[] Iv =
        {
        138, 55, 153, 253, 198, 46, 121, 219, 160, 128, 89, 7, 214, 156, 148, 220
        };

        public static Task<byte[]> GenerateAesKey()
        {
            byte[] keyData = new byte[32];
            RandomNumberGenerator.Fill(keyData);
            //byte[] keyData = {1,1,1,1,1,1,1,1, 1, 1, 1, 1, 1, 1, 1, 1 , 1, 1, 1, 1, 1, 1, 1, 1 , 1, 1, 1, 1, 1, 1, 1, 1 };
            return Task.FromResult(keyData);
        }

        public static byte[] HmacSha3_256(byte[] key, byte[] message)
        {
            var hmac = new HMac(new Sha3Digest(256));
            hmac.Init(new KeyParameter(key));
            hmac.BlockUpdate(message, 0, message.Length);
            var output = new byte[hmac.GetMacSize()];
            hmac.DoFinal(output, 0);
            return output;
        }
    }


    // --- AesGcm256 SINIFININ DÜZELTİLMİŞ VERSİYONU ---
    public class AesGcm256 : IEncryptionInput
    {
        public byte[] Plaintext { get; }
        public byte[] Aad { get; }

        public AesGcm256(byte[] msg, byte[] aad)
        {
            Plaintext = msg;
            Aad = aad;
        }

        public Task<byte[]> GenerateKey() => DemUtils.GenerateAesKey();

        public Task<Ciphertext> Encrypt(byte[] key)
        {
            if (key.Length != 32) throw new ArgumentException("Key must be 32 bytes");

            var cipher = new GcmBlockCipher(new AesEngine());
            var parameters = new AeadParameters(new KeyParameter(key), 128, DemUtils.Iv, Aad);
            cipher.Init(true, parameters); // true = encryption

            // Çıktı buffer'ı, şifreli metin + 16 byte'lık tag için yeterli boyutta olmalı.
            var cipherText = new byte[cipher.GetOutputSize(Plaintext.Length)];
            int len = cipher.ProcessBytes(Plaintext, 0, Plaintext.Length, cipherText, 0);
            // DoFinal, son bloğu işler ve tag'i sona ekler.
            cipher.DoFinal(cipherText, len);

            var result = new Aes256GcmCiphertext
            {
                blob = cipherText,
                aad = Aad
            };

            return Task.FromResult<Ciphertext>(result);
        }

        public static Task<byte[]> Decrypt(byte[] key, Aes256GcmCiphertext ciphertext)
        {
            if (key.Length != 32) throw new ArgumentException("Key must be 32 bytes");

            var cipher = new GcmBlockCipher(new AesEngine());
            var parameters = new AeadParameters(new KeyParameter(key), 128, DemUtils.Iv, ciphertext.aad);
            cipher.Init(false, parameters); // false = decryption

            // Çıktı buffer'ı, en fazla şifreli metin (blob) kadar olabilir.
            var outputBuffer = new byte[cipher.GetOutputSize(ciphertext.blob.Length)];

            try
            {
                // ProcessBytes'tan gelen byte sayısını alıyoruz.
                int len = cipher.ProcessBytes(ciphertext.blob, 0, ciphertext.blob.Length, outputBuffer, 0);

                // DoFinal'dan gelen byte sayısını da ekliyoruz. Bu, tag kontrolünü yapar.
                len += cipher.DoFinal(outputBuffer, len);

                // *** İŞTE DÜZELTME BURADA ***
                // Buffer'ın tamamını değil, sadece içine veri yazılan kısmını alıyoruz.
                var finalPlaintext = new byte[len];
                Array.Copy(outputBuffer, 0, finalPlaintext, 0, len);

                return Task.FromResult(finalPlaintext);
            }
            catch (InvalidCipherTextException e)
            {
                throw new CryptographicException("AES-GCM decryption failed: Invalid MAC (tag).", e);
            }
        }
    }


    public class Hmac256Ctr : IEncryptionInput
    {
        private static readonly byte[] EncryptionKeyTag = Encoding.UTF8.GetBytes("HMAC-CTR-ENC");
        private static readonly byte[] MacKeyTag = Encoding.UTF8.GetBytes("HMAC-CTR-MAC");

        public byte[] Plaintext { get; }
        public byte[] Aad { get; }

        public Hmac256Ctr(byte[] msg, byte[] aad)
        {
            Plaintext = msg;
            Aad = aad;
        }

        public Task<byte[]> GenerateKey() => DemUtils.GenerateAesKey();

        public Task<Ciphertext> Encrypt(byte[] key)
        {
            var blob = EncryptInCtrMode(key, Plaintext);
            var mac = ComputeMac(key, Aad, blob);

            var result = new Hmac256CtrCiphertext
            {
                blob = blob,
                mac = mac,
                aad = Aad
            };

            return Task.FromResult<Ciphertext>(result);
        }

        public static Task<byte[]> Decrypt(byte[] key, Hmac256CtrCiphertext ciphertext)
        {
            if (key.Length != 32) throw new ArgumentException("Key must be 32 bytes");

            var calculatedMac = ComputeMac(key, ciphertext.aad ?? Array.Empty<byte>(), ciphertext.blob);

            if (!CryptographicOperations.FixedTimeEquals(calculatedMac, ciphertext.mac))
            {
                throw new CryptographicException("Invalid MAC");
            }

            return Task.FromResult(EncryptInCtrMode(key, ciphertext.blob));
        }

        private static byte[] ComputeMac(byte[] key, byte[] aad, byte[] ciphertext)
        {
            byte[] aadLength = BitConverter.GetBytes((ulong)aad.Length);
            if (!BitConverter.IsLittleEndian) Array.Reverse(aadLength);

            byte[] macInput = MacKeyTag.Concat(aadLength).Concat(aad).Concat(ciphertext).ToArray();
            return DemUtils.HmacSha3_256(key, macInput);
        }

        private static byte[] EncryptInCtrMode(byte[] key, byte[] msg)
        {
            const int blockSize = 32;
            var result = new byte[msg.Length];

            for (int i = 0; i * blockSize < msg.Length; i++)
            {
                int currentBlockOffset = i * blockSize;
                int currentBlockSize = Math.Min(blockSize, msg.Length - currentBlockOffset);
                var block = msg.AsSpan().Slice(currentBlockOffset, currentBlockSize);

                byte[] counterBytes = BitConverter.GetBytes((ulong)i);
                if (!BitConverter.IsLittleEndian) Array.Reverse(counterBytes);

                byte[] maskInput = EncryptionKeyTag.Concat(counterBytes).ToArray();
                byte[] mask = DemUtils.HmacSha3_256(key, maskInput);

                for (int j = 0; j < block.Length; j++)
                {
                    result[currentBlockOffset + j] = (byte)(block[j] ^ mask[j]);
                }
            }
            return result;
        }
    }
}
