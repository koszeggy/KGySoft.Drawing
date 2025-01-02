#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: TestBase.cs
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
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;

using KGySoft.CoreLibraries;
using KGySoft.Drawing.Imaging;

using SkiaSharp;

#endregion

namespace KGySoft.Drawing.SkiaSharp.UnitTests
{
    public abstract class TestBase
    {
        #region Properties

        protected static bool SaveToFile => false;

        #endregion

        #region Methods

        protected static void GenerateAlphaGradient(IReadWriteBitmapData bitmapData)
        {
            var firstRow = new Color32[bitmapData.Width];
            float ratio = 255f / (bitmapData.Width / 6f);
            float limit = bitmapData.Width / 6f;

            for (int x = 0; x < bitmapData.Width; x++)
            {
                // red -> yellow
                if (x < limit)
                    firstRow[x] = new Color32(255, (x * ratio).ClipToByte(), 0);
                // yellow -> green
                else if (x < limit * 2)
                    firstRow[x] = new Color32((255 - (x - limit) * ratio).ClipToByte(), 255, 0);
                // green -> cyan
                else if (x < limit * 3)
                    firstRow[x] = new Color32(0, 255, ((x - limit * 2) * ratio).ClipToByte());
                // cyan -> blue
                else if (x < limit * 4)
                    firstRow[x] = new Color32(0, (255 - (x - limit * 3) * ratio).ClipToByte(), 255);
                // blue -> magenta
                else if (x < limit * 5)
                    firstRow[x] = new Color32(((x - limit * 4) * ratio).ClipToByte(), 0, 255);
                // magenta -> red
                else
                    firstRow[x] = new Color32(255, 0, (255 - (x - limit * 5) * ratio).ClipToByte());
            }

            for (int x = 0; x < bitmapData.Width; x++)
                bitmapData.SetPixel(x, 0, firstRow[x]);

            if (bitmapData.Height < 2)
                return;

            var row = bitmapData.GetMovableRow(1);
            ratio = 255f / bitmapData.Height;
            do
            {
                byte a = (255 - row.Index * ratio).ClipToByte();
                for (int x = 0; x < bitmapData.Width; x++)
                    row[x] = Color32.FromArgb(a, firstRow[x]);

            } while (row.MoveNextRow());
        }

        protected static void GenerateAlphaGradient(SKBitmap bitmap)
        {
            var size = new Size(bitmap.Width, bitmap.Height);
            var firstRow = new SKColor[size.Width];
            float ratio = 255f / (size.Width / 6f);
            float limit = size.Width / 6f;

            for (int x = 0; x < size.Width; x++)
            {
                // red -> yellow
                if (x < limit)
                    firstRow[x] = new SKColor(255, (x * ratio).ClipToByte(), 0);
                // yellow -> green
                else if (x < limit * 2)
                    firstRow[x] = new SKColor((255 - (x - limit) * ratio).ClipToByte(), 255, 0);
                // green -> cyan
                else if (x < limit * 3)
                    firstRow[x] = new SKColor(0, 255, ((x - limit * 2) * ratio).ClipToByte());
                // cyan -> blue
                else if (x < limit * 4)
                    firstRow[x] = new SKColor(0, (255 - (x - limit * 3) * ratio).ClipToByte(), 255);
                // blue -> magenta
                else if (x < limit * 5)
                    firstRow[x] = new SKColor(((x - limit * 4) * ratio).ClipToByte(), 0, 255);
                // magenta -> red
                else
                    firstRow[x] = new SKColor(255, 0, (255 - (x - limit * 5) * ratio).ClipToByte());
            }

            for (int x = 0; x < size.Width; x++)
                bitmap.SetPixel(x, 0, firstRow[x]);

            if (size.Height < 2)
                return;

            for (int y = 1; y < size.Height; y++)
            {
                ratio = 255f / size.Height;
                byte a = (255 - y * ratio).ClipToByte();
                for (int x = 0; x < size.Width; x++)
                {
                    var c = firstRow[x];
                    bitmap.SetPixel(x, y, new SKColor(c.Red, c.Green, c.Blue, a));
                }
            }
        }

        protected static void LoadInto(SKBitmap target, string fileName)
        {
            using SKBitmap source = SKBitmap.Decode(fileName);

            using var canvas = new SKCanvas(target);
            using var paint = new SKPaint { BlendMode = SKBlendMode.Src };
            canvas.DrawBitmap(source, source.Info.Rect, target.Info.Rect, paint);
        }

        protected static void LoadInto(IReadWriteBitmapData target, string fileName)
        {
            using SKBitmap source = SKBitmap.Decode(fileName);
            using var sourceBitmapData = source.GetReadableBitmapData();
            target.Clear(default);
            sourceBitmapData.DrawInto(target, new Rectangle(Point.Empty, sourceBitmapData.Size), new Rectangle(Point.Empty, target.Size));
        }

        protected static void SaveBitmap(string imageName, SKBitmap bitmap, [CallerMemberName]string testName = null!)
        {
            if (!SaveToFile)
                return;

            string dir = Path.Combine(Files.GetExecutingPath(), "TestResults");
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            string fileName = Path.Combine(dir, $"{testName}{(imageName == null ? null : $"_{imageName}")}.{DateTime.Now:yyyyMMddHHmmssffff}.png");
            using var stream = File.Create(fileName);

            //using SKData data = bitmap.Encode(SKEncodedImageFormat.Png, 100);
            //data.SaveTo(stream);


            //if ((bitmap.ColorType, bitmap.AlphaType) is not ((SKColorType.Rgba1010102, SKAlphaType.Opaque) or (SKColorType.RgbaF16Clamped, SKAlphaType.Opaque) or (SKColorType.RgbaF16, SKAlphaType.Opaque) or (SKColorType.RgbaF32, SKAlphaType.Opaque))
            //    && bitmap.Encode(stream, SKEncodedImageFormat.Png, 100))
            //    return;

            // failed to save: converting pixel format
            using var surface = SKSurface.Create(new SKImageInfo(bitmap.Width, bitmap.Height, SKColorType.Bgra8888, bitmap.AlphaType, SKColorSpace.CreateSrgb()));
            surface.Canvas.DrawBitmap(bitmap, 0, 0);
            using var pixels = surface.PeekPixels();
            pixels.Encode(stream, SKEncodedImageFormat.Png, 100);
        }

        #endregion
    }
}
