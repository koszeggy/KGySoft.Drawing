using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Maui.Graphics;

namespace KGySoft.Drawing.Examples.Maui.Extensions
{
    internal static class ColorExtensions
    {
        internal static System.Drawing.Color ToDrawingColor(this Color color) => System.Drawing.Color.FromArgb(color.ToInt());
    }
}
