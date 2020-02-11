﻿#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: Icons.cs
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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Resources;
using System.Security;
using KGySoft.CoreLibraries;
using KGySoft.Drawing.WinApi;

#endregion

namespace KGySoft.Drawing
{
    /// <summary>
    /// Provides some icon-related methods as well as properties returning general icons in multi resolution. Unlike <see cref="SystemIcons"/>, these icons should be disposed when not used any more.
    /// </summary>
    public static class Icons
    {
        #region Fields

        private static ResourceManager resourceManager;
        private static readonly Dictionary<StockIcon, RawIcon> systemIconsCache = new Dictionary<StockIcon, RawIcon>(EnumComparer<StockIcon>.Comparer);
        private static readonly Dictionary<string, RawIcon> resourceIconsCache = new Dictionary<string, RawIcon>();

        #endregion

        #region Properties

        #region Public Properties

        /// <summary>
        /// <img src="../Help/Images/Information16W10.png" alt="Information (small version for the summary)"/>
        /// Gets an <see cref="Icon"/> instance that contains a large and a small
        /// Information icon as it is displayed by the current operating system.
        /// <br/>In Windows Vista and above sizes are depending on current DPI settings, in Windows XP the icon has always 32x32 and 16x16 image sizes.
        /// </summary>
        /// <remarks>
        /// <para>
        /// In Windows 8 and Windows 10 at 100% DPI settings the icon contains the following images:<br/>
        /// <img src="../Help/Images/Information32W10.png" alt="Information 32x32"/>
        /// <img src="../Help/Images/Information16W10.png" alt="Information 16x16"/>
        /// </para>
        /// <para>
        /// In Windows Vista and Windows 7 at 100% DPI settings the icon contains the following images:<br/>
        /// <img src="../Help/Images/Information32.png" alt="Information 32x32"/>
        /// <img src="../Help/Images/Information16.png" alt="Information 16x16"/>
        /// </para>
        /// <para>
        /// In Windows XP the icon contains the following images:<br/>
        /// <img src="../Help/Images/InformationXP32.png" alt="Information Windows XP 32x32"/>
        /// <img src="../Help/Images/InformationXP16.png" alt="Information Windows XP 16x16"/>
        /// </para>
        /// </remarks>
        public static Icon SystemInformation => GetSystemIcon(StockIcon.Information, () => SystemIcons.Information);

        /// <summary>
        /// <img src="../Help/Images/Warning16W10.png" alt="Warning (small version for the summary)"/>
        /// Gets an <see cref="Icon"/> instance that contains a large and a small
        /// Warning icon as it is displayed by the current operating system.
        /// <br/>In Windows Vista and above sizes are depending on current DPI settings, in Windows XP the icon has always 32x32 and 16x16 image sizes.
        /// </summary>
        /// <remarks>
        /// <para>
        /// In Windows 8 and Windows 10 at 100% DPI settings the icon contains the following images:<br/>
        /// <img src="../Help/Images/Warning32W10.png" alt="Warning 32x32"/>
        /// <img src="../Help/Images/Warning16W10.png" alt="Warning 16x16"/>
        /// </para>
        /// <para>
        /// In Windows Vista and Windows 7 at 100% DPI settings the icon contains the following images:<br/>
        /// <img src="../Help/Images/Warning32.png" alt="Warning 32x32"/>
        /// <img src="../Help/Images/Warning16.png" alt="Warning 16x16"/>
        /// </para>
        /// <para>
        /// In Windows XP the icon contains the following images:<br/>
        /// <img src="../Help/Images/WarningXP32.png" alt="Warning Windows XP 32x32"/>
        /// <img src="../Help/Images/WarningXP16.png" alt="Warning Windows XP 16x16"/>
        /// </para>
        /// </remarks>
        public static Icon SystemWarning => GetSystemIcon(StockIcon.Warning, () => SystemIcons.Warning);

