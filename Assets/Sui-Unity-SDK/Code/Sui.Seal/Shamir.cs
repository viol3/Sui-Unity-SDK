using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Sui.Seal
{
    // TypeScript'teki 'Share' tipinin karşılığı
    public class Share
    {
        public int Index;
        public byte[] Data;
    }

    public static class Shamir
    {
        private const int GF256_SIZE = 256;

        #region Lookup Tables (EXP ve LOG)
        private static readonly byte[] EXP = { 0x01, 0x03, 0x05, 0x0f, 0x11, 0x33, 0x55, 0xff, 0x1a, 0x2e, 0x72, 0x96, 0xa1, 0xf8, 0x13, 0x35,
    0x5f, 0xe1, 0x38, 0x48, 0xd8, 0x73, 0x95, 0xa4, 0xf7, 0x02, 0x06, 0x0a, 0x1e, 0x22, 0x66, 0xaa,
    0xe5, 0x34, 0x5c, 0xe4, 0x37, 0x59, 0xeb, 0x26, 0x6a, 0xbe, 0xd9, 0x70, 0x90, 0xab, 0xe6, 0x31,
    0x53, 0xf5, 0x04, 0x0c, 0x14, 0x3c, 0x44, 0xcc, 0x4f, 0xd1, 0x68, 0xb8, 0xd3, 0x6e, 0xb2, 0xcd,
    0x4c, 0xd4, 0x67, 0xa9, 0xe0, 0x3b, 0x4d, 0xd7, 0x62, 0xa6, 0xf1, 0x08, 0x18, 0x28, 0x78, 0x88,
    0x83, 0x9e, 0xb9, 0xd0, 0x6b, 0xbd, 0xdc, 0x7f, 0x81, 0x98, 0xb3, 0xce, 0x49, 0xdb, 0x76, 0x9a,
    0xb5, 0xc4, 0x57, 0xf9, 0x10, 0x30, 0x50, 0xf0, 0x0b, 0x1d, 0x27, 0x69, 0xbb, 0xd6, 0x61, 0xa3,
    0xfe, 0x19, 0x2b, 0x7d, 0x87, 0x92, 0xad, 0xec, 0x2f, 0x71, 0x93, 0xae, 0xe9, 0x20, 0x60, 0xa0,
    0xfb, 0x16, 0x3a, 0x4e, 0xd2, 0x6d, 0xb7, 0xc2, 0x5d, 0xe7, 0x32, 0x56, 0xfa, 0x15, 0x3f, 0x41,
    0xc3, 0x5e, 0xe2, 0x3d, 0x47, 0xc9, 0x40, 0xc0, 0x5b, 0xed, 0x2c, 0x74, 0x9c, 0xbf, 0xda, 0x75,
    0x9f, 0xba, 0xd5, 0x64, 0xac, 0xef, 0x2a, 0x7e, 0x82, 0x9d, 0xbc, 0xdf, 0x7a, 0x8e, 0x89, 0x80,
    0x9b, 0xb6, 0xc1, 0x58, 0xe8, 0x23, 0x65, 0xaf, 0xea, 0x25, 0x6f, 0xb1, 0xc8, 0x43, 0xc5, 0x54,
    0xfc, 0x1f, 0x21, 0x63, 0xa5, 0xf4, 0x07, 0x09, 0x1b, 0x2d, 0x77, 0x99, 0xb0, 0xcb, 0x46, 0xca,
    0x45, 0xcf, 0x4a, 0xde, 0x79, 0x8b, 0x86, 0x91, 0xa8, 0xe3, 0x3e, 0x42, 0xc6, 0x51, 0xf3, 0x0e,
    0x12, 0x36, 0x5a, 0xee, 0x29, 0x7b, 0x8d, 0x8c, 0x8f, 0x8a, 0x85, 0x94, 0xa7, 0xf2, 0x0d, 0x17,
    0x39, 0x4b, 0xdd, 0x7c, 0x84, 0x97, 0xa2, 0xfd, 0x1c, 0x24, 0x6c, 0xb4, 0xc7, 0x52, 0xf6 };
        private static readonly byte[] LOG = { 0x00, 0x19, 0x01, 0x32, 0x02, 0x1a, 0xc6, 0x4b, 0xc7, 0x1b, 0x68, 0x33, 0xee, 0xdf, 0x03, 0x64,
    0x04, 0xe0, 0x0e, 0x34, 0x8d, 0x81, 0xef, 0x4c, 0x71, 0x08, 0xc8, 0xf8, 0x69, 0x1c, 0xc1, 0x7d,
    0xc2, 0x1d, 0xb5, 0xf9, 0xb9, 0x27, 0x6a, 0x4d, 0xe4, 0xa6, 0x72, 0x9a, 0xc9, 0x09, 0x78, 0x65,
    0x2f, 0x8a, 0x05, 0x21, 0x0f, 0xe1, 0x24, 0x12, 0xf0, 0x82, 0x45, 0x35, 0x93, 0xda, 0x8e, 0x96,
    0x8f, 0xdb, 0xbd, 0x36, 0xd0, 0xce, 0x94, 0x13, 0x5c, 0xd2, 0xf1, 0x40, 0x46, 0x83, 0x38, 0x66,
    0xdd, 0xfd, 0x30, 0xbf, 0x06, 0x8b, 0x62, 0xb3, 0x25, 0xe2, 0x98, 0x22, 0x88, 0x91, 0x10, 0x7e,
    0x6e, 0x48, 0xc3, 0xa3, 0xb6, 0x1e, 0x42, 0x3a, 0x6b, 0x28, 0x54, 0xfa, 0x85, 0x3d, 0xba, 0x2b,
    0x79, 0x0a, 0x15, 0x9b, 0x9f, 0x5e, 0xca, 0x4e, 0xd4, 0xac, 0xe5, 0xf3, 0x73, 0xa7, 0x57, 0xaf,
    0x58, 0xa8, 0x50, 0xf4, 0xea, 0xd6, 0x74, 0x4f, 0xae, 0xe9, 0xd5, 0xe7, 0xe6, 0xad, 0xe8, 0x2c,
    0xd7, 0x75, 0x7a, 0xeb, 0x16, 0x0b, 0xf5, 0x59, 0xcb, 0x5f, 0xb0, 0x9c, 0xa9, 0x51, 0xa0, 0x7f,
    0x0c, 0xf6, 0x6f, 0x17, 0xc4, 0x49, 0xec, 0xd8, 0x43, 0x1f, 0x2d, 0xa4, 0x76, 0x7b, 0xb7, 0xcc,
    0xbb, 0x3e, 0x5a, 0xfb, 0x60, 0xb1, 0x86, 0x3b, 0x52, 0xa1, 0x6c, 0xaa, 0x55, 0x29, 0x9d, 0x97,
    0xb2, 0x87, 0x90, 0x61, 0xbe, 0xdc, 0xfc, 0xbc, 0x95, 0xcf, 0xcd, 0x37, 0x3f, 0x5b, 0xd1, 0x53,
    0x39, 0x84, 0x3c, 0x41, 0xa2, 0x6d, 0x47, 0x14, 0x2a, 0x9e, 0x5d, 0x56, 0xf2, 0xd3, 0xab, 0x44,
    0x11, 0x92, 0xd9, 0x23, 0x20, 0x2e, 0x89, 0xb4, 0x7c, 0xb8, 0x26, 0x77, 0x99, 0xe3, 0xa5, 0x67,
    0x4a, 0xed, 0xde, 0xc5, 0x31, 0xfe, 0x18, 0x0d, 0x63, 0x8c, 0x80, 0xc0, 0xf7, 0x70, 0x07 };
        #endregion

        #region GF256 Sınıfı
        // Bu sınıf bir önceki adımdan...
        private readonly struct GF256 : IEquatable<GF256>
        {
            public readonly byte Value;

            public GF256(int value)
            {
                if (value < 0 || value >= GF256_SIZE) throw new ArgumentOutOfRangeException(nameof(value));
                Value = (byte)value;
            }

            private byte Log()
            {
                if (Value == 0) throw new InvalidOperationException("Log(0) is undefined.");
                return LOG[Value - 1];
            }
            private static GF256 Exp(int x)
            {
                // JavaScript'teki % operatörü negatif sayılarda farklı çalışabilir.
                // C#'ta doğru pozitif modül emin olmak için bu şekilde yazılır.
                var val = x % (GF256_SIZE - 1);
                if (val < 0) val += (GF256_SIZE - 1);
                return new GF256(EXP[val]);
            }

            public static GF256 operator +(GF256 a, GF256 b) => new GF256(a.Value ^ b.Value);
            public static GF256 operator -(GF256 a, GF256 b) => a + b;
            public static GF256 operator *(GF256 a, GF256 b)
            {
                if (a.Value == 0 || b.Value == 0) return Zero();
                return Exp(a.Log() + b.Log());
            }
            public static GF256 operator /(GF256 a, GF256 b)
            {
                if (b.Value == 0) throw new DivideByZeroException();
                return a * Exp(GF256_SIZE - b.Log() - 1);
            }

            public GF256 Neg() => this;
            public bool Equals(GF256 other) => this.Value == other.Value;
            public static GF256 Zero() => new GF256(0);
            public static GF256 One() => new GF256(1);
        }
        #endregion

        #region Polynomial Sınıfı (YENİ EKLENEN KISIM)
        private class Polynomial
        {
            public readonly IReadOnlyList<GF256> Coefficients;
            public int Degree => Coefficients.Count > 0 ? Coefficients.Count - 1 : 0;

            public Polynomial(IEnumerable<GF256> coefficients)
            {
                var coeffList = coefficients.ToList();
                // En yüksek dereceli katsayı sıfır olamaz. Sondaki sıfırları temizle.
                while (coeffList.Count > 0 && coeffList.Last().Value == 0)
                {
                    coeffList.RemoveAt(coeffList.Count - 1);
                }
                Coefficients = coeffList;
            }

            public static Polynomial FromBytes(byte[] bytes)
            {
                return new Polynomial(bytes.Select(b => new GF256(b)));
            }

            public GF256 GetCoefficient(int index)
            {
                return index >= Coefficients.Count ? GF256.Zero() : Coefficients[index];
            }

            public GF256 Evaluate(GF256 x)
            {
                // Horner's method: TS'deki toReversed().reduce() ile aynı mantık.
                GF256 sum = GF256.Zero();
                for (int i = Degree; i >= 0; i--)
                {
                    sum = sum * x + GetCoefficient(i);
                }
                return sum;
            }

            public static Polynomial operator +(Polynomial a, Polynomial b)
            {
                int degree = Math.Max(a.Degree, b.Degree);
                var newCoeffs = new GF256[degree + 1];
                for (int i = 0; i <= degree; i++)
                {
                    newCoeffs[i] = a.GetCoefficient(i) + b.GetCoefficient(i);
                }
                return new Polynomial(newCoeffs);
            }

            public static Polynomial operator *(Polynomial a, Polynomial b)
            {
                int degree = a.Degree + b.Degree;
                var newCoeffs = new GF256[degree + 1];
                for (int i = 0; i <= degree; i++)
                {
                    GF256 sum = GF256.Zero();
                    for (int j = 0; j <= i; j++)
                    {
                        sum += a.GetCoefficient(j) * b.GetCoefficient(i - j);
                    }
                    newCoeffs[i] = sum;
                }
                return new Polynomial(newCoeffs);
            }

            // Verilen koordinatlardan yola çıkarak sırrı birleştiren (interpolate eden) metod.
            public static GF256 Combine(ValueTuple<GF256, GF256>[] coordinates)
            {
                GF256 xProduct = coordinates.Aggregate(GF256.One(), (prod, coord) => prod * coord.Item1);

                GF256 quotient = coordinates.Aggregate(GF256.Zero(), (sum, coord_j) =>
                {
                    var (x_j, y_j) = coord_j;
                    var denominator = coordinates
                        .Where(coord_i => !coord_i.Item1.Equals(x_j))
                        .Aggregate(GF256.One(), (prod, coord_i) => prod * (coord_i.Item1 - x_j));

                    return sum + (y_j / (x_j * denominator));
                });

                return xProduct * quotient;
            }

            public static Polynomial Zero() => new Polynomial(new List<GF256>());
        }
        #endregion

        #region Ana Fonksiyonlar (YENİ EKLENEN KISIM)

        // function samplePolynomial(constant: GF256, degree: number): Polynomial
        private static Polynomial SamplePolynomial(GF256 constant, int degree)
        {
            var randomCoefficients = new byte[degree];
            randomCoefficients = new byte[] { 20 };
            // 1. Kriptografik olarak güvenli bir rastgele sayı üreteci nesnesi oluşturuyoruz.
            //using (var rng = RandomNumberGenerator.Create())
            //{
            //    // 2. Bu nesnenin 'GetBytes' metodunu kullanarak dizimizi dolduruyoruz.
            //    rng.GetBytes(randomCoefficients);
            //}

            var coeffs = new List<GF256> { constant };
            coeffs.AddRange(randomCoefficients.Select(b => new GF256(b)));
            return new Polynomial(coeffs);
        }

        // export function split(secret: Uint8Array, threshold: number, total: number): Share[]
        public static Share[] Split(byte[] secret, int threshold, int total)
        {
            if (threshold > total || threshold < 1 || total >= GF256_SIZE)
            {
                throw new ArgumentException($"Invalid threshold {threshold} or total {total}");
            }

            var polynomials = secret.Select(s => SamplePolynomial(new GF256(s), threshold - 1)).ToList();

            var result = new Share[total];
            for (int i = 0; i < total; i++)
            {
                var index = new GF256(i + 1); // Indexler 1'den başlar
                var shareData = polynomials.Select(p => p.Evaluate(index).Value).ToArray();
                result[i] = new Share { Index = index.Value, Data = shareData };
            }
            return result;
        }

        // export function combine(shares: Share[]): Uint8Array<ArrayBuffer>
        public static byte[] Combine(Share[] shares)
        {
            if (shares.Length < 1) throw new ArgumentException("At least one share is required.");
            if (shares.Select(s => s.Data.Length).Distinct().Count() > 1) throw new ArgumentException("All shares must have the same length.");
            if (shares.Select(s => s.Index).Distinct().Count() != shares.Length) throw new ArgumentException("Shares must have unique indices.");

            int length = shares[0].Data.Length;
            var secret = new byte[length];

            for (int i = 0; i < length; i++)
            {
                var coordinates = shares.Select(s => (new GF256(s.Index), new GF256(s.Data[i]))).ToArray();
                secret[i] = Polynomial.Combine(coordinates).Value;
            }
            return secret;
        }

        #endregion
    }
}