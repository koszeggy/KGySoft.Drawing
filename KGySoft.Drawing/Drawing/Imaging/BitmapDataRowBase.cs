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

#endregion

namespace KGySoft.Drawing.Imaging
{
    internal abstract class BitmapDataRowBase : IBitmapDataRow
    {
        #region Fields

        internal IBitmapDataAccessor Accessor;
        internal int Line;

        #endregion

        #region Properties and Indexers

        #region Properties

        internal abstract unsafe byte* Address { get; set; }

        #endregion

        #region Indexers

        public Color this[int x]
        {
            get => GetPixelColor32(x);
            set => SetPixelColor(x, value);
        }

        #endregion

        #endregion

        #region Methods

        #region Public Methods

        public Color32 GetPixelColor32(int x)
        {
            if ((uint)x > Accessor.Width)
                ThrowXOutOfRange();
            return DoGetColor32(x);
        }

        public Color64 GetPixelColor64(int x)
        {
            if ((uint)x > Accessor.Width)
                ThrowXOutOfRange();
            return DoGetColor64(x);
        }

        public int GetPixelColorIndex(int x)
        {
            if ((uint)x > Accessor.Width)
                ThrowXOutOfRange();
            return DoGetColorIndex(x);
        }

        public Color32 SetPixelColor(int x, Color32 color)
        {
            if ((uint)x > Accessor.Width)
                ThrowXOutOfRange();
            return DoSetColor32(x, color);
        }

        public Color64 SetPixelColor(int x, Color64 color)
        {
            if ((uint)x > Accessor.Width)
                ThrowXOutOfRange();
            return DoSetColor64(x, color);
        }

        public void SetPixelColorIndex(int x, int colorIndex)
        {
            if ((uint)x > Accessor.Width)
                ThrowXOutOfRange();
            DoSetColorIndex(x, colorIndex);
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

        #region Protected Methods

        protected abstract Color32 DoGetColor32(int x);
        protected abstract Color32 DoSetColor32(int x, Color32 c);
        protected abstract Color64 DoGetColor64(int x);
        protected abstract Color64 DoSetColor64(int x, Color64 c);
        protected abstract int DoGetColorIndex(int x);
        protected abstract void DoSetColorIndex(int x, int colorIndex);

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