        /// <summary>
        /// <img src="../Help/Images/Error16W10.png" alt="Error (small version for the summary)"/>
        /// Gets an <see cref="Icon"/> instance that contains a large and a small
        /// Error icon as it is displayed by the current operating system.
        /// <br/>In Windows Vista and above sizes are depending on current DPI settings, in Windows XP the icon has always 32x32 and 16x16 image sizes.
        /// </summary>
        /// <remarks>
        /// <para>
        /// In Windows 8 and Windows 10 at 100% DPI settings the icon contains the following images:<br/>
        /// <img src="../Help/Images/Error32W10.png" alt="Error 32x32"/>
        /// <img src="../Help/Images/Error16W10.png" alt="Error 16x16"/>
        /// </para>
        /// <para>
        /// In Windows Vista and Windows 7 at 100% DPI settings the icon contains the following images:<br/>
        /// <img src="../Help/Images/Error32.png" alt="Error 32x32"/>
        /// <img src="../Help/Images/Error16.png" alt="Error 16x16"/>
        /// </para>
        /// <para>
        /// In Windows XP the icon contains the following images:<br/>
        /// <img src="../Help/Images/ErrorXP32.png" alt="Error Windows XP 32x32"/>
        /// <img src="../Help/Images/ErrorXP16.png" alt="Error Windows XP 16x16"/>
        /// </para>
        /// </remarks>
        public static Icon SystemError => GetSystemIcon(StockIcon.Error, () => SystemIcons.Error);

        /// <summary>
        /// <img src="../Help/Images/Question16W10.png" alt="Question (small version for the summary)"/>
        /// Gets an <see cref="Icon"/> instance that contains a large and a small
        /// Question icon as it is displayed by the current operating system.
        /// <br/>In Windows Vista and above sizes are depending on current DPI settings, in Windows XP the icon has always 32x32 and 16x16 image sizes.
        /// </summary>
        /// <remarks>
        /// <para>
        /// In Windows 8 and Windows 10 at 100% DPI settings the icon contains the following images:<br/>
        /// <img src="../Help/Images/Question32W10.png" alt="Question 32x32"/>
        /// <img src="../Help/Images/Question16W10.png" alt="Question 16x16"/>
        /// </para>
        /// <para>
        /// In Windows Vista and Windows 7 at 100% DPI settings the icon contains the following images:<br/>
        /// <img src="../Help/Images/Question32.png" alt="Question 32x32"/>
        /// <img src="../Help/Images/Question16.png" alt="Question 16x16"/>
        /// </para>
        /// <para>
        /// In Windows XP the icon contains the following images:<br/>
        /// <img src="../Help/Images/QuestionXP32.png" alt="Question Windows XP 32x32"/>
        /// <img src="../Help/Images/QuestionXP16.png" alt="Question Windows XP 16x16"/>
        /// </para>
        /// </remarks>
        public static Icon SystemQuestion => GetSystemIcon(StockIcon.Help, () => SystemIcons.Question);

        /// <summary>
        /// <img src="../Help/Images/Application16W10.png" alt="Application (small version for the summary)"/>
        /// Gets an <see cref="Icon"/> instance that contains a large and a small
        /// Application icon as it is displayed by the current operating system.
        /// <br/>In Windows Vista and above sizes are depending on current DPI settings, in Windows XP the icon has always 32x32 and 16x16 image sizes.
        /// </summary>
        /// <remarks>
        /// <para>
        /// In Windows 8 and Windows 10 at 100% DPI settings the icon contains the following images:<br/>
        /// <img src="../Help/Images/Application32W10.png" alt="Application 32x32"/>
        /// <img src="../Help/Images/Application16W10.png" alt="Application 16x16"/>
        /// </para>
        /// <para>
        /// In Windows Vista and Windows 7 at 100% DPI settings the icon contains the following images:<br/>
        /// <img src="../Help/Images/Application32.png" alt="Application 32x32"/>
        /// <img src="../Help/Images/Application16.png" alt="Application 16x16"/>
        /// </para>
        /// <para>
        /// In Windows XP the icon contains the following images:<br/>
        /// <img src="../Help/Images/ApplicationXP32.png" alt="Application Windows XP 32x32"/>
        /// <img src="../Help/Images/ApplicationXP16.png" alt="Application Windows XP 16x16"/>
        /// </para>
        /// </remarks>
        public static Icon SystemApplication => GetSystemIcon(StockIcon.Application, () => SystemIcons.Application);

