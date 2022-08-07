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
#if NET7_0_OR_GREATER
using System.Runtime.Versioning;
#endif
using System.Threading;

using KGySoft.Collections;
#if !NETCOREAPP3_0_OR_GREATER
using KGySoft.CoreLibraries;
#endif
using KGySoft.Reflection;

#endregion

namespace KGySoft.Drawing
{
#if NET7_0_OR_GREATER
    [SupportedOSPlatform("windows")]
#endif
    internal static class Accessors
    {
        #region Fields

        private static readonly LockFreeCacheOptions cacheProfile128 = new LockFreeCacheOptions { ThresholdCapacity = 128, HashingStrategy = HashingStrategy.And, MergeInterval = TimeSpan.FromSeconds(1) };

        private static IThreadSafeCacheAccessor<(Type DeclaringType, Type? FieldType, string? FieldNamePattern), FieldAccessor?>? fields;

        #endregion

        #region Methods

        #region Internal Methods

        #region Graphics

        internal static Image? GetBackingImage(this Graphics graphics) => GetFieldValueOrDefault<Graphics, Image?>(graphics, null, "backingImage");

        #endregion

        #region Icon
        
        internal static bool HasIconData(this Icon icon) => (typeof(Icon).GetField(null, "iconData")
            ?? typeof(Icon).GetField(null, "imageData"))?.Get(icon) != null;

        #endregion

        #region ColorPalette
        
        internal static bool TrySetEntries(this ColorPalette palette, Color[] value) => TrySetFieldValue(palette, "entries", value);
        internal static void SetFlags(this ColorPalette palette, int value) => TrySetFieldValue(palette, "flags", value);

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

        private static TField? GetFieldValueOrDefault<TInstance, TField>(TInstance obj, TField? defaultValue = default, string? fieldNamePattern = null)
            where TInstance : class
        {
            FieldAccessor? field = GetField(obj.GetType(), typeof(TField), fieldNamePattern);
            return field == null ? defaultValue : field.GetInstanceValue<TInstance, TField>(obj);
        }

        private static bool TrySetFieldValue<TInstance, TField>(TInstance obj, string? fieldNamePattern, TField value)
            where TInstance : class
        {
            Type type = obj.GetType();
            FieldAccessor? field = GetField(type, typeof(TField), fieldNamePattern);
            if (field == null)
                return false;

#if NETSTANDARD2_0
            if (field.IsReadOnly || field.MemberInfo.DeclaringType?.IsValueType == true)
            {
                ((FieldInfo)field.MemberInfo).SetValue(obj, value);
                return true;
            }
#endif

            field.SetInstanceValue(obj, value);
            return true;
        }

        #endregion

        #endregion
    }
}
