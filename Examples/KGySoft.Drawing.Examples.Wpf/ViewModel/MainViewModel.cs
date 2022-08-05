#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: MainViewModel.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2022 - All Rights Reserved
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
using System.IO;
using System.Linq;
using System.Windows.Media;

using KGySoft.ComponentModel;
using KGySoft.CoreLibraries;
using KGySoft.Drawing.Wpf;

#endregion

namespace KGySoft.Drawing.Examples.Wpf.ViewModel
{
    internal class MainViewModel : ValidatingObjectBase
    {
        #region Enumerations

        public enum Ditherer
        {
            None,
            Ordered,
            ErrorDiffusion,
            RandomNoise,
            InterleavedGradientNoise
        }

        #endregion

        #region Fields

        private static HashSet<string> affectsPreview = new()
            {
                nameof(ImageFile),
                nameof(OverlayFile),
                nameof(ShowOverlay),
                nameof(SelectedFormat),
                nameof(BackColor),
                nameof(AlphaThreshold),
                nameof(OptimizePalette),
                nameof(SelectedDitherer),
            };

        #endregion

        #region Properties

        public PixelFormat[] PixelFormats { get; } = typeof(PixelFormats).GetProperties()
            .Where(p => p.Name != nameof(System.Windows.Media.PixelFormats.Default))
            .Select(p => (PixelFormat)p.GetValue(null, null)!)
            .ToArray();

        public Ditherer[] Ditherers { get; } = Enum<Ditherer>.GetValues();
        public string? ImageFile { get => Get<string?>(); set => Set(value); }
        public string? OverlayFile { get => Get<string?>(); set => Set(value); }
        public bool ShowOverlay { get => Get<bool>(); set => Set(value); }
        public PixelFormat SelectedFormat { get => Get<PixelFormat>(); set => Set(value); }
        public string BackColorText { get => Get<string>(); set => Set(value); }
        public Brush BackColor { get => Get<Brush>(); set => Set(value); }
        public byte AlphaThreshold { get => Get<byte>(); set => Set(value); }
        public bool OptimizePalette { get => Get<bool>(); set => Set(value); }
        public bool OptimizePaletteEnabled { get => Get<bool>(); set => Set(value); }
        public Ditherer SelectedDitherer { get => Get<Ditherer>(); set => Set(value); }
        public ImageSource ResultImage { get => Get<ImageSource>(); set => Set(value); }

        #endregion

        #region Constructors

        public MainViewModel()
        {
            ImageFile = @"..\..\..\..\..\Help\Images\Shield256.png";
            OverlayFile = @"..\..\..\..\..\Help\Images\AlphaGradient.png";
            SelectedFormat = System.Windows.Media.PixelFormats.Indexed8;
            BackColorText = "#FFC0C0C0";
            AlphaThreshold = 128;
        }

        #endregion

        #region Methods

        #region Protected Methods

        protected override ValidationResultsCollection DoValidation()
        {
            var result = new ValidationResultsCollection();
            if (String.IsNullOrEmpty(ImageFile) || !File.Exists(ImageFile))
                result.AddError(nameof(ImageFile), "The specified file does not exist");
            if (ShowOverlay && String.IsNullOrEmpty(OverlayFile) || !File.Exists(OverlayFile))
                result.AddError(nameof(OverlayFile), "The specified file does not exist");
            try
            {
                ColorConverter.ConvertFromString(BackColorText);
            }
            catch (Exception e)
            {
                result.AddError(nameof(BackColorText), e.Message);
            }

            return result;
        }

        protected override void OnPropertyChanged(PropertyChangedExtendedEventArgs e)
        {
            base.OnPropertyChanged(e);
            switch (e.PropertyName)
            {
                case nameof(SelectedFormat):
                    OptimizePaletteEnabled = SelectedFormat.IsIndexed();
                    break;

                case nameof(BackColorText):
                    var color = (Color?)ColorConverter.ConvertFromString(BackColorText);
                    BackColor = new SolidColorBrush(color ?? Colors.Black);
                    break;
            }

            if (affectsPreview.Contains(e.PropertyName!))
                GenerateResult();
        }

        #endregion

        #region Private Methods

        private void GenerateResult()
        {
        }

        #endregion

        #endregion
    }
}
