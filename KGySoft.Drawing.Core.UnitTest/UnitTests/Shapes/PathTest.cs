#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: PathTest.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2025 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

#nullable enable

#region Usings

#region Used Namespaces

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;

using KGySoft.Drawing.Imaging;
using KGySoft.Drawing.Shapes;
using KGySoft.Threading;

using NUnit.Framework;

#endregion

#region Used Aliases

using Brush = KGySoft.Drawing.Shapes.Brush;
using Path = KGySoft.Drawing.Shapes.Path;
using Pen = KGySoft.Drawing.Shapes.Pen;

#endregion

#endregion

namespace KGySoft.Drawing.UnitTests.Shapes
{
    [TestFixture]
    public class PathTest : TestBase
    {
        #region Properties

        private static object?[][] FillPathTestSource =>
        [
            // string name, KnownPixelFormat pixelFormat, WorkingColorSpace colorSpace, Color backColor /*Empty: AlphaGradient*/, Color fillColor, DrawingOptions options
            ["32bppArgb_Alternate_NQ_Srgb_NA_NB", KnownPixelFormat.Format32bppArgb, WorkingColorSpace.Srgb, Color.Cyan, Color.Blue, new DrawingOptions { FillMode = ShapeFillMode.Alternate, AlphaBlending = false, AntiAliasing = false } ],
            ["32bppArgb_NonZero_NQ_Srgb_NA_NB", KnownPixelFormat.Format32bppArgb, WorkingColorSpace.Srgb, Color.Cyan, Color.Blue, new DrawingOptions { FillMode = ShapeFillMode.NonZero, AlphaBlending = false, AntiAliasing = false } ],
            ["32bppArgb_Alternate_NQ_Srgb_AA_NB", KnownPixelFormat.Format32bppArgb, WorkingColorSpace.Srgb, Color.Cyan, Color.Blue, new DrawingOptions { FillMode = ShapeFillMode.Alternate, AlphaBlending = false, AntiAliasing = true } ],
            ["32bppArgb_Alternate_NQ_Srgb_AA_AB", KnownPixelFormat.Format32bppArgb, WorkingColorSpace.Srgb, Color.Magenta, Color.Lime, new DrawingOptions { FillMode = ShapeFillMode.Alternate, AlphaBlending = true, AntiAliasing = true } ],
            ["32bppArgb_Alternate_NQ_Linear_AA_AB", KnownPixelFormat.Format32bppArgb, WorkingColorSpace.Linear, Color.Magenta, Color.Lime, new DrawingOptions { FillMode = ShapeFillMode.Alternate, AlphaBlending = true, AntiAliasing = true } ],
            ["32bppArgb_Alternate_NQ_Srgb_NA_NB_Tr", KnownPixelFormat.Format32bppArgb, WorkingColorSpace.Srgb, Color.Empty, Color.Empty, new DrawingOptions { FillMode = ShapeFillMode.Alternate, AlphaBlending = false, AntiAliasing = false } ],
            ["32bppArgb_Alternate_NQ_Srgb_NA_AB_A128", KnownPixelFormat.Format32bppArgb, WorkingColorSpace.Srgb, Color.Empty, Color.FromArgb(128, Color.Blue), new DrawingOptions { FillMode = ShapeFillMode.Alternate, AlphaBlending = true, AntiAliasing = false } ],
            ["32bppArgb_Alternate_NQ_Linear_NA_AB_A128", KnownPixelFormat.Format32bppArgb, WorkingColorSpace.Linear, Color.Empty, Color.FromArgb(128, Color.Blue), new DrawingOptions { FillMode = ShapeFillMode.Alternate, AlphaBlending = true, AntiAliasing = false } ],
            ["32bppArgb_Alternate_NQ_Srgb_AA_NB_A128", KnownPixelFormat.Format32bppArgb, WorkingColorSpace.Srgb, Color.Empty, Color.FromArgb(128, Color.Blue), new DrawingOptions { FillMode = ShapeFillMode.Alternate, AlphaBlending = false, AntiAliasing = true } ],
            ["32bppArgb_Alternate_NQ_Srgb_AA_AB_A128", KnownPixelFormat.Format32bppArgb, WorkingColorSpace.Srgb, Color.Empty, Color.FromArgb(128, Color.Blue), new DrawingOptions { FillMode = ShapeFillMode.Alternate, AlphaBlending = true, AntiAliasing = true } ],
            ["32bppArgb_Alternate_NQ_Linear_AA_AB_A128", KnownPixelFormat.Format32bppArgb, WorkingColorSpace.Linear, Color.Empty, Color.FromArgb(128, Color.Blue), new DrawingOptions { FillMode = ShapeFillMode.Alternate, AlphaBlending = true, AntiAliasing = true } ],
            ["32bppArgb_Alternate_QSys256_Srgb_NA_NB_A128", KnownPixelFormat.Format32bppArgb, WorkingColorSpace.Srgb, Color.Empty, Color.FromArgb(128, Color.Blue), new DrawingOptions { FillMode = ShapeFillMode.Alternate, AlphaBlending = false, AntiAliasing = false, Quantizer = PredefinedColorsQuantizer.SystemDefault8BppPalette(Color.Silver) } ],
            ["32bppArgb_Alternate_QSys256_Srgb_NA_NB_Tr", KnownPixelFormat.Format32bppArgb, WorkingColorSpace.Srgb, Color.Empty, Color.Transparent, new DrawingOptions { FillMode = ShapeFillMode.Alternate, AlphaBlending = false, AntiAliasing = false, Quantizer = PredefinedColorsQuantizer.SystemDefault8BppPalette(Color.Silver) } ],
            ["32bppArgb_Alternate_QSys256_Srgb_NA_AB_A64", KnownPixelFormat.Format32bppArgb, WorkingColorSpace.Srgb, Color.Empty, Color.FromArgb(64, Color.Blue), new DrawingOptions { FillMode = ShapeFillMode.Alternate, AlphaBlending = true, AntiAliasing = false, Quantizer = PredefinedColorsQuantizer.SystemDefault8BppPalette(Color.Silver) } ],
            ["32bppArgb_Alternate_QSys256_Linear_NA_AB_A64", KnownPixelFormat.Format32bppArgb, WorkingColorSpace.Linear, Color.Empty, Color.FromArgb(64, Color.Blue), new DrawingOptions { FillMode = ShapeFillMode.Alternate, AlphaBlending = true, AntiAliasing = false, Quantizer = PredefinedColorsQuantizer.SystemDefault8BppPalette(Color.Silver).ConfigureColorSpace(WorkingColorSpace.Linear) } ],
            ["32bppArgb_Alternate_QSys256_Srgb_AA_NB", KnownPixelFormat.Format32bppArgb, WorkingColorSpace.Srgb, Color.Empty, Color.Blue, new DrawingOptions { FillMode = ShapeFillMode.Alternate, AlphaBlending = false, AntiAliasing = true, Quantizer = PredefinedColorsQuantizer.SystemDefault8BppPalette(Color.Silver) } ],
            ["32bppArgb_Alternate_QSys256_Srgb_AA_NB_A128", KnownPixelFormat.Format32bppArgb, WorkingColorSpace.Srgb, Color.Empty, Color.FromArgb(128, Color.Blue), new DrawingOptions { FillMode = ShapeFillMode.Alternate, AlphaBlending = false, AntiAliasing = true, Quantizer = PredefinedColorsQuantizer.SystemDefault8BppPalette(Color.Silver) } ],
            ["32bppArgb_Alternate_QSys256_Srgb_AA_AB", KnownPixelFormat.Format32bppArgb, WorkingColorSpace.Srgb, Color.Empty, Color.Blue, new DrawingOptions { FillMode = ShapeFillMode.Alternate, AlphaBlending = true, AntiAliasing = true, Quantizer = PredefinedColorsQuantizer.SystemDefault8BppPalette(Color.Silver) } ],
            ["32bppArgb_Alternate_QSys256_Srgb_AA_AB_A64", KnownPixelFormat.Format32bppArgb, WorkingColorSpace.Srgb, Color.Empty, Color.FromArgb(64, Color.Blue), new DrawingOptions { FillMode = ShapeFillMode.Alternate, AlphaBlending = true, AntiAliasing = true, Quantizer = PredefinedColorsQuantizer.SystemDefault8BppPalette(Color.Silver) } ],
            ["32bppArgb_Alternate_QSys256_Linear_AA_AB_A64", KnownPixelFormat.Format32bppArgb, WorkingColorSpace.Linear, Color.Empty, Color.FromArgb(64, Color.Blue), new DrawingOptions { FillMode = ShapeFillMode.Alternate, AlphaBlending = true, AntiAliasing = true, Quantizer = PredefinedColorsQuantizer.SystemDefault8BppPalette(Color.Silver).ConfigureColorSpace(WorkingColorSpace.Linear) } ],
            ["32bppArgb_Alternate_DB8_Srgb_NA_NB_A128", KnownPixelFormat.Format32bppArgb, WorkingColorSpace.Srgb, Color.Empty, Color.FromArgb(128, Color.Blue), new DrawingOptions { FillMode = ShapeFillMode.Alternate, AlphaBlending = false, AntiAliasing = false, Quantizer = PredefinedColorsQuantizer.SystemDefault8BppPalette(Color.Silver), Ditherer = OrderedDitherer.Bayer8x8 } ],
            ["32bppArgb_Alternate_DB8_Srgb_NA_AB_A64", KnownPixelFormat.Format32bppArgb, WorkingColorSpace.Srgb, Color.Empty, Color.FromArgb(64, Color.Blue), new DrawingOptions { FillMode = ShapeFillMode.Alternate, AlphaBlending = true, AntiAliasing = false, Quantizer = PredefinedColorsQuantizer.SystemDefault8BppPalette(Color.Silver), Ditherer = OrderedDitherer.Bayer8x8 } ],
            ["32bppArgb_Alternate_DB8_Linear_NA_AB_A64", KnownPixelFormat.Format32bppArgb, WorkingColorSpace.Linear, Color.Empty, Color.FromArgb(64, Color.Blue), new DrawingOptions { FillMode = ShapeFillMode.Alternate, AlphaBlending = true, AntiAliasing = false, Quantizer = PredefinedColorsQuantizer.SystemDefault8BppPalette(Color.Silver).ConfigureColorSpace(WorkingColorSpace.Linear), Ditherer = OrderedDitherer.Bayer8x8 } ],
            ["32bppArgb_Alternate_DB8_Srgb_AA_NB_A128", KnownPixelFormat.Format32bppArgb, WorkingColorSpace.Srgb, Color.Empty, Color.FromArgb(128, Color.Blue), new DrawingOptions { FillMode = ShapeFillMode.Alternate, AlphaBlending = false, AntiAliasing = true, Quantizer = PredefinedColorsQuantizer.SystemDefault8BppPalette(Color.Silver), Ditherer = OrderedDitherer.Bayer8x8 } ],
            ["32bppArgb_Alternate_DB8_Srgb_AA_AB", KnownPixelFormat.Format32bppArgb, WorkingColorSpace.Srgb, Color.Empty, Color.Blue, new DrawingOptions { FillMode = ShapeFillMode.Alternate, AlphaBlending = true, AntiAliasing = true, Quantizer = PredefinedColorsQuantizer.SystemDefault8BppPalette(Color.Silver), Ditherer = OrderedDitherer.Bayer8x8 } ],
            ["32bppArgb_Alternate_DB8_Srgb_AA_AB_A64", KnownPixelFormat.Format32bppArgb, WorkingColorSpace.Srgb, Color.Empty, Color.FromArgb(64, Color.Blue), new DrawingOptions { FillMode = ShapeFillMode.Alternate, AlphaBlending = true, AntiAliasing = true, Quantizer = PredefinedColorsQuantizer.SystemDefault8BppPalette(Color.Silver), Ditherer = OrderedDitherer.Bayer8x8 } ],
            ["32bppArgb_Alternate_DB8_Linear_AA_AB_A64", KnownPixelFormat.Format32bppArgb, WorkingColorSpace.Linear, Color.Empty, Color.FromArgb(64, Color.Blue), new DrawingOptions { FillMode = ShapeFillMode.Alternate, AlphaBlending = true, AntiAliasing = true, Quantizer = PredefinedColorsQuantizer.SystemDefault8BppPalette(Color.Silver).ConfigureColorSpace(WorkingColorSpace.Linear), Ditherer = OrderedDitherer.Bayer8x8 } ],
            ["32bppArgb_Alternate_QWu_Srgb_NA_NB_A128", KnownPixelFormat.Format32bppArgb, WorkingColorSpace.Srgb, Color.Empty, Color.FromArgb(128, Color.Blue), new DrawingOptions { FillMode = ShapeFillMode.Alternate, AlphaBlending = false, AntiAliasing = false, Quantizer = OptimizedPaletteQuantizer.Wu(256, Color.Silver) } ],
            ["32bppArgb_Alternate_QWu_Srgb_NA_AB_A64", KnownPixelFormat.Format32bppArgb, WorkingColorSpace.Srgb, Color.Empty, Color.FromArgb(64, Color.Blue), new DrawingOptions { FillMode = ShapeFillMode.Alternate, AlphaBlending = true, AntiAliasing = false, Quantizer = OptimizedPaletteQuantizer.Wu(256, Color.Silver) } ],
            ["32bppArgb_Alternate_QWu_Linear_NA_AB_A64", KnownPixelFormat.Format32bppArgb, WorkingColorSpace.Linear, Color.Empty, Color.FromArgb(64, Color.Blue), new DrawingOptions { FillMode = ShapeFillMode.Alternate, AlphaBlending = true, AntiAliasing = false, Quantizer = OptimizedPaletteQuantizer.Wu(256, Color.Silver).ConfigureColorSpace(WorkingColorSpace.Linear) } ],
            ["32bppArgb_Alternate_QWu_Srgb_AA_NB", KnownPixelFormat.Format32bppArgb, WorkingColorSpace.Srgb, Color.Empty, Color.Blue, new DrawingOptions { FillMode = ShapeFillMode.Alternate, AlphaBlending = false, AntiAliasing = true, Quantizer = OptimizedPaletteQuantizer.Wu(256, Color.Silver) } ],
            ["32bppArgb_Alternate_QWu_Srgb_AA_NB_A128", KnownPixelFormat.Format32bppArgb, WorkingColorSpace.Srgb, Color.Empty, Color.FromArgb(128, Color.Blue), new DrawingOptions { FillMode = ShapeFillMode.Alternate, AlphaBlending = false, AntiAliasing = true, Quantizer = OptimizedPaletteQuantizer.Wu(256, Color.Silver) } ],
            ["32bppArgb_Alternate_QWu_Srgb_AA_AB", KnownPixelFormat.Format32bppArgb, WorkingColorSpace.Srgb, Color.Empty, Color.Blue, new DrawingOptions { FillMode = ShapeFillMode.Alternate, AlphaBlending = true, AntiAliasing = true, Quantizer = OptimizedPaletteQuantizer.Wu(256, Color.Silver) } ],
            ["32bppArgb_Alternate_QWu_Srgb_AA_AB_A64", KnownPixelFormat.Format32bppArgb, WorkingColorSpace.Srgb, Color.Empty, Color.FromArgb(64, Color.Blue), new DrawingOptions { FillMode = ShapeFillMode.Alternate, AlphaBlending = true, AntiAliasing = true, Quantizer = OptimizedPaletteQuantizer.Wu(256, Color.Silver) } ],
            ["32bppArgb_Alternate_QWu_Linear_AA_AB", KnownPixelFormat.Format32bppArgb, WorkingColorSpace.Linear, Color.Empty, Color.Blue, new DrawingOptions { FillMode = ShapeFillMode.Alternate, AlphaBlending = true, AntiAliasing = true, Quantizer = OptimizedPaletteQuantizer.Wu(256, Color.Silver).ConfigureColorSpace(WorkingColorSpace.Linear) } ],
            ["32bppArgb_Alternate_QWu_Linear_AA_AB_A64", KnownPixelFormat.Format32bppArgb, WorkingColorSpace.Linear, Color.Empty, Color.FromArgb(64, Color.Blue), new DrawingOptions { FillMode = ShapeFillMode.Alternate, AlphaBlending = true, AntiAliasing = true, Quantizer = OptimizedPaletteQuantizer.Wu(256, Color.Silver).ConfigureColorSpace(WorkingColorSpace.Linear) } ],
            ["32bppArgb_Alternate_DFS_Srgb_NA_NB_A128", KnownPixelFormat.Format32bppArgb, WorkingColorSpace.Srgb, Color.Empty, Color.FromArgb(128, Color.Blue), new DrawingOptions { FillMode = ShapeFillMode.Alternate, AlphaBlending = false, AntiAliasing = false, Quantizer = PredefinedColorsQuantizer.SystemDefault8BppPalette(Color.Silver), Ditherer = ErrorDiffusionDitherer.FloydSteinberg.ConfigureProcessingDirection(true) } ],
            ["32bppArgb_Alternate_DFS_Srgb_NA_AB_A64", KnownPixelFormat.Format32bppArgb, WorkingColorSpace.Srgb, Color.Empty, Color.FromArgb(64, Color.Blue), new DrawingOptions { FillMode = ShapeFillMode.Alternate, AlphaBlending = true, AntiAliasing = false, Quantizer = PredefinedColorsQuantizer.SystemDefault8BppPalette(Color.Silver), Ditherer = ErrorDiffusionDitherer.FloydSteinberg.ConfigureProcessingDirection(true) } ],
            ["32bppArgb_Alternate_DFS_Linear_NA_AB_A64", KnownPixelFormat.Format32bppArgb, WorkingColorSpace.Linear, Color.Empty, Color.FromArgb(64, Color.Blue), new DrawingOptions { FillMode = ShapeFillMode.Alternate, AlphaBlending = true, AntiAliasing = false, Quantizer = PredefinedColorsQuantizer.SystemDefault8BppPalette(Color.Silver).ConfigureColorSpace(WorkingColorSpace.Linear), Ditherer = ErrorDiffusionDitherer.FloydSteinberg.ConfigureProcessingDirection(true) } ],
            ["32bppArgb_Alternate_DFS_Srgb_AA_NB", KnownPixelFormat.Format32bppArgb, WorkingColorSpace.Srgb, Color.Empty, Color.Blue, new DrawingOptions { FillMode = ShapeFillMode.Alternate, AlphaBlending = false, AntiAliasing = true, Quantizer = PredefinedColorsQuantizer.SystemDefault8BppPalette(Color.Silver), Ditherer = ErrorDiffusionDitherer.FloydSteinberg.ConfigureProcessingDirection(true) } ],
            ["32bppArgb_Alternate_DFS_Srgb_AA_NB_A128", KnownPixelFormat.Format32bppArgb, WorkingColorSpace.Srgb, Color.Empty, Color.FromArgb(128, Color.Blue), new DrawingOptions { FillMode = ShapeFillMode.Alternate, AlphaBlending = false, AntiAliasing = true, Quantizer = PredefinedColorsQuantizer.SystemDefault8BppPalette(Color.Silver), Ditherer = ErrorDiffusionDitherer.FloydSteinberg.ConfigureProcessingDirection(true) } ],
            ["32bppArgb_Alternate_DFS_Srgb_AA_AB", KnownPixelFormat.Format32bppArgb, WorkingColorSpace.Srgb, Color.Empty, Color.Blue, new DrawingOptions { FillMode = ShapeFillMode.Alternate, AlphaBlending = true, AntiAliasing = true, Quantizer = PredefinedColorsQuantizer.SystemDefault8BppPalette(Color.Silver), Ditherer = ErrorDiffusionDitherer.FloydSteinberg.ConfigureProcessingDirection(true) } ],
            ["32bppArgb_Alternate_DFS_Srgb_AA_AB_A64", KnownPixelFormat.Format32bppArgb, WorkingColorSpace.Srgb, Color.Empty, Color.FromArgb(64, Color.Blue), new DrawingOptions { FillMode = ShapeFillMode.Alternate, AlphaBlending = true, AntiAliasing = true, Quantizer = PredefinedColorsQuantizer.SystemDefault8BppPalette(Color.Silver), Ditherer = ErrorDiffusionDitherer.FloydSteinberg.ConfigureProcessingDirection(true) } ],
            ["32bppArgb_Alternate_DFS_Linear_AA_AB_A64", KnownPixelFormat.Format32bppArgb, WorkingColorSpace.Linear, Color.Empty, Color.FromArgb(64, Color.Blue), new DrawingOptions { FillMode = ShapeFillMode.Alternate, AlphaBlending = true, AntiAliasing = true, Quantizer = PredefinedColorsQuantizer.SystemDefault8BppPalette(Color.Silver).ConfigureColorSpace(WorkingColorSpace.Linear), Ditherer = ErrorDiffusionDitherer.FloydSteinberg.ConfigureProcessingDirection(true) } ],
            ["1bppIndexed_Alternate_NQ_Srgb_NA_NB", KnownPixelFormat.Format1bppIndexed, WorkingColorSpace.Srgb, Color.Cyan, Color.Blue, new DrawingOptions { FillMode = ShapeFillMode.Alternate, AlphaBlending = false, AntiAliasing = false } ],
            ["1bppIndexed_NonZero_NQ_Srgb_NA_NB", KnownPixelFormat.Format1bppIndexed, WorkingColorSpace.Srgb, Color.Cyan, Color.Blue, new DrawingOptions { FillMode = ShapeFillMode.NonZero, AlphaBlending = false, AntiAliasing = false } ],
            ["1bppIndexed_Alternate_DB8_Srgb_NA_NB", KnownPixelFormat.Format1bppIndexed, WorkingColorSpace.Srgb, Color.Cyan, Color.Blue, new DrawingOptions { FillMode = ShapeFillMode.Alternate, AlphaBlending = false, AntiAliasing = false, Ditherer = OrderedDitherer.Bayer8x8 } ],
            ["8bppIndexed_Alternate_NQ_Srgb_NA_AB", KnownPixelFormat.Format8bppIndexed, WorkingColorSpace.Srgb, Color.Empty, Color.Blue, new DrawingOptions { FillMode = ShapeFillMode.Alternate, AlphaBlending = true } ],
            ["8bppIndexed_Alternate_NQ_Srgb_AA_NB_A64", KnownPixelFormat.Format8bppIndexed, WorkingColorSpace.Srgb, Color.Empty, Color.FromArgb(64, Color.Blue), new DrawingOptions { FillMode = ShapeFillMode.Alternate, AlphaBlending = false, AntiAliasing = true } ],
            ["8bppIndexed_Alternate_NQ_Srgb_AA_AB", KnownPixelFormat.Format8bppIndexed, WorkingColorSpace.Srgb, Color.Empty, Color.Blue, new DrawingOptions { FillMode = ShapeFillMode.Alternate, AlphaBlending = true, AntiAliasing = true } ],
            ["8bppIndexed_Alternate_DB8_Srgb_AA_AB_A64", KnownPixelFormat.Format8bppIndexed, WorkingColorSpace.Srgb, Color.Empty, Color.FromArgb(64, Color.Blue), new DrawingOptions { FillMode = ShapeFillMode.Alternate, AlphaBlending = true, AntiAliasing = true, Ditherer = OrderedDitherer.Bayer8x8 } ],
            ["32bppPArgb_Alternate_NQ_Srgb_NA_NB", KnownPixelFormat.Format32bppPArgb, WorkingColorSpace.Srgb, Color.Cyan, Color.Blue, new DrawingOptions { FillMode = ShapeFillMode.Alternate, AlphaBlending = false, AntiAliasing = false } ],
            ["32bppPArgb_Alternate_NQ_Srgb_AA_NB", KnownPixelFormat.Format32bppPArgb, WorkingColorSpace.Srgb, Color.Cyan, Color.Blue, new DrawingOptions { FillMode = ShapeFillMode.Alternate, AlphaBlending = false, AntiAliasing = true } ],
            ["32bppPArgb_Alternate_NQ_Srgb_AA_AB", KnownPixelFormat.Format32bppPArgb, WorkingColorSpace.Srgb, Color.Magenta, Color.Lime, new DrawingOptions { FillMode = ShapeFillMode.Alternate, AlphaBlending = true, AntiAliasing = true } ],
            ["32bppPArgb_Alternate_NQ_Linear_AA_AB", KnownPixelFormat.Format32bppPArgb, WorkingColorSpace.Linear, Color.Magenta, Color.Lime, new DrawingOptions { FillMode = ShapeFillMode.Alternate, AlphaBlending = true, AntiAliasing = true } ],
            ["32bppPArgb_Alternate_NQ_Srgb_NA_NB_Tr", KnownPixelFormat.Format32bppPArgb, WorkingColorSpace.Srgb, Color.Empty, Color.Empty, new DrawingOptions { FillMode = ShapeFillMode.Alternate, AlphaBlending = false, AntiAliasing = false } ],
            ["32bppPArgb_Alternate_NQ_Srgb_NA_AB_A128", KnownPixelFormat.Format32bppPArgb, WorkingColorSpace.Srgb, Color.Empty, Color.FromArgb(128, Color.Blue), new DrawingOptions { FillMode = ShapeFillMode.Alternate, AlphaBlending = true, AntiAliasing = false } ],
            ["32bppPArgb_Alternate_NQ_Linear_NA_AB_A128", KnownPixelFormat.Format32bppPArgb, WorkingColorSpace.Linear, Color.Empty, Color.FromArgb(128, Color.Blue), new DrawingOptions { FillMode = ShapeFillMode.Alternate, AlphaBlending = true, AntiAliasing = false } ],
            ["32bppPArgb_Alternate_NQ_Srgb_AA_NB_A128", KnownPixelFormat.Format32bppPArgb, WorkingColorSpace.Srgb, Color.Empty, Color.FromArgb(128, Color.Blue), new DrawingOptions { FillMode = ShapeFillMode.Alternate, AlphaBlending = false, AntiAliasing = true } ],
            ["32bppPArgb_Alternate_NQ_Srgb_AA_AB_A128", KnownPixelFormat.Format32bppPArgb, WorkingColorSpace.Srgb, Color.Empty, Color.FromArgb(128, Color.Blue), new DrawingOptions { FillMode = ShapeFillMode.Alternate, AlphaBlending = true, AntiAliasing = true } ],
            ["32bppPArgb_Alternate_NQ_Linear_AA_AB_A128", KnownPixelFormat.Format32bppPArgb, WorkingColorSpace.Linear, Color.Empty, Color.FromArgb(128, Color.Blue), new DrawingOptions { FillMode = ShapeFillMode.Alternate, AlphaBlending = true, AntiAliasing = true } ],
            ["64bppArgb_Alternate_NQ_Srgb_NA_NB", KnownPixelFormat.Format64bppArgb, WorkingColorSpace.Srgb, Color.Cyan, Color.Blue, new DrawingOptions { FillMode = ShapeFillMode.Alternate, AlphaBlending = false, AntiAliasing = false } ],
            ["64bppArgb_Alternate_NQ_Srgb_AA_NB", KnownPixelFormat.Format64bppArgb, WorkingColorSpace.Srgb, Color.Cyan, Color.Blue, new DrawingOptions { FillMode = ShapeFillMode.Alternate, AlphaBlending = false, AntiAliasing = true } ],
            ["64bppArgb_Alternate_NQ_Srgb_AA_AB", KnownPixelFormat.Format64bppArgb, WorkingColorSpace.Srgb, Color.Magenta, Color.Lime, new DrawingOptions { FillMode = ShapeFillMode.Alternate, AlphaBlending = true, AntiAliasing = true } ],
            ["64bppArgb_Alternate_NQ_Linear_AA_AB", KnownPixelFormat.Format64bppArgb, WorkingColorSpace.Linear, Color.Magenta, Color.Lime, new DrawingOptions { FillMode = ShapeFillMode.Alternate, AlphaBlending = true, AntiAliasing = true } ],
            ["64bppArgb_Alternate_NQ_Srgb_NA_NB_Tr", KnownPixelFormat.Format64bppArgb, WorkingColorSpace.Srgb, Color.Empty, Color.Empty, new DrawingOptions { FillMode = ShapeFillMode.Alternate, AlphaBlending = false, AntiAliasing = false } ],
            ["64bppArgb_Alternate_NQ_Srgb_NA_AB_A128", KnownPixelFormat.Format64bppArgb, WorkingColorSpace.Srgb, Color.Empty, Color.FromArgb(128, Color.Blue), new DrawingOptions { FillMode = ShapeFillMode.Alternate, AlphaBlending = true, AntiAliasing = false } ],
            ["64bppArgb_Alternate_NQ_Linear_NA_AB_A128", KnownPixelFormat.Format64bppArgb, WorkingColorSpace.Linear, Color.Empty, Color.FromArgb(128, Color.Blue), new DrawingOptions { FillMode = ShapeFillMode.Alternate, AlphaBlending = true, AntiAliasing = false } ],
            ["64bppArgb_Alternate_NQ_Srgb_AA_NB_A128", KnownPixelFormat.Format64bppArgb, WorkingColorSpace.Srgb, Color.Empty, Color.FromArgb(128, Color.Blue), new DrawingOptions { FillMode = ShapeFillMode.Alternate, AlphaBlending = false, AntiAliasing = true } ],
            ["64bppArgb_Alternate_NQ_Srgb_AA_AB_A128", KnownPixelFormat.Format64bppArgb, WorkingColorSpace.Srgb, Color.Empty, Color.FromArgb(128, Color.Blue), new DrawingOptions { FillMode = ShapeFillMode.Alternate, AlphaBlending = true, AntiAliasing = true } ],
            ["64bppArgb_Alternate_NQ_Linear_AA_AB_A128", KnownPixelFormat.Format64bppArgb, WorkingColorSpace.Linear, Color.Empty, Color.FromArgb(128, Color.Blue), new DrawingOptions { FillMode = ShapeFillMode.Alternate, AlphaBlending = true, AntiAliasing = true } ],
            ["64bppPArgb_Alternate_NQ_Srgb_NA_NB", KnownPixelFormat.Format64bppPArgb, WorkingColorSpace.Srgb, Color.Cyan, Color.Blue, new DrawingOptions { FillMode = ShapeFillMode.Alternate, AlphaBlending = false, AntiAliasing = false } ],
            ["64bppPArgb_Alternate_NQ_Srgb_AA_NB", KnownPixelFormat.Format64bppPArgb, WorkingColorSpace.Srgb, Color.Cyan, Color.Blue, new DrawingOptions { FillMode = ShapeFillMode.Alternate, AlphaBlending = false, AntiAliasing = true } ],
            ["64bppPArgb_Alternate_NQ_Srgb_AA_AB", KnownPixelFormat.Format64bppPArgb, WorkingColorSpace.Srgb, Color.Magenta, Color.Lime, new DrawingOptions { FillMode = ShapeFillMode.Alternate, AlphaBlending = true, AntiAliasing = true } ],
            ["64bppPArgb_Alternate_NQ_Linear_AA_AB", KnownPixelFormat.Format64bppPArgb, WorkingColorSpace.Linear, Color.Magenta, Color.Lime, new DrawingOptions { FillMode = ShapeFillMode.Alternate, AlphaBlending = true, AntiAliasing = true } ],
            ["64bppPArgb_Alternate_NQ_Srgb_NA_NB_Tr", KnownPixelFormat.Format64bppPArgb, WorkingColorSpace.Srgb, Color.Empty, Color.Empty, new DrawingOptions { FillMode = ShapeFillMode.Alternate, AlphaBlending = false, AntiAliasing = false } ],
            ["64bppPArgb_Alternate_NQ_Srgb_NA_AB_A128", KnownPixelFormat.Format64bppPArgb, WorkingColorSpace.Srgb, Color.Empty, Color.FromArgb(128, Color.Blue), new DrawingOptions { FillMode = ShapeFillMode.Alternate, AlphaBlending = true, AntiAliasing = false } ],
            ["64bppPArgb_Alternate_NQ_Linear_NA_AB_A128", KnownPixelFormat.Format64bppPArgb, WorkingColorSpace.Linear, Color.Empty, Color.FromArgb(128, Color.Blue), new DrawingOptions { FillMode = ShapeFillMode.Alternate, AlphaBlending = true, AntiAliasing = false } ],
            ["64bppPArgb_Alternate_NQ_Srgb_AA_NB_A128", KnownPixelFormat.Format64bppPArgb, WorkingColorSpace.Srgb, Color.Empty, Color.FromArgb(128, Color.Blue), new DrawingOptions { FillMode = ShapeFillMode.Alternate, AlphaBlending = false, AntiAliasing = true } ],
            ["64bppPArgb_Alternate_NQ_Srgb_AA_AB_A128", KnownPixelFormat.Format64bppPArgb, WorkingColorSpace.Srgb, Color.Empty, Color.FromArgb(128, Color.Blue), new DrawingOptions { FillMode = ShapeFillMode.Alternate, AlphaBlending = true, AntiAliasing = true } ],
            ["64bppPArgb_Alternate_NQ_Linear_AA_AB_A128", KnownPixelFormat.Format64bppPArgb, WorkingColorSpace.Linear, Color.Empty, Color.FromArgb(128, Color.Blue), new DrawingOptions { FillMode = ShapeFillMode.Alternate, AlphaBlending = true, AntiAliasing = true } ],
            ["128bppRgba_Alternate_NQ_Srgb_NA_NB", KnownPixelFormat.Format128bppRgba, WorkingColorSpace.Srgb, Color.Cyan, Color.Blue, new DrawingOptions { FillMode = ShapeFillMode.Alternate, AlphaBlending = false, AntiAliasing = false } ],
            ["128bppRgba_Alternate_NQ_Srgb_AA_NB", KnownPixelFormat.Format128bppRgba, WorkingColorSpace.Srgb, Color.Cyan, Color.Blue, new DrawingOptions { FillMode = ShapeFillMode.Alternate, AlphaBlending = false, AntiAliasing = true } ],
            ["128bppRgba_Alternate_NQ_Srgb_AA_AB", KnownPixelFormat.Format128bppRgba, WorkingColorSpace.Srgb, Color.Magenta, Color.Lime, new DrawingOptions { FillMode = ShapeFillMode.Alternate, AlphaBlending = true, AntiAliasing = true } ],
            ["128bppRgba_Alternate_NQ_Linear_AA_AB", KnownPixelFormat.Format128bppRgba, WorkingColorSpace.Linear, Color.Magenta, Color.Lime, new DrawingOptions { FillMode = ShapeFillMode.Alternate, AlphaBlending = true, AntiAliasing = true } ],
            ["128bppRgba_Alternate_NQ_Srgb_NA_NB_Tr", KnownPixelFormat.Format128bppRgba, WorkingColorSpace.Srgb, Color.Empty, Color.Empty, new DrawingOptions { FillMode = ShapeFillMode.Alternate, AlphaBlending = false, AntiAliasing = false } ],
            ["128bppRgba_Alternate_NQ_Srgb_NA_AB_A128", KnownPixelFormat.Format128bppRgba, WorkingColorSpace.Srgb, Color.Empty, Color.FromArgb(128, Color.Blue), new DrawingOptions { FillMode = ShapeFillMode.Alternate, AlphaBlending = true, AntiAliasing = false } ],
            ["128bppRgba_Alternate_NQ_Linear_NA_AB_A128", KnownPixelFormat.Format128bppRgba, WorkingColorSpace.Linear, Color.Empty, Color.FromArgb(128, Color.Blue), new DrawingOptions { FillMode = ShapeFillMode.Alternate, AlphaBlending = true, AntiAliasing = false } ],
            ["128bppRgba_Alternate_NQ_Srgb_AA_NB_A128", KnownPixelFormat.Format128bppRgba, WorkingColorSpace.Srgb, Color.Empty, Color.FromArgb(128, Color.Blue), new DrawingOptions { FillMode = ShapeFillMode.Alternate, AlphaBlending = false, AntiAliasing = true } ],
            ["128bppRgba_Alternate_NQ_Srgb_AA_AB_A128", KnownPixelFormat.Format128bppRgba, WorkingColorSpace.Srgb, Color.Empty, Color.FromArgb(128, Color.Blue), new DrawingOptions { FillMode = ShapeFillMode.Alternate, AlphaBlending = true, AntiAliasing = true } ],
            ["128bppRgba_Alternate_NQ_Linear_AA_AB_A128", KnownPixelFormat.Format128bppRgba, WorkingColorSpace.Linear, Color.Empty, Color.FromArgb(128, Color.Blue), new DrawingOptions { FillMode = ShapeFillMode.Alternate, AlphaBlending = true, AntiAliasing = true } ],
            ["128bppPRgba_Alternate_NQ_Srgb_NA_NB", KnownPixelFormat.Format128bppPRgba, WorkingColorSpace.Srgb, Color.Cyan, Color.Blue, new DrawingOptions { FillMode = ShapeFillMode.Alternate, AlphaBlending = false, AntiAliasing = false } ],
            ["128bppPRgba_Alternate_NQ_Srgb_AA_NB", KnownPixelFormat.Format128bppPRgba, WorkingColorSpace.Srgb, Color.Cyan, Color.Blue, new DrawingOptions { FillMode = ShapeFillMode.Alternate, AlphaBlending = false, AntiAliasing = true } ],
            ["128bppPRgba_Alternate_NQ_Srgb_AA_AB", KnownPixelFormat.Format128bppPRgba, WorkingColorSpace.Srgb, Color.Magenta, Color.Lime, new DrawingOptions { FillMode = ShapeFillMode.Alternate, AlphaBlending = true, AntiAliasing = true } ],
            ["128bppPRgba_Alternate_NQ_Linear_AA_AB", KnownPixelFormat.Format128bppPRgba, WorkingColorSpace.Linear, Color.Magenta, Color.Lime, new DrawingOptions { FillMode = ShapeFillMode.Alternate, AlphaBlending = true, AntiAliasing = true } ],
            ["128bppPRgba_Alternate_NQ_Srgb_NA_NB_Tr", KnownPixelFormat.Format128bppPRgba, WorkingColorSpace.Srgb, Color.Empty, Color.Empty, new DrawingOptions { FillMode = ShapeFillMode.Alternate, AlphaBlending = false, AntiAliasing = false } ],
            ["128bppPRgba_Alternate_NQ_Srgb_NA_AB_A128", KnownPixelFormat.Format128bppPRgba, WorkingColorSpace.Srgb, Color.Empty, Color.FromArgb(128, Color.Blue), new DrawingOptions { FillMode = ShapeFillMode.Alternate, AlphaBlending = true, AntiAliasing = false } ],
            ["128bppPRgba_Alternate_NQ_Linear_NA_AB_A128", KnownPixelFormat.Format128bppPRgba, WorkingColorSpace.Linear, Color.Empty, Color.FromArgb(128, Color.Blue), new DrawingOptions { FillMode = ShapeFillMode.Alternate, AlphaBlending = true, AntiAliasing = false } ],
            ["128bppPRgba_Alternate_NQ_Srgb_AA_NB_A128", KnownPixelFormat.Format128bppPRgba, WorkingColorSpace.Srgb, Color.Empty, Color.FromArgb(128, Color.Blue), new DrawingOptions { FillMode = ShapeFillMode.Alternate, AlphaBlending = false, AntiAliasing = true } ],
            ["128bppPRgba_Alternate_NQ_Srgb_AA_AB_A128", KnownPixelFormat.Format128bppPRgba, WorkingColorSpace.Srgb, Color.Empty, Color.FromArgb(128, Color.Blue), new DrawingOptions { FillMode = ShapeFillMode.Alternate, AlphaBlending = true, AntiAliasing = true } ],
            ["128bppPRgba_Alternate_NQ_Linear_AA_AB_A128", KnownPixelFormat.Format128bppPRgba, WorkingColorSpace.Linear, Color.Empty, Color.FromArgb(128, Color.Blue), new DrawingOptions { FillMode = ShapeFillMode.Alternate, AlphaBlending = true, AntiAliasing = true } ],

            ["32bppArgb_Alternate_NQ_Srgb_NA_NB_Rotated", KnownPixelFormat.Format32bppArgb, WorkingColorSpace.Srgb, Color.Cyan, Color.Blue, new DrawingOptions { FillMode = ShapeFillMode.Alternate, AlphaBlending = false, AntiAliasing = false, Transformation = TransformationMatrix.CreateRotation(13, new(100, 100)) } ],
            ["32bppArgb_Alternate_NQ_Srgb_AA_AB_A128_Rotated", KnownPixelFormat.Format32bppArgb, WorkingColorSpace.Srgb, Color.Empty, Color.FromArgb(128, Color.Blue), new DrawingOptions { FillMode = ShapeFillMode.Alternate, AlphaBlending = true, AntiAliasing = true, Transformation = TransformationMatrix.CreateRotation(13, new(100, 100)) } ],
        ];

