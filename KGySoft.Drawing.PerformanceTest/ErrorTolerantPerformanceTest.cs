#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ErrorTolerantPerformanceTest.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2020 - All Rights Reserved
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

#endregion

namespace KGySoft.Drawing
{
    internal class ErrorTolerantPerformanceTest : PerformanceTest
    {
        #region Methods

        protected override object Invoke(Action del)
        {
            try
            {
                return base.Invoke(del);
            }
            catch (Exception e)
            {
                return e;
            }
        }

        #endregion
    }
}