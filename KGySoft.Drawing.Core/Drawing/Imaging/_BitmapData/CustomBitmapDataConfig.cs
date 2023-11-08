#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: CustomBitmapDataConfig.cs
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
using System.Drawing;

#endregion


namespace KGySoft.Drawing.Imaging
{
    /// <summary>
    /// Represents the configuration of a non-indexed custom bitmap data that can be created by the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataFactory.CreateBitmapData">CreateBitmapData</see>
    /// methods that have a <see cref="CustomBitmapDataConfig"/> parameter.
    /// </summary>
    /// <remarks>
    /// <para>The <see cref="CustomBitmapDataConfigBase.PixelFormat"/> property and and at least one getter or setter delegate must be set
    /// to create a valid custom bitmap data.</para>
    /// <para>It is enough to set only one getter and/or setter with the best matching color type. For example, if you set the <see cref="CustomBitmapDataConfig.RowGetColor64"/> property only,
    /// which returns the pixels as <see cref="Color64"/> values, then all of the other pixel-reading methods will use this delegate and will convert the result from <see cref="Color64"/>.</para>
    /// <para>If none of the setter delegates are set, then the custom bitmap data will be read-only.
    /// And if none of the getter delegates are set, then the custom bitmap data will be write-only.</para>
    /// <para>The delegates should not reference or capture the back buffer directly. Instead, they should use the <see cref="ICustomBitmapDataRow"/>
    /// property of the accessor delegates to access the bitmap data. If this is true for all of the delegates you can set the <see cref="CustomBitmapDataConfigBase.BackBufferIndependentPixelAccess"/>
    /// property to provide better performance and quality in case of certain operations.</para>
    /// </remarks>
    public sealed class CustomBitmapDataConfig : CustomBitmapDataConfigBase
    {
        #region Nested structs

        private struct _ { }

        #endregion

        #region Properties

        #region Public sProperties

        /// <summary>
        /// Gets or sets a <see cref="Color32"/> value for pixel formats without alpha gradient support that specifies the <see cref="IBitmapData.BackColor"/> value of the created bitmap data.
        /// It does not affect the actual created bitmap content. The alpha value (<see cref="Color32.A">Color32.A</see> field) of the specified background color is ignored.
        /// <br/>Default value: The default value of the <see cref="Color32"/> type, which has the same RGB values as <see cref="Color.Black"/>.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="BitmapDataFactory.CreateBitmapData(Size, KnownPixelFormat, Imaging.WorkingColorSpace, Color32, byte)"/> method for details about back color and alpha threshold.
        /// </summary>
        public Color32 BackColor { get; set; }

        /// <summary>
        /// Gets or sets a value for pixel formats without alpha gradient support that specifies the <see cref="IBitmapData.AlphaThreshold"/> value of the created bitmap data.
        /// See the <strong>Remarks</strong> section for details.
        /// <br/>Default value: <c>128</c>.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="BitmapDataFactory.CreateBitmapData(Size, KnownPixelFormat, Imaging.WorkingColorSpace, Color32, byte)"/> method for details about back color and alpha threshold.
        /// </summary>
        public byte AlphaThreshold { get; set; } = 128;

        /// <summary>
        /// Gets or sets the preferred color space that should be used when working with the result bitmap data.
        /// <br/>Default value: <see cref="Imaging.WorkingColorSpace.Default"/>, which means the linear color space of <see cref="PixelFormatInfo.LinearGamma"/> is set in <see cref="CustomBitmapDataConfigBase.PixelFormat"/>
        /// and the sRGB color space otherwise.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="Imaging.WorkingColorSpace"/> enumeration for more details.
        /// </summary>
        public WorkingColorSpace WorkingColorSpace { get; set; }

        /// <summary>
        /// Gets or sets a delegate that can retrieve a pixel of a row in the custom bitmap data as a <see cref="Color32"/> value.
        /// An image processing operation may prefer this delegate by default.
        /// If this property is not set, the other delegates tried to be used as a fallback.
        /// Make sure you access the row content via the <see cref="ICustomBitmapDataRow"/> parameter of the delegate.
        /// <br/>Default value: <see langword="null"/>.
        /// </summary>
        public Func<ICustomBitmapDataRow, int, Color32>? RowGetColor32 { get; set; }

        /// <summary>
        /// Gets or sets a delegate that can set a pixel of a row in the custom bitmap data from a <see cref="Color32"/> value.
        /// An image processing operation may prefer this delegate by default.
        /// If this property is not set, the other delegates tried to be used as a fallback.
        /// Make sure you access the row content via the <see cref="ICustomBitmapDataRow"/> parameter of the delegate.
        /// <br/>Default value: <see langword="null"/>.
        /// </summary>
        public Action<ICustomBitmapDataRow, int, Color32>? RowSetColor32 { get; set; }

