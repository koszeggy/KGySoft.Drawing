#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: OctreeQuantizer.cs
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
using System.Diagnostics;
using System.Drawing;

#endregion

namespace KGySoft.Drawing.Imaging
{
    public sealed class OctreeQuantizer : IQuantizer
    {
        #region Nested classes

        #region OctreeQuantizerSession class

        private sealed class OctreeQuantizerSession : IQuantizingSession
        {
            #region Nested classes

            #region Octree class

            private sealed class Octree
            {
                #region Fields

                private readonly List<OctreeNode>[] levels;
                private readonly OctreeNode root;

                private int leavesCount;

                #endregion

                #region Properties

                internal int MaxColors { get; }

                internal int Bpp { get; }

                internal int ColorCount => leavesCount + (HasTransparent ? 1 : 0);

                internal bool HasTransparent { get; private set; }

                internal int Size { get; }

                #endregion

                #region Constructors

                internal Octree(int maxColors, int size)
                {
                    MaxColors = maxColors;
                    Size = size;

                    // bits per pixel is actually ceiling of log2(maxColors)
                    for (int n = maxColors - 1; n > 0; n >>= 1)
                        Bpp++;

                    levels = new List<OctreeNode>[Bpp];
                    for (int level = 0; level < Bpp; level++)
                        levels[level] = new List<OctreeNode>();

                    root = new OctreeNode(-1, this);
                }

                #endregion

                #region Methods

                #region Internal Methods

                internal void AddColor(Color32 color)
                {
                    if (color.A == 0)
                        HasTransparent = true;
                    else if (root.AddColor(color, 0))
                        leavesCount++;
                }

                internal void AddToLevel(OctreeNode octreeNode, int level) => levels[level].Add(octreeNode);

                internal Color32[] GeneratePalette()
                {
                    if (ColorCount > MaxColors)
                        ReduceTree();

                    Debug.Assert(ColorCount <= MaxColors);

                    var result = new Color32[ColorCount];
                    if (leavesCount > 0)
                    {
                        int palIndex = 0;
                        root.PopulatePalette(result, ref palIndex, ref leavesCount);
                        Debug.Assert(leavesCount == 0);
                    }

                    // If transparent color is needed, then it will be automatically the last color in the result
                    return result;
                }

                #endregion

                #region Private Methods

                private void ReduceTree()
                {
                    // Scanning all levels towards root. Leaves are skipped (hence -2) because they are not reducible.
                    for (int level = Bpp - 2; level >= 0; level--)
                    {
                        if (levels[level].Count <= 0)
                            continue;

                        // Sorting nodes of the current level (least significant ones first)
                        // while merging them into their parents until we go under MaxColors
                        List<OctreeNode> nodes = levels[level];
                        nodes.Sort((a, b) => a.DeepPixelCount - b.DeepPixelCount);

                        foreach (OctreeNode node in nodes)
                        {
                            // As merging is stopped when we reach MaxColors leavesCount may include
                            // some half-merged non-leaf nodes as well.
                            node.MergeNodes(ref leavesCount);
                            if (ColorCount <= MaxColors)
                                return;
                        }
                    }

                    // If we are here, we need to reduce also the root node (less than 8 colors or 8 colors + transparency)
                    root.MergeNodes(ref leavesCount);
                    Debug.Assert(ColorCount == MaxColors);
                }

                #endregion

                #endregion
            }

            #endregion

            #region OctreeNode class

            private sealed class OctreeNode
            {
                #region Fields

                private readonly Octree parent;

                private uint sumRed;
                private uint sumGreen;
                private uint sumBlue;
                private int pixelCount;
                private OctreeNode[] children;

                #endregion

                #region Properties

                #region Internal Properties

                internal int DeepPixelCount
                {
                    get
                    {
                        int result = pixelCount;
                        if (children == null)
                            return result;

                        // Adding also the direct children because reducing the tree starts at level BPP - 2.
                        // And due to reducing no more than two levels can have non-empty nodes.
                        for (int index = 0; index < 8; index++)
                        {
                            OctreeNode node = children[index];
                            if (node != null)
                                result += node.pixelCount;
                        }

                        return result;
                    }
                }

                #endregion

                #region Private Properties

                private bool IsEmpty => pixelCount == 0;

                #endregion

                #endregion

                #region Constructors

                internal OctreeNode(int level, Octree parent)
                {
                    this.parent = parent;
                    Debug.Assert(level < parent.Bpp);

                    if (level >= 0)
                        parent.AddToLevel(this, level);
                }

                #endregion

                #region Methods

                #region Internal Methods

                internal bool AddColor(Color32 color, int level)
                {
                    // In the populating phase all colors are summed up in leaves at deepest level.
                    if (level == parent.Bpp)
                    {
                        sumRed += color.R;
                        sumGreen += color.G;
                        sumBlue += color.B;
                        pixelCount++;

                        // returning whether a new leaf has been added
                        return pixelCount == 1;
                    }

                    Debug.Assert(level < parent.Bpp);

                    if (children == null)
                        children = new OctreeNode[8];

                    // Generating a 0..7 index based on the color components and adding new branches on demand.
                    int mask = 128 >> level;
                    int branchIndex = ((color.R & mask) == mask ? 4 : 0)
                        | ((color.G & mask) == mask ? 2 : 0)
                        | ((color.B & mask) == mask ? 1 : 0);

                    if (children[branchIndex] == null)
                        children[branchIndex] = new OctreeNode(level, parent);
                    return children[branchIndex].AddColor(color, level + 1);
                }