        /// <summary>
        /// <img src="../Help/Images/Shield16W10.png" alt="Shield (small version for the summary)"/>
        /// Gets an <see cref="Icon"/> instance that contains the
        /// Shield icon as it is displayed by the current operating system.
        /// <br/>In Windows Vista and above sizes are depending on current DPI settings, in Windows XP the icon has always 48x48, 32x32 and 16x16 image sizes in three different color depths.
        /// </summary>
        /// <remarks>
        /// <para>
        /// In Windows 8 and Windows 10 at 100% DPI settings the icon contains the following images:<br/>
        /// <img src="../Help/Images/Shield32W10.png" alt="Shield Windows 8/10 32x32"/>
        /// <img src="../Help/Images/Shield16W10.png" alt="Shield Windows 8/10 16x16"/>
        /// </para>
        /// <para>
        /// In Windows 7 at 100% DPI settings the icon contains the following images:<br/>
        /// <img src="../Help/Images/SecurityShield32.png" alt="Shield Windows 7 32x32"/>
        /// <img src="../Help/Images/SecurityShield16.png" alt="Shield Windows 7 16x16"/>
        /// </para>
        /// <para>
        /// In Windows Vista at 100% DPI settings the icon contains the following images:<br/>
        /// <img src="../Help/Images/Shield32.png" alt="Shield Windows Vista 32x32"/>
        /// <img src="../Help/Images/Shield16.png" alt="Shield Windows Vista 16x16"/>
        /// </para>
        /// <para>
        /// In Windows XP the icon contains three different color depth version of the following images:<br/>
        /// <img src="../Help/Images/ShieldXP48.png" alt="Shield Windows XP 48x48"/>
        /// <img src="../Help/Images/ShieldXP32.png" alt="Shield Windows XP 32x32"/>
        /// <img src="../Help/Images/ShieldXP16.png" alt="Shield Windows XP 16x16"/>
        /// </para>
        /// </remarks>
        public static Icon SystemShield => GetSystemIcon(StockIcon.Shield, () => SystemIcons.Shield);

        /// <summary>
        /// <img src="../Help/Images/Information16.png" alt="Information (small version for the summary)"/>
        /// Gets the Information icon displaying a white "i" a blue circle (Sizes: 256x256, 64x64, 48x48, 32x32, 24x24, 20x20, 16x16)
        /// </summary>
        /// <remarks>
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
        /// </remarks>
        public static Icon Information => GetResourceIcon(nameof(Information));

        /// <summary>
        /// <img src="../Help/Images/Warning16.png" alt="Warning (small version for the summary)"/>
        /// Gets the Warning icon displaying a black "!" in a yellow triangle (Sizes: 256x256, 64x64, 48x48, 32x32, 24x24, 20x20, 16x16)
        /// </summary>
        /// <remarks>
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
        /// </remarks>
        public static Icon Warning => GetResourceIcon(nameof(Warning));

        /// <summary>
        /// <img src="../Help/Images/Question16.png" alt="Question (small version for the summary)"/>
        /// Gets the Question icon displaying a white "?" in a blue circle (Sizes: 256x256, 64x64, 48x48, 32x32, 24x24, 20x20, 16x16)
        /// </summary>
        /// <remarks>
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
        /// </remarks>
        public static Icon Question => GetResourceIcon(nameof(Question));

        /// <summary>
        /// <img src="../Help/Images/Error16.png" alt="Error (small version for the summary)"/>
        /// Gets the Error icon displaying a white "X" in a red circle (Sizes: 256x256, 64x64, 48x48, 32x32, 24x24, 20x20, 16x16)
        /// </summary>
        /// <remarks>
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
        /// </remarks>
        public static Icon Error => GetResourceIcon(nameof(Error));

