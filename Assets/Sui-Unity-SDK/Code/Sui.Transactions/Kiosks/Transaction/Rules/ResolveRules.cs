//
//  ResolveRules.cs
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

using OpenDive.BCS;
using Sui.Kiosks.TransferPolicy.Types;
using Sui.Transactions;
using Sui.Types;
using Sui.Utilities;

namespace Sui.Kiosks.Transaction.Rules
{
    public class ResolveRules
    {
        /// <summary>
        /// A helper to resolve the royalty rule.
        /// </summary>
        /// <param name="parameters">The parameters for the function, using Transaction, PackageID, ItemType, PolicyID, and Price.</param>
        public static void ResolveRoyaltyRule
        (
             ref RulesResolvingParams parameters
        )
        {
            TransactionArgument policyObj =
                parameters.Transaction.AddObjectInput(parameters.PolicyID);

            // calculates the amount
            TransactionArgument amount = parameters.Transaction.AddMoveCallTx
            (
                target: SuiMoveNormalizedStructType.FromStr($"{parameters.PackageID}::royalty_rule::fee_amount"),
                type_arguments: new SerializableTypeTag[] { new SerializableTypeTag(parameters.ItemType) },
                arguments: new TransactionArgument[]
                {
                    policyObj, parameters.Transaction.AddPure(new U64(ulong.Parse(parameters.Price)))
                }
            )[0];

            // splits the coin.
            TransactionArgument feeCoin = parameters.Transaction.AddSplitCoinsTx(parameters.Transaction.gas, amount)[0];

            // pays the policy
            parameters.Transaction.AddMoveCallTx
            (
                target: SuiMoveNormalizedStructType.FromStr($"{parameters.PackageID}::royalty_rule::pay"),
                type_arguments: new SerializableTypeTag[] { new SerializableTypeTag(parameters.ItemType) },
                arguments: new TransactionArgument[]
                {
                    policyObj,
                    parameters.TransferRequest.Argument, feeCoin
                }
            );
        }

        public static SuiResult<string> ResolveKioskLockRule(ref RulesResolvingParams parameters)
        {
            if (parameters.Kiosk == null || parameters.KioskCap == null)
                return new SuiResult<string>(null, new SuiError(-1, "Missing Owned Kiosk or Owned Kiosk Cap", null));

            TransactionBlock tx = parameters.Transaction;

            KioskTransactions.Lock
            (
                ref tx,
                parameters.ItemType,
                parameters.Kiosk,
                parameters.KioskCap,
                parameters.PolicyID,
                parameters.PurchasedItem
            );

            parameters.Transaction = tx;

            // proves that the item is locked in the kiosk to the TP.
            parameters.Transaction.AddMoveCallTx
            (
                target: SuiMoveNormalizedStructType.FromStr($"{parameters.PackageID}::kiosk_lock_rule::prove"),
                type_arguments: new SerializableTypeTag[] { new SerializableTypeTag(KioskConstants.KioskType) },
                arguments: new TransactionArgument[]
                {
                    parameters.TransferRequest.Argument,
                    parameters.Transaction.AddObjectInput(parameters.Kiosk)
                }
            );

            return new SuiResult<string>(null);
        }

        /// <summary>
        /// A helper to resolve the personalKioskRule.
        /// </summary>
        /// <param name="parameters">The parameters for the function, using Transaction, Kiosk, TransferRequest, and PackageID.</param>
        /// <returns>If there is an error, it is passed up to handle, otherwise it returns a null result.</returns>
        public static SuiResult<string> ResolvePersonalKioskRule
        (
            ref RulesResolvingParams parameters
        )
        {
            if (parameters.Kiosk == null)
                return new SuiResult<string>(null, new SuiError(-1, "Missing owned Kiosk.", null));

            // proves that the destination kiosk is personal.
            parameters.Transaction.AddMoveCallTx
            (
                target: SuiMoveNormalizedStructType.FromStr($"{parameters.PackageID}::personal_kiosk_rule::prove"),
                type_arguments: new SerializableTypeTag[] { new SerializableTypeTag(KioskConstants.KioskType) },
                arguments: new TransactionArgument[]
                {
                    parameters.Transaction.AddObjectInput(parameters.Kiosk),
                    parameters.TransferRequest.Argument
                }
            );

            return new SuiResult<string>(null);
        }

        /// <summary>
        /// Resolves the floor price rule.
        /// </summary>
        /// <param name="parameters">The parameters for the function, using Transaction, PolicyID, TransferRequest, and PackageID.</param>
        public static void ResolveFloorPriceRule
        (
            ref RulesResolvingParams parameters
        )
        {
            // proves that the destination kiosk is personal
            parameters.Transaction.AddMoveCallTx
            (
                target: SuiMoveNormalizedStructType.FromStr($"{parameters.PackageID}::floor_price_rule::prove"),
                type_arguments: new SerializableTypeTag[] { new SerializableTypeTag(KioskConstants.KioskType) },
                arguments: new TransactionArgument[]
                {
                    parameters.Transaction.AddObjectInput(parameters.PolicyID),
                    parameters.TransferRequest.Argument
                }
            );
        }
    }
}