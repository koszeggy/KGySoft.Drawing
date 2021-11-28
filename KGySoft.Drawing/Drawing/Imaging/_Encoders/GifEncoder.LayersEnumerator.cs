#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: GifEncoder.Encode.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2021 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;

using KGySoft.CoreLibraries;

#endregion

namespace KGySoft.Drawing.Imaging
{
    public partial class GifEncoder
    {
        private sealed class LayersEnumerator : IDisposable
        {
            #region Fields

            private readonly IAsyncContext asyncContext;
            private readonly IReadableBitmapData imageData;
            private readonly Color32 backColor;
            private readonly byte alphaThreshold;
            private readonly bool fullScan;
            private readonly Size size;
            private readonly HashSet<Color32> currentColors;
            private readonly HashSet<Color32> additionalColors;
            private readonly IReadableBitmapDataRow[] sourceRows;
            private readonly IReadableBitmapDataRow[] maskRows;
            private readonly IReadWriteBitmapData mask;

            private Point currentOrigin;
            private Rectangle currentRegion;
            private Size partialExpansion;

            #endregion

            #region Properties

            internal IReadableBitmapData? Layer { get; private set; }
            internal Point Location { get; private set; }

            #endregion

            #region Constructors

            internal LayersEnumerator(IAsyncContext asyncContext, IReadableBitmapData imageData, Color32 backColor, byte alphaThreshold, bool fullScan)
            {
                this.asyncContext = asyncContext;
                this.imageData = imageData;
                this.backColor = backColor;
                this.alphaThreshold = alphaThreshold;
                this.fullScan = fullScan;
                size = imageData.GetSize();

                currentColors = new HashSet<Color32>(256);
                additionalColors = new HashSet<Color32>();
                sourceRows = new IReadableBitmapDataRow[16];
                maskRows = new IReadableBitmapDataRow[16];
                mask = BitmapDataFactory.CreateBitmapData(size, PixelFormat.Format1bppIndexed);
            }

            #endregion

            #region Methods

            #region Internal Methods

            internal bool MoveNext()
            {
                Layer?.Dispose();
                // TODO: mask even in non-transparent mode if region goes beyond next origin
                // TODO: use array for colors if faster
                // TODO: parallelize
                // TODO: progress
                // TODO: reduce complexity if possible
                // TODO: refactoring to not use additional colors?

                if (currentOrigin.Y >= size.Height)
                {
                    Layer = null;
                    return false;
                }

                // 1.) Initializing an up to 16x16 block and expanding it as long as can
                // Keeping it rectangular helps reducing the generated size, even if full scanning is allowed
                InitializeCurrentRegion();
                ExpandRight();
                ExpandDown();
                ExpandLeft();

                // 2.) Generating the layer
                Palette palette = new Palette(currentColors.ToArray(), backColor, alphaThreshold);
                if (!palette.HasAlpha)
                {
                    // TODO: mask if needed (region goes beyond next origin)
                    // No transparent color: just adding the current region
                    Layer = imageData.DoClone(asyncContext, currentRegion, PixelFormat.Format8bppIndexed, new Palette(currentColors.ToArray(), backColor, alphaThreshold));
                    Location = currentRegion.Location;
                }
                else
                {
                    // There is transparency: masking the complete area and shrink region if possible
                    Rectangle layerRegion = fullScan
                        ? new Rectangle(0, currentRegion.Top, size.Width, size.Height - currentRegion.Top)
                        : currentRegion;

                    IReadWriteBitmapData layer = BitmapDataFactory.CreateBitmapData(layerRegion.Size, PixelFormat.Format8bppIndexed, palette);
                    if (palette.TransparentIndex != 0)
                        layer.DoClear(asyncContext, default);

                    // filling up colors in the whole remaining image
                    // TODO: parallel
                    for (int y = 0; y < layerRegion.Height; y++)
                    {
                        IReadableBitmapDataRow rowSource = imageData[layerRegion.Top + y];
                        IReadWriteBitmapDataRow rowMask = mask[layerRegion.Top + y];
                        IReadWriteBitmapDataRow rowLayer = layer[y];

                        for (int x = 0; x < layerRegion.Width; x++)
                        {
                            // already masked out
                            if (rowMask.GetColorIndex(x + layerRegion.Left) != 0)
                                continue;
                            Color32 color = GetColor(rowSource[x + layerRegion.Left]);

                            // cannot include yet
                            if (!currentColors.Contains(color))
                                continue;

                            // can include, masking out
                            rowMask.SetColorIndex(x + layerRegion.Left, 1);
                            if (color.A != 0)
                                rowLayer[x] = color;
                        }
                    }

                    layerRegion = GetContentArea(layer, true);
                    if (layerRegion.IsEmpty)
                    {
                        layer.Dispose();
                        Layer = null;
                    }
                    else
                    {
                        IReadWriteBitmapData clipped = layer.Clip(layerRegion, true);
                        if (fullScan)
                            layerRegion.Y += currentRegion.Top;
                        else
                            layerRegion.Location += new Size(currentRegion.Location);
                        Layer = clipped;
                        Location = layerRegion.Location;
                    }
                }

                if (currentOrigin.X != size.Width)
                    return true;

                // 3.) Adjusting origin for the next session
                currentOrigin.X = 0;
                while (currentOrigin.Y < size.Height)
                {
                    // trying to skip complete rows
                    IReadableBitmapDataRow rowSource = imageData[currentOrigin.Y];
                    IReadWriteBitmapDataRow rowMask = mask[currentOrigin.Y];
                    int x;
                    for (x = 0; x < size.Width; x++)
                    {
                        if (rowMask.GetColorIndex(x) == 0 && GetColor(rowSource[x]).A != 0)
                            break;
                    }

                    if (x != size.Width)
                        break;

                    // a complete row can be skipped
                    currentOrigin.Y += 1;
                }

                return true;
            }

