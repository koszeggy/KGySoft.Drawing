#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: BitmapDataRow32PArgb.cs
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

using System;

namespace KGySoft.Drawing.Imaging
{
    internal sealed class BitmapDataRow32PArgb : BitmapDataRowNonIndexedBase
    {
        private static readonly byte[][] premultipliedTable = InitPremultipliedTable();
        private static readonly byte[][] premultipliedTableInverse = InitPremultipliedTableInverse();

        private static byte[][] InitPremultipliedTable()
        {
            var table = new byte[256][];
            for (int i = 0; i < 256; i++)
            {
                table[i] = new byte[256];
                for (int a = 0; a < 256; a++)
                    table[i][a] = (byte)(i * a / 255);
            }

            return table;
        }

        private static byte[][] InitPremultipliedTableInverse()
        {
            var table = new byte[256][];
            for (int i = 0; i < 256; i++)
            {
                table[i] = new byte[256];
                for (int a = 0; a < 256; a++)
                    table[i][a] = a == 0 ? (byte)0 : (byte)Math.Min(255, i * 255 / a);
            }

            return table;
        }


        #region Methods

        #region Static Methods

        private static Color32 ToPremultiplied(byte a, Color32 c)
        {
            static byte Transform(byte alpha, byte channel) => (byte)(channel * alpha / 255);

            //return a == 0
            //    ? default
            //    : new Color32(a,
            //        Transform(a, c.R),
            //        Transform(a, c.G),
            //        Transform(a, c.B));
            return new Color32(a,
                premultipliedTable[c.R][a],
                premultipliedTable[c.G][a],
                premultipliedTable[c.B][a]);
        }

        private static Color32 ToStraight(Color32 c)
        {
            static byte Transform(byte alpha, byte channel) => (byte)(channel * 255 / alpha);

            byte a = c.A;
            //return a == 0
            //    ? c
            //    : new Color32(a,
            //        Transform(a, c.R),
            //        Transform(a, c.G),
            //        Transform(a, c.B));
            return new Color32(a,
                premultipliedTableInverse[c.R][a],
                premultipliedTableInverse[c.G][a],
                premultipliedTableInverse[c.B][a]);
        }

        #endregion

        #region Instance Methods

        internal override unsafe Color32 DoGetColor32(int x)
        {
            Color32 result = ((Color32*)Address)[x];
            if (result.A == 255)
                return result;
            return ToStraight(result);
        }

        internal override unsafe void DoSetColor32(int x, Color32 c)
        {
            byte a = c.A;
            if (a == 255)
            {
                ((Color32*)Address)[x] = c;
                return;
            }

            // premultiplication needed
            c = ToPremultiplied(a, c);
            ((Color32*)Address)[x] = c;
        }

        #endregion

        #endregion
    }
}
