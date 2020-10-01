#if !NET35
#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: TaskConfig.cs
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

using System.Threading;
using System.Threading.Tasks;

#endregion

namespace KGySoft.Drawing
{
    /// <summary>
    /// Represents asynchronous configuration for <see cref="Task"/>-returning methods.
    /// </summary>
    public sealed class TaskConfig : AsyncConfigBase
    {
        #region Properties

        /// <summary>
        /// Gets or sets the cancellation token for this operation.
        /// </summary>
        public CancellationToken CancellationToken { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskConfig"/> class.
        /// </summary>
        public TaskConfig()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskConfig"/> class.
        /// </summary>
        /// <param name="cancellationToken">Specifies the cancellation token for this operation.</param>
        public TaskConfig(CancellationToken cancellationToken) => CancellationToken = cancellationToken;

        #endregion
    }
}
#endif