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
            byte[] bytes = null;
            int actualIndex = ZKLogin.Utils.FindFirstNonZeroIndex(bytes);
            throw new NotImplementedException();
        }

        // TODO: Implement ToPaddedBigEndianBytes
        [Test]
        public void ToPaddedBigEndianBytesTest()
        {
            BigInteger bigInt = 0;
            byte[] paddedBigIntBytes = bigInt.ToPaddedBigEndianBytes(20);
            throw new NotImplementedException();
        }

        // TODO: Implement ToBigEndianByte
        [Test]
        public void ToBigEndianBytesTest()
        {
            BigInteger bigInt = 0;
            byte[] bigEndianBytes = bigInt.ToPaddedBigEndianBytes(20);
            throw new NotImplementedException();
        }
    }
}