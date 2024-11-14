using System;
using System.Collections.Generic;
using System.Numerics;
using System.Linq;

namespace OpenDive.Crypto.PoseidonLite
{
    public static class BigIntUnstringifier
    {
        public static object UnstringifyBigInts(object o)
        {
            if (o is object[] array)
            {
                return array.Select(UnstringifyBigInts).ToArray();
            }
            else if (o is Dictionary<string, object> dict)
            {
                return dict.ToDictionary(
                    kvp => kvp.Key,
                    kvp => UnstringifyBigInts(kvp.Value)
                );
            }
            else if (o is string str)
            {
                byte[] byteArray = Convert.FromBase64String(str);
                string hex = BitConverter.ToString(byteArray).Replace("-", "").ToLower();
                return BigInteger.Parse($"0x{hex}", System.Globalization.NumberStyles.HexNumber);
            }

            return o;
        }
    }
}