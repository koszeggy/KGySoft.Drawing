#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: EnumResNameProc.cs
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

using System;
using System.Runtime.InteropServices;

#endregion

namespace KGySoft.Drawing.WinApi
{
    /// <summary>
    /// An application-defined callback function used with the EnumResourceNames and EnumResourceNamesEx functions. It receives the type and name of a resource.
    /// The ENUMRESNAMEPROC type defines a pointer to this callback function. EnumResNameProc is a placeholder for the application-defined function name.
    /// </summary>
    /// <param name="hModule">A handle to the module whose executable file contains the resources that are being enumerated. If this parameter is NULL, the function enumerates the resource names in the module used to create the current process.</param>
    /// <param name="lpszType">The type of resource for which the name is being enumerated. Alternately, rather than a pointer, this parameter can be MAKEINTRESOURCE(ID), where ID is an integer value representing a predefined resource type. For standard resource types, see Resource Types. For more information, see the Remarks section below.</param>
    /// <param name="lpszName">The name of a resource of the type being enumerated. Alternately, rather than a pointer, this parameter can be MAKEINTRESOURCE(ID), where ID is the integer identifier of the resource. For more information, see the Remarks section below.</param>
    /// <param name="lParam">An application-defined parameter passed to the EnumResourceNames or EnumResourceNamesEx function. This parameter can be used in error checking.</param>
    /// <returns>Returns TRUE to continue enumeration or FALSE to stop enumeration.</returns>
    /// <remarks>
    /// <para>If IS_INTRESOURCE(lpszType) is TRUE, then lpszType specifies the integer identifier of the given resource type. Otherwise, it is a pointer to a null-terminated string. If the first character of the string is a pound sign (#), then the remaining characters represent a decimal number that specifies the integer identifier of the resource type. For example, the string "#258" represents the identifier 258.</para>
    /// <para>Similarly, if IS_INTRESOURCE(lpszName) is TRUE, then lpszName specifies the integer identifier of the given resource. Otherwise, it is a pointer to a null-terminated string. If the first character of the string is a pound sign (#), then the remaining characters represent a decimal number that specifies the integer identifier of the resource.</para>
    /// <para>An application must register this function by passing its address to the EnumResourceNames or EnumResourceNamesEx function.</para>
    /// <para>If the callback function returns FALSE, then EnumResourceNames or EnumResourceNamesEx will stop enumeration and return FALSE. On Windows XP and earlier the value obtained from GetLastError will be ERROR_SUCCESS; starting with Windows Vista, the last error value will be ERROR_RESOURCE_ENUM_USER_STOP.</para>
    /// </remarks>
    [UnmanagedFunctionPointer(CallingConvention.Winapi, SetLastError = true, CharSet = CharSet.Unicode)]
    internal delegate bool EnumResNameProc(IntPtr hModule, IntPtr lpszType, IntPtr lpszName, IntPtr lParam);
}