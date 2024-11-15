using System.Collections.Generic;
using System.Numerics;
using OpenDive.Crypto.PoseidonLite;
using OpenDive.Crypto.PoseidonLite.Constants;

public class Poseidon15
{
    private Dictionary<string, object> c15;
    private Dictionary<string, object> c;

    public Poseidon15()
    {
        c15 = new Dictionary<string, object> { ["C"] = C15.C, ["M"] = C15.M };
        c = BigIntUnstringifier.UnstringifyBigInts(c15);
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
