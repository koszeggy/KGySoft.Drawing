#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: VariableStrengthDitheringSessionSrgbBase.cs
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
    internal abstract class VariableStrengthDitheringSessionSrgbBase : VariableStrengthDitheringSessionBase
    {
        #region Fields
        
        private float whiteStrength;
        private float blackStrength;

        #endregion

        #region Constructors

        protected VariableStrengthDitheringSessionSrgbBase(IQuantizingSession quantizingSession) : base(quantizingSession)
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

            sbyte offset = GetOffset(x, y);
            return Strength switch
            {
                1f => DoGetDitheredColor(result, offset),
                >0f => DoGetDitheredColor(result, (sbyte)(offset * Strength)),
                _ => DoGetDitheredColorDynamic(result, offset)
            };
        }

        #endregion

        #region Protected Methods

        protected abstract sbyte GetOffset(int x, int y);

        protected float CalibrateStrength(sbyte min, sbyte max, bool dynamicStrengthCalibration)
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

        private float DoCalibrateStrength(Color32 referenceColor, Color32 expectedResult, sbyte offset)
        {
            if (DoGetDitheredColor(referenceColor, offset) == expectedResult)
                return 1f;

            float result = 1f;

            // Halving the strength until we find an acceptable value
            while (true)
            {
                result /= 2f;
                if (DoGetDitheredColor(referenceColor, (sbyte)(offset * result)) == expectedResult)
                    break;
            }

            // Doing the same again with the lastly found good value as upper limit
            float lo = result;
            float hi = result * 2f;
            while (true)
            {
                result = (hi + lo) / 2f;
                if (DoGetDitheredColor(referenceColor, (sbyte)(offset * result)) == expectedResult)
                    break;
                hi = result;
            }

            return result;
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        private Color32 DoGetDitheredColor(Color32 c, sbyte offset)
        {
            // getting the quantized value of the dithered result
            // (it might be quantized further if the target image cannot represent it)
            return QuantizingSession.GetQuantizedColor(new Color32(
                (c.R + offset).ClipToByte(),
                (c.G + offset).ClipToByte(),
                (c.B + offset).ClipToByte()));
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        private Color32 DoGetDitheredColorDynamic(Color32 c, sbyte offset)
        {
            // Here we need to adjust offset by black/white strength
            float brightness = c.GetBrightness() / 255f;
            offset = whiteStrength > blackStrength
                ? (sbyte)(offset * ((whiteStrength - blackStrength) * brightness + blackStrength))
                : (sbyte)(offset * ((blackStrength - whiteStrength) * (1 - brightness) + whiteStrength));
            return DoGetDitheredColor(c, offset);
        }

        #endregion

        #endregion
    }
}
