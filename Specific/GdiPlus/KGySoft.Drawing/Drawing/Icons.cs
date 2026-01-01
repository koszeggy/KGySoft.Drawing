#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: Icons.cs
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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Resources;
#if NET
using System.Runtime.Versioning;
#endif
using System.Security;
using System.Threading;

using KGySoft.Collections;
using KGySoft.CoreLibraries;
using KGySoft.Drawing.WinApi;
using KGySoft.Reflection;
using KGySoft.Serialization.Binary;

#endregion

namespace KGySoft.Drawing
{
    /// <summary>
    /// Provides some icon-related methods as well as properties returning general icons in multi resolution.
    /// Unlike <see cref="SystemIcons"/>, these icons should be disposed when not used anymore.
    /// <div style="display: none;"><br/>See the <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Icons.htm">online help</a> to observe the icons visually that are returned by the properties.</div>
    /// </summary>
    /// <remarks>
    /// <note>When targeting .NET 7.0 or later versions this class is supported on Windows only.</note>
    /// </remarks>
#if NET7_0_OR_GREATER
    [SupportedOSPlatform("windows")]
#endif
    public static class Icons
    {
        #region Constants

        private const int stockIconIdMask = 0xFFFF;
        private const int alternativeIconIdMask = 0x7FFF0000;
        private const int isImageResIconFlag = unchecked((int)0x80000000); // set: imageres.dll; not set: user32.dll
        private const int alternativeIdShift = 16;

        private const int idInformation = (int)StockIcon.Information | 104 << alternativeIdShift;
        private const int idWarning = (int)StockIcon.Warning | 101 << alternativeIdShift;
        private const int idError = (int)StockIcon.Error | 103 << alternativeIdShift;
        private const int idQuestion = (int)StockIcon.Help | 102 << alternativeIdShift;
        private const int idApplication = (int)StockIcon.Application | 100 << alternativeIdShift;
        private const int idShield = (int)StockIcon.Shield; // In User32 there is a shield with ID 106, but it does not exist in XP yet. But no problem, the fallback SystemIcons.Shield has more images anyway.
        private const int idSecuritySuccess = isImageResIconFlag | (106 << alternativeIdShift);
        private const int idSecurityWarning = isImageResIconFlag | (107 << alternativeIdShift);
        private const int idSecurityQuestion = isImageResIconFlag | (104 << alternativeIdShift);
        private const int idSecurityError = isImageResIconFlag | (105 << alternativeIdShift);

        #endregion

        #region Fields

        private static readonly Func<int, Func<Icon>?, RawIcon?> getSystemIconAddValueFactory = GetStockIconOrDefault;
        private static readonly Func<string, RawIcon> getResourceIconAddValueFactory = DoGetResourceIcon;

        private static ResourceManager? resourceManager;
        private static ThreadSafeDictionary<int, RawIcon?>? systemIconsCache;
        private static ThreadSafeDictionary<string, RawIcon>? resourceIconsCache;

        #endregion

        #region Properties

        #region Public Properties

        /// <summary>
        /// <img src="../Help/Images/Information16W11.png" alt="Information (small version for the summary)"/>
        /// Gets an <see cref="Icon"/> instance that contains the
        /// Information icon as it is displayed by the current operating system.
        /// <br/>On Windows Vista and above sizes range from 16x16 to 256x256, on Windows XP the largest resolution is 48x48, and on Linux the icon has always 32x32 and 16x16 image sizes.
        /// <div style="display: none;"><br/>See the <a href="https://koszeggy.github.io/docs/drawing/html/P_KGySoft_Drawing_Icons_SystemInformation.htm">online help</a> for the icon images.</div>
        /// </summary>
        /// <example>
        /// <para>
        /// On Windows 11 the icon contains 8 images from 16x16 to 256x256. The 32x32 and 16x16 ones look like this:<br/>
        /// <img src="../Help/Images/Information32W11.png" alt="Information 32x32 Windows 11"/>
        /// <img src="../Help/Images/Information16W11.png" alt="Information 16x16 Windows 11"/>
        /// </para>
        /// <para>
        /// On Windows 10 the icon contains 11 images from 16x16 to 256x256. The 32x32 and 16x16 ones look like this:<br/>
        /// <img src="../Help/Images/Information32W10.png" alt="Information 32x32 Windows 10"/>
        /// <img src="../Help/Images/Information16W10.png" alt="Information 16x16 Windows 10"/>
        /// </para>
        /// <para>
        /// On Windows 8 the icon contains 8 images from 16x16 to 256x256. The 32x32 and 16x16 ones look like this:<br/>
        /// <img src="../Help/Images/Information32W8.png" alt="Information 32x32 Windows 8"/>
        /// <img src="../Help/Images/Information16W8.png" alt="Information 16x16 Windows 8"/>
        /// </para>
        /// <para>
        /// On Windows Vista and Windows 7 the icon contains 4 images from 16x16 to 256x256. The 32x32 and 16x16 ones look like this:<br/>
        /// <img src="../Help/Images/Information32.png" alt="Information 32x32 Windows Vista"/>
        /// <img src="../Help/Images/Information16.png" alt="Information 16x16 Windows Vista"/>
        /// <note>The <see cref="Information"/> property returns a similar icon, but with 3 additional resolutions.</note>
        /// </para>
        /// <para>
        /// On Windows XP the icon contains 3 images from 16x16 to 48x48. The 32x32 and 16x16 ones look like this:<br/>
        /// <img src="../Help/Images/Information32XP.png" alt="Information Windows XP 32x32"/>
        /// <img src="../Help/Images/Information16XP.png" alt="Information Windows XP 16x16"/>
        /// </para>
        /// <para>
        /// On Linux/Mono the icon contains the following 32x32 and 16x16 images:<br/>
        /// <img src="../Help/Images/Information32Mono.png" alt="Information Linux/Mono 32x32"/>
        /// <img src="../Help/Images/Information16Mono.png" alt="Information Linux/Mono 16x16"/>
        /// <note>On Linux the .NET Core build mistakenly returns the <see cref="SystemError"/> icon, which is an issue in the <see cref="SystemIcons"/> implementation of .NET Core.</note>
        /// </para>
        /// </example>
        public static Icon SystemInformation => GetSystemIcon(idInformation, () => SystemIcons.Information);

        /// <summary>
        /// <img src="../Help/Images/Warning16W11.png" alt="Warning (small version for the summary)"/>
        /// Gets an <see cref="Icon"/> instance that contains the
        /// Warning icon as it is displayed by the current operating system.
        /// <br/>On Windows Vista and above sizes range from 16x16 to 256x256, on Windows XP the largest resolution is 48x48, and on Linux the icon has always 32x32 and 16x16 image sizes.
        /// <div style="display: none;"><br/>See the <a href="https://koszeggy.github.io/docs/drawing/html/P_KGySoft_Drawing_Icons_SystemWarning.htm">online help</a> for the icon images.</div>
        /// </summary>
        /// <example>
        /// <para>
        /// On Windows 11 the icon contains 8 images from 16x16 to 256x256. The 32x32 and 16x16 ones look like this:<br/>
        /// <img src="../Help/Images/Warning32W11.png" alt="Warning 32x32 Windows 11"/>
        /// <img src="../Help/Images/Warning16W11.png" alt="Warning 16x16 Windows 11"/>
        /// </para>
        /// <para>
        /// On Windows 10 the icon contains 11 images from 16x16 to 256x256. The 32x32 and 16x16 ones look like this:<br/>
        /// <img src="../Help/Images/Warning32W10.png" alt="Warning 32x32 Windows 10"/>
        /// <img src="../Help/Images/Warning16W10.png" alt="Warning 16x16 Windows 10"/>
        /// </para>
        /// <para>
        /// On Windows Vista, Windows 7 and Windows 8 the icon contains 4 images from 16x16 to 256x256. The 32x32 and 16x16 ones look like this:<br/>
        /// <img src="../Help/Images/Warning32.png" alt="Warning 32x32 Windows Vista"/>
        /// <img src="../Help/Images/Warning16.png" alt="Warning 16x16 Windows Vista"/>
        /// <note>The <see cref="Warning"/> property returns a similar icon, but with 3 additional resolutions.</note>
        /// </para>
        /// <para>
        /// On Windows XP the icon contains 3 images from 16x16 to 48x48. The 32x32 and 16x16 ones look like this:<br/>
        /// <img src="../Help/Images/Warning32XP.png" alt="Warning Windows XP 32x32"/>
        /// <img src="../Help/Images/Warning16XP.png" alt="Warning Windows XP 16x16"/>
        /// </para>
        /// <para>
        /// On Linux/Mono the icon contains the following 32x32 and 16x16 images:<br/>
        /// <img src="../Help/Images/Warning32Mono.png" alt="Warning Linux/Mono 32x32"/>
        /// <img src="../Help/Images/Warning16Mono.png" alt="Warning Linux/Mono 16x16"/>
        /// <note>On Linux the .NET Core build mistakenly returns the <see cref="SystemError"/> icon, which is an issue in the <see cref="SystemIcons"/> implementation of .NET Core.</note>
        /// </para>
        /// </example>
        public static Icon SystemWarning => GetSystemIcon(idWarning, () => SystemIcons.Warning);