        /// <summary>
        /// Gets or sets a delegate that can retrieve a pixel of a row in the custom bitmap data as a <see cref="PColor32"/> value.
        /// An image processing operation may prefer this delegate if <see cref="PixelFormatInfo.HasPremultipliedAlpha"/> is set
        /// in <see cref="CustomBitmapDataConfigBase.PixelFormat"/>. If this property is not set, the other delegates tried to be used as a fallback.
        /// Make sure you access the row content via the <see cref="ICustomBitmapDataRow"/> parameter of the delegate.
        /// <br/>Default value: <see langword="null"/>.
        /// </summary>
        public Func<ICustomBitmapDataRow, int, PColor32>? RowGetPColor32 { get; set; }

        /// <summary>
        /// Gets or sets a delegate that can set a pixel of a row in the custom bitmap data from a <see cref="PColor32"/> value.
        /// An image processing operation may prefer this delegate if <see cref="PixelFormatInfo.HasPremultipliedAlpha"/> is set
        /// in <see cref="CustomBitmapDataConfigBase.PixelFormat"/>. If this property is not set, the other delegates tried to be used as a fallback.
        /// Make sure you access the row content via the <see cref="ICustomBitmapDataRow"/> parameter of the delegate.
        /// <br/>Default value: <see langword="null"/>.
        /// </summary>
        public Action<ICustomBitmapDataRow, int, PColor32>? RowSetPColor32 { get; set; }

        /// <summary>
        /// Gets or sets a delegate that can retrieve a pixel of a row in the custom bitmap data as a <see cref="Color64"/> value.
        /// An image processing operation may prefer this delegate if <see cref="PixelFormatInfo.Prefers64BitColors"/> is set
        /// in <see cref="CustomBitmapDataConfigBase.PixelFormat"/>. If this property is not set, the other delegates tried to be used as a fallback.
        /// Make sure you access the row content via the <see cref="ICustomBitmapDataRow"/> parameter of the delegate.
        /// <br/>Default value: <see langword="null"/>.
        /// </summary>
        public Func<ICustomBitmapDataRow, int, Color64>? RowGetColor64 { get; set; }

        /// <summary>
        /// Gets or sets a delegate that can set a pixel of a row in the custom bitmap data from a <see cref="Color64"/> value.
        /// An image processing operation may prefer this delegate if <see cref="PixelFormatInfo.Prefers64BitColors"/> is set
        /// in <see cref="CustomBitmapDataConfigBase.PixelFormat"/>. If this property is not set, the other delegates tried to be used as a fallback.
        /// Make sure you access the row content via the <see cref="ICustomBitmapDataRow"/> parameter of the delegate.
        /// <br/>Default value: <see langword="null"/>.
        /// </summary>
        public Action<ICustomBitmapDataRow, int, Color64>? RowSetColor64 { get; set; }

        /// <summary>
        /// Gets or sets a delegate that can retrieve a pixel of a row in the custom bitmap data as a <see cref="PColor64"/> value.
        /// An image processing operation may prefer this delegate if <see cref="PixelFormatInfo.Prefers64BitColors"/> and <see cref="PixelFormatInfo.HasPremultipliedAlpha"/>
        /// are set in <see cref="CustomBitmapDataConfigBase.PixelFormat"/>. If this property is not set, the other delegates tried to be used as a fallback.
        /// Make sure you access the row content via the <see cref="ICustomBitmapDataRow"/> parameter of the delegate.
        /// <br/>Default value: <see langword="null"/>.
        /// </summary>
        public Func<ICustomBitmapDataRow, int, PColor64>? RowGetPColor64 { get; set; }

        /// <summary>
        /// Gets or sets a delegate that can set a pixel of a row in the custom bitmap data from a <see cref="PColor64"/> value.
        /// An image processing operation may prefer this delegate if <see cref="PixelFormatInfo.Prefers64BitColors"/> and <see cref="PixelFormatInfo.HasPremultipliedAlpha"/>
        /// are set in <see cref="CustomBitmapDataConfigBase.PixelFormat"/>. If this property is not set, the other delegates tried to be used as a fallback.
        /// Make sure you access the row content via the <see cref="ICustomBitmapDataRow"/> parameter of the delegate.
        /// <br/>Default value: <see langword="null"/>.
        /// </summary>
        public Action<ICustomBitmapDataRow, int, PColor64>? RowSetPColor64 { get; set; }

        /// <summary>
        /// Gets or sets a delegate that can retrieve a pixel of a row in the custom bitmap data as a <see cref="ColorF"/> value.
        /// An image processing operation may prefer this delegate if <see cref="PixelFormatInfo.Prefers128BitColors"/> is set
        /// in <see cref="CustomBitmapDataConfigBase.PixelFormat"/>. If this property is not set, the other delegates tried to be used as a fallback.
        /// Make sure you access the row content via the <see cref="ICustomBitmapDataRow"/> parameter of the delegate.
        /// <br/>Default value: <see langword="null"/>.
        /// </summary>
        public Func<ICustomBitmapDataRow, int, ColorF>? RowGetColorF { get; set; }

