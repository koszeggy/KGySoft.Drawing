#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ColorExtensions.cs
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
        /// Converts a <see cref="Color">System.Windows.Media.Color</see> struct to <see cref="Color32">KGySoft.Drawing.Imaging.Color32</see>.
        /// </summary>
        /// <param name="color">The source color.</param>
        /// <returns>The result of the conversion.</returns>
        public static Color32 ToColor32(this Color color) => new Color32(color.A, color.R, color.G, color.B);

        /// <summary>
        /// Converts a <see cref="Color32">KGySoft.Drawing.Imaging.Color32</see> struct to <see cref="Color">System.Windows.Media.Color</see>.
        /// </summary>
        /// <param name="color">The source color.</param>
        /// <returns>The result of the conversion.</returns>
        public static Color ToMediaColor(this Color32 color) => Color.FromArgb(color.A, color.R, color.G, color.B);

        /// <summary>
        /// Converts a <see cref="Color">System.Windows.Media.Color</see> struct to <see cref="System.Drawing.Color">System.Drawing.Color</see>.
        /// </summary>
        /// <param name="color">The source color.</param>
        /// <returns>The result of the conversion.</returns>
        public static System.Drawing.Color ToDrawingColor(this Color color) => System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);

        /// <summary>
        /// Converts a <see cref="System.Drawing.Color">System.Drawing.Color</see> struct to <see cref="Color">System.Windows.Media.Color</see>.
        /// </summary>
        /// <param name="color">The source color.</param>
        /// <returns>The result of the conversion.</returns>
        public static Color ToMediaColor(this System.Drawing.Color color) => Color.FromArgb(color.A, color.R, color.G, color.B);

        #endregion
    }
}
