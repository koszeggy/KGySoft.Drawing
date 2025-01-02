#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: IColor.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2025 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

namespace KGySoft.Drawing.Imaging
{
    internal interface IColor<TColor> where TColor : unmanaged, IColor<TColor>
    {
        #region Properties

        bool IsTransparent { get; }
        bool IsOpaque { get; }

        #endregion

        #region Methods

        TColor BlendSrgb(TColor backColor);
        TColor BlendLinear(TColor backColor);

        #endregion
    }

    internal interface IColor<TColor, in TBaseColor> : IColor<TColor>
        where TColor : unmanaged, IColor<TColor, TBaseColor>
    {
        #region Methods

        // If these were public it would be nicer with float, or ushort/float overloads should be added
        TColor WithAlpha(byte a, TBaseColor baseColor); // this could be a static abstract method but that's not supported on every targeted platform
        TColor AdjustAlpha(byte a, TBaseColor baseColor);

        #endregion
    }
}