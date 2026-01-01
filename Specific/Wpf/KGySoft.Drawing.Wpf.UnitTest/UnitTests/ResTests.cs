#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ResTests.cs
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

namespace KGySoft.Drawing.Wpf.UnitTests
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
        public void TestUnknownResource() => Assert.IsTrue(Reflector.InvokeMethod(typeof(WpfRes), "Get", "unknown")!.ToString()!.StartsWith(unavailableResourcePrefix, StringComparison.Ordinal));

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
            PropertyInfo[] properties = typeof(WpfRes).GetProperties(BindingFlags.Static | BindingFlags.NonPublic);
            foreach (PropertyInfo property in properties)
            {
                string value = property.GetValue(null, null)!.ToString()!;
                Assert.IsTrue(!value.StartsWith(unavailableResourcePrefix, StringComparison.Ordinal), $"{nameof(WpfRes)}.{property.Name} refers to an undefined resource.");
                Assert.IsTrue(!value.ContainsAny("{", "}"), $"{nameof(WpfRes)}.{property.Name} refers to a parameterized resource.");
                obtainedMembers.Add(property.Name);
            }
        }

        private void CheckMethods(HashSet<string> obtainedMembers)
        {
            IEnumerable<MethodInfo> methods = typeof(WpfRes).GetMethods(BindingFlags.Static | BindingFlags.NonPublic).Where(m => m.IsAssembly);
            var generateSettings = new GenerateObjectSettings { AllowCreateObjectWithoutConstructor = true }; // for PropertyDescriptors
            foreach (MethodInfo method in methods)
            {
                if (method.ReturnType == typeof(void))
                    continue;

                object[] parameters = method.GetParameters().Select(p => random.NextObject(p.ParameterType, generateSettings)).ToArray()!;
                string value = method.Invoke(null, parameters)!.ToString()!;
                Assert.IsTrue(!value.StartsWith(unavailableResourcePrefix, StringComparison.Ordinal), $"{nameof(WpfRes)}.{method.Name} refers to an undefined resource.");
                Assert.IsTrue(!value.StartsWith(invalidResourcePrefix, StringComparison.Ordinal), $"{nameof(WpfRes)}.{method.Name} uses too few parameters.");
                for (int i = 0; i < parameters.Length; i++)
                {
                    var parameter = parameters[i];
                    Assert.IsTrue(value.Contains(parameter.ToString()!, StringComparison.Ordinal)
                        , $"{nameof(WpfRes)}.{method.Name} does not use parameter #{i}.");
                }

                obtainedMembers.Add(method.Name);
            }
        }

        private void CheckCoverage(HashSet<string> obtainedMembers)
        {
            var rm = (ResourceManager)Reflector.GetField(typeof(WpfRes), "resourceManager")!;
            ResourceSet rs = rm.GetResourceSet(CultureInfo.InvariantCulture, true, false)!;
            IDictionaryEnumerator enumerator = rs.GetEnumerator();
            var uncovered = new List<string>();
            while (enumerator.MoveNext())
            {
                // ReSharper disable once PossibleNullReferenceException
                string key = ((string)enumerator.Key).Replace("_", String.Empty);
                if (key.StartsWith("General", StringComparison.Ordinal))
                    key = key.Substring("General".Length);
                if (key.EndsWith("Format", StringComparison.Ordinal))
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
