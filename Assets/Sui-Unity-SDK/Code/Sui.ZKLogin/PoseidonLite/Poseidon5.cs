using System.Collections.Generic;
using System.Numerics;
using OpenDive.Crypto.PoseidonLite;
using OpenDive.Crypto.PoseidonLite.Constants;

public class Poseidon5
{
    private Dictionary<string, object> c5;
    private Dictionary<string, object> c;

    public Poseidon5()
    {
        c5 = new Dictionary<string, object> { ["C"] = C5.C, ["M"] = C5.M };
        c = BigIntUnstringifier.UnstringifyBigInts(c5);
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
