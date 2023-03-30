#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ColorsHelper.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2023 - All Rights Reserved
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
using System.Security;

#endregion

namespace KGySoft.Drawing.Imaging
{
    internal static class ColorsHelper
    {
        #region Fields

        private static readonly object syncRoot = new object();

        private static bool lookupTable8To16BppInitialized;
        private static ushort[]? lookupTable8To16Bpp;
        private static ushort max16BppValue;

        private static bool lookupTable16To8BppInitialized;
        private static byte[]? lookupTable16To8Bpp;

        #endregion

        #region Properties

        internal static ushort Max16BppValue
        {
            [SecuritySafeCritical]
            get
            {
                if (!lookupTable8To16BppInitialized)
                    InitializeLookupTable8To16Bpp();
                return max16BppValue;
            }
        }

        #endregion

        #region Methods

        #region Internal Methods

        [SecuritySafeCritical]
        internal static ushort[]? GetLookupTable8To16Bpp()
        {
            if (!lookupTable8To16BppInitialized)
                InitializeLookupTable8To16Bpp();
            return lookupTable8To16Bpp;
        }

        [SecuritySafeCritical]
        internal static byte[]? GetLookupTable16To8Bpp()
        {
            if (!lookupTable16To8BppInitialized)
                InitializeLookupTable16To8Bpp();
            return lookupTable16To8Bpp;
        }

        #endregion

        #region Private Methods

        [SecurityCritical]
#if NET7_0_OR_GREATER
        [SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "If Bitmap is not supported there will be linear transform")]
#endif
        private static unsafe void InitializeLookupTable8To16Bpp()
        {
            // Shared sync root is not a problem, lock will be acquired only once per table
            lock (syncRoot)
            {
                // lost race
                if (lookupTable8To16BppInitialized)
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
                        lookupTable8To16Bpp = new ushort[256];
                        for (int i = 0; i < 256; i++)
                        {
                            lookupTable8To16Bpp[i] = *(ushort*)&row[i]; // row[i].B
                            isSrgb = isSrgb && lookupTable8To16Bpp[i] == ColorSpaceHelper.ToUInt16((byte)i);
                        }

                        if (isSrgb)
                            lookupTable8To16Bpp = null;
                    }
                    finally
                    {
                        bmp64.UnlockBits(data);
                    }

                    max16BppValue = lookupTable8To16Bpp?[lookupTable8To16Bpp.Length - 1] ?? UInt16.MaxValue;
                    lookupTable8To16BppInitialized = true;
                }
                catch (Exception e) when (e is not StackOverflowException)
                {
                    // catching even OutOfMemoryException because Gdip.StatusException() can throw it for unsupported formats
                    lookupTable8To16Bpp = null;
                    max16BppValue = UInt16.MaxValue;
                    lookupTable8To16BppInitialized = true;
                }
            }
        }

        [SecurityCritical]
#if NET7_0_OR_GREATER
        [SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "If Bitmap is not supported there will be linear transform")]
#endif
        private static unsafe void InitializeLookupTable16To8Bpp()
        {
            // Shared sync root is not a problem, lock will be acquired only once per table
            lock (syncRoot)
            {
                // lost race
                if (lookupTable16To8BppInitialized)
                    return;

                // Initializing the other direction first.
                if (!lookupTable8To16BppInitialized)
                    InitializeLookupTable8To16Bpp();

                // if there is no table for the inverse operation we don't need it here either
                if (lookupTable8To16Bpp == null)
                {
                    lookupTable16To8BppInitialized = true;
                    return;
                }

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
                    lookupTable16To8Bpp = new byte[max16BppValue + 1];
                    for (int i = 0, x = 0, y = 0; i <= max16BppValue; i++, x++)
                    {
                        if (x == size.Width)
                        {
                            x = 0;
                            y++;
                        }

                        Color translatedColor = bmp64.GetPixel(x, y);
                        lookupTable16To8Bpp[i] = translatedColor.R;
                    }

                    lookupTable16To8BppInitialized = true;
                }
                catch (Exception e) when (e is not StackOverflowException)
                {
                    // catching even OutOfMemoryException because Gdip.StatusException() can throw it for unsupported formats
                    lookupTable16To8Bpp = null;
                    lookupTable16To8BppInitialized = true;
                }
            }
        }

        #endregion

        #endregion
    }
}
