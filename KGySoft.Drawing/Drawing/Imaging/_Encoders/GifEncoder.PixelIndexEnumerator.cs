#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: GifEncoder.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2021 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System;

#endregion

namespace KGySoft.Drawing.Imaging
{
    public partial class GifEncoder
    {
        #region Nested structs

        #region PixelIndexEnumerator struct

        private struct PixelIndexEnumerator
        {
            #region Fields

            private readonly int maxX;
            private readonly IReadableBitmapDataRow row;

            private int x;

            #endregion

            #region Properties

            public byte Current => (byte)row.GetColorIndex(x);

            #endregion

            #region Constructors

            internal PixelIndexEnumerator(IReadableBitmapData bitmapData)
            {
                Debug.Assert(bitmapData.PixelFormat.IsIndexed());
                maxX = bitmapData.Width - 1;
                row = bitmapData.FirstRow;
                x = -1;
            }

            #endregion

            #region Methods

            public bool MoveNext()
            {
                // end of row
                if (x == maxX)
                {
                    // next row
                    if (row.MoveNextRow())
                    {
                        x = 0;
                        return true;
                    }

                    // end of enumeration
                    x = Int32.MinValue;
                    return false;
                }

                // next pixel within the current row (-1 due to the very first pixel)
                if (x >= -1)
                {
                    x += 1;
                    return true;
                }

                // after last
                return false;
            }

            #endregion
        }

        #endregion

        #endregion
    }
}
