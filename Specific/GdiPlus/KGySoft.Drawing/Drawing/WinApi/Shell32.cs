#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: Shell32.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2023 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System;
using System.Runtime.InteropServices;
#if NET
using System.Runtime.Versioning;
#endif
using System.Security;

using KGySoft.Reflection;

#endregion

namespace KGySoft.Drawing.WinApi
{
    [SecurityCritical]
#if NET
    [SupportedOSPlatform("windows")]
#endif
    internal static class Shell32
    {
        #region NativeMethods class

        private static class NativeMethods
        {
            #region Methods

            /// <summary>
            /// The ExtractIconEx function creates an array of handles to large or small icons extracted from the specified executable file, DLL, or icon file.
            /// </summary>
            /// <param name="lpszFile">[in] (Type: LPCTSTR)
            /// Pointer to a null-terminated string that specifies the name of an executable file, DLL, or icon file from which icons will be extracted.
            /// </param>
            /// <param name="nIconIndex">[in] (Type: int)
            /// Specifies the zero-based index of the first icon to extract. For example, if this value is zero, the function extracts the first icon in the specified file.
            /// If this value is –1 and phiconLarge and phiconSmall are both NULL, the function returns the total number of icons in the specified file. If the file is an executable file or DLL, the return value is the number of RT_GROUP_ICON resources. If the file is an .ico file, the return value is 1.
            /// If this value is a negative number and either phiconLarge or phiconSmall is not NULL, the function begins by extracting the icon whose resource identifier is equal to the absolute value of nIconIndex. For example, use -3 to extract the icon whose resource identifier is 3.
            /// </param>
            /// <param name="phIconLarge">[out] (Type: HICON*)
            /// Pointer to an array of icon handles that receives handles to the large icons extracted from the file. If this parameter is NULL, no large icons are extracted from the file.
            /// </param>
            /// <param name="phIconSmall">[out] (Type: HICON*)
            /// Pointer to an array of icon handles that receives handles to the small icons extracted from the file. If this parameter is NULL, no small icons are extracted from the file.
            /// </param>
            /// <param name="nIcons">Type: UINT
            /// The number of icons to extract from the file.
            /// </param>
            /// <returns>If the nIconIndex parameter is -1, the phiconLarge parameter is NULL, and the phiconSmall parameter is NULL,
            /// then the return value is the number of icons contained in the specified file.
            /// Otherwise, the return value is the number of icons successfully extracted from the file.</returns>
            [DllImport("shell32.dll", CharSet = CharSet.Unicode, EntryPoint = "ExtractIconExW")]
            internal static extern int ExtractIconEx([MarshalAs(UnmanagedType.LPWStr)] string lpszFile, int nIconIndex, IntPtr[]? phIconLarge, IntPtr[]? phIconSmall, int nIcons);

            /// <summary>
            /// Retrieves information about an object in the file system, such as a file, folder, directory, or drive root.
            /// </summary>
            /// <param name="pszPath">[in] (Type: LPCSTR)
            /// A pointer to a null-terminated string of maximum length MAX_PATH that contains the path and file name. Both absolute and relative paths are valid.
            ///
            /// If the uFlags parameter includes the SHGFI_PIDL flag, this parameter must be the address of an ITEMIDLIST (PIDL) structure that contains the list of item identifiers
            /// that uniquely identifies the file within the Shell's namespace. The PIDL must be a fully qualified PIDL. Relative PIDLs are not allowed.
            ///
            /// If the uFlags parameter includes the SHGFI_USEFILEATTRIBUTES flag, this parameter does not have to be a valid file name. The function will proceed as if the file exists
            /// with the specified name and with the file attributes passed in the dwFileAttributes parameter. This allows you to obtain information about a file type by passing just
            /// the extension for pszPath and passing FILE_ATTRIBUTE_NORMAL in dwFileAttributes.
            ///
            /// This string can use either short (the 8.3 form) or long file names.
            /// </param>
            /// <param name="dwFileAttributes">Type: DWORD
            /// A combination of one or more file attribute flags (FILE_ATTRIBUTE_ values as defined in Winnt.h). If uFlags does not include the SHGFI_USEFILEATTRIBUTES flag,
            /// this parameter is ignored.
            /// </param>
            /// <param name="psfi">[in, out] (Type: SHFILEINFO*)
            /// Pointer to a SHFILEINFO structure to receive the file information.
            /// </param>
            /// <param name="cbSizeFileInfo">Type: UINT
            /// The size, in bytes, of the SHFILEINFO structure pointed to by the psfi parameter.
            /// </param>
            /// <param name="uFlags">Type: UINT
            /// The flags that specify the file information to retrieve.
            /// </param>
            /// <returns>Type: DWORD_PTR
            /// Returns a value whose meaning depends on the uFlags parameter.
            ///
            /// If uFlags does not contain SHGFI_EXETYPE or SHGFI_SYSICONINDEX, the return value is nonzero if successful, or zero otherwise.
            ///
            /// If uFlags contains the SHGFI_EXETYPE flag, the return value specifies the type of the executable file.
            /// </returns>
            [DllImport("shell32.dll", CharSet = CharSet.Unicode, EntryPoint = "SHGetFileInfoW")]
            internal static extern IntPtr SHGetFileInfo([MarshalAs(UnmanagedType.LPWStr)] string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbSizeFileInfo, SGHFI uFlags);

