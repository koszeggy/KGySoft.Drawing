#region Copyright

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
using System.Drawing;
using System.Globalization;
using System.Resources;
using System.Runtime.InteropServices;

using KGySoft.CoreLibraries;
using KGySoft.Drawing.WinApi;

#endregion

namespace KGySoft.Drawing
{
    /// <summary>
    /// Provides general icons in multi resolution. Unlike <see cref="SystemIcons"/>, these icons should be disposed when not used any more.
    /// </summary>
    public static class Icons
    {
        #region Enumerations

        private enum IconId
        {
            Application = 2,
            Question = 23,
            Shield = 77,
            Warning = 78,
            Information = 79,
            Error = 80,
            SecurityQuestion = -1,
            SecurityWarning = -2,
            SecurityError = -3,
            SecuritySuccess = -4,
            SecurityShield = -5,
        }

        #endregion

        #region Fields

        private static ResourceManager resourceManager;
        private static readonly Dictionary<IconId, RawIcon> systemIconsCache = new Dictionary<IconId, RawIcon>(EnumComparer<IconId>.Comparer);
        private static readonly Dictionary<IconId, RawIcon> resourceIconsCache = new Dictionary<IconId, RawIcon>(EnumComparer<IconId>.Comparer);

        #endregion

        #region Properties

        #region Public Properties

        /// <summary>
        /// <img src="../Help/Images/Information16.png" alt="Information (small version for the summary)"/>
        /// Gets an <see cref="Icon"/> instance that contains a large and a small
        /// Information icon as it is stored in the current operating system.
        /// On Windows Vista and above sizes are depending on current DPI settings, on Windows XP they have always 32x32 and 16x16 sizes.
        /// </summary>
        /// <remarks>
        /// <para>
        /// On Windows Vista and Windows 7, with default DPI settings the icon contains the following images:<br/>
        /// <img src="../Help/Images/Information32.png" alt="Information 32x32"/>
        /// <img src="../Help/Images/Information16.png" alt="Information 16x16"/>
        /// </para>
        /// <para>
        /// On Windows XP the icon contains the following images:<br/>
        /// <img src="../Help/Images/InformationXP32.png" alt="Information Windows XP 32x32"/>
        /// <img src="../Help/Images/InformationXP16.png" alt="Information Windows XP 16x16"/>
        /// </para>
        /// </remarks>
        public static Icon SystemInformation => GetSystemIcon(IconId.Information, () => SystemIcons.Information);

        /// <summary>
        /// <img src="../Help/Images/Warning16.png" alt="Warning (small version for the summary)"/>
        /// Gets an <see cref="Icon"/> instance that contains a large and a small
        /// Warning icon as it is stored in the current operating system.
        /// On Windows Vista and above sizes are depending on current DPI settings, on Windows XP they have always 32x32 and 16x16 sizes.
        /// </summary>
        /// <remarks>
        /// <para>
        /// On Windows Vista and Windows 7, with default DPI settings the icon contains the following images:<br/>
        /// <img src="../Help/Images/Warning32.png" alt="Warning 32x32"/>
        /// <img src="../Help/Images/Warning16.png" alt="Warning 16x16"/>
        /// </para>
        /// <para>
        /// On Windows XP the icon contains the following images:<br/>
        /// <img src="../Help/Images/WarningXP32.png" alt="Warning Windows XP 32x32"/>
        /// <img src="../Help/Images/WarningXP16.png" alt="Warning Windows XP 16x16"/>
        /// </para>
        /// </remarks>
        public static Icon SystemWarning => GetSystemIcon(IconId.Warning, () => SystemIcons.Warning);

        /// <summary>
        /// <img src="../Help/Images/Error16.png" alt="Error (small version for the summary)"/>
        /// Gets an <see cref="Icon"/> instance that contains a large and a small
        /// Error icon as it is stored in the current operating system.
        /// On Windows Vista and above sizes are depending on current DPI settings, on Windows XP they have always 32x32 and 16x16 sizes.
        /// </summary>
        /// <remarks>
        /// <para>
        /// On Windows Vista and Windows 7, with default DPI settings the icon contains the following images:<br/>
        /// <img src="../Help/Images/Error32.png" alt="Error 32x32"/>
        /// <img src="../Help/Images/Error16.png" alt="Error 16x16"/>
        /// </para>
        /// <para>
        /// On Windows XP the icon contains the following images:<br/>
        /// <img src="../Help/Images/ErrorXP32.png" alt="Error Windows XP 32x32"/>
        /// <img src="../Help/Images/ErrorXP16.png" alt="Error Windows XP 16x16"/>
        /// </para>
        /// </remarks>
        public static Icon SystemError => GetSystemIcon(IconId.Error, () => SystemIcons.Error);

