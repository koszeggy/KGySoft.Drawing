#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: PathTest.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2024 - All Rights Reserved
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
using System.Drawing;
using System.Numerics;

using KGySoft.Diagnostics;
using KGySoft.Drawing.Imaging;
using KGySoft.Drawing.Shapes;
using KGySoft.Threading;

using NUnit.Framework;

#endregion

#region Used Aliases

using Brush = KGySoft.Drawing.Shapes.Brush;
using Pen = KGySoft.Drawing.Shapes.Pen;
using SolidBrush = KGySoft.Drawing.Shapes.SolidBrush;

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

            ["32bppArgb_Alternate_NQ_Srgb_NA_NB_Rotated", KnownPixelFormat.Format32bppArgb, WorkingColorSpace.Srgb, Color.Cyan, Color.Blue, new DrawingOptions { FillMode = ShapeFillMode.Alternate, AlphaBlending = false, AntiAliasing = false, Transformation = new TransformationMatrix(Matrix3x2.CreateRotation(13, new(100, 100))) } ],
            ["32bppArgb_Alternate_NQ_Srgb_AA_AB_A128_Rotated", KnownPixelFormat.Format32bppArgb, WorkingColorSpace.Srgb, Color.Empty, Color.FromArgb(128, Color.Blue), new DrawingOptions { FillMode = ShapeFillMode.Alternate, AlphaBlending = true, AntiAliasing = true, Transformation = new TransformationMatrix(Matrix3x2.CreateRotation(13, new(100, 100))) } ],
        ];

        private static object?[][] ScanPixelOffsetTestSource =>
        [
            // string name, Path path
            ["Rectangle", new Path().AddRectangle(new RectangleF(1, 1, 10, 5))],
            ["AlmostRectangle", new Path().AddLines(new(2, 1), new(23, 2), new(24, 8), new(1, 9))],
            ["Star", new Path().AddLines(new(50, 0), new(90, 100), new(0, 40), new(100, 40), new(10, 100))],
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
            ["LineLong10", new Path().AddLine(new PointF(1, 1), new PointF(13, 2)), 10f],
            ["Joints10", new Path().AddLines(new(0, 100), new(50, 20), new(100, 100)), 10f],
            ["TetragonOpen", new Path().AddLines(new PointF(1, 1), new PointF(40, 1), new PointF(100, 50), new PointF(0, 50)), 10f],
            ["SelfCrossingStarOpen_01", new Path().AddLines(new(51, 1), new(81, 91), new(3, 36), new(99, 36), new(22, 91)), 1f],
            ["SelfCrossingStarOpen_10", new Path().AddLines(new(60, 10), new(90, 100), new(12, 45), new(108, 45), new(31, 100)), 10f],
            ["ArcHalfEllipse01", new Path().AddArc(new RectangleF(1, 1, 98, 48), 0, 180), 1f],
            ["ArcHalfEllipse10", new Path().AddArc(new RectangleF(10, 10, 100, 50), 0, 180), 10f],
            // TODO: Bezier, Ellipse, Rectangle, Arc, RoundedRectangle, MoreFigures (eg. circle+star)
        ];

        private static object?[][] DrawClosedPathTestSource =>
        [
            // string name, Path path, float width
            ["TetragonClose", new Path().AddLines(new PointF(1, 1), new PointF(40, 1), new PointF(100, 50), new PointF(0, 50)).CloseFigure(), 10f],
            ["SelfCrossingStarClose", new Path().AddLines(new(51, 1), new(81, 91), new(3, 36), new(99, 36), new(22, 91)).CloseFigure()],
            ["Ellipse", new Path().AddEllipse(new RectangleF(2, 2, 95, 45))],
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
            ["Rectangle", new Path().AddRectangle(new RectangleF(2, 2, 95, 45))],
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
            //path.AddLines(new(300, 200), new(260, 300), new(350, 240), new(250, 240), new(340, 300), new(300, 200));
            //path.AddLines(new(300, 200), new(260, 300), new(350, 240), new(250, 240), new(340, 300));
            //path.CloseFigure(); // combine with the following to mix two closed figures - note: causes holes even with Alternate mode, but the same happens for GDI+, too

            // Multiple stars with all possible edge relations (to test EdgeInfo.ConfigureEdgeRelation)
            path.AddLines(new(300, 300), new(260, 200), new(350, 260), new(250, 260), new(340, 200));
            path.CloseFigure();
            path.AddLines(new(50, 50), new(90, 150), new(0, 90), new(100, 90), new(10, 150));
            path.CloseFigure();
            path.AddLines(new(300, 50), new(260, 150), new(350, 90), new(250, 90), new(340, 150));
            path.CloseFigure();
            path.AddLines(new(50, 300), new(90, 200), new(0, 260), new(100, 260), new(10, 200));

            // shapes with an almost horizontal top line
            //path.AddLines(new(1, 1), new(260, 2), new(260, 15)/*, new(1, 15)*/);
            //path.CloseFigure();
            //path.AddLines(new(1, 16), new(260, 17), new(260, 30), new(1, 30));

            // A rectangle implicitly closes the previous figure
            //path.AddLines(new(1, 1), new(25, 2), new(25, 15)/*, new(1, 15)*/);
            //path.AddRectangle(new RectangleF(1, 16, 24, 13));

            //path.AddRectangle(new Rectangle(0, 0, 1, 1));
            //path.AddRectangle(new Rectangle(1, 1, 1, 1));

            // beziers
            //path.AddBeziers(new(10, 10), new(42, 10), new(42, 42), new(10, 42));

            //path.AddArc(new RectangleF(1, 1, 98, 48), 0, 180);
            //path.AddEllipse(new RectangleF(1, 1, 1920, 1440));
            //path.AddEllipse(new RectangleF(1, 1, 98, 48));
            //path.AddEllipse(new RectangleF(1, 1, 3, 3));

            using var bitmapDataBackground = BitmapDataFactory.CreateBitmapData(path.Bounds.Size + new Size(path.Bounds.Location) + new Size(Math.Abs(path.Bounds.X), Math.Abs(path.Bounds.Y) /*path.Bounds.Location*/), pixelFormat, colorSpace);
            if (backColor != Color.Empty)
                bitmapDataBackground.Clear(backColor, options.Ditherer);
            else
                GenerateAlphaGradient(bitmapDataBackground);

            IAsyncContext context = new SimpleContext(-1);

            // non-cached region
            using var bitmapData1 = bitmapDataBackground.Clone();
            bitmapData1.FillPath(context, path, Brush.CreateSolid(fillColor), options);
            SaveBitmapData(name, bitmapData1);

            // generating cached region
            path.PreferCaching = true;
            using var bitmapData2 = bitmapDataBackground.Clone();
            bitmapData2.FillPath(context, path, Brush.CreateSolid(fillColor), options);
            AssertAreEqual(bitmapData1, bitmapData2);

            // re-using region from cache
            using var bitmapData3 = bitmapDataBackground.Clone();
            bitmapData3.FillPath(context, path, Brush.CreateSolid(fillColor), options);
            AssertAreEqual(bitmapData1, bitmapData3);

            //using var bitmapData = bitmapDataBackground.Clone();
            //new PerformanceTest { TestName = $"{name} - {path.Bounds.Size}", TestTime = 5000, /*Iterations = 10_000,*/ Repeat = 3 }
            //    .AddCase(() =>
            //    {
            //        //bitmapDataBackground.CopyTo(bitmapData);
            //        bitmapData.FillPath(null, path, Brush.CreateSolid(fillColor), options, false);
            //    }, "NoCache")
            //    .AddCase(() =>
            //    {
            //        //bitmapDataBackground.CopyTo(bitmapData);
            //        bitmapData.FillPath(null, path, Brush.CreateSolid(fillColor), options, true);
            //    }, "Cache")
            //    //.AddCase(() =>
            //    //{
            //    //    //bitmapData.Clear(default);
            //    //    bitmapData.FillPath(AsyncHelper.DefaultContext, path, new SolidBrush(Color.Blue), options);
            //    //}, "MultiThread")
            //    .DoTest()
            //    .DumpResults(Console.Out);
        }

        [TestCaseSource(nameof(FillPathTestSource))]
        public void ClippedFillPathTest(string name, KnownPixelFormat pixelFormat, WorkingColorSpace colorSpace, Color backColor, Color fillColor, DrawingOptions options)
        {
            var offset = new SizeF(-10, -10);
            var path = new Path(false);
            path.AddLines(new PointF(50, 0) + offset, new PointF(90, 100) + offset, new PointF(0, 40) + offset, new PointF(100, 40) + offset, new PointF(10, 100) + offset);

            using var bitmapDataBackground = BitmapDataFactory.CreateBitmapData((path.Bounds.Size + offset * 2).ToSize(), pixelFormat, colorSpace);
            if (backColor != Color.Empty)
                bitmapDataBackground.Clear(backColor, options.Ditherer);
            else
                GenerateAlphaGradient(bitmapDataBackground);

            //var singleThreadContext = new SimpleContext(1);

            // non-cached region
            using var bitmapData1 = bitmapDataBackground.Clone();
            bitmapData1.FillPath(null, path, Brush.CreateSolid(fillColor), options);
            SaveBitmapData(name, bitmapData1);

            // generating cached region
            path.PreferCaching = true;
            using var bitmapData2 = bitmapDataBackground.Clone();
            bitmapData2.FillPath(null, path, Brush.CreateSolid(fillColor), options);
            AssertAreEqual(bitmapData1, bitmapData2);

            // re-using region from cache
            using var bitmapData3 = bitmapDataBackground.Clone();
            bitmapData3.FillPath(null, path, Brush.CreateSolid(fillColor), options);
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
                path = Path.Transform(path, TransformationMatrix.CreateTranslation(MathF.Floor(width / 2f), MathF.Floor(width / 2f)));

            using var bitmapData = BitmapDataFactory.CreateBitmapData(size, pixelFormat, colorSpace);
            IAsyncContext context = new SimpleContext(-1);

            foreach (bool antiAliasing in new[] { false, true })
            {
                var drawingOptions = new DrawingOptions { AntiAliasing = antiAliasing };
                LineCapStyle[] capStyles = width <= 1f ? [LineCapStyle.Flat] : [LineCapStyle.Flat, LineCapStyle.Square, LineCapStyle.Triangle, LineCapStyle.Round];
                foreach (LineCapStyle capStyle in capStyles)
                {
                    bitmapData.Clear(Color.Cyan);

                    var pen = new Pen(Color.Blue, width) { StartCap = capStyle, EndCap = capStyle };
                    bitmapData.DrawPath(context, path, pen, drawingOptions);
                    SaveBitmapData(name, bitmapData, $"{(antiAliasing ? "AA" : "NA")}_W{width:00}_{capStyle}");
                }
            }
        }

        [TestCaseSource(nameof(DrawClosedPathTestSource))]
        public void DrawClosedPathTest(string name, Path path, float width)
        {
            var pixelFormat = KnownPixelFormat.Format32bppArgb;
            var colorSpace = WorkingColorSpace.Linear;
            Size size = path.Bounds.Size + new Size(path.Bounds.Location) + new Size(Math.Abs(path.Bounds.X), Math.Abs(path.Bounds.Y));
            if (size.IsEmpty)
            {
                size = new Size(10, 10);
                path.TransformTranslation(5, 5);
            }

            using var bitmapData = BitmapDataFactory.CreateBitmapData(size, pixelFormat, colorSpace);
            IAsyncContext context = new SimpleContext(-1);

            foreach (bool antiAliasing in new[] { false, true })
            {
                var drawingOptions = new DrawingOptions { AntiAliasing = antiAliasing };
                LineJoinStyle[] joinStyles = width <= 1f ? [LineJoinStyle.Bevel] : [LineJoinStyle.Miter, LineJoinStyle.Bevel, LineJoinStyle.Round];
                foreach (LineJoinStyle joinStyle in joinStyles)
                {
                    bitmapData.Clear(Color.Cyan);

                    var pen = new Pen(Color.Blue, width) { LineJoin = joinStyle };
                    bitmapData.DrawPath(context, path, pen, drawingOptions);
                    SaveBitmapData(name, bitmapData, $"{(antiAliasing ? "AA" : "NA")}_W{width:00}_{joinStyle}");
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
            //var pen = new Pen(Color.FromArgb(128, Color.Yellow)); // This always uses region-draw but makes the layers visible
            var brush = new SolidBrush(Color.Blue);
            foreach (bool antiAliasing in new[] { false, true })
            {
                var drawingOptions = new DrawingOptions { AntiAliasing = antiAliasing };

                path = new Path(path) { PreferCaching = false };
                using var bitmapData1 = bitmapDataBackground.Clone();
                bitmapData1.FillPath(context, path, brush, drawingOptions);
                bitmapData1.DrawPath(context, path, pen, drawingOptions);
                SaveBitmapData(name, bitmapData1, $"{(antiAliasing ? "AA" : "NA")}");

                path.PreferCaching = true;
                using var bitmapData2 = bitmapDataBackground.Clone();
                bitmapData2.FillPath(context, path, brush, drawingOptions);
                bitmapData2.DrawPath(context, path, pen, drawingOptions);
                AssertAreEqual(bitmapData1, bitmapData2);

                using var bitmapData3 = bitmapDataBackground.Clone();
                bitmapData3.FillPath(context, path, brush, drawingOptions);
                bitmapData3.DrawPath(context, path, pen, drawingOptions);
                AssertAreEqual(bitmapData1, bitmapData3);
            }
        }

        [TestCaseSource(nameof(DrawThinLinesWithFormatTestSource))]
        public void DrawThinLinesWithFormatTest(string name, KnownPixelFormat pixelFormat, Color backColor, Color drawColor, DrawingOptions? options)
        {
            var path = new Path(false)
                .TransformTranslation(1, 1)
                .AddLines(new(50, 0), new(79, 90), new(2, 35), new(97, 35), new(21, 90)).CloseFigure()
                .AddEllipse(new RectangleF(0, 0, 100, 100))
                .AddRectangle(new RectangleF(0, 0, 100, 100))
                ;
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
            bitmapData1.DrawPath(context, path, pen, options);
            SaveBitmapData(name, bitmapData1);

            path.PreferCaching = true;
            using var bitmapData2 = bitmapDataBackground.Clone();
            bitmapData2.DrawPath(context, path, pen, options);
            AssertAreEqual(bitmapData1, bitmapData2);

            using var bitmapData3 = bitmapDataBackground.Clone();
            bitmapData3.DrawPath(context, path, pen, options);
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
            bitmapData1.DrawPath(context, path, pen);
            SaveBitmapData(name, bitmapData1);

            path.PreferCaching = true;
            using var bitmapData2 = bitmapDataBackground.Clone();
            bitmapData2.DrawPath(context, path, pen);
            AssertAreEqual(bitmapData1, bitmapData2);

            using var bitmapData3 = bitmapDataBackground.Clone();
            bitmapData3.DrawPath(context, path, pen);
            AssertAreEqual(bitmapData1, bitmapData3);
        }

        [TestCaseSource(nameof(ScanPixelOffsetTestSource))]
        public void ScanPixelOffsetTest(string name, Path path)
        {
            var bounds = path.RawPath.Bounds;
            Size size = bounds.Size + new Size(bounds.Location) + new Size(Math.Abs(bounds.X), Math.Abs(bounds.Y));
            Assert.IsFalse(bounds.IsEmpty());

            using var bitmapDataBackground = BitmapDataFactory.CreateBitmapData(size);
            bitmapDataBackground.Clear(Color.Cyan);
            IAsyncContext context = new SimpleContext(-1);
            var brush = new SolidBrush(Color.Blue);
            foreach (bool antiAliasing in new[] { false, true })
            foreach (PixelOffset pixelOffset in new[] { PixelOffset.None, PixelOffset.Half })
            {
                var options = new DrawingOptions { ScanPathPixelOffset = pixelOffset, AntiAliasing = antiAliasing };
                path.PreferCaching = false;
                using var bitmapData1 = bitmapDataBackground.Clone();
                bitmapData1.FillPath(context, path, brush, options);
                SaveBitmapData(name, bitmapData1, $"{(antiAliasing ? "AA" : "NA")}_{pixelOffset}");

                path.PreferCaching = true;
                using var bitmapData2 = bitmapDataBackground.Clone();
                bitmapData2.FillPath(context, path, brush, options);
                AssertAreEqual(bitmapData1, bitmapData2);

                using var bitmapData3 = bitmapDataBackground.Clone();
                bitmapData3.FillPath(context, path, brush, options);
                AssertAreEqual(bitmapData1, bitmapData3);
            }
        }

        [Explicit]
        [Test]
        public void DrawThinLinesPerfTest()
        {
            var path = new Path(false)
                    .AddLines(new(50, 0), new(79, 90), new(2, 35), new(97, 35), new(21, 90)).CloseFigure()
                    .AddEllipse(new RectangleF(0, 0, 100, 100))
                    .AddRectangle(new RectangleF(0, 0, 100, 100))
                    ;
            var bounds = path.RawPath.DrawOutlineBounds;
            Size size = bounds.Size + new Size(bounds.Location) + new Size(Math.Abs(bounds.X), Math.Abs(bounds.Y));

            using var bitmapDataBackground = BitmapDataFactory.CreateBitmapData(size);
            bitmapDataBackground.Clear(Color.Cyan);

            IAsyncContext context = new SimpleContext(-1);
            var pen = new Pen(Color.Blue);

            //DrawingOptions options1 = new DrawingOptions { TestBehavior = -1 }; // Region
            //using var bitmapData1 = bitmapDataBackground.Clone();
            //bitmapData1.DrawPath(context, path, pen, options1); // no caching
            //SaveBitmapData(null, bitmapData1);

            //DrawingOptions options2 = new DrawingOptions { TestBehavior = 0 }; // Direct SetColor32
            //using var bitmapData2 = bitmapDataBackground.Clone();
            //bitmapData2.DrawPath(context, path, pen, options2);
            //AssertAreEqual(bitmapData1, bitmapData2);

            //var options3 = new DrawingOptions { TestBehavior = 1 }; // Delegate
            //using var bitmapData3 = bitmapDataBackground.Clone();
            //bitmapData3.DrawPath(context, path, pen, options3);
            //AssertAreEqual(bitmapData1, bitmapData3);

            //var options4 = new DrawingOptions { TestBehavior = 2 }; // FuncPtr
            //using var bitmapData4 = bitmapDataBackground.Clone();
            //bitmapData4.DrawPath(context, path, pen, options4);
            //AssertAreEqual(bitmapData1, bitmapData4);

            new PerformanceTest { Repeat = 3, TestTime = 5000, TestName = $"Size: {size}; Vertices: {path.RawPath.TotalVertices}" }
                //.AddCase(() => bitmapData1.DrawPath(context, path, pen, options1), "No caching")
                //.AddCase(() => bitmapData2.DrawPath(context, path, pen, options2), "Caching")
                //.AddCase(() => bitmapData3.DrawPath(context, path, pen, options3), "Delegate")
                //.AddCase(() => bitmapData4.DrawPath(context, path, pen, options4), "Function pointer")
                .DoTest()
                .DumpResults(Console.Out);
        }

        #endregion
    }
}
