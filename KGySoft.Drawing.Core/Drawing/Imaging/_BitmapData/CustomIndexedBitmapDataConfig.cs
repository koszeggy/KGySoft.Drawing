#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: CustomIndexedBitmapDataConfig.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2023 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System;

#endregion

namespace KGySoft.Drawing.Imaging
{
    public sealed class CustomIndexedBitmapDataConfig : CustomBitmapDataConfigBase
    {
        #region Properties
        
        #region Public Properties

        public Palette? Palette { get; set; }

        public Func<Palette, bool>? TrySetPaletteCallback { get; set; }

        public Func<ICustomBitmapDataRow, int, int>? RowGetColorIndex { get; set; }

        public Action<ICustomBitmapDataRow, int, int>? RowSetColorIndex { get; set; }

        #endregion

        #region Internal Properties

        internal Delegate? RowGetColorIndexLegacy { get; set; }
        internal Delegate? RowSetColorIndexLegacy { get; set; }

        #endregion

        #endregion

        #region Methods

        internal Func<ICustomBitmapDataRow<T>, int, int> GetRowGetColorIndex<T>()
            where T : unmanaged
        {
            // Saving to local variables to prevent capturing self reference
            var getColorIndexLegacy = RowGetColorIndexLegacy as Func<ICustomBitmapDataRow<T>, int, int>;
            Func<ICustomBitmapDataRow, int, int>? getColorIndex = RowGetColorIndex;

            return getColorIndexLegacy
#if NET35
                ?? (getColorIndex != null ? (r, x) => getColorIndex.Invoke(r, x)
                    : new Func<ICustomBitmapDataRow<T>, int, int>((_, _) => throw new InvalidOperationException(Res.ImagingCustomBitmapDataWriteOnly)));
#else
                ?? getColorIndex ?? new Func<ICustomBitmapDataRow<T>, int, int>((_, _) => throw new InvalidOperationException(Res.ImagingCustomBitmapDataWriteOnly));
#endif
        }

        internal Func<ICustomBitmapDataRow, int, int> GetRowGetColorIndex()
        {
            Debug.Assert(RowGetColorIndexLegacy == null, "Expected to call from unmanaged bitmap data only");

            // Saving to local variables to prevent capturing self reference
            Func<ICustomBitmapDataRow, int, int>? getColorIndex = RowGetColorIndex;
            return getColorIndex ?? new Func<ICustomBitmapDataRow, int, int>((_, _) => throw new InvalidOperationException(Res.ImagingCustomBitmapDataWriteOnly));
        }

        internal Action<ICustomBitmapDataRow<T>, int, int> GetRowSetColorIndex<T>()
            where T : unmanaged
        {
            // Saving to local variables to prevent capturing self reference
            var setColorIndexLegacy = RowSetColorIndexLegacy as Action<ICustomBitmapDataRow<T>, int, int>;
            Action<ICustomBitmapDataRow, int, int>? setColorIndex = RowSetColorIndex;

            return setColorIndexLegacy
#if NET35
                ?? (setColorIndex != null ? (r, x, c) => setColorIndex.Invoke(r, x, c)
                    : new Action<ICustomBitmapDataRow<T>, int, int>((_, _, _) => throw new InvalidOperationException(Res.ImagingCustomBitmapDataReadOnly)));
#else
                ?? setColorIndex ?? new Action<ICustomBitmapDataRow<T>, int, int>((_, _, _) => throw new InvalidOperationException(Res.ImagingCustomBitmapDataReadOnly));
#endif
        }

        internal Action<ICustomBitmapDataRow, int, int> GetRowSetColorIndex()
        {
            Debug.Assert(RowSetColorIndexLegacy == null, "Expected to call from unmanaged bitmap data only");

            // Saving to local variables to prevent capturing self reference
            Action<ICustomBitmapDataRow, int, int>? setColorIndex = RowSetColorIndex;
            return setColorIndex ?? new Action<ICustomBitmapDataRow, int, int>((_, _, _) => throw new InvalidOperationException(Res.ImagingCustomBitmapDataReadOnly));
        }

        #endregion
    }
}