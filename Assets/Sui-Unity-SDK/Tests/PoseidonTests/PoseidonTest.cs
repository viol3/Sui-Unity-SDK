using System.Numerics;
using NUnit.Framework;

namespace Sui.Tests.PoseidonHash
{
    [TestFixture]
    public class PoseidonTest
    {
        [Test]
        public void PoseidonHash1Test()
        {
            string[] inputs = { "0x5b1f0533dd" };
            string input = "05b1f0533dd"; //TODO: Look into why need to remove `x` from the hex string in C#.

            BigInteger bigIntInput = BigInteger.Parse(input, System.Globalization.NumberStyles.HexNumber);
            BigInteger[] bigIntArrInput = { bigIntInput };

            string expectedOutput = "9318308185879295164714774571585163653334223568644020086992052983287926868898";
            BigInteger expectedOuputBigInt = BigInteger.Parse(expectedOutput);

            BigInteger actual = Poseidon1.Hash(bigIntArrInput);
            Assert.AreEqual(expectedOuputBigInt, actual, "ACTUAL: " + actual.ToString());
        }

        [Test]
        public void PoseidonHash2Test()
        {
            BigInteger[] bigIntArrInput = {
                BigInteger.Parse("0568570a0c4",     System.Globalization.NumberStyles.HexNumber),
                BigInteger.Parse("0fa742d6517434f", System.Globalization.NumberStyles.HexNumber)
            };

            BigInteger expectedOuputBigInt = BigInteger.Parse("12886590615193023928990309918723719693793770204591928268188166210615666393999");

            BigInteger actual = Poseidon2.Hash(bigIntArrInput);
            Assert.AreEqual(expectedOuputBigInt, actual, "ACTUAL: " + actual.ToString());
        }

        [Test]
        public void PoseidonHash3Test()
        {
            BigInteger[] bigIntArrInput = {
                BigInteger.Parse("0d96def96651128", System.Globalization.NumberStyles.HexNumber),
                BigInteger.Parse("01f7d",           System.Globalization.NumberStyles.HexNumber),
                BigInteger.Parse("0bf557e2e",       System.Globalization.NumberStyles.HexNumber)
            };

            BigInteger expectedOuputBigInt = BigInteger.Parse("7722661377568892714328861529862730512085148820504565133363463965621597421200");

            BigInteger actual = Poseidon3.Hash(bigIntArrInput);
            Assert.AreEqual(expectedOuputBigInt, actual, "ACTUAL: " + actual.ToString());
        }

        [Test]
        public void PoseidonHash4Test()
        {
            BigInteger[] bigIntArrInput = {
                BigInteger.Parse("0cc6d20772509",       System.Globalization.NumberStyles.HexNumber),
                BigInteger.Parse("0c3e08d080def6ce2",   System.Globalization.NumberStyles.HexNumber),
                BigInteger.Parse("04622f595",           System.Globalization.NumberStyles.HexNumber),
                BigInteger.Parse("072",                 System.Globalization.NumberStyles.HexNumber)
            };

            BigInteger expectedOuputBigInt = BigInteger.Parse("15102662994204263113152726795771945095413709771678878395298412054401617168492");

            BigInteger actual = Poseidon4.Hash(bigIntArrInput);
            Assert.AreEqual(expectedOuputBigInt, actual, "ACTUAL: " + actual.ToString());
        }

        [Test]
        public void PoseidonHash5Test()
        {
            BigInteger[] bigIntArrInput = {
                BigInteger.Parse("0119cc70bda751b46214b",   System.Globalization.NumberStyles.HexNumber),
                BigInteger.Parse("0d5ebd4812a",             System.Globalization.NumberStyles.HexNumber),
                BigInteger.Parse("06cdf678ca608fc",         System.Globalization.NumberStyles.HexNumber),
                BigInteger.Parse("0819643836601fc501172",   System.Globalization.NumberStyles.HexNumber),
                BigInteger.Parse("0fb9507a26a",             System.Globalization.NumberStyles.HexNumber)
            };

            BigInteger expectedOuputBigInt = BigInteger.Parse("475355438621302339608817371644128544331343530654767674429160180520836681578");

            BigInteger actual = Poseidon5.Hash(bigIntArrInput);
            Assert.AreEqual(expectedOuputBigInt, actual, "ACTUAL: " + actual.ToString());
        }

        [Test]
        public void PoseidonHash6Test()
        {
            BigInteger[] bigIntArrInput = {
                BigInteger.Parse("0873e8fdae701b8",         System.Globalization.NumberStyles.HexNumber),
                BigInteger.Parse("0bb4bf78a3ecb46d436e7",   System.Globalization.NumberStyles.HexNumber),
                BigInteger.Parse("0cf627275bfe27e1869",     System.Globalization.NumberStyles.HexNumber),
                BigInteger.Parse("00ddd89",                 System.Globalization.NumberStyles.HexNumber),
                BigInteger.Parse("079910b8008",             System.Globalization.NumberStyles.HexNumber),
                BigInteger.Parse("0fc04af87e25434",         System.Globalization.NumberStyles.HexNumber)
            };

            BigInteger expectedOuputBigInt = BigInteger.Parse("19515825097958319939544949050353165672719977680261864201249067521141962147653");

            BigInteger actual = Poseidon6.Hash(bigIntArrInput);
            Assert.AreEqual(expectedOuputBigInt, actual, "ACTUAL: " + actual.ToString());
        }


        [Test]
        public void PoseidonHash16Test()
        {
            BigInteger[] bigIntArrInput = {
                BigInteger.Parse("0dc9ebdf1be32335505",    System.Globalization.NumberStyles.HexNumber),
                BigInteger.Parse("0010cac726c97e11ff9f9",  System.Globalization.NumberStyles.HexNumber),
                BigInteger.Parse("063eead1f",              System.Globalization.NumberStyles.HexNumber),
                BigInteger.Parse("0aa53ffd5b1",            System.Globalization.NumberStyles.HexNumber),

                BigInteger.Parse("0f35069a25c",            System.Globalization.NumberStyles.HexNumber),
                BigInteger.Parse("069a3e9",                System.Globalization.NumberStyles.HexNumber),
                BigInteger.Parse("0ad8cfd203f91f6eae3",    System.Globalization.NumberStyles.HexNumber),
                BigInteger.Parse("04129a8ff2dd1b3",        System.Globalization.NumberStyles.HexNumber),

                BigInteger.Parse("072",                     System.Globalization.NumberStyles.HexNumber),
                BigInteger.Parse("00cb2bc69e1e7ed",         System.Globalization.NumberStyles.HexNumber),
                BigInteger.Parse("00deadf",                 System.Globalization.NumberStyles.HexNumber),
                BigInteger.Parse("061ddd0",                 System.Globalization.NumberStyles.HexNumber),

                BigInteger.Parse("0b8a2bf",                 System.Globalization.NumberStyles.HexNumber),
                BigInteger.Parse("090c5ecdef4608ca30b",     System.Globalization.NumberStyles.HexNumber),
                BigInteger.Parse("0b95b1bd9acb3514905",     System.Globalization.NumberStyles.HexNumber),
                BigInteger.Parse("0d8c05c11d4fc9451",       System.Globalization.NumberStyles.HexNumber)
            };

            BigInteger expectedOuputBigInt = BigInteger.Parse("17659853214326367823052875610415715805694234135492096018138758656568336777027");

            BigInteger actual = Poseidon16.Hash(bigIntArrInput);
            Assert.AreEqual(expectedOuputBigInt, actual, "ACTUAL: " + actual.ToString());
        }
    }
}