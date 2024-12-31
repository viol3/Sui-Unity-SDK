using System;
using System.Numerics;
using NUnit.Framework;
using Sui.ZKLogin;

namespace Sui.Tests.ZkLogin
{
    public class UtilsTest
    {
        // TODO: Implement FindFirstNonZeroIndex
        [Test]
        public void FindFirstNonZeroIndexTest()
        {
            byte[] bytes = { 0, 0, 0, 1, 2, 3 };
            int actualIndex = Utils.FindFirstNonZeroIndex(bytes);
            Assert.AreEqual(3, actualIndex);
        }

        // TODO: Implement ToPaddedBigEndianBytes
        [Test]
        public void ToPaddedBigEndianBytesTest()
        {
            BigInteger bigInt = new BigInteger(255);
            byte[] paddedBytes = bigInt.ToPaddedBigEndianBytes(4);
            Assert.AreEqual(string.Join(",", paddedBytes), "0,0,0,255");
        }

        // TODO: Implement ToBigEndianByte
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
    }
}