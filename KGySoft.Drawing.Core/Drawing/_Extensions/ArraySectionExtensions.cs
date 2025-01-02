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

using KGySoft.Collections;

#endregion

namespace KGySoft.Drawing
{
    internal static class ArraySectionExtensions
    {
        #region Methods

        internal static unsafe CastArray<byte, T> Allocate<T>(this ref ArraySection<byte> buffer, int elementCount)
            where T : unmanaged
        {
            ArraySection<byte> result = buffer.Slice(0, elementCount * sizeof(T));
            buffer = buffer.Slice(result.Length);
            return result.Cast<byte, T>();
        }

        #endregion
    }
}