using System.Collections.Generic;
using System.Numerics;
using OpenDive.Crypto.PoseidonLite;
using OpenDive.Crypto.PoseidonLite.Constants;

public class Poseidon7
{
    private Dictionary<string, object> c7;
    private Dictionary<string, object> c;

    public Poseidon7()
    {
        c7 = new Dictionary<string, object> { ["C"] = C7.C, ["M"] = C7.M };
        c = BigIntUnstringifier.UnstringifyBigInts(c7);
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
