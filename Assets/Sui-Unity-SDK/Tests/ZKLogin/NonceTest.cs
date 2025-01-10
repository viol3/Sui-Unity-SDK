using System;
using System.Numerics;
using NUnit.Framework;
using Org.BouncyCastle.Security;
using Sui.Cryptography.Ed25519;
using Sui.ZKLogin.SDK;
using UnityEngine;

namespace Sui.Tests.ZkLogin
{
    [TestFixture]
    public class NonceTest : MonoBehaviour
    {
        string pkBase64 = "suiprivkey1qqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqq509duq";
        string pubKeyBase64Expected = "O2onvM62pC1io6jQKm8Nc2UyFXcd4kOmOsBIoYtZ2ik=";

        /// <summary>
        /// PrivateKey:
        ///     {"schema":"ED25519","privateKey":"lHezoWY/4pRWe+iajFHw62hQjmVQ6wlL+C8CJxw4bY0="}
        /// PublicKey:
        ///     "/CTTrykDrvNxtl0WfBo3Q+H/L9VLJAzwXAJew6cMP70="
        /// </summary>
        // Test Case 1: Empty private key, small BigInteger
        [Test]
        public void GenerateNonceTest_1()
        {
            PrivateKey pk = new PrivateKey(new byte[32]);
            string pubKey = pk.PublicKey().KeyBase64;
            Assert.AreEqual(pubKeyBase64Expected, pubKey);

            int maxEpoch = 0;
            BigInteger randomness = new BigInteger(91593735651025872);

            string nonce = NonceGenerator.GenerateNonce(
                (PublicKey)pk.PublicKey(),
                maxEpoch,
                randomness
            );

            string nonceExpected = "SoeqFjFH8qNU8t4z1oh2aWp1jJI"; 
            Assert.AreEqual(nonceExpected, nonce, "RAND: " + randomness.ToString());
        }

        [Test]
        // Test Case 2: Empty private key, small BigInteger
        public void GenerateNonceTest_2()
        {
            PrivateKey pk = new PrivateKey(new byte[32]);
            string pubKey = pk.PublicKey().KeyBase64;

            Assert.AreEqual(pubKeyBase64Expected, pubKey);

            int maxEpoch = 0;
            BigInteger randomness = new BigInteger(915937356510258724);

            string nonce = NonceGenerator.GenerateNonce(
                (PublicKey)pk.PublicKey(),
                maxEpoch,
                randomness
            );

            string nonceExpected = "smaC7ju0NrM0birjNuUhZspaBOQ";
            Debug.Log("RAND: LOG: " + randomness);

            Assert.AreEqual(nonceExpected, nonce, "RAND: " + randomness.ToString());
        }

        [Test]
        // Test Case 3: Empty private key, large BigInteger
        public void GenerateNonceTest_3()
        {
            PrivateKey pk = new PrivateKey(new byte[32]);
            string pubKey = pk.PublicKey().KeyBase64;
            Assert.AreEqual(pubKeyBase64Expected, pubKey);

            int maxEpoch = 0;
            BigInteger randomness = BigInteger.Parse("144441570523660387698699922682251371601");

            string nonce = NonceGenerator.GenerateNonce(
                (PublicKey)pk.PublicKey(),
                maxEpoch,
                randomness
            );

            string nonceExpected = "Au6gNOIjTVy5y7yaoq63UVNWNOc";
            Assert.AreEqual(nonceExpected, nonce, "RAND: " + randomness.ToString());
        }

        [Test]
        // Test Case 4: Empty private key, large BigInteger
        public void GenerateNonceTest_4()
        {
            PrivateKey pk = new PrivateKey(new byte[32]);
            string pubKey = pk.PublicKey().KeyBase64;
            Assert.AreEqual(pubKeyBase64Expected, pubKey);

            int maxEpoch = 0;
            BigInteger randomness = BigInteger.Parse("52795003160875479850680435799259259156");

            string nonce = NonceGenerator.GenerateNonce(
                (PublicKey)pk.PublicKey(),
                maxEpoch,
                randomness
            );

            string nonceExpected = "5OR1kjf9JnXLjqOFoCf3oWNYUuI";
            Assert.AreEqual(nonceExpected, nonce, "RAND: " + randomness.ToString());
        }

        [Test]
        // Test Case 5: Sample private key, bigger epoch number and large BigInteger for randomness
        public void GenerateNonceTest_5()
        {
            PrivateKey pk = new PrivateKey("d9zN88TckfIma6bORNvc55gYyNExHMfYWDPackyptVE=");
            string pubKey = pk.PublicKey().KeyBase64;
            string pubKeyExpectedB64 = "MAJEmJmINxz1EUAh5WAkA14HuK+UmGR/mh0KFwqWsh4=";
            Assert.AreEqual(pubKeyExpectedB64, pubKey);

            int maxEpoch = 31;
            BigInteger randomness = BigInteger.Parse("135690536260876761130952245550993691844");

            string nonce = NonceGenerator.GenerateNonce(
                (PublicKey)pk.PublicKey(),
                maxEpoch,
                randomness
            );

            string nonceExpected = "XHL72OBEiaVtQkO_i_3BWB3dDEw";
            Assert.AreEqual(nonceExpected, nonce, "RAND: " + randomness.ToString());
        }

