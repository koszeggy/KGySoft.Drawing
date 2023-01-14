#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: VariableStrengthDitheringSessionBase.cs
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

#endregion

namespace KGySoft.Drawing.Imaging
{
    internal abstract class VariableStrengthDitheringSessionBase : IDitheringSession
    {
        #region Properties

        #region Public Properties

        public abstract bool IsSequential { get; }

        #endregion

        #region Protected Properties

        protected IQuantizingSession QuantizingSession { get; }

        protected float Strength { get; set; }

        #endregion

        #endregion

        #region Constructors

        protected VariableStrengthDitheringSessionBase(IQuantizingSession quantizingSession)
        {
            QuantizingSession = quantizingSession ?? throw new ArgumentNullException(nameof(quantizingSession), PublicResources.ArgumentNull);
        }

        #endregion

        #region Methods

        #region Public Methods

        public void Dispose()
        {
        }

        public Color32 GetDitheredColor(Color32 origColor, int x, int y)
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

            return QuantizeWithOffset(result, GetOffset(x, y));
        }

        #endregion

        #region Protected Methods

        protected abstract sbyte GetOffset(int x, int y);

        protected void CalibrateStrength(sbyte min, sbyte max)
        {
            // Calibrating strength between 0 and 1
            Strength = 1f;

            Color32 quantizedWhite = QuantizingSession.GetQuantizedColor(Color32.White);
            Color32 quantizedBlack = QuantizingSession.GetQuantizedColor(Color32.Black);

            // Checking 1 (strongest) first. If this is alright, we are done
            if (CheckStrength(quantizedWhite, quantizedBlack, min, max))
                return;

            // Halving the strength until we find an acceptable value
            while (true)
            {
                Strength /= 2f;
                if (CheckStrength(quantizedWhite, quantizedBlack, min, max))
                    break;
            }

            // Doing the same again with the lastly found good value as upper limit
            float lo = Strength;
            float hi = Strength * 2f;
            while (true)
            {
                Strength = (hi + lo) / 2f;
                if (CheckStrength(quantizedWhite, quantizedBlack, min, max))
                    break;
                hi = Strength;
            }
        }

        protected Color32 QuantizeWithOffset(Color32 c, sbyte offset)
        {
            if (Strength < 1f)
                offset = (sbyte)(offset * Strength);

            Color32 result = new Color32(
                    (c.R + offset).ClipToByte(),
                    (c.G + offset).ClipToByte(),
                    (c.B + offset).ClipToByte());

            // getting the quantized value of the dithered result
            // (it might be quantized further if the target image cannot represent it)
            return QuantizingSession.GetQuantizedColor(result);
        }

        #endregion

        #region Private Methods

        private bool CheckStrength(Color32 quantizedWhite, Color32 quantizedBlack, sbyte min, sbyte max)
        {
            // Current strength is considered alright if neither whitest nor blackest color is
            // affected by min/max dither offset. This prevents "overdithering" black and white colors
            // while reduces banding. Of course, if colors are not evenly distributed banding will
            // be not perfectly removed and even overdithering may occur between some colors.
            return QuantizeWithOffset(Color32.White, min) == quantizedWhite
                && QuantizeWithOffset(Color32.Black, max) == quantizedBlack;
        }

        #endregion

        #endregion
    }
}