        /// <summary>
        /// <img src="../Help/Images/Error16W11.png" alt="Error (small version for the summary)"/>
        /// Gets an <see cref="Icon"/> instance that contains the
        /// Error icon as it is displayed by the current operating system.
        /// <br/>On Windows Vista and above sizes range from 16x16 to 256x256, on Windows XP the largest resolution is 48x48, and on Linux the icon has always 32x32 and 16x16 image sizes.
        /// <div style="display: none;"><br/>See the <a href="https://koszeggy.github.io/docs/drawing/html/P_KGySoft_Drawing_Icons_SystemError.htm">online help</a> for the icon images.</div>
        /// </summary>
        /// <example>
        /// <para>
        /// On Windows 11 the icon contains 8 images from 16x16 to 256x256. The 32x32 and 16x16 ones look like this:<br/>
        /// <img src="../Help/Images/Error32W11.png" alt="Error 32x32 Windows 11"/>
        /// <img src="../Help/Images/Error16W11.png" alt="Error 16x16 Windows 11"/>
        /// </para>
        /// <para>
        /// On Windows 10 the icon contains 11 images from 16x16 to 256x256. The 32x32 and 16x16 ones look like this:<br/>
        /// <img src="../Help/Images/Error32W10.png" alt="Error 32x32 Windows 10"/>
        /// <img src="../Help/Images/Error16W10.png" alt="Error 16x16 Windows 10"/>
        /// </para>
        /// <para>
        /// On Windows Vista, Windows 7 and Windows 8 the icon contains 5 images from 16x16 to 256x256. The 32x32 and 16x16 ones look like this:<br/>
        /// <img src="../Help/Images/Error32.png" alt="Error 32x32 Windows Vista"/>
        /// <img src="../Help/Images/Error16.png" alt="Error 16x16 Windows Vista"/>
        /// <note>The <see cref="Error"/> property returns a similar icon, but with 2 additional resolutions.</note>
        /// </para>
        /// <para>
        /// On Windows XP the icon contains 3 images from 16x16 to 48x48. The 32x32 and 16x16 ones look like this:<br/>
        /// <img src="../Help/Images/Error32XP.png" alt="Error Windows XP 32x32"/>
        /// <img src="../Help/Images/Error16XP.png" alt="Error Windows XP 16x16"/>
        /// </para>
        /// <para>
        /// On Linux/Mono the icon contains the following 32x32 and 16x16 images:<br/>
        /// <img src="../Help/Images/Error32Mono.png" alt="Error Linux/Mono 32x32"/>
        /// <img src="../Help/Images/Error16Mono.png" alt="Error Linux/Mono 16x16"/>
        /// </para>
        /// </example>
        public static Icon SystemError => GetSystemIcon(idError, () => SystemIcons.Error);

        /// <summary>
        /// <img src="../Help/Images/Question16W11.png" alt="Question (small version for the summary)"/>
        /// Gets an <see cref="Icon"/> instance that contains the
        /// Question icon as it is displayed by the current operating system.
        /// <br/>On Windows Vista and above sizes range from 16x16 to 256x256, on Windows XP the largest resolution is 48x48, and on Linux the icon has always 32x32 and 16x16 image sizes.
        /// <div style="display: none;"><br/>See the <a href="https://koszeggy.github.io/docs/drawing/html/P_KGySoft_Drawing_Icons_SystemQuestion.htm">online help</a> for the icon images.</div>
        /// </summary>
        /// <example>
        /// <para>
        /// On Windows 11 the icon contains 8 images from 16x16 to 256x256. The 32x32 and 16x16 ones look like this:<br/>
        /// <img src="../Help/Images/Question32W11.png" alt="Question 32x32 Windows 11"/>
        /// <img src="../Help/Images/Question16W11.png" alt="Question 16x16 Windows 11"/>
        /// </para>
        /// <para>
        /// On Windows 10 the icon contains 8 images from 16x16 to 256x256. The 32x32 and 16x16 ones look like this:<br/>
        /// <img src="../Help/Images/Question32W10.png" alt="Question 32x32 Windows 10"/>
        /// <img src="../Help/Images/Question16W10.png" alt="Question 16x16 Windows 10"/>
        /// </para>
        /// <para>
        /// On Windows 8 the icon contains 8 images from 16x16 to 256x256. The 32x32 and 16x16 ones look like this:<br/>
        /// <img src="../Help/Images/Question32W8.png" alt="Question 32x32 Windows 8"/>
        /// <img src="../Help/Images/Question16W8.png" alt="Question 16x16 Windows 8"/>
        /// </para>
        /// <para>
        /// On Windows Vista and Windows 7 the icon contains 5 images from 16x16 to 256x256. The 32x32 and 16x16 ones look like this:<br/>
        /// <img src="../Help/Images/Question32.png" alt="Question 32x32 Windows Vista"/>
        /// <img src="../Help/Images/Question16.png" alt="Question 16x16 Windows Vista"/>
        /// <note>The <see cref="Question"/> property returns a similar icon, but with 2 additional resolutions.</note>
        /// </para>
        /// <para>
        /// On Windows XP the icon contains 3 images from 16x16 to 48x48. The 32x32 and 16x16 ones look like this:<br/>
        /// <img src="../Help/Images/Question32XP.png" alt="Question Windows XP 32x32"/>
        /// <img src="../Help/Images/Question16XP.png" alt="Question Windows XP 16x16"/>
        /// </para>
        /// <para>
        /// On Linux/Mono the icon contains the following 32x32 and 16x16 images:<br/>
        /// <img src="../Help/Images/Question32Mono.png" alt="Question Linux/Mono 32x32"/>
        /// <img src="../Help/Images/Question16Mono.png" alt="Question Linux/Mono 16x16"/>
        /// <note>On Linux the .NET Core build mistakenly returns the <see cref="SystemError"/> icon, which is an issue in the <see cref="SystemIcons"/> implementation of .NET Core.</note>
        /// </para>
        /// </example>
        public static Icon SystemQuestion => GetSystemIcon(idQuestion, () => SystemIcons.Question);

        /// <summary>
        /// <img src="../Help/Images/Application16W11.png" alt="Application (small version for the summary)"/>
        /// Gets an <see cref="Icon"/> instance that contains the
        /// Application icon as it is displayed by the current operating system.
        /// <br/>On Windows Vista and above sizes range from 16x16 to 256x256, on Windows XP the largest resolution is 48x48, and on Linux the icon has always 32x32 and 16x16 image sizes.
        /// <div style="display: none;"><br/>See the <a href="https://koszeggy.github.io/docs/drawing/html/P_KGySoft_Drawing_Icons_SystemApplication.htm">online help</a> for the icon images.</div>
        /// </summary>
        /// <example>
        /// <para>
        /// On Windows 11 the icon contains 8 images from 16x16 to 256x256. The 32x32 and 16x16 ones look like this:<br/>
        /// <img src="../Help/Images/Application32W11.png" alt="Application 32x32 Windows 11"/>
        /// <img src="../Help/Images/Application16W11.png" alt="Application 16x16 Windows 11"/>
        /// </para>
        /// <para>
        /// On Windows 10 the icon contains 8 images from 16x16 to 256x256. The 32x32 and 16x16 ones look like this:<br/>
        /// <img src="../Help/Images/Application32W10.png" alt="Application 32x32 Windows 10"/>
        /// <img src="../Help/Images/Application16W10.png" alt="Application 16x16 Windows 10"/>
        /// </para>
        /// <para>
        /// On Windows Vista, Windows 7 and Windows 8 the icon contains 5 images from 16x16 to 256x256. The 32x32 and 16x16 ones look like this:<br/>
        /// <img src="../Help/Images/Application32.png" alt="Application 32x32 Windows Vista"/>
        /// <img src="../Help/Images/Application16.png" alt="Application 16x16 Windows Vista"/>
        /// <note>The <see cref="Application"/> property returns a similar icon, but with one additional resolution.</note>
        /// </para>
        /// <para>
        /// On Windows XP the icon contains 3 images from 16x16 to 48x48. The 32x32 and 16x16 ones look like this:<br/>
        /// <img src="../Help/Images/Application32XP.png" alt="Application Windows XP 32x32"/>
        /// <img src="../Help/Images/Application16XP.png" alt="Application Windows XP 16x16"/>
        /// </para>
        /// <para>
        /// On Linux/Mono the icon contains the following images:<br/>
        /// <img src="../Help/Images/Application32Mono.png" alt="Application Linux/Mono 32x32"/>
        /// <img src="../Help/Images/Application16Mono.png" alt="Application Linux/Mono 16x16"/>
        /// <note>On Linux the .NET Core build mistakenly returns the <see cref="SystemError"/> icon, which is an issue in the <see cref="SystemIcons"/> implementation of .NET Core.</note>
        /// </para>
        /// </example>
        public static Icon SystemApplication => GetSystemIcon(idApplication, () => SystemIcons.Application);

