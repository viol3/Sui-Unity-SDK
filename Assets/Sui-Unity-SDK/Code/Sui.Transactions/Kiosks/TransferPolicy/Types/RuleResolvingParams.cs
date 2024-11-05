//
//  RulesResolvingParams.cs
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
using OpenDive.BCS;
using Sui.Transactions;

namespace Sui.Kiosks.TransferPolicy.Types
{
    /// <summary>
    /// The object a Rule resolving function accepts
    /// It can accept a set of fixed fields, that are part of every purchase flow
    /// as well any extra arguments to resolve custom policies!
    /// Each rule resolving function should check that the key it's seeking is in the object
    /// e.g. `if(!'my_key' in ruleParams!) throw new Error("Can't resolve that rule!")`
    /// </summary>
    public class RulesResolvingParams
    {
        public TransactionBlock Transaction { get; set; }

        public string ItemType { get; set; }

        public string ItemID { get; set; }

        public string Price { get; set; }

        public IObjectArgument PolicyID { get; set; }

        public IObjectArgument SellerKiosk { get; set; }

        public IObjectArgument Kiosk { get; set; }

        public IObjectArgument KioskCap { get; set; }

        public TransactionObjectArgument TransferRequest { get; set; }

        public TransactionObjectArgument PurchasedItem { get; set; }

        public string PackageID { get; set; }

        /// <summary>
        /// Contains more possible {key, values} to pass for custom rules.
        /// </summary>
        public Dictionary<string, ISerializable> ExtraArgs { get; set; }

        public RulesResolvingParams
        (
            TransactionBlock Transaction,
            string ItemType,
            string ItemID,
            string Price,
            IObjectArgument PolicyID,
            IObjectArgument SellerKiosk,
            IObjectArgument Kiosk,
            IObjectArgument KioskCap,
            TransactionObjectArgument TransferRequest,
            TransactionObjectArgument PurchasedItem,
            string PackageID,
            Dictionary<string, ISerializable> ExtraArgs
        )
        {
            this.Transaction = Transaction;
            this.ItemType = ItemType;
            this.ItemID = ItemID;
            this.Price = Price;
            this.PolicyID = PolicyID;
            this.SellerKiosk = SellerKiosk;
            this.Kiosk = Kiosk;
            this.KioskCap = KioskCap;
            this.TransferRequest = TransferRequest;
            this.PurchasedItem = PurchasedItem;
            this.PackageID = PackageID;
            this.ExtraArgs = ExtraArgs;
        }
    }
}