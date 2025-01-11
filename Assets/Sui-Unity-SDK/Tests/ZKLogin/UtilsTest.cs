using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Numerics;
using NUnit.Framework;
using Sui.ZKLogin;

namespace Sui.Tests.ZkLogin
{
    public class UtilsTest
    {
        [Test]
        public void FindFirstNonZeroIndexTest()
        {
            byte[] bytes = { 0, 0, 0, 1, 2, 3 };
            int actualIndex = Utils.FindFirstNonZeroIndex(bytes);
            Assert.AreEqual(3, actualIndex);
        }

        [Test]
        public void ToPaddedBigEndianBytesTest()
        {
            BigInteger bigInt = new BigInteger(255);
            byte[] paddedBytes = bigInt.ToPaddedBigEndianBytes(4);
            Assert.AreEqual(string.Join(",", paddedBytes), "0,0,0,255");
        }

        [Test]
        public void ToBigEndianBytesTest()
        {
            BigInteger num1 = new BigInteger(255);
            byte[] bigEndianBytes = num1.ToBigEndianBytes(4);
            Assert.AreEqual(string.Join(",", bigEndianBytes), "255");

            BigInteger num2 = new BigInteger(0);
            byte[] bigEndianBytesZero = num2.ToBigEndianBytes(4);
            Assert.AreEqual(string.Join(",", bigEndianBytesZero), "0");
        }

        [Test]
        public void BytesBEToBigIntTest_ValidBytes()
        {
            byte[] bytes = { 0x01, 0x02, 0x03 };
            BigInteger result = Utils.BytesBEToBigInt(bytes);
            Assert.AreEqual(new BigInteger(0x010203), result);
        }

        [Test]
        public void BytesBEToBigIntTest_EmptyArray()
        {
            byte[] bytes = { };
            BigInteger result = Utils.BytesBEToBigInt(bytes);
            Assert.AreEqual(BigInteger.Zero, result);
        }

        [Test]
        public void BytesBEToBigIntTest_AllZeros()
        {
            byte[] bytes = { 0x00, 0x00, 0x00 };
            BigInteger result = Utils.BytesBEToBigInt(bytes);
            Assert.AreEqual(BigInteger.Zero, result);
        }

        [Test]
        public void BytesBEToBigIntTest_SingleByte()
        {
            byte[] bytes = { 0xFF };
            BigInteger result = Utils.BytesBEToBigInt(bytes);
            Assert.AreEqual(new BigInteger(0xFF), result);
        }

        [Test]
        public void BytesBEToBigIntTest_LargeValue()
        {
            byte[] bytes = { 0xFF, 0xFE, 0xFD, 0xFC };
            BigInteger result = Utils.BytesBEToBigInt(bytes);
            Assert.AreEqual(new BigInteger(0xFFFEFDFC), result);
        }

        [Test]
        public void BytesBEToBigIntTest_NullArray()
        {
            byte[] bytes = null;
            BigInteger result = Utils.BytesBEToBigInt(bytes);
            Assert.AreEqual(BigInteger.Zero, result);
        }

        [Test]
        public void ChunkArrayTest_EvenSplit()
        {
            int[] array = { 1, 2, 3, 4, 5, 6 };
            var chunks = Utils.ChunkArray(array, 2);

            Assert.AreEqual(3, chunks.Count);
            Assert.AreEqual(new List<int> { 1, 2 }, chunks[0]);
            Assert.AreEqual(new List<int> { 3, 4 }, chunks[1]);
            Assert.AreEqual(new List<int> { 5, 6 }, chunks[2]);
        }

        [Test]
        public void ChunkArrayTest_OddSplit()
        {
            int[] array = { 1, 2, 3, 4, 5 };
            var chunks = Utils.ChunkArray(array, 2);

            Assert.AreEqual(3, chunks.Count);
            Assert.AreEqual(new List<int> { 1, 2 }, chunks[0]);
            Assert.AreEqual(new List<int> { 3, 4 }, chunks[1]);
            Assert.AreEqual(new List<int> { 5 }, chunks[2]);
        }

        [Test]
        public void ChunkArrayTest_EmptyArray()
        {
            int[] array = { };
            var chunks = Utils.ChunkArray(array, 2);

            Assert.AreEqual(0, chunks.Count);
        }

        [Test]
        public void ChunkArrayTest_ChunkSizeLargerThanArray()
        {
            int[] array = { 1, 2, 3 };
            var chunks = Utils.ChunkArray(array, 5);

            Assert.AreEqual(1, chunks.Count);
            Assert.AreEqual(new List<int> { 1, 2, 3 }, chunks[0]);
        }

        [Test]
        public void ChunkArrayTest_InvalidChunkSize()
        {
            int[] array = { 1, 2, 3 };

            Assert.Throws<ArgumentException>(() => Utils.ChunkArray(array, 0));
            Assert.Throws<ArgumentException>(() => Utils.ChunkArray(array, -1));
        }

        [Test]
        public void ChunkArrayTest_SingleElementArray()
        {
            int[] array = { 42 };
            var chunks = Utils.ChunkArray(array, 2);

            Assert.AreEqual(1, chunks.Count);
            Assert.AreEqual(new List<int> { 42 }, chunks[0]);
        }

        [Test]
        public void BytesToHex_EmptyArray_ReturnsEmptyString()
        {
            byte[] bytes = Array.Empty<byte>();
            string result = ZKLogin.SDK.Utils.BytesToHex(bytes);
            Assert.That(result, Is.Empty);
        }

        [Test]
        public void BytesToHex_SingleByte_ReturnsCorrectHex()
        {
            byte[] bytes = { 0xab };
            string result = ZKLogin.SDK.Utils.BytesToHex(bytes);
            Assert.That(result, Is.EqualTo("ab"));
        }

        [Test]
        public void BytesToHex_MultipleBytesExample_ReturnsCorrectHex()
        {
            byte[] bytes = { 0xca, 0xfe, 0x01, 0x23 };
            string result = ZKLogin.SDK.Utils.BytesToHex(bytes);
            Assert.That(result, Is.EqualTo("cafe0123"));
        }

        [Test]
        public void BytesToHex_ByteWithLeadingZeros_PreservesZeros()
        {
            byte[] bytes = { 0x00, 0x01, 0x02 };
            string result = ZKLogin.SDK.Utils.BytesToHex(bytes);
            Assert.That(result, Is.EqualTo("000102"));
        }

        [Test]
        public void BytesToHex_NullArray_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => ZKLogin.SDK.Utils.BytesToHex(null));
        }

        [Test]
        public void BytesToHex_MaxByteValue_ReturnsCorrectHex()
        {
            byte[] bytes = { 0xFF };
            string result = ZKLogin.SDK.Utils.BytesToHex(bytes);
            Assert.That(result, Is.EqualTo("ff"));
        }

        [Test]
        public void BytesToHex_MinByteValue_ReturnsCorrectHex()
        {
            byte[] bytes = { 0x00 };
            string result = ZKLogin.SDK.Utils.BytesToHex(bytes);
            Assert.That(result, Is.EqualTo("00"));
        }

        [Test]
        public void BytesToHex_LargeByteArray_ReturnsCorrectHex()
        {
            byte[] bytes = new byte[] { 0x12, 0x34, 0x56, 0x78, 0x9a, 0xbc, 0xde, 0xf0 };
            string result = ZKLogin.SDK.Utils.BytesToHex(bytes);
            Assert.That(result, Is.EqualTo("123456789abcdef0"));
        }
    }
}