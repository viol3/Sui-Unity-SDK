//
//  PrivateKey.cs
//  Sui-Unity-SDK
//
//  Copyright (c) 2025 OpenDive
//
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
//
//  The above copyright notice and this permission notice shall be included in
//  all copies or substantial portions of the Software.
//
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//  THE SOFTWARE.
//

using System;
using Chaos.NaCl;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Math;

using Sui.Utilities;
using static Sui.Cryptography.SignatureUtils;

namespace Sui.Cryptography.Secp256r1
{
    /// <summary>
    /// Implements the functionality of a SECP256R1 private key.
    /// The key may be viewed as a Hex or Base64 string as supported by Sui.
    /// </summary>
    public class PrivateKey : SuiPrivateKeyBase
    {
        // Declare and initialize the readonly fields properly
        private readonly X9ECParameters curveParameters;
        private readonly ECDomainParameters domainParameters;
        private ECPrivateKeyParameters _extended_key_params;

        /// <summary>
        /// SECP256R1 signature scheme identifier.
        /// </summary>
        public override SignatureScheme SignatureScheme => SignatureScheme.Secp256r1;

        /// <summary>
        /// Byte array representation of a SECP256R1 private key.
        /// Handles lazy initialization of the key bytes and extended parameters.
        /// </summary>
        public override byte[] KeyBytes
        {
            get
            {
                // Similar lazy initialization pattern as ED25519
                if (this._key_bytes == null && this._extended_key_params == null)
                {
                    if (this._key_hex != null)
                    {
                        string key = this._key_hex;
                        if (_key_hex[0..2].Equals("0x")) key = this._key_hex[2..];

                        // Convert hex string to private key bytes
                        this._key_bytes = key.HexStringToByteArray();
                    }
                    else // _keyBase64 is not null
                    {
                        string key = this._key_base64;
                        this._key_bytes = CryptoBytes.FromBase64String(key);
                    }

                    // Create the extended key parameters using Bouncy Castle
                    var d = new BigInteger(1, this._key_bytes);
                    this._extended_key_params = new ECPrivateKeyParameters(d, domainParameters);
                }
                return this._key_bytes;
            }
            set
            {
                if (value.Length != this.KeyLength)
                {
                    this.SetError<SuiError>($"Invalid key length: {value.Length}");
                    return;
                }

                this._key_bytes = value;
                // Create extended parameters from the raw bytes
                var d = new BigInteger(1, value);
                this._extended_key_params = new ECPrivateKeyParameters(d, domainParameters);
            }
        }

        public PrivateKey(byte[] private_key) : base(private_key)
        {
            // Initialize curve parameters in constructor
            curveParameters = ECNamedCurveTable.GetByName("secp256r1");
            domainParameters = new ECDomainParameters(
                curveParameters.Curve,
                curveParameters.G,
                curveParameters.N,
                curveParameters.H,
                curveParameters.GetSeed()
            );

            this.SetExtendedParameters();
        }

        public PrivateKey(ReadOnlySpan<byte> private_key) : base(private_key)
        {
            // Initialize curve parameters in constructor
            curveParameters = ECNamedCurveTable.GetByName("secp256r1");
            domainParameters = new ECDomainParameters(
                curveParameters.Curve,
                curveParameters.G,
                curveParameters.N,
                curveParameters.H,
                curveParameters.GetSeed()
            );

            this.SetExtendedParameters();
        }

        public PrivateKey(string private_key) : base(private_key)
        {
            // Initialize curve parameters in constructor
            curveParameters = ECNamedCurveTable.GetByName("secp256r1");
            domainParameters = new ECDomainParameters(
                curveParameters.Curve,
                curveParameters.G,
                curveParameters.N,
                curveParameters.H,
                curveParameters.GetSeed()
            );

            this.SetExtendedParameters();
        }

        public PrivateKey() : base(PrivateKey.GetRandomSeed())
        {
            // Initialize curve parameters in constructor
            curveParameters = ECNamedCurveTable.GetByName("secp256r1");
            domainParameters = new ECDomainParameters(
                curveParameters.Curve,
                curveParameters.G,
                curveParameters.N,
                curveParameters.H,
                curveParameters.GetSeed()
            );

            this.SetExtendedParameters();
        }

        /// <summary>
        /// Generates the corresponding SECP256R1 public key.
        /// </summary>
        public override PublicKeyBase PublicKey()
        {
            // Use the private key parameters to generate the public key point
            var q = domainParameters.G.Multiply(this._extended_key_params.D);
            var publicKeyParams = new ECPublicKeyParameters(q, domainParameters);

            // Convert the public key parameters to compressed format bytes
            byte[] publicKeyBytes = publicKeyParams.Q.GetEncoded(true);
            return new PublicKey(publicKeyBytes);
        }

        /// <summary>
        /// Signs a message using ECDSA with SHA256.
        /// </summary>
        public override SignatureBase Sign(byte[] message)
        {
            var signer = SignerUtilities.GetSigner("SHA-256withECDSA");
            signer.Init(true, this._extended_key_params);
            signer.BlockUpdate(message, 0, message.Length);

            byte[] signature = signer.GenerateSignature();
            return new Signature(signature);
        }

        public override SignatureBase Sign(string b64_message)
            => (Signature)this.Sign(CryptoBytes.FromBase64String(b64_message));

        /// <summary>
        /// Generates a random seed for the SECP256R1 private key.
        /// </summary>
        private static byte[] GetRandomSeed()
        {
            var secureRandom = new SecureRandom();
            byte[] seed = new byte[32]; // SECP256R1 uses 32-byte private keys
            secureRandom.NextBytes(seed);
            return seed;
        }

        /// <summary>
        /// Sets up the extended key parameters using the raw key bytes.
        /// </summary>
        private void SetExtendedParameters()
        {
            if (this.Error != null)
                return;

            var d = new BigInteger(1, this.KeyBytes);
            this._extended_key_params = new ECPrivateKeyParameters(d, domainParameters);
        }
    }
}