//
//  TransferPolicyConstants.cs
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

namespace Sui.Kiosks.TransferPolicy
{
    public class TransferPolicyConstants
    {
        /// <summary>
        /// The Transfer Policy module.
        /// </summary>
        public static string TransferPolicyModule = "0x2::transfer_policy";

        /// <summary>
        /// Name of the event emitted when a TransferPolicy for T is created.
        /// </summary>
        public static string TransferPolicyCreatedEvent = $"{TransferPolicyConstants.TransferPolicyModule}::TransferPolicyCreated";

        /// <summary>
        /// The Transfer Policy Type.
        /// </summary>
        public static string TransferPolicyType = $"{TransferPolicyConstants.TransferPolicyModule}::TransferPolicy";

        /// <summary>
        /// The Transfer Policy Cap Type.
        /// </summary>
        public static string TransferPolicyCapType = $"{TransferPolicyConstants.TransferPolicyModule}::TransferPolicyCap";

        /// <summary>
        /// The Kiosk Lock Rule.
        /// </summary>
        public static string KioskLockRule = "iosk_lock_rule::Rule";

        /// <summary>
        /// The Royalty rule.
        /// </summary>
        public static string RoyaltyRule = "royalty_rule::Rule";
    }
}