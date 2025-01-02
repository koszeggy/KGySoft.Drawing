﻿#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ResTests.cs
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
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Resources;

using KGySoft.CoreLibraries;
using KGySoft.Reflection;
using KGySoft.Resources;

using NUnit.Framework;

#endregion

namespace KGySoft.Drawing.UnitTests
{
    [TestFixture]
    public class ResTests
    {
        #region Constants
        
        private const string unavailableResourcePrefix = "Resource ID not found";
        private const string invalidResourcePrefix = "Resource text is not valid";

        #endregion

        #region Fields

        private static readonly Random random = new Random();

        #endregion

        #region Methods

        #region Public Methods

        [OneTimeSetUp]
        public void Initialize() => LanguageSettings.DynamicResourceManagersSource = ResourceManagerSources.CompiledOnly;

        [Test]
        public void TestUnknownResource() => Assert.IsTrue(Reflector.InvokeMethod(typeof(Res), "Get", "unknown").ToString().StartsWith(unavailableResourcePrefix, StringComparison.Ordinal));

        [Test]
        public void TestInvalidResource() => Assert.IsTrue(Reflector.InvokeMethod(typeof(Res), "Get", "General_InternalErrorFormat", new object[0]).ToString().StartsWith(invalidResourcePrefix, StringComparison.Ordinal));

        [Test]
        public void TestResources()
        {
            var obtainedMembers = new HashSet<string>();

            // note: these should be 3 different tests but if coverage is tested in ClassCleanup method, then the assert is suppressed
            CheckProperties(obtainedMembers);
            CheckMethods(obtainedMembers);
            CheckCoverage(obtainedMembers);
        }

        #endregion

        #region Private Methods

        private void CheckProperties(HashSet<string> obtainedMembers)
        {
            PropertyInfo[] properties = typeof(Res).GetProperties(BindingFlags.Static | BindingFlags.NonPublic);
            foreach (PropertyInfo property in properties)
            {
                string value = property.GetValue(null, null).ToString();
                Assert.IsTrue(!value.StartsWith(unavailableResourcePrefix, StringComparison.Ordinal), $"{nameof(Res)}.{property.Name} refers to an undefined resource.");
                Assert.IsTrue(!value.ContainsAny("{", "}"), $"{nameof(Res)}.{property.Name} refers to a parameterized resource.");
                obtainedMembers.Add(property.Name);
            }
        }

        private void CheckMethods(HashSet<string> obtainedMembers)
        {
            IEnumerable<MethodInfo> methods = typeof(Res).GetMethods(BindingFlags.Static | BindingFlags.NonPublic).Where(m => m.IsAssembly);
            var generateSettings = new GenerateObjectSettings { AllowCreateObjectWithoutConstructor = true }; // for PropertyDescriptors
            foreach (MethodInfo mi in methods)
            {
                var method = mi.IsGenericMethodDefinition ? mi.MakeGenericMethod(random.NextObject(typeof(Enum)).GetType()) : mi;
                if (method.ReturnType == typeof(void))
                    continue;

                object[] parameters = method.GetParameters().Select(p => random.NextObject(p.ParameterType, generateSettings)).ToArray();
                string value = method.Invoke(null, parameters).ToString();
                Assert.IsTrue(!value.StartsWith(unavailableResourcePrefix, StringComparison.Ordinal), $"{nameof(Res)}.{method.Name} refers to an undefined resource.");
                Assert.IsTrue(!value.StartsWith(invalidResourcePrefix, StringComparison.Ordinal), $"{nameof(Res)}.{method.Name} uses too few parameters.");
                for (int i = 0; i < parameters.Length; i++)
                {
                    var parameter = parameters[i];
                    Assert.IsTrue(value.Contains(parameter.ToString(), StringComparison.Ordinal)
                        //|| mi.IsGenericMethodDefinition // Xxx<TEnum>(TEnum value) - not value but possible TValue is printed
                        //|| parameter is float f && value.Contains(f.ToString("P2"), StringComparison.Ordinal) // percentage format of float
                        //|| parameter is double d && value.ContainsAny(StringComparison.Ordinal, d.ToString("N2"), d.ToString("P2")) // double: percentage or number
                        //|| parameter is bool b && value.Contains(b ? PublicResources.Yes : PublicResources.No, StringComparison.Ordinal) // percentage format of float
                        //|| parameter is int n && value.Contains(n.ToString("N0"), StringComparison.Ordinal) // normal ToString checked above, number format checked here
                        //|| parameter is long l && value.Contains(l.ToString("N0"), StringComparison.Ordinal), // normal ToString checked above, number format checked here
                        , $"{nameof(Res)}.{method.Name} does not use parameter #{i}.");
                }

                obtainedMembers.Add(method.Name);
            }
        }

        private void CheckCoverage(HashSet<string> obtainedMembers)
        {
            var rm = (ResourceManager)Reflector.GetField(typeof(Res), "resourceManager");
            ResourceSet rs = rm.GetResourceSet(CultureInfo.InvariantCulture, true, false);
            IDictionaryEnumerator enumerator = rs.GetEnumerator();
            var uncovered = new List<string>();
            while (enumerator.MoveNext())
            {
                // ReSharper disable once PossibleNullReferenceException
                string key = ((string)enumerator.Key).Replace("_", String.Empty);
                if (key.StartsWith("General", StringComparison.Ordinal))
                    key = key.Substring("General".Length);
                if (!obtainedMembers.Contains(key) && key.EndsWith("Format", StringComparison.Ordinal))
                    key = key.Substring(0, key.Length - "Format".Length);
                if (!obtainedMembers.Contains(key))
                    uncovered.Add((string)enumerator.Key);
            }

            Assert.IsTrue(uncovered.Count == 0, $"{uncovered.Count} orphan compiled resources detected:{Environment.NewLine}{String.Join(Environment.NewLine, uncovered.ToArray())}");
        }

        #endregion

        #endregion
    }
}
