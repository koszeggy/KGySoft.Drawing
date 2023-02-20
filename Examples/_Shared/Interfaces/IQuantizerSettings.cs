#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: IQuantizerSettings.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2022 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System.Drawing;

using KGySoft.Drawing.Imaging;

#endregion

namespace KGySoft.Drawing.Examples.Shared.Interfaces
{
    public interface IQuantizerSettings
    {
        #region Properties

        Color BackColor { get; }
        byte AlphaThreshold { get; }
        byte WhiteThreshold { get; }
        bool DirectMapping { get; }
        int PaletteSize { get; }
        byte? BitLevel { get; }
        WorkingColorSpace WorkingColorSpace { get; }

        #endregion
    }
}