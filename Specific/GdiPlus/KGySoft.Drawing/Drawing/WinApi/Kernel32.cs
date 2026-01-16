#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: Kernel32.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2026 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
#if NET
using System.Runtime.Versioning;
#endif
using System.Security;

#endregion


namespace KGySoft.Drawing.WinApi
{
    [SecurityCritical]
#if NET
    [SupportedOSPlatform("windows")]
#endif
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "WinAPI")]
    [SuppressMessage("ReSharper", "IdentifierTypo", Justification = "WinAPI")]
    internal static class Kernel32
    {
        #region NativeMethods class

        [SuppressMessage("ReSharper", "MemberHidesStaticFromOuterClass", Justification = "Outer class is never accessed from inside")]
        private static class NativeMethods
        {
            #region Methods

            /// <summary>
            /// Loads the specified module into the address space of the calling process. The specified module may cause other modules to be loaded.
            /// </summary>
            /// <param name="lpFileName">A string that specifies the file name of the module to load. This name is not related to the name stored in a library module itself, as specified by the LIBRARY keyword in the module-definition (.def) file.</param>
            /// <param name="hFile">This parameter is reserved for future use. It must be NULL.</param>
            /// <param name="dwFlags">The action to be taken when loading the module. If no flags are specified, the behavior of this function is identical to that of the LoadLibrary function. This parameter can be one of the following values.
            /// [...]
            /// LOAD_LIBRARY_AS_DATAFILE: If this value is used, the system maps the file into the calling process's virtual address space as if it were a data file. Nothing is done to execute or prepare to execute the mapped file. 
            /// [...]
            /// </param>
            /// <returns></returns>
            [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern IntPtr LoadLibraryEx(string lpFileName, IntPtr hFile, uint dwFlags);

            /// <summary>
            /// Frees the loaded dynamic-link library (DLL) module and, if necessary, decrements its reference count.
            /// When the reference count reaches zero, the module is unloaded from the address space of the calling process and the handle is no longer valid.
            /// </summary>
            /// <param name="hModule">A handle to the loaded library module. The LoadLibrary, LoadLibraryEx, GetModuleHandle, or GetModuleHandleEx function returns this handle.</param>
            /// <returns>If the function succeeds, the return value is nonzero. If the function fails, the return value is zero. To get extended error information, call the GetLastError function.</returns>
            [DllImport("kernel32.dll", SetLastError = true)]
            internal static extern bool FreeLibrary(IntPtr hModule);

            /// <summary>
            /// Enumerates resources of a specified type within a binary module. For Windows Vista and later, this is typically a language-neutral Portable Executable (LN file),
            /// and the enumeration will also include resources from the corresponding language-specific resource files (.mui files) that contain localizable language resources.
            /// It is also possible for hModule to specify an .mui file, in which case only that file is searched for resources.
            /// </summary>
            /// <param name="hModule">A handle to a module to be searched. Starting with Windows Vista, if this is an LN file, then appropriate .mui files (if any exist) are included in the search.
            ///  If this parameter is NULL, that is equivalent to passing in a handle to the module used to create the current process.</param>
            /// <param name="lpszType">The type of the resource for which the name is being enumerated. Alternately, rather than a pointer, this parameter can be MAKEINTRESOURCE(ID), where ID is an integer value representing a predefined resource type.
            /// For a list of predefined resource types, see Resource Types. For more information, see the Remarks section below.</param>
            /// <param name="lpEnumFunc">A pointer to the callback function to be called for each enumerated resource name or ID. For more information, see EnumResNameProc.</param>
            /// <param name="lParam">An application-defined value passed to the callback function. This parameter can be used in error checking.</param>
            /// <returns>The return value is TRUE if the function succeeds or FALSE if the function does not find a resource of the type specified, or if the function fails for another reason. To get extended error information, call GetLastError.</returns>
            /// <remarks>
            /// <para>If IS_INTRESOURCE(lpszType) is TRUE, then lpszType specifies the integer identifier of the given resource type. Otherwise, it is a pointer to a null-terminated string. If the first character of the string is a pound sign (#), then the remaining characters represent a decimal number that specifies the integer identifier of the resource type. For example, the string "#258" represents the identifier 258.</para>
            /// <para>For each resource found, EnumResourceNames calls an application-defined callback function lpEnumFunc, passing the name or the ID of each resource it finds, as well as the various other parameters that were passed to EnumResourceNames. The passed name is only valid inside the callback - if the passed name is a string pointer, it points to an internal buffer that is reused for all callback invocations.</para>
            /// <para>The EnumResourceNames function continues to enumerate resources until the callback function returns FALSE or all resources have been enumerated.</para>
            /// <para>The enumeration never includes duplicates: if resources with the same name are contained in both the LN file and in an .mui file, the resource will only be enumerated once.</para>
            /// </remarks>
            [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern bool EnumResourceNames(IntPtr hModule, IntPtr lpszType, EnumResNameProc lpEnumFunc, IntPtr lParam);

            /// <summary>
            /// Determines the location of a resource with the specified type and name in the specified module.
            /// </summary>
            /// <param name="hModule">A handle to the module whose portable executable file or an accompanying MUI file contains the resource. If this parameter is NULL, the function searches the module used to create the current process.</param>
            /// <param name="lpName">The name of the resource. Alternately, rather than a pointer, this parameter can be MAKEINTRESOURCE(ID), where ID is the integer identifier of the resource. For more information, see the Remarks section below.</param>
            /// <param name="lpType">The resource type. Alternately, rather than a pointer, this parameter can be MAKEINTRESOURCE(ID), where ID is the integer identifier of the given resource type. For standard resource types, see Resource Types. For more information, see the Remarks section below.</param>
            /// <returns>Type: HRSRC
            /// If the function succeeds, the return value is a handle to the specified resource's information block. To obtain a handle to the resource, pass this handle to the LoadResource function.
            /// If the function fails, the return value is NULL. To get extended error information, call GetLastError.</returns>
            /// <remarks>
            /// <para>If IS_INTRESOURCE is TRUE for x = lpName or lpType, x specifies the integer identifier of the name or type of the given resource. Otherwise, those parameters are long pointers to null-terminated strings. If the first character of the string is a pound sign (#), the remaining characters represent a decimal number that specifies the integer identifier of the resource's name or type. For example, the string "#258" represents the integer identifier 258.</para>
            /// <para>To reduce the amount of memory required for a resource, an application should refer to it by integer identifier instead of by name.</para>
            /// <para>An application can use FindResource to find any type of resource, but this function should be used only if the application must access the binary resource data by making subsequent calls to LoadResource and then to LockResource.</para>
            /// </remarks>
            [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern IntPtr FindResource(IntPtr hModule, IntPtr lpName, IntPtr lpType);

            /// <summary>
            /// Retrieves a handle that can be used to obtain a pointer to the first byte of the specified resource in memory.
            /// </summary>
            /// <param name="hModule">A handle to the module whose executable file contains the resource. If hModule is NULL, the system loads the resource from the module that was used to create the current process.</param>
            /// <param name="hResInfo">A handle to the resource to be loaded. This handle is returned by the FindResource or FindResourceEx function.</param>
            /// <returns>
            /// Type: HGLOBAL
            /// If the function succeeds, the return value is a handle to the data associated with the resource.
            /// If the function fails, the return value is NULL. To get extended error information, call GetLastError.
            /// </returns>
            /// <remarks>
            /// <para>The return type of LoadResource is HGLOBAL for backward compatibility, not because the function returns a handle to a global memory block. Do not pass this handle to the GlobalLock or GlobalFree function. To obtain a pointer to the first byte of the resource data, call the LockResource function; to obtain the size of the resource, call SizeofResource.</para>
            /// <para>GlobalSize returns 0 for a resource HGLOBAL. As a result, any APIs that depend on GlobalSize to determine the size of the HGLOBAL will not function correctly. For example, use SHCreateMemStream instead of CreateStreamOnHGlobal.</para>
            /// </remarks>
            [DllImport("kernel32.dll", SetLastError = true)]
            internal static extern IntPtr LoadResource(IntPtr hModule, IntPtr hResInfo);

            /// <summary>
            /// Retrieves a pointer to the specified resource in memory.
            /// </summary>
            /// <param name="hResData">A handle to the resource to be accessed. The LoadResource function returns this handle. Note that this parameter is listed as an HGLOBAL variable only for backward compatibility.
            /// Do not pass any value as a parameter other than a successful return value from the LoadResource function.</param>
            /// <returns>Type: LPVOID
            /// If the loaded resource is available, the return value is a pointer to the first byte of the resource; otherwise, it is NULL.</returns>
            /// <remarks>
            /// <para>The pointer returned by LockResource is valid until the module containing the resource is unloaded. It is not necessary to unlock resources because the system automatically deletes them when the process that created them terminates.</para>
            /// <para>Do not try to lock a resource by using the handle returned by the FindResourceA function or FindResourceExA function. Such a handle points to random data.</para>
            /// <note>LockResource does not actually lock memory; it is just used to obtain a pointer to the memory containing the resource data. The name of the function comes from versions prior to Windows XP, when it was used to lock a global memory block allocated by LoadResource.</note>
            /// </remarks>
            [DllImport("kernel32.dll", SetLastError = true)]
            internal static extern IntPtr LockResource(IntPtr hResData);

            /// <summary>
            /// Retrieves the size, in bytes, of the specified resource.
            /// </summary>
            /// <param name="hModule">A handle to the module whose executable file contains the resource. Default is the module used to create the current process.</param>
            /// <param name="hResInfo">A handle to the resource. This handle must be created by using the FindResource or FindResourceEx function.</param>
            /// <returns>If the function succeeds, the return value is the number of bytes in the resource.
            /// If the function fails, the return value is zero. To get extended error information, call GetLastError.</returns>
            [DllImport("kernel32.dll", SetLastError = true)]
            internal static extern uint SizeofResource(IntPtr hModule, IntPtr hResInfo);

            #endregion
        }

        #endregion

        #region Methods

        internal static IntPtr LoadLibraryData(string fileName, bool throwError = true)
        {
            IntPtr result = NativeMethods.LoadLibraryEx(fileName, IntPtr.Zero, Constants.LOAD_LIBRARY_AS_DATAFILE);
            if (result == IntPtr.Zero && throwError)
                throw new Win32Exception(Marshal.GetLastWin32Error());
            return result;
        }

        internal static void EnumResourceNames(IntPtr hModule, IntPtr lpszType, EnumResNameProc lpEnumFunc)
        {
            if (!NativeMethods.EnumResourceNames(hModule, lpszType, lpEnumFunc, IntPtr.Zero))
            {
                int error = Marshal.GetLastWin32Error();
                if (error != 0 && error != Constants.ERROR_RESOURCE_ENUM_USER_STOP)
                    throw new Win32Exception(error);
            }
        }

        internal static void FreeLibrary(IntPtr hModule)
        {
            if (!NativeMethods.FreeLibrary(hModule))
                throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        internal static byte[] ExtractResourceData(IntPtr hModule, IntPtr name, IntPtr type)
        {
            IntPtr hResInfo = NativeMethods.FindResource(hModule, name, type);
            if (hResInfo == IntPtr.Zero)
                throw new Win32Exception(Marshal.GetLastWin32Error());
            IntPtr hResData = NativeMethods.LoadResource(hModule, hResInfo);
            if (hResData == IntPtr.Zero)
                throw new Win32Exception(Marshal.GetLastWin32Error());
            IntPtr pResData = NativeMethods.LockResource(hResData);
            if (pResData == IntPtr.Zero)
                throw new Win32Exception(Marshal.GetLastWin32Error());
            uint size = NativeMethods.SizeofResource(hModule, hResInfo);
            byte[] buf = new byte[size];
            Marshal.Copy(pResData, buf, 0, buf.Length);
            return buf;
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static IntPtr MAKEINTRESOURCE(int id)
#if NET7_0_OR_GREATER
                    => (ushort)id;
#else
                    => (IntPtr)((ushort)id);
#endif

        #endregion
    }
}