        private static object?[][] PixelOffsetTestSource =>
        [
            // string name, Path path
            ["Rectangle", new Path().AddRectangle(new RectangleF(2, 2, 10, 5))],
            ["AlmostRectangle", new Path().AddPolygon(new(2, 1), new(23, 2), new(24, 8), new(1, 9))],
            ["Star", new Path().AddPolygon(new(50, 0), new(90, 100), new(0, 40), new(100, 40), new(10, 100))],
            ["Ellipse", new Path().AddEllipse(new RectangleF(1, 1, 98, 48))],
        ];

        private static object?[][] DrawOpenPathTestSource =>
        [
            // string name, Path path, float width
            ["Point01", new Path().AddPoint(new PointF(1, 1)), 1f],
            ["Point.5", new Path().AddPoint(new PointF(1, 1)), 0.5f],
            ["Point10", new Path().AddPoint(new PointF(1, 1)), 10f],
            ["LineEpsilon01", new Path().AddLine(new PointF(1, 1), new PointF(1 + 1f / 64f, 1)), 1f],
            ["LineEpsilon.5", new Path().AddLine(new PointF(1, 1), new PointF(1 + 1f / 64f, 1)), 0.5f],
            ["LineEpsilon10", new Path().AddLine(new PointF(1, 1), new PointF(1 + 1f / 64f, 1)), 10f],
            ["Line2px01", new Path().AddLine(new PointF(1, 1), new PointF(2, 1)), 1f],
            ["Line2px.5", new Path().AddLine(new PointF(1, 1), new PointF(2, 1)), 0.5f],
            ["Line2px10", new Path().AddLine(new PointF(1, 1), new PointF(2, 1)), 10f],
            ["LineLong01", new Path().AddLine(new PointF(1, 1), new PointF(13, 2)), 1f],
            ["LineLong.5", new Path().AddLine(new PointF(1, 1), new PointF(13, 2)), 0.5f],
            ["LineLong.Thin", new Path().AddLine(new PointF(1, 1), new PointF(13, 2)), 1f/48f],
            ["LineLong10", new Path().AddLine(new PointF(1, 1), new PointF(13, 2)), 10f],
            ["LineLongHorizontal10", new Path().AddLine(new PointF(1, 5), new PointF(100, 5)), 10f],
            ["Joints01", new Path().AddLines(new(0, 100), new(50, 20), new(100, 100)), 1f],
            ["Joints10", new Path().AddLines(new(0, 100), new(50, 20), new(100, 100)), 10f],
            ["TetragonOpen", new Path().AddLines(new PointF(1, 1), new PointF(40, 1), new PointF(100, 50), new PointF(0, 50)), 10f],
            ["SelfCrossingStarOpen_01", new Path().AddLines(new(51, 1), new(81, 91), new(3, 36), new(99, 36), new(22, 91)), 1f],
            ["SelfCrossingStarOpen_10", new Path().AddLines(new(60, 10), new(90, 100), new(12, 45), new(108, 45), new(31, 100)), 10f],
            ["Bezier0", new Path().AddBeziers(), 1f],
            ["Bezier1", new Path().AddBeziers(new PointF(1, 1)), 1f],
            ["BezierS", new Path().AddBezier(new PointF(0, 100), new PointF(50, 100), new PointF(50, 0), new PointF(100, 0)), 1f],
            ["BezierC", new Path().AddBezier(new PointF(0, 100), new PointF(0, 50), new PointF(50, 0), new PointF(100, 0)), 1f],
            ["BezierZ", new Path().AddBezier(new PointF(0, 100), new PointF(100, 50), new PointF(0, 50), new PointF(100, 0)), 1f],
            ["BezierL", new Path().AddBezier(new PointF(0, 100), new PointF(100, 50), new PointF(0, 0), new PointF(50, 100)), 1f],
            ["BezierN", new Path().AddBezier(new PointF(0, 100), new PointF(100, 0), new PointF(0, 0), new PointF(100, 100)), 1f],
            ["Arc0_180Deg01", new Path().AddArc(new RectangleF(1, 1, 98, 48), 0, 180), 1f],
            ["Arc0_180Deg10", new Path().AddArc(new RectangleF(1, 1, 98, 48), 0, 180), 10f],
            ["Arc90_90Deg01", new Path().AddArc(new RectangleF(1, 1, 98, 48), 90, 90), 1f],
            ["Arc90_180Deg01", new Path().AddArc(new RectangleF(1, 1, 98, 48), 90, 180), 1f],
            ["Arc270_90Deg01", new Path().AddArc(new RectangleF(1, 1, 98, 48), 270, 90), 1f],
            ["Arc270_180Deg01", new Path().AddArc(new RectangleF(1, 1, 98, 48), 270, 180), 1f],
            ["Arc0_45Deg01", new Path().AddArc(new RectangleF(1, 1, 98, 48), 0, 45), 1f],
            ["Arc0-45Deg01", new Path().AddArc(new RectangleF(1, 1, 98, 48), 0, -45), 1f],
        ];

