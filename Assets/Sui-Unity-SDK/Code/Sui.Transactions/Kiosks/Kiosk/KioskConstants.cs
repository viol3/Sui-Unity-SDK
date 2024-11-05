//
//  KioskConstants.cs
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

public class KioskConstants
{
    /// <summary>
    /// The Kiosk module.
    /// </summary>
    public static string KioskModule = "0x2::kiosk";

    /// <summary>
    /// The Kiosk type.
    /// </summary>
    public static string KioskType = $"{KioskConstants.KioskModule}::Kiosk";

    /// <summary>
    /// The Kiosk Owner Cap Type.
    /// </summary>
    public static string KioskOwnerCap = $"{KioskConstants.KioskModule}::KioskOwnerCap";

    /// <summary>
    /// The Kiosk Item Type.
    /// </summary>
    public static string KioskItem = $"{KioskConstants.KioskModule}::Item";

    /// <summary>
    /// The Kiosk Listing Type.
    /// </summary>
    public static string KioskListing = $"{KioskConstants.KioskModule}::Listing";

    /// <summary>
    /// The Kiosk Lock Type.
    /// </summary>
    public static string KioskLock = $"{KioskConstants.KioskModule}::Lock";

    /// <summary>
    /// The Kiosk PurchaseCap Type.
    /// </summary>
    public static string KioskPurchaseCap = $"{KioskConstants.KioskModule}::PurchaseCap";
}

