#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: QuantizingSessionExtensions.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2025 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

namespace KGySoft.Drawing.Imaging
{
    internal static class QuantizingSessionExtensions
    {
        #region Methods

        internal static Color32 BlendOrMakeTransparent(this IQuantizingSession session, Color32 origColor)
        {
            // the color can be considered fully transparent
            if (origColor.A < session.AlphaThreshold)
            {
                // and even the quantizer returns a transparent color
                var c = session.GetQuantizedColor(origColor);
                if (c.A == 0)
                    return c;
            }

            // the color will not be transparent in the end: blending
            return origColor.BlendWithBackground(session.BackColor, session.WorkingColorSpace);
        }

        #endregion
    }
}