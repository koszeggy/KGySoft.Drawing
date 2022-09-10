#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: TestBase.cs
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
using System.Threading.Tasks;

using Windows.UI.Core;

#endregion

namespace KGySoft.Drawing.Uwp.UnitTest
{
    public abstract class TestBase
    {
        #region Constructors

        protected TestBase() => Console.SetOut(Program.ConsoleWriter);

        #endregion

        #region Methods

        protected async Task ExecuteTest(DispatchedHandler callback) => await Program.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, callback);

        #endregion
    }
}