        /// <summary>
        /// <img src="../Help/Images/Shield16W11.png" alt="Shield (small version for the summary)"/>
        /// Gets an <see cref="Icon"/> instance that contains the
        /// Shield icon as it is displayed by the current operating system.
        /// <br/>On Windows Vista and above sizes range from 16x16 to 256x256, on Windows XP the largest resolution is 48x48, and on Linux the icon has always 32x32 and 16x16 image sizes.
        /// <div style="display: none;"><br/>See the <a href="https://koszeggy.github.io/docs/drawing/html/P_KGySoft_Drawing_Icons_SystemShield.htm">online help</a> for the icon images.</div>
        /// </summary>
        /// <remarks>
        /// <note>For compatibility reasons in Windows 7 and above this property returns the two-colored UAC shield as it appears in the current operating system.
        /// To return the system-specific counterpart of the <see cref="Shield"/> property (the four-colored Windows shield)
        /// you can use the <see cref="FromFile(string,int)"/> method on Windows 7 and above like this:
        /// <br/><c>Icon systemWindowsShield = Icons.FromFile("imageres", 1028);</c></note>
        /// </remarks>
        /// <example>
        /// <para>
        /// On Windows 11 the icon contains 8 images from 16x16 to 256x256. The 32x32 and 16x16 ones look like this:<br/>
        /// <img src="../Help/Images/Shield32W11.png" alt="Shield 32x32 Windows 11"/>
        /// <img src="../Help/Images/Shield16W11.png" alt="Shield 16x16 Windows 11"/>
        /// </para>
        /// <para>
        /// On Windows 10 the icon contains 8 images from 16x16 to 256x256. The 32x32 and 16x16 ones look like this:<br/>
        /// <img src="../Help/Images/Shield32W10.png" alt="Shield Windows 8/10 32x32 Windows 10"/>
        /// <img src="../Help/Images/Shield16W10.png" alt="Shield Windows 8/10 16x16 Windows 10"/>
        /// </para>
        /// <para>
        /// On Windows 7 and Windows 8 the icon sizes vary from 8x8 to 256x256. Windows 7 has 7 images,
        /// whereas Windows 8 has an additional 64x64 image, which is missing on Windows 7. The 32x32 and 16x16 ones look like this:<br/>
        /// <img src="../Help/Images/SecurityShield32.png" alt="Shield Windows 7 32x32"/>
        /// <img src="../Help/Images/SecurityShield16.png" alt="Shield Windows 7 16x16"/>
        /// </para>
        /// <para>
        /// On Windows Vista the icon contains 7 images from 8x8 to 256x256. The 32x32 and 16x16 ones look like this:<br/>
        /// <img src="../Help/Images/Shield32.png" alt="Shield Windows Vista 32x32"/>
        /// <img src="../Help/Images/Shield16.png" alt="Shield Windows Vista 16x16"/>
        /// </para>
        /// <para>
        /// On Windows XP the icon contains 3 images from 16x16 to 48x48. The 32x32 and 16x16 ones look like this:<br/>
        /// <img src="../Help/Images/Shield32XP.png" alt="Shield Windows XP 32x32"/>
        /// <img src="../Help/Images/Shield16XP.png" alt="Shield Windows XP 16x16"/>
        /// </para>
        /// <para>
        /// On Linux/Mono the icon contains the following 32x32 and 16x16 images:<br/>
        /// <img src="../Help/Images/Shield32Mono.png" alt="Shield Linux/Mono 29x32"/>
        /// <img src="../Help/Images/Shield16Mono.png" alt="Shield Linux/Mono 16x16"/>
        /// <note>On Linux the .NET Core build mistakenly returns the <see cref="SystemError"/> icon, which is an issue in the <see cref="SystemIcons"/> implementation of .NET Core.</note>
        /// </para>
        /// </example>
        public static Icon SystemShield => GetSystemIcon(idShield, () => SystemIcons.Shield);

        /// <summary>
        /// <img src="../Help/Images/SecuritySuccess16W11.png" alt="Security Success (small version for the summary)"/>
        /// Gets an <see cref="Icon"/> instance that contains the
        /// Security Success icon as it is displayed by the current operating system.
        /// <br/>On Windows XP and on Linux this property returns the same icon as the <see cref="SecuritySuccess"/> property.
        /// <div style="display: none;"><br/>See the <a href="https://koszeggy.github.io/docs/drawing/html/P_KGySoft_Drawing_Icons_SystemSecuritySuccess.htm">online help</a> for the icon images.</div>
        /// </summary>
        /// <example>
        /// <para>
        /// On Windows 11 the icon contains 8 images from 16x16 to 256x256. The 32x32 and 16x16 ones look like this:<br/>
        /// <img src="../Help/Images/SecuritySuccess32W11.png" alt="Security Success 32x32 Windows 11"/>
        /// <img src="../Help/Images/SecuritySuccess16W11.png" alt="Security Success 16x16 Windows 11"/>
        /// </para>
        /// <para>
        /// On Windows 10 the icon contains 8 images from 16x16 to 256x256. The 32x32 and 16x16 ones look like this:<br/>
        /// <img src="../Help/Images/SecuritySuccess32W10.png" alt="Security Success 32x32 Windows 10"/>
        /// <img src="../Help/Images/SecuritySuccess16W10.png" alt="Security Success 16x16 Windows 10"/>
        /// </para>
        /// <para>
        /// On Windows Vista, Windows 7 and Windows 8 the icon contains 5 images from 16x16 to 256x256. The 32x32 and 16x16 ones look like this:<br/>
        /// <img src="../Help/Images/SecuritySuccess32.png" alt="Security Success 32x32 Windows Vista"/>
        /// <img src="../Help/Images/SecuritySuccess16.png" alt="Security Success 16x16 Windows Vista"/>
        /// </para>
        /// <para>On Windows XP and Linux/Mono this property returns the same value as the <see cref="SecuritySuccess"/> property.</para>
        /// </example>
        public static Icon SystemSecuritySuccess => GetSystemIcon(idSecuritySuccess, () => SecuritySuccess);

        /// <summary>
        /// <img src="../Help/Images/SecurityWarning16W11.png" alt="Security Warning (small version for the summary)"/>
        /// Gets an <see cref="Icon"/> instance that contains the
        /// Security Warning icon as it is displayed by the current operating system.
        /// <br/>On Windows XP and on Linux this property returns the same icon as the <see cref="SecurityWarning"/> property.
        /// <div style="display: none;"><br/>See the <a href="https://koszeggy.github.io/docs/drawing/html/P_KGySoft_Drawing_Icons_SystemSecurityWarning.htm">online help</a> for the icon images.</div>
        /// </summary>
        /// <example>
        /// <para>
        /// On Windows 11 the icon contains 8 images from 16x16 to 256x256. The 32x32 and 16x16 ones look like this:<br/>
        /// <img src="../Help/Images/SecurityWarning32W11.png" alt="Security Warning 32x32 Windows 11"/>
        /// <img src="../Help/Images/SecurityWarning16W11.png" alt="Security Warning 16x16 Windows 11"/>
        /// </para>
        /// <para>
        /// On Windows 10 the icon contains 8 images from 16x16 to 256x256. The 32x32 and 16x16 ones look like this:<br/>
        /// <img src="../Help/Images/SecurityWarning32W10.png" alt="Security Warning 32x32 Windows 10"/>
        /// <img src="../Help/Images/SecurityWarning16W10.png" alt="Security Warning 16x16 Windows 10"/>
        /// </para>
        /// <para>
        /// On Windows Vista, Windows 7 and Windows 8 the icon contains 5 images from 16x16 to 256x256. The 32x32 and 16x16 ones look like this:<br/>
        /// <img src="../Help/Images/SecurityWarning32.png" alt="Security Warning 32x32 Windows Vista"/>
        /// <img src="../Help/Images/SecurityWarning16.png" alt="Security Warning 16x16 Windows Vista"/>
        /// </para>
        /// <para>On Windows XP and Linux/Mono this property returns the same value as the <see cref="SecurityWarning"/> property.</para>
        /// </example>
        public static Icon SystemSecurityWarning => GetSystemIcon(idSecurityWarning, () => SecurityWarning);

        /// <summary>
        /// <img src="../Help/Images/SecurityQuestion16W11.png" alt="Security Question (small version for the summary)"/>
        /// Gets an <see cref="Icon"/> instance that contains the
        /// Security Question icon as it is displayed by the current operating system.
        /// <br/>On Windows XP and on Linux this property returns the same icon as the <see cref="SecurityQuestion"/> property.
        /// <div style="display: none;"><br/>See the <a href="https://koszeggy.github.io/docs/drawing/html/P_KGySoft_Drawing_Icons_SystemSecurityQuestion.htm">online help</a> for the icon images.</div>
        /// </summary>
        /// <example>
        /// <para>
        /// On Windows 11 the icon contains 8 images from 16x16 to 256x256. The 32x32 and 16x16 ones look like this:<br/>
        /// <img src="../Help/Images/SecurityQuestion32W11.png" alt="Security Question 32x32 Windows 11"/>
        /// <img src="../Help/Images/SecurityQuestion16W11.png" alt="Security Question 16x16 Windows 11"/>
        /// </para>
        /// <para>
        /// On Windows 10 the icon contains 8 images from 16x16 to 256x256. The 32x32 and 16x16 ones look like this:<br/>
        /// <img src="../Help/Images/SecurityQuestion32W10.png" alt="Security Question 32x32 Windows 10"/>
        /// <img src="../Help/Images/SecurityQuestion16W10.png" alt="Security Question 16x16 Windows 10"/>
        /// </para>
        /// <para>
        /// On Windows Vista, Windows 7 and Windows 8 the icon contains 5 images from 16x16 to 256x256. The 32x32 and 16x16 ones look like this:<br/>
        /// <img src="../Help/Images/SecurityQuestion32.png" alt="Security Question 32x32 Windows Vista"/>
        /// <img src="../Help/Images/SecurityQuestion16.png" alt="Security Question 16x16 Windows Vista"/>
        /// </para>
        /// <para>On Windows XP and Linux/Mono this property returns the same value as the <see cref="SecurityQuestion"/> property.</para>
        /// </example>
        public static Icon SystemSecurityQuestion => GetSystemIcon(idSecurityQuestion, () => SecurityQuestion);

