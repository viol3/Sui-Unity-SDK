//
//  KioskTransactions.cs
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
            (TransactionObjectArgument, TransactionObjectArgument) kioskResult = CreateKiosk(ref tx);
            ShareKiosk(ref tx, kioskResult.Item1.Argument);
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

        /// <summary>
        /// Call the `kiosk::purchase<T>(Kiosk, ID, Coin<SUI>)` function and receive an Item and
        /// a TransferRequest which needs to be dealt with (via a matching TransferPolicy).
        /// </summary>
        /// <param name="tx">The transaction to append the purchase move call.</param>
        /// <param name="itemType">The type of item being purchased and received.</param>
        /// <param name="kiosk">The kiosk sending the purchased item.</param>
        /// <param name="itemId">The Object ID of the purchased item.</param>
        /// <param name="payment">The type of coin being used to purchase the item.</param>
        /// <returns>A tuple representing the item and transfer request.</returns>
        public static (TransactionObjectArgument, TransactionObjectArgument) Purchase
        (
            ref TransactionBlock tx,
            string itemType,
            IObjectArgument kiosk,
            string itemId,
            IObjectArgument payment
        )
        {
            List<TransactionArgument> purchaseReturnValues = tx.AddMoveCallTx
            (
                target: SuiMoveNormalizedStructType.FromStr($"{KioskConstants.KioskModule}::purchase"),
                type_arguments: new SerializableTypeTag[] { new SerializableTypeTag(itemType) },
                arguments: new TransactionArgument[]
                {
                    tx.AddObjectInput(kiosk),
                    tx.AddPure(new AccountAddress(itemId)),
                    tx.AddObjectInput(payment),
                },
                return_value_count: 2
            );
            return
            (
                new TransactionObjectArgument(purchaseReturnValues[0]),
                new TransactionObjectArgument(purchaseReturnValues[1])
            );
        }

        /// <summary>
        /// Call the `kiosk::withdraw(Kiosk, KioskOwnerCap, Option<u64>)` function and receive a Coin<SUI>.
        /// If the amount is null, then the entire balance will be withdrawn.
        /// </summary>
        /// <param name="tx">The transaction to append the withdraw from kiosk move call.</param>
        /// <param name="kiosk">The kiosk being withdrawn from.</param>
        /// <param name="kioskCap">The purchase cap for the withdrawn kiosk.</param>
        /// <param name="amount">The amount to withdraw.</param>
        /// <returns>The type of coin withdrawn.</returns>
        public static TransactionObjectArgument WithdrawFromKiosk
        (
            ref TransactionBlock tx,
            IObjectArgument kiosk,
            IObjectArgument kioskCap,
            ulong? amount = null
        )
        {
            Serialization amountArg = amount != null ?
                (new Serialization()).SerializeU64((ulong)amount) :
                (new Serialization()).SerializeU8(0);

            TransactionArgument coin = tx.AddMoveCallTx
            (
                target: SuiMoveNormalizedStructType.FromStr($"{KioskConstants.KioskModule}::withdraw"),
                arguments: new TransactionArgument[]
                {
                    tx.AddObjectInput(kiosk),
                    tx.AddObjectInput(kioskCap),
                    tx.AddPure(amountArg.GetBytes())
                }
            )[0];

            return new TransactionObjectArgument(coin);
        }

        /// <summary>
        /// Call the `kiosk::borrow_value<T>(Kiosk, KioskOwnerCap, ID): T` function.
        /// Immutably borrow an item from the Kiosk and return it in the end.
        ///
        /// Requires calling `returnValue` to return the item.
        /// </summary>
        /// <param name="tx">The transaction to append the borrow value move call.</param>
        /// <param name="itemType">The type of item to borrow its value from.</param>
        /// <param name="kiosk">The kiosk that owns the borrowed item.</param>
        /// <param name="kioskCap">The kiosk cap for the kiosk with the borrowed item.</param>
        /// <param name="itemId">The Object ID of the borrowed item.</param>
        /// <returns>A tuple representing the item and the promise.</returns>
        public static (TransactionArgument, TransactionArgument) BorrowValue
        (
            ref TransactionBlock tx,
            string itemType,
            IObjectArgument kiosk,
            IObjectArgument kioskCap,
            string itemId
        )
        {
            List<TransactionArgument> item = tx.AddMoveCallTx
            (
                target: SuiMoveNormalizedStructType.FromStr($"{KioskConstants.KioskModule}::borrow_val"),
                type_arguments: new SerializableTypeTag[] { new SerializableTypeTag(itemType) },
                arguments: new TransactionArgument[]
                {
                    tx.AddObjectInput(kiosk),
                    tx.AddObjectInput(kioskCap),
                    tx.AddPure(new AccountAddress(itemId))
                },
                return_value_count: 2
            );

            return (item[0], item[1]);
        }

        /// <summary>
        /// Call the `kiosk::return_value<T>(Kiosk, Item, Borrow)` function.
        /// Return an item to the Kiosk after it was `borrowValue`-d.
        /// </summary>
        /// <param name="tx">The transaction to append the return value move call.</param>
        /// <param name="itemType">The type of borrowed item being returned.</param>
        /// <param name="kiosk">The kiosk to return the item to.</param>
        /// <param name="item">The borrowed item.</param>
        /// <param name="promise">The promise for the item.</param>
        public static void ReturnValue
        (
            ref TransactionBlock tx,
            string itemType,
            IObjectArgument kiosk,
            TransactionArgument item,
            TransactionArgument promise
        )
        {
            tx.AddMoveCallTx
            (
                target: SuiMoveNormalizedStructType.FromStr($"{KioskConstants.KioskModule}::return_val"),
                type_arguments: new SerializableTypeTag[] { new SerializableTypeTag(itemType) },
                arguments: new TransactionArgument[]
                {
                    tx.AddObjectInput(kiosk),
                    item,
                    promise
                }
            );
        }
    }
}