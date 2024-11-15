using System.Collections.Generic;
using System.Numerics;
using OpenDive.Crypto.PoseidonLite;
using OpenDive.Crypto.PoseidonLite.Constants;

public class Poseidon13
{
    private Dictionary<string, object> c13;
    private Dictionary<string, object> c;

    public Poseidon13()
    {
        c13 = new Dictionary<string, object> { ["C"] = C13.C, ["M"] = C13.M };
        c = BigIntUnstringifier.UnstringifyBigInts(c13);
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
