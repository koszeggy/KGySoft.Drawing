#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ColorHelper.cs
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
    internal static class ColorHelper
    {
        #region Fields

        private static readonly object syncRoot = new object();

        private static bool lookupTable8To16BppInitialized;
        private static (ushort Alpha, ushort Shade)[] lookupTable8To16Bpp;
        
        private static bool lookupTable16To8BppInitialized;
        private static (byte Alpha, byte Shade)[] lookupTable16To8Bpp;

        private static byte[][] premultipliedTable;
        private static byte[][] premultipliedTableInv;

        #endregion

        #region Methods

        #region Internal Methods

        internal static Color64 Argb32ToArgb64(this Color32 c)
        {
            if (!lookupTable8To16BppInitialized)
                InitializeLookupTable8To16Bpp();
            return lookupTable8To16Bpp == null
                ? new Color64(c)
                : new Color64(lookupTable8To16Bpp[c.A].Alpha, lookupTable8To16Bpp[c.R].Shade, lookupTable8To16Bpp[c.G].Shade, lookupTable8To16Bpp[c.B].Shade);
        }

        internal static Color64 Argb32ToPArgb64(this Color32 c)
        {
            if (!lookupTable8To16BppInitialized)
                InitializeLookupTable8To16Bpp();
            return lookupTable8To16Bpp == null
                ? new Color64(c)
                : new Color64(lookupTable8To16Bpp[c.A].Alpha, lookupTable8To16Bpp[c.R].Shade, lookupTable8To16Bpp[c.G].Shade, lookupTable8To16Bpp[c.B].Shade);
        }

        internal static Color32 Argb64ToArgb32(this Color64 c)
        {
            if (!lookupTable16To8BppInitialized)
                InitializeLookupTable16To8Bpp();
            return lookupTable16To8Bpp == null
                ? c.ToColor32()
                : new Color32(lookupTable16To8Bpp[c.A].Alpha, lookupTable16To8Bpp[c.R].Shade, lookupTable16To8Bpp[c.G].Shade, lookupTable16To8Bpp[c.B].Shade);
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
                        bmp64.SetPixel(i, 0, Color.FromArgb(i, i, i, i));
                    BitmapData data = bmp64.LockBits(new Rectangle(0, 0, 256, 1), ImageLockMode.ReadOnly, PixelFormat.Format64bppArgb);
                    bool isLinear = true;
                    try
                    {
                        Color64* row = (Color64*)data.Scan0;
                        lookupTable8To16Bpp = new (ushort, ushort)[256];
                        for (int i = 0; i < 256; i++)
                        {
                            lookupTable8To16Bpp[i].Alpha = row[i].A;
                            lookupTable8To16Bpp[i].Shade = row[i].R;
                            isLinear = isLinear && row[i] == new Color64(new Color32((byte)i, (byte)i, (byte)i, (byte)i));
                        }
                    }
                    finally
                    {
                        bmp64.UnlockBits(data);
                        lookupTable8To16BppInitialized = true;
                        if (isLinear)
                            lookupTable8To16Bpp = null;
                    }
                }
                catch (Exception e) when (!(e is StackOverflowException))
                {
                    // catching even OutOfMemoryException because Gdip.StatusException() can throw it for unsupported formats
                    lookupTable8To16Bpp = null;
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
                    ushort max16 = lookupTable8To16Bpp[lookupTable8To16Bpp.Length - 1].Shade;
                    Size size = new Size(256, (int)Math.Ceiling(((double)max16 + 1) / 256));

                    // Initializing a grayscale 64 bpp image from deep color shades.
                    using var bmp64 = new Bitmap(size.Width, size.Height, PixelFormat.Format64bppArgb);
                    BitmapData data = bmp64.LockBits(new Rectangle(Point.Empty, size), ImageLockMode.WriteOnly, PixelFormat.Format64bppArgb);
                    try
                    {
                        Color64* row = (Color64*)data.Scan0;
                        for (int i = 0, x = 0; i <= max16; i++, x++)
                        {
                            if (x == size.Width)
                            {
                                x = 0;
                                row = (Color64*)((byte*)row + data.Stride);
                            }

                            row[x] = new Color64((ushort)i, (ushort)i, (ushort)i, (ushort)i);
                        }
                    }
                    finally
                    {
                        bmp64.UnlockBits(data);
                    }

                    // Initializing the lookup table from the result of native GetPixel if that is supported on current operating system.
                    // It will be quite slow but it is executed only once and since it is OS dependent there is no other reliable way.
                    bool isLinear = true;
                    try
                    {
                        lookupTable16To8Bpp = new (byte, byte)[max16 + 1];
                        for (int i = 0, x = 0, y = 0; i <= max16; i++, x++)
                        {
                            if (x == size.Width)
                            {
                                x = 0;
                                y++;
                            }

                            Color translatedColor = bmp64.GetPixel(x, y);
                            lookupTable16To8Bpp[i].Alpha = translatedColor.A;
                            lookupTable16To8Bpp[i].Shade = translatedColor.R;
                            isLinear = isLinear &&  new Color64((ushort)i, (ushort)i, (ushort)i, (ushort)i).ToColor32() == new Color32(translatedColor);
                        }
                    }
                    finally
                    {
                        lookupTable16To8BppInitialized = true;
                        if (isLinear)
                            lookupTable16To8Bpp = null;
                    }
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
