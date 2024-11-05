using OpenDive.BCS;
using Sui.Kiosks.TransferPolicy.Types;
using Sui.Transactions;
using Sui.Types;
using Sui.Utilities;

namespace Sui.Kiosks.Transaction.Rules
{
    public class ResolveRules
    {
        public static void ResolveRoyaltyRule
        (
             ref RulesResolvingParams parameters
        )
        {
            TransactionArgument policyObj =
                parameters.Transaction.AddObjectInput(parameters.PolicyID);

            TransactionArgument amount = parameters.Transaction.AddMoveCallTx
            (
                target: SuiMoveNormalizedStructType.FromStr($"{parameters.PackageID}::royalty_rule::fee_amount"),
                type_arguments: new SerializableTypeTag[] { new SerializableTypeTag(parameters.ItemType) },
                arguments: new TransactionArgument[]
                {
                    policyObj, parameters.Transaction.AddPure(new U64(ulong.Parse(parameters.Price)))
                }
            )[0];

            TransactionArgument feeCoin = parameters.Transaction.AddSplitCoinsTx(parameters.Transaction.gas, amount)[0];

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
    }

    // TODO: Implement Kiosk Transaction Wrapper first for the lock function.
    //public static void ResolveKioskLockRule(ref RulesResolvingParams parameters)
    //{

    //}
}