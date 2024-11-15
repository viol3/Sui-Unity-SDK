using System.Collections.Generic;
using System.Numerics;
using OpenDive.Crypto.PoseidonLite;
using OpenDive.Crypto.PoseidonLite.Constants;

public class Poseidon6
{
    private Dictionary<string, object> c6;
    private Dictionary<string, object> c;

    public Poseidon6()
    {
        c6 = new Dictionary<string, object> { ["C"] = C6.C, ["M"] = C6.M };
        c = BigIntUnstringifier.UnstringifyBigInts(c6);
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
