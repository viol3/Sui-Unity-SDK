using System.Collections.Generic;
using System.Numerics;
using OpenDive.Crypto.PoseidonLite;
using OpenDive.Crypto.PoseidonLite.Constants;

public class Poseidon4
{
    private Dictionary<string, object> c4;
    private Dictionary<string, object> c;

    public Poseidon4()
    {
        c4 = new Dictionary<string, object> { ["C"] = C4.C, ["M"] = C4.M };
        c = BigIntUnstringifier.UnstringifyBigInts(c4);
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
