#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: BitmapDataRowBase.cs
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
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Security;

#endregion

namespace KGySoft.Drawing.Imaging
{
    internal abstract class BitmapDataRowBase : IBitmapDataRowInternal
    {
        #region Properties and Indexers

        #region Properties
        
        #region Public Properties

        public int Index { get; internal set; }
        public int Width => BitmapData.Width;
        public int Size => BitmapData.RowSize;

        #endregion

        #region Internal Properties

        [AllowNull]internal BitmapDataBase BitmapData { get; set; }

        #endregion

        #region Explicitly Implemented Interface Properties

        IBitmapData IBitmapDataRowInternal.BitmapData => BitmapData;

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
        protected static void ThrowXOutOfRange()
        {
            // ReSharper disable once NotResolvedInText
            throw new ArgumentOutOfRangeException("x", PublicResources.ArgumentOutOfRange);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowYOutOfRange()
        {
            // ReSharper disable once NotResolvedInText
            throw new ArgumentOutOfRangeException("y", PublicResources.ArgumentOutOfRange);
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

        public bool MoveNextRow()
        {
            if (Index == BitmapData.Height - 1)
                return false;
            Index += 1;
            DoMoveToIndex();
            return true;
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public void MoveToRow(int y)
        {
            if (y >= (uint)BitmapData.Height)
                ThrowYOutOfRange();
            DoMoveToRow(y);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public void DoMoveToRow(int y)
        {
            Index = y;
            DoMoveToIndex();
        }

        public abstract Color32 DoGetColor32(int x);
        public abstract void DoSetColor32(int x, Color32 c);
        public abstract T DoReadRaw<T>(int x) where T : unmanaged;
        public abstract void DoWriteRaw<T>(int x, T data) where T : unmanaged;
        public virtual int DoGetColorIndex(int x) => throw new InvalidOperationException(Res.ImagingInvalidOperationIndexedOnly);
        public virtual void DoSetColorIndex(int x, int colorIndex) => throw new InvalidOperationException(Res.ImagingInvalidOperationIndexedOnly);

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public virtual PColor32 DoGetColor32Premultiplied(int x) => DoGetColor32(x).ToPremultiplied();

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public virtual void DoSetColor32Premultiplied(int x, PColor32 c) => DoSetColor32(x, c.ToStraight());

        #endregion

        #region Protected Methods

        protected abstract void DoMoveToIndex();

        #endregion

        #endregion

        #endregion
    }
}
