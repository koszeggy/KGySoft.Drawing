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
        /// <img src="../Help/Images/Information16.png" alt="Information (small version for the summary)"/>
        /// Gets an <see cref="Icon"/> instance that contains a large and a small
        /// Information icon as it is stored in the current operating system.
        /// In Windows Vista and above sizes are depending on current DPI settings, in Windows XP they have always 32x32 and 16x16 sizes.
        /// </summary>
        /// <remarks>
        /// <para>
        /// In Windows Vista and Windows 7, with default DPI settings the icon contains the following images:<br/>
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
        /// <img src="../Help/Images/Warning16.png" alt="Warning (small version for the summary)"/>
        /// Gets an <see cref="Icon"/> instance that contains a large and a small
        /// Warning icon as it is stored in the current operating system.
        /// In Windows Vista and above sizes are depending on current DPI settings, in Windows XP they have always 32x32 and 16x16 sizes.
        /// </summary>
        /// <remarks>
        /// <para>
        /// In Windows Vista and Windows 7, with default DPI settings the icon contains the following images:<br/>
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
        /// <img src="../Help/Images/Error16.png" alt="Error (small version for the summary)"/>
        /// Gets an <see cref="Icon"/> instance that contains a large and a small
        /// Error icon as it is stored in the current operating system.
        /// In Windows Vista and above sizes are depending on current DPI settings, in Windows XP they have always 32x32 and 16x16 sizes.
        /// </summary>
        /// <remarks>
        /// <para>
        /// In Windows Vista and Windows 7, with default DPI settings the icon contains the following images:<br/>
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
        /// <img src="../Help/Images/Question16.png" alt="Question (small version for the summary)"/>
        /// Gets an <see cref="Icon"/> instance that contains a large and a small
        /// Question icon as it is stored in the current operating system.
        /// In Windows Vista and above sizes are depending on current DPI settings, in Windows XP they have always 32x32 and 16x16 sizes.
        /// </summary>
        /// <remarks>
        /// <para>
        /// In Windows Vista and Windows 7, with default DPI settings the icon contains the following images:<br/>
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
        /// <img src="../Help/Images/Application16.png" alt="Application (small version for the summary)"/>
        /// Gets an <see cref="Icon"/> instance that contains a large and a small
        /// Application icon as it is stored in the current operating system.
        /// In Windows Vista and above sizes are depending on current DPI settings, in Windows XP they have always 32x32 and 16x16 sizes.
        /// </summary>
        /// <remarks>
        /// <para>
        /// In Windows Vista and Windows 7, with default DPI settings the icon contains the following images:<br/>
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
        /// <img src="../Help/Images/SecurityShield16.png" alt="Shield (small version for the summary)"/>
        /// Gets an <see cref="Icon"/> instance that contains the
        /// Shield icon as it is stored in the current operating system.
        /// In Windows Vista and above icon contains two sizes, which are depending on current DPI settings,
        /// in Windows XP the icon contains multiple resolution and color depths.
        /// </summary>
        /// <remarks>
        /// <para>
        /// In Windows 7, with default DPI settings the icon contains the following images:<br/>
        /// <img src="../Help/Images/SecurityShield32.png" alt="Shield Windows 7 32x32"/>
        /// <img src="../Help/Images/SecurityShield16.png" alt="Shield Windows 7 16x16"/>
        /// </para>
        /// <para>
        /// In Windows Vista, with default DPI settings the icon contains the following images:<br/>
        /// <img src="../Help/Images/Shield32.png" alt="Shield Windows Vista 32x32"/>
        /// <img src="../Help/Images/Shield16.png" alt="Shield Windows Vista 16x16"/>
        /// </para>
        /// <para>
        /// In Windows XP the icon contains different color depth version of the following images:<br/>
        /// <img src="../Help/Images/ShieldXP48.png" alt="Shield Windows XP 48x48"/>
        /// <img src="../Help/Images/ShieldXP32.png" alt="Shield Windows XP 32x32"/>
        /// <img src="../Help/Images/ShieldXP16.png" alt="Shield Windows XP 16x16"/>
        /// </para>
        /// </remarks>
        public static Icon SystemShield => GetSystemIcon(StockIcon.Shield, () => SystemIcons.Shield);

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
        public static Icon Information => GetResourceIcon(nameof(Information));

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
        public static Icon Warning => GetResourceIcon(nameof(Warning));

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
        public static Icon Question => GetResourceIcon(nameof(Question));

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
        public static Icon Application => GetResourceIcon(nameof(Application));

        #endregion

        #region Private Properties

        private static ResourceManager ResourceManager => resourceManager ?? (resourceManager = new ResourceManager(typeof(Icons)));

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
        /// </remarks>
#if !NET35
        [SecuritySafeCritical]
