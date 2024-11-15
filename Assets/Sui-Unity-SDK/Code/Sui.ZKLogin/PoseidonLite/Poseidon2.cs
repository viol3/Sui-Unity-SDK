using System.Collections.Generic;
using System.Numerics;
using OpenDive.Crypto.PoseidonLite;
using OpenDive.Crypto.PoseidonLite.Constants;

public class Poseidon2
{
    private Dictionary<string, object> c2;
    private Dictionary<string, object> c;

    public Poseidon2()
    {
        c2 = new Dictionary<string, object> { ["C"] = C2.C, ["M"] = C2.M };
        c = BigIntUnstringifier.UnstringifyBigInts(c2);
    }

    public BigInteger[] Hash(object[] inputs, int nOuts = 1)
    {
        return Poseidon.Hash(inputs, this.c, nOuts);
    }

    //public BigInteger[] Hash(string[] inputs, int nOuts)
    //{
    //    //PoseidonHash.Hash();
    //    throw new NotSupportedException();
    //}
}
