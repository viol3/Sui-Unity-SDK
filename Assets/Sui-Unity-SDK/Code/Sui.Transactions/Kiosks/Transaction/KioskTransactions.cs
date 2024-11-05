using OpenDive.BCS;
using Sui.Transactions;
using Sui.Types;
using System.Collections.Generic;

namespace Sui.Kiosks.Transaction
{
    public class KioskTransactions
    {
        /// <summary>
        /// Create a new shared Kiosk and returns the [kiosk, kioskOwnerCap] tuple.
        /// </summary>
        /// <param name="tx">Transaction to append new Kiosk move call to.</param>
        /// <returns>The Kiosk and KioskOwnerCap as TransactionObjectArguments</returns>
        public static (TransactionObjectArgument, TransactionObjectArgument) CreateKiosk
        (
            ref TransactionBlock tx
        )
        {
            List<TransactionArgument> kioskReturnValues = tx.AddMoveCallTx
            (
                target: SuiMoveNormalizedStructType.FromStr($"{KioskConstants.KioskModule}::new"),
                return_value_count: 2
            );
            return
            (
                new TransactionObjectArgument(kioskReturnValues[0]),
                new TransactionObjectArgument(kioskReturnValues[1])
            );
        }

        /// <summary>
        /// Calls the `kiosk::new()` function and shares the kiosk.
        /// </summary>
        /// <param name="tx">Transaction to append new kiosk and share move calls to.</param>
        /// <returns>The `kioskOwnerCap` object.</returns>
        public static TransactionObjectArgument CreateKioskAndShare
        (
            ref TransactionBlock tx
        )
        {
            (TransactionObjectArgument, TransactionObjectArgument) kioskResult = KioskTransactions.CreateKiosk(ref tx);
            KioskTransactions.ShareKiosk(ref tx, kioskResult.Item1.Argument);
            return kioskResult.Item2;
        }

        /// <summary>
        /// Converts Transfer Policy to a shared object.
        /// </summary>
        /// <param name="tx">Transaction to append share kiosk move calls to.</param>
        /// <param name="kiosk">The kiosk object to make into a shared object.</param>
        public static void ShareKiosk(ref TransactionBlock tx, TransactionArgument kiosk)
        {
            tx.AddMoveCallTx
            (
                target: SuiMoveNormalizedStructType.FromStr("0x2::transfer::public_share_object"),
                type_arguments: new SerializableTypeTag[] { new SerializableTypeTag(KioskConstants.KioskType) },
                arguments: new TransactionArgument[] { kiosk }
            );
        }

        /// <summary>
        /// Call the `kiosk::place<T>(Kiosk, KioskOwnerCap, Item)` function.
        /// Place an item to the Kiosk.
        /// </summary>
        /// <param name="tx">The transaction to append the place kiosk move call.</param>
        /// <param name="itemType">The type of item being placed.</param>
        /// <param name="kiosk">The kiosk being placed.</param>
        /// <param name="kioskCap">The object representing the kiosk price cap.</param>
        /// <param name="item">The object being placed.</param>
        public static void Place
        (
            ref TransactionBlock tx,
            string itemType,
            IObjectArgument kiosk,
            IObjectArgument kioskCap,
            IObjectArgument item
        )
        {
            tx.AddMoveCallTx
            (
                target: SuiMoveNormalizedStructType.FromStr($"{KioskConstants.KioskModule}::place"),
                type_arguments: new SerializableTypeTag[] { new SerializableTypeTag(itemType) },
                arguments: new TransactionArgument[]
                {
                    tx.AddObjectInput(kiosk),
                    tx.AddObjectInput(kioskCap),
                    tx.AddObjectInput(item)
                }
            ); 
        }
    }
}