#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: BitmapDataRowPArgb32.cs
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
    internal sealed class BitmapDataRowPArgb32 : BitmapDataRowBaseNonIndexed
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

        #region Static Methods

        private static Color32 ToPremultiplied(byte a, Color32 c)
        {
            static byte Transform(byte alpha, byte channel) => (byte)(channel * alpha / 255);

            return a == 0
                ? default
                : new Color32(a,
                    Transform(a, c.R),
                    Transform(a, c.G),
                    Transform(a, c.B));
        }

        private static Color32 ToStraight(Color32 c)
        {
            static byte Transform(byte alpha, byte channel) => (byte)(channel * 255 / alpha);

            byte a = c.A;
            return a == 0
                ? c
                : new Color32(a,
                    Transform(a, c.R),
                    Transform(a, c.G),
                    Transform(a, c.B));
        }

        #endregion

        #region Instance Methods

        protected override unsafe Color32 DoGetColor32(int x)
        {
            Color32 result = row[x];
            if (result.A == 255)
                return result;
            return ToStraight(result);
        }

        protected override unsafe Color32 DoSetColor32(int x, Color32 c)
        {
            byte a = c.A;
            if (a == 255)
            {
                row[x] = c;
                return c;
            }

            // premultiplication needed
            c = ToPremultiplied(a, c);
            row[x] = c;

            // returning the color that can be restored from premultiplied color
            return ToStraight(c);
        }

        #endregion

        #endregion
    }
}