        /// <summary>
        /// <img src="../Help/Images/Shield16.png" alt="Shield (small version for the summary)"/>
        /// Gets the Windows Shield icon displaying a red-green-blue-yellow shield (Sizes: 256x256, 128x128, 48x48, 32x32, 24x24, 16x16, 8x8)
        /// </summary>
        /// <remarks>
        /// <para>
        /// The icon contains the following images:<br/>
        /// <img src="../Help/Images/Shield256.png" alt="Security Shield 256x256"/>
        /// <img src="../Help/Images/Shield128.png" alt="Security Shield 128x128"/>
        /// <img src="../Help/Images/Shield48.png" alt="Security Shield 48x48"/>
        /// <img src="../Help/Images/Shield32.png" alt="Security Shield 32x32"/>
        /// <img src="../Help/Images/Shield24.png" alt="Security Shield 24x24"/>
        /// <img src="../Help/Images/Shield16.png" alt="Security Shield 16x16"/>
        /// <img src="../Help/Images/Shield8.png" alt="Security Shield 8x8"/>
        /// </para>
        /// </remarks>
        public static Icon Shield => GetResourceIcon(nameof(Shield));

        /// <summary>
        /// <img src="../Help/Images/SecurityShield16.png" alt="Security Shield (small version for the summary)"/>
        /// Gets the Security Shield icon displaying a blue-yellow shield (Sizes: 256x256, 128x128, 48x48, 32x32, 24x24, 16x16, 8x8)
        /// </summary>
        /// <remarks>
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
        /// </remarks>
        public static Icon SecurityShield => GetResourceIcon(nameof(SecurityShield));

        /// <summary>
        /// <img src="../Help/Images/SecuritySuccess16.png" alt="Security Success (small version for the summary)"/>
        /// Gets the Security Success icon displaying a green shield with a white check (Sizes: 256x256, 48x48, 32x32, 24x24, 16x16)
        /// </summary>
        /// <remarks>
        /// <para>
        /// The icon contains the following images:<br/>
        /// <img src="../Help/Images/SecuritySuccess256.png" alt="Security Success 256x256"/>
        /// <img src="../Help/Images/SecuritySuccess48.png" alt="Security Success 48x48"/>
        /// <img src="../Help/Images/SecuritySuccess32.png" alt="Security Success 32x32"/>
        /// <img src="../Help/Images/SecuritySuccess24.png" alt="Security Success 24x24"/>
        /// <img src="../Help/Images/SecuritySuccess16.png" alt="Security Success 16x16"/>
        /// </para>
        /// </remarks>
        public static Icon SecuritySuccess => GetResourceIcon(nameof(SecuritySuccess));

        /// <summary>
        /// <img src="../Help/Images/SecurityWarning16.png" alt="Security Warning (small version for the summary)"/>
        /// Gets the Security Warning icon displaying a yellow shield with a black "!" (Sizes: 256x256, 48x48, 32x32, 24x24, 16x16)
        /// </summary>
        /// <remarks>
        /// <para>
        /// The icon contains the following images:<br/>
        /// <img src="../Help/Images/SecurityWarning256.png" alt="Security Warning 256x256"/>
        /// <img src="../Help/Images/SecurityWarning48.png" alt="Security Warning 48x48"/>
        /// <img src="../Help/Images/SecurityWarning32.png" alt="Security Warning 32x32"/>
        /// <img src="../Help/Images/SecurityWarning24.png" alt="Security Warning 24x24"/>
        /// <img src="../Help/Images/SecurityWarning16.png" alt="Security Warning 16x16"/>
        /// </para>
        /// </remarks>
        public static Icon SecurityWarning => GetResourceIcon(nameof(SecurityWarning));

        /// <summary>
        /// <img src="../Help/Images/SecurityQuestion16.png" alt="Security Question (small version for the summary)"/>
        /// Gets the Security Question icon displaying a blue shield with a white "?" (Sizes: 256x256, 48x48, 32x32, 24x24, 16x16)
        /// </summary>
        /// <remarks>
        /// <para>
        /// The icon contains the following images:<br/>
        /// <img src="../Help/Images/SecurityQuestion256.png" alt="Security Question 256x256"/>
        /// <img src="../Help/Images/SecurityQuestion48.png" alt="Security Question 48x48"/>
        /// <img src="../Help/Images/SecurityQuestion32.png" alt="Security Question 32x32"/>
        /// <img src="../Help/Images/SecurityQuestion24.png" alt="Security Question 24x24"/>
        /// <img src="../Help/Images/SecurityQuestion16.png" alt="Security Question 16x16"/>
        /// </para>
        /// </remarks>
        public static Icon SecurityQuestion => GetResourceIcon(nameof(SecurityQuestion));

