#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: QuantizerDescriptor.cs
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
using System.ComponentModel;
using System.Linq;
using System.Reflection;

using KGySoft.Drawing.Examples.Shared.Interfaces;
using KGySoft.Drawing.Imaging;

using KGySoft.Reflection;

#endregion

namespace KGySoft.Drawing.Examples.Shared.Model
{
    public class QuantizerDescriptor
    {
        #region Fields

        #region Static Fields

        internal static readonly QuantizerDescriptor[] Quantizers =
        {
            new("Black & White", typeof(PredefinedColorsQuantizer), nameof(PredefinedColorsQuantizer.BlackAndWhite), false),
            new("Grayscale (4 shades)", typeof(PredefinedColorsQuantizer), nameof(PredefinedColorsQuantizer.Grayscale4), false),
            new("Grayscale (16 shades)", typeof(PredefinedColorsQuantizer), nameof(PredefinedColorsQuantizer.Grayscale16), false),
            new("Grayscale (256 shades)", typeof(PredefinedColorsQuantizer), nameof(PredefinedColorsQuantizer.Grayscale), false),
            new("System default 4 bpp palette (16 colors)", typeof(PredefinedColorsQuantizer), nameof(PredefinedColorsQuantizer.SystemDefault4BppPalette), false),
            new("System default 8 bpp palette (256 colors)", typeof(PredefinedColorsQuantizer), nameof(PredefinedColorsQuantizer.SystemDefault8BppPalette), true),
            new("RGB332 palette (256 colors)", typeof(PredefinedColorsQuantizer), nameof(PredefinedColorsQuantizer.Rgb332), false),
            new("RGB555 color space (32K colors)", typeof(PredefinedColorsQuantizer), nameof(PredefinedColorsQuantizer.Rgb555), false),
            new("RGB565 color space (64K colors)", typeof(PredefinedColorsQuantizer), nameof(PredefinedColorsQuantizer.Rgb565), false),
            new("ARGB1555 color space (32K colors with transparency)", typeof(PredefinedColorsQuantizer), nameof(PredefinedColorsQuantizer.Argb1555), true),
            new("RGB888 color space (16.7M colors)", typeof(PredefinedColorsQuantizer), nameof(PredefinedColorsQuantizer.Rgb888), false),

            new("Optimized palette (Octree algorithm)", typeof(OptimizedPaletteQuantizer), nameof(OptimizedPaletteQuantizer.Octree), true),
            new("Optimized palette (Median Cut algorithm)", typeof(OptimizedPaletteQuantizer), nameof(OptimizedPaletteQuantizer.MedianCut), true),
            new("Optimized palette (Wu's algorithm)", typeof(OptimizedPaletteQuantizer), nameof(OptimizedPaletteQuantizer.Wu), true),
        };

        #endregion

        #region Instance Fields

        private readonly string displayName;
        private readonly MethodAccessor method;
        private readonly ParameterInfo[] parameters;

        #endregion

        #endregion

        #region Properties

        public bool HasAlphaThreshold { get; }
        public bool HasWhiteThreshold { get; }
        public bool HasDirectMapping { get; }
        public bool HasBitLevel { get; }
        public bool HasMaxColors { get; }

        #endregion

        #region Constructors

        private QuantizerDescriptor(string name, Type type, string methodName, bool hasAlpha)
        {
            displayName = name;
            MethodInfo mi = GetMethod(type, methodName);
            method = MethodAccessor.GetAccessor(mi);
            parameters = mi.GetParameters();
            HasAlphaThreshold = hasAlpha;
            HasWhiteThreshold = parameters.Any(p => p.Name == "whiteThreshold");
            HasDirectMapping = parameters.Any(p => p.Name == "directMapping");
            HasMaxColors = parameters.Any(p => p.Name == "maxColors");
            HasBitLevel = mi.DeclaringType == typeof(OptimizedPaletteQuantizer);
        }

        #endregion

        #region Methods

        #region Static Methods

        private static MethodInfo GetMethod(Type type, string methodName)
        {
            MemberInfo[] methods = type.GetMember(methodName, MemberTypes.Method, BindingFlags.Public | BindingFlags.Static);
            foreach (MemberInfo method in methods)
            {
                if (!Attribute.IsDefined(method, typeof(EditorBrowsableAttribute)))
                    return (MethodInfo)method;
            }

            throw new InvalidOperationException($"Method not found: {methodName}");
        }

        #endregion

        #region Instance Methods

        #region Public Methods

        public override string ToString() => displayName;

        #endregion

        #region Internal Methods

        internal IQuantizer Create(IQuantizerSettings settings)
        {
            object[] args = new object[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                args[i] = parameters[i].Name switch
                {
                    "backColor" => settings.BackColor,
                    // ReSharper disable once RedundantCast - false alarm, would be int without the cast, which causes an exception
                    "alphaThreshold" => HasAlphaThreshold ? settings.AlphaThreshold : (byte)0,
                    "whiteThreshold" => settings.WhiteThreshold,
                    "directMapping" => settings.DirectMapping,
                    "maxColors" => settings.PaletteSize,
                    _ => throw new InvalidOperationException($"Unhandled parameter: {parameters[i].Name}")
                };
            }

            IQuantizer result = (IQuantizer)method.Invoke(null, args)!;
            result = result switch
            {
                OptimizedPaletteQuantizer optimized => optimized.ConfigureBitLevel(settings.BitLevel).ConfigureColorSpace(settings.WorkingColorSpace),
                PredefinedColorsQuantizer predefined => predefined.ConfigureColorSpace(settings.WorkingColorSpace),
                _ => result
            };
            return result;
        }

        #endregion

        #endregion

        #endregion

    }
}
