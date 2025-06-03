//
//  ProgrammableTransaction.cs
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

using System.Linq;
using Unity.Plastic.Newtonsoft.Json;
using OpenDive.BCS;
using Sui.Types;

namespace Sui.Transactions
{
    /// <summary>
    /// Represents a Sui programmable transaction that can execute
    /// different types of commands.
    /// </summary>
    public class ProgrammableTransaction : ITransactionKind
    {
        /// <summary>
        /// Can be a pure type (native BCS),, `Input::Pure(PureArg)`
        ///     or a Sui object (shared, or ImmutableOwned) `Input::Object(ObjectArg)`.
        /// For object inputs, the metadata needed differs depending on the type of ownership of the object
        /// Both type extend ISerialzable interface.
        ///
        /// NOTE: For historical reasons, `Input` is `CallArg` in the Rust implementation.
        /// NOTE: For inputs, there is a single array, but for results,
        ///     there is an array for each individual transaction command, creating a 2D-array of result values.
        ///     
        /// You can access these values by borrowing (mutably or immutably),
        /// by copying (if the type permits), or by moving (which takes the
        /// value out of the array without re-indexing).
        /// 
        /// </summary>
        [JsonProperty("inputs")]
        public CallArg[] Inputs { get; set; }

        /// <summary>
        /// Holds a set of transaction commands,
        ///     e.g. MoveCallTransaction, TransferObjectsTransaction, etc.
        /// </summary>
        [JsonProperty("transactions")]
        public Command[] Transactions { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <param name="inputs">
        /// Inputs are the values that are provided to the PTB,
        /// and can use in transaction commands. 
        /// 
        /// The inputs value is a vector of arguments, [`Input`].
        /// These arguments are either objects or pure values that you can use in the commands.
        /// The objects are either owned by the sender or are shared/immutable objects.
        /// The pure values represent simple Move values, such as u64 or String values,
        /// which you can be construct purely from their bytes.
        ///
        /// NOTE: For historical reasons, `Input` is `CallArg` in the Rust implementation.
        /// </param>
        /// 
        /// <param name="transactions">
        /// The commands value is a vector of commands, [`Command`]. The possible commands are:
        /// - `TransferObjects` sends multiple(one or more) objects to a specified address.
        /// - `SplitCoins` splits off multiple(one or more) coins from a single coin.
        ///         It can be any sui::coin::Coin<_> object.
        /// - `MergeCoins` merges multiple(one or more) coins into a single coin.
        ///         Any sui::coin::Coin<_> objects can be merged, as long as they are all of the same type.
        /// - `MakeMoveVec` creates a vector (potentially empty) of Move values.
        ///         This is used primarily to construct vectors of Move values to be used as arguments to MoveCall.
        /// - `MoveCall` invokes either an entry or a public Move function in a published package.
        /// - `Publish` creates a new package and calls the init function of each module in the package.
        /// - `Upgrade` upgrades an existing package.
        ///         The upgrade is gated by the sui::package::UpgradeCap for that package.
        /// </param>
        public ProgrammableTransaction(CallArg[] inputs, Command[] transactions)
        {
            this.Inputs = inputs;
            this.Transactions = transactions;
        }

        public void Serialize(Serialization serializer)
        {
            serializer.Serialize(new Sequence(this.Inputs));
            serializer.Serialize(new Sequence(this.Transactions));
        }

        public static ISerializable Deserialize(Deserialization deserializer)
            => new ProgrammableTransaction
               (
                   deserializer.DeserializeSequence(typeof(CallArg)).Values.Cast<CallArg>().ToArray(),
                   deserializer.DeserializeSequence(typeof(Command)).Values.Cast<Command>().ToArray()
               );
    }
}