        /// <summary>
        /// <img src="../Help/Images/SecurityError16.png" alt="Security Error (small version for the summary)"/>
        /// Gets the Security Error icon displaying a red shield with a white "X" (Sizes: 256x256, 48x48, 32x32, 24x24, 16x16)
        /// </summary>
        /// <remarks>
        /// <para>
        /// The icon contains the following images:<br/>
        /// <img src="../Help/Images/SecurityError256.png" alt="Security Error 256x256"/>
        /// <img src="../Help/Images/SecurityError48.png" alt="Security Error 48x48"/>
        /// <img src="../Help/Images/SecurityError32.png" alt="Security Error 32x32"/>
        /// <img src="../Help/Images/SecurityError24.png" alt="Security Error 24x24"/>
        /// <img src="../Help/Images/SecurityError16.png" alt="Security Error 16x16"/>
        /// </para>
        /// </remarks>
        public static Icon SecurityError => GetResourceIcon(nameof(SecurityError));

        /// <summary>
        /// <img src="../Help/Images/Application16.png" alt="Application (small version for the summary)"/>
        /// Gets the Application icon displaying a window (Sizes: 256x256, 64x64, 48x48, 32x32, 24x24, 16x16)
        /// </summary>
        /// <remarks>
        /// <para>
        /// The icon contains the following images:<br/>
        /// <img src="../Help/Images/Application256.png" alt="Application 256x256"/>
        /// <img src="../Help/Images/Application64.png" alt="Application 64x64"/>
        /// <img src="../Help/Images/Application48.png" alt="Application 48x48"/>
        /// <img src="../Help/Images/Application32.png" alt="Application 32x32"/>
        /// <img src="../Help/Images/Application24.png" alt="Application 24x24"/>
        /// <img src="../Help/Images/Application16.png" alt="Application 16x16"/>
        /// </para>
        /// </remarks>
        public static Icon Application => GetResourceIcon(nameof(Application));

        #endregion

        #region Private Properties

        private static ResourceManager ResourceManager => resourceManager ??= new ResourceManager(typeof(Icons));

        #endregion

        #endregion

        #region Methods

        #region Public Methods

        /// <summary>
        /// Tries to get a system stock icon. When there is no icon defined for provided <paramref name="id"/>,
        /// or Windows version is below Vista, this method returns <see langword="null"/>.
        /// In Windows XP use the predefined property members to retrieve system icons.
        /// </summary>
        /// <param name="id">Id of the icon to retrieve. For future compatibility reasons non-defined <see cref="StockIcon"/> values are also allowed.</param>
        /// <returns>An <see cref="Icon"/> instance containing a small and large icon when an icon belongs to <paramref name="id"/>, or <see langword="null"/>,
        /// when no icon found or Windows version is below Vista.</returns>
        /// <remarks>
        /// <note>On non-Windows platforms this method always returns <see langword="null"/>.</note>
        /// </remarks>
        public static Icon GetStockIcon(StockIcon id) => GetSystemIcon(id, null);

        /// <summary>
        /// Extracts icons of the specified <paramref name="size"/> from a file and returns them as separated <see cref="Icon"/> instances.
        /// </summary>
        /// <param name="fileName">The name of the file. Can be an executable file, a .dll or icon file.</param>
        /// <param name="size">The size of the icons to be extracted.</param>
        /// <returns>The icons of the specified file, or an empty array if the file does not exist or does not contain any icons.</returns>
        /// <remarks>
        /// <para>If <paramref name="fileName"/> refers to an icon file use the <see cref="Icon(string)"/> constructor instead.</para>
        /// <para>The images of an <see cref="Icon"/> can be extracted by the <see cref="O:KGySoft.Drawing.IconExtensions.ExtractBitmaps">IconExtensions.ExtractBitmaps</see> methods.</para>
        /// <note>On non-Windows platforms this method always returns an empty array.</note>
        /// </remarks>
#if !NET35
        [SecuritySafeCritical]
#endif
        public static Icon[] FromFile(string fileName, SystemIconSize size)
        {
            if (fileName == null)
                throw new ArgumentNullException(nameof(fileName), PublicResources.ArgumentNull);
            if (!Enum<SystemIconSize>.IsDefined(size))
                throw new ArgumentOutOfRangeException(nameof(size), PublicResources.EnumOutOfRangeWithValues(size));
            if (!OSUtils.IsWindows)
#if NETFRAMEWORK
                return new Icon[0];
#else
                return Array.Empty<Icon>();
#endif

            IntPtr[][] handles = Shell32.ExtractIconHandles(fileName, size);
            Icon[] result = new Icon[handles.Length];

            for (int i = 0; i < handles.Length; i++)
                result[i] = Icon.FromHandle(handles[i][0]).ToManagedIcon();

            return result;
        }

