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

using System.Windows.Media;

using KGySoft.Drawing.Imaging;

#endregion

namespace KGySoft.Drawing.Wpf
{
    /// <summary>
    /// Contains extension methods for the <see cref="Color"/> type.
    /// </summary>
    public static class ColorExtensions
    {
        #region Methods

        /// <summary>
        /// Converts a <see cref="Color">System.Windows.Media.Color</see> struct to <see cref="System.Drawing.Color">System.Drawing.Color</see>.
        /// </summary>
        /// <param name="color">The source color.</param>
        /// <returns>The result of the conversion.</returns>
        public static System.Drawing.Color ToDrawingColor(this Color color) => System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);

        /// <summary>
        /// Converts a <see cref="Color">System.Windows.Media.Color</see> struct to <see cref="Color32"/>.
        /// </summary>
        /// <param name="color">The source color.</param>
        /// <returns>The result of the conversion.</returns>
        public static Color32 ToColor32(this Color color) => new Color32(color.A, color.R, color.G, color.B);

        /// <summary>
        /// Converts a <see cref="Color">System.Windows.Media.Color</see> struct to <see cref="PColor32"/>.
        /// </summary>
        /// <param name="color">The source color.</param>
        /// <returns>The result of the conversion.</returns>
        public static PColor32 ToPColor32(this Color color) => color.ToColor32().ToPColor32();

        /// <summary>
        /// Converts a <see cref="Color">System.Windows.Media.Color</see> struct to <see cref="Color64"/>.
        /// </summary>
        /// <param name="color">The source color.</param>
        /// <returns>The result of the conversion.</returns>
        public static Color64 ToColor64(this Color color) => color.ToColorF().ToColor64();

        /// <summary>
        /// Converts a <see cref="Color">System.Windows.Media.Color</see> struct to <see cref="PColor64"/>.
        /// </summary>
        /// <param name="color">The source color.</param>
        /// <returns>The result of the conversion.</returns>
        public static PColor64 ToPColor64(this Color color) => color.ToColorF().ToPColor64();

        /// <summary>
        /// Converts a <see cref="Color">System.Windows.Media.Color</see> struct to <see cref="ColorF"/>.
        /// </summary>
        /// <param name="color">The source color.</param>
        /// <returns>The result of the conversion.</returns>
        public static ColorF ToColorF(this Color color) => new ColorF(color.ScA, color.ScR, color.ScG, color.ScB);

        /// <summary>
        /// Converts a <see cref="Color">System.Windows.Media.Color</see> struct to <see cref="ColorF"/>.
        /// </summary>
        /// <param name="color">The source color.</param>
        /// <returns>The result of the conversion.</returns>
        public static PColorF ToPColorF(this Color color) => color.ToColorF().ToPColorF();

        /// <summary>
        /// Converts a <see cref="System.Drawing.Color">System.Drawing.Color</see> struct to <see cref="Color">System.Windows.Media.Color</see>.
        /// </summary>
        /// <param name="color">The source color.</param>
        /// <returns>The result of the conversion.</returns>
        public static Color ToMediaColor(this System.Drawing.Color color) => Color.FromArgb(color.A, color.R, color.G, color.B);

        /// <summary>
        /// Converts a <see cref="Color32"/> struct to <see cref="Color">System.Windows.Media.Color</see>.
        /// </summary>
        /// <param name="color">The source color.</param>
        /// <returns>The result of the conversion.</returns>
        public static Color ToMediaColor(this Color32 color) => Color.FromArgb(color.A, color.R, color.G, color.B);

        /// <summary>
        /// Converts a <see cref="PColor32"/> struct to <see cref="Color">System.Windows.Media.Color</see>.
        /// </summary>
        /// <param name="color">The source color.</param>
        /// <returns>The result of the conversion.</returns>
        public static Color ToMediaColor(this PColor32 color) => color.ToColor32().ToMediaColor();

        /// <summary>
        /// Converts a <see cref="Color64"/> struct to <see cref="Color">System.Windows.Media.Color</see>.
        /// </summary>
        /// <param name="color">The source color.</param>
        /// <returns>The result of the conversion.</returns>
        public static Color ToMediaColor(this Color64 color) => color.ToColorF().ToMediaColor();

        /// <summary>
        /// Converts a <see cref="PColor64"/> struct to <see cref="Color">System.Windows.Media.Color</see>.
        /// </summary>
        /// <param name="color">The source color.</param>
        /// <returns>The result of the conversion.</returns>
        public static Color ToMediaColor(this PColor64 color) => color.ToColorF().ToMediaColor();

        /// <summary>
        /// Converts a <see cref="ColorF"/> struct to <see cref="Color">System.Windows.Media.Color</see>.
        /// </summary>
        /// <param name="color">The source color.</param>
        /// <returns>The result of the conversion.</returns>
        public static Color ToMediaColor(this ColorF color) => Color.FromScRgb(color.A, color.R, color.G, color.B);

        /// <summary>
        /// Converts a <see cref="ColorF"/> struct to <see cref="Color">System.Windows.Media.Color</see>.
        /// </summary>
        /// <param name="color">The source color.</param>
        /// <returns>The result of the conversion.</returns>
        public static Color ToMediaColor(this PColorF color) => color.ToColorF().ToMediaColor();

        #endregion
    }
}