        [Test]
        // Test Case 1: Empty Array
        public void ToBigIntBETest_EmptyArray()
        {
            byte[] bytes = { };
            BigInteger toBigintBE = NonceGenerator.ToBigIntBE(bytes);
            Assert.AreEqual(BigInteger.Zero, toBigintBE, "BigInteger Value: " + toBigintBE);
        }

        [Test]
        // Test Case 2: Single Byte
        public void ToBigIntBETest_SingleByte()
        {
            byte[] bytes = new byte[] { 0x12 };
            BigInteger toBigintBE = NonceGenerator.ToBigIntBE(bytes);
            Assert.AreEqual(new BigInteger(0x12), toBigintBE, "BigInteger Value: " + toBigintBE);
        }

        [Test]
        // Test Case 3: Multi-byte (Big-Endian Interpretation)
        public void ToBigIntBETest_MultiByte()
        {
            byte[] bytes = new byte[] { 0x12, 0x34, 0x56, 0x78 };
            BigInteger toBigintBE = NonceGenerator.ToBigIntBE(bytes);
            Assert.AreEqual(new BigInteger(0x12345678), toBigintBE, "BigInteger Value: " + toBigintBE);
        }

        [Test]
        // Test Case 4: Multi-byte with Leading Zeros
        public void ToBigIntBETest_MultiByte_LeadingZeros()
        {
            byte[] bytes = new byte[] { 0xff, 0xee, 0xdd, 0xcc, 0xbb, 0xaa, 0x99, 0x88 };
            BigInteger toBigintBE = NonceGenerator.ToBigIntBE(bytes);
            Assert.AreEqual(new BigInteger(0xffeeddccbbaa9988), toBigintBE, "BigInteger Value: " + toBigintBE);
        }

        [Test]
        // Test Case 5: Large Byte Array
        public void ToBigIntBETest_LargeByteArray()
        {
            byte[] bytes = new byte[] { 0x00, 0x01, 0x02, 0x03 };
            BigInteger toBigintBE = NonceGenerator.ToBigIntBE(bytes);
            Assert.AreEqual(new BigInteger(0x010203), toBigintBE, "BigInteger Value: " + toBigintBE);
        }

        [Test]
        // Test Case 6: Maximum Single Byte Value
        public void ToBigIntBETest_MaximumSingleByteValue()
        {
            byte[] bytes = new byte[] { 0xff };
            BigInteger toBigintBE = NonceGenerator.ToBigIntBE(bytes);
            Assert.AreEqual(new BigInteger(0xff), toBigintBE, "BigInteger Value: " + toBigintBE);
        }

        /// <summary>
        /// Verifies that when calling RandomBytes with no parameters, 
        /// it returns an array of exactly 32 bytes, which is the default length.
        /// This test ensures the default behavior remains consistent.
        /// </summary>
        [Test]
        [Description("Ensures the default RandomBytes call returns 32 bytes")]
        public void RandomBytes_DefaultLength_Returns32Bytes()
        {
            // Act
            byte[] result = NonceGenerator.RandomBytes();

            // Assert
            Assert.AreEqual(32, result.Length);
        }

        /// <summary>
        /// Validates that RandomBytes correctly handles custom length requests.
        /// Tests if the returned array matches the specified length.
        /// This ensures the method correctly respects user-specified lengths.
        /// </summary>
        [Test]
        [Description("Verifies RandomBytes returns the exact number of bytes requested")]
        public void RandomBytes_CustomLength_ReturnsCorrectLength()
        {
            // Arrange
            int customLength = 16;

            // Act
            byte[] result = NonceGenerator.RandomBytes(customLength);

            // Assert
            Assert.AreEqual(customLength, result.Length);
        }

        /// <summary>
        /// Tests the randomness of the generated bytes by comparing two consecutive calls.
        /// While there's a theoretical possibility of getting identical arrays,
        /// the probability is astronomically low (1 in 2^256 for 32 bytes).
        /// This test helps ensure the cryptographic randomness of the implementation.
        /// </summary>
        [Test]
        [Description("Confirms that consecutive calls return different random values")]
        public void RandomBytes_CalledTwice_ReturnsDifferentValues()
        {
            // Act
            byte[] firstResult = NonceGenerator.RandomBytes();
            byte[] secondResult = NonceGenerator.RandomBytes();

            // Assert
            bool areEqual = true;
            for (int i = 0; i < firstResult.Length; i++)
            {
                if (firstResult[i] != secondResult[i])
                {
                    areEqual = false;
                    break;
                }
            }
            Assert.IsFalse(areEqual, "Two consecutive calls should return different random values");
        }

        /// <summary>
        /// Verifies that requesting zero bytes returns an empty array rather than null.
        /// This test ensures consistent behavior with edge cases and follows the principle
        /// that array-returning methods should return empty arrays rather than null.
        /// </summary>
        [Test]
        [Description("Confirms that requesting 0 bytes returns an empty array")]
        public void RandomBytes_ZeroLength_ReturnsEmptyArray()
        {
            // Act
            byte[] result = NonceGenerator.RandomBytes(0);

            // Assert
            Assert.AreEqual(0, result.Length);
        }

        /// <summary>
        /// Validates that the method properly handles invalid input by throwing
        /// an appropriate exception when given a negative length.
        /// This test ensures robust error handling and input validation.
        /// </summary>
        [Test]
        [Description("Verifies that negative length input throws ArgumentOutOfRangeException")]
        public void RandomBytes_NegativeLength_ThrowsArgumentException()
        {
            // Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => NonceGenerator.RandomBytes(-1));
        }
    }
}