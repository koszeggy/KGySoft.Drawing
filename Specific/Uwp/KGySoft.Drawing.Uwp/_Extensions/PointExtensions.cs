#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: PointExtensions.cs
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

using System.Drawing;

#endregion

#region Used Aliases

using UwpPoint = Windows.Foundation.Point;

#endregion

#endregion

namespace KGySoft.Drawing.Uwp
{
    internal static class PointExtensions
    {
        #region Methods

        internal static PointF ToPointF(this UwpPoint point) => new PointF((float)point.X, (float)point.Y);

        #endregion
    }
}