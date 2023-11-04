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

#endregion


namespace KGySoft.Drawing.Imaging
{
    public sealed class CustomBitmapDataConfig : CustomBitmapDataConfigBase
    {
        #region Nested structs

        private struct _ { }

        #endregion

        #region Properties

        #region Public sProperties

        public Color32 BackColor { get; set; }
        public byte AlphaThreshold { get; set; }
        public WorkingColorSpace WorkingColorSpace { get; set; }

        public Func<ICustomBitmapDataRow, int, Color32>? RowGetColor32 { get; set; }
        public Action<ICustomBitmapDataRow, int, Color32>? RowSetColor32 { get; set; }
        public Func<ICustomBitmapDataRow, int, PColor32>? RowGetPColor32 { get; set; }
        public Action<ICustomBitmapDataRow, int, PColor32>? RowSetPColor32 { get; set; }
        public Func<ICustomBitmapDataRow, int, Color64>? RowGetColor64 { get; set; }
        public Action<ICustomBitmapDataRow, int, Color64>? RowSetColor64 { get; set; }
        public Func<ICustomBitmapDataRow, int, PColor64>? RowGetPColor64 { get; set; }
        public Action<ICustomBitmapDataRow, int, PColor64>? RowSetPColor64 { get; set; }
        public Func<ICustomBitmapDataRow, int, ColorF>? RowGetColorF { get; set; }
        public Action<ICustomBitmapDataRow, int, ColorF>? RowSetColorF { get; set; }
        public Func<ICustomBitmapDataRow, int, PColorF>? RowGetPColorF { get; set; }
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

        internal override bool CanWrite() => (RowSetColor32 ?? RowSetPColor32 ?? RowSetColor64 ?? RowSetPColor64 ?? RowSetColorF ?? RowSetPColorF ?? RowSetColorLegacy) != null;

        #endregion
    }
}
