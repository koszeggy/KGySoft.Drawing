#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: BitmapDataRowArgb32.cs
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

namespace KGySoft.Drawing.Imaging
{
    internal sealed class BitmapDataRowArgb32 : BitmapDataRowBaseNonIndexed
    {
        #region Fields

        private unsafe Color32* row;

        #endregion

        #region Properties

        internal override unsafe byte* Address
        {
            get => (byte*)row;
            set => row = (Color32*)value;
        }

        #endregion

        #region Methods

        protected override unsafe Color32 DoGetColor32(int x) => row[x];

        protected override unsafe Color32 DoSetColor32(int x, Color32 c)
        {
            row[x] = c;
            return c;
        }

        #endregion
    }
}