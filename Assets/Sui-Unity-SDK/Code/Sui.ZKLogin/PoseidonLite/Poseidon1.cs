using System.Collections.Generic;
using System.Numerics;
using OpenDive.Crypto.PoseidonLite;
using OpenDive.Crypto.PoseidonLite.Constants;

public class Poseidon1
{
    private Dictionary<string, object> c1;
    private Dictionary<string, object> c;

    public Poseidon1()
    {
        c1 = new Dictionary<string, object> { ["C"] = C1.C, ["M"] = C1.M };
        c = BigIntUnstringifier.UnstringifyBigInts(c1);
    }

    public BigInteger[] Hash(object[] inputs, int nOuts = 1)
    {
        return Poseidon.Hash(inputs, this.c, nOuts);
    }
}