        private static object?[][] DrawClosedPathTestSource =>
        [
            // string name, Path path, float width
            ["TetragonClose01", new Path().AddPolygon(new PointF(1, 1), new PointF(40, 1), new PointF(100, 50), new PointF(0, 50)), 1f],
            ["TetragonClose10", new Path().AddPolygon(new PointF(1, 1), new PointF(40, 1), new PointF(100, 50), new PointF(0, 50)), 10f],
            ["SelfCrossingStarClose01", new Path().AddPolygon(new(51, 1), new(81, 91), new(3, 36), new(99, 36), new(22, 91)), 1f],
            ["SelfCrossingStarClose10", new Path().AddPolygon(new(51, 1), new(81, 91), new(3, 36), new(99, 36), new(22, 91)), 10f],
            ["Rectangle00_01", new Path().AddRectangle(new RectangleF(0, 0, 0, 0)), 1f],
            ["Rectangle01_01", new Path().AddRectangle(new RectangleF(0, 0, 1, 1)), 1f],
            ["Rectangle10_01", new Path().AddRectangle(new RectangleF(0, 0, 10, 10)), 1f],
            ["Circle00_01", new Path().AddEllipse(new RectangleF(1, 1, 0, 0)), 1f],
            ["Circle01_01", new Path().AddEllipse(new RectangleF(1, 1, 1, 1)), 1f],
            ["Circle10_01", new Path().AddEllipse(new RectangleF(1, 1, 10, 10)), 1f],
            ["Ellipse01", new Path().AddEllipse(new RectangleF(2, 2, 95, 45)), 1f],
            ["Ellipse10", new Path().AddEllipse(new RectangleF(2, 2, 95, 45)), 10f],
            ["RotatedEllipse01", new Path().SetTransformation(TransformationMatrix.CreateRotationDegrees(45f, new PointF(50, 25))).AddEllipse(new RectangleF(2, 20, 95, 45)), 1f],
            ["RoundedRectangle5_01", new Path().AddRoundedRectangle(new RectangleF(2, 2, 95, 45), 5f), 1f],
            ["RoundedRectangle25_01", new Path().AddRoundedRectangle(new RectangleF(2, 2, 95, 45), 25f), 1f],
            ["RoundedRectangle50_01", new Path().AddRoundedRectangle(new RectangleF(2, 2, 95, 45), 50f), 1f],
            ["RoundedRectangleAsymmetric5_6_7_8_01", new Path().AddRoundedRectangle(new RectangleF(2, 2, 95, 45), 5f, 6f, 7f, 8f), 1f],
            ["RoundedRectangleAsymmetric5_16_27_38_01", new Path().AddRoundedRectangle(new RectangleF(2, 2, 95, 45), 5f, 16f, 27f, 38f), 1f],
            ["RoundedRectangleAsymmetric25_36_47_58_01", new Path().AddRoundedRectangle(new RectangleF(2, 2, 95, 45), 25f, 36f, 47f, 58f), 1f],
            ["Pie0_180Deg01", new Path().AddPie(new RectangleF(1, 1, 98, 48), 0, 180), 1f],
            ["Pie0_180Deg10", new Path().AddPie(new RectangleF(1, 1, 98, 48), 0, 180), 10f],
            ["Pie90_90Deg01", new Path().AddPie(new RectangleF(1, 1, 98, 48), 90, 90), 1f],
            ["Pie90_180Deg01", new Path().AddPie(new RectangleF(1, 1, 98, 48), 90, 180), 1f],
            ["Pie270_90Deg01", new Path().AddPie(new RectangleF(1, 1, 98, 48), 270, 90), 1f],
            ["Pie270_180Deg01", new Path().AddPie(new RectangleF(1, 1, 98, 48), 270, 180), 1f],
            ["Pie0_45Deg01", new Path().AddPie(new RectangleF(1, 1, 98, 48), 0, 45), 1f],
            ["Pie0-45Deg01", new Path().AddPie(new RectangleF(1, 1, 98, 48), 0, -45), 1f],
            ["Pie45_90Deg01", new Path().AddPie(new RectangleF(1, 1, 98, 48), 45, 90), 1f],
            ["Pie135_90Deg01", new Path().AddPie(new RectangleF(1, 1, 98, 48), 135, 90), 1f],
            ["Pie225_90Deg01", new Path().AddPie(new RectangleF(1, 1, 98, 48), 225, 90), 1f],
            ["Pie315_90Deg01", new Path().AddPie(new RectangleF(1, 1, 98, 48), 315, 90), 1f],
        ];

