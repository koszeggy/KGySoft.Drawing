namespace KGySoft.Drawing.Shapes
{
    /// <summary>
    /// Specifies how to the interior of a path is filled when it contains intersecting edges.
    /// If a path has no intersecting edges, then both strategies produce the same result.
    /// </summary>
    public enum ShapeFillMode
    {
        /// <summary>
        /// Specifies the alternate fill mode. If a scanline of the region to fill crosses an odd number of path segments,
        /// the starting point is inside the closed region and is therefore part of the fill area. An even number of crossings means
        /// that the point is not in an area to be filled. This strategy is faster than the <see cref="NonZero"/> mode, though
        /// it may produce "holes" when a polygon has self-crossing lines.
        /// </summary>
        Alternate,

        /// <summary>
        /// Specifies the nonzero fill mode. It considers the direction of the path segments at each intersection.
        /// It adds one for every clockwise intersection, and subtracts one for every counterclockwise intersection.
        /// If the result is nonzero, the point is considered inside the fill area. A zero count means that the point lies outside the fill area.
        /// This strategy is slower than the <see cref="Alternate"/> mode, though it makes a difference in the result only
        /// when the path to fill has intersections.
        /// </summary>
        NonZero
    }
}
