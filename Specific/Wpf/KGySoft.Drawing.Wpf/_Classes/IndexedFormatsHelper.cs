#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: IndexedFormatsHelper.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2026 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System.Security;

using KGySoft.Drawing.Imaging;

#endregion

namespace KGySoft.Drawing.Wpf
{
    internal static class IndexedFormatsHelper
    {
        #region Methods

        internal static bool TrySetPalette(Palette _) => false;

        [SecuritySafeCritical]
        internal static int GetColorIndexI1(ICustomBitmapDataRow row, int x)
        {
            int bits = row.UnsafeGetRefAs<byte>(x >> 3);
            int mask = 128 >> (x & 7);
            return (bits & mask) != 0 ? 1 : 0;
        }

        [SecuritySafeCritical]
        internal static void SetColorIndexI1(ICustomBitmapDataRow row, int x, int colorIndex)
        {
            ref byte bits = ref row.UnsafeGetRefAs<byte>(x >> 3);
            int mask = 128 >> (x & 7);
            if (colorIndex == 0)
                bits &= (byte)~mask;
            else
                bits |= (byte)mask;
        }

        [SecuritySafeCritical]
        internal static int GetColorIndexI2(ICustomBitmapDataRow row, int x)
        {
            int bits = row.UnsafeGetRefAs<byte>(x >> 2);
            return (x & 3) switch
            {
                0 => bits >> 6,
                1 => (bits >> 4) & 3,
                2 => (bits >> 2) & 3,
                _ => bits & 3,
            };
        }

        [SecuritySafeCritical]
        internal static void SetColorIndexI2(ICustomBitmapDataRow row, int x, int colorIndex)
        {
            int pos = x >> 2;
            ref byte bits = ref row.UnsafeGetRefAs<byte>(pos);
            switch (x & 3)
            {
                case 0:
                    bits &= 0b00111111;
                    bits |= (byte)(colorIndex << 6);
                    break;
                case 1:
                    bits &= 0b11001111;
                    bits |= (byte)(colorIndex << 4);
                    break;
                case 2:
                    bits &= 0b11110011;
                    bits |= (byte)(colorIndex << 2);
                    break;
                default:
                    bits &= 0b11111100;
                    bits |= (byte)colorIndex;
                    break;
            }
        }

        [SecuritySafeCritical]
        internal static int GetColorIndexI4(ICustomBitmapDataRow row, int x)
        {
            int nibbles = row.UnsafeGetRefAs<byte>(x >> 1);
            return (x & 1) == 0
                ? nibbles >> 4
                : nibbles & 0b00001111;
        }

        [SecuritySafeCritical]
        internal static void SetColorIndexI4(ICustomBitmapDataRow row, int x, int colorIndex)
        {
            ref byte nibbles = ref row.UnsafeGetRefAs<byte>(x >> 1);
            if ((x & 1) == 0)
            {
                nibbles &= 0b00001111;
                nibbles |= (byte)(colorIndex << 4);
            }
            else
            {
                nibbles &= 0b11110000;
                nibbles |= (byte)colorIndex;
            }
        }

        #endregion
    }
}
