#region Used namespaces

using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;

using KGySoft.Drawing.Properties;
using KGySoft.Drawing.WinApi;
using KGySoft.Libraries;

#endregion

namespace KGySoft.Drawing
{
    using Resources = KGySoft.Drawing.Properties.Resources;
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

        private static readonly Dictionary<IconId, RawIcon> systemIconsCache = new Dictionary<IconId, RawIcon>(EnumComparer<IconId>.Comparer);
        private static readonly Dictionary<IconId, RawIcon> localIconsCache = new Dictionary<IconId, RawIcon>(EnumComparer<IconId>.Comparer);

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
        public static Icon SystemInformation
        {
            get
            {
                IconId id = IconId.Information;
                RawIcon icon;
                if (systemIconsCache.TryGetValue(id, out icon))
                    return icon.ToIcon();

                Icon result = TryGetSystemIconById((int)id);
                if (result != null)
                    return result;

                return RetrieveLegacySystemIcon(id, SystemIcons.Information);
            }
        }

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
        public static Icon SystemWarning
        {
            get
            {
                IconId id = IconId.Warning;
                RawIcon icon;
                if (systemIconsCache.TryGetValue(id, out icon))
                    return icon.ToIcon();

                Icon result = TryGetSystemIconById((int)id);
                if (result != null)
                    return result;

                return RetrieveLegacySystemIcon(id, SystemIcons.Warning);
            }
        }

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
        public static Icon SystemError
        {
            get
            {
                IconId id = IconId.Error;
                RawIcon icon;
                if (systemIconsCache.TryGetValue(id, out icon))
                    return icon.ToIcon();

                Icon result = TryGetSystemIconById((int)id);
                if (result != null)
                    return result;

                return RetrieveLegacySystemIcon(id, SystemIcons.Error);
            }
        }

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
        public static Icon SystemQuestion
        {
            get
            {
                IconId id = IconId.Question;
                RawIcon icon;
                if (systemIconsCache.TryGetValue(id, out icon))
                    return icon.ToIcon();

                Icon result = TryGetSystemIconById((int)id);
                if (result != null)
                    return result;

                return RetrieveLegacySystemIcon(id, SystemIcons.Question);
            }
        }

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
        public static Icon SystemApplication
        {
            get
            {
                IconId id = IconId.Application;
                RawIcon icon;
                if (systemIconsCache.TryGetValue(id, out icon))
                    return icon.ToIcon();

                Icon result = TryGetSystemIconById((int)id);
                if (result != null)
                    return result;

                return RetrieveLegacySystemIcon(id, SystemIcons.Application);
            }
        }

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
        public static Icon SystemShield
        {
            get
            {
                IconId id = IconId.Shield;
                RawIcon icon;
                if (systemIconsCache.TryGetValue(id, out icon))
                    return icon.ToIcon();

                Icon result = TryGetSystemIconById((int)id);
                if (result != null)
                    return result;

                icon = new RawIcon(SystemIcons.Shield);
                systemIconsCache[id] = icon;
                return icon.ToIcon();
            }
        }

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
        public static Icon Information
        {
            get { return InformationIcon.ToIcon(); }
        }

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
        public static Icon Warning
        {
            get { return WarningIcon.ToIcon(); }
        }

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
        public static Icon Question
        {
            get { return QuestionIcon.ToIcon(); }
        }

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
        public static Icon Error
        {
            get { return ErrorIcon.ToIcon(); }
        }

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
        public static Icon Shield
        {
            get { return ShieldIcon.ToIcon(); }
        }

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
        public static Icon SecurityShield
        {
            get { return SecurityShieldIcon.ToIcon(); }
        }

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
        public static Icon SecuritySuccess
        {
            get { return SecuritySuccessIcon.ToIcon(); }
        }

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
        public static Icon SecurityWarning
        {
            get { return SecurityWarningIcon.ToIcon(); }
        }

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
        public static Icon SecurityQuestion
        {
            get { return SecurityQuestionIcon.ToIcon(); }
        }

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
        public static Icon SecurityError
        {
            get { return SecurityErrorIcon.ToIcon(); }
        }

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
        public static Icon Application
        {
            get { return ApplicationIcon.ToIcon(); }
        }

        #endregion

        #region Internal Properties

        internal static RawIcon InformationIcon
        {
            get
            {
                IconId id = IconId.Information;
                RawIcon icon;
                if (!localIconsCache.TryGetValue(id, out icon))
                    localIconsCache[id] = icon = new RawIcon(Resources.InformationIcon);

                return icon;
            }
        }

        internal static RawIcon WarningIcon
        {
            get
            {
                IconId id = IconId.Warning;
                RawIcon icon;
                if (!localIconsCache.TryGetValue(id, out icon))
                    localIconsCache[id] = icon = new RawIcon(Resources.WarningIcon);

                return icon;
            }
        }

        internal static RawIcon QuestionIcon
        {
            get
            {
                IconId id = IconId.Question;
                RawIcon icon;
                if (!localIconsCache.TryGetValue(id, out icon))
                    localIconsCache[id] = icon = new RawIcon(Resources.QuestionIcon);

                return icon;
            }
        }

