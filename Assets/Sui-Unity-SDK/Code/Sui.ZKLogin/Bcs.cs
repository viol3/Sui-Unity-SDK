using System.Numerics;
using Newtonsoft.Json;
using OpenDive.BCS;

namespace Sui.ZKLogin
{
    [JsonObject]
    public class ZkLoginSignature: ISerializable
    {
        [JsonProperty("inputs")]
        public Inputs Inputs { get; set; }

        [JsonProperty("maxEpoch")]
        public BigInteger MaxEpoch { get; set; }

        [JsonProperty("userSignature")]
        public BigInteger UserSignature { get; set; }

        public void Serialize(Serialization serializer)
        {
            throw new System.NotImplementedException();
        }

        public static ISerializable Deserialize(Deserialization deserializer)
        {
            throw new System.NotImplementedException();
        }
    }

    [JsonObject]
    public class Inputs
    {
        [JsonProperty("proofPoints")]
        public ProofPoints proofPoints { get; set; }

        [JsonProperty("issBase64Details")]
        public ZkLoginSignatureInputsClaim IssBase64Details { get; set; }

        [JsonProperty("headerBase64")]
        public BigInteger HeaderBase64 { get; set; }

        [JsonProperty("addressSeed")]
        public BigInteger AddressSeed { get; set; }
    }

    [JsonObject]
    public class ProofPoints
    {
        // Sequence input = new Sequence(new string[] { "a", "abc", "def", "ghi" }.ToList().Select(str => new BString(str)).ToArray());
        [JsonProperty("a")]
        public Sequence A { get; set; } // a: bcs.vector(bcs.string())

        [JsonProperty("b")]
        public Sequence B { get; set; } // b: bcs.vector(bcs.vector(bcs.string())),

        [JsonProperty("c")]
        public Sequence C { get; set; } // c: bcs.vector(bcs.string()),
    }

    [JsonObject]
    public class ZkLoginSignatureInputsClaim
    {
        [JsonProperty("value")]
        public BigInteger Value { get; set; }

        [JsonProperty("indexMod4")]
        public BigInteger IndexMod4 { get; set; }
    }

    public static class Bcs
    {
    }
}
