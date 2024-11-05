//
//  AttachRules.cs
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

using Sui.Transactions;
using Sui.Types;
using OpenDive.BCS;
using Sui.Utilities;

namespace Sui.Kiosks.Transaction.Rules
{
    public class AttachRules
    {
        public static void AttachKioskLockRuleTx
        (
            ref TransactionBlock tx,
            string type,
            IObjectArgument policy,
            IObjectArgument policyCap,
            string packageId
        )
        {
            tx.AddMoveCallTx
            (
                target: SuiMoveNormalizedStructType.FromStr($"{packageId}::kiosk_lock_rule::add"),
                type_arguments: new SerializableTypeTag[] { new SerializableTypeTag(type) },
                arguments: new TransactionArgument[] { tx.AddObjectInput(policy), tx.AddObjectInput(policyCap) }
            );
        }

        public static SuiResult<string> AttachRoyaltyRuleTx
        (
            ref TransactionBlock tx,
            string type,
            IObjectArgument policy,
            IObjectArgument policyCap,
            ushort percentageBps, // this is in basis points.
            ulong minAmount,
            string packageId
        )
        {
            if (percentageBps < 0 || percentageBps > 10_000)
            {
                return new SuiResult<string>
                (
                    null,
                    new SuiError
                    (
                        -1,
                        "Invalid basis point percentage. Use a value between [0,10000].",
                        null
                    )
                );
            }

            tx.AddMoveCallTx
            (
                target: SuiMoveNormalizedStructType.FromStr($"{packageId}::royalty_rule::add"),
                type_arguments: new SerializableTypeTag[] { new SerializableTypeTag(type) },
                arguments: new TransactionArgument[]
                {
                    tx.AddObjectInput(policy),
                    tx.AddObjectInput(policyCap),
                    tx.AddPure(new U16(percentageBps)),
                    tx.AddPure(new U64(minAmount))
                }
            );

            return new SuiResult<string>(null);
        }

        public static void AttachPersonalKioskRuleTx
        (
            ref TransactionBlock tx,
            string type,
            IObjectArgument policy,
            IObjectArgument policyCap,
            string packageId
        )
        {
            tx.AddMoveCallTx
            (
                target: SuiMoveNormalizedStructType.FromStr($"{packageId}::personal_kiosk_rule::add"),
                type_arguments: new SerializableTypeTag[] { new SerializableTypeTag(type) },
                arguments: new TransactionArgument[] { tx.AddObjectInput(policy), tx.AddObjectInput(policyCap) }
            );
        }

        public static void AttachFloorPriceRuleTx
        (
            ref TransactionBlock tx,
            string type,
            IObjectArgument policy,
            IObjectArgument policyCap,
            ulong minPrice,
            string packageId
        )
        {
            tx.AddMoveCallTx
            (
                target: SuiMoveNormalizedStructType.FromStr($"{packageId}::floor_price_rule::add"),
                type_arguments: new SerializableTypeTag[] { new SerializableTypeTag(type) },
                arguments: new TransactionArgument[]
                {
                    tx.AddObjectInput(policy),
                    tx.AddObjectInput(policyCap),
                    tx.AddPure(new U64(minPrice))
                }
            );
        }
    }
}