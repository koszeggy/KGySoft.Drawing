#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ColorsHelper.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2025 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System;
#if NET7_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif
using System.Drawing;
using System.Drawing.Imaging;
#if NETCOREAPP || NET46_OR_GREATER || NETSTANDARD2_1_OR_GREATER
using System.Numerics;
#endif
using System.Security;

#endregion

namespace KGySoft.Drawing.Imaging
{
    internal static class ColorsHelper
    {
        #region Constants

        /// <summary>
        /// Used when on the current OS the Max16BppValue is less than UInt16.MaxValue, in which case it is a power of two.
        /// For lossless conversions using also a power of two for the scaling. Otherwise, tests fail on Windows.
        /// </summary>
        private const int scalingMax = UInt16.MaxValue + 1;

        #endregion

        #region Fields

        private static readonly object syncRoot = new object();

        private static bool lookupTableSrgb8ToLinear16BitInitialized;
        private static ushort[]? lookupTableSrgb8ToLinear16Bit;
        private static ushort max16BppValue;
#if NETCOREAPP || NET46_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        private static Vector4 max16BppValueF;
        private static Vector4 max16BppInv;
#endif

        private static bool lookupTableLinear16ToSrgb8BitInitialized;
        private static byte[]? lookupTableLinear16ToSrgb8Bit;

        private static bool lookupTableSrgb16ToLinear16BitInitialized;
        private static ushort[]? lookupTableSrgb16ToLinear16Bit;

        private static bool lookupTableLinear16ToSrgb16BitInitialized;
        private static ushort[]? lookupTableLinear16ToSrgb16Bit;

        #endregion

        #region Properties

        /// <summary>
        /// Gets whether this OS uses linear gamma for 48/64bpp bitmap.
        /// Actually we ASSUME that the gamma is linear if the conversion from 32 to 64 bpp is nonlinear (so we detect non-sRGB colors).
        /// </summary>
        internal static bool LinearWideColors => GetLookupTableSrgb8ToLinear16Bit() != null;

        internal static ushort Max16BppValue
        {
            [SecuritySafeCritical]
            get
            {
                Debug.Assert(LinearWideColors, "This property is not expected to be called when wide formats are the same as KnowPixelFormats on the current platform");
                return max16BppValue;
            }
        }

#if NETCOREAPP || NET46_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        internal static Vector4 Max16BppValueF
        {
            [SecuritySafeCritical]
            get
            {
                Debug.Assert(LinearWideColors, "This property is not expected to be called when wide formats are the same as KnowPixelFormats on the current platform");
                return max16BppValueF;
            }
        } 

        internal static Vector4 Max16BppInv
        {
            [SecuritySafeCritical]
            get
            {
                Debug.Assert(LinearWideColors, "This property is not expected to be called when wide formats are the same as KnowPixelFormats on the current platform");
                return max16BppInv;
            }
        }
#endif

        #endregion

        #region Methods

        #region Internal Methods

        [SecuritySafeCritical]
        internal static ushort[]? GetLookupTableSrgb8ToLinear16Bit()
        {
            if (!lookupTableSrgb8ToLinear16BitInitialized)
                InitializeLookupTableSrgb8ToLinear16Bit();
            return lookupTableSrgb8ToLinear16Bit;
        }

        [SecuritySafeCritical]
        internal static byte[] GetLookupTableLinear16ToSrgb8Bit()
        {
            Debug.Assert(LinearWideColors, "This method is not expected to be called when wide formats are the same as KnowPixelFormats on the current platform");
            if (!lookupTableLinear16ToSrgb8BitInitialized)
                InitializeLookupTableLinear16ToSrgb8Bit();
            return lookupTableLinear16ToSrgb8Bit!;
        }

        internal static ushort[] GetLookupTableSrgb16ToLinear16Bit()
        {
            Debug.Assert(LinearWideColors, "This method is not expected to be called when wide formats are the same as KnowPixelFormats on the current platform");
            if (!lookupTableSrgb16ToLinear16BitInitialized)
                InitializeLookupTableSrgb16ToLinear16Bit();
            return lookupTableSrgb16ToLinear16Bit!;
        }

        internal static ushort[] GetLookupTableLinear16ToSrgb16Bit()
        {
            Debug.Assert(LinearWideColors, "This method is not expected to be called when wide formats are the same as KnowPixelFormats on the current platform");
            if (!lookupTableLinear16ToSrgb16BitInitialized)
                InitializeLookupTableLinear16ToSrgb16Bit();
            return lookupTableLinear16ToSrgb16Bit!;
        }

        internal static ushort ToGdiPlusUInt16(byte value)
        {
            Debug.Assert(LinearWideColors, "This method is not expected to be called when wide formats are the same as KnowPixelFormats on the current platform");
            return value == Byte.MaxValue ? max16BppValue
                : max16BppValue == UInt16.MaxValue ? ColorSpaceHelper.ToUInt16(value)
                : (ushort)((uint)ColorSpaceHelper.ToUInt16(value) * max16BppValue / scalingMax);
        }