        /// <summary>
        /// Gets or sets a delegate that can set a pixel of a row in the custom bitmap data from a <see cref="ColorF"/> value.
        /// An image processing operation may prefer this delegate if <see cref="PixelFormatInfo.Prefers128BitColors"/> is set
        /// in <see cref="CustomBitmapDataConfigBase.PixelFormat"/>. If this property is not set, the other delegates tried to be used as a fallback.
        /// Make sure you access the row content via the <see cref="ICustomBitmapDataRow"/> parameter of the delegate.
        /// <br/>Default value: <see langword="null"/>.
        /// </summary>
        public Action<ICustomBitmapDataRow, int, ColorF>? RowSetColorF { get; set; }

        /// <summary>
        /// Gets or sets a delegate that can retrieve a pixel of a row in the custom bitmap data as a <see cref="PColorF"/> value.
        /// An image processing operation may prefer this delegate if <see cref="PixelFormatInfo.Prefers128BitColors"/> and <see cref="PixelFormatInfo.HasPremultipliedAlpha"/>
        /// are set in <see cref="CustomBitmapDataConfigBase.PixelFormat"/>. If this property is not set, the other delegates tried to be used as a fallback.
        /// Make sure you access the row content via the <see cref="ICustomBitmapDataRow"/> parameter of the delegate.
        /// <br/>Default value: <see langword="null"/>.
        /// </summary>
        public Func<ICustomBitmapDataRow, int, PColorF>? RowGetPColorF { get; set; }

        /// <summary>
        /// Gets or sets a delegate that can set a pixel of a row in the custom bitmap data from a <see cref="PColorF"/> value.
        /// An image processing operation may prefer this delegate if <see cref="PixelFormatInfo.Prefers128BitColors"/> and <see cref="PixelFormatInfo.HasPremultipliedAlpha"/>
        /// are set in <see cref="CustomBitmapDataConfigBase.PixelFormat"/>. If this property is not set, the other delegates tried to be used as a fallback.
        /// Make sure you access the row content via the <see cref="ICustomBitmapDataRow"/> parameter of the delegate.
        /// <br/>Default value: <see langword="null"/>.
        /// </summary>
        public Action<ICustomBitmapDataRow, int, PColorF>? RowSetPColorF { get; set; }

        #endregion

        #region Internal Properties

        internal Delegate? RowGetColorLegacy { get; set; }
        internal Delegate? RowSetColorLegacy { get; set; }

        #endregion

        #endregion

        #region Methods

