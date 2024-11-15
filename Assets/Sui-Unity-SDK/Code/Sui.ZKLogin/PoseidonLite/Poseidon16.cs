using System.Collections.Generic;
using System.Numerics;
using OpenDive.Crypto.PoseidonLite;
using OpenDive.Crypto.PoseidonLite.Constants;

public class Poseidon16
{
    private Dictionary<string, object> c16;
    private Dictionary<string, object> c;

    public Poseidon16()
    {
        c16 = new Dictionary<string, object> { ["C"] = C16.C, ["M"] = C16.M };
        c = BigIntUnstringifier.UnstringifyBigInts(c16);
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