        internal static ushort ToGdiPlusUInt16(ushort value)
        {
            Debug.Assert(LinearWideColors, "This method is not expected to be called when wide formats are the same as KnowPixelFormats on the current platform");
            return value == UInt16.MaxValue ? max16BppValue
                : max16BppValue == UInt16.MaxValue ? value
                : (ushort)((uint)value * max16BppValue / scalingMax);
        }

        internal static ushort ToGdiPlusUInt16(float value)
        {
            Debug.Assert(LinearWideColors, "This method is not expected to be called when wide formats are the same as KnowPixelFormats on the current platform");

            // Not using Math.Clamp because that does not convert NaN
            value = value * max16BppValue + 0.5f;
            return value < 0f ? (ushort)0
                : value > max16BppValue ? max16BppValue
                : (ushort)value; // including NaN, which will be 0
        }

        internal static byte ToByte(ushort value)
        {
            Debug.Assert(LinearWideColors, "This method is not expected to be called when wide formats are the same as KnowPixelFormats on the current platform");
            return value == max16BppValue ? Byte.MaxValue
                : max16BppValue == UInt16.MaxValue ? ColorSpaceHelper.ToByte(value)
                : ColorSpaceHelper.ToByte((ushort)((uint)value * scalingMax / max16BppValue));
        }

        internal static ushort ToUInt16(ushort value)
        {
            Debug.Assert(LinearWideColors, "This method is not expected to be called when wide formats are the same as KnowPixelFormats on the current platform");
            return value == max16BppValue ? UInt16.MaxValue
                : max16BppValue == UInt16.MaxValue ? value
                : (ushort)((uint)value * scalingMax / max16BppValue);
        }

        internal static float ToFloat(ushort value) => (float)value / max16BppValue;

        #endregion

        #region Private Methods

        [SecurityCritical]
#if NET7_0_OR_GREATER
        [SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "If Bitmap is not supported there will be linear transform")]
#endif
        private static unsafe void InitializeLookupTableSrgb8ToLinear16Bit()
        {
            // Shared sync root is not a problem, lock will be acquired only once per table
            lock (syncRoot)
            {
                // lost race
                if (lookupTableSrgb8ToLinear16BitInitialized)
                    return;

                try
                {
                    // Initializing the lookup table from the result of native SetPixel if that is supported on current operating system.
                    // It will be quite slow but it is executed only once and since it is OS dependent there is no other reliable way.
                    // - On Windows it transforms color channels into a 13 bit range with linear gamma
                    // - On ReactOS the full 16-bit range is used with the same sRGB color space as in case of the 32-bit formats
                    // - On Linux 64bpp formats are not even supported by the current version of libgdiplus but we try to be prepared for future changes
                    using var bmp64 = new Bitmap(256, 1, PixelFormat.Format64bppArgb);
                    for (int i = 0; i < 256; i++)
                        bmp64.SetPixel(i, 0, Color.FromArgb(i, i, i));
                    BitmapData data = bmp64.LockBits(new Rectangle(0, 0, 256, 1), ImageLockMode.ReadOnly, PixelFormat.Format64bppArgb);
                    bool isSrgb = true;
                    try
                    {
                        GdiPlusColor64* row = (GdiPlusColor64*)data.Scan0;
                        lookupTableSrgb8ToLinear16Bit = new ushort[256];
                        for (int i = 0; i < 256; i++)
                        {
                            lookupTableSrgb8ToLinear16Bit[i] = *(ushort*)&row[i]; // row[i].B
                            isSrgb = isSrgb && lookupTableSrgb8ToLinear16Bit[i] == ColorSpaceHelper.ToUInt16((byte)i);
                        }

                        // On this OS there is no need for a lookup table - the sRGB Color64 with KnownPixelFormats can be used (eg. ReactOS)
                        if (isSrgb)
                            lookupTableSrgb8ToLinear16Bit = null;
                    }
                    finally
                    {
                        bmp64.UnlockBits(data);
                    }

                    max16BppValue = lookupTableSrgb8ToLinear16Bit?[lookupTableSrgb8ToLinear16Bit.Length - 1] ?? UInt16.MaxValue;
#if NETCOREAPP || NET46_OR_GREATER || NETSTANDARD2_1_OR_GREATER
                    max16BppValueF = new Vector4(max16BppValue);
                    max16BppInv = new Vector4(1f / max16BppValue);
#endif
                }
                catch (Exception e) when (e is not StackOverflowException)
                {
                    // catching even OutOfMemoryException because Gdip.StatusException() can throw it for unsupported formats
                    lookupTableSrgb8ToLinear16Bit = null;
                    max16BppValue = UInt16.MaxValue;
                }
                finally
                {
                    lookupTableSrgb8ToLinear16BitInitialized = true;
                }
            }
        }

        [SecurityCritical]
#if NET7_0_OR_GREATER
        [SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "If Bitmap is not supported there will be linear transform")]