        private static object?[][] DrawThinLinesTestSource =>
        [
            // string name, Path path
            ["Point", new Path().AddPoint(new PointF(1, 1))],
            ["LineLeftRight", new Path().AddLine(new PointF(1, 1), new PointF(2, 1)).StartFigure().AddLine(new PointF(2, 5), new PointF(1, 5))],
            ["LineRightLeft.5", new Path().AddLine(new PointF(1.5f, 1.5f), new PointF(2.5f, 1.5f)).StartFigure().AddLine(new PointF(2.5f, 5.5f), new PointF(1.5f, 5.5f))],
            ["LineUpDown", new Path().AddLine(new PointF(1, 2), new PointF(1, 1)).StartFigure().AddLine(new PointF(5, 1), new PointF(5, 2))],
            ["LineRightDownLandscape", new Path().AddLine(new PointF(1, 1), new PointF(3, 2)).StartFigure().AddLine(new PointF(3, 6), new PointF(1, 5))],
            ["LineRightUpLandscape", new Path().AddLine(new PointF(1, 2), new PointF(3, 1)).StartFigure().AddLine(new PointF(3, 5), new PointF(1, 6))],
            ["LineRightDownPortrait", new Path().AddLine(new PointF(1, 1), new PointF(2, 3)).StartFigure().AddLine(new PointF(6, 3), new PointF(5, 1))],
            ["LineRightUpPortrait", new Path().AddLine(new PointF(1, 3), new PointF(2, 1)).StartFigure().AddLine(new PointF(6, 1), new PointF(5, 3))],
            ["Rectangle", new Path().AddRectangle(new RectangleF(2, 2, 25, 15))],
            ["Rectangle0", new Path().AddRectangle(new RectangleF(1, 1, 0, 0))],
            ["Rectangle1", new Path().AddRectangle(new RectangleF(1, 1, 1, 1))],
            ["Rectangle2", new Path().AddRectangle(new RectangleF(1, 1, 2, 2))],
            ["StarOpen", new Path().AddLines(new(51, 1), new(81, 91), new(3, 36), new(99, 36), new(22, 91))],
            ["StarClose", new Path().AddLines(new(51, 1), new(80, 91), new(3, 36), new(98, 36), new(22, 91)).CloseFigure()],
            ["Pentagon", new Path().AddLines(new(51, 1), new(99, 36), new(81, 91), new(22, 91), new(3, 36)).CloseFigure()],
            ["ArcHalfEllipse", new Path().AddArc(new RectangleF(1, 1, 98, 48), 0, 180)],
            ["Ellipse", new Path().AddEllipse(new RectangleF(2, 2, 95, 45))],
            ["Circle0", new Path().AddRectangle(new RectangleF(1, 1, 0, 0))],
            ["Circle1", new Path().AddRectangle(new RectangleF(1, 1, 1, 1))],
            ["Circle2", new Path().AddRectangle(new RectangleF(1, 1, 2, 2))],
            ["Ellipse10", new Path().AddRectangle(new RectangleF(1, 1, 1, 0))],
            ["Ellipse21", new Path().AddRectangle(new RectangleF(1, 1, 2, 1))],
            ["Ellipse32", new Path().AddRectangle(new RectangleF(1, 1, 3, 2))],
        ];

        private static object?[][] DrawThinLinesWithFormatTestSource =>
        [
            // string name, KnownPixelFormat pixelFormat, Color backColor, Color drawColor, DrawingOptions drawingOptions
            ["32bppArgb_NQ_NB_NA", KnownPixelFormat.Format32bppArgb, Color.Cyan, Color.Blue, new DrawingOptions { AlphaBlending = false } ],
            ["32bppArgb_NQ_AB_NA", KnownPixelFormat.Format32bppArgb, Color.Cyan, Color.Blue, new DrawingOptions { AlphaBlending = true } ],
            ["32bppArgb_NQ_NB_AA", KnownPixelFormat.Format32bppArgb, Color.Empty, Color.FromArgb(128, Color.Blue), new DrawingOptions { AlphaBlending = false, AntiAliasing = true } ],
            ["32bppArgb_NQ_AB_AA", KnownPixelFormat.Format32bppArgb, Color.Empty, Color.FromArgb(128, Color.Blue), new DrawingOptions { AlphaBlending = true, AntiAliasing = true } ],
            ["32bppArgb_NQ_Tr_NB", KnownPixelFormat.Format32bppArgb, Color.Empty, Color.Transparent, new DrawingOptions { AlphaBlending = false } ],
            ["32bppArgb_NQ_Tr_AB", KnownPixelFormat.Format32bppArgb, Color.Empty, Color.Transparent, new DrawingOptions { AlphaBlending = true } ],
            ["32bppArgb_QSys256_NB", KnownPixelFormat.Format32bppArgb, Color.Empty, Color.FromArgb(128, Color.Blue), new DrawingOptions { AlphaBlending = false, Quantizer = PredefinedColorsQuantizer.SystemDefault8BppPalette() } ],
            ["32bppArgb_QSys256_AB", KnownPixelFormat.Format32bppArgb, Color.Empty, Color.FromArgb(128, Color.Blue), new DrawingOptions { AlphaBlending = true, Quantizer = PredefinedColorsQuantizer.SystemDefault8BppPalette() } ],
            ["32bppArgb_B8_NB", KnownPixelFormat.Format32bppArgb, Color.Empty, Color.FromArgb(128, Color.Blue), new DrawingOptions { AlphaBlending = false, Quantizer = PredefinedColorsQuantizer.SystemDefault8BppPalette(), Ditherer = OrderedDitherer.Bayer8x8} ],
            ["32bppArgb_B8_AB", KnownPixelFormat.Format32bppArgb, Color.Empty, Color.FromArgb(128, Color.Blue), new DrawingOptions { AlphaBlending = true, Quantizer = PredefinedColorsQuantizer.SystemDefault8BppPalette(), Ditherer = OrderedDitherer.Bayer8x8 } ],
            ["32bppArgb_Wu256_NB", KnownPixelFormat.Format32bppArgb, Color.Empty, Color.FromArgb(128, Color.Blue), new DrawingOptions { AlphaBlending = false, Quantizer = OptimizedPaletteQuantizer.Wu() } ],
            ["32bppArgb_FSSerp_NB", KnownPixelFormat.Format32bppArgb, Color.Empty, Color.FromArgb(128, Color.Blue), new DrawingOptions { AlphaBlending = false, Quantizer = PredefinedColorsQuantizer.SystemDefault8BppPalette(), Ditherer = ErrorDiffusionDitherer.FloydSteinberg.ConfigureProcessingDirection(true) } ],
            ["1bppIndexed", KnownPixelFormat.Format1bppIndexed, Color.Cyan, Color.Blue, null],
            ["1bppIndexed_DB", KnownPixelFormat.Format1bppIndexed, Color.Cyan, Color.Blue, new DrawingOptions { Ditherer = OrderedDitherer.BlueNoise } ],
            ["32bppPArgb", KnownPixelFormat.Format32bppPArgb, Color.Cyan, Color.Blue, null],
            ["32bppPArgb_NB_AA", KnownPixelFormat.Format32bppPArgb, Color.Empty, Color.FromArgb(128, Color.Blue), new DrawingOptions { AlphaBlending = false }],
            ["64bppArgb", KnownPixelFormat.Format64bppArgb, Color.Cyan, Color.Blue, null],
            ["64bppPArgb", KnownPixelFormat.Format64bppPArgb, Color.Cyan, Color.Blue, null],
            ["128bppArgb", KnownPixelFormat.Format128bppRgba, Color.Cyan, Color.Blue, null],
            ["128bppPArgb", KnownPixelFormat.Format128bppPRgba, Color.Cyan, Color.Blue, null],
        ];

        private static object?[][] DrawThinLineClippingCasesTestSource =>
        [
            // string name, Path path
            ["HorizontalShort", new Path().AddLine(new PointF(0, 0), new PointF(1, 0)).StartFigure().AddLine(new PointF(9, 9), new PointF(8, 9))],
            ["HorizontalFull", new Path().AddLine(new PointF(0, 0), new PointF(9, 0)).StartFigure().AddLine(new PointF(9, 9), new PointF(0, 9))],
            ["HorizontalCutLeftRight", new Path().AddLine(new PointF(-1, 0), new PointF(0, 0)).StartFigure().AddLine(new PointF(10, 9), new PointF(9, 9))],
            ["HorizontalOffLeftRight", new Path().AddLine(new PointF(-10, 5), new PointF(-1, 5)).StartFigure().AddLine(new PointF(10, 5), new PointF(15, 5))],
            ["HorizontalOffTopBottom", new Path().AddLine(new PointF(1, -1), new PointF(2, -1)).StartFigure().AddLine(new PointF(1, 10), new PointF(2, 10))],
            ["VerticalShort", new Path().AddLine(new PointF(0, 0), new PointF(0, 1)).StartFigure().AddLine(new PointF(9, 9), new PointF(9, 8))],
            ["VerticalFull", new Path().AddLine(new PointF(0, 0), new PointF(0, 9)).StartFigure().AddLine(new PointF(9, 9), new PointF(9, 0))],
            ["VerticalCutTopBottom", new Path().AddLine(new PointF(0, -1), new PointF(0, 0)).StartFigure().AddLine(new PointF(9, 10), new PointF(9, 9))],
            ["VerticalOffTopBottom", new Path().AddLine(new PointF(5, -10), new PointF(5, -1)).StartFigure().AddLine(new PointF(5, 10), new PointF(5, 15))],
            ["VerticalOffLeftRight", new Path().AddLine(new PointF(-1, 1), new PointF(-1, 2)).StartFigure().AddLine(new PointF(10, 1), new PointF(10, 2))],
            [
                "LandscapeShort", new Path().AddLine(new PointF(0, 0), new PointF(2, 1))
                    .StartFigure().AddLine(new PointF(9, 0), new PointF(7, 1))
                    .StartFigure().AddLine(new PointF(9, 9), new PointF(7, 8))
                    .StartFigure().AddLine(new PointF(0, 9), new PointF(2, 8))
            ],
            ["LandscapeFull", new Path().AddLine(new PointF(0, 0), new PointF(9, 1)).StartFigure().AddLine(new PointF(9, 9), new PointF(0, 8))],
            [
                "LandscapeCut", new Path().AddLines(new PointF(-1, 2), new PointF(5, -1), new PointF(10, 2))
                    .StartFigure().AddLines(new PointF(-1, 7), new PointF(5, 10), new PointF(10, 7))
            ],
            ["LandscapeOffLeftRight", new Path().AddLine(new PointF(-10, 5), new PointF(-1, 6)).StartFigure().AddLine(new PointF(10, 5), new PointF(15, 6))],
            ["LandscapeOffTopBottom", new Path().AddLine(new PointF(1, -1), new PointF(3, -2)).StartFigure().AddLine(new PointF(1, 10), new PointF(3, 11))],
            [
                "PortraitShort", new Path().AddLine(new PointF(0, 0), new PointF(1, 2))
                    .StartFigure().AddLine(new PointF(9, 0), new PointF(8, 2))
                    .StartFigure().AddLine(new PointF(9, 9), new PointF(8, 7))
                    .StartFigure().AddLine(new PointF(0, 9), new PointF(1, 7))
            ],
            ["PortraitFull", new Path().AddLine(new PointF(0, 0), new PointF(1, 9)).StartFigure().AddLine(new PointF(9, 9), new PointF(8, 0))],
            [
                "PortraitCut", new Path().AddLines(new PointF(2, -1), new PointF(-1, 5), new PointF(2, 10))
                    .StartFigure().AddLines(new PointF(7, -1), new PointF(10, 5), new PointF(7, 10))
            ],
            ["PortraitOffLeftRight", new Path().AddLine(new PointF(-1, 1), new PointF(-2, 3)).StartFigure().AddLine(new PointF(10, 1), new PointF(11, 3))],
            ["PortraitOffTopBottom", new Path().AddLine(new PointF(5, -10), new PointF(6, -1)).StartFigure().AddLine(new PointF(5, 10), new PointF(6, 15))],
        ];