        /// <summary>
        /// <img src="../Help/Images/SecurityError16W11.png" alt="Security Error (small version for the summary)"/>
        /// Gets an <see cref="Icon"/> instance that contains the
        /// Security Error icon as it is displayed by the current operating system.
        /// <br/>On Windows XP and on Linux this property returns the same icon as the <see cref="SecurityError"/> property.
        /// <div style="display: none;"><br/>See the <a href="https://koszeggy.github.io/docs/drawing/html/P_KGySoft_Drawing_Icons_SystemSecurityError.htm">online help</a> for the icon images.</div>
        /// </summary>
        /// <example>
        /// <para>
        /// On Windows 11 the icon contains 8 images from 16x16 to 256x256. The 32x32 and 16x16 ones look like this:<br/>
        /// <img src="../Help/Images/SecurityError32W11.png" alt="Security Error 32x32 Windows 11"/>
        /// <img src="../Help/Images/SecurityError16W11.png" alt="Security Error 16x16 Windows 11"/>
        /// </para>
        /// <para>
        /// On Windows 10 the icon contains 8 images from 16x16 to 256x256. The 32x32 and 16x16 ones look like this:<br/>
        /// <img src="../Help/Images/SecurityError32W10.png" alt="Security Error 32x32 Windows 10"/>
        /// <img src="../Help/Images/SecurityError16W10.png" alt="Security Error 16x16 Windows 10"/>
        /// </para>
        /// <para>
        /// On Windows Vista, Windows 7 and Windows 8 the icon contains 5 images from 16x16 to 256x256. The 32x32 and 16x16 ones look like this:<br/>
        /// <img src="../Help/Images/SecurityError32.png" alt="Security Error 32x32 Windows Vista"/>
        /// <img src="../Help/Images/SecurityError16.png" alt="Security Error 16x16 Windows Vista"/>
        /// </para>
        /// <para>On Windows XP and Linux/Mono this property returns the same value as the <see cref="SecurityError"/> property.</para>
        /// </example>
        public static Icon SystemSecurityError => GetSystemIcon(idSecurityError, () => SecurityError);

        /// <summary>
        /// <img src="../Help/Images/Information16.png" alt="Information (small version for the summary)"/>
        /// Gets the Information icon displaying a white "i" in a blue circle (Sizes: 256x256, 64x64, 48x48, 32x32, 24x24, 20x20, 16x16)
        /// <div style="display: none;"><br/>See the <a href="https://koszeggy.github.io/docs/drawing/html/P_KGySoft_Drawing_Icons_Information.htm">online help</a> for the icon images.</div>
        /// </summary>
        /// <remarks>
        /// <note>This icon has a Windows Vista-like appearance on all platforms. To get an OS-specific appearance use the <see cref="SystemInformation"/>
        /// property instead (though that one lacks the 20x20, 24x24 and 64x64 sizes on Windows Vista and Windows 7).</note>
        /// </remarks>
        /// <example>
        /// <para>
        /// The icon contains the following images:<br/>
        /// <img src="../Help/Images/Information256.png" alt="Information 256x256"/>
        /// <img src="../Help/Images/Information64.png" alt="Information 64x64"/>
        /// <img src="../Help/Images/Information48.png" alt="Information 48x48"/>
        /// <img src="../Help/Images/Information32.png" alt="Information 32x32"/>
        /// <img src="../Help/Images/Information24.png" alt="Information 24x24"/>
        /// <img src="../Help/Images/Information20.png" alt="Information 20x20"/>
        /// <img src="../Help/Images/Information16.png" alt="Information 16x16"/>
        /// </para>
        /// </example>
        public static Icon Information => GetResourceIcon(nameof(Information));

        /// <summary>
        /// <img src="../Help/Images/Warning16.png" alt="Warning (small version for the summary)"/>
        /// Gets the Warning icon displaying a black "!" in a yellow triangle (Sizes: 256x256, 64x64, 48x48, 32x32, 24x24, 20x20, 16x16)
        /// <div style="display: none;"><br/>See the <a href="https://koszeggy.github.io/docs/drawing/html/P_KGySoft_Drawing_Icons_Warning.htm">online help</a> for the icon images.</div>
        /// </summary>
        /// <remarks>
        /// <note>This icon has a Windows Vista-like appearance on all platforms. To get an OS-specific appearance use the <see cref="SystemWarning"/>
        /// property instead (though that one lacks the 20x20, 24x24 and 64x64 sizes on Windows Vista and Windows 7/8).</note>
        /// </remarks>
        /// <example>
        /// <para>
        /// The icon contains the following images:<br/>
        /// <img src="../Help/Images/Warning256.png" alt="Warning 256x256"/>
        /// <img src="../Help/Images/Warning64.png" alt="Warning 64x64"/>
        /// <img src="../Help/Images/Warning48.png" alt="Warning 48x48"/>
        /// <img src="../Help/Images/Warning32.png" alt="Warning 32x32"/>
        /// <img src="../Help/Images/Warning24.png" alt="Warning 24x24"/>
        /// <img src="../Help/Images/Warning20.png" alt="Warning 20x20"/>
        /// <img src="../Help/Images/Warning16.png" alt="Warning 16x16"/>
        /// </para>
        /// </example>
        public static Icon Warning => GetResourceIcon(nameof(Warning));

        /// <summary>
        /// <img src="../Help/Images/Question16.png" alt="Question (small version for the summary)"/>
        /// Gets the Question icon displaying a white "?" in a blue circle (Sizes: 256x256, 64x64, 48x48, 32x32, 24x24, 20x20, 16x16)
        /// <div style="display: none;"><br/>See the <a href="https://koszeggy.github.io/docs/drawing/html/P_KGySoft_Drawing_Icons_Question.htm">online help</a> for the icon images.</div>
        /// </summary>
        /// <remarks>
        /// <note>This icon has a Windows Vista-like appearance on all platforms. To get an OS-specific appearance use the <see cref="SystemQuestion"/>
        /// property instead (though that one lacks the 20x20 and 24x24 sizes on Windows Vista and Windows 7).</note>
        /// </remarks>
        /// <example>
        /// <para>
        /// The icon contains the following images:<br/>
        /// <img src="../Help/Images/Question256.png" alt="Question 256x256"/>
        /// <img src="../Help/Images/Question64.png" alt="Question 64x64"/>
        /// <img src="../Help/Images/Question48.png" alt="Question 48x48"/>
        /// <img src="../Help/Images/Question32.png" alt="Question 32x32"/>
        /// <img src="../Help/Images/Question24.png" alt="Question 24x24"/>
        /// <img src="../Help/Images/Question20.png" alt="Question 20x20"/>
        /// <img src="../Help/Images/Question16.png" alt="Question 16x16"/>
        /// </para>
        /// </example>
        public static Icon Question => GetResourceIcon(nameof(Question));

        /// <summary>
        /// <img src="../Help/Images/Error16.png" alt="Error (small version for the summary)"/>
        /// Gets the Error icon displaying a white "X" in a red circle (Sizes: 256x256, 64x64, 48x48, 32x32, 24x24, 20x20, 16x16)
        /// <div style="display: none;"><br/>See the <a href="https://koszeggy.github.io/docs/drawing/html/P_KGySoft_Drawing_Icons_Error.htm">online help</a> for the icon images.</div>
        /// </summary>
        /// <remarks>
        /// <note>This icon has a Windows Vista-like appearance on all platforms. To get an OS-specific appearance use the <see cref="SystemError"/>
        /// property instead (though that one lacks the 20x20 and 64x64 sizes on Windows Vista and Windows 7/8).</note>
        /// </remarks>
        /// <example>
        /// <para>
        /// The icon contains the following images:<br/>
        /// <img src="../Help/Images/Error256.png" alt="Error 256x256"/>
        /// <img src="../Help/Images/Error64.png" alt="Error 64x64"/>
        /// <img src="../Help/Images/Error48.png" alt="Error 48x48"/>
        /// <img src="../Help/Images/Error32.png" alt="Error 32x32"/>
        /// <img src="../Help/Images/Error24.png" alt="Error 24x24"/>
        /// <img src="../Help/Images/Error20.png" alt="Error 20x20"/>
        /// <img src="../Help/Images/Error16.png" alt="Error 16x16"/>
        /// </para>
        /// </example>
        public static Icon Error => GetResourceIcon(nameof(Error));

        /// <summary>
        /// <img src="../Help/Images/Shield16.png" alt="Shield (small version for the summary)"/>
        /// Gets the Windows Shield icon displaying a red-green-blue-yellow shield (Sizes: 256x256, 128x128, 48x48, 32x32, 24x24, 16x16, 8x8)
        /// <div style="display: none;"><br/>See the <a href="https://koszeggy.github.io/docs/drawing/html/P_KGySoft_Drawing_Icons_Shield.htm">online help</a> for the icon images.</div>
        /// </summary>
        /// <remarks>
        /// <note>This icon has the Windows Vista appearance on all platforms. To get an OS-specific appearance use the <see cref="SystemShield"/> property, which returns
        /// the UAC shield on Windows 7 and above. To get the OS-specific four-colored Windows shield on Window 7 and above, use the <see cref="FromFile(string,int)"/> method
        /// like this: <c>Icons.FromFile("imageres", 1028)</c></note>
        /// </remarks>
        /// <example>
        /// <para>
        /// The icon contains the following images:<br/>
        /// <img src="../Help/Images/Shield256.png" alt="Windows Shield 256x256"/>
        /// <img src="../Help/Images/Shield128.png" alt="Windows Shield 128x128"/>
        /// <img src="../Help/Images/Shield48.png" alt="Windows Shield 48x48"/>
        /// <img src="../Help/Images/Shield32.png" alt="Windows Shield 32x32"/>
        /// <img src="../Help/Images/Shield24.png" alt="Windows Shield 24x24"/>
        /// <img src="../Help/Images/Shield16.png" alt="Windows Shield 16x16"/>
        /// <img src="../Help/Images/Shield8.png" alt="Windows Shield 8x8"/>
        /// </para>
        /// </example>
        public static Icon Shield => GetResourceIcon(nameof(Shield));

