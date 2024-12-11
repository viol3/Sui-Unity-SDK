using System.Numerics;
using NUnit.Framework;
using Sui.Cryptography.Ed25519;
using Sui.ZKLogin.SDK;
using UnityEngine;

namespace Sui.Tests.ZkLogin
{
    [TestFixture]
    public class NonceTest : MonoBehaviour
    {
        int maxEpoch = 26;
        string pkBase64 = "lHezoWY/4pRWe+iajFHw62hQjmVQ6wlL+C8CJxw4bY0=";
        string pubKeyBase64Expected = "/CTTrykDrvNxtl0WfBo3Q+H/L9VLJAzwXAJew6cMP70=";

        //BigInteger randomness = 91593735651025872471886891147594672981; // too long
        //BigInteger randomness = 9159373565102587247;
        BigInteger randomness = BigInteger.Parse("91593735651025872471886891147594672981");

        /// <summary>
        /// PrivateKey:
        ///     {"schema":"ED25519","privateKey":"lHezoWY/4pRWe+iajFHw62hQjmVQ6wlL+C8CJxw4bY0="}
        /// PublicKey:
        ///     "/CTTrykDrvNxtl0WfBo3Q+H/L9VLJAzwXAJew6cMP70="
        /// </summary>
        [Test]
        public void GenerateNonceTest()
        {
            PrivateKey pk = new PrivateKey(pkBase64);
            string pubKey = pk.PublicKey().KeyBase64;

            Assert.AreEqual(pubKeyBase64Expected, pubKey);

            string nonce = NonceGenerator.GenerateNonce(
                (PublicKey)pk.PublicKey(),
                maxEpoch,
                randomness
            );

            string nonceExpected = "LSLuhEjHLSeRvyI26wfPQSjYNbc";
            Debug.Log("RAND: LOG: " + randomness);
            Assert.AreEqual(nonceExpected, nonce, "RAND: " + randomness.ToString());
        }

        [Test]
        public void ToBigIntBETest()
        {
            byte[] bytes = { };
            BigInteger toBigintBE = NonceGenerator.ToBigIntBE(bytes);
            Assert.AreEqual(null, bytes, "BigInteger Valye: " + "[IMPLEMENT]");
        }
    }
}