        private static object?[][] TextureBrushTestSource =>
        [
            // string name, KnownPixelFormat pixelFormat, bool hasAlphaHint, DrawingOptions options
            ["FillSessionNoBlend_NA_NB", true, new DrawingOptions { AlphaBlending = false } ],
            ["FillSessionBlend_NA_AB", true, new DrawingOptions { AlphaBlending = true } ],
            ["FillSessionWithQuantizing_NA_AB_QRgb24", true, new DrawingOptions { AlphaBlending = true, Quantizer = PredefinedColorsQuantizer.Rgb888(Color.White) } ],
            ["FillSessionWithQuantizing_NA_NB_QSys4", true, new DrawingOptions { AlphaBlending = false, Quantizer = PredefinedColorsQuantizer.SystemDefault4BppPalette(Color.White) } ],
            ["FillSessionWithQuantizing_NA_AB_QSys4", true, new DrawingOptions { AlphaBlending = true, Quantizer = PredefinedColorsQuantizer.SystemDefault4BppPalette() } ],
            ["FillSessionWithQuantizing_NA_AB_QSys4_DFS", true, new DrawingOptions { AlphaBlending = true, Quantizer = PredefinedColorsQuantizer.SystemDefault4BppPalette(), Ditherer = ErrorDiffusionDitherer.FloydSteinberg } ],
            ["FillSessionWithQuantizing_NA_AB_QSys4_DFS_Op", false, new DrawingOptions { AlphaBlending = true, Quantizer = PredefinedColorsQuantizer.SystemDefault4BppPalette(), Ditherer = ErrorDiffusionDitherer.FloydSteinberg } ],
            ["FillSessionWithQuantizing_NA_AB_QSys4_DFSS", true, new DrawingOptions { AlphaBlending = true, Quantizer = PredefinedColorsQuantizer.SystemDefault4BppPalette(), Ditherer = ErrorDiffusionDitherer.FloydSteinberg.ConfigureProcessingDirection(true) } ],
            ["FillSessionWithQuantizing_NA_NB_QWu", true, new DrawingOptions { AlphaBlending = false, Quantizer = OptimizedPaletteQuantizer.Wu() } ],
            ["FillSessionWithQuantizing_NA_NB_QWu_Op", false, new DrawingOptions { AlphaBlending = false, Quantizer = OptimizedPaletteQuantizer.Wu() } ],
            ["FillSessionWithQuantizing_NA_AB_QWu", true, new DrawingOptions { AlphaBlending = true, Quantizer = OptimizedPaletteQuantizer.Wu() } ],
            ["FillSessionWithQuantizing_AA_NB_QWu", true, new DrawingOptions { AntiAliasing = true, AlphaBlending = false, Quantizer = OptimizedPaletteQuantizer.Wu() } ],
            ["FillSessionWithQuantizing_AA_NB_QWu_Op", false, new DrawingOptions { AntiAliasing = true, AlphaBlending = false, Quantizer = OptimizedPaletteQuantizer.Wu() } ],
            ["FillSessionWithQuantizing_AA_AB_QWu", true, new DrawingOptions { AntiAliasing = true, AlphaBlending = true, Quantizer = OptimizedPaletteQuantizer.Wu() } ],
            ["FillSessionWithQuantizing_AA_AB_QWu_Linear", true, new DrawingOptions { AntiAliasing = true, AlphaBlending = true, Quantizer = OptimizedPaletteQuantizer.Wu().ConfigureColorSpace(WorkingColorSpace.Linear) } ],
        ];

        #endregion

        #region Methods

        [TestCaseSource(nameof(FillPathTestSource))]
        public void FillPathTest(string name, KnownPixelFormat pixelFormat, WorkingColorSpace colorSpace, Color backColor, Color fillColor, DrawingOptions options)
        {
            var path = new Path(false)
                //.TransformScale(2, 2)
                //.TransformTranslation(0.5f, 0.5f)
                //.TransformRotation(45)
                ;

            // reference polygon from https://www.cs.rit.edu/~icss571/filling/example.html (appears with inverted Y coords)
            //path.AddLines(new(10, 10), new(10, 16), new(16, 20), new(28, 10), new(28, 16), new(22, 10));

            // star, small
            //path.AddLines(new(30, 20), new(26, 30), new(35, 24), new(25, 24), new(34, 30));

            // star, big
            //path.AddPolygon(new(50, 0), new(79, 90), new(2, 35), new(97, 35), new(21, 90));
            //path.AddLines(new(300, 200), new(260, 300), new(350, 240), new(250, 240), new(340, 300));
            //path.CloseFigure(); // combine with the following to mix two closed figures - note: causes holes even with Alternate mode, but the same happens for GDI+, too

            // Multiple stars with all possible edge relations (to test EdgeInfo.ConfigureEdgeRelation)
            path.TransformTranslation(50, 0);
            path.AddPolygon(new(300, 300), new(260, 200), new(350, 260), new(250, 260), new(340, 200));
            path.AddPolygon(new(50, 50), new(90, 150), new(0, 90), new(100, 90), new(10, 150));
            path.AddPolygon(new(300, 50), new(260, 150), new(350, 90), new(250, 90), new(340, 150));
            path.AddPolygon(new(50, 300), new(90, 200), new(0, 260), new(100, 260), new(10, 200));

            using var bitmapDataBackground = BitmapDataFactory.CreateBitmapData(path.Bounds.Size + new Size(path.Bounds.Location) + new Size(Math.Abs(path.Bounds.X), Math.Abs(path.Bounds.Y) /*path.Bounds.Location*/), pixelFormat, colorSpace);
            if (backColor != Color.Empty)
                bitmapDataBackground.Clear(backColor, options.Ditherer);
            else
                GenerateAlphaGradient(bitmapDataBackground);

            IAsyncContext context = new SimpleContext(-1);

            // non-cached region
            using var bitmapData1 = bitmapDataBackground.Clone();
            bitmapData1.FillPath(context, Brush.CreateSolid(fillColor), path, options);
            SaveBitmapData(name, bitmapData1);

            // generating cached region
            var pathCached = new Path(path) { PreferCaching = true };
            using var bitmapData2 = bitmapDataBackground.Clone();
            bitmapData2.FillPath(context, Brush.CreateSolid(fillColor), pathCached, options);
            AssertAreEqual(bitmapData1, bitmapData2);

            // re-using region from cache
            using var bitmapData3 = bitmapDataBackground.Clone();
            bitmapData3.FillPath(context, Brush.CreateSolid(fillColor), pathCached, options);
            AssertAreEqual(bitmapData1, bitmapData3);
        }

        [TestCaseSource(nameof(FillPathTestSource))]
        public void ClippedFillPathTest(string name, KnownPixelFormat pixelFormat, WorkingColorSpace colorSpace, Color backColor, Color fillColor, DrawingOptions options)
        {
            var offset = new SizeF(-10, -10);
            var path = new Path(false);
            path.AddLines(new PointF(50, 0) + offset, new PointF(90, 100) + offset, new PointF(0, 40) + offset, new PointF(100, 40) + offset, new PointF(10, 100) + offset);
            //path
            //    .AddLines(new(50, 0), new(79, 90), new(2, 35), new(97, 35), new(21, 90)).CloseFigure()
            //    .AddEllipse(new RectangleF(0, 0, 100, 100))
            //    .AddRectangle(new RectangleF(0, 0, 100, 100));

            using var bitmapDataBackground = BitmapDataFactory.CreateBitmapData((path.Bounds.Size + new SizeF(offset.Width * 2, offset.Height * 2)).ToSize(), pixelFormat, colorSpace);
            if (backColor != Color.Empty)
                bitmapDataBackground.Clear(backColor, options.Ditherer);
            else
                GenerateAlphaGradient(bitmapDataBackground);

            //var singleThreadContext = new SimpleContext(1);

            // non-cached region
            using var bitmapData1 = bitmapDataBackground.Clone();
            bitmapData1.FillPath(null, Brush.CreateSolid(fillColor), path, options);
            SaveBitmapData(name, bitmapData1);

            // generating cached region
            path.PreferCaching = true;
            using var bitmapData2 = bitmapDataBackground.Clone();
            bitmapData2.FillPath(null, Brush.CreateSolid(fillColor), path, options);
            AssertAreEqual(bitmapData1, bitmapData2);

            // re-using region from cache
            using var bitmapData3 = bitmapDataBackground.Clone();
            bitmapData3.FillPath(null, Brush.CreateSolid(fillColor), path, options);
            AssertAreEqual(bitmapData1, bitmapData3);
        }

        [TestCaseSource(nameof(DrawOpenPathTestSource))]
        public void DrawOpenPathTest(string name, Path path, float width)
        {
            var pixelFormat = KnownPixelFormat.Format32bppArgb;
            var colorSpace = WorkingColorSpace.Linear;
            Size size = path.Bounds.Size
                + new Size(path.Bounds.Location)
                + new Size(Math.Abs(path.Bounds.X), Math.Abs(path.Bounds.Y))
                + Size.Ceiling(new SizeF(width, width));

            if (width > 1f)
                path = Path.Transform(path, TransformationMatrix.CreateTranslation((float)Math.Floor(width / 2f), (float)Math.Floor(width / 2f)));

            using var bitmapData = BitmapDataFactory.CreateBitmapData(size, pixelFormat, colorSpace);
            IAsyncContext context = new SimpleContext(-1);

            foreach (bool antiAliasing in new[] { false, true })
            {
                LineCapStyle[] capStyles = width <= 1f ? [LineCapStyle.Flat] : [LineCapStyle.Flat, LineCapStyle.Square, LineCapStyle.Triangle, LineCapStyle.Round];
                foreach (LineCapStyle capStyle in capStyles)
                foreach (bool fastThinLines in new[] { false, true })
                {
                    if (fastThinLines && (width > 1f || antiAliasing))
                        continue;
                    bitmapData.Clear(Color.Cyan);

                    var drawingOptions = new DrawingOptions { AntiAliasing = antiAliasing, FastThinLines = fastThinLines, DrawPathPixelOffset = ((int)Math.Ceiling(width) & 1) == 1 ? PixelOffset.Half : PixelOffset.None };
                    var pen = new Pen(Color.Blue, width) { StartCap = capStyle, EndCap = capStyle };
                    bitmapData.DrawPath(context, pen, path, drawingOptions);
                    SaveBitmapData($"{name}_{(antiAliasing ? "AA" : "NA")}_W{width:F2}_{capStyle}{(fastThinLines ? "_F" : null)}", bitmapData);
                }
            }
        }

        [TestCaseSource(nameof(DrawClosedPathTestSource))]
        public void DrawClosedPathTest(string name, Path path, float width)
        {
            path.TransformAdded(TransformationMatrix.CreateTranslation(width * 2, width * 2));
            var pixelFormat = KnownPixelFormat.Format32bppArgb;
            var colorSpace = WorkingColorSpace.Linear;
            var bounds = path.Bounds;
            Size size = bounds.Size + new Size(bounds.Location) + new Size(Math.Abs(bounds.X), Math.Abs(bounds.Y)) + new Size((int)width * 2, (int)width * 2);

            using var bitmapData = BitmapDataFactory.CreateBitmapData(size, pixelFormat, colorSpace);
            IAsyncContext context = new SimpleContext(-1);

            foreach (bool antiAliasing in new[] { false, true })
            foreach (bool fastThinLines in new[] { false, true })
            {
                if (fastThinLines && (width > 1f || antiAliasing))
                    continue;
                var drawingOptions = new DrawingOptions { AntiAliasing = antiAliasing, FastThinLines = fastThinLines, DrawPathPixelOffset = ((int)Math.Ceiling(width) & 1) == 1 ? PixelOffset.Half : PixelOffset.None };
                LineJoinStyle[] joinStyles = [LineJoinStyle.Miter, LineJoinStyle.Bevel, LineJoinStyle.Round];
                foreach (LineJoinStyle joinStyle in joinStyles)
                {
                    if (joinStyle > LineJoinStyle.Miter && width <= 1f)
                        continue;

                    bitmapData.Clear(Color.Cyan);

                    var pen = new Pen(Color.Blue, width) { LineJoin = joinStyle };
                    bitmapData.DrawPath(context, pen, path, drawingOptions);
                    SaveBitmapData($"{name}_{(antiAliasing ? "AA" : "NA")}{(width <= 1f && fastThinLines ? "_F" : "")}_W{width:00}_{joinStyle}", bitmapData);
                }
            }
        }

        [TestCaseSource(nameof(DrawThinLinesTestSource))]
        public void DrawThinLinesTest(string name, Path path)
        {
            var pixelFormat = KnownPixelFormat.Format32bppArgb;
            var colorSpace = WorkingColorSpace.Srgb; // the bad blending makes the unaligned fill/draw shapes more apparent
            var bounds = path.RawPath.DrawOutlineBounds;
            Size size = bounds.Size + new Size(bounds.Location) + new Size(Math.Abs(bounds.X), Math.Abs(bounds.Y));
            Assert.IsFalse(bounds.IsEmpty());

            using var bitmapDataBackground = BitmapDataFactory.CreateBitmapData(size, pixelFormat, colorSpace);
            bitmapDataBackground.Clear(Color.Green);
            IAsyncContext context = new SimpleContext(-1);
            var pen = new Pen(Color.Yellow);
            //var pen = new Pen(Color.FromArgb(128, Color.Yellow)); // This always uses region-draw but makes the layers visible. Non-region-draw cases are covered by DrawThinLinesWithFormatTest
            var brush = Brush.CreateSolid(Color.Blue);
            foreach (bool antiAliasing in new[] { false, true })
            foreach (bool fastThinLines in new[] { true, false })
            foreach (var offset in new[] { PixelOffset.None, PixelOffset.Half })
            {
                if (antiAliasing && fastThinLines)
                    continue;

                var drawingOptions = new DrawingOptions { AntiAliasing = antiAliasing, DrawPathPixelOffset = offset, FastThinLines = fastThinLines };

                path.PreferCaching = false;
                using var bitmapData1 = bitmapDataBackground.Clone();
                bitmapData1.FillPath(context, brush, path, drawingOptions);
                bitmapData1.DrawPath(context, pen, path, drawingOptions);
                SaveBitmapData($"{name}_{(antiAliasing ? "AA" : $"NA{(fastThinLines ? "F" : "S")}")}_{offset}", bitmapData1);

                path.PreferCaching = true;
                using var bitmapData2 = bitmapDataBackground.Clone();
                bitmapData2.FillPath(context, brush, path, drawingOptions);
                bitmapData2.DrawPath(context, pen, path, drawingOptions);
                AssertAreEqual(bitmapData1, bitmapData2);

                using var bitmapData3 = bitmapDataBackground.Clone();
                bitmapData3.FillPath(context, brush, path, drawingOptions);
                bitmapData3.DrawPath(context, pen, path, drawingOptions);
                AssertAreEqual(bitmapData1, bitmapData3);
            }
        }

