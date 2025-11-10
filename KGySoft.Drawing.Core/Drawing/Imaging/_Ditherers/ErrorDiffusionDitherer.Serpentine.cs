#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ErrorDiffusionDitherer.Serpentine.cs
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
    public partial class ErrorDiffusionDitherer
    {
        #region DitheringSessionSerpentineSrgb class

        private sealed class DitheringSessionSerpentineSrgb : DitheringSessionRasterSrgb
        {
            #region Fields

            private readonly IReadableBitmapData source;
            private readonly Color32[] preprocessedResults;

            #endregion

            #region Constructors

            internal DitheringSessionSerpentineSrgb(IQuantizingSession quantizingSession, ErrorDiffusionDitherer ditherer, IReadableBitmapData source)
                : base(quantizingSession, ditherer, source)
            {
                this.source = source;
                preprocessedResults = new Color32[ImageWidth];
            }

            #endregion

            #region Methods

            #region Public Methods

            public override Color32 GetDitheredColor(Color32 origColor, int x, int y)
            {
                // even row: processing by base
                if ((y & 1) == 0)
                    return base.GetDitheredColor(origColor, x, y);

                if (y != LastRow)
                    PrepareNewRow(y);

                // odd row: returning preprocessed result
                Debug.Assert(origColor == source.GetColor32(x, y), "Unexpected odd row color. Make sure you initialize the session with the actual content when InitializeReliesOnContent returns true.");
                return preprocessedResults[x];
            }

            #endregion

            #region Protected Methods

            protected override void PrepareNewRow(int y)
            {
                base.PrepareNewRow(y);
                IsRightToLeft = (y & 1) == 1;
                if (IsRightToLeft)
                    PreprocessOddRow(y);
            }

            #endregion

            #region Private Methods

            private void PreprocessOddRow(int y)
            {
                // Odd rows are preprocessed so we don't need to complicate the IDitheringSession interface with processing path.
                // It is not even a significant overhead unless not every pixel is queried (e.g. DrawInto with clipped region).
                IReadableBitmapDataRow row = source[y];

                // processing odd rows from right to left
                for (int x = ImageWidth - 1; x >= 0; x--)
                    preprocessedResults[x] = DoErrorDiffusion(row[x], x, y);
            }

            #endregion

            #endregion
        }

        #endregion 
        
        #region DitheringSessionSerpentineLinear class

        private sealed class DitheringSessionSerpentineLinear: DitheringSessionRasterLinear
        {
            #region Fields

            private readonly IReadableBitmapData source;
            private readonly Color32[] preprocessedResults;

            #endregion

            #region Constructors

            internal DitheringSessionSerpentineLinear(IQuantizingSession quantizingSession, ErrorDiffusionDitherer ditherer, IReadableBitmapData source)
                : base(quantizingSession, ditherer, source)
            {
                this.source = source;
                preprocessedResults = new Color32[ImageWidth];
            }

            #endregion

            #region Methods

            #region Public Methods

            public override Color32 GetDitheredColor(Color32 origColor, int x, int y)
            {
                // even row: processing by base
                if ((y & 1) == 0)
                    return base.GetDitheredColor(origColor, x, y);

                if (y != LastRow)
                    PrepareNewRow(y);

                // odd row: returning preprocessed result
                Debug.Assert(origColor == source.GetColor32(x, y));
                return preprocessedResults[x];
            }

            #endregion

            #region Protected Methods

            protected override void PrepareNewRow(int y)
            {
                base.PrepareNewRow(y);
                IsRightToLeft = (y & 1) == 1;
                if (IsRightToLeft)
                    PreprocessOddRow(y);
            }

            #endregion

            #region Private Methods

            private void PreprocessOddRow(int y)
            {
                // Odd rows are preprocessed so we don't need to complicate the IDitheringSession interface with processing path.
                // It is not even a significant overhead unless not every pixel is queried (e.g. DrawInto with clipped region).
                IReadableBitmapDataRow row = source[y];

                // processing odd rows from right to left
                for (int x = ImageWidth - 1; x >= 0; x--)
                    preprocessedResults[x] = DoErrorDiffusion(row[x], x, y);
            }

            #endregion

            #endregion
        }

        #endregion
    }
}
