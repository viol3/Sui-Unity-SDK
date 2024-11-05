//
//  KioskType.cs
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

public class KioskType : ISerializable
{
    public AccountAddress ID { get; set; }

    public ulong Profits { get; set; }

    public AccountAddress Owner { get; set; }

    public uint ItemCount { get; set; }

    public bool AllowExtensions { get; set; }

    public KioskType
    (
        AccountAddress ID,
        ulong Profits,
        AccountAddress Owner,
        uint ItemCount,
        bool AllowExtensions
    )
    {
        this.ID = ID;
        this.Profits = Profits;
        this.Owner = Owner;
        this.ItemCount = ItemCount;
        this.AllowExtensions = AllowExtensions;
    }

    public void Serialize(Serialization serializer)
    {
        serializer.Serialize(this.ID);
        serializer.Serialize(this.Profits);
        serializer.Serialize(this.Owner);
        serializer.Serialize(this.ItemCount);
        serializer.Serialize(this.AllowExtensions);
    }

    public static ISerializable Deserialize(Deserialization deserializer)
        => new KioskType
           (
               (AccountAddress.Deserialize(deserializer) as AccountAddress),
               (U64.Deserialize(deserializer) as U64).Value,
               (AccountAddress.Deserialize(deserializer) as AccountAddress),
               ((U32.Deserialize(deserializer) as U32).Value),
               (Bool.Deserialize(deserializer) as Bool).Value
           );
}