            #endregion

            #region Private Methods

            private Color32 GetColor(Color32 color) =>
                color.A == Byte.MaxValue ? color
                : color.A >= alphaThreshold ? color.BlendWithBackground(backColor)
                : default;

            private void InitializeCurrentRegion()
            {
                currentColors.Clear();
                partialExpansion = Size.Empty;
                currentRegion = new Rectangle(currentOrigin, new Size(Math.Min(16, size.Width - currentOrigin.X), Math.Min(16, size.Height - currentOrigin.Y)));
                for (int y = 0; y < currentRegion.Height; y++)
                {
                    IReadableBitmapDataRow row = sourceRows[y] = imageData[currentRegion.Top + y];
                    IReadableBitmapDataRow maskRow = maskRows[y] = mask[currentRegion.Top + y];
                    for (int x = currentRegion.Left; x < currentRegion.Right; x++)
                        currentColors.Add(maskRow.GetColorIndex(x) == 0 ? GetColor(row[x]) : default);
                }
            }

            private void ExpandRight()
            {
                while (currentRegion.Right < size.Width)
                {
                    additionalColors.Clear();
                    int additionalLimit = 256 - currentColors.Count;

                    int y;
                    for (y = 0; y < currentRegion.Height; y++)
                    {
                        Color32 color = maskRows[y].GetColorIndex(currentRegion.Right) != 0
                            ? default
                            : GetColor(sourceRows[y][currentRegion.Right]);
                        if (currentColors.Contains(color))
                            continue;
                        if (additionalColors.Count < additionalLimit)
                            additionalColors.Add(color);
                        else
                            break;
                    }

                    // could not complete the new column
                    if (y != currentRegion.Height)
                    {
                        // not even the first pixel
                        if (y == 0 || additionalLimit == 0 && !currentColors.Contains(default))
                            break;

                        partialExpansion.Width = 1;

                        if (additionalColors.Count == additionalLimit && !currentColors.Contains(default))
                        {
                            currentColors.Add(default);
                            currentColors.AddRange(additionalColors.Take(additionalLimit - 1));
                        }
                        else
                            currentColors.AddRange(additionalColors);

                        currentRegion.Width += 1;
                        break;
                    }

                    // the region can be expanded
                    currentColors.AddRange(additionalColors);
                    currentRegion.Width += 1;
                }

                currentOrigin.X += currentRegion.Width - partialExpansion.Width;
            }