        /// <summary>
        /// <img src="../Help/Images/SecurityShield16.png" alt="Security Shield (small version for the summary)"/>
        /// Gets the Security Shield icon displaying a blue-yellow shield (Sizes: 256x256, 128x128, 48x48, 32x32, 24x24, 16x16, 8x8)
        /// <div style="display: none;"><br/>See the <a href="https://koszeggy.github.io/docs/drawing/html/P_KGySoft_Drawing_Icons_SecurityShield.htm">online help</a> for the icon images.</div>
        /// </summary>
        /// <remarks>
        /// <note>This icon has the Windows Vista appearance on all platforms. To get an OS-specific appearance use the <see cref="SystemShield"/> property instead.</note>
        /// </remarks>
        /// <example>
        /// <para>
        /// The icon contains the following images:<br/>
        /// <img src="../Help/Images/SecurityShield256.png" alt="Security Shield 256x256"/>
        /// <img src="../Help/Images/SecurityShield128.png" alt="Security Shield 128x128"/>
        /// <img src="../Help/Images/SecurityShield48.png" alt="Security Shield 48x48"/>
        /// <img src="../Help/Images/SecurityShield32.png" alt="Security Shield 32x32"/>
        /// <img src="../Help/Images/SecurityShield24.png" alt="Security Shield 24x24"/>
        /// <img src="../Help/Images/SecurityShield16.png" alt="Security Shield 16x16"/>
        /// <img src="../Help/Images/SecurityShield8.png" alt="Security Shield 8x8"/>
        /// </para>
        /// </example>
        public static Icon SecurityShield => GetResourceIcon(nameof(SecurityShield));

        /// <summary>
        /// <img src="../Help/Images/SecuritySuccess16.png" alt="Security Success (small version for the summary)"/>
        /// Gets the Security Success icon displaying a green shield with a white check (Sizes: 256x256, 48x48, 32x32, 24x24, 16x16)
        /// <div style="display: none;"><br/>See the <a href="https://koszeggy.github.io/docs/drawing/html/P_KGySoft_Drawing_Icons_SecuritySuccess.htm">online help</a> for the icon images.</div>
        /// </summary>
        /// <remarks>
        /// <note>This icon has the Windows Vista appearance on all platforms. To get an OS-specific appearance use the <see cref="SystemSecuritySuccess"/> property instead.</note>
        /// </remarks>
        /// <example>
        /// <para>
        /// The icon contains the following images:<br/>
        /// <img src="../Help/Images/SecuritySuccess256.png" alt="Security Success 256x256"/>
        /// <img src="../Help/Images/SecuritySuccess48.png" alt="Security Success 48x48"/>
        /// <img src="../Help/Images/SecuritySuccess32.png" alt="Security Success 32x32"/>
        /// <img src="../Help/Images/SecuritySuccess24.png" alt="Security Success 24x24"/>
        /// <img src="../Help/Images/SecuritySuccess16.png" alt="Security Success 16x16"/>
        /// </para>
        /// </example>
        public static Icon SecuritySuccess => GetResourceIcon(nameof(SecuritySuccess));

        /// <summary>
        /// <img src="../Help/Images/SecurityWarning16.png" alt="Security Warning (small version for the summary)"/>
        /// Gets the Security Warning icon displaying a yellow shield with a black "!" (Sizes: 256x256, 48x48, 32x32, 24x24, 16x16)
        /// <div style="display: none;"><br/>See the <a href="https://koszeggy.github.io/docs/drawing/html/P_KGySoft_Drawing_Icons_SecurityWarning.htm">online help</a> for the icon images.</div>
        /// </summary>
        /// <remarks>
        /// <note>This icon has the Windows Vista appearance on all platforms. To get an OS-specific appearance use the <see cref="SystemSecurityWarning"/> property instead.</note>
        /// </remarks>
        /// <example>
        /// <para>
        /// The icon contains the following images:<br/>
        /// <img src="../Help/Images/SecurityWarning256.png" alt="Security Warning 256x256"/>
        /// <img src="../Help/Images/SecurityWarning48.png" alt="Security Warning 48x48"/>
        /// <img src="../Help/Images/SecurityWarning32.png" alt="Security Warning 32x32"/>
        /// <img src="../Help/Images/SecurityWarning24.png" alt="Security Warning 24x24"/>
        /// <img src="../Help/Images/SecurityWarning16.png" alt="Security Warning 16x16"/>
        /// </para>
        /// </example>
        public static Icon SecurityWarning => GetResourceIcon(nameof(SecurityWarning));

        /// <summary>
        /// <img src="../Help/Images/SecurityQuestion16.png" alt="Security Question (small version for the summary)"/>
        /// Gets the Security Question icon displaying a blue shield with a white "?" (Sizes: 256x256, 48x48, 32x32, 24x24, 16x16)
        /// <div style="display: none;"><br/>See the <a href="https://koszeggy.github.io/docs/drawing/html/P_KGySoft_Drawing_Icons_SecurityQuestion.htm">online help</a> for the icon images.</div>
        /// </summary>
        /// <remarks>
        /// <note>This icon has the Windows Vista appearance on all platforms. To get an OS-specific appearance use the <see cref="SystemSecurityQuestion"/> property instead.</note>
        /// </remarks>
        /// <example>
        /// <para>
        /// The icon contains the following images:<br/>
        /// <img src="../Help/Images/SecurityQuestion256.png" alt="Security Question 256x256"/>
        /// <img src="../Help/Images/SecurityQuestion48.png" alt="Security Question 48x48"/>
        /// <img src="../Help/Images/SecurityQuestion32.png" alt="Security Question 32x32"/>
        /// <img src="../Help/Images/SecurityQuestion24.png" alt="Security Question 24x24"/>
        /// <img src="../Help/Images/SecurityQuestion16.png" alt="Security Question 16x16"/>
        /// </para>
        /// </example>
        public static Icon SecurityQuestion => GetResourceIcon(nameof(SecurityQuestion));

        /// <summary>
        /// <img src="../Help/Images/SecurityError16.png" alt="Security Error (small version for the summary)"/>
        /// Gets the Security Error icon displaying a red shield with a white "X" (Sizes: 256x256, 48x48, 32x32, 24x24, 16x16)
        /// <div style="display: none;"><br/>See the <a href="https://koszeggy.github.io/docs/drawing/html/P_KGySoft_Drawing_Icons_SecurityError.htm">online help</a> for the icon images.</div>
        /// </summary>
        /// <remarks>
        /// <note>This icon has the Windows Vista appearance on all platforms. To get an OS-specific appearance use the <see cref="SystemSecurityError"/> property instead.</note>
        /// </remarks>
        /// <example>
        /// <para>
        /// The icon contains the following images:<br/>
        /// <img src="../Help/Images/SecurityError256.png" alt="Security Error 256x256"/>
        /// <img src="../Help/Images/SecurityError48.png" alt="Security Error 48x48"/>
        /// <img src="../Help/Images/SecurityError32.png" alt="Security Error 32x32"/>
        /// <img src="../Help/Images/SecurityError24.png" alt="Security Error 24x24"/>
        /// <img src="../Help/Images/SecurityError16.png" alt="Security Error 16x16"/>
        /// </para>
        /// </example>
        public static Icon SecurityError => GetResourceIcon(nameof(SecurityError));

        /// <summary>
        /// <img src="../Help/Images/Application16.png" alt="Application (small version for the summary)"/>
        /// Gets the Application icon displaying a window (Sizes: 256x256, 64x64, 48x48, 32x32, 24x24, 16x16)
        /// <div style="display: none;"><br/>See the <a href="https://koszeggy.github.io/docs/drawing/html/P_KGySoft_Drawing_Icons_Application.htm">online help</a> for the icon images.</div>
        /// </summary>
        /// <remarks>
        /// <note>This icon has a Windows Vista-like appearance on all platforms. To get an OS-specific appearance use the <see cref="SystemApplication"/>
        /// property instead (though that one lacks the 64x64 size image on Windows Vista and Windows 7/8).</note>
        /// </remarks>
        /// <example>
        /// <para>
        /// The icon contains the following images:<br/>
        /// <img src="../Help/Images/Application256.png" alt="Application 256x256"/>
        /// <img src="../Help/Images/Application64.png" alt="Application 64x64"/>
        /// <img src="../Help/Images/Application48.png" alt="Application 48x48"/>
        /// <img src="../Help/Images/Application32.png" alt="Application 32x32"/>
        /// <img src="../Help/Images/Application24.png" alt="Application 24x24"/>
        /// <img src="../Help/Images/Application16.png" alt="Application 16x16"/>
        /// </para>
        /// </example>
        public static Icon Application => GetResourceIcon(nameof(Application));

        #endregion

        #region Private Properties

        private static ResourceManager ResourceManager
        {
            get
            {
                if (resourceManager == null)
                    Interlocked.CompareExchange(ref resourceManager, new ResourceManager(typeof(Icons)), null);
                return resourceManager;
            }
        }