                internal void MergeNodes(ref int leavesCount)
                {
                    #region Local Methods

                    static int CompareByBrightness(OctreeNode a, OctreeNode b)
                    {
                        if (a == null || b == null)
                            return Int32.MinValue;

                        Color32 ca = a.ToColor();
                        Color32 cb = b.ToColor();
                        return ca.GetBrightness() - cb.GetBrightness();
                    }

                    int CompareByWeightedBrightness(OctreeNode a, OctreeNode b)
                    {
                        if (a == null || b == null)
                            return Int32.MinValue;

                        Color32 ca = a.ToColor();
                        Color32 cb = b.ToColor();
                        return (int)(ca.GetBrightness() * (a.DeepPixelCount / (float)parent.Size) - cb.GetBrightness() * (b.DeepPixelCount / (float)parent.Size));
                    }

                    #endregion

                    if (children == null)
                        return;

                    // If there are fewer than 8 removals left we sort them to merge the least relevant ones first.
                    // For 2 colors (and 3 + TR) the distance is measured purely by brightness to avoid returning very similar colors.
                    // Note: reordering children is not a problem because we don't add more colors in merging phase.
                    if (parent.ColorCount - parent.MaxColors < 8)
                        Array.Sort(children, parent.MaxColors - (parent.HasTransparent ? 1 : 0) <= 2
                            ? (Comparison<OctreeNode>)CompareByBrightness
                            : CompareByWeightedBrightness);

                    for (int i = 0; i < 8; i++)
                    {
                        OctreeNode node = children[i];
                        if (node == null)
                            continue;

                        Debug.Assert(!node.IsEmpty);

                        // Decreasing only if this node is not becoming a "leaf" while cutting a branch down.
                        if (!IsEmpty)
                            leavesCount--;

                        sumRed += node.sumRed;
                        sumGreen += node.sumGreen;
                        sumBlue += node.sumBlue;
                        pixelCount += node.pixelCount;

                        children[i] = null;

                        // As we can return before merging all children,
                        // leavesCount may include "not-quite leaf" elements in the end.
                        if (parent.ColorCount == parent.MaxColors)
                            return;
                    }
                }

                internal void PopulatePalette(Color32[] result, ref int palIndex, ref int remainingColors)
                {
                    // if a non-empty node is found, adding it to the resulting palette
                    if (!IsEmpty)
                    {
                        result[palIndex] = ToColor();
                        palIndex += 1;
                        remainingColors -= 1;
                        if (remainingColors == 0)
                            return;
                    }

                    if (children == null)
                        return;

                    foreach (OctreeNode child in children)
                    {
                        if (child == null)
                            continue;
                        child.PopulatePalette(result, ref palIndex, ref remainingColors);
                        if (remainingColors == 0)
                            return;
                    }
                }

                #endregion

                #region Private Methods

                private Color32 ToColor()
                {
                    Debug.Assert(!IsEmpty);
                    return pixelCount == 1
                        ? new Color32((byte)sumRed, (byte)sumGreen, (byte)sumBlue)
                        : new Color32((byte)(sumRed / pixelCount), (byte)(sumGreen / pixelCount), (byte)(sumBlue / pixelCount));
                }

                #endregion

                #endregion
            }

            #endregion

            #endregion

            #region Fields

            private readonly OctreeQuantizer quantizer;
            private readonly Palette palette;

            #endregion

            #region Properties

            public Color32[] Palette => palette.Entries;

            public bool SupportsPositionalResult => false;

            #endregion

            #region Constructors

            internal OctreeQuantizerSession(OctreeQuantizer quantizer, IBitmapDataAccessor source)
            {
                this.quantizer = quantizer;
                palette = InitializePalette(source);
            }

            #endregion

            #region Methods

            #region Public Methods

            public void Dispose()
            {
            }

            public Color32 GetQuantizedColor(Color32 origColor) => palette.GetNearestColor(origColor);

            public Color32 GetQuantizedColorByPosition(int x, int y) => throw new NotSupportedException(PublicResources.NotSupported);

            #endregion

            #region Private Methods

            private Palette InitializePalette(IBitmapDataAccessor source)
            {
                Octree octree = new Octree(quantizer.maxColors, source.Width * source.Height);
                int width = source.Width;
                IBitmapDataRow row = source.FirstRow;
                do
                {
                    // TODO: parallel if possible
                    for (int x = 0; x < width; x++)
                    {
                        Color32 c = row[x];

                        // handling alpha including full transparency
                        if (c.A != Byte.MaxValue)
                            c = c.A < quantizer.alphaThreshold ? default : c.BlendWithBackground(quantizer.backColor);
                        octree.AddColor(c);
                    }
                } while (row.MoveNextRow());

                return new Palette(octree.GeneratePalette())
                {
                    AlphaThreshold = quantizer.alphaThreshold,
                    BackColor = quantizer.backColor
                };
            }

            #endregion

            #endregion
        }

        #endregion

        #endregion

        #region Fields

        private readonly int maxColors;
        private readonly Color32 backColor;
        private readonly byte alphaThreshold;

        #endregion

        #region Constructors

        public OctreeQuantizer(int maxColors = 256, Color backColor = default, byte alphaThreshold = 128)
        {
            if (maxColors < 2 || maxColors > 256)
                throw new ArgumentOutOfRangeException(nameof(maxColors), PublicResources.ArgumentMustBeBetween(2, 256));
            this.maxColors = maxColors;
            this.backColor = new Color32(backColor);
            this.alphaThreshold = alphaThreshold;
        }

        #endregion

        #region Methods

        IQuantizingSession IQuantizer.Initialize(IBitmapDataAccessor source) => new OctreeQuantizerSession(this, source);

        #endregion
    }
}
