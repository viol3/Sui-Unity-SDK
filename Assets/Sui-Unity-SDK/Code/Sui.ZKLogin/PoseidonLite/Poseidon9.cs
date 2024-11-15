using System.Collections.Generic;
using System.Numerics;
using OpenDive.Crypto.PoseidonLite;
using OpenDive.Crypto.PoseidonLite.Constants;

public class Poseidon9
{
    private Dictionary<string, object> c9;
    private Dictionary<string, object> c;

    public Poseidon9()
    {
        c9 = new Dictionary<string, object> { ["C"] = C9.C, ["M"] = C9.M };
        c = BigIntUnstringifier.UnstringifyBigInts(c9);
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