        private static ThreadSafeDictionary<int, RawIcon?> SystemIconsCache
        {
            get
            {
                if (systemIconsCache == null)
                    Interlocked.CompareExchange(ref systemIconsCache, new ThreadSafeDictionary<int, RawIcon?>(), null);
                return systemIconsCache;
            }
        }

        private static ThreadSafeDictionary<string, RawIcon> ResourceIconsCache
        {
            get
            {
                if (resourceIconsCache == null)
                    Interlocked.CompareExchange(ref resourceIconsCache, new ThreadSafeDictionary<string, RawIcon>(), null);
                return resourceIconsCache;
            }
        }

        #endregion

        #endregion

        #region Methods

        #region Public Methods

        /// <summary>
        /// Tries to get a system stock icon. When there is no icon defined for provided <paramref name="id"/>,
        /// or Windows version is below Vista, this method returns <see langword="null"/>.
        /// On Windows XP use the predefined property members to retrieve system icons.
        /// </summary>
        /// <param name="id">The identifier of the icon to retrieve. For future compatibility reasons non-defined <see cref="StockIcon"/> values are also allowed.</param>
        /// <returns>An <see cref="Icon"/> instance containing a small and large icon when an icon belongs to <paramref name="id"/>, or <see langword="null"/>,
        /// when no icon found or Windows version is below Vista, or the method is called in a non-Windows environment.</returns>
        /// <remarks>
        /// <note>On non-Windows platforms this method always returns <see langword="null"/>.</note>
        /// </remarks>
        public static Icon? GetStockIcon(StockIcon id) => GetSystemIcon((int)id & stockIconIdMask, null);

        /// <summary>
        /// Extracts icons of the specified <paramref name="size"/> from a file and returns them as separated <see cref="Icon"/> instances.
        /// </summary>
        /// <param name="fileName">The name of the file. Can be an executable file, a .dll or icon file.</param>
        /// <param name="size">The size of the icons to be extracted.</param>
        /// <returns>The icons of the specified file, or an empty array if the file does not exist or does not contain any icons.</returns>
        /// <remarks>
        /// <para>The actual resolution represented by <paramref name="size"/> depends on the DPI of the main display. To retrieve all actual icon images use the <see cref="FromFile(string)"/>
        /// overload instead, and then you can extract the exact resolution by the <see cref="IconExtensions.ExtractIcon(Icon,Size)">IconExtensions.ExtractIcon</see> extension method.</para>
        /// <para>If <paramref name="fileName"/> refers to an icon file use the <see cref="Icon(string)"/> constructor instead.</para>
        /// <para>The images of an <see cref="Icon"/> can be extracted by the <see cref="O:KGySoft.Drawing.IconExtensions.ExtractBitmaps">IconExtensions.ExtractBitmaps</see> methods.</para>
        /// <note>On non-Windows platforms this method always returns an empty array.</note>
        /// </remarks>
        [SecuritySafeCritical]
        public static Icon[] FromFile(string fileName, SystemIconSize size)
        {
            if (fileName == null)
                throw new ArgumentNullException(nameof(fileName), PublicResources.ArgumentNull);
            if (!Enum<SystemIconSize>.IsDefined(size))
                throw new ArgumentOutOfRangeException(nameof(size), PublicResources.EnumOutOfRangeWithValues(size));
            if (!OSUtils.IsWindows)
                return Reflector.EmptyArray<Icon>();

            IntPtr[][] handles = Shell32.ExtractIconHandles(fileName, size);
            Icon[] result = new Icon[handles.Length];

            for (int i = 0; i < handles.Length; i++)
                result[i] = Icon.FromHandle(handles[i][0]).ToManagedIcon();

            return result;
        }

        /// <summary>
        /// Extracts every icon from a file and returns them as separated <see cref="Icon"/> instances.
        /// </summary>
        /// <param name="fileName">The name of the file. Can be an executable file, a .dll or icon file.</param>
        /// <returns>The icons of the specified file, or an empty array if the file does not contain any icons.</returns>
        /// <remarks>
        /// <para>If <paramref name="fileName"/> refers to an icon file it is recommended to use the <see cref="Icon(string)"/> constructor instead.</para>
        /// <para>The images of an <see cref="Icon"/> can be extracted by the <see cref="O:KGySoft.Drawing.IconExtensions.ExtractBitmaps">IconExtensions.ExtractBitmaps</see> methods.</para>
        /// <note>On non-Windows platforms this method works for icon files (.ico) only.</note>
        /// </remarks>
        [SecuritySafeCritical]
        public static Icon[] FromFile(string fileName)
        {
            if (fileName == null)
                throw new ArgumentNullException(nameof(fileName), PublicResources.ArgumentNull);

            if (!OSUtils.IsWindows)
            {
                using var stream = File.OpenRead(fileName);
                return new RawIcon(stream).ExtractIcons(true).Where(i => i != null).ToArray()!;
            }

            IntPtr hModule = Kernel32.LoadLibraryData(fileName);
            try
            {
                var data = new List<RawIcon>();
                EnumResNameProc enumFunc = (handle, _, resName, _) =>
                {
                    data.Add(GetModuleIcon(handle, resName));
                    return true;
                };

                Kernel32.EnumResourceNames(hModule, Constants.RT_GROUP_ICON, enumFunc);
                GC.KeepAlive(enumFunc);

                var result = new Icon[data.Count];
                for (int i = 0; i < data.Count; i++)
                {
                    using RawIcon rawIcon = data[i];
                    result[i] = rawIcon.ToIcon(OSUtils.IsXpOrEarlier)!;
                }

                return result;
            }
            finally
            {
                Kernel32.FreeLibrary(hModule);
            }
        }

        /// <summary>
        /// Extracts the icon with the specified integer identifier from a file.
        /// </summary>
        /// <param name="fileName">The name of the file. Can be an executable file, a .dll or icon file. If the extension is omitted, .dll is applied.</param>
        /// <param name="id">The integer identifier of the icon resource withing the file.</param>
        /// <returns>The icon of the specified identifier.</returns>
        /// <remarks>
        /// <para>If <paramref name="fileName"/> refers to an icon file it is recommended to use the <see cref="Icon(string)"/> constructor instead.</para>
        /// <para>The images of an <see cref="Icon"/> can be extracted by the <see cref="O:KGySoft.Drawing.IconExtensions.ExtractBitmaps">IconExtensions.ExtractBitmaps</see> methods.</para>
        /// <para>If <paramref name="fileName"/> has no full path, then it refers to a system library. For example, <c>Icons.FromFile("shell32", 13)</c> loads a chip icon from <c>shell32.dll</c>.</para>
        /// <note>This method is supported on Windows only.</note>
        /// </remarks>
        [SecuritySafeCritical]
        public static Icon FromFile(string fileName, int id)
        {
            if (fileName == null)
                throw new ArgumentNullException(nameof(fileName), PublicResources.ArgumentNull);
            if (!OSUtils.IsWindows)
                throw new PlatformNotSupportedException(DrawingRes.RequiresWindows);

            using RawIcon result = DoLoadIconFromFile(fileName, id, true)!;
            return result.ToIcon(OSUtils.IsXpOrEarlier)!;
        }

        /// <summary>
        /// Gets the system-associated icon of a file or an extension.
        /// </summary>
        /// <param name="fileOrExtension">A file name (can be a non-existing one) or an extension (with or without a leading dot character)
        /// for which the associated icon is about to be retrieved.</param>
        /// <param name="size">The size of the icon to be retrieved.</param>
        /// <returns>The system-associated icon of the specified file or extension.</returns>
        /// <remarks>
        /// <para>If <paramref name="size"/> is <see cref="SystemIconSize.Large"/> and <paramref name="fileOrExtension"/> is an existing file, then the result
        /// is usually the same as for the <see cref="Icon.ExtractAssociatedIcon">Icon.ExtractAssociatedIcon</see> method.</para>
        /// <note>On non-Windows platforms this method always returns the <see cref="SystemIcons.WinLogo">SystemIcons.WinLogo</see> icon.</note>
        /// </remarks>
        [SecuritySafeCritical]
        public static Icon FromExtension(string fileOrExtension, SystemIconSize size)
        {
            // NOTE: we could implement an overload without the size parameter, and add ExtraLarge/Jumbo icons to the result, but that requires
            // determining whether Comctl32.dll version 6.0 is loaded, which typically happens in a Windows Forms application when visual styles are enabled.
            // This is needed to use the IImageList COM interface to retrieve the larger icons by SHGetImageList.
            if (fileOrExtension == null)
                throw new ArgumentNullException(nameof(fileOrExtension), PublicResources.ArgumentNull);
            if (!Enum<SystemIconSize>.IsDefined(size))
                throw new ArgumentOutOfRangeException(nameof(size), PublicResources.EnumOutOfRangeWithValues(size));
            if (!OSUtils.IsWindows)
                return SystemIcons.WinLogo;

            if (!Path.HasExtension(fileOrExtension))
                fileOrExtension = Path.GetFileName(fileOrExtension) == fileOrExtension ? '.' + fileOrExtension : ".";
            
            IntPtr handle = Shell32.GetFileIconHandle(fileOrExtension, size);
            if (handle == IntPtr.Zero)
                throw new ArgumentException(PublicResources.ArgumentInvalidString, nameof(fileOrExtension));

            return Icon.FromHandle(handle).ToManagedIcon();
        }