        [TestCaseSource(nameof(DrawThinLinesWithFormatTestSource))]
        public void DrawThinLinesWithFormatTest(string name, KnownPixelFormat pixelFormat, Color backColor, Color drawColor, DrawingOptions? options)
        {
            var path = new Path(false)
                .TransformTranslation(1, 1)
                .AddPolygon(new(50, 0), new(79, 90), new(2, 35), new(97, 35), new(21, 90))
                .AddEllipse(0, 0, 100, 100)
                .AddRoundedRectangle(0, 0, 100, 100, cornerRadius: 10);

            var bounds = path.RawPath.DrawOutlineBounds;
            Size size = bounds.Size + new Size(bounds.Location) + new Size(Math.Abs(bounds.X), Math.Abs(bounds.Y));
            Assert.IsFalse(bounds.IsEmpty());

            using var bitmapDataBackground = BitmapDataFactory.CreateBitmapData(size, pixelFormat);
            if (backColor != Color.Empty)
                bitmapDataBackground.Clear(backColor, options?.Ditherer);
            else
                GenerateAlphaGradient(bitmapDataBackground);

            IAsyncContext context = new SimpleContext(-1);
            var pen = new Pen(drawColor);
            
            using var bitmapData1 = bitmapDataBackground.Clone();
            bitmapData1.DrawPath(context, pen, path, options);
            SaveBitmapData(name, bitmapData1);

            path.PreferCaching = true;
            using var bitmapData2 = bitmapDataBackground.Clone();
            bitmapData2.DrawPath(context, pen, path, options);
            AssertAreEqual(bitmapData1, bitmapData2);

            using var bitmapData3 = bitmapDataBackground.Clone();
            bitmapData3.DrawPath(context, pen, path, options);
            AssertAreEqual(bitmapData1, bitmapData3);
        }

        [TestCaseSource(nameof(DrawThinLineClippingCasesTestSource))]
        public void DrawThinLineClippingCasesTest(string name, Path path)
        {
            Size size = new Size(10, 10);

            using var bitmapDataBackground = BitmapDataFactory.CreateBitmapData(size);
            bitmapDataBackground.Clear(Color.Cyan);

            IAsyncContext context = new SimpleContext(-1);
            var pen = new Pen(Color.Blue);

            path.PreferCaching = false;
            using var bitmapData1 = bitmapDataBackground.Clone();
            bitmapData1.DrawPath(context, pen, path);
            SaveBitmapData(name, bitmapData1);

            path.PreferCaching = true;
            using var bitmapData2 = bitmapDataBackground.Clone();
            bitmapData2.DrawPath(context, pen, path);
            AssertAreEqual(bitmapData1, bitmapData2);

            using var bitmapData3 = bitmapDataBackground.Clone();
            bitmapData3.DrawPath(context, pen, path);
            AssertAreEqual(bitmapData1, bitmapData3);
        }

        [TestCaseSource(nameof(PixelOffsetTestSource))]
        public void ScanPixelOffsetTest(string name, Path path)
        {
            var bounds = path.RawPath.Bounds;
            Size size = bounds.Size + new Size(bounds.Location) + new Size(Math.Abs(bounds.X), Math.Abs(bounds.Y));
            Assert.IsFalse(bounds.IsEmpty());

            using var bitmapDataBackground = BitmapDataFactory.CreateBitmapData(size);
            bitmapDataBackground.Clear(Color.Cyan);
            IAsyncContext context = new SimpleContext(-1);
            var brush = Brush.CreateSolid(Color.Blue);
            foreach (bool antiAliasing in new[] { false, true })
            foreach (PixelOffset pixelOffset in new[] { PixelOffset.None, PixelOffset.Half })
            {
                var options = new DrawingOptions { ScanPathPixelOffset = pixelOffset, AntiAliasing = antiAliasing };
                path.PreferCaching = false;
                using var bitmapData1 = bitmapDataBackground.Clone();
                bitmapData1.FillPath(context, brush, path, options);
                SaveBitmapData($"{name}_{(antiAliasing ? "AA" : "NA")}_{pixelOffset}", bitmapData1);

                path.PreferCaching = true;
                using var bitmapData2 = bitmapDataBackground.Clone();
                bitmapData2.FillPath(context, brush, path, options);
                AssertAreEqual(bitmapData1, bitmapData2);

                using var bitmapData3 = bitmapDataBackground.Clone();
                bitmapData3.FillPath(context, brush, path, options);
                AssertAreEqual(bitmapData1, bitmapData3);
            }
        }

        [TestCaseSource(nameof(PixelOffsetTestSource))]
        public void DrawPathPixelOffsetTest(string name, Path path)
        {
            var bounds = path.RawPath.DrawOutlineBounds;
            Size size = bounds.Size + new Size(bounds.Location) + new Size(Math.Abs(bounds.X), Math.Abs(bounds.Y));
            Assert.IsFalse(bounds.IsEmpty());

            using var bitmapDataBackground = BitmapDataFactory.CreateBitmapData(size);
            bitmapDataBackground.Clear(Color.Cyan);
            IAsyncContext context = new SimpleContext(-1);
            foreach (float width in new[] { 1f, 2f })
            foreach (bool antiAliasing in new[] { false, true })
            foreach (PixelOffset pixelOffset in new[] { PixelOffset.None, PixelOffset.Half })
            {
                // Missing right/bottom line
                var pen = new Pen(Color.Blue, width);
                var options = new DrawingOptions { DrawPathPixelOffset = pixelOffset, AntiAliasing = antiAliasing, FastThinLines = false };
                path.PreferCaching = false;
                using var bitmapData1 = bitmapDataBackground.Clone();
                bitmapData1.DrawPath(context, pen, path, options);
                SaveBitmapData($"{name}_{width}_{(antiAliasing ? "AA" : "NA")}_{pixelOffset}", bitmapData1);

                path.PreferCaching = true;
                using var bitmapData2 = bitmapDataBackground.Clone();
                bitmapData2.DrawPath(context, pen, path, options);
                AssertAreEqual(bitmapData1, bitmapData2);

                using var bitmapData3 = bitmapDataBackground.Clone();
                bitmapData3.DrawPath(context, pen, path, options);
                AssertAreEqual(bitmapData1, bitmapData3);
            }
        }

        [TestCaseSource(nameof(TextureBrushTestSource))]
        public void TextureBrushTest(string name, bool hasAlphaHint, DrawingOptions options)
        {
            var path = new Path()
                .TransformTranslation(10, 10)
                .AddLines(new(50, 0), new(79, 90), new(2, 35), new(97, 35), new(21, 90)).CloseFigure()
                .AddEllipse(new RectangleF(0, 0, 100, 100))
                .AddRoundedRectangle(new RectangleF(0, 0, 100, 100), 10);

            var bounds = path.RawPath.DrawOutlineBounds;
            Size size = bounds.Size + new Size(bounds.Location) + new Size(Math.Abs(bounds.X), Math.Abs(bounds.Y));
            
            using IReadWriteBitmapData background = BitmapDataFactory.CreateBitmapData(size);
            GenerateAlphaGradient(background);
            //Color32 backColor = pixelFormat.HasAlpha() ? Color.Empty : Color.White;
            //if (backColor != default)
            //    background.Clear(backColor);

            using IReadWriteBitmapData fillTexture = GenerateAlphaGradientBitmapData(new Size(size.Width / 4, size.Height / 4));

            //using IReadWriteBitmapData fillTexture = GetInfoIcon16();

            using IReadWriteBitmapData drawTexture = BitmapDataFactory.CreateBitmapData(2, 2);
            drawTexture.SetPixel(0, 0, Color32.FromArgb(64, Color32.White));
            drawTexture.SetPixel(1, 0, Color32.FromArgb(64, Color.Red));
            drawTexture.SetPixel(0, 1, Color32.FromArgb(64, Color.Lime));
            drawTexture.SetPixel(1, 1, Color32.FromArgb(64, Color.Blue));

            if (!hasAlphaHint)
            {
                fillTexture.MakeOpaque(Color32.White);
                drawTexture.MakeOpaque(Color32.White);
            }

            IAsyncContext context = new SimpleContext(-1);
            var brush = Brush.CreateTexture(fillTexture, hasAlphaHint: hasAlphaHint);
            var pen = new Pen(Brush.CreateTexture(drawTexture));

            path.PreferCaching = false;
            using var bitmapData1 = background.Clone();
            bitmapData1.FillPath(context, brush, path, options);
            bitmapData1.DrawPath(context, pen, path, options);
            SaveBitmapData(name, bitmapData1);

            path.PreferCaching = true;
            using var bitmapData2 = background.Clone();
            bitmapData2.FillPath(context, brush, path, options);
            bitmapData2.DrawPath(context, pen, path, options);
            AssertAreEqual(bitmapData1, bitmapData2);

            using var bitmapData3 = background.Clone();
            bitmapData3.FillPath(context, brush, path, options);
            bitmapData3.DrawPath(context, pen, path, options);
            AssertAreEqual(bitmapData1, bitmapData3);
        }

        [TestCase(TextureMapMode.Tile)]
        [TestCase(TextureMapMode.TileFlipX)]
        [TestCase(TextureMapMode.TileFlipY)]
        [TestCase(TextureMapMode.TileFlipXY)]
        [TestCase(TextureMapMode.Clip)]
        [TestCase(TextureMapMode.Extend)]
        [TestCase(TextureMapMode.Center)]
        [TestCase(TextureMapMode.CenterExtend)]
        [TestCase(TextureMapMode.Stretch)]
        [TestCase(TextureMapMode.Zoom)]
        public void TextureBrushTilingTest(TextureMapMode mapMode)
        {
            var path = new Path(false)
                .AddEllipse(new RectangleF(0, 0, 100, 50));

            using var bitmap = BitmapDataFactory.CreateBitmapData(100, 150);
            using var texture = GetShieldIcon16();
            using var textureOpaque = texture.Clone(KnownPixelFormat.Format24bppRgb);

            foreach (bool blend in new[] { false, true })
            foreach (bool antiAliasing in new[] { false, true })
            foreach (Point textureOffset in new[] { Point.Empty, new Point(-texture.Width / 4, texture.Height / 2) })
            {
                bitmap.Clear(Color.Navy);
                var brush = Brush.CreateTexture(mapMode < TextureMapMode.Clip ? texture : textureOpaque, mapMode: mapMode, textureOffset);
                foreach (int offset in new[] { -50, 0, 50 })
                    bitmap.FillPath(null, brush, path, new DrawingOptions { AlphaBlending = blend, AntiAliasing = antiAliasing, Transformation = TransformationMatrix.CreateTranslation(offset, offset + 50) });

                SaveBitmapData($"{mapMode}_{(blend ? "AB" : "NB")}_{(antiAliasing ? "AA" : "NA")}{(textureOffset.IsEmpty ? null : "_Offset")}", bitmap);
            }
        }

        [TestCase("horizontal", 20f, 10f, 80f, 10f)]
        [TestCase("vertical", 10f, 20f, 10f, 120f)]
        [TestCase("diagonal", -10f, 10f, 10f, -10f)]
        public void GradientBrushWithEndpointsTest(string name, float x1, float y1, float x2, float y2)
        {
            var path = new Path(false)
                .AddEllipse(new RectangleF(0, 0, 100, 50));

            using var bitmap = BitmapDataFactory.CreateBitmapData(100, 150);

            foreach (GradientWrapMode mapMode in new[] { GradientWrapMode.Stop, GradientWrapMode.Clip, GradientWrapMode.Repeat, GradientWrapMode.Mirror })
            foreach (bool blend in new[] { false, true })
            foreach (bool antiAliasing in new[] { false, true })
            {
                bitmap.Clear(Color.Navy);
                var brush = Brush.CreateLinearGradient(new PointF(x1, y1), new PointF(x2, y2), Color.Red, Color.Blue, mapMode, WorkingColorSpace.Linear);
                foreach (int offset in new[] { -50, 0, 50 })
                    bitmap.FillPath(null, brush, path, new DrawingOptions { AlphaBlending = blend, AntiAliasing = antiAliasing, Transformation = TransformationMatrix.CreateTranslation(offset, offset + 50) });

                SaveBitmapData($"{name}_{mapMode}_{(blend ? "AB" : "NB")}_{(antiAliasing ? "AA" : "NA")}", bitmap);
            }
        }

        [TestCase("horizontal right", 0f)]
        [TestCase("horizontal left", 180f)]
        [TestCase("vertical down", 90f)]
        [TestCase("vertical up", 270f)]
        [TestCase("diagonal", 45f)]
        [TestCase("almost horizontal", 13f)]
        public void GradientBrushWithAngleTest(string name, float angle)
        {
            var path = new Path(false)
                .AddEllipse(new RectangleF(0, 0, 100, 50));

            using var bitmap = BitmapDataFactory.CreateBitmapData(100, 150);

            foreach (bool antiAliasing in new[] { false, true })
            {
                bitmap.Clear(Color.Cyan);
                var brush = Brush.CreateLinearGradient(angle, Color.White, Color.Black);
                foreach (int offset in new[] { -50, 0, 50 })
                    bitmap.FillPath(null, brush, path, new DrawingOptions {AntiAliasing = antiAliasing, Transformation = TransformationMatrix.CreateTranslation(offset, offset + 50) });

                SaveBitmapData($"{name}_{(antiAliasing ? "AA" : "NA")}", bitmap);
            }
        }

