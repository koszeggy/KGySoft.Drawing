#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: AsyncConfigBase.cs
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
    /// <summary>
    /// Represents the base class for configuration of asynchronous operations.
    /// </summary>
    public abstract class AsyncConfigBase
    {
        #region Properties

        /// <summary>
        /// Gets or sets an <see cref="IDrawingProgress"/> instance that can handle progress notifications.
        /// <br/>Default value: <see langword="null"/>.
        /// </summary>
        public IDrawingProgress Progress { get; set; }

        /// <summary>
        /// Gets or sets the maximum degree of parallelism. Zero means a default value based on number of CPU cores.
        /// Set one to execute the operation on a single core. The asynchronous operation will not be blocking even if 1 is set.
        /// <br/>Default value: 0.
        /// </summary>
        public int MaxDegreeOfParallelism { get; set; }

        /// <summary>
        /// Gets or sets whether obtaining the result of the asynchronous operation should return the default value of its return type instead of
        /// throwing an <see cref="OperationCanceledException"/> if the operation has been canceled.
        /// <br/>Default value: <see langword="false"/>.
        /// </summary>
        public bool ReturnDefaultIfCanceled { get; set; }

        /// <summary>
        /// Gets or sets a user-provided object that will be returned by the <see cref="IAsyncResult.AsyncState"/> property that
        /// can be used to distinguish this particular asynchronous operation from other ones.
        /// <br/>Default value: <see langword="null"/>.
        /// </summary>
        public object State { get; set; }

        #endregion
    }
}
