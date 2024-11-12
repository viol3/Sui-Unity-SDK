using System.Collections.Generic;
using OpenDive.BCS;
using Sui.Kiosks.TransferPolicy;
using Sui.Transactions;
using Sui.Types;
using Sui.Kiosks.Environment;

namespace Sui.Kiosks.Transaction
{
    public class TransactionPolicyTransactions
    {
        /// <summary>
        /// Call the `transfer_policy::new` function to create a new transfer policy.
        /// </summary>
        /// <param name="tx">The transaction to append transfer policy creation move calls.</param>
        /// <param name="itemType">The type of object the transfer policy will share.</param>
        /// <param name="publisher">The owner of the transfer policy.</param>
        /// <returns>A `transferPolicyCap` object.</returns>
        public static TransactionObjectArgument CreateTransferPolicy
        (
            ref TransactionBlock tx,
            string itemType,
            IObjectArgument publisher
        )
        {
            (TransactionObjectArgument transferPolicy, TransactionObjectArgument transferPolicyCap) =
                CreateTransferPolicyWithoutSharing
                (
                    ref tx,
                    itemType,
                    publisher
                );

            ShareTransferPolicy(ref tx, itemType, transferPolicy);

            return transferPolicyCap;
        }

        /// <summary>
        /// Creates a transfer Policy and returns both the Policy and the Cap.
        /// Used if we want to use the policy before making it a shared object.
        /// </summary>
        /// <param name="tx">The transaction to append the transfer policy creation move call.</param>
        /// <param name="itemType">The type of object the transfer policy will share.</param>
        /// <param name="publisher">The owner of the transfer policy.</param>
        /// <returns>The transfer policy and its cap.</returns>
        public static (TransactionObjectArgument, TransactionObjectArgument) CreateTransferPolicyWithoutSharing
        (
            ref TransactionBlock tx,
            string itemType,
            IObjectArgument publisher
        )
        {
            List<TransactionArgument> transfer = tx.AddMoveCallTx
            (
                target: SuiMoveNormalizedStructType.FromStr
                (
                    $"{TransferPolicyConstants.TransferPolicyModule}::new"
                ),
                type_arguments: new SerializableTypeTag[] { new SerializableTypeTag(itemType) },
                arguments: new TransactionArgument[] { tx.AddObjectInput(publisher) },
                return_value_count: 2
            );

            return
            (
                new TransactionObjectArgument(transfer[0]),
                new TransactionObjectArgument(transfer[1])
            );
        }

        /// <summary>
        /// Converts Transfer Policy to a shared object.
        /// </summary>
        /// <param name="tx">The transaction to append the shared object move call.</param>
        /// <param name="itemType">The type of object to be shared.</param>
        /// <param name="transferPolicy">The transfer policy to share.</param>
        public static void ShareTransferPolicy
        (
            ref TransactionBlock tx,
            string itemType,
            TransactionObjectArgument transferPolicy
        )
        {
            tx.AddMoveCallTx
            (
                target: SuiMoveNormalizedStructType.FromStr
                (
                    "0x2::transfer::public_share_object"
                ),
                type_arguments: new SerializableTypeTag[]
                {
                    new SerializableTypeTag($"{TransferPolicyConstants.TransferPolicyType}<{itemType}>")
                },
                arguments: new TransactionArgument[] { transferPolicy.Argument }
            );
        }

        /// <summary>
        /// Call the `transfer_policy::withdraw` function to withdraw profits from a transfer policy.
        /// </summary>
        /// <param name="tx">The transaction to append the withdraw move call.</param>
        /// <param name="itemType">The type of item to be withdrawn</param>
        /// <param name="policy">The transfer policy itself.</param>
        /// <param name="policyCap">The representation of the owner for the transfer policy.</param>
        /// <param name="amount">The amount the item costs, default to null.</param>
        /// <returns></returns>
        public static TransactionObjectArgument WithdrawFromPolicy
        (
            ref TransactionBlock tx,
            string itemType,
            TransactionObjectArgument policy,
            TransactionObjectArgument policyCap,
            ulong? amount = null
        )
        {
            TransactionArgument amountArg = amount != null ?
                tx.AddPure(new U64((ulong)amount)) :
                tx.AddPure(new U8(0));

            TransactionArgument profits = tx.AddMoveCallTx
            (
                target: SuiMoveNormalizedStructType.FromStr
                (
                    $"{TransferPolicyConstants.TransferPolicyModule}::withdraw"
                ),
                type_arguments: new SerializableTypeTag[]
                {
                    new SerializableTypeTag(itemType)
                },
                arguments: new TransactionArgument[]
                {
                    tx.AddObjectInput(policy),
                    tx.AddObjectInput(policyCap),
                    amountArg
                }
            )[0];

            return new TransactionObjectArgument(profits);
        }

        /// <summary>
        /// Call the `transfer_policy::confirm_request` function to unblock the
        /// transaction.
        /// </summary>
        /// <param name="tx">The transaction to append the confirm request move call.</param>
        /// <param name="itemType">The type of item having its transaction being unblocked.</param>
        /// <param name="policy">The transaction policy to have the request confirmed.</param>
        /// <param name="request">The request itself.</param>
        public static void ConfirmRequest
        (
            ref TransactionBlock tx,
            string itemType,
            TransactionObjectArgument policy,
            TransactionArgument request
        )
        {
            tx.AddMoveCallTx
            (
                target: SuiMoveNormalizedStructType.FromStr
                (
                    $"{TransferPolicyConstants.TransferPolicyModule}::confirm_request"
                ),
                type_arguments: new SerializableTypeTag[]
                {
                    new SerializableTypeTag(itemType)
                },
                arguments: new TransactionArgument[]
                {
                    tx.AddObjectInput(policy),
                    request
                }
            );
        }

        /// <summary>
        /// Calls the `transfer_policy::remove_rule` function to remove a Rule
        /// from the transfer policy's ruleset.
        /// </summary>
        /// <param name="tx">The transaction to append the remove rule move call.</param>
        /// <param name="itemType">The type of item having its rule removed from.</param>
        /// <param name="ruleType">The type of rule being removed.</param>
        /// <param name="configType">The configuration type for the item.</param>
        /// <param name="policy">The policy having its rule removed from.</param>
        /// <param name="policyCap">The policy cap that owns the policy.</param>
        public static void RemoveTransferPolicyRule
        (
            ref TransactionBlock tx,
            string itemType,
            string ruleType,
            string configType,
            TransactionObjectArgument policy,
            TransactionObjectArgument policyCap
        )
        {
            tx.AddMoveCallTx
            (
                target: SuiMoveNormalizedStructType.FromStr
                (
                    $"{TransferPolicyConstants.TransferPolicyModule}::remove_rule"
                ),
                type_arguments: new SerializableTypeTag[]
                {
                    new SerializableTypeTag(itemType),
                    new SerializableTypeTag(ruleType),
                    new SerializableTypeTag(configType)
                },
                arguments: new TransactionArgument[]
                {
                    tx.AddObjectInput(policy),
                    tx.AddObjectInput(policyCap)
                }
            );
        }
    }
}