        /// <summary>
        /// Extracts dual-resolution icons from a file and returns them as separated <see cref="Icon"/> instances.
        /// </summary>
        /// <param name="fileName">The name of the file. Can be an executable file, a .dll or icon file.</param>
        /// <returns>The icons of the specified file, or an empty array if the file does not exist or does not contain any icons.</returns>
        /// <remarks>
        /// <para>If <paramref name="fileName"/> refers to an icon file use the <see cref="Icon(string)"/> constructor instead.</para>
        /// <para>The images of an <see cref="Icon"/> can be extracted by the <see cref="O:KGySoft.Drawing.IconExtensions.ExtractBitmaps">IconExtensions.ExtractBitmaps</see> methods.</para>
        /// <note>On non-Windows platforms this method always returns an empty array.</note>
        /// </remarks>
#if !NET35
        [SecuritySafeCritical]
#endif
        public static Icon[] FromFile(string fileName)
        {
            if (fileName == null)
                throw new ArgumentNullException(nameof(fileName), PublicResources.ArgumentNull);
            if (!OSUtils.IsWindows)
#if NETFRAMEWORK
                return new Icon[0];
#else
                return Array.Empty<Icon>();
#endif

            IntPtr[][] handles = Shell32.ExtractIconHandles(fileName, null);
            Icon[] result = new Icon[handles.Length];

            for (int i = 0; i < handles.Length; i++)
            {
                result[i] = Combine(handles[i].Select(Icon.FromHandle).ToArray());
                foreach (IntPtr handle in handles[i])
                    User32.DestroyIcon(handle);
            }

            return result;
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
#if !NET35
        [SecuritySafeCritical]
#endif
        public static Icon FromExtension(string fileOrExtension, SystemIconSize size)
        {
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
        /// <returns>The <see cref="Icon"/> loaded from the <paramref name="stream"/>.</returns>
        /// <remarks>
        /// <para>The result <see cref="Icon"/> is compatible with Windows XP if the method is executed in a Windows XP environment.</para>
        /// </remarks>
        public static Icon FromStream(Stream stream) => FromStream(stream, OSUtils.IsXpOrEarlier);

        /// <summary>
        /// Loads an <see cref="Icon"/> from the specified <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">The stream to load the icon from.</param>
        /// <param name="forceUncompressedResult"><see langword="true"/>&#160;to force returning an uncompressed icon;
        /// <see langword="false"/>&#160;to allow PNG compression, which is supported by Windows Vista and above.</param>
        /// <returns>The <see cref="Icon"/> loaded from the <paramref name="stream"/>.</returns>
#if !NET35
        [SecuritySafeCritical]
#endif
        public static Icon FromStream(Stream stream, bool forceUncompressedResult)
        {
            using (var rawIcon = new RawIcon(stream))
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
        public static Icon Combine(params Icon[] icons) => Combine(OSUtils.IsXpOrEarlier, icons);

        /// <summary>
        /// Combines the provided <paramref name="icons"/> into a multi-resolution <see cref="Icon"/> instance.
        /// </summary>
        /// <param name="forceUncompressedResult"><see langword="true"/>&#160;to force returning an uncompressed icon;
        /// <see langword="false"/>&#160;to allow PNG compression, which is supported by Windows Vista and above.</param>
        /// <param name="icons">The icons to be combined.</param>
        /// <returns>An <see cref="Icon"/> instance that contains every image of the source <paramref name="icons"/>.</returns>
        /// <remarks>The elements of <paramref name="icons"/> may contain multiple icons.</remarks>
#if !NET35
        [SecuritySafeCritical]
#endif
        public static Icon Combine(bool forceUncompressedResult, params Icon[] icons)
        {
            if (icons == null || icons.Length == 0)
                return null;

            using (RawIcon rawIcon = new RawIcon())
            {
                foreach (Icon icon in icons)
                    rawIcon.Add(icon);

                return rawIcon.ToIcon(forceUncompressedResult);
            }
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
        public static Icon Combine(params Bitmap[] images) => Combine(OSUtils.IsXpOrEarlier, images);

        /// <summary>
        /// Combines the provided <paramref name="images"/> into a multi-resolution <see cref="Icon"/> instance.
        /// </summary>
        /// <param name="forceUncompressedResult"><see langword="true"/>&#160;to force returning an uncompressed icon;
        /// <see langword="false"/>&#160;to allow PNG compression, which is supported by Windows Vista and above.</param>
        /// <param name="images">The images to be added to the result icon. Images can be non-squared ones.
        /// Transparency is determined automatically by image format.</param>
        /// <returns>An <see cref="Icon"/> instance that contains every image of the source <paramref name="images"/>.</returns>
        /// <remarks>
        /// <para>The elements of <paramref name="images"/> may contain multiple icons.</para>
        /// </remarks>
#if !NET35
        [SecuritySafeCritical]
#endif
        public static Icon Combine(bool forceUncompressedResult, params Bitmap[] images)
        {
            if (images == null || images.Length == 0)
                return null;

            using (RawIcon rawIcon = new RawIcon())
            {
                foreach (Bitmap image in images)
                    rawIcon.Add(image);

                return rawIcon.ToIcon(forceUncompressedResult);
            }
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
        public static Icon Combine(Bitmap[] images, Color[] transparentColors) => Combine(images, transparentColors, OSUtils.IsXpOrEarlier);

        /// <summary>
        /// Combines the provided <paramref name="images"/> into a multi-resolution <see cref="Icon"/> instance.
        /// </summary>
        /// <param name="images">The images to be added to the icon. Images can be non-squares ones.</param>
        /// <param name="transparentColors">An array of transparent colors of the images. The array must have as many elements as <paramref name="images"/>.</param>
        /// <param name="forceUncompressedResult"><see langword="true"/>&#160;to force returning an uncompressed icon;
        /// <see langword="false"/>&#160;to allow PNG compression, which is supported by Windows Vista and above.</param>
        /// <returns>
        /// An <see cref="Icon"/> instance that contains every image of the source <paramref name="images"/>.
        /// </returns>
        /// <remarks>
        /// <para>The elements of <paramref name="images"/> may contain multiple icons.</para>
        /// </remarks>
#if !NET35
        [SecuritySafeCritical]
#endif
        public static Icon Combine(Bitmap[] images, Color[] transparentColors, bool forceUncompressedResult)
        {
            int imageCount = images?.Length ?? 0;
            int colorCount = transparentColors?.Length ?? 0;
            if (imageCount != colorCount)
                throw new ArgumentException(Res.IconExtensionsImagesColorsDifferentLength);

            if (images == null || transparentColors == null || imageCount == 0)
                return null;

            using (RawIcon rawIcon = new RawIcon())
            {
                for (int i = 0; i < imageCount; i++)
                    rawIcon.Add(images[i], transparentColors[i]);

                return rawIcon.ToIcon(forceUncompressedResult);
            }
        }

        #endregion

        #region Internal Methods

        [SecurityCritical]
        internal static CursorHandle ToCursorHandle(IntPtr iconHandle, Point cursorHotspot)
        {
            if (!OSUtils.IsWindows)
                throw new PlatformNotSupportedException(Res.RequiresWindows);
            User32.GetIconInfo(iconHandle, out ICONINFO iconInfo);
            iconInfo.xHotspot = cursorHotspot.X;
            iconInfo.yHotspot = cursorHotspot.Y;
            iconInfo.fIcon = false;
            return new CursorHandle(User32.CreateIconIndirect(ref iconInfo));
        }

        [SecurityCritical] // GetHicon
        internal static Icon FromImage(Image image, Size targetSize, bool keepAspectRatio)
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image), PublicResources.ArgumentNull);

            // Same size and image is Bitmap
            if (image is Bitmap bitmap && bitmap.Size == targetSize)
                return Icon.FromHandle(bitmap.GetHicon()).ToManagedIcon();

            // Different size or image is not a Bitmap
            Size sourceSize = image.Size;
            Point targetLocation = Point.Empty;

            if (keepAspectRatio && targetSize != sourceSize)
            {
                float ratio = Math.Min((float)targetSize.Width / sourceSize.Width, (float)targetSize.Height / sourceSize.Height);
                targetSize = new Size((int)(sourceSize.Width * ratio), (int)(sourceSize.Height * ratio));
                targetLocation = new Point(targetSize.Width / 2 - targetSize.Width / 2, targetSize.Height / 2 - targetSize.Height / 2);
            }

            using (bitmap = new Bitmap(targetSize.Width, targetSize.Height))
            {
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    g.DrawImage(image, new Rectangle(targetLocation, targetSize), new Rectangle(Point.Empty, sourceSize), GraphicsUnit.Pixel);
                    g.Flush();

                    return Icon.FromHandle(bitmap.GetHicon()).ToManagedIcon();
                }
            }
        }

        #endregion

        #region Private Methods

#if !NET35
        [SecuritySafeCritical]
#endif
        private static Icon GetSystemIcon(StockIcon id, Func<Icon> getLegacyIcon)
        {
            RawIcon result;
            lock (systemIconsCache)
            {
                if (!systemIconsCache.TryGetValue(id, out result))
                {
                    result = DoGetStockIcon(id);
                    if (result == null && getLegacyIcon != null)
                        result = ToCombinedIcon(getLegacyIcon.Invoke());
                    systemIconsCache[id] = result;
                }
            }

            return result?.ToIcon(false);
        }

#if !NET35
        [SecuritySafeCritical]
#endif
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "The result must be disposed by the caller.")]
        private static Icon GetResourceIcon(string resourceName)
        {
            RawIcon result;
            lock (resourceIconsCache)
            {
                if (!resourceIconsCache.TryGetValue(resourceName, out result))
                {
                    result = new RawIcon((Icon)ResourceManager.GetObject(resourceName, CultureInfo.InvariantCulture));
                    resourceIconsCache[resourceName] = result;
                }

            }

            return result.ToIcon(OSUtils.IsXpOrEarlier);
        }

