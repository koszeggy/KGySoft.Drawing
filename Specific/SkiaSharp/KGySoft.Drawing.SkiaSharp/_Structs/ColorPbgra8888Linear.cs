#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ColorPbgra8888Linear.cs
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

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using KGySoft.Drawing.Imaging;

#endregion

#region Suppressions

#if NET8_0_OR_GREATER
#pragma warning disable CS9193 // Argument should be a variable because it is passed to a 'ref readonly' parameter - false alarm
#pragma warning disable CS9195 // Argument should be passed with the 'in' keyword - false alarm
#endif

#endregion

namespace KGySoft.Drawing.SkiaSharp
{
    [StructLayout(LayoutKind.Explicit)]
    internal readonly struct ColorPbgra8888Linear
    {
        #region Fields

        [FieldOffset(0)]private readonly byte b;
        [FieldOffset(1)]private readonly byte g;
        [FieldOffset(2)]private readonly byte r;
        [FieldOffset(3)]private readonly byte a;

        #endregion

        #region Constructors
        
        internal ColorPbgra8888Linear(PColorF c) => this = Unsafe.As<PColor32, ColorPbgra8888Linear>(ref Unsafe.AsRef(c.ToPColor32(false)));

        #endregion

        #region Methods

        internal Color32 ToColor32()
        {
            Color32 linear32 = new PColor32(a, r, g, b).ToStraight();
            return new Color32(a, linear32.R.ToSrgb(), linear32.G.ToSrgb(), linear32.B.ToSrgb());
        }

        internal Color64 ToColor64()
        {
            Color64 linear64 = new PColor32(a, r, g, b).ToPColor64().ToStraight();
            return new Color64(ColorSpaceHelper.ToUInt16(a), linear64.R.ToSrgb(), linear64.G.ToSrgb(), linear64.B.ToSrgb());
        }

        internal PColorF ToPColorF() => Unsafe.As<ColorPbgra8888Linear, PColor32>(ref Unsafe.AsRef(this)).ToPColorF(false);

        #endregion
    }
}
