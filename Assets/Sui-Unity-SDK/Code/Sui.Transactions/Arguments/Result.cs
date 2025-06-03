//
//  Result.cs
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

namespace Sui.Transactions
{
    /// <summary>
    /// The result of another transaction command (from `ProgrammableTransactionBlock` transactions).
    ///
    /// Each transaction command produces a (possibly empty) array of values.
    /// The type of the value can be any arbitrary Move type, so unlike inputs,
    /// the values are not limited to objects or pure values.
    /// The number of results generated and their types are specific to each transaction command.
    ///
    ///  The specifics for each command can be found in the section for that command, but in summary:
    ///  - MoveCall: the number of results and their types are determined by
    ///     the Move function being called.Move functions that return references
    ///     are not supported at this time.
    ///  - SplitCoins: produces (one or more) coins from a single coin.
    ///     The type of each coin is sui::coin::Coin<T> where the specific
    ///     coin type T matches the coin being split.
    ///  - Publish: returns the upgrade capability, sui::package::UpgradeCap,
    ///     for the newly published package.
    ///  - Upgrade: returns the upgrade receipt, sui::package::UpgradeReceipt,
    ///     for the upgraded package.
    ///  - TransferObjects and MergeCoins do not produce any results (an empty
    ///     result vector).
    ///
    /// Result(u16) is a special form of NestedResult where Result(i)
    /// is roughly equivalent to NestedResult(i, 0).
    /// Unlike NestedResult(i, 0), Result(i), however,
    /// this errors if the result array at index i is empty or has more than one value.
    /// The ultimate intention of Result is to allow accessing the entire result array,
    /// but that is not yet supported.
    /// So in its current state, NestedResult can be used instead of Result in all circumstances.
    /// 
    /// Each command takes Arguments that specify the input or result being used.
    /// 
    /// NOTE: Inputs and results are the two types of values you can use in transaction commands.
    ///     - Inputs are the values that are provided to the PTB
    ///     - Results are the values that are produced by the PTB commands
    ///
    /// NOTE: The inputs are either objects or simple Move values,
    ///     and the results are arbitrary Move values (including objects).
    ///
    /// NOTE: Because C# is statically typed we define an index value beforehand.
    /// </summary>
    public class Result : ITransactionArgument
    {
        /// <summary>
        /// The index of the transactions's resulting output.
        /// </summary>
        public int Index { get; set; }

        public Result(int index)
        {
            Index = index;
        }

        public void Serialize(Serialization serializer)
            => serializer.SerializeU16((ushort)this.Index);

        public static ISerializable Deserialize(Deserialization deserializer)
            => new Result(deserializer.DeserializeU16().Value);
    }
}