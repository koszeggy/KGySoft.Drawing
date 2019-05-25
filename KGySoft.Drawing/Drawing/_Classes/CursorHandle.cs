#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: CursorHandle.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2019 - All Rights Reserved
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
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Security;
using KGySoft.Drawing.WinApi;

#endregion

namespace KGySoft.Drawing
{
    /// <summary>
    /// Represents a windows cursor. The <see cref="CursorHandle"/> instance can be passed to the
    /// <a href="https://msdn.microsoft.com/en-us/library/system.windows.forms.cursor.aspx" target="_blank">System.Windows.Forms.Cursor</a> constructor
    /// to create a new cursor.
    /// </summary>
    /// <remarks>
    /// <para>A <see cref="CursorHandle"/> instance can be created from an <see cref="Icon"/> or <see cref="Bitmap"/> instance by using the
    /// <see cref="IconExtensions.ToCursorHandle(Icon,Point)">IconExtensions.ToCursorHandle</see> and <see cref="BitmapExtensions.ToCursorHandle">BitmapExtensions.ToCursorHandle</see> extension methods.</para>
    /// <para>This class can be used to create a custom Windows Forms <a href="https://msdn.microsoft.com/en-us/library/system.windows.forms.cursor.aspx" target="_blank">System.Windows.Forms.Cursor</a>.
    /// <note type="important">Do keep a reference to this <see cref="CursorHandle"/> instance until the cursor is in use; otherwise, the cursor resources might be disposed too soon.</note></para>
    /// </remarks>
    public sealed class CursorHandle : SafeHandle
    {
        #region Properties

        /// <summary>
        /// Gets a value indicating whether the handle value is invalid.
        /// </summary>
        public override bool IsInvalid
        {
#if !NET35
            [SecuritySafeCritical]
#endif
            get => handle == IntPtr.Zero;
        }

        #endregion

        #region Operators

        /// <summary>
        /// Performs an implicit conversion from <see cref="CursorHandle"/> to <see cref="IntPtr"/>.
        /// </summary>
        /// <param name="cursorHandle">The cursor handle.</param>
#if !NET35
        [SecuritySafeCritical]
#endif
        public static implicit operator IntPtr(CursorHandle cursorHandle) => cursorHandle.DangerousGetHandle();

        #endregion

        #region Constructors

        [SecurityCritical]
        internal CursorHandle(IntPtr handle) : base(handle, true)
        {
            // a possibly null handle was created by CreateIconIndirect, which sets last error
            if (handle == IntPtr.Zero)
                throw new ArgumentException(Res.CursorHandleInvalidHandle, nameof(handle), new Win32Exception(Marshal.GetLastWin32Error()));
            this.handle = handle;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="CursorHandle" /> class.
        /// </summary>
        /// <param name="disposing">true for a normal dispose operation; false to finalize the handle.</param>
#if !NET35
        [SecuritySafeCritical]
#endif
        protected override void Dispose(bool disposing)
        {
            if (handle == IntPtr.Zero)
                return;

            base.Dispose(disposing);
            handle = IntPtr.Zero;
        }

        /// <summary>
        /// Free the unmanaged cursor handle.
        /// </summary>
        [SecurityCritical]
        protected override bool ReleaseHandle()
        {
            if (handle == IntPtr.Zero)
                return false;

            User32.DestroyCursor(handle);
            return true;
        }

        #endregion
    }
}