        /// <summary>
        /// <img src="../Help/Images/Question16.png" alt="Question (small version for the summary)"/>
        /// Gets an <see cref="Icon"/> instance that contains a large and a small
        /// Question icon as it is stored in the current operating system.
        /// On Windows Vista and above sizes are depending on current DPI settings, on Windows XP they have always 32x32 and 16x16 sizes.
        /// </summary>
        /// <remarks>
        /// <para>
        /// On Windows Vista and Windows 7, with default DPI settings the icon contains the following images:<br/>
        /// <img src="../Help/Images/Question32.png" alt="Question 32x32"/>
        /// <img src="../Help/Images/Question16.png" alt="Question 16x16"/>
        /// </para>
        /// <para>
        /// On Windows XP the icon contains the following images:<br/>
        /// <img src="../Help/Images/QuestionXP32.png" alt="Question Windows XP 32x32"/>
        /// <img src="../Help/Images/QuestionXP16.png" alt="Question Windows XP 16x16"/>
        /// </para>
        /// </remarks>
        public static Icon SystemQuestion => GetSystemIcon(IconId.Question, () => SystemIcons.Question);

        /// <summary>
        /// <img src="../Help/Images/Application16.png" alt="Application (small version for the summary)"/>
        /// Gets an <see cref="Icon"/> instance that contains a large and a small
        /// Application icon as it is stored in the current operating system.
        /// On Windows Vista and above sizes are depending on current DPI settings, on Windows XP they have always 32x32 and 16x16 sizes.
        /// </summary>
        /// <remarks>
        /// <para>
        /// On Windows Vista and Windows 7, with default DPI settings the icon contains the following images:<br/>
        /// <img src="../Help/Images/Application32.png" alt="Application 32x32"/>
        /// <img src="../Help/Images/Application16.png" alt="Application 16x16"/>
        /// </para>
        /// <para>
        /// On Windows XP the icon contains the following images:<br/>
        /// <img src="../Help/Images/ApplicationXP32.png" alt="Application Windows XP 32x32"/>
        /// <img src="../Help/Images/ApplicationXP16.png" alt="Application Windows XP 16x16"/>
        /// </para>
        /// </remarks>
        public static Icon SystemApplication => GetSystemIcon(IconId.Application, () => SystemIcons.Application);

        /// <summary>
        /// <img src="../Help/Images/SecurityShield16.png" alt="Shield (small version for the summary)"/>
        /// Gets an <see cref="Icon"/> instance that contains the
        /// Shield icon as it is stored in the current operating system.
        /// On Windows Vista and above icon contains two sizes, which are depending on current DPI settings,
        /// on Windows XP the icon contains multiple resolution and color depths.
        /// </summary>
        /// <remarks>
        /// <para>
        /// On Windows 7, with default DPI settings the icon contains the following images:<br/>
        /// <img src="../Help/Images/SecurityShield32.png" alt="Shield Windows 7 32x32"/>
        /// <img src="../Help/Images/SecurityShield16.png" alt="Shield Windows 7 16x16"/>
        /// </para>
        /// <para>
        /// On Windows Vista, with default DPI settings the icon contains the following images:<br/>
        /// <img src="../Help/Images/Shield32.png" alt="Shield Windows Vista 32x32"/>
        /// <img src="../Help/Images/Shield16.png" alt="Shield Windows Vista 16x16"/>
        /// </para>
        /// <para>
        /// On Windows XP the icon contains different color depth version of the following images:<br/>
        /// <img src="../Help/Images/ShieldXP48.png" alt="Shield Windows XP 48x48"/>
        /// <img src="../Help/Images/ShieldXP32.png" alt="Shield Windows XP 32x32"/>
        /// <img src="../Help/Images/ShieldXP16.png" alt="Shield Windows XP 16x16"/>
        /// </para>
        /// </remarks>
        public static Icon SystemShield => GetSystemIcon(IconId.Shield, () => SystemIcons.Shield);

