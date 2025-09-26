#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ArraySectionExtensions.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2025 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System;
using System.Runtime.CompilerServices;

using KGySoft.Collections;

#endregion

namespace KGySoft.Drawing
{
    internal static class ArraySectionExtensions
    {
        #region Methods

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static unsafe CastArray<byte, T> Allocate<T>(this ref ArraySection<byte> buffer, int elementCount)
            where T : unmanaged
        {
            ArraySection<byte> result = buffer.Slice(0, elementCount * sizeof(T));
            buffer = buffer.Slice(result.Length);
            return result.Cast<byte, T>();
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static void SetBitRange(this ref ArraySection<byte> buffer, int startIndex, int endIndex)
        {
            Debug.Assert(endIndex >= startIndex && startIndex >= 0 && (endIndex >> 3) < buffer.Length);

            int maskPos = startIndex >> 3;
            int endMaskPos = endIndex >> 3;

            // up to 8 pixels on the same byte
            if (maskPos == endMaskPos)
            {
                buffer.GetElementReferenceUnchecked(maskPos).SetBitRange(startIndex, endIndex);
                return;
            }

            // first partial byte
            if ((startIndex & 7) != 0)
            {
                buffer.GetElementReferenceUnchecked(maskPos).SetBitRange(startIndex, ((maskPos + 1) << 3) - 1);
                maskPos += 1;
            }

            // whole bytes
            switch (endMaskPos - maskPos)
            {
                case > 1:
                    buffer.Slice(maskPos, endMaskPos - maskPos).Fill(Byte.MaxValue);
                    break;
                case 1:
                    buffer.SetElementUnchecked(maskPos, Byte.MaxValue);
                    break;
            }

            // last [partial] byte
            if ((endIndex & 7) != 7)
                buffer.GetElementReferenceUnchecked(endMaskPos).SetBitRange(endMaskPos << 3, endIndex);
            else
                buffer.SetElementUnchecked(endMaskPos, Byte.MaxValue);
        }

        #endregion
    }
}