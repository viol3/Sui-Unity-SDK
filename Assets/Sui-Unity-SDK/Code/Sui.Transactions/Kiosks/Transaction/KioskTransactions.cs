using OpenDive.BCS;
using Sui.Accounts;
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

        /// <summary>
        /// Call the `kiosk::lock<T>(Kiosk, KioskOwnerCap, TransferPolicy, Item)`
        /// function. Lock an item in the Kiosk.
        ///
        /// Unlike `place` this function requires a `TransferPolicy` to exist
        /// and be passed in. This is done to make sure the item does not get
        /// locked without an option to take it out.
        /// </summary>
        /// <param name="tx">The transaction to append the lock kiosk move call.</param>
        /// <param name="itemType">The type of item being locked in the kiosk</param>
        /// <param name="kiosk">The kiosk having the item being locked into.</param>
        /// <param name="kioskCap">The price cap for the kiosk being locked into with the item.</param>
        /// <param name="policy">The policy for the lock. Required to have an option to take out the item.</param>
        /// <param name="item">The item being locked into the kiosk.</param>
        public static void Lock
        (
            ref TransactionBlock tx,
            string itemType,
            IObjectArgument kiosk,
            IObjectArgument kioskCap,
            IObjectArgument policy,
            IObjectArgument item
        )
        {
            tx.AddMoveCallTx
            (
                target: SuiMoveNormalizedStructType.FromStr($"{KioskConstants.KioskModule}::lock"),
                type_arguments: new SerializableTypeTag[] { new SerializableTypeTag(itemType) },
                arguments: new TransactionArgument[]
                {
                    tx.AddObjectInput(kiosk),
                    tx.AddObjectInput(kioskCap),
                    tx.AddObjectInput(policy),
                    tx.AddObjectInput(item)
                }
            );
        }

        /// <summary>
        /// Call the `kiosk::take<T>(Kiosk, KioskOwnerCap, ID)` function.
        /// Take an item from the Kiosk.
        /// </summary>
        /// <param name="tx">The transaction to append the take kiosk move call.</param>
        /// <param name="itemType">The type of item being taken from the kiosk.</param>
        /// <param name="kiosk">The kiosk having the item being taken away from.</param>
        /// <param name="kioskCap">The kiosk price cap of the kiosk having its item being taken away.</param>
        /// <param name="itemId">The Object ID of the item being taken away.</param>
        /// <returns>A `TransactionObjectArgument` representing the taken item.</returns>
        public static TransactionObjectArgument Take
        (
            ref TransactionBlock tx,
            string itemType,
            IObjectArgument kiosk,
            IObjectArgument kioskCap,
            string itemId
        ) => new TransactionObjectArgument
             (
                 tx.AddMoveCallTx
                 (
                     target: SuiMoveNormalizedStructType.FromStr($"{KioskConstants.KioskModule}::take"),
                     type_arguments: new SerializableTypeTag[] { new SerializableTypeTag(itemType) },
                     arguments: new TransactionArgument[]
                     {
                         tx.AddObjectInput(kiosk),
                         tx.AddObjectInput(kioskCap),
                         tx.AddPure(new AccountAddress(itemId))
                     }
                 )[0]
             );

        /// <summary>
        /// Call the `kiosk::list<T>(Kiosk, KioskOwnerCap, ID, u64)` function.
        /// List an item for sale.
        /// </summary>
        /// <param name="tx">The transaction to append the list kiosk move call.</param>
        /// <param name="itemType">The type of item being placed for sale.</param>
        /// <param name="kiosk">The kiosk that holds the current item.</param>
        /// <param name="kioskCap">The kiosk price cap of the kiosk having its item being listed.</param>
        /// <param name="itemId">The Object ID of the item being listed.</param>
        /// <param name="price">The price of the item.</param>
        public static void List
        (
            ref TransactionBlock tx,
            string itemType,
            IObjectArgument kiosk,
            IObjectArgument kioskCap,
            string itemId,
            ulong price
        )
        {
            tx.AddMoveCallTx
            (
                target: SuiMoveNormalizedStructType.FromStr($"{KioskConstants.KioskModule}::list"),
                type_arguments: new SerializableTypeTag[] { new SerializableTypeTag(itemType) },
                arguments: new TransactionArgument[]
                {
                    tx.AddObjectInput(kiosk),
                    tx.AddObjectInput(kioskCap),
                    tx.AddPure(new AccountAddress(itemId)),
                    tx.AddPure(new U64(price))
                }
            );
        }

        /// <summary>
        /// Call the `kiosk::list<T>(Kiosk, KioskOwnerCap, ID, u64)` function.
        /// List an item for sale.
        /// </summary>
        /// <param name="tx">The transaction to append the delist kiosk move call.</param>
        /// <param name="itemType">The type of item being delisted.</param>
        /// <param name="kiosk">The kiosk that contains the item being delisted.</param>
        /// <param name="kioskCap">The kiosk price cap of the kiosk having its item being delisted.</param>
        /// <param name="itemId">The Object ID of the item being delisted.</param>
        public static void Delist
        (
            ref TransactionBlock tx,
            string itemType,
            IObjectArgument kiosk,
            IObjectArgument kioskCap,
            string itemId
        )
        {
            tx.AddMoveCallTx
            (
                target: SuiMoveNormalizedStructType.FromStr($"{KioskConstants.KioskModule}::delist"),
                type_arguments: new SerializableTypeTag[] { new SerializableTypeTag(itemType) },
                arguments: new TransactionArgument[]
                {
                    tx.AddObjectInput(kiosk),
                    tx.AddObjectInput(kioskCap),
                    tx.AddPure(new AccountAddress(itemId))
                }
            );
        }

        /// <summary>
        /// Call the `kiosk::place_and_list<T>(Kiosk, KioskOwnerCap, Item, u64)` function.
        /// Place an item to the Kiosk and list it for sale.
        /// </summary>
        /// <param name="tx">The transaction to append the place and list move call.</param>
        /// <param name="itemType">The type of item being placed and listed.</param>
        /// <param name="kiosk">The kiosk that contains the item being placed and listed.</param>
        /// <param name="kioskCap">The kiosk cap of the kiosk with the item contained in it being placed and listed.</param>
        /// <param name="item">The item being placed and listed.</param>
        /// <param name="price">The listing price of the item.</param>
        public static void PlaceAndList
        (
            ref TransactionBlock tx,
            string itemType,
            IObjectArgument kiosk,
            IObjectArgument kioskCap,
            IObjectArgument item,
            ulong price
        )
        {
            tx.AddMoveCallTx
            (
                target: SuiMoveNormalizedStructType.FromStr($"{KioskConstants.KioskModule}::place_and_list"),
                type_arguments: new SerializableTypeTag[] { new SerializableTypeTag(itemType) },
                arguments: new TransactionArgument[]
                {
                    tx.AddObjectInput(kiosk),
                    tx.AddObjectInput(kioskCap),
                    tx.AddObjectInput(item),
                    tx.AddPure(new U64(price))
                }
            );
        }
    }
}