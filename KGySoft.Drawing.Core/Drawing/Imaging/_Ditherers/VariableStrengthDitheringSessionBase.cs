#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: VariableStrengthDitheringSessionBase.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2024 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System;

#endregion

namespace KGySoft.Drawing.Imaging
{
    internal abstract class VariableStrengthDitheringSessionBase : IDitheringSession
    {
        #region Properties

        #region Public Properties

        public abstract bool IsSequential { get; }

        #endregion

        #region Protected Properties

        protected IQuantizingSession QuantizingSession { get; }

        protected float Strength { get; set; }

        #endregion

        #endregion

        #region Constructors

        protected VariableStrengthDitheringSessionBase(IQuantizingSession quantizingSession)
        {
            QuantizingSession = quantizingSession ?? throw new ArgumentNullException(nameof(quantizingSession), PublicResources.ArgumentNull);
        }

        #endregion

        #region Methods

        #region Public Methods
        
        public abstract Color32 GetDitheredColor(Color32 origColor, int x, int y);

        public void Dispose() => Dispose(true);

        #endregion

        #region Protected Methods

        protected virtual void Dispose(bool disposing)
        {
        }

        #endregion

        #endregion
    }
}