        internal static RawIcon ErrorIcon
        {
            get
            {
                IconId id = IconId.Error;
                RawIcon icon;
                if (!localIconsCache.TryGetValue(id, out icon))
                    localIconsCache[id] = icon = new RawIcon(Resources.ErrorIcon);

                return icon;
            }
        }

        internal static RawIcon ShieldIcon
        {
            get
            {
                IconId id = IconId.Shield;
                RawIcon icon;
                if (!localIconsCache.TryGetValue(id, out icon))
                    localIconsCache[id] = icon = new RawIcon(Resources.ShieldIcon);

                return icon;
            }
        }

        internal static RawIcon SecurityShieldIcon
        {
            get
            {
                IconId id = IconId.SecurityShield;
                RawIcon icon;
                if (!localIconsCache.TryGetValue(id, out icon))
                    localIconsCache[id] = icon = new RawIcon(Resources.SecurityShieldIcon);

                return icon;
            }
        }

        #endregion

        #region Private Properties

        private static RawIcon SecuritySuccessIcon
        {
            get
            {
                IconId id = IconId.SecuritySuccess;
                RawIcon icon;
                if (!localIconsCache.TryGetValue(id, out icon))
                    localIconsCache[id] = icon = new RawIcon(Resources.SecuritySuccessIcon);

                return icon;
            }
        }

        private static RawIcon SecurityWarningIcon
        {
            get
            {
                IconId id = IconId.SecurityWarning;
                RawIcon icon;
                if (!localIconsCache.TryGetValue(id, out icon))
                    localIconsCache[id] = icon = new RawIcon(Resources.SecurityWarningIcon);

                return icon;
            }
        }

        private static RawIcon SecurityQuestionIcon
        {
            get
            {
                IconId id = IconId.SecurityQuestion;
                RawIcon icon;
                if (!localIconsCache.TryGetValue(id, out icon))
                    localIconsCache[id] = icon = new RawIcon(Resources.SecurityQuestionIcon);

                return icon;
            }
        }

        private static RawIcon SecurityErrorIcon
        {
            get
            {
                IconId id = IconId.SecurityError;
                RawIcon icon;
                if (!localIconsCache.TryGetValue(id, out icon))
                    localIconsCache[id] = icon = new RawIcon(Resources.SecurityErrorIcon);

                return icon;
            }
        }

        private static RawIcon ApplicationIcon
        {
            get
            {
                IconId id = IconId.Application;
                RawIcon icon;
                if (!localIconsCache.TryGetValue(id, out icon))
                    localIconsCache[id] = icon = new RawIcon(Resources.ApplicationIcon);

                return icon;
            }
        }

        #endregion

        #endregion

        #region Methods

        #region Public Methods

        /// <summary>
        /// Tries to get the system icon by id. When there is no icon defined for provided <paramref name="id"/>,
        /// or Windows version is below Vista, this method returns <see langword="null"/>.
        /// On Windows XP use the predefined property members to retrieve system icons.
        /// </summary>
        /// <param name="id">Id of the icon to retrieve.</param>
        /// <returns>An <see cref="Icon"/> instance containing a small and large icon when an icon belongs to <paramref name="id"/>, or <see langword="null"/>,
        /// when no icon found, or Windows version is below Vista.</returns>
        public static Icon TryGetSystemIconById(int id)
        {
            if (id < 0 || !WindowsUtils.IsVistaOrLater)
                return null;

            RawIcon rawIcon;
            if (systemIconsCache.TryGetValue((IconId)id, out rawIcon))
                return rawIcon.ToIcon();

            SHSTOCKICONINFO iconInfo = new SHSTOCKICONINFO();
            iconInfo.cbSize = (uint)Marshal.SizeOf(typeof(SHSTOCKICONINFO));

            SHGSI flags = SHGSI.SHGSI_ICON | SHGSI.SHGSI_LARGEICON;
            if (Shell32.SHGetStockIconInfo(id, flags, ref iconInfo) != 0)
                return null;

            Icon icon = Icon.FromHandle(iconInfo.hIcon);
            rawIcon = new RawIcon(icon);
            User32.DestroyIcon(iconInfo.hIcon);

            flags = SHGSI.SHGSI_ICON | SHGSI.SHGSI_SMALLICON;
            if (Shell32.SHGetStockIconInfo(id, flags, ref iconInfo) != 0)
            {
                systemIconsCache.Add((IconId)id, rawIcon);
                return rawIcon.ToIcon();
            }

            icon = Icon.FromHandle(iconInfo.hIcon);
            rawIcon.Add(icon);
            User32.DestroyIcon(iconInfo.hIcon);
            systemIconsCache.Add((IconId)id, rawIcon);
            return rawIcon.ToIcon();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Gets a multi size version of a system icon provided in <paramref name="icon"/> by generating the small version internally.
        /// </summary>
        private static Icon RetrieveLegacySystemIcon(IconId id, Icon icon)
        {
            Bitmap imageLarge = icon.ToAlphaBitmap();
            Bitmap imageSmall = imageLarge.Resize(new Size(16, 16), true);
            RawIcon cacheItem = new RawIcon();
            cacheItem.Add(imageLarge);
            cacheItem.Add(imageSmall);
            systemIconsCache[id] = cacheItem;
            imageLarge.Dispose();
            imageSmall.Dispose();
            return cacheItem.ToIcon();
        }

        #endregion

        #endregion
    }
}
