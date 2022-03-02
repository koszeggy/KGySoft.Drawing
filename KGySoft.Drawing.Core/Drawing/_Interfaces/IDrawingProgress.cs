#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: IDrawingProgress.cs
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

#if !(NET35 || NET40)
using System;
#endif
#if !NET35
using System.Threading.Tasks; 
#endif

#endregion

#if NET35
#pragma warning disable CS1574 // XML comment has cref attribute that could not be resolved - in .NET 3.5 not all members are available
#endif

namespace KGySoft.Drawing
{
    /// <summary>
    /// Represents a provider for progress updates for drawing operations.
    /// </summary>
    public interface IDrawingProgress
#if !(NET35 || NET40)
        : IProgress<DrawingProgress> 
#endif
    {
        #region Methods

        /// <summary>
        /// Reports a progress update to any arbitrary state.
        /// For parallel operations it is recommended to use the <see cref="Increment">Increment</see> method
        /// after starting a new progress because this method cannot guarantee that <see cref="DrawingProgress.CurrentValue"/> will be a strictly
        /// increasing value when called from <see cref="Parallel"/> members, for example.
        /// </summary>
        /// <param name="progress">The value of the updated progress.</param>
#if !(NET35 || NET40)
        new
#endif
        void Report(DrawingProgress progress);

        /// <summary>
        /// Indicates that a new progress session is started that consists of <paramref name="maximumValue"/>.
        /// </summary>
        /// <param name="operationType">Type of the new operation.</param>
        /// <param name="maximumValue">Specifies the possible maximum steps of the new operation (the <see cref="Increment">Increment</see> method is expected to be called later on
        /// as many times as the value of this parameter). 0 means an operation with no separate steps. This parameter is optional.
        /// <br/>Default value: <c>0</c>.</param>
        /// <param name="currentValue">Specifies the initial current value for the new progress. Should be between 0 and <paramref name="maximumValue"/>. This parameter is optional.
        /// <br/>Default value: <c>0</c>.</param>
        void New(DrawingOperation operationType, int maximumValue = 0, int currentValue = 0);

        /// <summary>
        /// Indicates a progress update of a single step. Expected to be called after the <see cref="New">New</see> or <see cref="Report">Report</see> methods with nonzero maximum steps
        /// but should not be sensitive for concurrency racing conditions.
        /// </summary>
        void Increment();

        /// <summary>
        /// Indicates that the current progress is at a specific position.
        /// </summary>
        /// <param name="value">The current progress value. Should not exceed the maximum value of the last <see cref="New">New</see> or <see cref="Report">Report</see> calls
        /// but should not be sensitive for concurrency racing conditions.
        /// </param>
        void SetProgressValue(int value);

        /// <summary>
        /// Indicates that a progress value of the last <see cref="New">New</see> or <see cref="Report">Report</see> method should be set to the maximum value.
        /// It is not needed to be called at the end of each sessions.
        /// </summary>
        void Complete();

        #endregion
    }
}