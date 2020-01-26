﻿#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: PerformanceTestResultCollectionExtensions.cs
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

using System.IO;

using KGySoft.Diagnostics;

#endregion

namespace KGySoft.Drawing
{
    internal static class PerformanceTestResultCollectionExtensions
    {
        #region Methods

        internal static void DumpResultsAndReturnValues(this IPerformanceTestResultCollection results, TextWriter textWriter)
        {
            results.DumpResults(textWriter);
            textWriter.WriteLine("==[Return Values]=====================");
            foreach (ITestCaseResult testCaseResult in results)
                textWriter.WriteLine($"{testCaseResult.Name}: {testCaseResult.Result ?? PublicResources.Null}");
        }

        #endregion
    }
}