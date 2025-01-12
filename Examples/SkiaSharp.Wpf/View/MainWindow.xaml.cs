#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: MainWindow.cs
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
using System.ComponentModel;
using System.Windows;

using KGySoft.Drawing.Examples.SkiaSharp.Wpf.ViewModel;

using SkiaSharp;
using SkiaSharp.Views.Desktop;

#endregion

// ReSharper disable once CheckNamespace - Crazy WPF bug if the XAML uses SkiaSharp.Views.WPF.SKElement:
// the actual namespace should be KGySoft.Drawing.Examples.SkiaSharp.Wpf.View but
// if either Drawing or SkiaSharp is in the namespace, it triggers CS0234: Type or namespace 'Views' does not exist...
namespace KGySoft.Examples.Skia.Wpf.View
{
    public partial class MainWindow : Window
    {
        #region Properties

        private MainViewModel ViewModel => (MainViewModel)DataContext;

        #endregion

        #region Constructors

        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
        }

        #endregion

        #region Methods

        #region Protected Methods

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            (DataContext as IDisposable)?.Dispose();
        }

        #endregion

        #region Event Handlers

        private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ViewModel.DisplayImageBitmap))
                canvas.InvalidateVisual();
        }

        private void SKElement_OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
        {
            SKRectI targetRect = e.Info.Rect;
            if (targetRect.Height < 1 || targetRect.Width < 1)
                return;

            e.Surface.Canvas.Clear();
            if (ViewModel.DisplayImageBitmap is not SKBitmap bitmap)
                return;

            SKSizeI sourceSize = bitmap.Info.Size;
            float ratio = Math.Min((float)targetRect.Width / sourceSize.Width, (float)targetRect.Height / sourceSize.Height);
            var targetSize = new SKSizeI((int)(sourceSize.Width * ratio), (int)(sourceSize.Height * ratio));
            var targetLocation = new SKPointI((targetRect.Width >> 1) - (targetSize.Width >> 1), (targetRect.Height >> 1) - (targetSize.Height >> 1));
            using var image = SKImage.FromBitmap(bitmap);
            e.Surface.Canvas.DrawImage(image, SKRectI.Create(targetRect.Location + targetLocation,  targetSize), new SKSamplingOptions(SKCubicResampler.Mitchell));
        }

        #endregion

        #endregion
    }
}