#endif
        private static unsafe void InitializeLookupTableLinear16ToSrgb8Bit()
        {
            Debug.Assert(LinearWideColors, "This method is not expected to be called when wide formats are the same as KnowPixelFormats on the current platform");

            // Shared sync root is not a problem, lock will be acquired only once per table
            lock (syncRoot)
            {
                // lost race
                if (lookupTableLinear16ToSrgb8BitInitialized)
                    return;

                lookupTableLinear16ToSrgb8Bit = new byte[max16BppValue + 1];

                try
                {
                    // On Windows this will be 8192 (13bpp)
                    Size size = new Size(256, (int)Math.Ceiling(((double)max16BppValue + 1) / 256));

                    // Initializing a grayscale 64 bpp image from deep color shades.
                    using var bmp64 = new Bitmap(size.Width, size.Height, PixelFormat.Format64bppArgb);
                    BitmapData data = bmp64.LockBits(new Rectangle(Point.Empty, size), ImageLockMode.WriteOnly, PixelFormat.Format64bppArgb);
                    try
                    {
                        GdiPlusColor64* row = (GdiPlusColor64*)data.Scan0;
                        for (int i = 0, x = 0; i <= max16BppValue; i++, x++)
                        {
                            if (x == size.Width)
                            {
                                x = 0;
                                row = (GdiPlusColor64*)((byte*)row + data.Stride);
                            }

                            // ReSharper disable once PossibleNullReferenceException - row is not null
                            row[x] = new GdiPlusColor64(max16BppValue, (ushort)i, (ushort)i, (ushort)i);
                        }
                    }
                    finally
                    {
                        bmp64.UnlockBits(data);
                    }

                    // Initializing the lookup table from the result of native GetPixel if that is supported on current operating system.
                    // It will be quite slow but it is executed only once and since it is OS dependent there is no other reliable way.
                    for (int i = 0, x = 0, y = 0; i <= max16BppValue; i++, x++)
                    {
                        if (x == size.Width)
                        {
                            x = 0;
                            y++;
                        }

                        Color translatedColor = bmp64.GetPixel(x, y);
                        lookupTableLinear16ToSrgb8Bit[i] = translatedColor.R;
                    }
                }
                catch (Exception e) when (e is not StackOverflowException)
                {
                    // Not this IS a problem because initialization of the other direction in GetLookupTableSrgb8ToLinear16Bit was successful.
                    // Reinitializing the table by the official Linear -> sRGB formula
                    for (int i = 0; i <= max16BppValue; i++)
                        lookupTableLinear16ToSrgb8Bit[i] = ColorSpaceHelper.ToByte(ColorSpaceHelper.LinearToSrgb(ToFloat((ushort)i)));
                }
                finally
                {
                    lookupTableLinear16ToSrgb8BitInitialized = true;
                }
            }
        }

        private static void InitializeLookupTableSrgb16ToLinear16Bit()
        {
            Debug.Assert(LinearWideColors, "This method is not expected to be called when wide formats are the same as KnowPixelFormats on the current platform");

            // Shared sync root is not a problem, lock will be acquired only once per table
            lock (syncRoot)
            {
                // lost race
                if (lookupTableSrgb16ToLinear16BitInitialized)
                    return;

                // The conversion is performed by the official sRGB -> Linear formula.
                // NOT checking if we are actually consistent with the native conversion table but assuming linear color space if LinearWideColors returns true.
                lookupTableSrgb16ToLinear16Bit = new ushort[UInt16.MaxValue + 1];
                for (int i = 0; i < lookupTableSrgb16ToLinear16Bit.Length; i++)
                    lookupTableSrgb16ToLinear16Bit[i] = ToGdiPlusUInt16(ColorSpaceHelper.SrgbToLinear(ColorSpaceHelper.ToFloat((ushort)i)));

                lookupTableSrgb16ToLinear16BitInitialized = true;
            }
        }

        private static void InitializeLookupTableLinear16ToSrgb16Bit()
        {
            Debug.Assert(LinearWideColors, "This method is not expected to be called when wide formats are the same as KnowPixelFormats on the current platform");

            // Shared sync root is not a problem, lock will be acquired only once per table
            lock (syncRoot)
            {
                // lost race
                if (lookupTableLinear16ToSrgb16BitInitialized)
                    return;

                // The conversion is performed by the official Linear -> sRGB formula.
                // NOT checking if we are actually consistent with the native conversion table but assuming linear color space if LinearWideColors returns true.
                int count = max16BppValue + 1;
                lookupTableLinear16ToSrgb16Bit = new ushort[count];
                for (int i = 0; i < count; i++)
                    lookupTableLinear16ToSrgb16Bit[i] = ColorSpaceHelper.ToUInt16(ColorSpaceHelper.LinearToSrgb(ToFloat((ushort)i)));

                lookupTableLinear16ToSrgb16BitInitialized = true;
            }
        }

        #endregion

        #endregion
    }
}
