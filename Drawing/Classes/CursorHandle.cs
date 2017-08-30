using System;
using System.Drawing;
using System.Runtime.InteropServices;

using KGySoft.Drawing.WinApi;

namespace KGySoft.Drawing
{
    using System.ComponentModel;

    /// <summary>
    /// Represents a windows cursor. The <see cref="CursorHandle"/> instance can be passed to the
    /// <a href="https://msdn.microsoft.com/en-us/library/system.windows.forms.cursor.aspx" target="_blank">System.Windows.Forms.Cursor</a> constructor
    /// to create a new cursor.
    /// </summary>
    /// <remarks>
    /// <para>This class can be created from an <see cref="Icon"/> or <see cref="Bitmap"/> instance by using the
    /// <see cref="IconTools.ToCursorHandle"/> and <see cref="BitmapTools.ToCursorHandle"/> extension methods.</para>
    /// <para>This class can be used to create a custom Windows Forms <a href="https://msdn.microsoft.com/en-us/library/system.windows.forms.cursor.aspx" target="_blank">System.Windows.Forms.Cursor</a>.
    /// <note>Do keep a reference to this <see cref="CursorHandle"/> instance until the cursor is in use; otherwise, the cursor resources might be disposed too soon.</note></para>
    /// </remarks>
    public sealed class CursorHandle: SafeHandle
    {
        internal CursorHandle(IntPtr handle): base(handle, true)
        {
            // a possibly null handle was created by CreateIconIndirect
            if (handle == IntPtr.Zero)
                throw new ArgumentException("Invalid cursor handle", new Win32Exception(Marshal.GetLastWin32Error()));
            this.handle = handle;
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="CursorHandle"/> to <see cref="IntPtr"/>.
        /// </summary>
        /// <param name="cursorHandle">The cursor handle.</param>
        public static implicit operator IntPtr(CursorHandle cursorHandle)
        {
            return cursorHandle.DangerousGetHandle();
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="CursorHandle" /> class.
        /// </summary>
        /// <param name="disposing">true for a normal dispose operation; false to finalize the handle.</param>
        protected override void Dispose(bool disposing)
        {
            if (handle == IntPtr.Zero)
                return;

            base.Dispose(disposing);
            handle = IntPtr.Zero;
        }

        /// <summary>
        /// Gets a value indicating whether the handle value is invalid.
        /// </summary>
        public override bool IsInvalid
        {
            get { return handle == IntPtr.Zero; }
        }

        /// <summary>
        /// Free the unmanaged cursor handle.
        /// </summary>
        protected override bool ReleaseHandle()
        {
            return User32.DestroyCursor(handle);
        }
    }
}
