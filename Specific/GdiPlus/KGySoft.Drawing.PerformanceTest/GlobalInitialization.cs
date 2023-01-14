#if !NETFRAMEWORK
#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: GlobalInitialization.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2023 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using NUnit.Framework;

#endregion

namespace KGySoft.Drawing
{
    [SetUpFixture]
    public class GlobalInitialization
    {
        #region Methods

        [OneTimeSetUp]
        public void Initialize()
        {
#if NET7_0_OR_GREATER && !WINDOWS
            Assert.Inconclusive("When targeting .NET 7 or later, executing the tests require Windows. For Unix systems target .NET 6 or earlier.");
#endif

            // To make sure that System.Drawing types can be used also on Unix systems
            DrawingModule.Initialize();
        }

        #endregion
    }
}
#endif