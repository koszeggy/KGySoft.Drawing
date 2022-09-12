#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: QuantizerViewModel.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2022 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

#nullable enable

#region Usings

using System;
using System.Linq;
using System.Reflection;

using KGySoft.Drawing.Imaging;
using KGySoft.Drawing.Uwp;
using KGySoft.Reflection;

#endregion

namespace KGySoft.Drawing.Examples.Uwp.ViewModel
{
    internal class QuantizerViewModel
    {
        #region Fields

        #region Static Fields

        internal static readonly QuantizerViewModel[] Quantizers =
        {
            new("Black & White", typeof(PredefinedColorsQuantizer), nameof(PredefinedColorsQuantizer.BlackAndWhite)),
            new("Grayscale (4 shades)", typeof(PredefinedColorsQuantizer), nameof(PredefinedColorsQuantizer.Grayscale4)),
            new("Grayscale (16 shades)", typeof(PredefinedColorsQuantizer), nameof(PredefinedColorsQuantizer.Grayscale16)),
            new("Grayscale (256 shades)", typeof(PredefinedColorsQuantizer), nameof(PredefinedColorsQuantizer.Grayscale)),
            new("System default 4 bpp palette (16 colors)", typeof(PredefinedColorsQuantizer), nameof(PredefinedColorsQuantizer.SystemDefault4BppPalette)),
            new("System default 8 bpp palette (256 colors)", typeof(PredefinedColorsQuantizer), nameof(PredefinedColorsQuantizer.SystemDefault8BppPalette)),
            new("RGB332 palette (256 colors)", typeof(PredefinedColorsQuantizer), nameof(PredefinedColorsQuantizer.Rgb332)),
            new("ARGB1555 color space (32K colors)", typeof(PredefinedColorsQuantizer), nameof(PredefinedColorsQuantizer.Argb1555)),
            new("RGB565 color space (64K colors)", typeof(PredefinedColorsQuantizer), nameof(PredefinedColorsQuantizer.Rgb565)),

            new("Optimized palette (Octree algorithm)", typeof(OptimizedPaletteQuantizer), nameof(OptimizedPaletteQuantizer.Octree)),
            new("Optimized palette (Median Cut algorithm)", typeof(OptimizedPaletteQuantizer), nameof(OptimizedPaletteQuantizer.MedianCut)),
            new("Optimized palette (Wu's algorithm)", typeof(OptimizedPaletteQuantizer), nameof(OptimizedPaletteQuantizer.Wu)),
        };

        #endregion

        #region Instance Fields

        private readonly string displayName;
        private readonly MethodAccessor method;
        private readonly ParameterInfo[] parameters;

        #endregion

        #endregion

        #region Properties

        internal bool HasAlphaThreshold { get; }
        internal bool HasWhiteThreshold { get; }
        internal bool HasDirectMapping { get; }
        internal bool HasBitLevel { get; }
        internal bool HasMaxColors { get; }

        #endregion

        #region Constructors

        private QuantizerViewModel(string name, Type type, string methodName)
        {
            displayName = name;
            MethodInfo mi = type.GetMethod(methodName)!;
            method = MethodAccessor.GetAccessor(mi);
            parameters = mi.GetParameters();
            HasAlphaThreshold = parameters.Any(p => p.Name == "alphaThreshold");
            HasWhiteThreshold = parameters.Any(p => p.Name == "whiteThreshold");
            HasDirectMapping = parameters.Any(p => p.Name == "directMapping");
            HasMaxColors = parameters.Any(p => p.Name == "maxColors");
            HasBitLevel = mi.DeclaringType == typeof(OptimizedPaletteQuantizer);
        }

        #endregion

        #region Methods

        #region Public Methods

        public override string ToString() => displayName;

        #endregion

        #region Internal Methods

        internal IQuantizer Create(MainViewModel settings)
        {
            object[] args = new object[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                args[i] = parameters[i].Name switch
                {
                    "backColor" => settings.BackColor.ToDrawingColor(),
                    "alphaThreshold" => (byte)settings.AlphaThreshold,
                    "whiteThreshold" => (byte)settings.WhiteThreshold,
                    "directMapping" => false,
                    "maxColors" => settings.PaletteSize,
                    _ => throw new InvalidOperationException($"Unhandled parameter: {parameters[i].Name}")
                };
            }

            IQuantizer result = (IQuantizer)method.Invoke(null, args)!;
            return result;
        }

        #endregion

        #endregion
    }
}
