#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: MetafileExtensions.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2019 - All Rights Reserved
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
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;

using KGySoft.Drawing.WinApi;
using KGySoft.Serialization.Binary;

#endregion

namespace KGySoft.Drawing
{
    /// <summary>
    /// Contains extension methods for the <see cref="Metafile"/> type.
    /// </summary>
    public static class MetafileExtensions
    {
        #region WmfHeader struct

        [StructLayout(LayoutKind.Sequential, Pack = 2)]
        private struct WmfHeader
        {
            #region Fields

            /// <summary>
            /// Magic number (always 9AC6CDD7h)
            /// </summary>
            private uint key;

            /// <summary>
            /// Metafile HANDLE number (always 0)
            /// </summary>
            private ushort handle;

            /// <summary>
            /// Left coordinate in metafile units
            /// </summary>
            private short left;

            /// <summary>
            /// Top coordinate in metafile units
            /// </summary>
            private short top;

            /// <summary>
            /// Right coordinate in metafile units
            /// </summary>
            private short right;

            /// <summary>
            /// Bottom coordinate in metafile units
            /// </summary>
            private short bottom;

            /// <summary>
            /// Number of metafile units per inch
            /// </summary>
            private ushort inch;

            /// <summary>
            /// Reserved (always 0)
            /// </summary>
            private uint reserved;

            /// <summary>
            /// Checksum value for previous 10 WORDs
            /// </summary>
            private ushort checksum;

            #endregion

            #region Constructors

            internal WmfHeader(short width, short height, ushort dpi)
                : this()
            {
                key = 0x9AC6CDD7;
                right = width;
                bottom = height;
                inch = dpi;
                checksum = (ushort)(key & 0x0000FFFF);
                checksum ^= (ushort)(key >> 16);
                checksum ^= (ushort)right;
                checksum ^= (ushort)bottom;
                checksum ^= inch;
            }

            #endregion
        }

        #endregion

        #region Methods

        #region Public Methods

        /// <summary>
        /// Creates a <see cref="Bitmap"/> of a <see cref="Metafile"/> instance provided in the <paramref name="metafile"/> parameter.
        /// </summary>
        /// <param name="metafile">The <see cref="Metafile"/> to convert.</param>
        /// <param name="requestedSize">The requested size of the result <see cref="Bitmap"/>.</param>
        /// <param name="antiAliased"><see langword="true"/>&#160;to create an anti-aliased result; otherwise, <see langword="false"/>.
        /// <br/>Default value: <see langword="false"/>.</param>
        /// <returns>A <see cref="Bitmap"/> instance of the requested size.</returns>
        public static Bitmap ToBitmap(this Metafile metafile, Size requestedSize, bool antiAliased = false)
        {
            if (metafile == null)
                throw new ArgumentNullException(nameof(metafile), PublicResources.ArgumentNull);

            if (requestedSize.Width < 1 || requestedSize.Height < 1)
                throw new ArgumentOutOfRangeException(nameof(requestedSize), PublicResources.ArgumentOutOfRange);

            if (!antiAliased)
                return new Bitmap(metafile, requestedSize);

            using (Bitmap bmpDouble = new Bitmap(metafile, requestedSize.Width << 1, requestedSize.Height << 1))
            {
                return bmpDouble.Resize(requestedSize, false);
            }
        }

        /// <summary>
        /// Saves a <see cref="Metafile"/> instance into a <see cref="Stream"/>.
        /// Actual format is selected by the raw format of the metafile.
        /// </summary>
        /// <param name="metafile">The <see cref="Metafile"/> instance to save.</param>
        /// <param name="stream">The <see cref="Stream"/> into the metafile should be saved.</param>
        public static void Save(this Metafile metafile, Stream stream)
        {
            if (metafile == null)
                throw new ArgumentNullException(nameof(metafile), PublicResources.ArgumentNull);
            if (stream == null)
                throw new ArgumentNullException(nameof(stream), PublicResources.ArgumentNull);

            Save(metafile, stream, false);
        }

        /// <summary>
        /// Saves a <see cref="Metafile"/> instance into a <see cref="Stream"/> using the required format.
        /// </summary>
        /// <param name="metafile">The <see cref="Metafile"/> instance to save.</param>
        /// <param name="stream">The <see cref="Stream"/> into the metafile should be saved.</param>
        /// <param name="forceWmfFormat">When <see langword="true"/>, forces to use the Windows Metafile Format (WMF), even if
        /// the <paramref name="metafile"/> itself is encoded by Enhanced Metafile Format (EMF). When <see langword="false"/>, uses the appropriate format automatically.</param>
#if !NET35
        [SecuritySafeCritical]
#endif
        public static void Save(this Metafile metafile, Stream stream, bool forceWmfFormat)
        {
            if (metafile == null)
                throw new ArgumentNullException(nameof(metafile), PublicResources.ArgumentNull);
            if (stream == null)
                throw new ArgumentNullException(nameof(stream), PublicResources.ArgumentNull);

            bool isWmf = metafile.RawFormat.Guid == ImageFormat.Wmf.Guid;
            if (isWmf || forceWmfFormat)
                WriteWmfHeader(metafile, stream);

            // making a clone, otherwise, it will not be usable after saving
            metafile = (Metafile)metafile.Clone();
            IntPtr handle = metafile.GetHenhmetafile();
            try
            {
                byte[] buffer = isWmf ? Gdi32.GetWmfContent(handle)
                        : (forceWmfFormat ? Gdi32.GetWmfContentFromEmf(handle) : Gdi32.GetEmfContent(handle));
                stream.Write(buffer, 0, buffer.Length);
            }
            finally
            {
                Gdi32.DeleteEnhMetaFile(handle);
                metafile.Dispose();
            }
        }

        #endregion

        #region Private Methods

        [SecurityCritical]
        private static void WriteWmfHeader(Metafile metafile, Stream stream)
        {
            WmfHeader header = new WmfHeader((short)metafile.Width, (short)metafile.Height, (ushort)metafile.HorizontalResolution);
            byte[] rawHeader = BinarySerializer.SerializeValueType(header);
            stream.Write(rawHeader, 0, rawHeader.Length);
        }

        #endregion

        #endregion
    }
}
