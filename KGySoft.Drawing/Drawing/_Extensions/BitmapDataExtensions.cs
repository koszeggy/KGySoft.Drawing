﻿using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Text;

namespace KGySoft.Drawing
{
    internal static class BitmapDataExtensions
    {
        internal static BitmapLine GetLine(this BitmapData bitmapData, int y) => new BitmapLine(bitmapData, y);
    }
}