            /// <summary>
            /// Retrieves information about system-defined Shell icons.
            /// </summary>
            /// <param name="siid">One of the values from the SHSTOCKICONID enumeration that specifies which icon should be retrieved.</param>
            /// <param name="uFlags">A combination of zero or more of the <see cref="SHGSI"/> flags that specify which information is requested.</param>
            /// <param name="psii">A pointer to a <see cref="SHSTOCKICONINFO"/> structure. When this function is called, the cbSize member of this structure needs to be set to the size of the SHSTOCKICONINFO structure. When this function returns, contains a pointer to a SHSTOCKICONINFO structure that contains the requested information.</param>
            /// <returns>HRESULT</returns>
            [DllImport("shell32.dll", SetLastError = false)]
            internal static extern int SHGetStockIconInfo(StockIcon siid, SHGSI uFlags, ref SHSTOCKICONINFO psii);

            #endregion
        }

        #endregion

        #region Methods

        internal static IntPtr[][] ExtractIconHandles(string fileName, SystemIconSize? size)
        {
            // gets the number of icons in the file
            int iconCount = NativeMethods.ExtractIconEx(fileName, -1, null, null, 0);

            IntPtr[]? large = size == SystemIconSize.Small ? null : new IntPtr[iconCount];
            IntPtr[]? small = size == SystemIconSize.Large ? null : new IntPtr[iconCount];

            // extracts the icons by the size that was selected.
            if (NativeMethods.ExtractIconEx(fileName, 0, large, small, iconCount) == 0)
                return Reflector.EmptyArray<IntPtr[]>();
            IntPtr[][] result = new IntPtr[iconCount][];
            for (int i = 0; i < iconCount; i++)
            {
                result[i] = size == SystemIconSize.Small ? new[] { small![i] }
                    : size == SystemIconSize.Large ? new[] { large![i] }
                    : new[] { large![i], small![i] };
            }

            return result;
        }

        internal static unsafe IntPtr GetFileIconHandle(string extension, SystemIconSize size)
        {
            var tempFileInfo = new SHFILEINFO();
            NativeMethods.SHGetFileInfo(extension, 0, ref tempFileInfo, (uint)sizeof(SHFILEINFO), SGHFI.SHGFI_ICON | SGHFI.SHGFI_USEFILEATTRIBUTES | (SGHFI)size);
            return tempFileInfo.hIcon;
        }

        internal static unsafe IntPtr GetStockIconHandle(StockIcon id, SystemIconSize size)
        {
            var iconInfo = new SHSTOCKICONINFO { cbSize = (uint)sizeof(SHSTOCKICONINFO) };
            return NativeMethods.SHGetStockIconInfo(id, SHGSI.ICON | (SHGSI)size, ref iconInfo) == 0 ? iconInfo.hIcon : IntPtr.Zero;
        }

        #endregion
    }
}
