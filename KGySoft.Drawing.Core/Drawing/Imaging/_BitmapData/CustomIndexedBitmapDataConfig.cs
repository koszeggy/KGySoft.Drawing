#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: CustomIndexedBitmapDataConfig.cs
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

#endregion

namespace KGySoft.Drawing.Imaging
{
    /// <summary>
    /// Represents the configuration of an indexed custom bitmap data that can be created by the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataFactory.CreateBitmapData">CreateBitmapData</see>
    /// methods that have a <see cref="CustomIndexedBitmapDataConfig"/> parameter.
    /// </summary>
    /// <remarks>
    /// <para>The <see cref="CustomBitmapDataConfigBase.PixelFormat"/> property and at least either the <see cref="RowGetColorIndex"/> or
    /// the <see cref="RowSetColorIndex"/> property must be set to create a valid custom bitmap data.</para>
    /// <para>If <see cref="RowSetColorIndex"/> is not set, then the custom bitmap data will be read-only.
    /// And if <see cref="RowGetColorIndex"/> is not set, then the custom bitmap data will be write-only.</para>
    /// <para>The delegates should not reference or capture the back buffer directly. Instead, they should use the <see cref="ICustomBitmapDataRow"/>
    /// property of the accessor delegates to access the bitmap data. If this is true for both of the accessor delegates you can set
    /// the <see cref="CustomBitmapDataConfigBase.BackBufferIndependentPixelAccess"/> property to provide better performance and quality in case of certain operations.</para>
    /// </remarks>
    public sealed class CustomIndexedBitmapDataConfig : CustomBitmapDataConfigBase
    {
        #region Properties

        #region Public Properties

        /// <summary>
        /// Gets or sets the desired <see cref="IBitmapData.Palette"/> of the created custom bitmap data.
        /// It determines also the <see cref="IBitmapData.BackColor"/> and <see cref="IBitmapData.AlphaThreshold"/> properties of the result.
        /// It can contain fewer colors than the specified <see cref="CustomBitmapDataConfigBase.PixelFormat"/> can handle.
        /// <br/>Default value: <see langword="null"/>.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="BitmapDataFactory.CreateBitmapData{T}(T[], Size, int, CustomIndexedBitmapDataConfig)"/> method for more details.
        /// </summary>
        public Palette? Palette { get; set; }

        /// <summary>
        /// Gets or sets a delegate that will be invoked when the <see cref="BitmapDataExtensions.TrySetPalette">TrySetPalette</see> extension method is called.
        /// If the bitmap data to create belongs to a bitmap of a 3rd party library, then this delegate can be used to update the actual palette of the actual bitmap.
        /// <br/>Default value: <see langword="null"/>.
        /// </summary>
        public Func<Palette, bool>? TrySetPaletteCallback { get; set; }

        /// <summary>
        /// Gets or sets a delegate that can retrieve a pixel of a row in the custom bitmap data as a palette index.
        /// Make sure you access the row content via the <see cref="ICustomBitmapDataRow"/> parameter of the delegate.
        /// <br/>Default value: <see langword="null"/>.
        /// </summary>
        public Func<ICustomBitmapDataRow, int, int>? RowGetColorIndex { get; set; }

        /// <summary>
        /// Gets or sets a delegate that can set a pixel of a row in the custom bitmap from a palette index.
        /// Make sure you access the row content via the <see cref="ICustomBitmapDataRow"/> parameter of the delegate.
        /// <br/>Default value: <see langword="null"/>.
        /// </summary>
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

        internal override bool CanRead() => (RowGetColorIndex ?? RowGetColorIndexLegacy) != null;
        internal override bool CanWrite() => (RowSetColorIndex ?? RowSetColorIndexLegacy) != null;

        #endregion
    }
}