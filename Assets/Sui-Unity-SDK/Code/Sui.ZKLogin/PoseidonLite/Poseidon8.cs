using System.Collections.Generic;
using System.Numerics;
using OpenDive.Crypto.PoseidonLite;
using OpenDive.Crypto.PoseidonLite.Constants;

public class Poseidon8
{
    private Dictionary<string, object> c8;
    private Dictionary<string, object> c;

    public Poseidon8()
    {
        c8 = new Dictionary<string, object> { ["C"] = C8.C, ["M"] = C8.M };
        c = BigIntUnstringifier.UnstringifyBigInts(c8);
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
