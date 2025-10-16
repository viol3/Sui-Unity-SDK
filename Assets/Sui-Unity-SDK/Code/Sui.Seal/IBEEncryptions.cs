using OpenDive.BCS;
using System.Linq;
using System.Numerics;

namespace Sui.Seal
{
    public class BonehFranklinBLS12381 : ISerializable
    {
        public byte[] nonce;
        public byte[][] encryptedShares;
        public byte[] encryptedRandomness;

        public void Serialize(Serialization serializer)
        {
            // Struct'ın alanlarını sırayla serialize et
            serializer.Serialize(new Bytes(this.nonce));
            serializer.Serialize(this.encryptedShares.Select(s => new Bytes(s)).ToArray());
            serializer.Serialize(new Bytes(this.encryptedRandomness));
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
    }
}