        [TestCase(0f, 45f)]
        [TestCase(45f, 90f)]
        [TestCase(-45f, 90f)]
        [TestCase(15f, 400f)]
        public void ConnectedArcTest(float start, float sweep)
        {
            var path = new Path()
                //.AddPie(new RectangleF(0, 0, 100, 50), start, sweep)
                .AddPoint(new PointF(50, 0))
                .AddArc(new RectangleF(0, 0, 100, 50), start, sweep)
                .AddPoint(new PointF(50, 0));

            var bounds = path.RawPath.DrawOutlineBounds;
            Size size = bounds.Size + new Size(bounds.Location) + new Size(Math.Abs(bounds.X), Math.Abs(bounds.Y));

            using var bmp1 = BitmapDataFactory.CreateBitmapData(size);
            bmp1.DrawPath(new Pen(Color.Blue), path);
            SaveBitmapData($"{start}_{sweep}_F", bmp1);

            using var bmp2 = BitmapDataFactory.CreateBitmapData(size);
            bmp2.DrawPath(new Pen(Color.Blue), path, new DrawingOptions { FastThinLines = false });
            SaveBitmapData($"{start}_{sweep}_NF", bmp2);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void TransformedCircleTest(bool antiAliased)
        {
            // Adding a circle, and applying a transformation with 2x1 scale and 13° rotation: the result should be a rotated ellipse,
            // which is always drawn as Bézier curves, even when drawing fast thin lines.
            var path = new Path()
                .AddEllipse(new RectangleF(0, 0, 100, 100));

            var transform = TransformationMatrix.CreateTranslation(30, 0)
                 * TransformationMatrix.CreateScale(2, 1)
                 * TransformationMatrix.CreateRotation(13);
            path.TransformAdded(transform);

            var options = new DrawingOptions { AntiAliasing = antiAliased };
            var bounds = path.RawPath.DrawOutlineBounds;
            Size size = bounds.Size + new Size(bounds.Location) + new Size(Math.Abs(bounds.X), Math.Abs(bounds.Y));
            using var bmp = BitmapDataFactory.CreateBitmapData(size);
            bmp.Clear(Color.Cyan);
            bmp.DrawPath(new Pen(Color.Blue), path, options);
            SaveBitmapData($"{(antiAliased ? "AA" : "NA")}", bmp);
        }

        [Test]
        public void EdgeCasesTest()
        {
            Color32 backColor = Color.Cyan;
            Color32 color = Color.Blue;
            using var bmp = BitmapDataFactory.CreateBitmapData(100, 100);
            bmp.Clear(backColor);

            var optionsNoFastThinLines = new DrawingOptions { FastThinLines = false };
            var optionsTwoPassQuantizer = new DrawingOptions { Quantizer = OptimizedPaletteQuantizer.Wu() };

            // Horizontal line from -∞ to +∞
            Assert.Throws<OverflowException>(() => bmp.DrawLine(color, Single.NegativeInfinity, 1, Single.PositiveInfinity, 1)); // shortcut
            Assert.Throws<OverflowException>(() => bmp.DrawLine(color, Single.NegativeInfinity, 1, Single.PositiveInfinity, 1, optionsNoFastThinLines)); // no shortcut, no cache
            Assert.Throws<OverflowException>(() => bmp.DrawPath(color, new Path().AddLine(Single.NegativeInfinity, 1, Single.PositiveInfinity, 1))); // shortcut, no cache
            Assert.Throws<OverflowException>(() => bmp.DrawPath(color, new Path().AddLine(Single.NegativeInfinity, 1, Single.PositiveInfinity, 1), optionsNoFastThinLines)); // no shortcut, cache

            // NaN
            Assert.Throws<OverflowException>(() => bmp.DrawLine(color, Single.NaN, 1, 1, 1)); // shortcut
            Assert.Throws<OverflowException>(() => bmp.DrawLine(color, Single.NaN, 1, 1, 1, optionsNoFastThinLines)); // no shortcut, no cache
            Assert.Throws<OverflowException>(() => bmp.DrawPath(color, new Path().AddLine(Single.NaN, 1, 1, 1))); // shortcut, no cache
            Assert.Throws<OverflowException>(() => bmp.DrawPath(color, new Path().AddLine(Single.NaN, 1, 1, 1), optionsNoFastThinLines)); // no shortcut, cache

            // Horizontal line from Int32.MinValue to Int32.MaxValue: direct shortcut is OK, but Path-drawing bounds are not (Bounds.Width overflows)
            Assert.DoesNotThrow(() => bmp.DrawLine(color, Int32.MinValue, 1, Int32.MaxValue, 1)); // shortcut
            Assert.Throws<OverflowException>(() => bmp.DrawLine(color, Int32.MinValue, 1, Int32.MaxValue, 1, optionsNoFastThinLines)); // no shortcut, no cache
            Assert.Throws<OverflowException>(() => bmp.DrawPath(color, new Path().AddLine(Int32.MinValue, 1, Int32.MaxValue, 1))); // shortcut, no cache
            Assert.Throws<OverflowException>(() => bmp.DrawPath(color, new Path().AddLine(Int32.MinValue, 1, Int32.MaxValue, 1), optionsNoFastThinLines)); // no shortcut, cache

            // Diagonal line from Int32.MinValue to Int32.MaxValue: direct shortcut is OK, but Path-drawing bounds are not (Bounds.Width overflows)
            bmp.Clear(backColor);
            Assert.DoesNotThrow(() => bmp.DrawLine(color, Int32.MinValue, Int32.MinValue, Int32.MaxValue, Int32.MaxValue)); // shortcut
            Assert.DoesNotThrow(() => bmp.DrawLine(color, Int32.MinValue, Int32.MinValue, Int32.MaxValue, Int32.MaxValue)); // shortcut
            Assert.Throws<OverflowException>(() => bmp.DrawLine(color, Int32.MinValue, Int32.MinValue, Int32.MaxValue, Int32.MaxValue, optionsNoFastThinLines)); // no shortcut, no cache
            Assert.Throws<OverflowException>(() => bmp.DrawPath(color, new Path().AddLine(Int32.MinValue, Int32.MinValue, Int32.MaxValue, Int32.MaxValue))); // shortcut, no cache
            Assert.Throws<OverflowException>(() => bmp.DrawPath(color, new Path().AddLine(Int32.MinValue, Int32.MinValue, Int32.MaxValue, Int32.MaxValue), optionsNoFastThinLines)); // no shortcut, cache
            bmp.Clear(backColor);
            Assert.DoesNotThrow(() => bmp.DrawLine(color, -1, -1, Int32.MaxValue - 127, Int32.MaxValue - 127, optionsNoFastThinLines)); // no shortcut, not cached due to size - note that result is not visible because the "widened" line is a very thin triangle, with equal p and q for the long sides.
            Assert.DoesNotThrow(() => bmp.DrawLine(color, -1, -1, UInt16.MaxValue, UInt16.MaxValue, optionsNoFastThinLines)); // no shortcut, not cached due to size

            // DrawRectangle
            bmp.Clear(backColor);
            Assert.DoesNotThrow(() => bmp.DrawRectangle(color, Int32.MinValue, Int32.MinValue, Int32.MaxValue, Int32.MaxValue)); // shortcut
            Assert.Throws<OverflowException>(() => bmp.DrawRectangle(color, Int32.MaxValue, Int32.MaxValue, Int32.MaxValue, Int32.MaxValue)); // shortcut
            Assert.Throws<OverflowException>(() => bmp.DrawRectangle(color, (float)(Int32.MaxValue - 127), (Int32.MaxValue - 127), (Int32.MaxValue - 127), (Int32.MaxValue - 127))); // shortcut
            Assert.Throws<OverflowException>(() => bmp.DrawRectangle(color, Int32.MinValue, Int32.MinValue, Int32.MaxValue, Int32.MaxValue, optionsNoFastThinLines)); // no shortcut, no cache: Int32.MaxValue + 1 is the closest float to Int32.MaxValue. The previous one is Int32.MaxValue - 127.
            bmp.Clear(backColor);
            Assert.DoesNotThrow(() => bmp.DrawRectangle(color, Int32.MinValue, Int32.MinValue, Int32.MaxValue - 127, Int32.MaxValue - 127, optionsNoFastThinLines)); // no shortcut, no cache: Int32.MaxValue + 1 is the closest float to Int32.MaxValue. The previous one is Int32.MaxValue - 127.
            Assert.Throws<OverflowException>(() => bmp.DrawPath(color, new Path().AddRectangle(Int32.MinValue, Int32.MinValue, Int32.MaxValue, Int32.MaxValue))); // shortcut, no cache
            Assert.Throws<OverflowException>(() => bmp.DrawPath(color, new Path().AddRectangle(Int32.MinValue, Int32.MinValue, Int32.MaxValue, Int32.MaxValue), optionsNoFastThinLines)); // no shortcut, cache
            bmp.Clear(backColor);
            Assert.DoesNotThrow(() => bmp.DrawPath(color, new Path().AddRectangle(Int32.MinValue, Int32.MinValue, Int32.MaxValue - 127, Int32.MaxValue - 127), optionsNoFastThinLines)); // no shortcut, no cache due to size
            bmp.Clear(backColor);
            Assert.DoesNotThrow(() => bmp.DrawPath(color, new Path().AddRectangle(0, 0, Int32.MaxValue - 127, Int32.MaxValue - 127), optionsNoFastThinLines)); // no shortcut, no cache due to size, visible part is drawn

            // RoundedRectangle
            Assert.Throws<OverflowException>(() => bmp.DrawRoundedRectangle(color, 0, 0, 100, 50, Single.NaN));
            Assert.Throws<OverflowException>(() => bmp.DrawRoundedRectangle(color, 0, 0, 100, 50, Single.NaN, Single.NaN, Single.NaN, Single.NaN));

            // FillRectangle
            bmp.Clear(backColor);
            Assert.DoesNotThrow(() => bmp.FillRectangle(color, 1, 1, Int32.MaxValue, Int32.MaxValue)); // shortcut
            Assert.Throws<OverflowException>(() => bmp.FillRectangle(color, 1.5f, 1.5f, Int32.MaxValue, Int32.MaxValue)); // no shortcut, no cache
            bmp.Clear(backColor);
            Assert.DoesNotThrow(() => bmp.FillRectangle(color, 1.5f, 1.5f, Int32.MaxValue - 127, Int32.MaxValue - 127)); // no shortcut, no cache
            bmp.Clear(backColor);
            Assert.DoesNotThrow(() => bmp.FillPath(color, new Path().AddRectangle(1, 1, Int32.MaxValue - 127, Int32.MaxValue - 127))); // no shortcut, no cache due to size, visible part is drawn

            // DrawEllipse
            bmp.Clear(backColor);
            Assert.DoesNotThrow(() => bmp.DrawEllipse(color, Int32.MinValue, Int32.MinValue, Int32.MaxValue, Int32.MaxValue)); // shortcut, not visible
            Assert.DoesNotThrow(() => bmp.DrawEllipse(color, 0, 0, 916_396, 916_395)); // shortcut, not visible - the largest possible ellipse by Bresenham without overflow (but due to performance this size is drawn from lines)
            Assert.DoesNotThrow(() => bmp.DrawEllipse(color, 0, 0, Int32.MaxValue - 127, Int32.MaxValue - 127)); // shortcut, not visible - the largest possible ellipse by Bresenham without overflow (but due to performance this size is drawn from lines)
            Assert.DoesNotThrow(() => bmp.DrawEllipse(color, 0, 5, Int32.MaxValue - 127, 0)); // shortcut, visible part is drawn - would be very slow without switching to lines drawing
            Assert.DoesNotThrow(() => bmp.DrawEllipse(color, 5, 0, 0, Int32.MaxValue - 127)); // shortcut, visible part is drawn - would be very slow without switching to lines drawing
            bmp.Clear(backColor);
            Assert.DoesNotThrow(() => bmp.DrawPath(color, new Path().AddEllipse(0, 0, 916_396, 916_395))); // shortcut, not visible - the largest possible ellipse. Between Int32.MaxValue / [2343..2344], though it's above the threshold so drawn as Béziers
            Assert.DoesNotThrow(() => bmp.DrawPath(color, new Path().AddEllipse(0, 5, Int32.MaxValue - 127, 0))); // shortcut, visible part is drawn - drawn as flattened Béziers
            Assert.DoesNotThrow(() => bmp.DrawPath(color, new Path().AddEllipse(5, 0, 0, Int32.MaxValue - 127))); // shortcut, visible part is drawn - drawn as flattened Béziers
            bmp.Clear(backColor);
            Assert.DoesNotThrow(() => bmp.DrawPath(color, new Path().AddEllipse(0, 5, UInt16.MaxValue, 0))); // shortcut, visible part is drawn - the largest width drawn as Ellipse
            Assert.DoesNotThrow(() => bmp.DrawPath(color, new Path().AddEllipse(5, 0, 0, UInt16.MaxValue))); // shortcut, visible part is drawn - the largest height drawn as Ellipse
            bmp.Clear(backColor);
            Assert.DoesNotThrow(() => bmp.DrawPath(color, new Path().AddEllipse(1, 1, Int32.MaxValue - 127, 10), optionsNoFastThinLines)); // no shortcut, no cache due to size, visible part is drawn
            Assert.Throws<OverflowException>(() => bmp.DrawEllipse(color, Int32.MinValue, Int32.MinValue, Int32.MaxValue, Int32.MaxValue, optionsNoFastThinLines)); // no shortcut, no cache
            Assert.Throws<OverflowException>(() => bmp.DrawEllipse(color, 0, 10, Int32.MaxValue, 10, optionsNoFastThinLines)); // no shortcut, no cache
            bmp.Clear(backColor);
            Assert.DoesNotThrow(() => bmp.DrawEllipse(color, 0, 0, 916_396, 916_395, optionsNoFastThinLines)); // no shortcut, no cache
            Assert.DoesNotThrow(() => bmp.DrawEllipse(color, 0, 5, Int32.MaxValue / 2, 0, optionsNoFastThinLines)); // no shortcut, no cache
            Assert.DoesNotThrow(() => bmp.DrawEllipse(color, 5, 0, 1, Int32.MaxValue / 2, optionsNoFastThinLines)); // no shortcut, no cache

            // DrawArc
            bmp.Clear(backColor);
            Assert.DoesNotThrow(() => bmp.DrawArc(color, Int32.MinValue, Int32.MinValue, Int32.MaxValue, Int32.MaxValue, 0, 270)); // shortcut, not visible
            Assert.Throws<OverflowException>(() => bmp.DrawArc(color, Int32.MinValue, Int32.MinValue, Int32.MaxValue, Int32.MaxValue, 0, 270, optionsNoFastThinLines)); // no shortcut, no cache
            Assert.Throws<OverflowException>(() => bmp.DrawArc(color, 1, 1, 10, 10, Single.NaN, 270)); // shortcut
            Assert.Throws<OverflowException>(() => bmp.DrawArc(color, 1, 1, 10, 10, 0, Single.NaN)); // shortcut
            Assert.Throws<OverflowException>(() => bmp.DrawArc(color, 1, 1, 10, 10, Single.PositiveInfinity, 90)); // shortcut
            Assert.DoesNotThrow(() => bmp.DrawArc(color, 1, 1, 10, 10, 90, Single.PositiveInfinity)); // shortcut
            bmp.Clear(backColor);
            Assert.DoesNotThrow(() => bmp.DrawArc(color, 0, 0, 916_396, 916_395, 0, 270)); // shortcut, not visible - the largest possible ellipse. Between Int32.MaxValue / [2343..2344]
            Assert.DoesNotThrow(() => bmp.DrawArc(color, 0, 0, Int32.MaxValue - 127, Int32.MaxValue - 127, 0, 270)); // shortcut, not visible
            Assert.DoesNotThrow(() => bmp.DrawArc(color, 0, 5, Int32.MaxValue - 127, 0, 0, 270)); // shortcut, visible part is drawn
            Assert.DoesNotThrow(() => bmp.DrawArc(color, 5, 0, 1, Int32.MaxValue - 127, 0, 270)); // shortcut, vertical flat arc is not visible due to the way angles are transformed
            bmp.Clear(backColor);
            Assert.DoesNotThrow(() => bmp.DrawPath(color, new Path().AddArc(0, 0, 916_396, 916_395, 0, 270))); // shortcut, not visible - the largest possible ellipse. Between Int32.MaxValue / [2343..2344]
            Assert.DoesNotThrow(() => bmp.DrawPath(color, new Path().AddArc(0, 5, Int32.MaxValue >> 1, 0, 0, 270))); // shortcut, visible part is drawn - it would throw if it called DrawArc with the rounded float bounds
            Assert.DoesNotThrow(() => bmp.DrawPath(color, new Path().AddArc(1, 1, Int32.MaxValue - 127, 10, 90, 90), optionsNoFastThinLines)); // no shortcut, no cache due to size, visible part is drawn
            bmp.Clear(backColor);
            Assert.DoesNotThrow(() => bmp.DrawArc(color, 0, 10, Int32.MaxValue, 10, 90, 90)); // shortcut, visible part is drawn. Does not throw because of the angles

            // Mandatory region for two-pass processing
            Assert.DoesNotThrow(() => bmp.FillPath(color, new Path().AddRectangle(1, 1, Int32.MaxValue - 127, Int32.MaxValue - 127), optionsTwoPassQuantizer)); // no shortcut, no cache due to size, visible part is drawn
            bmp.Clear(backColor);
            Assert.DoesNotThrow(() => bmp.DrawPath(color, new Path().AddRectangle(1, 1, Int32.MaxValue - 127, Int32.MaxValue - 127), optionsTwoPassQuantizer)); // no shortcut, no cache due to size, visible part is drawn
            bmp.Clear(backColor);
            Assert.DoesNotThrow(() => bmp.DrawRectangle(color, 1, 1, Int32.MaxValue - 127, Int32.MaxValue - 127, optionsTwoPassQuantizer)); // no shortcut, no cache, visible part is drawn
            bmp.Clear(backColor);
            Assert.DoesNotThrow(() => bmp.DrawLine(color, -1, -1, Int32.MaxValue - 127, Int32.MaxValue - 127, optionsTwoPassQuantizer)); // no shortcut, no cache, visible part is drawn
            Assert.DoesNotThrow(() => bmp.DrawEllipse(color, 1, 10, Int32.MaxValue - 127, 10, optionsTwoPassQuantizer)); // no shortcut, no cache, visible part is drawn
            Assert.DoesNotThrow(() => bmp.DrawArc(color, 0, 20, Int32.MaxValue, 10, 90, 90, optionsTwoPassQuantizer)); // no shortcut, no cache, visible part is drawn
            bmp.Clear(backColor);
            Assert.DoesNotThrow(() => bmp.DrawPath(color, new Path().AddLine(-1, -1, Int32.MaxValue - 127, Int32.MaxValue - 127), optionsTwoPassQuantizer)); // no shortcut, no cache due to size, visible part is drawn
            Assert.DoesNotThrow(() => bmp.DrawPath(color, new Path().AddEllipse(1, 10, Int32.MaxValue - 127, 10), optionsTwoPassQuantizer)); // no shortcut, no cache due to size, visible part is drawn
            Assert.DoesNotThrow(() => bmp.DrawPath(color, new Path().AddEllipse(1, 10, Int32.MaxValue - 127, 10), optionsTwoPassQuantizer)); // no shortcut, no cache due to size, visible part is drawn
            Assert.DoesNotThrow(() => bmp.DrawPath(color, new Path().AddArc(1, 20, Int32.MaxValue - 127, 10, 90, 90), optionsTwoPassQuantizer)); // no shortcut, no cache due to size, visible part is drawn
        }

        [TestCase(KnownPixelFormat.Format8bppIndexed)]
        [TestCase(KnownPixelFormat.Format32bppArgb)]
        [TestCase(KnownPixelFormat.Format32bppPArgb)]
        [TestCase(KnownPixelFormat.Format64bppArgb)]
        [TestCase(KnownPixelFormat.Format64bppPArgb)]
        [TestCase(KnownPixelFormat.Format128bppRgba)]
        [TestCase(KnownPixelFormat.Format128bppPRgba)]
        public void DrawThinPathFormatsTest(KnownPixelFormat pixelFormat)
        {
            var path = new Path(false)
                .AddPolygon(new(50, 0), new(79, 90), new(2, 35), new(97, 35), new(21, 90))
                .AddEllipse(new RectangleF(0, 0, 100, 100))
                .AddRoundedRectangle(new RectangleF(0, 0, 100, 100), 10);
            var bounds = path.RawPath.DrawOutlineBounds;
            Size size = bounds.Size + new Size(bounds.Location) + new Size(Math.Abs(bounds.X), Math.Abs(bounds.Y));
            IAsyncContext context = new SimpleContext(-1);

            using var texture = BitmapDataFactory.CreateBitmapData(2, 2, KnownPixelFormat.Format8bppGrayScale);
            texture.SetPixel(0, 0, Color.Black);
            texture.SetPixel(1, 0, Color.Gray);
            texture.SetPixel(0, 1, Color.Silver);
            texture.SetPixel(1, 1, Color.White);

            Pen[] pens = [new Pen(Color.White), new Pen(Brush.CreateTexture(texture))];
            DrawingOptions?[] options = pixelFormat.IsIndexed()
                ? [null, new DrawingOptions { Quantizer = PredefinedColorsQuantizer.SystemDefault8BppPalette() }, new DrawingOptions { Ditherer = OrderedDitherer.Bayer8x8 }, new DrawingOptions { Ditherer = ErrorDiffusionDitherer.FloydSteinberg }]
                : [null];

            foreach (Pen pen in pens)
            foreach (DrawingOptions? option in options)
            {
                using IReadWriteBitmapData bitmapDataKnown = BitmapDataFactory.CreateBitmapData(size, pixelFormat);
                bitmapDataKnown.Clear(Color.Black);
                bitmapDataKnown.DrawPath(context, pen, path, option);

                // Ignoring actual pixel format width: encoding everything on 1 byte with every format. We just want to cover all the IBitmapDataAccessor implementations.
                // Stride and buffer size is still calculated correctly because the factory method validates it.
                int stride = pixelFormat.GetByteWidth(size.Width);
                var buffer = new byte[stride * size.Height];
                using IReadWriteBitmapData bitmapDataCustom = pixelFormat.IsIndexed()
                    ? BitmapDataFactory.CreateBitmapData(buffer, size, stride, new PixelFormatInfo(pixelFormat), (row, x) => row[x], (row, x, i) => row[x] = (byte)i, bitmapDataKnown.Palette)
                    : BitmapDataFactory.CreateBitmapData(buffer, size, stride, new PixelFormatInfo(pixelFormat), (row, x) => Color32.FromGray(row[x]), (row, x, c) => row[x] = c.GetBrightness());
                bitmapDataCustom.Clear(Color.Black);
                bitmapDataCustom.DrawPath(context, pen, path, option);

                AssertAreEqual(bitmapDataKnown, bitmapDataCustom);
            }
        }

        [Test]
        public void GetPointsTest()
        {
            var points = new PointF[] { new(50, 0), new(79, 90), new(2, 35), new(97, 35), new(21, 90) };

            var path1 = new Path().AddLines(points); // open
            IList<PointF[]> points1 = path1.GetPoints();
            Assert.AreEqual(1, points1.Count);
            Assert.AreEqual(points.Length, points1[0].Length);

            var path2 = new Path().AddPolygon(points); // closed
            IList<PointF[]> points2 = path2.GetPoints();
            Assert.AreEqual(1, points2.Count);
            Assert.AreEqual(points.Length + 1, points2[0].Length);
        }

        [Test]
        public void MiterTest()
        {
            using var bmp = BitmapDataFactory.CreateBitmapData(5050, 100);
            bmp.Clear(Color.Cyan);

            var options = new DrawingOptions { AntiAliasing = true, DrawPathPixelOffset = PixelOffset.Half };
            bmp.DrawLine(Color.Blue, 0, 5, bmp.Width, 5, options);
            var pen = new Pen(Color.Blue/*, 0.01f*/) { LineJoin = LineJoinStyle.Miter, MiterLimit = 10f };

            for (int x = 0, i = 1; i < 100; i++, x += i)
                bmp.DrawLines(pen, new PointF[] {new(x, bmp.Height), new(x + i / 2f, 5), new(x + i, bmp.Height)}, options);

            SaveBitmapData(null, bmp);

            // No mitered edges exceed beyond the top line if the line width is 1, even with a high miter limit. This keeps consistency with the fast thin lines rendering.
            AssertAreEqual(bmp.Clip(new Rectangle(0, 0, bmp.Width, 5)), new SolidBitmapData(new Size(bmp.Width, 5), bmp.GetColor32(0, 0)));
        }

        [TestCase(false, true)]
        [TestCase(false, false)]
        [TestCase(true, false)]
        public void FlatArcsTest(bool antiAliasing, bool fastThinLines)
        {
            Queue<float> diameters = new([100f, 50f, 20f, 10f, 5f, 2f, 1f, 0.5f, 0.1f, 1e-6f, 0f]);
            var options = new DrawingOptions
            {
                AntiAliasing = antiAliasing,
                DrawPathPixelOffset = fastThinLines ? PixelOffset.None : PixelOffset.Half,
                FastThinLines = fastThinLines,
                //Quantizer = OptimizedPaletteQuantizer.Wu() // to test drawing thin path into region
            };
            using var bmp = BitmapDataFactory.CreateBitmapData(500, 500);
            using var ms = new MemoryStream();
            using var texture = BitmapDataFactory.CreateBitmapData(2, 2, KnownPixelFormat.Format8bppGrayScale);
            texture.SetPixel(0, 0, Color.Blue);
            texture.SetPixel(1, 0, Color.DarkCyan);
            texture.SetPixel(0, 1, Color.Teal);
            texture.SetPixel(1, 1, Color.DarkBlue);
            GifEncoder.EncodeAnimation(new AnimatedGifConfiguration(GetNextFrame, () => TimeSpan.FromMilliseconds(500)), ms);

            // No assert, must be observed visually
            SaveStream($"{(antiAliasing ? "AA" : "NA")}{(fastThinLines ? "_Thin" : null)}", ms);

            #region Local Methods
            
            [SuppressMessage("ReSharper", "AccessToDisposedClosure", Justification = "False alarm, not used after disposed")]
            IReadableBitmapData? GetNextFrame()
            {
                if (diameters.Count == 0)
                    return null;
                float diameter = diameters.Dequeue();
                float radius = diameter / 2f;
                float sweepAngle = 300f;
                var path = new Path()
                    .TransformTranslation(13, 13)
                    .AddPoint(0, 0)
                    .AddArc(new RectangleF(0, 50 - radius, 100, diameter), -45, sweepAngle)
                    .AddPoint(100, 100)
                    .StartFigure().ResetTransformation().TransformTranslation(125 + 13, 13)
                    .AddPoint(0, 0)
                    .AddArc(new RectangleF(50 - radius, 0, diameter, 100), -45, sweepAngle)
                    .AddPoint(100, 100)
                    .StartFigure().ResetTransformation().TransformTranslation(13, 125 + 13)
                    .AddPoint(0, 0)
                    .AddArc(new RectangleF(0, 50 - radius, 100, diameter), 0, sweepAngle)
                    .AddPoint(100, 100)
                    .StartFigure().ResetTransformation().TransformTranslation(125 + 13, 125 + 13)
                    .AddPoint(0, 0)
                    .AddArc(new RectangleF(50 - radius, 0, diameter, 100), 0, sweepAngle)
                    .AddPoint(100, 100)
                    .StartFigure().ResetTransformation().TransformTranslation(13, 250 + 13)
                    .AddPoint(0, 0)
                    .AddArc(new RectangleF(0, 50 - radius, 100, diameter), 45, sweepAngle)
                    .AddPoint(100, 100)
                    .StartFigure().ResetTransformation().TransformTranslation(125 + 13, 250 + 13)
                    .AddPoint(0, 0)
                    .AddArc(new RectangleF(50 - radius, 0, diameter, 100), 45, sweepAngle)
                    .AddPoint(100, 100)
                    .StartFigure().ResetTransformation().TransformTranslation(13, 375 + 13)
                    .AddPoint(0, 0)
                    .AddArc(new RectangleF(0, 50 - radius, 100, diameter), 90, sweepAngle)
                    .AddPoint(100, 100)
                    .StartFigure().ResetTransformation().TransformTranslation(125 + 13, 375 + 13)
                    .AddPoint(0, 0)
                    .AddArc(new RectangleF(50 - radius, 0, diameter, 100), 90, sweepAngle)
                    .AddPoint(100, 100)

                    .StartFigure().ResetTransformation().TransformTranslation(250 + 13, 13)
                    .AddPoint(0, 0)
                    .AddArc(new RectangleF(0, 50 - radius, 100, diameter), -45 + 180, sweepAngle)
                    .AddPoint(100, 100)
                    .StartFigure().ResetTransformation().TransformTranslation(250 + 125 + 13, 13)
                    .AddPoint(0, 0)
                    .AddArc(new RectangleF(50 - radius, 0, diameter, 100), -45 + 180, sweepAngle)
                    .AddPoint(100, 100)
                    .StartFigure().ResetTransformation().TransformTranslation(250 + 13, 125 + 13)
                    .AddPoint(0, 0)
                    .AddArc(new RectangleF(0, 50 - radius, 100, diameter), 180, sweepAngle)
                    .AddPoint(100, 100)
                    .StartFigure().ResetTransformation().TransformTranslation(250 + 125 + 13, 125 + 13)
                    .AddPoint(0, 0)
                    .AddArc(new RectangleF(50 - radius, 0, diameter, 100), 180, sweepAngle)
                    .AddPoint(100, 100)
                    .StartFigure().ResetTransformation().TransformTranslation(250 + 13, 250 + 13)
                    .AddPoint(0, 0)
                    .AddArc(new RectangleF(0, 50 - radius, 100, diameter), 45 + 180, sweepAngle)
                    .AddPoint(100, 100)
                    .StartFigure().ResetTransformation().TransformTranslation(250 + 125 + 13, 250 + 13)
                    .AddPoint(0, 0)
                    .AddArc(new RectangleF(50 - radius, 0, diameter, 100), 45 + 180, sweepAngle)
                    .AddPoint(100, 100)
                    .StartFigure().ResetTransformation().TransformTranslation(250 + 13, 375 + 13)
                    .AddPoint(0, 0)
                    .AddArc(new RectangleF(0, 50 - radius, 100, diameter), 90 + 180, sweepAngle)
                    .AddPoint(100, 100)
                    .StartFigure().ResetTransformation().TransformTranslation(250 + 125 + 13, 375 + 13)
                    .AddPoint(0, 0)
                    .AddArc(new RectangleF(50 - radius, 0, diameter, 100), 90 + 180, sweepAngle)
                    .AddPoint(100, 100)
                    ;
                bmp.Clear(Color.Cyan);
                //bmp.FillPath(Color.Yellow, path, options);
                bmp.DrawPath(Color.Blue, path, options);
                //bmp.DrawPath(new Pen(Brush.CreateTexture(texture)), path, options); // to test TextureBrush.DrawArc
                return bmp;
            }

            #endregion
        }

        #endregion
    }
}
