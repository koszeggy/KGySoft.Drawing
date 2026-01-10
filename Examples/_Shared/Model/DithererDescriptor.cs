#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: DithererDescriptor.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2025 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

#nullable enable

#region Usings

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

using KGySoft.Drawing.Examples.Shared.Interfaces;
using KGySoft.Drawing.Imaging;
using KGySoft.Reflection;

#endregion

namespace KGySoft.Drawing.Examples.Shared.Model
{
    public class DithererDescriptor
    {
        #region Fields

        #region Static Fields

        internal static readonly DithererDescriptor[] Ditherers =
        {
            new("Bayer 2x2 (Ordered)", typeof(OrderedDitherer), nameof(OrderedDitherer.Bayer2x2)),
            new("Bayer 4x4 (Ordered)", typeof(OrderedDitherer), nameof(OrderedDitherer.Bayer4x4)),
            new("Bayer 8x8 (Ordered)", typeof(OrderedDitherer), nameof(OrderedDitherer.Bayer8x8)),
            new("Dotted Halftone (Ordered)", typeof(OrderedDitherer), nameof(OrderedDitherer.DottedHalftone)),
            new("Blue Noise (Ordered)", typeof(OrderedDitherer), nameof(OrderedDitherer.BlueNoise)),

            new("Atkinson (Error Diffusion)", typeof(ErrorDiffusionDitherer), nameof(ErrorDiffusionDitherer.Atkinson)),
            new("Floyd-Steinberg (Error Diffusion)", typeof(ErrorDiffusionDitherer), nameof(ErrorDiffusionDitherer.FloydSteinberg)),
            new("Sierra (Error Diffusion)", typeof(ErrorDiffusionDitherer), nameof(ErrorDiffusionDitherer.Sierra3)),

            new("Random White Noise", typeof(RandomNoiseDitherer).GetConstructor(new[] { typeof(float), typeof(int?) })!),
            new("Interleaved Gradient Noise", typeof(InterleavedGradientNoiseDitherer).GetConstructor(new[] { typeof(float) })!),
        };

        #endregion

        #region Instance Fields

        private readonly string displayName;
        private readonly CreateInstanceAccessor? ctor;
        private readonly ParameterInfo[]? parameters;
        private readonly PropertyAccessor? property;

        #endregion

        #endregion

        #region Properties

        public bool HasStrength { get; }
        public bool HasSeed { get; }
        public bool HasSerpentineProcessing { get; }
        public bool HasByBrightness { get; }

        #endregion

        #region Constructors

        private DithererDescriptor(string name, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]Type type, string propertyName)
            : this(name, type.GetProperty(propertyName)!)
        {
        }

        private DithererDescriptor(string name, MemberInfo member)
        {
            displayName = name;
            switch (member)
            {
                case ConstructorInfo ci:
                    parameters = ci.GetParameters();
                    ctor = CreateInstanceAccessor.GetAccessor(ci);
                    HasStrength = parameters.Any(p => p.Name == "strength");
                    HasSeed = parameters.Any(p => p.Name == "seed");
                    break;

                case PropertyInfo pi:
                    property = PropertyAccessor.GetAccessor(pi);
                    HasStrength = pi.DeclaringType == typeof(OrderedDitherer);
                    HasSerpentineProcessing = HasByBrightness = pi.DeclaringType == typeof(ErrorDiffusionDitherer);
                    break;

                default:
                    throw new ArgumentException($"Unexpected member: {member}");
            }
        }

        #endregion

        #region Methods

        #region Public Methods

        public override string ToString() => displayName;

        #endregion

        #region Internal Methods

        internal IDitherer Create(IDithererSettings settings)
        {
            IDitherer result;
            if (ctor != null)
            {
                object?[] args = new object[parameters!.Length];
                for (int i = 0; i < parameters.Length; i++)
                {
                    args[i] = parameters[i].Name switch
                    {
                        "strength" => settings.Strength,
                        "seed" => settings.Seed,
                        _ => throw new InvalidOperationException($"Unhandled parameter: {parameters[i].Name}")
                    };
                }

                result = (IDitherer)ctor.CreateInstance(args);
            }
            else
            {
                result = property!.GetStaticValue<IDitherer>();
                result = result switch
                {
                    OrderedDitherer ordered => ordered.ConfigureStrength(settings.Strength),
                    ErrorDiffusionDitherer errorDiffusion => errorDiffusion.ConfigureErrorDiffusionMode(settings.ByBrightness)
                        .ConfigureProcessingDirection(settings.DoSerpentineProcessing),
                    _ => result
                };
            }

            return result;
        }

        #endregion

        #endregion
    }
}
