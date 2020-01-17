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
    public sealed class RandomNoiseDitherer : IDitherer
    {
        #region OrderedDitheringSession class

        private sealed class RandomNoiseDitheringSession : IDitheringSession
        {
            #region Fields

            private readonly IQuantizingSession quantizer;
            private readonly RandomNoiseDitherer ditherer;
            private readonly float strength;
            private readonly Random random;

            #endregion

            #region Properties

            // if we have a seed we need to produce a consistent result
            public bool IsSequential => ditherer.seed.HasValue;

            #endregion

            #region Constructors

            internal RandomNoiseDitheringSession(IQuantizingSession quantizer, RandomNoiseDitherer ditherer)
            {
                this.quantizer = quantizer ?? throw new ArgumentNullException(nameof(quantizer), PublicResources.ArgumentNull);
                this.ditherer = ditherer;

                // If we have don't have a seed, we must use a thread safe random generator because pixels can be obtained in any order
                random = ditherer.seed == null ? new ThreadSafeRandom() : new Random(ditherer.seed.Value);

                if (ditherer.strength > 0f)
                {
                    strength = ditherer.strength;
                    return;
                }

                // Auto strength is calculated by color count. The correct value actually depends on the
                // used quantizer and the image. In general case (with not so perfect colors) the lower is better.
                int colorCount = quantizer.Palette?.Length ?? 0; // or 32768, 2^24 but we don't know exactly
                strength = colorCount == 0 ? 1 / 32f
                    : colorCount == 2 ? 1f
                    : 1 / (Math.Min(colorCount + 1, 16) / 2f);
            }

            #endregion

            #region Methods

            public Color32 GetDitheredColor(Color32 origColor, int x, int y)
            {
                Color32 currentColor;

                // handling alpha (and calling GetQuantizedColor before quantizing only if really necessary)
                if (origColor.A != Byte.MaxValue)
                {
                    // the color can be considered fully transparent
                    if (origColor.A < quantizer.AlphaThreshold)
                    {
                        // and even the quantizer returns a transparent color
                        currentColor = quantizer.GetQuantizedColor(origColor);
                        if (currentColor.A == 0)
                            return currentColor;
                    }

                    // the color will not be transparent in the end: blending
                    currentColor = origColor.BlendWithBackground(quantizer.BackColor);
                }
                else
                    currentColor = origColor;

                // generating random numbers between -127 and 127 so completely white/black pixels will not change
                int offset = random.NextSByte(-127, 127, true);
                if (strength < 1)
                    offset = (int)(offset * strength);

                currentColor = new Color32(
                    (currentColor.R + offset).ClipToByte(),
                    (currentColor.G + offset).ClipToByte(),
                    (currentColor.B + offset).ClipToByte());

                // getting the quantized value of the dithered result
                // (it might be quantized further if the target image cannot represent it)
                return quantizer.GetQuantizedColor(currentColor);
            }

            public void Dispose()
            {
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

        private RandomNoiseDitherer(float strength, int? seed)
        {
            this.strength = strength;
            this.seed = seed;
        }

        #endregion

        #region Methods

        #region Static Methods

        public static RandomNoiseDitherer WhiteNoise(float strength = 0f, int? seed = null) => new RandomNoiseDitherer(strength, seed);

        #endregion

        #region Instance Methods

        IDitheringSession IDitherer.Initialize(IBitmapDataAccessor source, IQuantizingSession quantizer)
            => new RandomNoiseDitheringSession(quantizer, this);

        #endregion

        #endregion
    }
}
