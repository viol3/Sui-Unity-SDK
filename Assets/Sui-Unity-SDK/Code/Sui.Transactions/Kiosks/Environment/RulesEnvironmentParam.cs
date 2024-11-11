//
//  RulesEnvironmentParam.cs
//  Sui-Unity-SDK
//
//  Copyright (c) 2024 OpenDive
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

namespace Sui.Kiosks.Environment
{
    public class RulesEnvironmentParam
    {
        public KioskRulesEnvironment Environment { get; set; }

        public string Address { get; set; }

        public RulesEnvironmentParam
        (
            KioskRulesEnvironment environment,
            string address
        )
        {
            this.Environment = environment;
            this.Address = address;
        }

        public static RulesEnvironmentParam Testnet()
            => new RulesEnvironmentParam
               (
                   KioskRulesEnvironment.Testnet,
                   "0xbd8fc1947cf119350184107a3087e2dc27efefa0dd82e25a1f699069fe81a585"
               );

        public static RulesEnvironmentParam Mainnet()
            => new RulesEnvironmentParam
               (
                   KioskRulesEnvironment.Mainnet,
                   "0x434b5bd8f6a7b05fede0ff46c6e511d71ea326ed38056e3bcd681d2d7c2a7879"
               );

        public static RulesEnvironmentParam Custom(string address)
            => new RulesEnvironmentParam(KioskRulesEnvironment.Custom, address);
    }
}