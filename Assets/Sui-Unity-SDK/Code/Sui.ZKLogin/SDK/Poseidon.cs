using System;
using System.Numerics;
using System.Linq;

namespace Sui.ZKLogin.SDK
{
    public static class PoseidonHasher
    {
        public static readonly BigInteger BN254_FIELD_SIZE = BigInteger.Parse("21888242871839275222246405745257275088548364400416034343698204186575808495617");

        private static readonly Func<BigInteger[], BigInteger>[] PoseidonNumToHashFN = {
        Poseidon.Hash1,
        Poseidon.Hash2,
        Poseidon.Hash3,
        Poseidon.Hash4,
        Poseidon.Hash5,
        Poseidon.Hash6,
        Poseidon.Hash7,
        Poseidon.Hash8,
        Poseidon.Hash9,
        Poseidon.Hash10,
        Poseidon.Hash11,
        Poseidon.Hash12,
        Poseidon.Hash13,
        Poseidon.Hash14,
        Poseidon.Hash15,
        Poseidon.Hash16
    };

        public static BigInteger PoseidonHash(object[] inputs)
        {
            var bigIntInputs = inputs.Select(x => {
                var b = ToBigInteger(x);
                if (b < 0 || b >= BN254_FIELD_SIZE)
                    throw new ArgumentException($"Element {b} not in the BN254 field");
                return b;
            }).ToArray();

            if (bigIntInputs.Length <= 16 && PoseidonNumToHashFN.Length >= bigIntInputs.Length)
            {
                return PoseidonNumToHashFN[bigIntInputs.Length - 1](bigIntInputs);
            }
            else if (bigIntInputs.Length <= 32)
            {
                var hash1 = PoseidonHash(bigIntInputs.Take(16).Cast<object>().ToArray());
                var hash2 = PoseidonHash(bigIntInputs.Skip(16).Cast<object>().ToArray());
                return PoseidonHash(new object[] { hash1, hash2 });
            }

            throw new ArgumentException($"Unable to hash a vector of length {bigIntInputs.Length}");
        }

        private static BigInteger ToBigInteger(object input)
        {
            return input switch
            {
                BigInteger bi => bi,
                int i => new BigInteger(i),
                long l => new BigInteger(l),
                string s => BigInteger.Parse(s),
                _ => throw new ArgumentException($"Unsupported input type: {input.GetType()}")
            };
        }
    }

    // This interface needs to be implemented based on the poseidon-lite functionality
    public static class Poseidon
    {
        public static BigInteger Hash1(BigInteger[] inputs) => throw new NotImplementedException();
        public static BigInteger Hash2(BigInteger[] inputs) => throw new NotImplementedException();
        public static BigInteger Hash3(BigInteger[] inputs) => throw new NotImplementedException();
        public static BigInteger Hash4(BigInteger[] inputs) => throw new NotImplementedException();
        public static BigInteger Hash5(BigInteger[] inputs) => throw new NotImplementedException();
        public static BigInteger Hash6(BigInteger[] inputs) => throw new NotImplementedException();
        public static BigInteger Hash7(BigInteger[] inputs) => throw new NotImplementedException();
        public static BigInteger Hash8(BigInteger[] inputs) => throw new NotImplementedException();
        public static BigInteger Hash9(BigInteger[] inputs) => throw new NotImplementedException();
        public static BigInteger Hash10(BigInteger[] inputs) => throw new NotImplementedException();
        public static BigInteger Hash11(BigInteger[] inputs) => throw new NotImplementedException();
        public static BigInteger Hash12(BigInteger[] inputs) => throw new NotImplementedException();
        public static BigInteger Hash13(BigInteger[] inputs) => throw new NotImplementedException();
        public static BigInteger Hash14(BigInteger[] inputs) => throw new NotImplementedException();
        public static BigInteger Hash15(BigInteger[] inputs) => throw new NotImplementedException();
        public static BigInteger Hash16(BigInteger[] inputs) => throw new NotImplementedException();
    }
}