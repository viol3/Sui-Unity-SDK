namespace Sui.Seal
{
    public class BonehFranklinBLS12381
    {
        public byte[] nonce;
        public byte[][] encryptedShares;
        public byte[] encryptedRandomness;
    }

    public class IBEEncryptions
    {
        public BonehFranklinBLS12381 BonehFranklinBLS12381;
    }
}