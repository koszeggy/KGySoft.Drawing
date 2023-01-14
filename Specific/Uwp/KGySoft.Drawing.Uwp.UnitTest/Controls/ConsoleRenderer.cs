#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ConsoleRenderer.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2023 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System;
using System.IO;
using System.Linq;
using System.Text;

using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Media;

#endregion

namespace KGySoft.Drawing.Uwp.UnitTest.Controls
{
    internal class ConsoleRenderer : UserControl
    {
        #region ConsoleWriter class

        private sealed class ConsoleWriter : TextWriter
        {
            #region Fields

            private readonly RichTextBlock text;

            private ConsoleColor foreColor;

            #endregion

            #region Properties

            #region Public Properties

            public override Encoding Encoding => Console.OutputEncoding;

            #endregion

            #region Internal Properties

            internal ConsoleColor ForegroundColor
            {
                get => foreColor;
                set
                {
                    if (foreColor == value)
                        return;

                    foreColor = value;
                    var color = ToColor(value);
                    Invoke(() =>
                    {
                        BlockCollection blocks = text.Blocks;
                        Paragraph paragraph;
                        if (blocks.Count == 0)
                            blocks.Add(paragraph = new Paragraph());
                        else
                            paragraph = (Paragraph)blocks[0];
                        InlineCollection inlines = paragraph.Inlines;
                        if (inlines.Count == 0 || ((SolidColorBrush)((Run)inlines[inlines.Count - 1]).Foreground).Color != color)
                            inlines.Add(new Run { Foreground = new SolidColorBrush(color) });
                    });
                }
            }

            #endregion

            #endregion

            #region Constructors

            internal ConsoleWriter(RichTextBlock text)
            {
                this.text = text;
                ForegroundColor = Console.ForegroundColor;
            }

            #endregion

            #region Methods

            #region Static Methods

            private static Color ToColor(ConsoleColor color) => color switch
            {
                ConsoleColor.Black => Colors.Black,
                ConsoleColor.DarkBlue => Colors.DarkBlue,
                ConsoleColor.DarkGreen => Colors.DarkGreen,
                ConsoleColor.DarkCyan => Colors.DarkCyan,
                ConsoleColor.DarkRed => Colors.DarkRed,
                ConsoleColor.DarkMagenta => Colors.DarkMagenta,
                ConsoleColor.DarkYellow => Colors.Olive,
                ConsoleColor.Gray => Colors.Gray,
                ConsoleColor.DarkGray => Colors.DarkGray,
                ConsoleColor.Blue => Colors.Blue,
                ConsoleColor.Green => Colors.Green,
                ConsoleColor.Cyan => Colors.Cyan,
                ConsoleColor.Red => Colors.Red,
                ConsoleColor.Magenta => Colors.Magenta,
                ConsoleColor.Yellow => Colors.Yellow,
                ConsoleColor.White => Colors.White,
                _ => throw new ArgumentOutOfRangeException(nameof(color))
            };

            #endregion

            #region Instance Methods

            #region Public Methods

            public override void Write(char value) => DoWrite(value.ToString());
            public override void Write(string value) => DoWrite(value);

            #endregion

            #region Private Methods

            private void DoWrite(string value) => Invoke(() => ((Run)((Paragraph)text.Blocks.Last()).Inlines.Last()).Text += value);

            private void Invoke(DispatchedHandler handler)
            {
                var _ = text.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, handler);
            }

            #endregion

            #endregion

            #endregion
        }

        #endregion

        #region Fields

        private readonly ConsoleWriter writer;

        #endregion

        #region Properties

        internal TextWriter Writer => writer;

        internal ConsoleColor ForegroundColor
        {
            get => writer.ForegroundColor;
            set => writer.ForegroundColor = value;
        }

        #endregion

        #region Constructors

        public ConsoleRenderer()
        {
            var textBlock = new RichTextBlock { FontFamily = new FontFamily("Consolas") };
            Content = textBlock;
            writer = new ConsoleWriter(textBlock);
            Console.SetOut(writer);
        }

        #endregion
    }
}
