using System;
using System.Numerics;
using Unity.Plastic.Newtonsoft.Json;
using OpenDive.BCS;
using Sui.Cryptography;

namespace Sui.ZKLogin
{
    /// <summary>
    /// TODO: See if it can be extended from the core Signature class.
    /// A ZKLogin Signature is composed of 3 things
    ///     1) "inputs" -- this is the ZKProof object + addressSeed
    ///     2) "maxEpoch" -- the max epoch
    ///     3) "userSignature"
    /// https://github.com/MystenLabs/ts-sdks/blob/a4bd7214e6090c2da2d325807073404033758b13/packages/typescript/src/zklogin/bcs.ts#L7
    /// </summary>
    [JsonObject]
    public class ZkLoginSignature : ISerializable
    {
        [JsonProperty("inputs")]
        public Inputs Inputs { get; set; }

        [JsonProperty("maxEpoch")]
        public ulong MaxEpoch { get; set; }

        [JsonProperty("userSignature")]
        public byte[] UserSignature { get; set; } // bcs.vector(bcs.u8()),

        public void Serialize(Serialization serializer)
        {
            serializer.Serialize(Inputs);
            serializer.SerializeU64(MaxEpoch);
            serializer.SerializeBytes(UserSignature);
        }

        public static ISerializable Deserialize(Deserialization deserializer)
        {
            Inputs inputs = (Inputs)Inputs.Deserialize(deserializer);
            ulong maxEpoch = deserializer.DeserializeU64().Value;
            byte[] userSignature =  ((Bytes) Bytes.Deserialize(deserializer)).Values;

            ZkLoginSignature sig = new ZkLoginSignature();
            sig.Inputs = inputs;
            sig.MaxEpoch = maxEpoch;
            sig.UserSignature = userSignature;
            return sig;
        }

        public static byte[] GetZkLoginSignatureBytes(
            Inputs inputs, ulong maxEpoch, byte[] userSignature)
        {
            ZkLoginSignature sig = new ZkLoginSignature();
            sig.Inputs = inputs;
            sig.MaxEpoch = maxEpoch;
            sig.UserSignature = userSignature;

            Serialization ser = new Serialization();
            sig.Serialize(ser);
            byte[] sigBytes = ser.GetBytes();
            return sigBytes;
        }

        public static string GetZkLoginSignature(
            Inputs inputs, ulong maxEpoch, byte[] userSignature)
        {
            byte[] bytes = GetZkLoginSignatureBytes(inputs, maxEpoch, userSignature);
            byte[] signatureBytes = new byte[bytes.Length + 1];
            signatureBytes[0] = SignatureSchemeToFlag.ZkLogin;
            Buffer.BlockCopy(bytes, 0, signatureBytes, 1, bytes.Length);
            return Convert.ToBase64String(signatureBytes);
        }

        /// <summary>
        /// Create a ZKLogin Signature from a Base64 signature
        /// </summary>
        /// <param name="strSignature"></param>
        /// <returns></returns>
        public ZkLoginSignature ParseZkLoginSignature(string strSignature)
        {
            byte[] signatureBytes = Signature.FromBase64(strSignature);
            Deserialization deserializer = new Deserialization(signatureBytes);
            return (ZkLoginSignature)Deserialize(deserializer);
        }

        public ZkLoginSignature ParseZkLoginSignature(byte[] signatureBytes)
        {
            Deserialization deserializer = new Deserialization(signatureBytes);
            return (ZkLoginSignature)ZkLoginSignature.Deserialize(deserializer);
        }
    }

    [JsonObject]
    public class Inputs : ISerializable
    {
        [JsonProperty("proofPoints")]
        public ProofPoints ProofPoints { get; set; }

        [JsonProperty("issBase64Details")]
        public ZkLoginSignatureInputsClaim IssBase64Details { get; set; }

        [JsonProperty("headerBase64")]
        public string HeaderBase64 { get; set; }

        [JsonProperty("addressSeed")]
        public string AddressSeed { get; set; }