        internal Func<ICustomBitmapDataRow<T>, int, Color32> GetRowGetColor32<T>()
            where T : unmanaged
        {
            // Saving to local variables to prevent capturing self reference
            var getColorLegacy = RowGetColorLegacy as Func<ICustomBitmapDataRow<T>, int, Color32>;
            Func<ICustomBitmapDataRow, int, Color32>? getColor32 = RowGetColor32;
            Func<ICustomBitmapDataRow, int, PColor32>? getPColor32 = RowGetPColor32;
            Func<ICustomBitmapDataRow, int, Color64>? getColor64 = RowGetColor64;
            Func<ICustomBitmapDataRow, int, PColor64>? getPColor64 = RowGetPColor64;
            Func<ICustomBitmapDataRow, int, ColorF>? getColorF = RowGetColorF;
            Func<ICustomBitmapDataRow, int, PColorF>? getPColorF = RowGetPColorF;

            return getColorLegacy
#if NET35
                ?? (getColor32 != null ? (r, x) => getColor32.Invoke(r, x)
                    : getColor64 != null ? (r, x) => getColor64.Invoke(r, x).ToColor32()
#else
                ?? getColor32 ?? (getColor64 != null ? (r, x) => getColor64.Invoke(r, x).ToColor32()
#endif
                    : getColorF != null ? (r, x) => getColorF.Invoke(r, x).ToColor32()
                    : getPColor32 != null ? (r, x) => getPColor32.Invoke(r, x).ToColor32()
                    : getPColor64 != null ? (r, x) => getPColor64.Invoke(r, x).ToColor32()
                    : getPColorF != null ? (r, x) => getPColorF.Invoke(r, x).ToColor32()
                    : new Func<ICustomBitmapDataRow<T>, int, Color32>((_, _) => throw new InvalidOperationException(Res.ImagingCustomBitmapDataWriteOnly)));
        }

        internal Func<ICustomBitmapDataRow, int, Color32> GetRowGetColor32()
        {
            Debug.Assert(RowGetColorLegacy == null, "Expected to call from unmanaged bitmap data only");

            // Saving to local variables to prevent capturing self reference
            Func<ICustomBitmapDataRow, int, Color32>? getColor32 = RowGetColor32;
            Func<ICustomBitmapDataRow, int, PColor32>? getPColor32 = RowGetPColor32;
            Func<ICustomBitmapDataRow, int, Color64>? getColor64 = RowGetColor64;
            Func<ICustomBitmapDataRow, int, PColor64>? getPColor64 = RowGetPColor64;
            Func<ICustomBitmapDataRow, int, ColorF>? getColorF = RowGetColorF;
            Func<ICustomBitmapDataRow, int, PColorF>? getPColorF = RowGetPColorF;

            return getColor32 ?? (getColor64 != null ? (r, x) => getColor64.Invoke(r, x).ToColor32()
                : getColorF != null ? (r, x) => getColorF.Invoke(r, x).ToColor32()
                : getPColor32 != null ? (r, x) => getPColor32.Invoke(r, x).ToColor32()
                : getPColor64 != null ? (r, x) => getPColor64.Invoke(r, x).ToColor32()
                : getPColorF != null ? (r, x) => getPColorF.Invoke(r, x).ToColor32()
                : new Func<ICustomBitmapDataRow, int, Color32>((_, _) => throw new InvalidOperationException(Res.ImagingCustomBitmapDataWriteOnly)));
        }

        internal Action<ICustomBitmapDataRow<T>, int, Color32> GetRowSetColor32<T>()
            where T : unmanaged
        {
            // Saving to local variables to prevent capturing self reference
            var setColorLegacy = RowSetColorLegacy as Action<ICustomBitmapDataRow<T>, int, Color32>;
            Action<ICustomBitmapDataRow, int, Color32>? setColor32 = RowSetColor32;
            Action<ICustomBitmapDataRow, int, PColor32>? setPColor32 = RowSetPColor32;
            Action<ICustomBitmapDataRow, int, Color64>? setColor64 = RowSetColor64;
            Action<ICustomBitmapDataRow, int, PColor64>? setPColor64 = RowSetPColor64;
            Action<ICustomBitmapDataRow, int, ColorF>? setColorF = RowSetColorF;
            Action<ICustomBitmapDataRow, int, PColorF>? setPColorF = RowSetPColorF;

            return setColorLegacy
#if NET35
                ?? (setColor32 != null ? (r, x, c) => setColor32.Invoke(r, x, c)
                    : setColor64 != null ? (r, x, c) => setColor64.Invoke(r, x, c.ToColor64())
#else
                ?? setColor32 ?? (setColor64 != null ? (r, x, c) => setColor64.Invoke(r, x, c.ToColor64())
#endif
                    : setColorF != null ? (r, x, c) => setColorF.Invoke(r, x, c.ToColorF())
                    : setPColor32 != null ? (r, x, c) => setPColor32.Invoke(r, x, c.ToPColor32())
                    : setPColor64 != null ? (r, x, c) => setPColor64.Invoke(r, x, c.ToPColor64())
                    : setPColorF != null ? (r, x, c) => setPColorF.Invoke(r, x, c.ToPColorF())
                    : new Action<ICustomBitmapDataRow<T>, int, Color32>((_, _, _) => throw new InvalidOperationException(Res.ImagingCustomBitmapDataReadOnly)));
        }

        internal Action<ICustomBitmapDataRow, int, Color32> GetRowSetColor32()
        {
            Debug.Assert(RowSetColorLegacy == null, "Expected to call from unmanaged bitmap data only");

            // Saving to local variables to prevent capturing self reference
            Action<ICustomBitmapDataRow, int, Color32>? setColor32 = RowSetColor32;
            Action<ICustomBitmapDataRow, int, PColor32>? setPColor32 = RowSetPColor32;
            Action<ICustomBitmapDataRow, int, Color64>? setColor64 = RowSetColor64;
            Action<ICustomBitmapDataRow, int, PColor64>? setPColor64 = RowSetPColor64;
            Action<ICustomBitmapDataRow, int, ColorF>? setColorF = RowSetColorF;
            Action<ICustomBitmapDataRow, int, PColorF>? setPColorF = RowSetPColorF;

            return setColor32 ?? (setColor64 != null ? (r, x, c) => setColor64.Invoke(r, x, c.ToColor64())
                : setColorF != null ? (r, x, c) => setColorF.Invoke(r, x, c.ToColorF())
                : setPColor32 != null ? (r, x, c) => setPColor32.Invoke(r, x, c.ToPColor32())
                : setPColor64 != null ? (r, x, c) => setPColor64.Invoke(r, x, c.ToPColor64())
                : setPColorF != null ? (r, x, c) => setPColorF.Invoke(r, x, c.ToPColorF())
                : new Action<ICustomBitmapDataRow, int, Color32>((_, _, _) => throw new InvalidOperationException(Res.ImagingCustomBitmapDataReadOnly)));
        }

        internal Func<ICustomBitmapDataRow, int, PColor32> GetRowGetPColor32<T>()
            where T : unmanaged
        {
            // Saving to local variables to prevent capturing self reference
            var getColorLegacy = RowGetColorLegacy as Func<ICustomBitmapDataRow<T>, int, Color32>;
            Func<ICustomBitmapDataRow, int, Color32>? getColor32 = RowGetColor32;
            Func<ICustomBitmapDataRow, int, PColor32>? getPColor32 = RowGetPColor32;
            Func<ICustomBitmapDataRow, int, Color64>? getColor64 = RowGetColor64;
            Func<ICustomBitmapDataRow, int, PColor64>? getPColor64 = RowGetPColor64;
            Func<ICustomBitmapDataRow, int, ColorF>? getColorF = RowGetColorF;
            Func<ICustomBitmapDataRow, int, PColorF>? getPColorF = RowGetPColorF;

            return getPColor32 ?? (getPColor64 != null ? (r, x) => getPColor64.Invoke(r, x).ToPColor32()
                : getColor32 != null ? (r, x) => getColor32.Invoke(r, x).ToPColor32()
                : getColorLegacy != null ? (r, x) => getColorLegacy.Invoke((ICustomBitmapDataRow<T>)r, x).ToPColor32()
                : getColor64 != null ? (r, x) => getColor64.Invoke(r, x).ToPColor32()
                : getColorF != null ? (r, x) => getColorF.Invoke(r, x).ToPColor32()
                : getPColorF != null ? (r, x) => getPColorF.Invoke(r, x).ToPColor32()
                : new Func<ICustomBitmapDataRow, int, PColor32>((_, _) => throw new InvalidOperationException(Res.ImagingCustomBitmapDataWriteOnly)));
        }

        internal Func<ICustomBitmapDataRow, int, PColor32> GetRowGetPColor32()
        {
            Debug.Assert(RowGetColorLegacy == null, "Expected to call from unmanaged bitmap data only");
            return GetRowGetPColor32<_>();
        }

        internal Action<ICustomBitmapDataRow, int, PColor32> GetRowSetPColor32<T>()
            where T : unmanaged
        {
            // Saving to local variables to prevent capturing self reference
            var setColorLegacy = RowSetColorLegacy as Action<ICustomBitmapDataRow<T>, int, Color32>;
            Action<ICustomBitmapDataRow, int, Color32>? setColor32 = RowSetColor32;
            Action<ICustomBitmapDataRow, int, PColor32>? setPColor32 = RowSetPColor32;
            Action<ICustomBitmapDataRow, int, Color64>? setColor64 = RowSetColor64;
            Action<ICustomBitmapDataRow, int, PColor64>? setPColor64 = RowSetPColor64;
            Action<ICustomBitmapDataRow, int, ColorF>? setColorF = RowSetColorF;
            Action<ICustomBitmapDataRow, int, PColorF>? setPColorF = RowSetPColorF;

            return setPColor32 ?? (setPColor64 != null ? (r, x, c) => setPColor64.Invoke(r, x, c.ToPColor64())
                : setColor32 != null ? (r, x, c) => setColor32.Invoke(r, x, c.ToColor32())
                : setColorLegacy != null ? (r, x, c) => setColorLegacy.Invoke((ICustomBitmapDataRow<T>)r, x, c.ToColor32())
                : setColorF != null ? (r, x, c) => setColorF.Invoke(r, x, c.ToColorF())
                : setColor64 != null ? (r, x, c) => setColor64.Invoke(r, x, c.ToColor64())
                : setPColorF != null ? (r, x, c) => setPColorF.Invoke(r, x, c.ToPColorF())
                : new Action<ICustomBitmapDataRow, int, PColor32>((_, _, _) => throw new InvalidOperationException(Res.ImagingCustomBitmapDataReadOnly)));
        }

        internal Action<ICustomBitmapDataRow, int, PColor32> GetRowSetPColor32()
        {
            Debug.Assert(RowSetColorLegacy == null, "Expected to call from unmanaged bitmap data only");
            return GetRowSetPColor32<_>();
        }

        internal Func<ICustomBitmapDataRow, int, Color64> GetRowGetColor64<T>()
            where T : unmanaged
        {
            // Saving to local variables to prevent capturing self reference
            var getColorLegacy = RowGetColorLegacy as Func<ICustomBitmapDataRow<T>, int, Color32>;
            Func<ICustomBitmapDataRow, int, Color32>? getColor32 = RowGetColor32;
            Func<ICustomBitmapDataRow, int, PColor32>? getPColor32 = RowGetPColor32;
            Func<ICustomBitmapDataRow, int, Color64>? getColor64 = RowGetColor64;
            Func<ICustomBitmapDataRow, int, PColor64>? getPColor64 = RowGetPColor64;
            Func<ICustomBitmapDataRow, int, ColorF>? getColorF = RowGetColorF;
            Func<ICustomBitmapDataRow, int, PColorF>? getPColorF = RowGetPColorF;

            return getColor64 ?? (getPColor64 != null ? (r, x) => getPColor64.Invoke(r, x).ToColor64()
                : getColorF != null ? (r, x) => getColorF.Invoke(r, x).ToColor64()
                : getPColorF != null ? (r, x) => getPColorF.Invoke(r, x).ToColor64()
                : getColor32 != null ? (r, x) => getColor32.Invoke(r, x).ToColor64()
                : getColorLegacy != null ? (r, x) => getColorLegacy.Invoke((ICustomBitmapDataRow<T>)r, x).ToColor64()
                : getPColor32 != null ? (r, x) => getPColor32.Invoke(r, x).ToColor64()
                : new Func<ICustomBitmapDataRow, int, Color64>((_, _) => throw new InvalidOperationException(Res.ImagingCustomBitmapDataWriteOnly)));
        }

        internal Func<ICustomBitmapDataRow, int, Color64> GetRowGetColor64()
        {
            Debug.Assert(RowGetColorLegacy == null, "Expected to call from unmanaged bitmap data only");
            return GetRowGetColor64<_>();
        }
        internal Action<ICustomBitmapDataRow, int, Color64> GetRowSetColor64<T>()
            where T : unmanaged
        {
            // Saving to local variables to prevent capturing self reference
            var setColorLegacy = RowSetColorLegacy as Action<ICustomBitmapDataRow<T>, int, Color32>;
            Action<ICustomBitmapDataRow, int, Color32>? setColor32 = RowSetColor32;
            Action<ICustomBitmapDataRow, int, PColor32>? setPColor32 = RowSetPColor32;
            Action<ICustomBitmapDataRow, int, Color64>? setColor64 = RowSetColor64;
            Action<ICustomBitmapDataRow, int, PColor64>? setPColor64 = RowSetPColor64;
            Action<ICustomBitmapDataRow, int, ColorF>? setColorF = RowSetColorF;
            Action<ICustomBitmapDataRow, int, PColorF>? setPColorF = RowSetPColorF;

            return setColor64 ?? (setColorF != null ? (r, x, c) => setColorF.Invoke(r, x, c.ToColorF())
                : setPColorF != null ? (r, x, c) => setPColorF.Invoke(r, x, c.ToPColorF())
                : setPColor64 != null ? (r, x, c) => setPColor64.Invoke(r, x, c.ToPColor64())
                : setColor32 != null ? (r, x, c) => setColor32.Invoke(r, x, c.ToColor32())
                : setColorLegacy != null ? (r, x, c) => setColorLegacy.Invoke((ICustomBitmapDataRow<T>)r, x, c.ToColor32())
                : setPColor32 != null ? (r, x, c) => setPColor32.Invoke(r, x, c.ToPColor32())
                : new Action<ICustomBitmapDataRow, int, Color64>((_, _, _) => throw new InvalidOperationException(Res.ImagingCustomBitmapDataReadOnly)));
        }

        internal Action<ICustomBitmapDataRow, int, Color64> GetRowSetColor64()
        {
            Debug.Assert(RowSetColorLegacy == null, "Expected to call from unmanaged bitmap data only");
            return GetRowSetColor64<_>();
        }

        internal Func<ICustomBitmapDataRow, int, PColor64> GetRowGetPColor64<T>()
            where T : unmanaged
        {
            // Saving to local variables to prevent capturing self reference
            var getColorLegacy = RowGetColorLegacy as Func<ICustomBitmapDataRow<T>, int, Color32>;
            Func<ICustomBitmapDataRow, int, Color32>? getColor32 = RowGetColor32;
            Func<ICustomBitmapDataRow, int, PColor32>? getPColor32 = RowGetPColor32;
            Func<ICustomBitmapDataRow, int, Color64>? getColor64 = RowGetColor64;
            Func<ICustomBitmapDataRow, int, PColor64>? getPColor64 = RowGetPColor64;
            Func<ICustomBitmapDataRow, int, ColorF>? getColorF = RowGetColorF;
            Func<ICustomBitmapDataRow, int, PColorF>? getPColorF = RowGetPColorF;

            return getPColor64 ?? (getColor64 != null ? (r, x) => getColor64.Invoke(r, x).ToPColor64()
                : getColorF != null ? (r, x) => getColorF.Invoke(r, x).ToPColor64()
                : getPColorF != null ? (r, x) => getPColorF.Invoke(r, x).ToPColor64()
                : getPColor32 != null ? (r, x) => getPColor32.Invoke(r, x).ToPColor64()
                : getColor32 != null ? (r, x) => getColor32.Invoke(r, x).ToPColor64()
                : getColorLegacy != null ? (r, x) => getColorLegacy.Invoke((ICustomBitmapDataRow<T>)r, x).ToPColor64()
                : new Func<ICustomBitmapDataRow, int, PColor64>((_, _) => throw new InvalidOperationException(Res.ImagingCustomBitmapDataWriteOnly)));
        }

        internal Func<ICustomBitmapDataRow, int, PColor64> GetRowGetPColor64()
        {
            Debug.Assert(RowGetColorLegacy == null, "Expected to call from unmanaged bitmap data only");
            return GetRowGetPColor64<_>();
        }

        internal Action<ICustomBitmapDataRow, int, PColor64> GetRowSetPColor64<T>()
            where T : unmanaged
        {
            // Saving to local variables to prevent capturing self reference
            var setColorLegacy = RowSetColorLegacy as Action<ICustomBitmapDataRow<T>, int, Color32>;
            Action<ICustomBitmapDataRow, int, Color32>? setColor32 = RowSetColor32;
            Action<ICustomBitmapDataRow, int, PColor32>? setPColor32 = RowSetPColor32;
            Action<ICustomBitmapDataRow, int, Color64>? setColor64 = RowSetColor64;
            Action<ICustomBitmapDataRow, int, PColor64>? setPColor64 = RowSetPColor64;
            Action<ICustomBitmapDataRow, int, ColorF>? setColorF = RowSetColorF;
            Action<ICustomBitmapDataRow, int, PColorF>? setPColorF = RowSetPColorF;

            return setPColor64 ?? (setColor64 != null ? (r, x, c) => setColor64.Invoke(r, x, c.ToColor64())
                : setColorF != null ? (r, x, c) => setColorF.Invoke(r, x, c.ToColorF())
                : setPColorF != null ? (r, x, c) => setPColorF.Invoke(r, x, c.ToPColorF())
                : setPColor32 != null ? (r, x, c) => setPColor32.Invoke(r, x, c.ToPColor32())
                : setColor32 != null ? (r, x, c) => setColor32.Invoke(r, x, c.ToColor32())
                : setColorLegacy != null ? (r, x, c) => setColorLegacy.Invoke((ICustomBitmapDataRow<T>)r, x, c.ToColor32())
                : new Action<ICustomBitmapDataRow, int, PColor64>((_, _, _) => throw new InvalidOperationException(Res.ImagingCustomBitmapDataReadOnly)));
        }

        internal Action<ICustomBitmapDataRow, int, PColor64> GetRowSetPColor64()
        {
            Debug.Assert(RowSetColorLegacy == null, "Expected to call from unmanaged bitmap data only");
            return GetRowSetPColor64<_>();
        }

        internal Func<ICustomBitmapDataRow, int, ColorF> GetRowGetColorF<T>()
            where T : unmanaged
        {
            // Saving to local variables to prevent capturing self reference
            var getColorLegacy = RowGetColorLegacy as Func<ICustomBitmapDataRow<T>, int, Color32>;
            Func<ICustomBitmapDataRow, int, Color32>? getColor32 = RowGetColor32;
            Func<ICustomBitmapDataRow, int, PColor32>? getPColor32 = RowGetPColor32;
            Func<ICustomBitmapDataRow, int, Color64>? getColor64 = RowGetColor64;
            Func<ICustomBitmapDataRow, int, PColor64>? getPColor64 = RowGetPColor64;
            Func<ICustomBitmapDataRow, int, ColorF>? getColorF = RowGetColorF;
            Func<ICustomBitmapDataRow, int, PColorF>? getPColorF = RowGetPColorF;

            return getColorF ?? (getPColorF != null ? (r, x) => getPColorF.Invoke(r, x).ToColorF()
                : getColor64 != null ? (r, x) => getColor64.Invoke(r, x).ToColorF()
                : getPColor64 != null ? (r, x) => getPColor64.Invoke(r, x).ToColorF()
                : getColor32 != null ? (r, x) => getColor32.Invoke(r, x).ToColorF()
                : getColorLegacy != null ? (r, x) => getColorLegacy.Invoke((ICustomBitmapDataRow<T>)r, x).ToColorF()
                : getPColor32 != null ? (r, x) => getPColor32.Invoke(r, x).ToColorF()
                : new Func<ICustomBitmapDataRow, int, ColorF>((_, _) => throw new InvalidOperationException(Res.ImagingCustomBitmapDataWriteOnly)));
        }

        internal Func<ICustomBitmapDataRow, int, ColorF> GetRowGetColorF()
        {
            Debug.Assert(RowGetColorLegacy == null, "Expected to call from unmanaged bitmap data only");
            return GetRowGetColorF<_>();
        }

        internal Action<ICustomBitmapDataRow, int, ColorF> GetRowSetColorF<T>()
            where T : unmanaged
        {
            // Saving to local variables to prevent capturing self reference
            var setColorLegacy = RowSetColorLegacy as Action<ICustomBitmapDataRow<T>, int, Color32>;
            Action<ICustomBitmapDataRow, int, Color32>? setColor32 = RowSetColor32;
            Action<ICustomBitmapDataRow, int, PColor32>? setPColor32 = RowSetPColor32;
            Action<ICustomBitmapDataRow, int, Color64>? setColor64 = RowSetColor64;
            Action<ICustomBitmapDataRow, int, PColor64>? setPColor64 = RowSetPColor64;
            Action<ICustomBitmapDataRow, int, ColorF>? setColorF = RowSetColorF;
            Action<ICustomBitmapDataRow, int, PColorF>? setPColorF = RowSetPColorF;

            return setColorF ?? (setPColorF != null ? (r, x, c) => setPColorF.Invoke(r, x, c.ToPColorF())
                : setColor64 != null ? (r, x, c) => setColor64.Invoke(r, x, c.ToColor64())
                : setPColor64 != null ? (r, x, c) => setPColor64.Invoke(r, x, c.ToPColor64())
                : setColor32 != null ? (r, x, c) => setColor32.Invoke(r, x, c.ToColor32())
                : setColorLegacy != null ? (r, x, c) => setColorLegacy.Invoke((ICustomBitmapDataRow<T>)r, x, c.ToColor32())
                : setPColor32 != null ? (r, x, c) => setPColor32.Invoke(r, x, c.ToPColor32())
                : new Action<ICustomBitmapDataRow, int, ColorF>((_, _, _) => throw new InvalidOperationException(Res.ImagingCustomBitmapDataReadOnly)));
        }


        internal Action<ICustomBitmapDataRow, int, ColorF> GetRowSetColorF()
        {
            Debug.Assert(RowSetColorLegacy == null, "Expected to call from unmanaged bitmap data only");
            return GetRowSetColorF<_>();
        }

        internal Func<ICustomBitmapDataRow, int, PColorF> GetRowGetPColorF<T>()
            where T : unmanaged
        {
            // Saving to local variables to prevent capturing self reference
            var getColorLegacy = RowGetColorLegacy as Func<ICustomBitmapDataRow<T>, int, Color32>;
            Func<ICustomBitmapDataRow, int, Color32>? getColor32 = RowGetColor32;
            Func<ICustomBitmapDataRow, int, PColor32>? getPColor32 = RowGetPColor32;
            Func<ICustomBitmapDataRow, int, Color64>? getColor64 = RowGetColor64;
            Func<ICustomBitmapDataRow, int, PColor64>? getPColor64 = RowGetPColor64;
            Func<ICustomBitmapDataRow, int, ColorF>? getColorF = RowGetColorF;
            Func<ICustomBitmapDataRow, int, PColorF>? getPColorF = RowGetPColorF;

            return getPColorF ?? (getColorF != null ? (r, x) => getColorF.Invoke(r, x).ToPColorF()
                : getColor64 != null ? (r, x) => getColor64.Invoke(r, x).ToPColorF()
                : getPColor64 != null ? (r, x) => getPColor64.Invoke(r, x).ToPColorF()
                : getColor32 != null ? (r, x) => getColor32.Invoke(r, x).ToPColorF()
                : getColorLegacy != null ? (r, x) => getColorLegacy.Invoke((ICustomBitmapDataRow<T>)r, x).ToPColorF()
                : getPColor32 != null ? (r, x) => getPColor32.Invoke(r, x).ToPColorF()
                : new Func<ICustomBitmapDataRow, int, PColorF>((_, _) => throw new InvalidOperationException(Res.ImagingCustomBitmapDataWriteOnly)));
        }

        internal Func<ICustomBitmapDataRow, int, PColorF> GetRowGetPColorF()
        {
            Debug.Assert(RowGetColorLegacy == null, "Expected to call from unmanaged bitmap data only");
            return GetRowGetPColorF<_>();
        }

        internal Action<ICustomBitmapDataRow, int, PColorF> GetRowSetPColorF<T>()
            where T : unmanaged
        {
            // Saving to local variables to prevent capturing self reference
            var setColorLegacy = RowSetColorLegacy as Action<ICustomBitmapDataRow<T>, int, Color32>;
            Action<ICustomBitmapDataRow, int, Color32>? setColor32 = RowSetColor32;
            Action<ICustomBitmapDataRow, int, PColor32>? setPColor32 = RowSetPColor32;
            Action<ICustomBitmapDataRow, int, Color64>? setColor64 = RowSetColor64;
            Action<ICustomBitmapDataRow, int, PColor64>? setPColor64 = RowSetPColor64;
            Action<ICustomBitmapDataRow, int, ColorF>? setColorF = RowSetColorF;
            Action<ICustomBitmapDataRow, int, PColorF>? setPColorF = RowSetPColorF;

            return setPColorF ?? (setColorF != null ? (r, x, c) => setColorF.Invoke(r, x, c.ToColorF())
                : setColor64 != null ? (r, x, c) => setColor64.Invoke(r, x, c.ToColor64())
                : setPColor64 != null ? (r, x, c) => setPColor64.Invoke(r, x, c.ToPColor64())
                : setColor32 != null ? (r, x, c) => setColor32.Invoke(r, x, c.ToColor32())
                : setColorLegacy != null ? (r, x, c) => setColorLegacy.Invoke((ICustomBitmapDataRow<T>)r, x, c.ToColor32())
                : setPColor32 != null ? (r, x, c) => setPColor32.Invoke(r, x, c.ToPColor32())
                : new Action<ICustomBitmapDataRow, int, PColorF>((_, _, _) => throw new InvalidOperationException(Res.ImagingCustomBitmapDataReadOnly)));
        }

        internal Action<ICustomBitmapDataRow, int, PColorF> GetRowSetPColorF()
        {
            Debug.Assert(RowSetColorLegacy == null, "Expected to call from unmanaged bitmap data only");
            return GetRowSetPColorF<_>();
        }

        internal override bool CanRead() => (RowGetColor32 ?? RowGetPColor32 ?? RowGetColor64 ?? RowGetPColor64 ?? RowGetColorF ?? RowGetPColorF ?? RowGetColorLegacy) != null;
        internal override bool CanWrite() => (RowSetColor32 ?? RowSetPColor32 ?? RowSetColor64 ?? RowSetPColor64 ?? RowSetColorF ?? RowSetPColorF ?? RowSetColorLegacy) != null;

        #endregion
    }
}
