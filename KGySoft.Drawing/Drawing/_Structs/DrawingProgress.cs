#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: DrawingProgress.cs
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
using System.Diagnostics.CodeAnalysis;
using KGySoft.CoreLibraries;

#endregion

namespace KGySoft.Drawing
{
    /// <summary>
    /// Represents the progress of a drawing operation.
    /// </summary>
    public readonly struct DrawingProgress : IEquatable<DrawingProgress>
    {
        #region Properties

        /// <summary>
        /// Gets the type of the drawing operation.
        /// </summary>
        public DrawingOperation OperationType { get; }

        /// <summary>
        /// Gets the maximum steps of this operation.
        /// </summary>
        public int MaximumValue { get; }

        /// <summary>
        /// Gets the current step of this operation. Its value is between zero and <see cref="MaximumValue"/>, inclusive bounds.
        /// </summary>
        public int CurrentValue { get; }

        #endregion

        #region Operators

        /// <summary>
        /// Gets whether two <see cref="DrawingProgress"/> structures are equal.
        /// </summary>
        /// <param name="left">The <see cref="DrawingProgress"/> instance that is to the left of the equality operator.</param>
        /// <param name="right">The <see cref="DrawingProgress"/> instance that is to the right of the equality operator.</param>
        /// <returns><see langword="true"/>&#160;if the two <see cref="DrawingProgress"/> structures are equal; otherwise, <see langword="false"/>.</returns>
        public static bool operator ==(DrawingProgress left, DrawingProgress right) => left.Equals(right);

        /// <summary>
        /// Gets whether two <see cref="DrawingProgress"/> structures are different.
        /// </summary>
        /// <param name="left">The <see cref="DrawingProgress"/> instance that is to the left of the inequality operator.</param>
        /// <param name="right">The <see cref="DrawingProgress"/> instance that is to the right of the inequality operator.</param>
        /// <returns><see langword="true"/>&#160;if the two <see cref="DrawingProgress"/> structures are different; otherwise, <see langword="false"/>.</returns>
        public static bool operator !=(DrawingProgress left, DrawingProgress right) => !(left == right);

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DrawingProgress"/> struct.
        /// </summary>
        /// <param name="operationType">Specifies the type of the drawing operation.</param>
        /// <param name="maximumValue">The maximum value.</param>
        /// <param name="currentValue">The current value.</param>
        public DrawingProgress(DrawingOperation operationType, int maximumValue, int currentValue)
        {
            if (!operationType.IsDefined())
                throw new ArgumentOutOfRangeException(nameof(operationType), PublicResources.EnumOutOfRange(operationType));
            if (maximumValue < 0)
                throw new ArgumentOutOfRangeException(nameof(maximumValue), PublicResources.ArgumentMustBeGreaterThanOrEqualTo(0));
            if ((uint)currentValue > (uint)maximumValue)
                throw new ArgumentOutOfRangeException(nameof(currentValue), PublicResources.ArgumentMustBeBetween(0, maximumValue));

            OperationType = operationType;
            MaximumValue = maximumValue;
            CurrentValue = currentValue;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Determines whether the specified <see cref="object"/> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="object" /> to compare with this instance.</param>
        /// <returns><see langword="true"/>&#160;if the specified <see cref="object" /> is equal to this instance; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj) => obj is DrawingProgress drawingProgress && Equals(drawingProgress);

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode", Justification = "A value type cannot be changed inside a hashed collection")]
        public override int GetHashCode() => ((int)OperationType, MaximumValue, CurrentValue).GetHashCode();

        /// <summary>
        /// Indicates whether the this <see cref="DrawingProgress"/> is equal to another one.
        /// </summary>
        /// <param name="other">A <see cref="DrawingProgress"/> instance to compare with this one.</param>
        /// <returns><see langword="true"/>&#160;if the current object is equal to the <paramref name="other"/>&#160;<see cref="DrawingProgress"/>; otherwise, <see langword="false"/>.</returns>
        public bool Equals(DrawingProgress other)
            => OperationType == other.OperationType && MaximumValue == other.MaximumValue && CurrentValue == other.CurrentValue;

        #endregion
    }
}
