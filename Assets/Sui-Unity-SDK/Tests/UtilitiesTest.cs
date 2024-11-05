//
//  UtilitiesTest.cs
//  Sui-Unity-SDK
//
//  Copyright (c) 2024 OpenDive
//
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
//
//  The above copyright notice and this permission notice shall be included in
//  all copies or substantial portions of the Software.
//
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//  THE SOFTWARE.
//

using System.Numerics;
using NUnit.Framework;

namespace Sui.Tests.Cryptography
{
    public class UtilitiesTest
    {
        [Test]
        public void IsValidEd25519HexKey()
        {
            string validPkHex = "0x99da9559e15e913ee9ab2e53e3dfad575da33b49be1125bb922e33494f498828";
            bool isValidHex = Utilities.Utils.IsValidHexKey(validPkHex);

            Assert.AreEqual(true, isValidHex);
        }

        [Test]
        public void IsInvalidEd25519HexKey()
        {
            string invalidPkHex1 = "0x99da9559e15e913ee9ab2e53e3dfad575da33b49be1125bb92";
            string invalidPkHex2 = "0x99da9559e15e913ee9ab2e53e3dfad5!5da33b49be1125bb922e33494f498828";

            bool isValidHex1 = Utilities.Utils.IsValidHexKey(invalidPkHex1);
            Assert.AreEqual(false, isValidHex1);
            bool isValidHex2 = Utilities.Utils.IsValidHexKey(invalidPkHex2);
            Assert.AreEqual(false, isValidHex2);
        }

        [Test]
        public void ToPaddedBigEndianBytes_ShouldConvertCorrectly()
        {
            // Test case 1: Basic conversion
            Assert.AreEqual(
                new byte[] { 0 },
                Utilities.Utils.ToPaddedBigEndianBytes(BigInteger.Parse("0"), 1)
            );

            // Test case 2: Max single byte
            Assert.AreEqual(
                new byte[] { 255 },
                Utilities.Utils.ToPaddedBigEndianBytes(BigInteger.Parse("255"), 1)
            );

            // Test case 3: Two bytes
            Assert.AreEqual(
                new byte[] { 1, 0 },
                Utilities.Utils.ToPaddedBigEndianBytes(BigInteger.Parse("256"), 2)
            );

            // Test case 4: Max two bytes
            Assert.AreEqual(
                new byte[] { 255, 255 },
                Utilities.Utils.ToPaddedBigEndianBytes(BigInteger.Parse("65535"), 2)
            );
        }

        [Test]
        public void ToPaddedBigEndianBytes_ShouldPadWithZeros()
        {
            // Test case 1: Padding small number to 4 bytes
            byte[] result1 = Utilities.Utils.ToPaddedBigEndianBytes(BigInteger.Parse("255"), 4);
            Assert.AreEqual(4, result1.Length);
            CollectionAssert.AreEqual(new byte[] { 0, 0, 0, 255 }, result1);

            // Test case 2: Padding medium number to 4 bytes
            byte[] result2 = Utilities.Utils.ToPaddedBigEndianBytes(BigInteger.Parse("65535"), 4);
            Assert.AreEqual(4, result2.Length);
            CollectionAssert.AreEqual(new byte[] { 0, 0, 255, 255 }, result2);
        }

        [Test]
        public void FindFirstNonZeroIndex_ShouldFindCorrectIndex()
        {
            // Test case 1: Leading zeros
            Assert.AreEqual(2, Utilities.Utils.FindFirstNonZeroIndex(new byte[] { 0, 0, 1, 2 }));

            // Test case 2: No leading zeros
            Assert.AreEqual(0, Utilities.Utils.FindFirstNonZeroIndex(new byte[] { 1, 2, 3, 4 }));

            // Test case 3: All zeros
            Assert.AreEqual(-1, Utilities.Utils.FindFirstNonZeroIndex(new byte[] { 0, 0, 0, 0 }));

            // Test case 4: Single zero
            Assert.AreEqual(-1, Utilities.Utils.FindFirstNonZeroIndex(new byte[] { 0 }));

            // Test case 5: Single non-zero
            Assert.AreEqual(0, Utilities.Utils.FindFirstNonZeroIndex(new byte[] { 1 }));
        }

        [Test]
        public void ToBigEndianBytes_ShouldRemoveLeadingZeros()
        {
            // Test case 1: Zero should return single zero byte
            CollectionAssert.AreEqual(
                new byte[] { 0 },
                Utilities.Utils.ToBigEndianBytes(BigInteger.Parse("0"), 4)
            );

            // Test case 2: Single byte number
            CollectionAssert.AreEqual(
                new byte[] { 255 },
                Utilities.Utils.ToBigEndianBytes(BigInteger.Parse("255"), 4)
            );

            // Test case 3: Two byte number
            CollectionAssert.AreEqual(
                new byte[] { 1, 0 },
                Utilities.Utils.ToBigEndianBytes(BigInteger.Parse("256"), 4)
            );

            // Test case 4: Max two bytes
            CollectionAssert.AreEqual(
                new byte[] { 255, 255 },
                Utilities.Utils.ToBigEndianBytes(BigInteger.Parse("65535"), 4)
            );
        }

        [Test]
        public void ToBigEndianBytes_ShouldReturnSingleZeroForZero()
        {
            byte[] result = Utilities.Utils.ToBigEndianBytes(BigInteger.Zero, 4);
            Assert.AreEqual(1, result.Length);
            Assert.AreEqual(0, result[0]);
        }

        [Test]
        public void ToPaddedBigEndianBytes_ShouldHandleLargeNumbers()
        {
            // Arrange
            BigInteger num = BigInteger.Parse("1234567890123456789012345678901234567890");
            int width = 32;

            // Act
            byte[] result = Utilities.Utils.ToPaddedBigEndianBytes(num, width);

            // Assert
            Assert.AreEqual(width, result.Length);
            Assert.AreNotEqual(0, result[result.Length - 1]); // Last byte shouldn't be zero
        }

        [Test]
        public void HexToBytes_ShouldConvertCorrectly()
        {
            // Test case 1: Normal hex string
            CollectionAssert.AreEqual(
                new byte[] { 255, 255 },
                Utilities.Utils.HexStringToByteArray("FFFF")
            );

            // Test case 2: Hex string with leading zeros
            CollectionAssert.AreEqual(
                new byte[] { 0, 255, 255 },
                Utilities.Utils.HexStringToByteArray("00FFFF")
            );

            // Test case 3: Single byte hex
            CollectionAssert.AreEqual(
                new byte[] { 255 },
                Utilities.Utils.HexStringToByteArray("FF")
            );
        }
    }
}