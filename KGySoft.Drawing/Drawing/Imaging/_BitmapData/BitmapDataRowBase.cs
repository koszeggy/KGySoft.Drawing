#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: BitmapDataRowBase.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2019 - All Rights Reserved
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
#if !(NET35 || NET40)
using System.Security;
#endif

#endregion

namespace KGySoft.Drawing.Imaging
{
    internal abstract class BitmapDataRowBase : IBitmapDataRow
    {
        #region Fields

        internal BitmapDataAccessorBase Accessor;
        internal unsafe byte* Address;
        internal int Line;

        #endregion

        #region Properties and Indexers

        #region Properties

        unsafe IntPtr IBitmapDataRow.Address
        {
#if !(NET35 || NET40)
            [SecuritySafeCritical] 
#endif
            get => (IntPtr)Address;
        }

        int IBitmapDataRow.Index => Line;

        #endregion

        #region Indexers

        public Color32 this[int x]
        {
            get
            {
                if ((uint)x >= Accessor.Width)
                    ThrowXOutOfRange();
                return DoGetColor32(x);
            }
            set
            {
                if ((uint)x >= Accessor.Width)
                    ThrowXOutOfRange();
                DoSetColor32(x, value);
            }
        }

        #endregion

        #endregion

        #region Methods

        #region Public Methods

        public Color GetColor(int x) => this[x].ToColor();

        public void SetColor(int x, Color color) => this[x] = new Color32(color);

        public int GetColorIndex(int x)
        {
            if ((uint)x >= Accessor.Width)
                ThrowXOutOfRange();
            return DoGetColorIndex(x);
        }

        public virtual void SetColorIndex(int x, int colorIndex)
        {
            if ((uint)x >= Accessor.Width)
                ThrowXOutOfRange();
            DoSetColorIndex(x, colorIndex);
        }

        public unsafe T ReadRaw<T>(int x)
            where T : unmanaged
        {
            if ((x + 1) * sizeof(T) > Accessor.Stride)
                ThrowXOutOfRange();
            return DoReadRaw<T>(x);
        }

        public unsafe void WriteRaw<T>(int x, T data)
            where T : unmanaged
        {
            if ((x + 1) * sizeof(T) > Accessor.Stride)
                ThrowXOutOfRange();
            DoWriteRaw(x, data);
        }

        public unsafe bool MoveNextRow()
        {
            if (Line == Accessor.Height - 1)
                return false;
            Line += 1;
            Address += Accessor.Stride;
            return true;
        }

        #endregion

        #region Internal Methods

        internal abstract Color32 DoGetColor32(int x);
        internal abstract void DoSetColor32(int x, Color32 c);
        internal abstract int DoGetColorIndex(int x);
        internal abstract void DoSetColorIndex(int x, int colorIndex);

        internal unsafe T DoReadRaw<T>(int x) where T : unmanaged => ((T*)Address)[x];
        internal unsafe void DoWriteRaw<T>(int x, T data) where T : unmanaged => ((T*)Address)[x] = data;

        #endregion

        #region Private Methods

        private static void ThrowXOutOfRange()
        {
#pragma warning disable CA2208
            // ReSharper disable once NotResolvedInText
            throw new ArgumentOutOfRangeException("x", PublicResources.ArgumentOutOfRange);
#pragma warning restore CA2208
        }

        #endregion

        #endregion
    }
}
