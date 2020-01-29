#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: Palette.cs
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;

#endregion

namespace KGySoft.Drawing.Imaging
{
    public class Palette
    {
        #region Fields

        private readonly int transparentIndex = -1;
        private readonly Dictionary<Color32, int> color32ToIndex;
        private readonly Func<Color32, int> customGetNearestColorIndex;

        #endregion

        #region Properties

        #region Public Properties
        
        public int Count => Entries.Length;
        public byte AlphaThreshold { get; }
        public Color32 BackColor { get; }

        #endregion

        #region Internal Properties

        internal Color32[] Entries { get; }
        internal bool IsGrayscale { get; }

        #endregion

        #endregion

        #region Constructors

        public Palette(Color32[] entries, Color32 backColor = default, byte alphaThreshold = 128, Func<Color32, int> customGetNearestColorIndex = null)
        {
            Entries = entries ?? throw new ArgumentNullException(nameof(entries), PublicResources.ArgumentNull);
            BackColor = Color32.FromArgb(Byte.MaxValue, backColor);
            AlphaThreshold = alphaThreshold;
            this.customGetNearestColorIndex = customGetNearestColorIndex;
            color32ToIndex = new Dictionary<Color32, int>(entries.Length);
            IsGrayscale = true;

            for (int i = 0; i < entries.Length; i++)
            {
                Color32 c = entries[i];
                if (!color32ToIndex.ContainsKey(c) && !(AlphaThreshold == 0 && c.A == 0))
                    color32ToIndex[c] = i;

                if (c.A == 0)
                {
                    if (transparentIndex < 0)
                        transparentIndex = i;
                }

                if (IsGrayscale)
                    IsGrayscale = c.R == c.G && c.R == c.B;
            }
        }

        public Palette(Color[] entries, Color backColor = default, byte alphaThreshold = 128, Func<Color32, int> customGetNearestColorIndex = null)
            : this(entries?.Select(c => new Color32(c)).ToArray(), new Color32(backColor), alphaThreshold, customGetNearestColorIndex)
        {
        }

        #endregion

        #region Methods

        #region Public Methods

        public Color32 GetColor(int index)
        {
            if ((uint)index >= (uint)Entries.Length)
                ThrowIndexInvalid();
            return Entries[index];
        }

        public int GetColorIndex(Color32 c)
            => color32ToIndex.TryGetValue(c, out int result)
                ? result
                : customGetNearestColorIndex?.Invoke(c) ?? FindNearestColorIndex(c);

        public Color32 GetNearestColor(Color32 c) => Entries[GetColorIndex(c)];

        // Could be IReadOnlyCollection but that is not available in .NET 3.5/4.0
        public IList<Color32> GetEntries() => new ReadOnlyCollection<Color32>(Entries);

        #endregion

        #region Private Methods

        private int FindNearestColorIndex(Color32 color)
        {
            // mapping alpha to full transparency
            if (color.A < AlphaThreshold && transparentIndex != -1)
                return transparentIndex;

            int minDiff = Int32.MaxValue;
            int resultIndex = 0;

            // blending the color with background and checking if there is an exact match now
            if (color.A != Byte.MaxValue)
            {
                color = color.BlendWithBackground(BackColor);
                if (color32ToIndex.TryGetValue(color, out resultIndex))
                    return resultIndex;
            }

            for (int i = 0; i < Entries.Length; i++)
            {
                Color32 current = Entries[i];

                // Palette color with alpha
                if (current.A != Byte.MaxValue)
                {
                    // Skipping fully transparent palette colors because they were handled above
                    if (current.A == 0)
                        continue;

                    // Blending also the current palette color
                    current = current.BlendWithBackground(BackColor);

                    // Exact match. Since the cache was checked before calling this method this can occur only after alpha blending.
                    if (current == color)
                        return i;
                }

                // If the palette is grayscale, then distance is measured by perceived brightness;
                // otherwise, by an Euclidean-like but much faster distance based on RGB components.
                int diff = IsGrayscale
                    ? Math.Abs(Entries[i].GetBrightness() - color.GetBrightness())
                    : Math.Abs(current.R - color.R) + Math.Abs(current.G - color.G) + Math.Abs(current.B - color.B);

                Debug.Assert(IsGrayscale || diff != 0, "Exact match should have been returned earlier");

                // new closest match
                if (diff < minDiff)
                {
                    minDiff = diff;
                    resultIndex = i;
                }
            }

            return resultIndex;
        }

        private void ThrowIndexInvalid()
        {
#pragma warning disable CA2208
            // ReSharper disable once NotResolvedInText
            throw new ArgumentOutOfRangeException("index", PublicResources.ArgumentMustBeBetween(0, Entries.Length - 1));
#pragma warning restore CA2208
        }

        #endregion

        #endregion
    }
}
