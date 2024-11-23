#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: CastArrayExtensions.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2024 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System;

using KGySoft.Collections;

#endregion

namespace KGySoft.Drawing
{
    internal static class CastArrayExtensions
    {
        #region Methods

        internal static void Sort<TBase, T>(this CastArray<TBase, T> array)
            where TBase : unmanaged
            where T : unmanaged
        {
            // TODO: an optionally parallel sort, especially if it's faster than the original
#if NET5_0_OR_GREATER
            array.AsSpan.Sort();
#else
            if (array.Length < 2)
                return;

            T[] temp = array.ToArray()!;
            Array.Sort(temp);
            for (int i = 0; i < array.Length; i++)
                array.SetElementUnsafe(i, temp[i]);
#endif
        }

        internal static void Sort<TKeyBase, TKey, TItemBase, TItem>(this CastArray<TKeyBase, TKey> keys, CastArray<TItemBase, TItem> items)
            where TKeyBase : unmanaged
            where TKey : unmanaged
            where TItemBase : unmanaged
            where TItem : unmanaged
        {
#if NET5_0_OR_GREATER
            keys.AsSpan.Sort(items.AsSpan);
#else
            if (keys.Length < 2)
                return;
            TItem[] itemsArr = items.ToArray()!;
            TKey[] keysArr = keys.ToArray()!;
            Array.Sort(keysArr, itemsArr);
            for (int i = 0; i < items.Length; i++)
            {
                keys.SetElementUnsafe(i, keysArr[i]);
                items.SetElementUnsafe(i, itemsArr[i]);
            }
#endif
        }

        #endregion
    }
}
