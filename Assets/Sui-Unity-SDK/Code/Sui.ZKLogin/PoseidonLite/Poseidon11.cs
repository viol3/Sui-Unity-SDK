using System.Collections.Generic;
using System.Numerics;
using OpenDive.Crypto.PoseidonLite;
using OpenDive.Crypto.PoseidonLite.Constants;

public class Poseidon11
{
    private Dictionary<string, object> c11;
    private Dictionary<string, object> c;

    public Poseidon11()
    {
        c11 = new Dictionary<string, object> { ["C"] = C11.C, ["M"] = C11.M };
        c = BigIntUnstringifier.UnstringifyBigInts(c11);
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
