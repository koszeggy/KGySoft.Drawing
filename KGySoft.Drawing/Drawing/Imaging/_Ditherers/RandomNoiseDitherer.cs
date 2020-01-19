#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: RandomNoiseDitherer.cs
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
using KGySoft.CoreLibraries;

#endregion

namespace KGySoft.Drawing.Imaging
{
    /// <summary>
    /// Represents a ditherer that applies a random white noise during the quantization.
    /// <br/>To apply non-random noise-like patterns you can use also the <see cref="OrderedDitherer.BlueNoise">OrderedDitherer.BlueNoise</see> method
    /// or the <see cref="InterleavedGradientNoiseDitherer"/> class.
    /// </summary>
    /// <seealso cref="IDitherer" />
    /// <seealso cref="OrderedDitherer" />
    /// <seealso cref="ErrorDiffusionDitherer" />
    /// <seealso cref="InterleavedGradientNoiseDitherer" />
    public sealed class RandomNoiseDitherer : IDitherer
    {
        #region RandomNoiseDitheringSession class

        private sealed class RandomNoiseDitheringSession : VariableStrengthDitheringSessionBase
        {
            #region Fields

            private readonly RandomNoiseDitherer ditherer;
            private readonly Random random;

            #endregion

            #region Properties

            // if we have a seed we need to produce a consistent result
            public override bool IsSequential => ditherer.seed.HasValue;

            #endregion

            #region Constructors

            internal RandomNoiseDitheringSession(IQuantizingSession quantizingSession, RandomNoiseDitherer ditherer)
                : base(quantizingSession)
            {
                this.ditherer = ditherer;

                // If we have don't have a seed, we must use a thread safe random generator because pixels can be queried in any order
                random = ditherer.seed == null ? new ThreadSafeRandom() : new Random(ditherer.seed.Value);

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
                // generating random numbers between -127 and 127 so completely white/black pixels will not change
                return random.NextSByte(-127, 127, true);
            }

            #endregion
        }

        #endregion

        #region Fields

        #region Instance Fields

        private readonly int? seed;
        private readonly float strength;

        #endregion

        #endregion

        #region Constructors

        public RandomNoiseDitherer(float strength = 0f, int? seed = null)
        {
            this.strength = strength;
            this.seed = seed;
        }

        #endregion

        #region Methods

        IDitheringSession IDitherer.Initialize(IBitmapDataAccessor source, IQuantizingSession quantizer)
            => new RandomNoiseDitheringSession(quantizer, this);

        #endregion
    }
}
