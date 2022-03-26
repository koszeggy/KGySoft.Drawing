#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: Accessors.cs
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
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Reflection;
using System.Threading;

using KGySoft.Collections;
using KGySoft.Reflection;

#endregion

namespace KGySoft.Drawing
{
    // ReSharper disable InconsistentNaming
    internal static class Accessors
    {
        #region Fields

        private static readonly LockFreeCacheOptions cacheProfile128 = new LockFreeCacheOptions { ThresholdCapacity = 128, HashingStrategy = HashingStrategy.And, MergeInterval = TimeSpan.FromSeconds(1) };

        private static IThreadSafeCacheAccessor<(Type DeclaringType, Type? FieldType, string? FieldNamePattern), FieldAccessor?>? fields;

        #endregion

        #region Methods

        #region Internal Methods

        #region Graphics

        internal static Image? GetBackingImage(this Graphics graphics) => graphics.GetFieldValueOrDefault<Image?>("backingImage");

        #endregion

        #region Icon
        
        internal static bool HasIconData(this Icon icon) => (typeof(Icon).GetField(null, "iconData")
            ?? typeof(Icon).GetField(null, "imageData"))?.Get(icon) != null;

        #endregion

        #region ColorPalette
        
        internal static bool TrySetEntries(this ColorPalette palette, Color[] value) => palette.TrySetFieldValue("entries", value);
        internal static void SetFlags(this ColorPalette palette, int value) => palette.TrySetFieldValue("flags", value);

        #endregion

        #endregion

        #region Private Methods

        private static FieldAccessor? GetField(this Type type, Type? fieldType, string? fieldNamePattern)
        {
            #region Local Methods

            // Fields are meant to be used for non-visible members either by type or name pattern (or both)
            static FieldAccessor? GetFieldAccessor((Type DeclaringType, Type? FieldType, string? FieldNamePattern) key)
            {
                for (Type t = key.DeclaringType; t != typeof(object); t = t.BaseType!)
                {
                    FieldInfo[] fieldsOfT = t.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                    FieldInfo? field = fieldsOfT.FirstOrDefault(f => (key.FieldType == null || f.FieldType == key.FieldType) && f.Name == key.FieldNamePattern) // exact name first
                        ?? fieldsOfT.FirstOrDefault(f => (key.FieldType == null || f.FieldType == key.FieldType)
                            && (key.FieldNamePattern == null || f.Name.Contains(key.FieldNamePattern, StringComparison.OrdinalIgnoreCase)));

                    if (field != null)
                        return FieldAccessor.GetAccessor(field);
                }

                return null;
            }

            #endregion

            if (fields == null)
                Interlocked.CompareExchange(ref fields, ThreadSafeCacheFactory.Create<(Type, Type?, string?), FieldAccessor?>(GetFieldAccessor, cacheProfile128), null);
            return fields[(type, fieldType, fieldNamePattern)];
        }

        private static T? GetFieldValueOrDefault<T>(this object obj, string? fieldNamePattern = null)
        {
            FieldAccessor? field = GetField(obj.GetType(), typeof(T), fieldNamePattern);
            return field == null ? default : (T)field.Get(obj)!;
        }

        private static bool TrySetFieldValue<T>(this object obj, string? fieldNamePattern, T value)
        {
            Type type = obj.GetType();
            FieldAccessor? field = GetField(type, typeof(T), fieldNamePattern);
            if (field == null)
                return false;

#if NETSTANDARD2_0
            if (field.IsReadOnly || field.MemberInfo.DeclaringType?.IsValueType == true)
            {
                ((FieldInfo)field.MemberInfo).SetValue(obj, value);
                return true;
            }
#endif

            field.Set(obj, value);
            return true;
        }

        #endregion


        #endregion
    }
}
