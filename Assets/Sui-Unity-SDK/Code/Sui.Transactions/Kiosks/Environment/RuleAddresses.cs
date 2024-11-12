//
//  RuleAddresses.cs
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

using System.Collections.Generic;
using Sui.Kiosks.Transaction.Rules;

namespace Sui.Kiosks.Environment
{
    using RuleLookup = Dictionary<KioskNetworkType, string>;

    public class RuleAddresses
    {
        public static RuleLookup RoyaltyRuleAddresses = new RuleLookup
        {
            { KioskNetworkType.Testnet, "0xbd8fc1947cf119350184107a3087e2dc27efefa0dd82e25a1f699069fe81a585" },
            { KioskNetworkType.Mainnet, "0x434b5bd8f6a7b05fede0ff46c6e511d71ea326ed38056e3bcd681d2d7c2a7879" },
            { KioskNetworkType.Custom, "" },
        };

        public static RuleLookup KioskLockRuleAddress = new RuleLookup
        {
            { KioskNetworkType.Testnet, "0xbd8fc1947cf119350184107a3087e2dc27efefa0dd82e25a1f699069fe81a585" },
            { KioskNetworkType.Mainnet, "0x434b5bd8f6a7b05fede0ff46c6e511d71ea326ed38056e3bcd681d2d7c2a7879" },
            { KioskNetworkType.Custom, "" },
        };

        public static RuleLookup FloorPriceRuleAddress = new RuleLookup
        {
            { KioskNetworkType.Testnet, "0x06f6bdd3f2e2e759d8a4b9c252f379f7a05e72dfe4c0b9311cdac27b8eb791b1" },
            { KioskNetworkType.Mainnet, "0x34cc6762780f4f6f153c924c0680cfe2a1fb4601e7d33cc28a92297b62de1e0e" },
            { KioskNetworkType.Custom, "" },
        };

        public static RuleLookup PersonalKioskRuleAddress = new RuleLookup
        {
            { KioskNetworkType.Testnet, "0x06f6bdd3f2e2e759d8a4b9c252f379f7a05e72dfe4c0b9311cdac27b8eb791b1" },
            { KioskNetworkType.Mainnet, "0x34cc6762780f4f6f153c924c0680cfe2a1fb4601e7d33cc28a92297b62de1e0e" },
            { KioskNetworkType.Custom, "" },
        };

        public static List<TransferPolicyRule> GetBaseRules
        (
            string royaltyRule = null,
            string kioskLockRule = null,
            string personalKioskRule = null,
            string floorPriceRule = null
        )
        {
            List<TransferPolicyRule> rules = new List<TransferPolicyRule>();

            if (royaltyRule != null)
            {
                rules.Add
                (
                    new TransferPolicyRule
                    (
                        rule: $"{royaltyRule}::royalty_rule::Rule",
                        packageId: royaltyRule,
                        resolveRuleFunction: ResolveRules.ResolveRoyaltyRule
                    )
                );
            }

            if (kioskLockRule != null)
            {
                rules.Add
                (
                    new TransferPolicyRule
                    (
                        rule: $"{kioskLockRule}::kiosk_lock_rule::Rule",
                        packageId: kioskLockRule,
                        resolveRuleFunction: ResolveRules.ResolveKioskLockRule
                    )
                );
            }

            if (personalKioskRule != null)
            {
                rules.Add
                (
                    new TransferPolicyRule
                    (
                        rule: $"{personalKioskRule}::personal_kiosk_rule::Rule",
                        packageId: personalKioskRule,
                        resolveRuleFunction: ResolveRules.ResolvePersonalKioskRule
                    )
                );
            }

            if (floorPriceRule != null)
            {
                rules.Add
                (
                    new TransferPolicyRule
                    (
                        rule: $"{floorPriceRule}::floor_price_rule::Rule",
                        packageId: floorPriceRule,
                        resolveRuleFunction: ResolveRules.ResolveFloorPriceRule
                    )
                );
            }

            return rules;
        }

        public static List<TransferPolicyRule> TestnetRules = GetBaseRules
        (
            royaltyRule: RoyaltyRuleAddresses[KioskNetworkType.Testnet],
            kioskLockRule: KioskLockRuleAddress[KioskNetworkType.Testnet],
            personalKioskRule: PersonalKioskRuleAddress[KioskNetworkType.Testnet],
            floorPriceRule: FloorPriceRuleAddress[KioskNetworkType.Testnet]
        );

        public static List<TransferPolicyRule> MainnetRules = GetBaseRules
        (
            royaltyRule: RoyaltyRuleAddresses[KioskNetworkType.Mainnet],
            kioskLockRule: KioskLockRuleAddress[KioskNetworkType.Mainnet]
        );
    }
}