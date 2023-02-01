#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: VariableStrengthDitheringSessionLinearBase.cs
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

using System;
using System.Runtime.CompilerServices;

#endregion

namespace KGySoft.Drawing.Imaging
{
    internal abstract class VariableStrengthDitheringSessionLinearBase : VariableStrengthDitheringSessionBase
    {
        #region Constants

        // Min/Max offsets are within -0.5..0.5 exclusive bounds so if calibrated for black and white, even the max strength
        // will not change a purely white or black pixel.
        protected const float MinOffset = -127f / 255f;
        protected const float MaxOffset = 127f / 255f;

        #endregion

        #region Fields

        private float whiteStrength;
        private float blackStrength;

        #endregion

        #region Constructors

        protected VariableStrengthDitheringSessionLinearBase(IQuantizingSession quantizingSession) : base(quantizingSession)
        {
        }

        #endregion

        #region Methods

        #region Public Methods

        public sealed override Color32 GetDitheredColor(Color32 origColor, int x, int y)
        {
            Color32 result;

            // handling alpha
            if (origColor.A != Byte.MaxValue)
            {
                result = QuantizingSession.BlendOrMakeTransparent(origColor);
                if (result.A == 0)
                    return result;
            }
            else
                result = origColor;

            float offset = GetOffset(x, y);
            return Strength > 0f
                ? DoGetDitheredColor(result, offset * Strength)
                : DoGetDitheredColorDynamic(result, offset);
        }

        #endregion

        #region Protected Methods

        protected abstract float GetOffset(int x, int y);

        protected float CalibrateStrength(float min, float max, bool dynamicStrengthCalibration)
        {
            Color32 quantizedWhite = QuantizingSession.GetQuantizedColor(Color32.White);
            Color32 quantizedBlack = QuantizingSession.GetQuantizedColor(Color32.Black);

            float white = DoCalibrateStrength(Color32.White, quantizedWhite, min);
            float black = DoCalibrateStrength(Color32.Black, quantizedBlack, max);

            if (!dynamicStrengthCalibration || white.Equals(black))
                return Math.Min(white, black);

            whiteStrength = white;
            blackStrength = black;
            return 0f;
        }

        #endregion

        #region Private Methods

        private float DoCalibrateStrength(Color32 referenceColor, Color32 expectedResult, float offset)
        {
            if (DoGetDitheredColor(referenceColor, offset) == expectedResult)
                return 1f;

            float result = 1f;

            // Halving the strength until we find an acceptable value
            while (true)
            {
                result /= 2f;
                if (DoGetDitheredColor(referenceColor, offset * result) == expectedResult)
                    break;
            }

            // Doing the same again with the lastly found good value as upper limit
            float lo = result;
            float hi = result * 2f;
            while (true)
            {
                result = (hi + lo) / 2f;
                if (DoGetDitheredColor(referenceColor, offset * result) == expectedResult)
                    break;
                hi = result;
            }

            return result;
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        private Color32 DoGetDitheredColor(Color32 c, float offset)
        {
            // Here offset is normalized with Strength
            return QuantizingSession.GetQuantizedColor((c.ToColorF() + new RgbF(offset)).ToColor32());
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        private Color32 DoGetDitheredColorDynamic(Color32 c, float offset)
        {
            // Here we need to adjust offset by black/white strength
            ColorF colorF = c.ToColorF();
            float brightness = colorF.GetBrightness();
            offset *= whiteStrength > blackStrength
                ? (whiteStrength - blackStrength) * brightness + blackStrength
                : (blackStrength - whiteStrength) * (1 - brightness) + whiteStrength;
            return QuantizingSession.GetQuantizedColor((colorF + new RgbF(offset)).ToColor32());
        }

        #endregion

        #endregion
    }
}
