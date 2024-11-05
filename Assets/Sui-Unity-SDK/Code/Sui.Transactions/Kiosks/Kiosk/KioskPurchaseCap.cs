//
//  KioskPurchaseCap.cs
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

namespace Sui.Kiosks.Kiosk
{
    /// <summary>
    /// PurchaseCap object fields (for BCS queries).
    /// </summary>
    public class KioskPurchaseCap : ISerializable
    {
        public AccountAddress ID { get; set; }

        public AccountAddress KioskID { get; set; }

        public AccountAddress ItemID { get; set; }

        public ulong MinPrice { get; set; }

        public KioskPurchaseCap
        (
            AccountAddress ID,
            AccountAddress KioskID,
            AccountAddress ItemID,
            ulong MinPrice
        )
        {
            this.ID = ID;
            this.KioskID = KioskID;
            this.ItemID = ItemID;
            this.MinPrice = MinPrice;
        }

        public void Serialize(Serialization serializer)
        {
            serializer.Serialize(this.ID);
            serializer.Serialize(this.KioskID);
            serializer.Serialize(this.ItemID);
            serializer.Serialize(this.MinPrice);
        }

        public static ISerializable Deserialize(Deserialization deserializer)
            => new KioskPurchaseCap
            (
                AccountAddress.Deserialize(deserializer) as AccountAddress,
                AccountAddress.Deserialize(deserializer) as AccountAddress,
                AccountAddress.Deserialize(deserializer) as AccountAddress,
                (U64.Deserialize(deserializer) as U64).Value
            );
    }
}