            private void ExpandDown()
            {
                if (partialExpansion.IsEmpty)
                {
                    while (currentRegion.Bottom < size.Height)
                    {
                        additionalColors.Clear();
                        int additionalLimit = 256 - currentColors.Count;
                        IReadableBitmapDataRow row = imageData[currentRegion.Bottom];
                        IReadableBitmapDataRow maskRow = mask[currentRegion.Bottom];
                        int x;
                        for (x = 0; x < currentRegion.Width; x++)
                        {
                            Color32 color = maskRow.GetColorIndex(x + currentRegion.Left) != 0
                                ? default
                                : GetColor(row[x + currentRegion.Left]);

                            if (currentColors.Contains(color))
                                continue;
                            if (additionalColors.Count < additionalLimit)
                                additionalColors.Add(color);
                            else
                                break;
                        }

                        // could not complete the new row
                        if (x != currentRegion.Width)
                        {
                            // not even the first pixel or already expanded
                            if (x == 0 || additionalLimit == 0 && !currentColors.Contains(default))
                                break;

                            partialExpansion.Height = 1;

                            if (additionalColors.Count == additionalLimit && !currentColors.Contains(default))
                            {
                                currentColors.Add(default);
                                currentColors.AddRange(additionalColors.Take(additionalLimit - 1));
                            }
                            else
                                currentColors.AddRange(additionalColors);

                            currentRegion.Height += 1;
                            break;
                        }

                        // the region can be expanded
                        currentColors.AddRange(additionalColors);
                        currentRegion.Height += 1;
                    }
                }

                if (currentOrigin.X == size.Width)
                    currentOrigin.Y += (currentRegion.Left == 0 ? currentRegion.Height : Math.Min(16, currentRegion.Height)) - partialExpansion.Height;
            }

            private void ExpandLeft()
            {
                if (currentRegion.Height <= 16 || !partialExpansion.IsEmpty || (currentColors.Count >= 256 && !currentColors.Contains(default)))
                    return;
                while (currentRegion.Left > 0)
                {
                    additionalColors.Clear();
                    int additionalLimit = 256 - currentColors.Count;
                    if (!currentColors.Contains(default))
                        additionalColors.Add(default);

                    // the first 16 row can be skipped as we passed beyond that region
                    int y;
                    for (y = currentRegion.Top + 16; y < currentRegion.Bottom; y++)
                    {
                        Color32 color = mask[y].GetColorIndex(currentRegion.Left - 1) != 0
                            ? default
                            : GetColor(imageData[y][currentRegion.Left - 1]);
                        if (currentColors.Contains(color))
                            continue;
                        if (additionalColors.Count < additionalLimit)
                            additionalColors.Add(color);
                        else
                            break;
                    }

                    // could not complete the new column
                    if (y != currentRegion.Bottom)
                    {
                        // not even the first pixel
                        if (y == currentRegion.Top + 16 || additionalLimit == 0 && !currentColors.Contains(default))
                            break;

                        if (additionalColors.Count == additionalLimit && !currentColors.Contains(default))
                        {
                            currentColors.Add(default);
                            currentColors.AddRange(additionalColors.Take(additionalLimit - 1));
                        }
                        else
                            currentColors.AddRange(additionalColors);

                        currentRegion.Width += 1;
                        currentRegion.X -= 1;
                        break;
                    }

                    // the region can be expanded
                    currentColors.AddRange(additionalColors);
                    currentRegion.Width += 1;
                    currentRegion.X -= 1;
                }
            }

            #endregion

            #region Explicitly Implemented Interface Methods

            void IDisposable.Dispose()
            {
                mask.Dispose();
                Layer?.Dispose();
            }

            #endregion

            #endregion
        }
    }
}