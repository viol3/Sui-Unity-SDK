//
//  KioskItem.cs
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

using Sui.Rpc.Models;

namespace Sui.Kiosks.Kiosk.Types
{
    /// <summary>
    /// A dynamic field `Item { ID }` attached to the Kiosk.
    /// Holds an Item `T`. The type of the item is known upfront.
    /// </summary>
    public class KioskItem
    {
        /// <summary>
        /// The ID of the Item.
        /// </summary>
        public string ObjectID { get; set; }

        /// <summary>
        /// The type of the Item.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Whether the item is Locked (there must be a `Lock` Dynamic Field).
        /// </summary>
        public bool IsLocked { get; set; }

        /// <summary>
        /// Optional listing.
        /// </summary>
        public KioskListing Listing { get; set; }

        /// <summary>
        /// The ID of the kiosk the item is placed in.
        /// </summary>
        public string KioskID { get; set; }

        /// <summary>
        /// Optional Kiosk Data.
        /// </summary>
        public ObjectData Data { get; set; }

        public KioskItem
        (
            string ObjectID,
            string Type,
            bool IsLocked,
            string KioskID,
            KioskListing Listing = null,
            ObjectData Data = null
        )
        {
            this.ObjectID = ObjectID;
            this.Type = Type;
            this.IsLocked = IsLocked;
            this.Listing = Listing;
            this.KioskID = KioskID;
            this.Data = Data;
        }
    }
}