        /// <summary>
        /// <img src="../Help/Images/Information16.png" alt="Information (small version for the summary)"/>
        /// Gets the Information icon displaying a white "i" a blue circle (Sizes: 256x256, 48x48, 32x32, 16x16)
        /// </summary>
        /// <remarks>
        /// <para>
        /// The icon contains the following images:<br/>
        /// <img src="../Help/Images/Information256.png" alt="Information 256x256"/>
        /// <img src="../Help/Images/Information48.png" alt="Information 48x48"/>
        /// <img src="../Help/Images/Information32.png" alt="Information 32x32"/>
        /// <img src="../Help/Images/Information16.png" alt="Information 16x16"/>
        /// </para>
        /// </remarks>
        public static Icon Information => GetResourceIcon(IconId.Information).ToIcon();

        /// <summary>
        /// <img src="../Help/Images/Warning16.png" alt="Warning (small version for the summary)"/>
        /// Gets the Warning icon displaying a black "!" in a yellow triangle (Sizes: 256x256, 48x48, 32x32, 16x16)
        /// </summary>
        /// <remarks>
        /// <para>
        /// The icon contains the following images:<br/>
        /// <img src="../Help/Images/Warning256.png" alt="Warning 256x256"/>
        /// <img src="../Help/Images/Warning48.png" alt="Warning 48x48"/>
        /// <img src="../Help/Images/Warning32.png" alt="Warning 32x32"/>
        /// <img src="../Help/Images/Warning16.png" alt="Warning 16x16"/>
        /// </para>
        /// </remarks>
        public static Icon Warning => GetResourceIcon(IconId.Warning).ToIcon();

        /// <summary>
        /// <img src="../Help/Images/Question16.png" alt="Question (small version for the summary)"/>
        /// Gets the Question icon displaying a white "?" in a blue circle (Sizes: 256x256, 64x64, 48x48, 32x32, 16x16)
        /// </summary>
        /// <remarks>
        /// <para>
        /// The icon contains the following images:<br/>
        /// <img src="../Help/Images/Question256.png" alt="Question 256x256"/>
        /// <img src="../Help/Images/Question64.png" alt="Question 64x64"/>
        /// <img src="../Help/Images/Question48.png" alt="Question 48x48"/>
        /// <img src="../Help/Images/Question32.png" alt="Question 32x32"/>
        /// <img src="../Help/Images/Question16.png" alt="Question 16x16"/>
        /// </para>
        /// </remarks>
        public static Icon Question => GetResourceIcon(IconId.Question).ToIcon();

        /// <summary>
        /// <img src="../Help/Images/Error16.png" alt="Error (small version for the summary)"/>
        /// Gets the Error icon displaying a white "X" in a red circle (Sizes: 256x256, 48x48, 32x32, 24x24, 16x16)
        /// </summary>
        /// <remarks>
        /// <para>
        /// The icon contains the following images:<br/>
        /// <img src="../Help/Images/Error256.png" alt="Error 256x256"/>
        /// <img src="../Help/Images/Error48.png" alt="Error 48x48"/>
        /// <img src="../Help/Images/Error32.png" alt="Error 32x32"/>
        /// <img src="../Help/Images/Error24.png" alt="Error 24x24"/>
        /// <img src="../Help/Images/Error16.png" alt="Error 16x16"/>
        /// </para>
        /// </remarks>
        public static Icon Error => GetResourceIcon(IconId.Error).ToIcon();

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
        public static Icon Shield => GetResourceIcon(IconId.Shield).ToIcon();

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
        public static Icon SecurityShield => GetResourceIcon(IconId.SecurityShield).ToIcon();

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
        public static Icon SecuritySuccess => GetResourceIcon(IconId.SecuritySuccess).ToIcon();

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
        public static Icon SecurityWarning => GetResourceIcon(IconId.SecurityWarning).ToIcon();

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
        public static Icon SecurityQuestion => GetResourceIcon(IconId.SecurityQuestion).ToIcon();

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
        public static Icon SecurityError => GetResourceIcon(IconId.SecurityError).ToIcon();

