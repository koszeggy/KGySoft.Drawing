#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: AnimatedGifConfiguration.cs
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

using System;
using System.Collections.Generic;
using System.Drawing;

#endregion

#region Suppressions

#if NET35
#pragma warning disable CS1574 // XML comment has cref attribute that could not be resolved - in .NET 3.5 not all members are available
#endif

#endregion

namespace KGySoft.Drawing.Imaging
{
    /// <summary>
    /// Represents the configuration for encoding a GIF animation by the <see cref="GifEncoder.EncodeAnimation">GifEncoder.EncodeAnimation</see> method.
    /// </summary>
    public sealed class AnimatedGifConfiguration
    {
        #region Fields

        private static readonly TimeSpan[] defaultDelays = { new TimeSpan(100 * TimeSpan.TicksPerMillisecond) };

        #endregion

        #region Properties

        #region Public Properties

        /// <summary>
        /// Gets or sets the desired size of the result animation. If <see langword="null"/>, then size is determined by the first frame.
        /// If set explicitly or the input frames can have different sizes, then <see cref="SizeHandling"/> should also be set accordingly.
        /// <br/>Default value: <see langword="null"/>.
        /// </summary>
        public Size? Size { get; set; }

        /// <summary>
        /// Gets or sets how possibly different input frame sizes are handled.
        /// <br/>Default value: <see cref="AnimationFramesSizeHandling.ErrorIfDiffers"/>.
        /// </summary>
        public AnimationFramesSizeHandling SizeHandling { get; set; }

        /// <summary>
        /// Gets or sets whether zero delay values are allowed in the result stream,
        /// which is usually interpreted as 100 ms by most GIF decoders.
        /// <br/>Default value: <see langword="true"/>.
        /// </summary>
        /// <value>
        /// If <see langword="true"/>, then zero delay values will be replaced to 100 milliseconds.
        /// <br/>If <see langword="false"/>, then zero delays will be preserved and the decoders decide how to interpret them.
        /// </value>
        public bool ReplaceZeroDelays { get; set; } = true;

        /// <summary>
        /// Gets or sets an optional quantizer to be used for the frames. 
        /// Using a non-<see langword="null"/> value forces quantizing also the already indexed images.
        /// Should use up to 256 colors; otherwise, the result might be quantized further with using the default system 8-bit palette.
        /// <br/>Default value: <see langword="null"/>.
        /// </summary>
        /// <value>
        /// If <see langword="null"/>, then the possibly existing palette of already indexed input frames are preserved.
        /// For frames with a non-indexed pixel format a quantizer returned by the <see cref="OptimizedPaletteQuantizer.Wu(int,Color32,byte)">OptimizedPaletteQuantizer.Wu</see> method will be used.
        /// <br/>If not <see langword="null"/>, then all frames will be quantized, even the already indexed ones. If it does not support transparency,
        /// then <see cref="AllowDeltaFrames"/> will be ignored.
        /// </value>
        public IQuantizer? Quantizer { get; set; }

        /// <summary>
        /// Gets or sets an optional ditherer to be used when quantizing the frames.
        /// <br/>Default value: <see langword="null"/>.
        /// </summary>
        public IDitherer? Ditherer { get; set; }

        /// <summary>
        /// Gets or sets the looping mode of the animation.
        /// <br/>Default value: <see cref="Imaging.AnimationMode.Repeat"/>.
        /// </summary>
        /// <value>
        /// If <see cref="Imaging.AnimationMode.Repeat"/>, then the animation will be repeated indefinitely.
        /// <br/>If <see cref="Imaging.AnimationMode.PingPong"/>, then the specified frames will be added in both ways so the final animation will be played back and forth.
        /// <br/>If <see cref="Imaging.AnimationMode.PlayOnce"/>, then the animation will be played only once.
        /// <br/>The <see cref="GifEncoder"/> actually supports any positive value less than or equal to <see cref="UInt16.MaxValue">UInt16.MaxValue</see> even though
        /// they don't have named values in the <see cref="Imaging.AnimationMode"/> enumeration.
        /// </value>
        public AnimationMode AnimationMode { get; set; }

