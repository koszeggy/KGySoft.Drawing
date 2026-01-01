#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ColorExtensions.cs
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

using Windows.UI;

using KGySoft.Drawing.Imaging;

#endregion

namespace KGySoft.Drawing.WinUI
{
    /// <summary>
    /// Contains extension methods for the <a href="https://learn.microsoft.com/en-us/dotnet/api/windows.ui.color" target="_blank">Color</a> type.
    /// </summary>
    public static class ColorExtensions
    {
        #region Methods

        /// <summary>
        /// Converts a <a href="https://learn.microsoft.com/en-us/dotnet/api/windows.ui.color" target="_blank">Windows.UI.Color</a> struct to <see cref="System.Drawing.Color">System.Drawing.Color</see>.
        /// </summary>
        /// <param name="color">The source color.</param>
        /// <returns>The result of the conversion.</returns>
        public static System.Drawing.Color ToDrawingColor(this Color color) => System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);

        /// <summary>
        /// Converts a <a href="https://learn.microsoft.com/en-us/dotnet/api/windows.ui.color" target="_blank">Windows.UI.Color</a> struct to <see cref="Color32"/>.
        /// </summary>
        /// <param name="color">The source color.</param>
        /// <returns>The result of the conversion.</returns>
        public static Color32 ToColor32(this Color color) => new (color.A, color.R, color.G, color.B);

        /// <summary>
        /// Converts a <a href="https://learn.microsoft.com/en-us/dotnet/api/windows.ui.color" target="_blank">Windows.UI.Color</a> struct to <see cref="PColor32"/>.
        /// </summary>
        /// <param name="color">The source color.</param>
        /// <returns>The result of the conversion.</returns>
        public static PColor32 ToPColor32(this Color color) => color.ToColor32().ToPColor32();

        /// <summary>
        /// Converts a <a href="https://learn.microsoft.com/en-us/dotnet/api/windows.ui.color" target="_blank">Windows.UI.Color</a> struct to <see cref="Color64"/>.
        /// </summary>
        /// <param name="color">The source color.</param>
        /// <returns>The result of the conversion.</returns>
        public static Color64 ToColor64(this Color color) => color.ToColor32().ToColor64();

        /// <summary>
        /// Converts a <a href="https://learn.microsoft.com/en-us/dotnet/api/windows.ui.color" target="_blank">Windows.UI.Color</a> struct to <see cref="PColor64"/>.
        /// </summary>
        /// <param name="color">The source color.</param>
        /// <returns>The result of the conversion.</returns>
        public static PColor64 ToPColor64(this Color color) => color.ToColor32().ToPColor64();

        /// <summary>
        /// Converts a <a href="https://learn.microsoft.com/en-us/dotnet/api/windows.ui.color" target="_blank">Windows.UI.Color</a> struct to <see cref="ColorF"/>.
        /// </summary>
        /// <param name="color">The source color.</param>
        /// <returns>The result of the conversion.</returns>
        public static ColorF ToColorF(this Color color) => color.ToColor32().ToColorF();

        /// <summary>
        /// Converts a <a href="https://learn.microsoft.com/en-us/dotnet/api/windows.ui.color" target="_blank">Windows.UI.Color</a> struct to <see cref="PColorF"/>.
        /// </summary>
        /// <param name="color">The source color.</param>
        /// <returns>The result of the conversion.</returns>
        public static PColorF ToPColorF(this Color color) => color.ToColor32().ToPColorF();

        /// <summary>
        /// Converts a <see cref="System.Drawing.Color">System.Drawing.Color</see> struct to <a href="https://learn.microsoft.com/en-us/dotnet/api/windows.ui.color" target="_blank">Windows.UI.Color</a>.
        /// </summary>
        /// <param name="color">The source color.</param>
        /// <returns>The result of the conversion.</returns>
        public static Color ToWindowsColor(this System.Drawing.Color color) => Color.FromArgb(color.A, color.R, color.G, color.B);

        /// <summary>
        /// Converts a <see cref="Color32"/> struct to <a href="https://learn.microsoft.com/en-us/dotnet/api/windows.ui.color" target="_blank">Windows.UI.Color</a>.
        /// </summary>
        /// <param name="color">The source color.</param>
        /// <returns>The result of the conversion.</returns>
        public static Color ToWindowsColor(this Color32 color) => Color.FromArgb(color.A, color.R, color.G, color.B);

        /// <summary>
        /// Converts a <see cref="PColor32"/> struct to <a href="https://learn.microsoft.com/en-us/dotnet/api/windows.ui.color" target="_blank">Windows.UI.Color</a>.
        /// </summary>
        /// <param name="color">The source color.</param>
        /// <returns>The result of the conversion.</returns>
        public static Color ToWindowsColor(this PColor32 color) => color.ToColor32().ToWindowsColor();

        /// <summary>
        /// Converts a <see cref="Color64"/> struct to <a href="https://learn.microsoft.com/en-us/dotnet/api/windows.ui.color" target="_blank">Windows.UI.Color</a>.
        /// </summary>
        /// <param name="color">The source color.</param>
        /// <returns>The result of the conversion.</returns>
        public static Color ToWindowsColor(this Color64 color) => color.ToColor32().ToWindowsColor();

        /// <summary>
        /// Converts a <see cref="PColor64"/> struct to <a href="https://learn.microsoft.com/en-us/dotnet/api/windows.ui.color" target="_blank">Windows.UI.Color</a>.
        /// </summary>
        /// <param name="color">The source color.</param>
        /// <returns>The result of the conversion.</returns>
        public static Color ToWindowsColor(this PColor64 color) => color.ToColor32().ToWindowsColor();

        /// <summary>
        /// Converts a <see cref="ColorF"/> struct to <a href="https://learn.microsoft.com/en-us/dotnet/api/windows.ui.color" target="_blank">Windows.UI.Color</a>.
        /// </summary>
        /// <param name="color">The source color.</param>
        /// <returns>The result of the conversion.</returns>
        public static Color ToWindowsColor(this ColorF color) => color.ToColor32().ToWindowsColor();

        /// <summary>
        /// Converts a <see cref="PColorF"/> struct to <a href="https://learn.microsoft.com/en-us/dotnet/api/windows.ui.color" target="_blank">Windows.UI.Color</a>.
        /// </summary>
        /// <param name="color">The source color.</param>
        /// <returns>The result of the conversion.</returns>
        public static Color ToWindowsColor(this PColorF color) => color.ToColor32().ToWindowsColor();

        #endregion
    }
}
