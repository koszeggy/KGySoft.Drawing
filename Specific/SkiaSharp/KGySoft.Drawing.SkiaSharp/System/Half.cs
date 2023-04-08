// This file is the stripped version of .NET System.Half from here: https://github.com/dotnet/runtime/blob/main/src/libraries/System.Private.CoreLib/src/System/Half.cs
// The original file is under the MIT license.
// The MIT license is available here: https://github.com/dotnet/runtime/blob/main/LICENSE.TXT

#if !NET5_0_OR_GREATER
#region Usings

using System.Numerics;
using System.Runtime.InteropServices;

#endregion

// ReSharper disable once CheckNamespace
namespace System
{
    [StructLayout(LayoutKind.Sequential)]
    internal readonly struct Half
    {
        #region Constants

        private const ushort positiveInfinityBits = 0x7C00; 
        private const ushort negativeInfinityBits = 0xFC00;
        private const ushort trailingSignificandMask = 0x03FF;
        private const byte shiftedBiasedExponentMask = biasedExponentMask >> biasedExponentShift;
        private const byte maxBiasedExponent = 0x1F;

        private const int signShift = 15;
        private const ushort biasedExponentMask = 0x7C00;
        private const int biasedExponentShift = 10;

        private const uint singleSignMask = 0x8000_0000;
        private const int singleSignShift = 31;
        private const uint singleBiasedExponentMask = 0x7F80_0000;
        private const int singleBiasedExponentShift = 23;
        private const uint singleTrailingSignificandMask = 0x007F_FFFF;

        #endregion

        #region Fields

        private readonly ushort value;

        #endregion

        #region Properties

        #region Static Properties
        
        private static Half NegativeInfinity => new Half(negativeInfinityBits);      // -1.0 / 0.0
        private static Half PositiveInfinity => new Half(positiveInfinityBits);      //  1.0 / 0.0;

        #endregion

        #region Instance Properties

        private byte BiasedExponent => ExtractBiasedExponentFromBits(value);
        internal ushort TrailingSignificand => ExtractTrailingSignificandFromBits(value);

        #endregion

        #endregion

        #region Operators

        public static unsafe explicit operator Half(float value)
        {
            const int singleMaxExponent = 0xFF;

            uint floatInt = *((uint*)&value);
            bool sign = (floatInt & singleSignMask) >> singleSignShift != 0;
            int exp = (int)(floatInt & singleBiasedExponentMask) >> singleBiasedExponentShift;
            uint sig = floatInt & singleTrailingSignificandMask;

            if (exp == singleMaxExponent)
            {
                if (sig != 0) // NaN
                {
                    return CreateHalfNaN(sign, (ulong)sig << 41); // Shift the significand bits to the left end
                }
                return sign ? NegativeInfinity : PositiveInfinity;
            }

            uint sigHalf = sig >> 9 | ((sig & 0x1FFU) != 0 ? 1U : 0U); // RightShiftJam

            if ((exp | (int)sigHalf) == 0)
            {
                return new Half(sign, 0, 0);
            }

            return new Half(RoundPackToHalf(sign, (short)(exp - 0x71), (ushort)(sigHalf | 0x4000)));
        }

        public static explicit operator float(Half value)
        {
            bool sign = IsNegative(value);
            int exp = value.BiasedExponent;
            uint sig = value.TrailingSignificand;

            if (exp == maxBiasedExponent)
            {
                if (sig != 0)
                {
                    return CreateSingleNaN(sign, (ulong)sig << 54);
                }
                return sign ? float.NegativeInfinity : float.PositiveInfinity;
            }

            if (exp == 0)
            {
                if (sig == 0)
                {
                    return sign ? -0f : 0f; // Positive / Negative zero
                }
                (exp, sig) = NormSubnormalF16Sig(sig);
                exp -= 1;
            }

            return CreateSingle(sign, (byte)(exp + 0x70), sig << 13);
        }

        #endregion

        #region Constructors

        private Half(ushort value) => this.value = value;

        private Half(bool sign, ushort exp, ushort sig)
            => value = (ushort)(((sign ? 1 : 0) << signShift) + (exp << biasedExponentShift) + sig);

        #endregion

        #region Methods

        private static bool IsNegative(Half value) => (short)value.value < 0;

        private static Half CreateHalfNaN(bool sign, ulong significand)
        {
            const uint naNBits = biasedExponentMask | 0x200; // Most significant significand bit

            uint signInt = (sign ? 1U : 0U) << signShift;
            uint sigInt = (uint)(significand >> 54);

            return new Half((ushort)(signInt | naNBits | sigInt));
        }

        private static ushort RoundPackToHalf(bool sign, short exp, ushort sig)
        {
            const int RoundIncrement = 0x8; // Depends on rounding mode but it's always towards closest / ties to even
            int roundBits = sig & 0xF;

            if ((uint)exp >= 0x1D)
            {
                if (exp < 0)
                {
                    sig = (ushort)ShiftRightJam(sig, -exp);
                    exp = 0;
                    roundBits = sig & 0xF;
                }
                else if (exp > 0x1D || sig + RoundIncrement >= 0x8000) // Overflow
                {
                    return sign ? negativeInfinityBits : positiveInfinityBits;
                }
            }

            sig = (ushort)((sig + RoundIncrement) >> 4);
            sig &= (ushort)~(((roundBits ^ 8) != 0 ? 0 : 1) & 1);

            if (sig == 0)
            {
                exp = 0;
            }

            return new Half(sign, (ushort)exp, sig).value;
        }

        private static unsafe float CreateSingleNaN(bool sign, ulong significand)
        {
            const uint naNBits = singleBiasedExponentMask | 0x400000; // Most significant significand bit

            uint signInt = (sign ? 1U : 0U) << singleSignShift;
            uint sigInt = (uint)(significand >> 41);

            uint result = signInt | naNBits | sigInt;
            return *(float*)&result;
        }

        private static uint ShiftRightJam(uint i, int dist) => dist < 31 ? (i >> dist) | (i << (-dist & 31) != 0 ? 1U : 0U) : (i != 0 ? 1U : 0U);
        internal static byte ExtractBiasedExponentFromBits(ushort bits) => (byte)((bits >> biasedExponentShift) & shiftedBiasedExponentMask);
        private static ushort ExtractTrailingSignificandFromBits(ushort bits) => (ushort)(bits & trailingSignificandMask);

        private static (int Exp, uint Sig) NormSubnormalF16Sig(uint sig)
        {
            int shiftDist = LeadingZeroCount(sig) - 16 - 5;
            return (1 - shiftDist, sig << shiftDist);
        }

        private static int LeadingZeroCount(uint value)
        {
#if NETCOREAPP3_0_OR_GREATER
            return BitOperations.LeadingZeroCount(value);
#else
            // Filling trailing zeros with ones, eg 00010010 becomes 00011111
            value |= value >> 1;
            value |= value >> 2;
            value |= value >> 4;
            value |= value >> 8;
            value |= value >> 16;
            
            // pop count
            value -= value >> 1 & 0x55555555;
            value = (value >> 2 & 0x33333333) + (value & 0x33333333);
            value = (value >> 4) + value & 0x0f0f0f0f;
            value += value >> 8;
            value += value >> 16;

            return 32 - (int)(value & 0x0000003f);
#endif
        }

        private static unsafe float CreateSingle(bool sign, byte exp, uint sig)
        {
            uint result = ((sign ? 1U : 0U) << singleSignShift) + ((uint)exp << singleBiasedExponentShift) + sig;
            return *(float*)&result;
        }

        #endregion
    }
}

#endif