        /// <summary>
        /// Loads an <see cref="Icon"/> from the specified <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">The stream to load the icon from.</param>
        /// <returns>The <see cref="Icon"/> loaded from the <paramref name="stream"/>, or <see langword="null"/>, if <paramref name="stream"/> contains an empty icon header.</returns>
        /// <remarks>
        /// <para>The result <see cref="Icon"/> is compatible with Windows XP if the method is executed in a Windows XP environment.</para>
        /// </remarks>
        public static Icon? FromStream(Stream stream) => FromStream(stream, OSUtils.IsXpOrEarlier);

        /// <summary>
        /// Loads an <see cref="Icon"/> from the specified <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">The stream to load the icon from.</param>
        /// <param name="forceUncompressedResult"><see langword="true"/> to force returning an uncompressed icon;
        /// <see langword="false"/> to allow PNG compression, which is supported by Windows Vista and above.</param>
        /// <returns>The <see cref="Icon"/> loaded from the <paramref name="stream"/>, or <see langword="null"/>, if <paramref name="stream"/> contains an empty icon header.</returns>
        [SecuritySafeCritical]
        public static Icon? FromStream(Stream stream, bool forceUncompressedResult)
        {
            using var rawIcon = new RawIcon(stream);
            return rawIcon.ToIcon(forceUncompressedResult);
        }

        /// <summary>
        /// Combines the provided <paramref name="icons"/> into a multi-resolution <see cref="Icon"/> instance.
        /// </summary>
        /// <param name="icons">The icons to be combined.</param>
        /// <returns>An <see cref="Icon"/> instance that contains every image of the source <paramref name="icons"/>.</returns>
        /// <remarks>
        /// <para>The elements of <paramref name="icons"/> may contain multiple icons.</para>
        /// <para>The result <see cref="Icon"/> is compatible with Windows XP if the method is executed in a Windows XP environment.</para>
        /// </remarks>
        [return:NotNullIfNotNull(nameof(icons))]public static Icon? Combine(IEnumerable<Icon>? icons)
            => Combine(OSUtils.IsXpOrEarlier, icons);

        /// <summary>
        /// Combines the provided <paramref name="icons"/> into a multi-resolution <see cref="Icon"/> instance.
        /// </summary>
        /// <param name="icons">The icons to be combined.</param>
        /// <returns>An <see cref="Icon"/> instance that contains every image of the source <paramref name="icons"/>.</returns>
        /// <remarks>
        /// <para>The elements of <paramref name="icons"/> may contain multiple icons.</para>
        /// <para>The result <see cref="Icon"/> is compatible with Windows XP if the method is executed in a Windows XP environment.</para>
        /// </remarks>
        [return:NotNullIfNotNull(nameof(icons))]public static Icon? Combine(params Icon[]? icons)
            => Combine(OSUtils.IsXpOrEarlier, (IEnumerable<Icon>?)icons);

        /// <summary>
        /// Combines the provided <paramref name="icons"/> into a multi-resolution <see cref="Icon"/> instance.
        /// </summary>
        /// <param name="forceUncompressedResult"><see langword="true"/> to force returning an uncompressed icon;
        /// <see langword="false"/> to allow PNG compression, which is supported by Windows Vista and above.</param>
        /// <param name="icons">The icons to be combined.</param>
        /// <returns>An <see cref="Icon"/> instance that contains every image of the source <paramref name="icons"/>.</returns>
        /// <remarks>The elements of <paramref name="icons"/> may contain multiple icons.</remarks>
        [return:NotNullIfNotNull(nameof(icons))]public static Icon? Combine(bool forceUncompressedResult, params Icon[]? icons)
            => Combine(forceUncompressedResult, (IEnumerable<Icon>?)icons);

        /// <summary>
        /// Combines the provided <paramref name="icons"/> into a multi-resolution <see cref="Icon"/> instance.
        /// </summary>
        /// <param name="forceUncompressedResult"><see langword="true"/> to force returning an uncompressed icon;
        /// <see langword="false"/> to allow PNG compression, which is supported by Windows Vista and above.</param>
        /// <param name="icons">The icons to be combined.</param>
        /// <returns>An <see cref="Icon"/> instance that contains every image of the source <paramref name="icons"/>.</returns>
        /// <remarks>The elements of <paramref name="icons"/> may contain multiple icons.</remarks>
        [SecuritySafeCritical]
        [return:NotNullIfNotNull(nameof(icons))]public static Icon? Combine(bool forceUncompressedResult, IEnumerable<Icon>? icons)
        {
            if (icons == null)
                return null;

            using var rawIcon = new RawIcon();
            foreach (Icon icon in icons)
                rawIcon.Add(icon);
            return rawIcon.ToIcon(forceUncompressedResult)!;
        }

        /// <summary>
        /// Combines the provided <paramref name="images"/> into a multi-resolution <see cref="Icon"/> instance.
        /// </summary>
        /// <param name="images">The images to be added to the result icon. Images can be non-squared ones.
        /// Transparency is determined automatically by image format.</param>
        /// <returns>An <see cref="Icon"/> instance that contains every image of the source <paramref name="images"/>.</returns>
        /// <remarks>
        /// <para>The result <see cref="Icon"/> is compatible with Windows XP if the method is executed in a Windows XP environment.</para>
        /// <para>The elements of <paramref name="images"/> may contain multiple icons.</para>
        /// </remarks>
        [return:NotNullIfNotNull(nameof(images))]public static Icon? Combine(params Bitmap[]? images)
            => Combine(OSUtils.IsXpOrEarlier, (IEnumerable<Bitmap>?)images);

        /// <summary>
        /// Combines the provided <paramref name="images"/> into a multi-resolution <see cref="Icon"/> instance.
        /// </summary>
        /// <param name="images">The images to be added to the result icon. Images can be non-squared ones.
        /// Transparency is determined automatically by image format.</param>
        /// <returns>An <see cref="Icon"/> instance that contains every image of the source <paramref name="images"/>.</returns>
        /// <remarks>
        /// <para>The result <see cref="Icon"/> is compatible with Windows XP if the method is executed in a Windows XP environment.</para>
        /// <para>The elements of <paramref name="images"/> may contain multiple icons.</para>
        /// </remarks>
        [return:NotNullIfNotNull(nameof(images))]public static Icon? Combine(IEnumerable<Bitmap>? images)
            => Combine(OSUtils.IsXpOrEarlier, images);

        /// <summary>
        /// Combines the provided <paramref name="images"/> into a multi-resolution <see cref="Icon"/> instance.
        /// </summary>
        /// <param name="forceUncompressedResult"><see langword="true"/> to force returning an uncompressed icon;
        /// <see langword="false"/> to allow PNG compression, which is supported by Windows Vista and above.</param>
        /// <param name="images">The images to be added to the result icon. Images can be non-squared ones.
        /// Transparency is determined automatically by image format.</param>
        /// <returns>An <see cref="Icon"/> instance that contains every image of the source <paramref name="images"/>.</returns>
        /// <remarks>
        /// <para>The elements of <paramref name="images"/> may contain multiple icons.</para>
        /// </remarks>
        [return:NotNullIfNotNull(nameof(images))]public static Icon? Combine(bool forceUncompressedResult, params Bitmap[]? images)
            => Combine(forceUncompressedResult, (IEnumerable<Bitmap>?)images);

        /// <summary>
        /// Combines the provided <paramref name="images"/> into a multi-resolution <see cref="Icon"/> instance.
        /// </summary>
        /// <param name="forceUncompressedResult"><see langword="true"/> to force returning an uncompressed icon;
        /// <see langword="false"/> to allow PNG compression, which is supported by Windows Vista and above.</param>
        /// <param name="images">The images to be added to the result icon. Images can be non-squared ones.
        /// Transparency is determined automatically by image format.</param>
        /// <returns>An <see cref="Icon"/> instance that contains every image of the source <paramref name="images"/>.</returns>
        /// <remarks>
        /// <para>The elements of <paramref name="images"/> may contain multiple icons.</para>
        /// </remarks>
        [SecuritySafeCritical]
        [return:NotNullIfNotNull(nameof(images))]public static Icon? Combine(bool forceUncompressedResult, IEnumerable<Bitmap>? images)
        {
            if (images == null)
                return null;

            using var rawIcon = new RawIcon();
            foreach (Bitmap image in images)
                rawIcon.Add(image);
            return rawIcon.ToIcon(forceUncompressedResult)!;
        }

        /// <summary>
        /// Combines the provided <paramref name="images"/> into a multi-resolution <see cref="Icon"/> instance.
        /// </summary>
        /// <param name="images">The images to be added to the icon. Images can be non-squares ones.</param>
        /// <param name="transparentColors">An array of transparent colors of the images. The array must have as many elements as <paramref name="images"/>.</param>
        /// <returns>
        /// An <see cref="Icon"/> instance that contains every image of the source <paramref name="images"/>.
        /// </returns>
        /// <remarks>
        /// <para>The result <see cref="Icon"/> is compatible with Windows XP if the method is executed in a Windows XP environment.</para>
        /// <para>The elements of <paramref name="images"/> may contain multiple icons.</para>
        /// </remarks>
        [return:NotNullIfNotNull(nameof(images))]public static Icon? Combine(Bitmap[]? images, Color[]? transparentColors)
            => Combine(images, transparentColors, OSUtils.IsXpOrEarlier);

