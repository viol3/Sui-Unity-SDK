//
//  TransferPolicyRule.cs
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

using Sui.Kiosks.TransferPolicy.Types;
using Sui.Transactions;

namespace Sui.Kiosks.Environment
{
    public delegate RuleExecutionResult<IObjectArgument>
        ResolveRuleDelegate(ref RulesResolvingParams parameters);

    public class TransferPolicyRule
    {
        public string Rule { get; set; }

        public string PackageID { get; set; }

        public ResolveRuleDelegate ResolveRuleFunction { get; set; }

        public bool? HasLockingRule { get; set; }

        public TransferPolicyRule
        (
            string rule,
            string packageId,
            ResolveRuleDelegate resolveRuleFunction,
            bool? hasLockingRule = null
        )
        {
            this.Rule = rule;
            this.PackageID = packageId;
            this.ResolveRuleFunction = resolveRuleFunction;
            this.HasLockingRule = hasLockingRule;
        }
    }
}