#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: Accessors.cs
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

#region Usings

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Reflection;
using System.Threading;

using KGySoft.Collections;
#if !NETCOREAPP3_0
using KGySoft.CoreLibraries; 
#endif
using KGySoft.Reflection;

#endregion

namespace KGySoft.Drawing
{
    // ReSharper disable InconsistentNaming
    internal static class Accessors
    {
        #region Fields

        private static IThreadSafeCacheAccessor<(Type DeclaringType, Type FieldType, string FieldNamePattern), FieldAccessor> fields;

        #endregion

        #region Methods

        #region Internal Methods

        #region Exception

#if NET35 || NET40
        internal static string GetSource(this Exception exception) => GetFieldValueOrDefault<string>(exception, "source");
        internal static void SetSource(this Exception exception, string value) => SetFieldValue(exception, "source", value, false);
        internal static void SetRemoteStackTraceString(this Exception exception, string value) => SetFieldValue(exception, "remoteStackTraceString", value, false);
        internal static void InternalPreserveStackTrace(this Exception exception) => Reflector.TryInvokeMethod(exception, "InternalPreserveStackTrace", out var _);
#endif

        #endregion

        #region Graphics

        internal static Image GetBackingImage(this Graphics graphics) => GetFieldValueOrDefault<Image>(graphics, "backingImage");

        #endregion

        #region Icon
        
        internal static bool HasIconData(this Icon icon) => (GetField(typeof(Icon), null, "iconData")
            ?? GetField(typeof(Icon), null, "imageData"))?.Get(icon) != null;

        #endregion

        #region ColorPalette
        
        internal static void SetEntries(this ColorPalette palette, Color[] value) => SetFieldValue(palette, "entries", value);
        internal static void SetFlags(this ColorPalette palette, int value) => SetFieldValue(palette, "flags", value);

        #endregion

        #endregion

        #region Private Methods

        private static FieldAccessor GetField(Type type, Type fieldType, string fieldNamePattern)
        {
            // Fields are meant to be used for non-visible members either by type or name pattern (or both)
            FieldAccessor GetFieldAccessor((Type DeclaringType, Type FieldType, string FieldNamePattern) key)
            {
                for (Type t = key.DeclaringType; t != typeof(object); t = t.BaseType)
                {
                    FieldInfo[] fields = t.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                    FieldInfo field = fields.FirstOrDefault(f => (key.FieldType == null || f.FieldType == key.FieldType) && f.Name == key.FieldNamePattern) // exact name first
                        ?? fields.FirstOrDefault(f => (key.FieldType == null || f.FieldType == key.FieldType)
                            && (key.FieldNamePattern == null || f.Name.Contains(key.FieldNamePattern, StringComparison.OrdinalIgnoreCase)));

                    if (field != null)
                        return FieldAccessor.GetAccessor(field);
                }

                return null;
            }

            if (fields == null)
                Interlocked.CompareExchange(ref fields, new Cache<(Type, Type, string), FieldAccessor>(GetFieldAccessor).GetThreadSafeAccessor(), null);
            return fields[(type, fieldType, fieldNamePattern)];
        }

        private static T GetFieldValueOrDefault<T>(object obj, string fieldNamePattern = null)
        {
            var field = GetField(obj.GetType(), typeof(T), fieldNamePattern);
            return field == null ? default : (T)field.Get(obj);
        }

        private static void SetFieldValue<T>(object obj, string fieldNamePattern, T value, bool throwIfMissing = true)
        {
            Type type = obj.GetType();
            FieldAccessor field = GetField(type, typeof(T), fieldNamePattern);
            if (field == null)
            {
                if (throwIfMissing)
                    throw new InvalidOperationException(Res.AccessorsInstanceFieldDoesNotExist(fieldNamePattern, type));
                return;
            }
#if NETSTANDARD2_0
            if (field.IsReadOnly || field.MemberInfo.DeclaringType?.IsValueType == true)
            {
                ((FieldInfo)field.MemberInfo).SetValue(obj, value);
                return;
            }
#endif

            field.Set(obj, value);
        }

        #endregion

        #endregion
    }
}
