using System.Collections.Generic;
using System.Numerics;
using OpenDive.Crypto.PoseidonLite;
using OpenDive.Crypto.PoseidonLite.Constants;

public class Poseidon3
{
    private Dictionary<string, object> c3;
    private Dictionary<string, object> c;

    public Poseidon3()
    {
        c3 = new Dictionary<string, object> { ["C"] = C3.C, ["M"] = C3.M };
        c = BigIntUnstringifier.UnstringifyBigInts(c3);
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
