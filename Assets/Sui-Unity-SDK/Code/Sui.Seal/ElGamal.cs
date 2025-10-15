using System;
using MCL.BLS12_381.Net; // Kendi MCL kütüphanemiz

namespace Sui.Seal
{
    public static class ElGamal
    {
        // export function generateSecretKey(): Uint8Array<ArrayBuffer>
        public static byte[] GenerateSecretKey()
        {
            // return Scalar.random().toBytes() as Uint8Array<ArrayBuffer>;
            return Fr.GetRandom().ToBytes();
        }

        // export function toPublicKey(sk: Uint8Array<ArrayBuffer>): Uint8Array<ArrayBuffer>
        public static byte[] ToPublicKey(byte[] sk)
        {
            // return G1Element.generator().multiply(Scalar.fromBytes(sk)).toBytes();
            return (G1.Generator * Fr.FromBytes(sk)).ToBytes();
        }

        // export function toVerificationKey(sk: Uint8Array<ArrayBuffer>): Uint8Array<ArrayBuffer>
        public static byte[] ToVerificationKey(byte[] sk)
        {
            // return G2Element.generator().multiply(Scalar.fromBytes(sk)).toBytes();
            return (G2.Generator * Fr.FromBytes(sk)).ToBytes();
        }

        // export function elgamalDecrypt(sk: Uint8Array, [c0, c1]: [Uint8Array, Uint8Array]): Uint8Array
        public static byte[] Decrypt(byte[] skBytes, byte[] c0Bytes, byte[] c1Bytes)
        {
            // return decrypt(Scalar.fromBytes(sk), [ G1Element.fromBytes(c0), G1Element.fromBytes(c1), ]).toBytes();
            var sk = Fr.FromBytes(skBytes);
            var c0 = G1.FromBytes(c0Bytes);
            var c1 = G1.FromBytes(c1Bytes);

            return Decrypt(sk, c0, c1).ToBytes();
        }

        // function decrypt(sk: Scalar, [c0, c1]: [G1Element, G1Element]): G1Element
        private static G1 Decrypt(Fr sk, G1 c0, G1 c1)
        {
            // return c1.subtract(c0.multiply(sk));
            // Operatörleri daha önce tanımladığımız için bu satır çok daha temiz görünüyor.
            return c1 - (c0 * sk);
        }
    }
}