        /// <summary>
        /// Gets or sets whether the transparent borders of the frames should be encoded as part of the frame.
        /// <br/>Default value: <see langword="false"/>.
        /// </summary>
        /// <value>
        /// If <see langword="true"/>, then transparent borders of the frames will be considered as image content (and possibly smaller frames will be virtually
        /// enlarged, too). This produces a bit larger encoded size but provides better compatibility.
        /// <br/>If <see langword="false"/>, then always only the smallest possible non-transparent area will be encoded. Some decoders may not tolerate this option.
        /// </value>
        [Obsolete("This property is obsolete. Use AllowClippedFrames instead.")]
        public bool EncodeTransparentBorders { get => !AllowClippedFrames; set => AllowClippedFrames = !value; }

        /// <summary>
        /// Gets or sets whether the encoder is allowed to add smaller actual frames than the <see cref="Size"/> of the animation.
        /// <br/>Default value: <see langword="true"/>.
        /// <br/>See also the <strong>Remarks</strong> section of the <see cref="AllowDeltaFrames"/> property for more details.
        /// </summary>
        /// <value>
        /// If <see langword="false"/>, then always full-sized frames are added to the animation. This might end up in a larger encoded size but provides better compatibility.
        /// <br/>If <see langword="true"/>, then actual frames might be clipped. If <see cref="AllowDeltaFrames"/> is <see langword="false"/>, then it affects only the clipping of possible
        /// transparent borders. Some decoders may not tolerate this option.
        /// </value>
        public bool AllowClippedFrames { get; set; } = true;

        /// <summary>
        /// Gets or sets whether it is allowed to encode only the changed region of a frame. In some circumstances the value of this property might be ignored.
        /// <br/>Default value: <see langword="true"/>.
        /// </summary>
        /// <value>
        /// If <see langword="true"/>, then the required memory during encoding may be larger but it allows creating more compact files and even high color frames (see also the <strong>Remarks</strong> section).
        /// <br/>If <see langword="false"/>, then all frames will be encoded individually. This provides lower memory consumption but may produce larger files.
        /// </value>
        /// <remarks>
        /// <para>If <see cref="Quantizer"/> is set to an <see cref="OptimizedPaletteQuantizer"/> that allows creating a specific palette for each frame,
        /// then setting this property to <see langword="true"/> might also allow producing high color frames.</para>
        /// <para>If <see cref="AllowClippedFrames"/> is <see langword="false"/>, then this property is ignored for quantizers with no transparency support.
        /// Therefore, make sure that you set also the <see cref="AllowClippedFrames"/> to <see langword="true"/> if you use a quantizer without transparency support.</para>
        /// <note>The difference to be encoded is always determined by the <em>original</em> images. If you use an <see cref="OptimizedPaletteQuantizer"/>, then it can occur
        /// that the first frame has a poorer mapping of the original colors than the delta images, which can cause a "ghosting image" effect (the traces of the changes
        /// can be seen in the animation). It can be avoided by using a fix palette instead. To minimize it by using an <see cref="OptimizedPaletteQuantizer"/>
        /// it is recommended to use the <see cref="OptimizedPaletteQuantizer.MedianCut(int,Color32,byte)">MedianCut</see> quantizer.</note>
        /// </remarks>
        public bool AllowDeltaFrames { get; set; } = true;

        /// <summary>
        /// Gets or sets the allowed maximum tolerance for detecting changes of consecutive frames when <see cref="AllowDeltaFrames"/> is <see langword="true"/>.
        /// <br/>Default value: 0.
        /// </summary>
        /// <value>
        /// If 0, then even a minimal color difference will be considered as a change to be encoded.
        /// <br/>If 255, then nothing will be treated as a change. The animation will have no new frames unless a frame contains new transparent pixels compared to the previous one.
        /// <br/>The reasonable range is between 0 and 16 with an optimized quantizer. Predefined quantizers may tolerate larger values (eg. up to 32) with some dithering.
        /// </value>
        public byte DeltaTolerance { get; set; }

        /// <summary>
        /// Gets or sets whether to report overall and/or sub-task progress when encoding by <see cref="GifEncoder.BeginEncodeAnimation">GifEncoder.BeginEncodeAnimation</see>
        /// and <see cref="GifEncoder.EncodeAnimationAsync">GifEncoder.EncodeAnimationAsync</see> methods and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">AsyncConfigBase.Progress</a> property is set.
        /// When <see langword="null"/>, then both are reported.
        /// <br/>Default value: <see langword="null"/>.
        /// </summary>
        /// <value>
        /// If <see langword="null"/>, then both overall and sub-task progress are reported. You can filter overall progress steps by considering <see cref="DrawingOperation.Saving"/> operations only.
        /// <br/>If <see langword="true"/>, then only overall progress steps are reported. Please note that if the count of the <see cref="IEnumerable{T}"/>
        /// instance passed to the constructor cannot be determined in a trivial way, then the maximum value of the steps will be adjusted dynamically.
        /// <br/>If <see langword="false"/>, then only sub-task progress steps are reported such as optimizing palette, quantizing and other processing operations.
        /// </value>
        public bool? ReportOverallProgress { get; set; }

