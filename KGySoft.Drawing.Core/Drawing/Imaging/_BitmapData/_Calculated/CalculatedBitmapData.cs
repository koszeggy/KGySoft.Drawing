#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: CalculatedBitmapData.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2026 - All Rights Reserved
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
using System.Security;

#endregion

namespace KGySoft.Drawing.Imaging
{
    internal class CalculatedBitmapData : BitmapDataBase
    {
        #region Nested classes

        #region Row class

        private sealed class Row : BitmapDataRowBase
        {
            #region Fields

            private readonly Func<int, int, Color32> getColor32;
            private readonly Func<int, int, Color64> getColor64;
            private readonly Func<int, int, ColorF> getColorF;

            #endregion

            #region Constructors

            public Row(Func<int, int, Color32> getColor32, Func<int, int, Color64> getColor64, Func<int, int, ColorF> getColorF)
            {
                this.getColor32 = getColor32;
                this.getColor64 = getColor64;
                this.getColorF = getColorF;
            }

            #endregion

            #region Methods

            #region Public Methods

            [SecurityCritical]public override Color32 DoGetColor32(int x) => getColor32.Invoke(x, Index);
            [SecurityCritical]public override Color64 DoGetColor64(int x) => getColor64.Invoke(x, Index);
            [SecurityCritical]public override ColorF DoGetColorF(int x) => getColorF.Invoke(x, Index);
            [SecurityCritical]public override void DoSetColor32(int x, Color32 c) => throw new NotSupportedException(PublicResources.NotSupported);
            [SecurityCritical]public override T DoReadRaw<T>(int x) => throw new NotSupportedException(PublicResources.NotSupported);
            [SecurityCritical]public override void DoWriteRaw<T>(int x, T data) => throw new NotSupportedException(PublicResources.NotSupported);

            #endregion

            #region Protected Methods

            protected override void DoMoveToIndex()
            {
            }

            #endregion

            #endregion
        }

        #endregion

        #endregion

        #region Fields

        private readonly Func<int, int, Color32> getColor32;
        private readonly Func<int, int, Color64> getColor64;
        private readonly Func<int, int, ColorF> getColorF;

        #endregion

        #region Constructors

        internal CalculatedBitmapData(Size size, Func<int, int, Color32> getColor, Action? disposeCallback)
            : base(new BitmapDataConfig(size, KnownPixelFormat.Format32bppArgb.ToInfoInternal(), disposeCallback: disposeCallback))
        {
            getColor32 = getColor;
            getColor64 = (x, y) => getColor.Invoke(x, y).ToColor64();
            getColorF = (x, y) => getColor.Invoke(x, y).ToColorF();
        }

        internal CalculatedBitmapData(Size size, Func<int, int, Color64> getColor, Action? disposeCallback)
            : base(new BitmapDataConfig(size, KnownPixelFormat.Format64bppArgb.ToInfoInternal(), disposeCallback: disposeCallback))
        {
            getColor32 = (x, y) => getColor.Invoke(x, y).ToColor32();
            getColor64 = getColor;
            getColorF = (x, y) => getColor.Invoke(x, y).ToColorF();
        }

        internal CalculatedBitmapData(Size size, Func<int, int, ColorF> getColor, Action? disposeCallback)
            : base(new BitmapDataConfig(size, KnownPixelFormat.Format128bppRgba.ToInfoInternal(), disposeCallback: disposeCallback))
        {
            getColor32 = (x, y) => getColor.Invoke(x, y).ToColor32();
            getColor64 = (x, y) => getColor.Invoke(x, y).ToColor64();
            getColorF = getColor;
        }

        #endregion

        #region Methods

        #region Public Methods

        [SecurityCritical]public override Color32 DoGetColor32(int x, int y) => getColor32.Invoke(x, y);
        [SecurityCritical]public override Color64 DoGetColor64(int x, int y) => getColor64.Invoke(x, y);
        [SecurityCritical]public override ColorF DoGetColorF(int x, int y) => getColorF.Invoke(x, y);
        [SecurityCritical]public override void DoSetColor32(int x, int y, Color32 c) => throw new NotSupportedException(PublicResources.NotSupported);
        [SecurityCritical]public override T DoReadRaw<T>(int x, int y) => throw new NotSupportedException(PublicResources.NotSupported);
        [SecurityCritical]public override void DoWriteRaw<T>(int x, int y, T data) => throw new NotSupportedException(PublicResources.NotSupported);

        #endregion

        #region Protected Methods

        protected override IBitmapDataRowInternal DoGetRow(int y) => new Row(getColor32, getColor64, getColorF)
        {
            BitmapData = this,
            Index = y,
        };

        #endregion

        #endregion
    }
}
