#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: AnimGifConfig.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2021 - All Rights Reserved
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

namespace KGySoft.Drawing.Imaging
{
    /// <summary>
    /// Represents the configuration for encoding a GIF animation by the <see cref="GifEncoder.EncodeAnimation">GifEncoder.EncodeAnimation</see> method.
    /// </summary>
    public sealed class AnimGifConfig
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
        /// If <see langword="true"/>, then zero values in the <see cref="Delays"/> property will be replaced to 100 milliseconds.
        /// <br/>If <see langword="false"/>, then zero delays will be preserved and the decoders decide how to interpret them.
        /// </value>
        public bool ReplaceZeroDelays { get; set; } = true;

        /// <summary>
        /// Gets or sets an optional quantizer to be used for the frames. 
        /// Using a non-<see langword="null"/>&#160;value forces the quantization also of already indexed images.
        /// Should use up to 256 colors; otherwise, the result might be quantized further with using the default system 8-bit palette.
        /// <br/>Default value: <see langword="null"/>.
        /// </summary>
        /// <value>
        /// If <see langword="null"/>, then the possibly existing palette of already indexed input frames are preserved.
        /// For frames with a non-indexed pixel format a quantizer returned by the <see cref="OptimizedPaletteQuantizer.Wu">OptimizedPaletteQuantizer.Wu</see> method will be used.
        /// <br/>If not <see langword="null"/>, then all frames will be quantized, even the already indexed ones. If does not support transparency,
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
        public bool EncodeTransparentBorders { get; set; }

        /// <summary>
        /// Gets or sets whether it is allowed to encode only the changed part of a frame. In some circumstances the value of this property might be ignored.
        /// <br/>Default value: <see langword="true"/>.
        /// </summary>
        /// <value>
        /// If <see langword="true"/>, then the encoding time and the required memory may be larger but it allows creating high-color GIF animations.
        /// This depends also on the used <see cref="Quantizer"/>, though: an <see cref="OptimizedPaletteQuantizer"/> allows creating a specific palette for each frame, for example.
        /// <br/>If <see langword="false"/>, then all frames will be encoded individually. This provides faster encoding time with lower memory consumption.
        /// </value>
        public bool AllowDeltaFrames { get; set; } = true;

        #endregion

        #region Internal Properties

        internal IEnumerable<IReadableBitmapData> Frames { get; }

        internal IEnumerable<TimeSpan> Delays { get; }

        #endregion

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AnimGifConfig"/> class.
        /// </summary>
        /// <param name="frames">The collection of the frames to be added to the result animation. Disposing of the frames must be performed by the caller.
        /// <see cref="GifEncoder.EncodeAnimation">GifEncoder.EncodeAnimation</see> enumerates the collection lazily so you can pass an iterator that disposes
        /// the previous frame once the next one is queried, or you can even re-use the same bitmap data for each frames if you generate them dynamically.</param>
        /// <param name="delay">An optional <see cref="TimeSpan"/> to specify the delay for all frames. If <see langword="null"/>,
        /// then a default 100 ms delay will be used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="frames"/> is <see langword="null"/>.</exception>
        public AnimGifConfig(IEnumerable<IReadableBitmapData> frames, TimeSpan? delay = null)
            : this(frames, delay == null ? null : new[] { delay.Value })
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AnimGifConfig"/> class.
        /// </summary>
        /// <param name="frames">The collection of the frames to be added to the result animation. Disposing of the frames must be performed by the caller.
        /// <see cref="GifEncoder.EncodeAnimation">GifEncoder.EncodeAnimation</see> enumerates the collection lazily so you can pass an iterator that disposes
        /// the previous frame once the next one is queried, or you can even re-use the same bitmap data for each frames if you generate them dynamically.</param>
        /// <param name="delays">The collection of the delays to be used for the animation. If <see langword="null"/>&#160;or empty,
        /// then a default 100 ms delay will be used for all frames.
        /// If contains less elements than <paramref name="frames"/>, then the last value will be re-used for the remaining frames.</param>
        /// <exception cref="ArgumentNullException"><paramref name="frames"/> is <see langword="null"/>.</exception>
        public AnimGifConfig(IEnumerable<IReadableBitmapData> frames, IEnumerable<TimeSpan>? delays)
        {
            Frames = frames ?? throw new ArgumentNullException(nameof(frames), PublicResources.ArgumentNull);
            Delays = delays ?? defaultDelays;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AnimGifConfig"/> class.
        /// </summary>
        /// <param name="getNextFrame">A delegate that returns the next frame of the animation. It should return <see langword="null"/>&#160;after the last frame.
        /// Frames are not disposed by the encoder so the caller can dispose them once the subsequent frame is requested.</param>
        /// <param name="getNextDelay">A delegate that returns the delay for the next frame. If it returns <see langword="null"/>&#160;sooner than <paramref name="getNextFrame"/>, then
        /// the last non-<see langword="null"/>&#160;value will be re-used for the remaining frames. If it returns <see langword="null"/>&#160;for the first time, then
        /// each frame will use a default 100 ms delay.</param>
        public AnimGifConfig(Func<IReadableBitmapData?> getNextFrame, Func<TimeSpan?> getNextDelay) : this(IterateFrames(getNextFrame), IterateDelays(getNextDelay))
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
