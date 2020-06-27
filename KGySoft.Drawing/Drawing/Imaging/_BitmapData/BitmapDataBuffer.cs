#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: BitmapDataBuffer.cs
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
using System.Drawing.Imaging;
using System.Runtime.CompilerServices;
using System.Security;

using KGySoft.Collections;

#endregion

namespace KGySoft.Drawing.Imaging
{
    /// <summary>
    /// Represents a managed bitmap data buffer for intermediate operations
    /// with a fixed <see cref="System.Drawing.Imaging.PixelFormat.Format32bppArgb"/> pixel format.
    /// </summary>
    internal class BitmapDataBuffer : IBitmapDataInternal
    {
        #region Nested classes

        #region BitmapDataRowBuffer class

        private sealed class BitmapDataRowBuffer : IBitmapDataRowInternal
        {
            #region Fields

            internal BitmapDataBuffer Owner;
            internal ArraySection<Color32> Row;

            #endregion

            #region Properties and Indexers

            #region Properties

            public int Index { get; internal set; }

            #endregion

            #region Indexers

            public Color32 this[int x]
            {
                [MethodImpl(MethodImpl.AggressiveInlining)]
                get
                {
                    if ((uint)x >= Owner.Width)
                        ThrowXOutOfRange();
                    return DoGetColor32(x);
                }
                [MethodImpl(MethodImpl.AggressiveInlining)]
                set
                {
                    if ((uint)x >= Owner.Width)
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

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public bool MoveNextRow()
            {
                if (Index == Owner.Height - 1)
                    return false;
                Index += 1;
                Row = Owner.buffer[Index];
                return true;
            }

            public void SetColor(int x, Color color) => this[x] = new Color32(color);

            public void SetColorIndex(int x, int colorIndex) => throw new InvalidOperationException(Res.ImagingInvalidOperationIndexedOnly);

            [SecuritySafeCritical]
            [MethodImpl(MethodImpl.AggressiveInlining)]
            public unsafe T ReadRaw<T>(int x) where T : unmanaged
            {
                if ((x + 1) * sizeof(T) > Owner.RowSize)
                    ThrowXOutOfRange();
                return DoReadRaw<T>(x);
            }

            [SecuritySafeCritical]
            [MethodImpl(MethodImpl.AggressiveInlining)]
            public unsafe void WriteRaw<T>(int x, T data) where T : unmanaged
            {
                if ((x + 1) * sizeof(T) > Owner.RowSize)
                    ThrowXOutOfRange();
                DoWriteRaw(x, data);
            }

            public Color GetColor(int x) => this[x].ToColor();

            public int GetColorIndex(int x) => throw new InvalidOperationException(Res.ImagingInvalidOperationIndexedOnly);

            public Color32 DoGetColor32(int x) => Row[x];

            public void DoSetColor32(int x, Color32 c) => Row[x] = c;

            [SecurityCritical]
            [MethodImpl(MethodImpl.AggressiveInlining)]
            public T DoReadRaw<T>(int x) where T : unmanaged
            {
#if NETFRAMEWORK
                unsafe
                {
                    fixed (Color32* pRow = Row)
                        return ((T*)pRow)[x];
                }
#else
                return Unsafe.Add(ref Unsafe.As<Color32, T>(ref Row.GetPinnableReference()), x);
#endif
            }

            [SecurityCritical]
            [MethodImpl(MethodImpl.AggressiveInlining)]
            public void DoWriteRaw<T>(int x, T data) where T : unmanaged
            {
#if NETFRAMEWORK
                unsafe
                {
                    fixed (Color32* pRow = Row)
                        ((T*)pRow)[x] = data;
                }
#else
                Unsafe.Add(ref Unsafe.As<Color32, T>(ref Row.GetPinnableReference()), x) = data;
#endif
            }

            #endregion

            #endregion
        }

        #endregion

        #endregion

        #region Fields

        private Array2D<Color32> buffer;
        private BitmapDataRowBuffer lastRow;

        #endregion

        #region Properties and Indexers

        #region Properties

        #region Public Properties

        public int Height => buffer.Height;
        public int Width => buffer.Width;
        public PixelFormat PixelFormat => PixelFormat.Format32bppArgb;
        public Palette Palette => null;
        public int RowSize => Width << 2;
        public Color32 BackColor { get; }
        public byte AlphaThreshold { get; }

        #endregion

        #region Explicitly Implemented Interface Properties

        IReadableBitmapDataRow IReadableBitmapData.FirstRow => GetRow(0);
        IReadWriteBitmapDataRow IReadWriteBitmapData.FirstRow => GetRow(0);
        IWritableBitmapDataRow IWritableBitmapData.FirstRow => GetRow(0);

        #endregion

        #endregion

        #region Indexers

        IReadWriteBitmapDataRow IReadWriteBitmapData.this[int y]
        {
            get
            {
                if ((uint)y >= Height)
                    ThrowYOutOfRange();
                return GetRow(y);
            }
        }

        IReadableBitmapDataRow IReadableBitmapData.this[int y] => ((IReadWriteBitmapData)this)[y];
        IWritableBitmapDataRow IWritableBitmapData.this[int y] => ((IReadWriteBitmapData)this)[y];

        #endregion

        #endregion

        #region Constructors

        internal BitmapDataBuffer(Size size, Color32 backColor = default, byte alphaThreshold = 128)
        {
            Debug.Assert(size.Width > 0 && size.Height > 0, "Non-empty size expected");
            buffer = new Array2D<Color32>(size.Height, size.Width);
            BackColor = backColor.ToOpaque();
            AlphaThreshold = alphaThreshold;
        }

        #endregion

        #region Methods

        #region Static Methods

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowYOutOfRange()
        {
#pragma warning disable CA2208
            // ReSharper disable once NotResolvedInText
            throw new ArgumentOutOfRangeException("y", PublicResources.ArgumentOutOfRange);
#pragma warning restore CA2208
        }

        #endregion

        #region Instance Methods

        public IBitmapDataRowInternal GetRow(int y)
        {
            // If the same row is accessed repeatedly we return the cached last row.
            BitmapDataRowBuffer result = lastRow;
            if (result?.Index == y)
                return result;

            // Otherwise, we create and cache the result.
            result = new BitmapDataRowBuffer
            {
                Row = buffer[y],
                Owner = this,
                Index = y,
            };

            return lastRow = result;
        }

        public Color GetPixel(int x, int y)
        {
            if ((uint)y >= Height)
                ThrowYOutOfRange();
            return GetRow(y).GetColor(x);
        }

        public void SetPixel(int x, int y, Color color)
        {
            if ((uint)y >= Height)
                ThrowYOutOfRange();
            GetRow(y).SetColor(x, color);
        }

        public void Dispose() => buffer.Dispose();

        #endregion

        #endregion
    }
}
