#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: BitmapDataRowBase.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2020 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution. If not, then this file is considered as
//  an illegal copy.
//
//  Unauthorized copying of this file, via any medium is strictly prohibited.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Security;

#endregion

namespace KGySoft.Drawing.Imaging
{
    internal abstract class BitmapDataRowBase : IBitmapDataRowInternal
    {
        #region Fields

        internal BitmapDataBase BitmapData;

        #endregion

        #region Properties and Indexers

        #region Properties

        #region Public Properties

        public int Index { get; internal set; }

        #endregion

        #endregion

        #region Indexers

        public Color32 this[int x]
        {
            [MethodImpl(MethodImpl.AggressiveInlining)]
            get
            {
                if ((uint)x >= BitmapData.Width)
                    ThrowXOutOfRange();
                return DoGetColor32(x);
            }
            [MethodImpl(MethodImpl.AggressiveInlining)]
            set
            {
                if ((uint)x >= BitmapData.Width)
                    ThrowXOutOfRange();
                DoSetColor32(x, value);
            }
        }

        #endregion

        #endregion

        #region Methods

        #region Static Methods

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowXOutOfRange()
        {
#pragma warning disable CA2208
            // ReSharper disable once NotResolvedInText
            throw new ArgumentOutOfRangeException("x", PublicResources.ArgumentOutOfRange);
#pragma warning restore CA2208
        }

        #endregion

        #region Instance Methods

        #region Public Methods

        public Color GetColor(int x) => this[x].ToColor();

        public void SetColor(int x, Color color) => this[x] = new Color32(color);

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public int GetColorIndex(int x)
        {
            if ((uint)x >= BitmapData.Width)
                ThrowXOutOfRange();
            return DoGetColorIndex(x);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public virtual void SetColorIndex(int x, int colorIndex)
        {
            if ((uint)x >= BitmapData.Width)
                ThrowXOutOfRange();
            DoSetColorIndex(x, colorIndex);
        }

        [SecuritySafeCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public unsafe T ReadRaw<T>(int x)
            where T : unmanaged
        {
            if ((x + 1) * sizeof(T) > BitmapData.RowSize)
                ThrowXOutOfRange();
            return DoReadRaw<T>(x);
        }

        [SecuritySafeCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public unsafe void WriteRaw<T>(int x, T data)
            where T : unmanaged
        {
            if ((x + 1) * sizeof(T) > BitmapData.RowSize)
                ThrowXOutOfRange();
            DoWriteRaw(x, data);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public virtual bool MoveNextRow()
        {
            if (Index == BitmapData.Height - 1)
                return false;
            Index += 1;
            return true;
        }

        public abstract Color32 DoGetColor32(int x);
        public abstract void DoSetColor32(int x, Color32 c);
        public abstract T DoReadRaw<T>(int x) where T : unmanaged;
        public abstract void DoWriteRaw<T>(int x, T data) where T : unmanaged;
        public virtual int DoGetColorIndex(int x) => throw new InvalidOperationException(Res.ImagingInvalidOperationIndexedOnly);
        public virtual void DoSetColorIndex(int x, int colorIndex) => throw new InvalidOperationException(Res.ImagingInvalidOperationIndexedOnly);

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public virtual Color32 DoGetColor32Premultiplied(int x) => DoGetColor32(x).ToPremultiplied();

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public virtual void DoSetColor32Premultiplied(int x, Color32 c) => DoSetColor32(x, c.ToStraight());

        #endregion

        #endregion

        #endregion
    }
}