        [SecurityCritical]
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "The result must not be disposed.")]
        private static RawIcon DoGetStockIcon(StockIcon id)
        {
            if (id < 0 || !OSUtils.IsVistaOrLater)
                return null;

            IntPtr largeHandle = Shell32.GetStockIconHandle(id, SystemIconSize.Large);
            if (largeHandle == IntPtr.Zero)
                return null;

            var result = new RawIcon(Icon.FromHandle(largeHandle));
            User32.DestroyIcon(largeHandle);

            IntPtr smallHandle = Shell32.GetStockIconHandle(id, SystemIconSize.Small);
            if (largeHandle == IntPtr.Zero)
                return result;

            result.Add(Icon.FromHandle(smallHandle));
            User32.DestroyIcon(smallHandle);
            return result;
        }

        /// <summary>
        /// Gets a multi size version of a system icon provided in <paramref name="icon"/> by generating the small version internally.
        /// </summary>
        [SecurityCritical]
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "The result must not be disposed.")]
        private static RawIcon ToCombinedIcon(Icon icon)
        {
            RawIcon result = new RawIcon(icon);
            if (result.ImageCount == 1)
            {
                using (Bitmap imageLarge = result.ExtractBitmap(0, false))
                using (Bitmap imageSmall = imageLarge.Resize(new Size(16, 16), true))
                    result.Add(imageSmall);
            }

            return result;
        }

        #endregion

        #endregion
    }
}
