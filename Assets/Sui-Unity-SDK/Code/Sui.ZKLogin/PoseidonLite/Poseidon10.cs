using System.Collections.Generic;
using System.Numerics;
using OpenDive.Crypto.PoseidonLite;
using OpenDive.Crypto.PoseidonLite.Constants;

public class Poseidon10
{
    private Dictionary<string, object> c10;
    private Dictionary<string, object> c;

    public Poseidon10()
    {
        c10 = new Dictionary<string, object> { ["C"] = C10.C, ["M"] = C10.M };
        c = BigIntUnstringifier.UnstringifyBigInts(c10);
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
