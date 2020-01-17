#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: Color32Extensions.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2019 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution. If not, then this file is considered as
//  an illegal copy.
//
//  Unauthorized copying of this file, via any medium is strictly prohibited.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System;
using System.Drawing;
using System.Drawing.Imaging;

#endregion

namespace KGySoft.Drawing.Imaging
{
    internal static class Color32Extensions
    {
        #region Constants

        internal const float RLum = 0.299f;
        internal const float GLum = 0.587f;
        internal const float BLum = 0.114f;

        #endregion

        #region Fields

        private static readonly object syncRoot = new object();

        private static bool lookupTable8To16BppInitialized;
        private static ushort[] lookupTable8To16Bpp;
        private static ushort max16BppValue;
        
        private static bool lookupTable16To8BppInitialized;
        private static byte[] lookupTable16To8Bpp;

        #endregion

        #region Methods

        #region Properties

        private static ushort Max16BppValue
        {
            get
            {
                if (!lookupTable8To16BppInitialized)
                    InitializeLookupTable8To16Bpp();
                return max16BppValue;
            }
        }

        #endregion

        #region Internal Methods

        internal static Color32 ToPremultiplied(this Color32 c)
        {
            if (c.A == Byte.MaxValue)
                return c;
            if (c.A == 0)
                return default;

            return new Color32(c.A,
                (byte)(c.R * c.A / Byte.MaxValue),
                (byte)(c.G * c.A / Byte.MaxValue),
                (byte)(c.B * c.A / Byte.MaxValue));
        }

        internal static Color32 ToStraight(this Color32 c)
        {
            if (c.A == Byte.MaxValue)
                return c;
            if (c.A == 0)
                return default;

            return new Color32(
                c.A,
                c.A == 0 ? (byte)0 : (byte)(c.R * Byte.MaxValue / c.A),
                c.A == 0 ? (byte)0 : (byte)(c.G * Byte.MaxValue / c.A),
                c.A == 0 ? (byte)0 : (byte)(c.B * Byte.MaxValue / c.A));
        }

        internal static Color64 ToArgb64(this Color32 c)
        {
            if (!lookupTable8To16BppInitialized)
                InitializeLookupTable8To16Bpp();

            if (lookupTable8To16Bpp == null)
                return new Color64(c);

            // alpha is always scaled linearly whereas other components may have a gamma correction in the lookup table
            ushort a = c.A == Byte.MaxValue ? max16BppValue
                : max16BppValue == UInt16.MaxValue ? (ushort)((c.A << 8) | c.A)
                : (ushort)(((c.A << 8) | c.A) * max16BppValue / UInt16.MaxValue);
            return new Color64(a, lookupTable8To16Bpp[c.R], lookupTable8To16Bpp[c.G], lookupTable8To16Bpp[c.B]);
        }

        internal static Color32 ToArgb32(this Color64 c)
        {
            if (!lookupTable16To8BppInitialized)
                InitializeLookupTable16To8Bpp();

            if (lookupTable16To8Bpp == null)
                return c.ToColor32();

            // alpha is always scaled linearly whereas other components may have a gamma correction in the lookup table
            byte a = c.A == max16BppValue ? Byte.MaxValue
                : max16BppValue == UInt16.MaxValue ? (byte)(c.A >> 8)
                : (byte)((c.A * UInt16.MaxValue / max16BppValue) >> 8);
            return new Color32(a, lookupTable16To8Bpp[c.R], lookupTable16To8Bpp[c.G], lookupTable16To8Bpp[c.B]);
        }

        internal static Color64 ToPArgb64(this Color32 c)
        {
            if (c.A == 0)
                return default;

            Color64 c64 = c.ToArgb64();
            if (c.A == Byte.MaxValue)
                return c64;

            return new Color64(c64.A,
                (ushort)(c64.R * c64.A / max16BppValue),
                (ushort)(c64.G * c64.A / max16BppValue),
                (ushort)(c64.B * c64.A / max16BppValue));
        }

        internal static Color32 ToStraightArgb32(this Color64 c)
        {
            if (c.A == 0)
                return default;

            ushort max = Max16BppValue;
            Color64 straight = new Color64(
                c.A,
                c.A == 0 ? (ushort)0 : (ushort)Math.Min(max, c.R * max / c.A),
                c.A == 0 ? (ushort)0 : (ushort)Math.Min(max, c.G * max / c.A),
                c.A == 0 ? (ushort)0 : (ushort)Math.Min(max, c.B * max / c.A));
            return ToArgb32(straight);
        }

        internal static Color48 ToRgb48(this Color32 c)
        {
            if (!lookupTable8To16BppInitialized)
                InitializeLookupTable8To16Bpp();

            return lookupTable8To16Bpp == null
                ? new Color48(c)
                : new Color48(lookupTable8To16Bpp[c.R], lookupTable8To16Bpp[c.G], lookupTable8To16Bpp[c.B]);
        }

