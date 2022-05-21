#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: Accessors.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2022 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System;
using System.Linq;
using System.Reflection;
using System.Threading;

using KGySoft.Collections;
#if !(NETCOREAPP2_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER)
using KGySoft.CoreLibraries; 
#endif
using KGySoft.Reflection;

#endregion

namespace KGySoft.Drawing
{
    internal static class Accessors
    {
        #region Fields

        private static readonly LockFreeCacheOptions cacheProfile128 = new LockFreeCacheOptions { ThresholdCapacity = 128, HashingStrategy = HashingStrategy.And, MergeInterval = TimeSpan.FromSeconds(1) };

        private static IThreadSafeCacheAccessor<(Type DeclaringType, Type? FieldType, string? FieldNamePattern), FieldAccessor?>? fields;

        #endregion

        #region Methods

        #region Exception

#if NET35 || NET40
        internal static string GetSource(this Exception exception) => GetFieldValueOrDefault<string>(exception, "source")!;
        internal static void SetSource(this Exception exception, string value) => TrySetFieldValue(exception, "source", value);
        internal static void SetRemoteStackTraceString(this Exception exception, string value) => TrySetFieldValue(exception, "remoteStackTraceString", value);
        internal static void InternalPreserveStackTrace(this Exception exception) => Reflector.TryInvokeMethod(exception, "InternalPreserveStackTrace", out var _);
#endif

        #endregion

        #region General field access

        internal static FieldAccessor? GetField(this Type type, Type? fieldType, string? fieldNamePattern)
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

        internal static T? GetFieldValueOrDefault<T>(this object obj, string? fieldNamePattern = null)
        {
            FieldAccessor? field = GetField(obj.GetType(), typeof(T), fieldNamePattern);
            return field == null ? default : (T)field.Get(obj)!;
        }

        internal static bool TrySetFieldValue<T>(this object obj, string? fieldNamePattern, T value)
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