        #endregion

        #region Internal Properties

        internal IEnumerable<IReadableBitmapData> Frames { get; }
        internal IEnumerable<TimeSpan> Delays { get; }

        #endregion

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AnimatedGifConfiguration"/> class.
        /// </summary>
        /// <param name="frames">The collection of the frames to be added to the result animation. Disposing of the frames must be performed by the caller.
        /// <see cref="GifEncoder.EncodeAnimation">GifEncoder.EncodeAnimation</see> enumerates the collection lazily so you can pass an iterator that disposes
        /// the previous frame once the next one is queried, or you can even re-use the same bitmap data for each frames if you generate them dynamically.</param>
        /// <param name="delay">An optional <see cref="TimeSpan"/> to specify the delay for all frames. If <see langword="null"/>,
        /// then a default 100 ms delay will be used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="frames"/> is <see langword="null"/>.</exception>
        public AnimatedGifConfiguration(IEnumerable<IReadableBitmapData> frames, TimeSpan? delay = null)
            : this(frames, delay == null ? null : new[] { delay.Value })
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AnimatedGifConfiguration"/> class.
        /// </summary>
        /// <param name="frames">The collection of the frames to be added to the result animation. Disposing of the frames must be performed by the caller.
        /// <see cref="GifEncoder.EncodeAnimation">GifEncoder.EncodeAnimation</see> enumerates the collection lazily so you can pass an iterator that disposes
        /// the previous frame once the next one is queried, or you can even re-use the same bitmap data for each frames if you generate them dynamically.</param>
        /// <param name="delays">The collection of the delays to be used for the animation. If <see langword="null"/> or empty,
        /// then a default 100 ms delay will be used for all frames.
        /// If contains less elements than <paramref name="frames"/>, then the last value will be re-used for the remaining frames.</param>
        /// <exception cref="ArgumentNullException"><paramref name="frames"/> is <see langword="null"/>.</exception>
        public AnimatedGifConfiguration(IEnumerable<IReadableBitmapData> frames, IEnumerable<TimeSpan>? delays)
        {
            Frames = frames ?? throw new ArgumentNullException(nameof(frames), PublicResources.ArgumentNull);
            Delays = delays ?? defaultDelays;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AnimatedGifConfiguration"/> class.
        /// </summary>
        /// <param name="getNextFrame">A delegate that returns the next frame of the animation. It should return <see langword="null"/> after the last frame.
        /// Frames are not disposed by the encoder so the caller can dispose them once the subsequent frame is requested.</param>
        /// <param name="getNextDelay">A delegate that returns the delay for the next frame. If it returns <see langword="null"/> sooner than <paramref name="getNextFrame"/>, then
        /// the last non-<see langword="null"/> value will be re-used for the remaining frames. If it returns <see langword="null"/> for the first time, then
        /// each frame will use a default 100 ms delay.</param>
        public AnimatedGifConfiguration(Func<IReadableBitmapData?> getNextFrame, Func<TimeSpan?> getNextDelay) : this(IterateFrames(getNextFrame), IterateDelays(getNextDelay))
        {
        }

        #endregion

        #region Methods

        private static IEnumerable<IReadableBitmapData> IterateFrames(Func<IReadableBitmapData?> getNextFrame)
        {
            if (getNextFrame == null)
                throw new ArgumentNullException(nameof(getNextFrame), PublicResources.ArgumentNull);
            IReadableBitmapData? nextFrame;
            while ((nextFrame = getNextFrame.Invoke()) != null)
                yield return nextFrame;
        }

        private static IEnumerable<TimeSpan> IterateDelays(Func<TimeSpan?> getNextDelay)
        {
            if (getNextDelay == null)
                throw new ArgumentNullException(nameof(getNextDelay), PublicResources.ArgumentNull);
            TimeSpan? nextDelay;
            while ((nextDelay = getNextDelay.Invoke()).HasValue)
                yield return nextDelay.Value;
        }

        #endregion
    }
}