        /// <summary>
        /// Combines the provided <paramref name="images"/> into a multi-resolution <see cref="Icon"/> instance.
        /// </summary>
        /// <param name="images">The images to be added to the icon. Images can be non-squares ones.</param>
        /// <param name="transparentColors">An array of transparent colors of the images. The array must have as many elements as <paramref name="images"/>.</param>
        /// <param name="forceUncompressedResult"><see langword="true"/> to force returning an uncompressed icon;
        /// <see langword="false"/> to allow PNG compression, which is supported by Windows Vista and above.</param>
        /// <returns>
        /// An <see cref="Icon"/> instance that contains every image of the source <paramref name="images"/>.
        /// </returns>
        /// <remarks>
        /// <para>The elements of <paramref name="images"/> may contain multiple icons.</para>
        /// </remarks>
        [SecuritySafeCritical]
        [return:NotNullIfNotNull(nameof(images))]public static Icon? Combine(Bitmap[]? images, Color[]? transparentColors, bool forceUncompressedResult)
        {
            int imageCount = images?.Length ?? 0;
            int colorCount = transparentColors?.Length ?? 0;
            if (imageCount != colorCount)
                throw new ArgumentException(DrawingRes.IconExtensionsImagesColorsDifferentLength);

            if (images == null || transparentColors == null || imageCount == 0)
                return null;

            using var rawIcon = new RawIcon();
            for (int i = 0; i < imageCount; i++)
                rawIcon.Add(images[i], transparentColors[i]);
            return rawIcon.ToIcon(forceUncompressedResult)!;
        }

        #endregion

        #region Internal Methods

        [SecurityCritical]
#if NET
        [SupportedOSPlatform("windows")]
#endif
        internal static CursorHandle ToCursorHandle(IntPtr iconHandle, Point cursorHotspot)
        {
            Debug.Assert(OSUtils.IsWindows);
            User32.GetIconInfo(iconHandle, out ICONINFO iconInfo);
            iconInfo.xHotspot = cursorHotspot.X;
            iconInfo.yHotspot = cursorHotspot.Y;
            iconInfo.fIcon = false;
            return new CursorHandle(User32.CreateIconIndirect(ref iconInfo));
        }

        [SecuritySafeCritical]
        internal static Icon FromImage(Image image, int size, bool keepAspectRatio, ScalingMode scalingMode)
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image), PublicResources.ArgumentNull);

            var resultSize = new Size(size, size);
            Size targetSize = resultSize;

            // Same size and image is Bitmap
            if (image is Bitmap bitmap && bitmap.Size == targetSize)
                return FromBitmap(bitmap);

            // Different size or image is not a Bitmap
            Size sourceSize = image.Size;
            var targetLocation = Point.Empty;

            if (keepAspectRatio && targetSize != sourceSize)
            {
                float ratio = Math.Min((float)targetSize.Width / sourceSize.Width, (float)targetSize.Height / sourceSize.Height);
                targetSize = new Size((int)(sourceSize.Width * ratio), (int)(sourceSize.Height * ratio));
                targetLocation = new Point((resultSize.Width >> 1) - (targetSize.Width >> 1), (resultSize.Height >> 1) - (targetSize.Height >> 1));
            }

            using var result = new Bitmap(resultSize.Width, resultSize.Height);
            image.DrawInto(result, new Rectangle(targetLocation, targetSize), scalingMode);
            return FromBitmap(result);
        }

        [SecurityCritical] // GetHicon
        internal static Icon FromBitmap(Bitmap bmp)
        {
            if (OSUtils.IsWindows)
                return Icon.FromHandle(bmp.GetHicon()).ToManagedIcon();

            using var rawIcon = new RawIcon();
            rawIcon.Add(bmp);
            return rawIcon.ToIcon(true)!; // forcing uncompressed on non-Windows platforms
        }

        #endregion

        #region Private Methods

        [return:NotNullIfNotNull(nameof(getLegacyIcon))]private static Icon? GetSystemIcon(int id, Func<Icon>? getLegacyIcon)
            => SystemIconsCache.GetOrAdd(id, getSystemIconAddValueFactory, getLegacyIcon)?.ToIcon(false);

        [SecuritySafeCritical]
        private static RawIcon? GetStockIconOrDefault(int id, Func<Icon>? getLegacyIcon)
        {
            RawIcon? result = DoGetStockIcon(id);
            if (result == null && getLegacyIcon != null)
                result = ToCombinedIcon(getLegacyIcon.Invoke());
            return result;
        }

        /// <summary>
        /// By stock icon we mean Vista+ stock icons from imageres.dll, explicit imageres.dll icons with resource id, or user32.dll icons.
        /// </summary>
        [SecurityCritical]
        private static RawIcon? DoGetStockIcon(int id)
        {
            if (!OSUtils.IsWindows)
                return null;

            // we have a stock icon id and running on Vista or later
            bool isVistaOrLater = OSUtils.IsVistaOrLater;
            if (isVistaOrLater && ((id & stockIconIdMask) != 0 || id == 0))
            {
                // Vista or later, we have a valid stock icon id
                Shell32.GetStockIconPath((StockIcon)(id & stockIconIdMask), out string path, out int index);
                if (!String.IsNullOrEmpty(path))
                {
                    RawIcon? result = DoLoadIconFromFile(path, Math.Abs(index), false);
                    if (result != null)
                        return result;
                }

            }

            // no alternative id is specified: exiting
            if ((id & alternativeIconIdMask) == 0)
                return null;

            // the alternative id is from user32.dll
            if ((id & isImageResIconFlag) == 0)
                return DoLoadIconFromFile("user32", id >> alternativeIdShift, false);

            // the alternative id is from imageres.dll
            if (isVistaOrLater)
                return DoLoadIconFromFile("imageres", (id & ~isImageResIconFlag) >> alternativeIdShift, false);

            return null;
        }

        /// <summary>
        /// Gets a multi size version of a system icon provided in <paramref name="icon"/> by generating the small version internally.
        /// </summary>
        private static RawIcon ToCombinedIcon(Icon icon)
        {
            var result = new RawIcon(icon);
            if (result.ImageCount == 1)
            {
                using Bitmap imageLarge = result.ExtractBitmap(0, false)!;
                using Bitmap imageSmall = imageLarge.Resize(new Size(16, 16), true);
                result.Add(imageSmall);
            }

            return result;
        }

        private static Icon GetResourceIcon(string resourceName)
            => ResourceIconsCache.GetOrAdd(resourceName, getResourceIconAddValueFactory).ToIcon(OSUtils.IsXpOrEarlier)!;

        private static RawIcon DoGetResourceIcon(string resourceName)
            => new RawIcon(ResourceManager.GetStream(resourceName, CultureInfo.InvariantCulture)!);

        [SecurityCritical]
        private static RawIcon? DoLoadIconFromFile(string fileName, int id, bool throwError)
        {
            IntPtr hModule = Kernel32.LoadLibraryData(fileName, throwError);
            if (hModule == IntPtr.Zero)
                return null;
            try
            {
                return GetModuleIcon(hModule, Kernel32.MAKEINTRESOURCE(id));
            }
            finally
            {
                Kernel32.FreeLibrary(hModule);
            }
        }

        [SecurityCritical]
        private static unsafe RawIcon GetModuleIcon(IntPtr hModule, IntPtr name)
        {
            byte[] groupIconRawData = Kernel32.ExtractResourceData(hModule, name, Constants.RT_GROUP_ICON);

            var iconDir = BinarySerializer.DeserializeValueType<GRPICONDIR>(groupIconRawData);
            int iconImagesCount = iconDir.idCount;

            // Calculating the raw .ico length. Note that we use ICONDIRENTRY here because we want to create an actual icon.
            int length = sizeof(ICONDIR) + sizeof(ICONDIRENTRY) * iconImagesCount;
            for (int i = 0; i < iconImagesCount; i++)
            {
                var entry = BinarySerializer.DeserializeValueType<GRPICONDIRENTRY>(groupIconRawData, sizeof(GRPICONDIR) + sizeof(GRPICONDIRENTRY) * i);
                length += (int)entry.dwBytesInRes;
            }

            var result = new byte[length];

            // As ICONDIR and GRPICONDIR are identical without the entries, we can copy the "header" directly.
            Array.Copy(groupIconRawData, result, sizeof(ICONDIR));

            int offset = sizeof(ICONDIR) + sizeof(ICONDIRENTRY) * iconImagesCount;
            for (int i = 0; i < iconImagesCount; i++)
            {
                // extracting the image data
                GRPICONDIRENTRY groupEntry = BinarySerializer.DeserializeValueType<GRPICONDIRENTRY>(groupIconRawData, sizeof(GRPICONDIR) + sizeof(GRPICONDIRENTRY) * i);
                byte[] iconImageRawData = Kernel32.ExtractResourceData(hModule, Kernel32.MAKEINTRESOURCE(groupEntry.nID), Constants.RT_ICON);

                // writing the new entry
                var iconEntry = new ICONDIRENTRY
                {
                    bWidth = groupEntry.bWidth,
                    bHeight = groupEntry.bHeight,
                    bColorCount = groupEntry.bColorCount,
                    bReserved = groupEntry.bReserved,
                    wPlanes = groupEntry.wPlanes,
                    wBitCount = groupEntry.wBitCount,
                    dwBytesInRes = (uint)iconImageRawData.Length,
                    dwImageOffset = (uint)offset
                };

                Array.Copy(BinarySerializer.SerializeValueType(iconEntry), 0, result, sizeof(ICONDIR) + sizeof(ICONDIRENTRY) * i, sizeof(ICONDIRENTRY));
                Array.Copy(iconImageRawData, 0, result, offset, iconImageRawData.Length);
                offset += iconImageRawData.Length;
            }

            return new RawIcon(new MemoryStream(result));
        }

        #endregion

        #endregion
    }
}
