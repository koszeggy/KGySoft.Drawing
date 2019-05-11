﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using KGySoft.Drawing.WinApi;
using KGySoft.Serialization;

namespace KGySoft.Drawing
{
    /// <summary>
    /// Provides extension methods for <see cref="Metafile"/> class.
    /// </summary>
    public static class MetafileTools
    {
        [StructLayout(LayoutKind.Sequential, Pack = 2)]
        private struct WmfHeader
        {
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
        }

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
                throw new ArgumentNullException("metafile");

            if (requestedSize.Width < 1 || requestedSize.Height < 1)
                throw new ArgumentOutOfRangeException("requestedSize");

            if (!antiAliased)
                return new Bitmap(metafile, requestedSize);

            using (Bitmap bmpDouble = new Bitmap(metafile, requestedSize.Width << 1, requestedSize.Height << 1))
            {
                return bmpDouble.Resize(requestedSize, false);
            }
        }

        /// <summary>
        /// Saves a <see cref="Metafile"/> instace into a <see cref="Stream"/>.
        /// Actual format is selected by the raw format of the metafile.
        /// </summary>
        /// <param name="metafile">The <see cref="Metafile"/> instance to save.</param>
        /// <param name="stream">The <see cref="Stream"/> into the metafile should be saved.</param>
        public static void Save(this Metafile metafile, Stream stream)
        {
            if (metafile == null)
                throw new ArgumentNullException("metafile");
            if (stream == null)
                throw new ArgumentNullException("stream");

            Save(metafile, stream, false);
        }

        /// <summary>
        /// Saves a <see cref="Metafile"/> instace into a <see cref="Stream"/> using the required format.
        /// </summary>
        /// <param name="metafile">The <see cref="Metafile"/> instance to save.</param>
        /// <param name="stream">The <see cref="Stream"/> into the metafile should be saved.</param>
        /// <param name="forceWmfFormat">When <see langword="true"/>, forces to use the Windows Metafile Format (WMF), even if
        /// the <paramref name="metafile"/> itself is encoded by Enhanced Metafile Format (EMF). When <see langword="false"/>, uses the appropriate format automatically.</param>
        public static void Save(this Metafile metafile, Stream stream, bool forceWmfFormat)
        {
            if (metafile == null)
                throw new ArgumentNullException("metafile");
            if (stream == null)
                throw new ArgumentNullException("stream");

            bool isWmf = metafile.RawFormat.Guid == ImageFormat.Wmf.Guid;
            if (isWmf || forceWmfFormat)
                WriteWmfHeader(metafile, stream);

            // making a clone, otherwise, it will not be usable after saving
            metafile = (Metafile)metafile.Clone();
            IntPtr handle = metafile.GetHenhmetafile();
            try
            {
                uint bufSize;
                byte[] buffer;

                if (isWmf)
                {
                    // Saving WMF
                    bufSize = Gdi32.GetMetaFileBitsEx(handle, 0, null);
                    buffer = new byte[bufSize];
                    if (Gdi32.GetMetaFileBitsEx(handle, bufSize, buffer) == 0)
                        throw new Win32Exception("GetMetaFileBitsEx was unable to retrieve WMF data");
                }
                else
                {
                    if (forceWmfFormat)
                    {
                        // Saving EMF as WMF
                        using (Graphics g = Graphics.FromHwndInternal(IntPtr.Zero))
                        {
                            IntPtr hdc = g.GetHdc();
                            try
                            {
                                bufSize = Gdi32.GetWinMetaFileBits(handle, 0, null, MappingModes.MM_ANISOTROPIC, hdc);
                                buffer = new byte[bufSize];
                                if (Gdi32.GetWinMetaFileBits(handle, bufSize, buffer, MappingModes.MM_ANISOTROPIC, hdc) == 0)
                                    throw new Win32Exception("GetWinMetaFileBits was unable to retrieve WMF data");
                            }
                            finally
                            {
                                g.ReleaseHdc(hdc);
                            }
                        }
                    }
                    else
                    {
                        // Saving EMF
                        bufSize = Gdi32.GetEnhMetaFileBits(handle, 0, null);
                        buffer = new byte[bufSize];
                        if (Gdi32.GetEnhMetaFileBits(handle, bufSize, buffer) == 0)
                            throw new Win32Exception("GetEnhMetaFileBits was unable to retrieve EMF data");
                    }
                }

                stream.Write(buffer, 0, (int)bufSize);
            }
            finally
            {
                Gdi32.DeleteEnhMetaFile(handle);
                metafile.Dispose();
            }
        }

        private static void WriteWmfHeader(Metafile metafile, Stream stream)
        {
            WmfHeader header = new WmfHeader((short)metafile.Width, (short)metafile.Height, (ushort)metafile.HorizontalResolution);
            byte[] rawHeader = BinarySerializer.SerializeValueType(header);
            stream.Write(rawHeader, 0, rawHeader.Length);
        }
    }
}