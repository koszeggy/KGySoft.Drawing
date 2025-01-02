#if NET35 || NET40

#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: Accessors.cs
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
        private static IThreadSafeCacheAccessor<(Type DeclaringType, string MethodName), MethodAccessor?>? methods;

        #endregion

        #region Methods

        #region Exception

        internal static string? GetSource(this Exception exception) => GetFieldValueOrDefault<Exception, string?>(exception, null, "_source");
        internal static void SetSource(this Exception exception, string? value) => GetField(typeof(Exception), null, "_source")?.SetInstanceValue(exception, value);
        internal static void SetRemoteStackTraceString(this Exception exception, string value) => GetField(typeof(Exception), null, "_remoteStackTraceString")?.SetInstanceValue(exception, value);
        internal static void InternalPreserveStackTrace(this Exception exception) => GetMethod(typeof(Exception), nameof(InternalPreserveStackTrace))?.InvokeInstanceAction(exception);

        #endregion

        #region Any Member

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

        private static MethodAccessor? GetMethod(Type type, string methodName)
        {
            static MethodAccessor? GetMethodAccessor((Type DeclaringType, string MethodName) key)
            {
                // Properties are meant to be used for visible members so always exact names are searched
                MethodInfo? method = key.DeclaringType.GetMethod(key.MethodName, BindingFlags.Instance | BindingFlags.NonPublic);
                return method == null ? null : MethodAccessor.GetAccessor(method);
            }

            if (methods == null)
                Interlocked.CompareExchange(ref methods, ThreadSafeCacheFactory.Create<(Type, string), MethodAccessor?>(GetMethodAccessor, cacheProfile128), null);
            return methods[(type, methodName)];
        }

        #endregion

        #endregion
    }
}
#endif