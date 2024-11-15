using System.Collections.Generic;
using System.Numerics;
using OpenDive.Crypto.PoseidonLite;
using OpenDive.Crypto.PoseidonLite.Constants;

public class Poseidon12
{
    private Dictionary<string, object> c12;
    private Dictionary<string, object> c;

    public Poseidon12()
    {
        c12 = new Dictionary<string, object> { ["C"] = C12.C, ["M"] = C12.M };
        c = BigIntUnstringifier.UnstringifyBigInts(c12);
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
