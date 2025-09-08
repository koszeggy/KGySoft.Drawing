#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: WpfGfx.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2025 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

#region Used Namespaces

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows.Media;

using KGySoft.Reflection;

#endregion

#region Used Aliases

using WpfPoint = System.Windows.Point;

#endregion

#endregion

namespace KGySoft.Drawing.Wpf.WinApi
{
    [SecuritySafeCritical]
    internal static class WpfGfx
    {
        #region NativeMethods class

        [SecurityCritical]
        private static class NativeMethods
        {
            #region Constants

#if NET35
            private const string libName = "wpfgfx_v0300.dll";
#elif NETFRAMEWORK
            private const string libName = "wpfgfx_v0400.dll";
#else
            private const string libName = "wpfgfx_cor3.dll";
#endif

            #endregion

            #region Methods

            [DllImport(libName)]
            internal static extern unsafe void MilUtility_ArcToBezier(
                WpfPoint ptStart,
                System.Windows.Size rRadii,
                double rRotation,
                bool fLargeArc,
                SweepDirection fSweepUp,
                WpfPoint ptEnd,
                void* pMatrix,
                WpfPoint* pPt,
                out int cPieces);

            #endregion
        }

        #endregion

        #region Methods

        internal static unsafe IList<PointF>? ToBezierPoints(this ArcSegment arcSegment, WpfPoint startPoint)
        {
            WpfPoint* bezierPoints = stackalloc WpfPoint[12];
            int pieceCount;
            try
            {
                NativeMethods.MilUtility_ArcToBezier(startPoint, arcSegment.Size, arcSegment.RotationAngle, arcSegment.IsLargeArc, arcSegment.SweepDirection, arcSegment.Point, null, bezierPoints, out pieceCount);
            }
            catch (Exception e) when (!e.IsCritical())
            {
                return null;
            }

            Debug.Assert(pieceCount <= 4);
            pieceCount = Math.Min(pieceCount, 4);
            if (pieceCount <= 0)
                return pieceCount == 0 ? Reflector.EmptyArray<PointF>() : null;

            var result = new List<PointF>(3 * pieceCount + 1) { startPoint.ToPointF() };
            for (int index = 0; index < pieceCount; ++index)
                result.AddRange([bezierPoints[3 * index].ToPointF(), bezierPoints[3 * index + 1].ToPointF(), bezierPoints[3 * index + 2].ToPointF()]);
            return result;
        }

        #endregion
    }
}
