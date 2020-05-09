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
using System.Security;

#endregion

namespace KGySoft.Drawing.Imaging
{
    internal abstract class BitmapDataRowBase : IBitmapDataRowInternal
    {
        #region Fields

        internal BitmapDataAccessorBase Accessor;
        
        [SecurityCritical]
        internal unsafe byte* Address;

        #endregion

        #region Properties and Indexers

        #region Properties

        public int Index { get; internal set; }

        #endregion

        #region Indexers

        public Color32 this[int x]
        {
#if !NET35
            [SecuritySafeCritical]
#endif
            get
            {
                if ((uint)x >= Accessor.Width)
                    ThrowXOutOfRange();
                return DoGetColor32(x);
            }
#if !NET35
            [SecuritySafeCritical]
#endif
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

#if !NET35
        [SecuritySafeCritical]
#endif
        public int GetColorIndex(int x)
        {
            if ((uint)x >= Accessor.Width)
                ThrowXOutOfRange();
            return DoGetColorIndex(x);
        }

#if !NET35
        [SecuritySafeCritical]
#endif
        public virtual void SetColorIndex(int x, int colorIndex)
        {
            if ((uint)x >= Accessor.Width)
                ThrowXOutOfRange();
            DoSetColorIndex(x, colorIndex);
        }

#if !NET35
        [SecuritySafeCritical]
#endif
        public unsafe T ReadRaw<T>(int x)
            where T : unmanaged
        {
            if ((x + 1) * sizeof(T) > Accessor.RowSize)
                ThrowXOutOfRange();
            return DoReadRaw<T>(x);
        }

#if !NET35
        [SecuritySafeCritical]
#endif
        public unsafe void WriteRaw<T>(int x, T data)
            where T : unmanaged
        {
            if ((x + 1) * sizeof(T) > Accessor.RowSize)
                ThrowXOutOfRange();
            DoWriteRaw(x, data);
        }

#if !NET35
        [SecuritySafeCritical]
#endif
        public unsafe bool MoveNextRow()
        {
            if (Index == Accessor.Height - 1)
                return false;
            Index += 1;
            Address += Accessor.Stride;
            return true;
        }

        [SecurityCritical]
        public abstract Color32 DoGetColor32(int x);

        [SecurityCritical]
        public abstract void DoSetColor32(int x, Color32 c);

        [SecurityCritical]
        public unsafe T DoReadRaw<T>(int x) where T : unmanaged => ((T*)Address)[x];

        [SecurityCritical]
        public unsafe void DoWriteRaw<T>(int x, T data) where T : unmanaged => ((T*)Address)[x] = data;

        #endregion

        #region Internal Methods

        [SecurityCritical]
        internal abstract int DoGetColorIndex(int x);

        [SecurityCritical]
        internal abstract void DoSetColorIndex(int x, int colorIndex);

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