        internal static Color32 ToArgb32(this Color48 c)
        {
            if (!lookupTable16To8BppInitialized)
                InitializeLookupTable16To8Bpp();

            return lookupTable16To8Bpp == null
                ? c.ToColor32()
                : new Color32(lookupTable16To8Bpp[c.R], lookupTable16To8Bpp[c.G], lookupTable16To8Bpp[c.B]);
        }

        internal static byte GetBrightness(this Color32 c)
            => c.R == c.G && c.R == c.B
                ? c.R
                : (byte)(c.R * RLum + c.G * GLum + c.B * BLum);

        internal static Color32 BlendWithBackground(this Color32 c, Color32 backColor)
        {
            // The blending is applied only to the color and not the resulting alpha, which always has the source alpha
            // so its distance still can be measured.
            if (c.A == 0)
                return Color32.FromArgb(Byte.MaxValue, backColor);
            float alpha = c.A / 255f;
            return new Color32(Byte.MaxValue,
                (byte)(c.R * alpha + backColor.R * (1 - alpha)),
                (byte)(c.G * alpha + backColor.G * (1 - alpha)),
                (byte)(c.B * alpha + backColor.B * (1 - alpha)));
        }

        #endregion

        #region Private Methods

        private static unsafe void InitializeLookupTable8To16Bpp()
        {
            // Shared sync root is not a problem, lock will acquired only once per table
            lock (syncRoot)
            {
                // lost race
                if (lookupTable8To16BppInitialized)
                    return;

                try
                {
                    // Initializing the lookup table from the result of native SetPixel if that is supported on current operating system.
                    // It will be quite slow but it is executed only once and since it is OS dependent there is no other reliable way.
                    // On Windows it transforms color channels into a 13 bit range with gamma correction 1/2.2 (with some cheating for darker shades).
                    using var bmp64 = new Bitmap(256, 1, PixelFormat.Format64bppArgb);
                    for (int i = 0; i < 256; i++)
                        bmp64.SetPixel(i, 0, Color.FromArgb(i, i, i));
                    BitmapData data = bmp64.LockBits(new Rectangle(0, 0, 256, 1), ImageLockMode.ReadOnly, PixelFormat.Format64bppArgb);
                    bool isLinear = true;
                    try
                    {
                        Color64* row = (Color64*)data.Scan0;
                        lookupTable8To16Bpp = new ushort[256];
                        for (int i = 0; i < 256; i++)
                        {
                            // ReSharper disable once PossibleNullReferenceException - row is not null
                            lookupTable8To16Bpp[i] = row[i].R;
                            isLinear = isLinear && row[i] == new Color64(new Color32((byte)i, (byte)i, (byte)i, (byte)i));
                        }

                        if (isLinear)
                            lookupTable8To16Bpp = null;
                    }
                    finally
                    {
                        bmp64.UnlockBits(data);
                    }

                    max16BppValue = lookupTable8To16Bpp?[lookupTable8To16Bpp.Length - 1] ?? UInt16.MaxValue;
                    lookupTable8To16BppInitialized = true;
                }
                catch (Exception e) when (!(e is StackOverflowException))
                {
                    // catching even OutOfMemoryException because Gdip.StatusException() can throw it for unsupported formats
                    lookupTable8To16Bpp = null;
                    max16BppValue = UInt16.MaxValue;
                    lookupTable8To16BppInitialized = true;
                }
            }
        }

        private static unsafe void InitializeLookupTable16To8Bpp()
        {
            // Shared sync root is not a problem, lock will acquired only once per table
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
                        Color64* row = (Color64*)data.Scan0;
                        for (int i = 0, x = 0; i <= max16BppValue; i++, x++)
                        {
                            if (x == size.Width)
                            {
                                x = 0;
                                row = (Color64*)((byte*)row + data.Stride);
                            }

                            // ReSharper disable once PossibleNullReferenceException - row is not null
                            row[x] = new Color64((ushort)i, (ushort)i, (ushort)i);
                        }
                    }
                    finally
                    {
                        bmp64.UnlockBits(data);
                    }

                    // Initializing the lookup table from the result of native GetPixel if that is supported on current operating system.
                    // It will be quite slow but it is executed only once and since it is OS dependent there is no other reliable way.
                    bool isLinear = true;
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
                        isLinear = isLinear && new Color64((ushort)i, (ushort)i, (ushort)i, (ushort)i).ToColor32() == new Color32(translatedColor);
                    }

                    if (isLinear)
                        lookupTable16To8Bpp = null;
                    lookupTable16To8BppInitialized = true;
                }
                catch (Exception e) when (!(e is StackOverflowException))
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
