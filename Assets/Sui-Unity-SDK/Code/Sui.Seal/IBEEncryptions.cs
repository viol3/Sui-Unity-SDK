using MCL.BLS12_381.Net;
using OpenDive.BCS;
using System;
using System.Linq;
using System.Numerics;

namespace Sui.Seal
{
    public class BonehFranklinBLS12381 : ISerializable
    {
        public byte[] nonce;
        public byte[][] encryptedShares;
        public byte[] encryptedRandomness;

        /// <summary>
        /// Verilen parça sır anahtarının (usk), belirtilen kimlik (id) ve
        /// sunucu genel anahtarı (pk) için geçerli olup olmadığını doğrular.
        /// Denklem: e(usk, G2.Generator) == e(H(id), pk)
        /// </summary>

        public void Serialize(Serialization serializer)
        {
            serializer.SerializeFixedBytes(this.nonce);
            serializer.SerializeU8((byte)encryptedShares.Length);
            for (int i = 0; i < encryptedShares.Length; i++)
            {
                serializer.SerializeFixedBytes(encryptedShares[i]);
            }
            
            serializer.SerializeFixedBytes(this.encryptedRandomness);
        }

        public static ISerializable Deserialize(Deserialization deserializer)
        {
            var obj = new BonehFranklinBLS12381();
            // Varsayım: nonce her zaman 96 byte, encryptedRandomness her zaman 32 byte.
            obj.nonce = deserializer.FixedBytes(96);
            var sharesLen = deserializer.DeserializeU8().Value;
            obj.encryptedShares = new byte[sharesLen][];
            for (int i = 0; i < sharesLen; i++)
            {
                // Varsayım: Her bir pay her zaman 32 byte.
                obj.encryptedShares[i] = deserializer.FixedBytes(32);
            }

            obj.encryptedRandomness = deserializer.FixedBytes(32);
            return obj;
        }
    }

    public class IBEEncryptions : ISerializable
    {
        public BonehFranklinBLS12381 BonehFranklinBLS12381;

        public void Serialize(Serialization serializer)
        {
            // IBEEncryptions bir enum olduğu için, varyant indeksini yazıyoruz.
            // TypeScript tanımında tek seçenek olduğu için indeks 0'dır.
            serializer.SerializeU8(0);

            // İçindeki nesneyi serialize ediyoruz.
            this.BonehFranklinBLS12381.Serialize(serializer);
        }

        public static ISerializable Deserialize(Deserialization deserializer)
        {
            var obj = new IBEEncryptions();
            var variantIndex = deserializer.DeserializeU8().Value;
            if (variantIndex == 0)
            {
                obj.BonehFranklinBLS12381 = (BonehFranklinBLS12381)BonehFranklinBLS12381.Deserialize(deserializer);
            }
            else
            {
                throw new Exception("Unknown IBEEncryptions variant");
            }
            return obj;
        }
    }
}