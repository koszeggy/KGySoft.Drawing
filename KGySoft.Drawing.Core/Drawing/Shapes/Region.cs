#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: Region.cs
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

using System.Diagnostics;
using System.Drawing;
using System.Threading;

using KGySoft.Collections;
using KGySoft.Drawing.Imaging;

#endregion

namespace KGySoft.Drawing.Shapes
{
    /// <summary>
    /// Represents the region mask of a raw path. Can be either anti-aliased or aliased.
    /// Should not be disposable because can be cached in RawPath, and should not allocate from the pool array for the same reason.
    /// </summary>
    internal sealed class Region
    {
        #region Fields

        #region Internal Fields

        internal readonly Array2D<byte> Mask;
        internal readonly bool IsAntiAliased;

        internal Rectangle Bounds;

        #endregion

        #region Private Fields

        /// <summary>
        /// null: newly created, IsGenerated returns false and creates an unset handle.
        /// Not set: IsGenerated blocks on the current thread until generating is still in progress in another thread, and then returns true
        /// Set: IsGenerated returns true, generating is complete
        /// </summary>
        private volatile ManualResetEventSlim? isGeneratedHandle;

        private volatile bool isCompleted;

        #endregion

        #endregion

        #region Properties

        /// <summary>
        /// Returns false if we need to generate the region in a thread-safe way.
        /// Might be a blocking call if called concurrently while a previous generation is still in progress.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] // to avoid blocking the debugger
        internal bool IsGenerated
        {
            get
            {
                if (isCompleted)
                    return true;

                while (true)
                {
                    // creating a local copy because the field can be nullified in Reset again
                    var local = isGeneratedHandle;

                    // 1st call (if not considering possible resets): creating the handle and returning false so generating the region can start
                    if (local == null)
                    {
                        var newInstance = new ManualResetEventSlim(false);
                        if (Interlocked.CompareExchange(ref isGeneratedHandle, newInstance, null) == null)
                            return false;

                        // lost race
                        newInstance.Dispose();
                        continue;
                    }

                    // non-first call: if the generation hasn't finished yet, waiting for the completion (or reset)
                    local.Wait();

                    // previous generate has been canceled, so our thread can be the "1st caller" again who returns false and lets the region be generated
                    if (!isCompleted)
                        continue;

                    return true;
                }
            }
        }

        #endregion

        #region Constructors

        internal Region(Rectangle bounds, bool isAntiAliased)
        {
            Bounds = bounds;
            IsAntiAliased = isAntiAliased;
            var size = bounds.Size;
            int byteWidth = isAntiAliased ? size.Width : KnownPixelFormat.Format1bppIndexed.GetByteWidth(size.Width);
            byte[] buffer = new byte[byteWidth * size.Height];
            Mask = new Array2D<byte>(buffer, size.Height, byteWidth);
        }

        #endregion

        #region Methods
        
        /// <summary>
        /// Indicates that the mask is generated successfully.
        /// </summary>
        internal void SetCompleted()
        {
            isCompleted = true;
            isGeneratedHandle?.Set();
            isGeneratedHandle?.Dispose();
        }

        /// <summary>
        /// Resets the handle by setting it to null again so the next IsGenerated will return false again.
        /// Called when generating has been canceled.
        /// </summary>
        internal void Reset()
        {
            if (isCompleted)
                return;

            Mask.Buffer.Clear();
            if (Interlocked.Exchange(ref isGeneratedHandle, null) is ManualResetEventSlim waitHandle)
            {
                // setting the nullified handle so possible waiters are unblocked
                waitHandle.Set();
                waitHandle.Dispose();
            }
        }

        #endregion
    }
}
