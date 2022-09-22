#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: CursorHandle.cs
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

using System;
using System.Drawing;
using System.Runtime.InteropServices;
#if NET
using System.Runtime.Versioning;
#endif
using System.Security;

using KGySoft.Drawing.WinApi;

#endregion

namespace KGySoft.Drawing
{
    /// <summary>
    /// Represents a windows cursor that supports colors and partial transparency. The <see cref="CursorHandle"/> instance can be passed to the
    /// <a href="https://docs.microsoft.com/en-us/dotnet/api/system.windows.forms.cursor" target="_blank">System.Windows.Forms.Cursor</a> constructor
    /// to create a new cursor.
    /// </summary>
    /// <remarks>
    /// <para>A <see cref="CursorHandle"/> instance can be created from an <see cref="Icon"/> or <see cref="Bitmap"/> instance by using the
    /// <see cref="IconExtensions.ToCursorHandle(Icon,Point)">IconExtensions.ToCursorHandle</see> and <see cref="BitmapExtensions.ToCursorHandle">BitmapExtensions.ToCursorHandle</see> extension methods.</para>
    /// <para>This class can be used to create a custom Windows Forms <a href="https://docs.microsoft.com/en-us/dotnet/api/system.windows.forms.cursor" target="_blank">Cursor</a> that supports colors and partial transparency.
    /// <note type="important">Do keep a reference to this <see cref="CursorHandle"/> instance until the cursor is in use; otherwise, the cursor resources might be disposed too soon.</note></para>
    /// </remarks>
    [SecurityCritical]
#if NET
    [SupportedOSPlatform("windows")]
#endif
    public sealed class CursorHandle : SafeHandle
    {
        #region Properties

        /// <summary>
        /// Gets whether the handle value is invalid.
        /// </summary>
        public override bool IsInvalid
        {
            [SecurityCritical]
            get => handle == IntPtr.Zero;
        }

        #endregion

        #region Operators

        /// <summary>
        /// Performs an implicit conversion from <see cref="CursorHandle"/> to <see cref="IntPtr"/>.
        /// </summary>
        /// <param name="cursorHandle">The cursor handle.</param>
        /// <returns>An <see cref="IntPtr"/> instance representing the native cursor handle.</returns>
        [SecurityCritical]
        public static implicit operator IntPtr(CursorHandle? cursorHandle) => cursorHandle?.DangerousGetHandle() ?? IntPtr.Zero;

        #endregion

        #region Constructors

        internal CursorHandle(IntPtr handle) : base(IntPtr.Zero, true) => SetHandle(handle);

        #endregion

        #region Methods

        /// <summary>
        /// Free the unmanaged cursor handle.
        /// </summary>
        [SecurityCritical]
        protected override bool ReleaseHandle()
        {
            if (handle != IntPtr.Zero && OSUtils.IsWindows)
                User32.DestroyCursor(handle);
            return true;
        }

        #endregion
    }
}