        public static ISerializable Deserialize(Deserialization deserializer)
        {
            ProofPoints proofPoints = (ProofPoints)ProofPoints.Deserialize(deserializer);
            ZkLoginSignatureInputsClaim IssBase64Details = (ZkLoginSignatureInputsClaim)ZkLoginSignatureInputsClaim.Deserialize(deserializer);
            string headerBase64 = deserializer.DeserializeString().ToString();
            string addressSeed = deserializer.DeserializeString().ToString();

            Inputs inputs = new Inputs();
            inputs.ProofPoints = proofPoints;
            inputs.IssBase64Details = IssBase64Details;
            inputs.HeaderBase64 = headerBase64;
            inputs.AddressSeed = addressSeed;

            return inputs;
        }

        public void Serialize(Serialization serializer)
        {
            serializer.Serialize(ProofPoints);
            serializer.Serialize(IssBase64Details);
            serializer.SerializeString(HeaderBase64);
            serializer.SerializeString(AddressSeed);
        }
    }

    /// <summary>
    /// Proof Points Object
    /// <code>
    /// "proofPoints": {
    ///     "a": [
    ///         "4454772426092695461025526140941405399454899318912669366419304644685541372991",
    ///         "8984360732286567349467607604973692590498659599949043946802050756343996655257",
    ///         "1"
    ///     ],
    ///     "b": [
    ///         [
    ///             "5939285442433252238643447962718695316515939493003067067660140419052394067374",
    ///             "10968346574805666769486235627786455340980429660905317386693376120772062842737"
    ///         ],
    ///         [
    ///             "7437397859167857539066360925390204696863004950945870991330674766958779783813",
    ///             "12539115227231208276932613867571779044478503094001957273918169691699545769778"
    ///         ],
    ///         [
    ///             "1",
    ///             "0"
    ///         ]
    ///     ],
    ///     "c": [
    ///         "1673409057821404283328916517110901257722439392303041984587615329246021137745",
    ///         "18444563522308523770311478643556861027404946545454450865686689352122468253916",
    ///         "1"
    ///     ]
    /// },
    /// </code>
    /// </summary>
    [JsonObject]
    public class ProofPoints : ISerializable
    {
        // Sequence input = new Sequence(new string[] { "a", "abc", "def", "ghi" }.ToList().Select(str => new BString(str)).ToArray());
        [JsonProperty("a")]
        public Sequence A { get; set; } // a: bcs.vector(bcs.string())

        [JsonProperty("b")]
        public Sequence B { get; set; } // b: bcs.vector(bcs.vector(bcs.string())),

        [JsonProperty("c")]
        public Sequence C { get; set; } // c: bcs.vector(bcs.string()),

        public static ISerializable Deserialize(Deserialization deserializer)
        {
            ProofPoints proofPoints = new ProofPoints();
            proofPoints.A = deserializer.DeserializeSequence(typeof(BString));
            proofPoints.B = deserializer.DeserializeSequence(typeof(BString[]));
            proofPoints.C = deserializer.DeserializeSequence(typeof(BString));
            return proofPoints;
        }

        public void Serialize(Serialization serializer)
        {
            serializer.Serialize(A);
            serializer.Serialize(B);
            serializer.Serialize(C);
        }
    }

    /// <summary>
    ///
    /// <code>
    /// "issBase64Details": {
    ///     "value": "yJpc3MiOiJodHRwczovL2FjY291bnRzLmdvb2dsZS5jb20iLC",
    ///     "indexMod4": 1
    /// },
    /// </code>
    /// </summary>
    [JsonObject]
    public class ZkLoginSignatureInputsClaim : ISerializable
    {
        [JsonProperty("value")]
        public string Value { get; set; }

        [JsonProperty("indexMod4")]
        public byte IndexMod4 { get; set; }

        public static ISerializable Deserialize(Deserialization deserializer)
        {
            ZkLoginSignatureInputsClaim issBase64Details = new ZkLoginSignatureInputsClaim();
            issBase64Details.Value = deserializer.DeserializeString().ToString();
            issBase64Details.IndexMod4 = deserializer.DeserializeU8().Value;
            return issBase64Details;
        }

        public void Serialize(Serialization serializer)
        {
            serializer.SerializeString(Value);
            serializer.SerializeU8(IndexMod4);
        }
    }
}