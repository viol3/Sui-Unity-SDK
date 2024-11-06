//
//  PersonalKioskTransactions.cs
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

namespace Sui.Kiosks.Transaction
{
    public class PersonalKioskTransactions
    {
        public static TransactionObjectArgument ConvertToPersonalTx
        (
            ref TransactionBlock tx,
            IObjectArgument kiosk,
            IObjectArgument kioskOwnerCap,
            string packageId
        ) => new TransactionObjectArgument
             (
                 tx.AddMoveCallTx
                 (
                     target: SuiMoveNormalizedStructType.FromStr($"{packageId}::personal_kiosk::new"),
                     arguments: new TransactionArgument[]
                     {
                         tx.AddObjectInput(kiosk),
                         tx.AddObjectInput(kioskOwnerCap)
                     }
                 )[0]
             );

        public static void TransferPersonalCapTx
        (
            ref TransactionBlock tx,
            TransactionObjectArgument personalKioskCap,
            string packageId
        )
        {
            tx.AddMoveCallTx
            (
                target: SuiMoveNormalizedStructType.FromStr($"{packageId}::personal_kiosk::transfer_to_sender"),
                arguments: new TransactionArgument[] { personalKioskCap.Argument }
            );
        }
    }
}