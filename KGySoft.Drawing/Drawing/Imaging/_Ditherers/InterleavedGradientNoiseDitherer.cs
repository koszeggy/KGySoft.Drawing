#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: InterleavedGradientNoiseDitherer.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2020 - All Rights Reserved
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

#endregion

namespace KGySoft.Drawing.Imaging
{
    public sealed class InterleavedGradientNoiseDitherer : IDitherer
    {
        #region InterleavedGradientNoiseDitheringSession class

        private sealed class InterleavedGradientNoiseDitheringSession : VariableStrengthDitheringSessionBase
        {
            #region Properties

            public override bool IsSequential => false;

            #endregion

            #region Constructors

            internal InterleavedGradientNoiseDitheringSession(IQuantizingSession quantizingSession, InterleavedGradientNoiseDitherer ditherer)
                : base(quantizingSession)
            {
                if (ditherer.strength > 0f)
                {
                    Strength = ditherer.strength;
                    return;
                }

                CalibrateStrength(-127, 127);
            }

            #endregion

            #region Methods

            protected override sbyte GetOffset(int x, int y)
            {
                static double Frac(double value) => value - Math.Floor(value);

                // Generating values between -127 and 127 so completely white/black pixels will not change
                // The formula is taken from here: https://bartwronski.com/2016/10/30/dithering-part-three-real-world-2d-quantization-dithering/
                return (sbyte)(Frac(52.9829189 * Frac(0.06711056 * x + 0.00583715 * y)) * 256 - 128);
            }

            #endregion
        }

        #endregion

        #region Fields

        #region Instance Fields

        private readonly float strength;

        #endregion

        #endregion

        #region Constructors

        public InterleavedGradientNoiseDitherer(float strength = 0f)
        {
            this.strength = strength;
        }

        #endregion

        #region Methods

        IDitheringSession IDitherer.Initialize(IBitmapDataAccessor source, IQuantizingSession quantizer)
            => new InterleavedGradientNoiseDitheringSession(quantizer, this);

        #endregion
    }
}