#endif
        public static Icon[] FromFile(string fileName)
        {
            if (fileName == null)
                throw new ArgumentNullException(nameof(fileName), PublicResources.ArgumentNull);

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
        /// Gets the system-associated icon of a file extension.
        /// </summary>
        /// <param name="extension">A file name (can be a non-existing one) or an extension for which the associated icon is about to be retrieved.</param>
        /// <param name="size">The size of the icon to be retrieved.</param>
        /// <returns>The icon of the specified extension.</returns>
#if !NET35
        [SecuritySafeCritical]
#endif
        public static Icon FromExtension(string extension, SystemIconSize size)
        {
            if (extension == null)
                throw new ArgumentNullException(nameof(extension), PublicResources.ArgumentNull);
            if (!Enum<SystemIconSize>.IsDefined(size))
                throw new ArgumentOutOfRangeException(nameof(size), PublicResources.EnumOutOfRangeWithValues(size));

            if (!Path.HasExtension(extension))
                extension = Path.GetFileName(extension) == extension ? '.' + extension : ".";

            IntPtr handle = Shell32.GetFileIconHandle(extension, size);
            if (handle == IntPtr.Zero)
                throw new ArgumentException(PublicResources.ArgumentInvalidString, nameof(extension));

            return Icon.FromHandle(handle).ToManagedIcon();
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
        public static Icon Combine(params Icon[] icons) => Combine(!WindowsUtils.IsVistaOrLater, icons);

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
        /// <remarks>The result <see cref="Icon"/> is compatible with Windows XP if the method is executed in a Windows XP environment.</remarks>
        public static Icon Combine(params Bitmap[] images) => Combine(!WindowsUtils.IsVistaOrLater, images);

        /// <summary>
        /// Combines the provided <paramref name="images"/> into a multi-resolution <see cref="Icon"/> instance.
        /// </summary>
        /// <param name="forceUncompressedResult"><see langword="true"/>&#160;to force returning an uncompressed icon;
        /// <see langword="false"/>&#160;to allow PNG compression, which is supported by Windows Vista and above.</param>
        /// <param name="images">The images to be added to the result icon. Images can be non-squared ones.
        /// Transparency is determined automatically by image format.</param>
        /// <returns>An <see cref="Icon"/> instance that contains every image of the source <paramref name="images"/>.</returns>
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
        /// <remarks>The result <see cref="Icon"/> is compatible with Windows XP if the method is executed in a Windows XP environment.</remarks>
        public static Icon Combine(Bitmap[] images, Color[] transparentColors) => Combine(images, transparentColors, !WindowsUtils.IsVistaOrLater);

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
            User32.GetIconInfo(iconHandle, out ICONINFO iconInfo);
            iconInfo.xHotspot = cursorHotspot.X;
            iconInfo.yHotspot = cursorHotspot.Y;
            iconInfo.fIcon = false;
            return new CursorHandle(User32.CreateIconIndirect(ref iconInfo));
        }

        /// <summary>
        /// Creates an <see cref="Icon" /> from an <see cref="Image" />.
        /// </summary>
        /// <param name="image">The image to be converted to an icon.</param>
        /// <param name="size">The required size of the icon.</param>
        /// <param name="keepAspectRatio">When source <paramref name="image"/> is not square sized, determines whether the image should keep aspect ratio.</param>
        /// <returns>An <see cref="Icon"/> instance created from the <paramref name="image"/>.</returns>
        /// <remarks>The result icon will be always square sized. To create a non-square icon, use <see cref="Combine(Bitmap[])"/> instead.</remarks>
        [SecurityCritical]
        internal static Icon FromImage(Image image, int size, bool keepAspectRatio)
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image), PublicResources.ArgumentNull);

            Bitmap bitmap;
            if (size == image.Width && size == image.Height && (bitmap = image as Bitmap) != null)
            {
                return Icon.FromHandle(bitmap.GetHicon()).ToManagedIcon();
            }

            using (bitmap = new Bitmap(size, size))
            {
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    int x, y, w, h; // dimensions for new image

                    if (!keepAspectRatio || image.Height == image.Width)
                    {
                        // just fill the square
                        x = y = 0; // set x and y to 0
                        w = h = size; // set width and height to size
                    }
                    else
                    {
                        // work out the aspect ratio
                        float r = (float)image.Width / image.Height;
                        // set dimensions accordingly to fit inside size^2 square

                        if (r > 1)
                        { // w is bigger, so divide h by r
                            w = size;
                            h = (int)(size / r);
                            x = 0;
                            y = (size - h) / 2; // center the image
                        }
                        else
                        { // h is bigger, so multiply w by r
                            w = (int)(size * r);
                            h = size;
                            y = 0;
                            x = (size - w) / 2; // center the image
                        }
                    }
                    // make the image shrink nicely by using HighQualityBicubic mode
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.PixelOffsetMode = PixelOffsetMode.Half;
                    g.DrawImage(image, x, y, w, h); // draw image with specified dimensions
                    g.Flush(); // make sure all drawing operations complete before we get the icon

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

            return result.ToIcon(!WindowsUtils.IsVistaOrLater);
        }

        [SecurityCritical]
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "The result must not be disposed.")]
        private static RawIcon DoGetStockIcon(StockIcon id)
        {
            if (id < 0 || !WindowsUtils.IsVistaOrLater)
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
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "The result must not be disposed.")]
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

        #endregion
    }
}
