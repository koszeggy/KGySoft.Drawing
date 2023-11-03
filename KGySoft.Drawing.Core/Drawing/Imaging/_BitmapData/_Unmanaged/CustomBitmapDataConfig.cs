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
    public sealed class CustomBitmapDataConfig : CustomNonIndexedBitmapDataConfigBase
    {
        #region Properties
        
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

        #region Methods

        internal Func<ICustomBitmapDataRow, int, Color32> GetRowGetColor32()
        {
            // Saving to local variables to prevent capturing this
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

        internal Action<ICustomBitmapDataRow, int, Color32> GetRowSetColor32()
        {
            // Saving to local variables to prevent capturing this
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

        internal Func<ICustomBitmapDataRow, int, PColor32> GetRowGetPColor32()
        {
            // Saving to local variables to prevent capturing this
            Func<ICustomBitmapDataRow, int, Color32>? getColor32 = RowGetColor32;
            Func<ICustomBitmapDataRow, int, PColor32>? getPColor32 = RowGetPColor32;
            Func<ICustomBitmapDataRow, int, Color64>? getColor64 = RowGetColor64;
            Func<ICustomBitmapDataRow, int, PColor64>? getPColor64 = RowGetPColor64;
            Func<ICustomBitmapDataRow, int, ColorF>? getColorF = RowGetColorF;
            Func<ICustomBitmapDataRow, int, PColorF>? getPColorF = RowGetPColorF;

            return getPColor32 ?? (getPColor64 != null ? (r, x) => getPColor64.Invoke(r, x).ToPColor32()
                : getColor32 != null ? (r, x) => getColor32.Invoke(r, x).ToPColor32()
                : getColor64 != null ? (r, x) => getColor64.Invoke(r, x).ToPColor32()
                : getColorF != null ? (r, x) => getColorF.Invoke(r, x).ToPColor32()
                : getPColorF != null ? (r, x) => getPColorF.Invoke(r, x).ToPColor32()
                : new Func<ICustomBitmapDataRow, int, PColor32>((_, _) => throw new InvalidOperationException(Res.ImagingCustomBitmapDataWriteOnly)));
        }

        internal Action<ICustomBitmapDataRow, int, PColor32> GetRowSetPColor32()
        {
            // Saving to local variables to prevent capturing this
            Action<ICustomBitmapDataRow, int, Color32>? setColor32 = RowSetColor32;
            Action<ICustomBitmapDataRow, int, PColor32>? setPColor32 = RowSetPColor32;
            Action<ICustomBitmapDataRow, int, Color64>? setColor64 = RowSetColor64;
            Action<ICustomBitmapDataRow, int, PColor64>? setPColor64 = RowSetPColor64;
            Action<ICustomBitmapDataRow, int, ColorF>? setColorF = RowSetColorF;
            Action<ICustomBitmapDataRow, int, PColorF>? setPColorF = RowSetPColorF;

            return setPColor32 ?? (setPColor64 != null ? (r, x, c) => setPColor64.Invoke(r, x, c.ToPColor64())
                : setColor32 != null ? (r, x, c) => setColor32.Invoke(r, x, c.ToColor32())
                : setColorF != null ? (r, x, c) => setColorF.Invoke(r, x, c.ToColorF())
                : setColor64 != null ? (r, x, c) => setColor64.Invoke(r, x, c.ToColor64())
                : setPColorF != null ? (r, x, c) => setPColorF.Invoke(r, x, c.ToPColorF())
                : new Action<ICustomBitmapDataRow, int, PColor32>((_, _, _) => throw new InvalidOperationException(Res.ImagingCustomBitmapDataReadOnly)));
        }

        internal Func<ICustomBitmapDataRow, int, Color64> GetRowGetColor64()
        {
            // Saving to local variables to prevent capturing this
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
                : getPColor32 != null ? (r, x) => getPColor32.Invoke(r, x).ToColor64()
                : new Func<ICustomBitmapDataRow, int, Color64>((_, _) => throw new InvalidOperationException(Res.ImagingCustomBitmapDataWriteOnly)));
        }

        internal Action<ICustomBitmapDataRow, int, Color64> GetRowSetColor64()
        {
            // Saving to local variables to prevent capturing this
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
                : setPColor32 != null ? (r, x, c) => setPColor32.Invoke(r, x, c.ToPColor32())
                : new Action<ICustomBitmapDataRow, int, Color64>((_, _, _) => throw new InvalidOperationException(Res.ImagingCustomBitmapDataReadOnly)));
        }

        internal Func<ICustomBitmapDataRow, int, PColor64> GetRowGetPColor64()
        {
            // Saving to local variables to prevent capturing this
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
                : new Func<ICustomBitmapDataRow, int, PColor64>((_, _) => throw new InvalidOperationException(Res.ImagingCustomBitmapDataWriteOnly)));
        }

        internal Action<ICustomBitmapDataRow, int, PColor64> GetRowSetPColor64()
        {
            // Saving to local variables to prevent capturing this
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
                : new Action<ICustomBitmapDataRow, int, PColor64>((_, _, _) => throw new InvalidOperationException(Res.ImagingCustomBitmapDataReadOnly)));
        }

        internal Func<ICustomBitmapDataRow, int, ColorF> GetRowGetColorF()
        {
            // Saving to local variables to prevent capturing this
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
                : getPColor32 != null ? (r, x) => getPColor32.Invoke(r, x).ToColorF()
                : new Func<ICustomBitmapDataRow, int, ColorF>((_, _) => throw new InvalidOperationException(Res.ImagingCustomBitmapDataWriteOnly)));
        }

        internal Action<ICustomBitmapDataRow, int, ColorF> GetRowSetColorF()
        {
            // Saving to local variables to prevent capturing this
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
                : setPColor32 != null ? (r, x, c) => setPColor32.Invoke(r, x, c.ToPColor32())
                : new Action<ICustomBitmapDataRow, int, ColorF>((_, _, _) => throw new InvalidOperationException(Res.ImagingCustomBitmapDataReadOnly)));
        }

        internal Func<ICustomBitmapDataRow, int, PColorF> GetRowGetPColorF()
        {
            // Saving to local variables to prevent capturing this
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
                : getPColor32 != null ? (r, x) => getPColor32.Invoke(r, x).ToPColorF()
                : new Func<ICustomBitmapDataRow, int, PColorF>((_, _) => throw new InvalidOperationException(Res.ImagingCustomBitmapDataWriteOnly)));
        }

        internal Action<ICustomBitmapDataRow, int, PColorF> GetRowSetPColorF()
        {
            // Saving to local variables to prevent capturing this
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
                : setPColor32 != null ? (r, x, c) => setPColor32.Invoke(r, x, c.ToPColor32())
                : new Action<ICustomBitmapDataRow, int, PColorF>((_, _, _) => throw new InvalidOperationException(Res.ImagingCustomBitmapDataReadOnly)));
        }

        #endregion
    }
}