        /// <summary>
        /// <img src="../Help/Images/Application16.png" alt="Application (small version for the summary)"/>
        /// Gets the Application icon displaying a window (Sizes: 256x256, 48x48, 32x32, 24x24, 16x16)
        /// </summary>
        /// <remarks>
        /// <para>
        /// The icon contains the following images:<br/>
        /// <img src="../Help/Images/Application256.png" alt="Application 256x256"/>
        /// <img src="../Help/Images/Application48.png" alt="Application 48x48"/>
        /// <img src="../Help/Images/Application32.png" alt="Application 32x32"/>
        /// <img src="../Help/Images/Application24.png" alt="Application 24x24"/>
        /// <img src="../Help/Images/Application16.png" alt="Application 16x16"/>
        /// </para>
        /// </remarks>
        public static Icon Application => GetResourceIcon(IconId.Application).ToIcon();

        #endregion

        #region Private Properties

        private static ResourceManager ResourceManager => resourceManager ?? (resourceManager = new ResourceManager(typeof(Icons)));

        #endregion

        #endregion

        #region Methods

        private static Icon GetSystemIcon(IconId id, Func<Icon> getLegacyIcon)
        {
            if (!systemIconsCache.TryGetValue(id, out RawIcon result))
            {
                result = GetSystemIconById(id) ?? ToCombinedIcon(getLegacyIcon.Invoke());
                systemIconsCache[id] = result;
            }

            return result.ToIcon();
        }

        private static RawIcon GetResourceIcon(IconId id)
        {
            if (resourceIconsCache.TryGetValue(id, out RawIcon result))
                return result;

            result = new RawIcon((Icon)ResourceManager.GetObject(Enum<IconId>.ToString(id) + "Icon", CultureInfo.InvariantCulture));
            resourceIconsCache[id] = result;
            return result;
        }

        /// <summary>
        /// Tries to get the system icon by id. When there is no icon defined for provided <paramref name="id"/>,
        /// or Windows version is below Vista, this method returns <see langword="null"/>.
        /// On Windows XP use the predefined property members to retrieve system icons.
        /// </summary>
        /// <param name="id">Id of the icon to retrieve.</param>
        /// <returns>An <see cref="Icon"/> instance containing a small and large icon when an icon belongs to <paramref name="id"/>, or <see langword="null"/>,
        /// when no icon found, or Windows version is below Vista.</returns>
        private static RawIcon GetSystemIconById(IconId id)
        {
            if (id < 0 || !WindowsUtils.IsVistaOrLater)
                return null;

            SHSTOCKICONINFO iconInfo = new SHSTOCKICONINFO();
            iconInfo.cbSize = (uint)Marshal.SizeOf(typeof(SHSTOCKICONINFO));

            SHGSI flags = SHGSI.SHGSI_ICON | SHGSI.SHGSI_LARGEICON;
            if (Shell32.SHGetStockIconInfo((int)id, flags, ref iconInfo) != 0)
                return null;

            Icon icon = Icon.FromHandle(iconInfo.hIcon);
            var result = new RawIcon(icon);
            User32.DestroyIcon(iconInfo.hIcon);

            flags = SHGSI.SHGSI_ICON | SHGSI.SHGSI_SMALLICON;
            if (Shell32.SHGetStockIconInfo((int)id, flags, ref iconInfo) != 0)
                return result;

            icon = Icon.FromHandle(iconInfo.hIcon);
            result.Add(icon);
            User32.DestroyIcon(iconInfo.hIcon);
            return result;
        }

        /// <summary>
        /// Gets a multi size version of a system icon provided in <paramref name="icon"/> by generating the small version internally.
        /// </summary>
        private static RawIcon ToCombinedIcon(Icon icon)
        {
            Bitmap imageLarge = icon.ToAlphaBitmap();
            Bitmap imageSmall = imageLarge.Resize(new Size(16, 16), true);
            RawIcon result = new RawIcon();
            result.Add(imageLarge);
            result.Add(imageSmall);
            imageLarge.Dispose();
            imageSmall.Dispose();
            return result;
        }

        #endregion
    }
}
