#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: MainForm.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2026 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

using KGySoft.Drawing.Examples.WinForms.ViewModel;
using KGySoft.WinForms;
using KGySoft.WinForms.Controls;
using KGySoft.WinForms.Forms;

#endregion

namespace KGySoft.Drawing.Examples.WinForms.View
{
    internal partial class MainForm : BaseForm
    {
        #region Fields

        private static readonly bool visualStyles = Application.RenderWithVisualStyles;

        private readonly MainViewModel viewModel = default!;

        #endregion

        #region Constructors

        #region Public Constructors

        public MainForm() => InitializeComponent(); // This ctor is just for the designer

        #endregion

        #region Internal Constructors

        internal MainForm(MainViewModel viewModel) : this()
        {
            this.viewModel = viewModel;
#if NETFRAMEWORK
            Font = SystemFonts.MessageBoxFont;
#endif
            InitPropertyBindings();
            InitCommandBindings();
            var _ = viewModel.ViewApplied();
        }

        #endregion

        #endregion

        #region Methods

        #region Protected Methods

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            ssStatus.Height = this.ScaleHeight(22);
            errorProvider.UpdateBinding(); // just to display the errors immediately at startup if the files are missing
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                components?.Dispose();

            base.Dispose(disposing);
        }

        #endregion

        #region Private Methods

        // Initializing property bindings. Using regular WinForms bindings for TextBoxes so ErrorProvider also works automatically.
        // For other controls using KGy SOFT bindings. For details, see https://github.com/koszeggy/KGySoft.CoreLibraries#command-binding
        private void InitPropertyBindings()
        {
            errorProvider.Icon = Icons.SystemError;
            errorProvider.DataSource = viewModel;

            // VM.ImageFile <-> txtImageFile.Text
            txtImageFile.DataBindings.Add(nameof(txtImageFile.Text), viewModel, nameof(viewModel.ImageFile), false, DataSourceUpdateMode.OnPropertyChanged);
            //CommandBindings.AddTwoWayPropertyBinding(viewModel, nameof(viewModel.ImageFile), txtImageFile, nameof(txtImageFile.Text));

            // chbImageOverlay.Checked -> VM.ShowOverlay -> txtImageOverlay.Enabled, tblOverlayShape.Enabled
            CommandBindings.AddPropertyBinding(chbImageOverlay, nameof(chbImageOverlay.Checked), nameof(viewModel.ShowOverlay), viewModel);
            CommandBindings.AddPropertyBinding(viewModel, nameof(viewModel.ShowOverlay), nameof(Control.Enabled), txtImageOverlay, tblOverlayShape);

            // VM.OutlineEnabled -> tblOutline.Enabled
            CommandBindings.AddPropertyBinding(viewModel, nameof(viewModel.OutlineEnabled), nameof(tblOutline.Enabled), tblOutline);

            // VM.OverlayShapes -> cmbOverlayShape.DataSource (once)
            cmbOverlayShape.DataSource = viewModel.OverlayShapes;

            // VM.OverlayShape -> cmbOverlayShape.SelectedItem (cannot use two-way for SelectedItem because there is no SelectedItemChanged event)
            CommandBindings.AddPropertyBinding(viewModel, nameof(viewModel.OverlayShape), nameof(cmbOverlayShape.SelectedItem), cmbOverlayShape);

            // VM.OverlayShape <- cmbOverlayShape.SelectedValue (cannot use two-way for SelectedValue because ValueMember is not set)
            CommandBindings.AddPropertyBinding(cmbOverlayShape, nameof(cmbOverlayShape.SelectedValue), nameof(viewModel.OverlayShape), viewModel);

            // VM.OutlineWidth (int) <-> numOutline.Value (decimal)
            CommandBindings.AddTwoWayPropertyBinding(viewModel, nameof(viewModel.OutlineWidth), numOutline, nameof(numOutline.Value),
                i => (decimal)(int)i!, d => (int)(decimal)d!);

            // VM.OutlineColor -> pnlOutlineColor.BackColor
            CommandBindings.AddPropertyBinding(viewModel, nameof(viewModel.OutlineColor), nameof(pnlOutlineColor.BackColor), pnlOutlineColor);

            // VM.OverlayFile <-> txtImageOverlay.Text
            txtImageOverlay.DataBindings.Add(nameof(txtImageOverlay.Text), viewModel, nameof(viewModel.OverlayFile), false, DataSourceUpdateMode.OnPropertyChanged);
            //CommandBindings.AddTwoWayPropertyBinding(viewModel, nameof(viewModel.OverlayFile), txtImageOverlay, nameof(txtImageOverlay.Text));

            // VM.PixelFormats -> cmbPixelFormat.DataSource (once)
            cmbPixelFormat.DataSource = viewModel.PixelFormats;

            // VM.SelectedFormat -> cmbPixelFormat.SelectedItem (cannot use two-way for SelectedItem because there is no SelectedItemChanged event)
            CommandBindings.AddPropertyBinding(viewModel, nameof(viewModel.SelectedFormat), nameof(cmbPixelFormat.SelectedItem), cmbPixelFormat);

            // VM.SelectedFormat <- cmbPixelFormat.SelectedValue (cannot use two-way for SelectedValue because ValueMember is not set)
            CommandBindings.AddPropertyBinding(cmbPixelFormat, nameof(cmbPixelFormat.SelectedValue), nameof(viewModel.SelectedFormat), viewModel);

            // chbForceLinear.Checked -> VM.ForceLinearColorSpace
            CommandBindings.AddPropertyBinding(chbForceLinear, nameof(chbForceLinear.Checked), nameof(viewModel.ForceLinearColorSpace), viewModel);

            // chbOptimizePalette.Checked -> VM.OptimizePalette
            CommandBindings.AddPropertyBinding(chbOptimizePalette, nameof(chbOptimizePalette.Checked), nameof(viewModel.OptimizePalette), viewModel);

            // VM.OptimizePaletteEnabled -> chbOptimizePalette.Enabled
            CommandBindings.AddPropertyBinding(viewModel, nameof(viewModel.OptimizePaletteEnabled), nameof(chbOptimizePalette.Enabled), chbOptimizePalette);

            // VM.BackColor -> pnlBackColor.BackColor
            CommandBindings.AddPropertyBinding(viewModel, nameof(viewModel.BackColor), nameof(pnlBackColor.BackColor), pnlBackColor);

            // VM.BackColorEnabled -> tbAlphaThreshold.Enabled
            CommandBindings.AddPropertyBinding(viewModel, nameof(viewModel.BackColorEnabled), nameof(tblBackColor.Enabled), tblBackColor);

            // VM.AlphaThreshold (byte) <-> tbAlphaThreshold.Value (int)
            CommandBindings.AddTwoWayPropertyBinding(viewModel, nameof(viewModel.AlphaThreshold), tbAlphaThreshold, nameof(tbAlphaThreshold.Value),
                b => (int)(byte)b!, i => (byte)(int)i!);

            // VM.AlphaThreshold (byte) -> lblAlphaThreshold.Text (string)
            CommandBindings.AddPropertyBinding(viewModel, nameof(viewModel.AlphaThreshold), nameof(lblAlphaThresholdValue.Text), b => $"{b}", lblAlphaThresholdValue);

            // VM.AlphaThresholdEnabled -> tblAlphaThreshold.Enabled
            CommandBindings.AddPropertyBinding(viewModel, nameof(viewModel.AlphaThresholdEnabled), nameof(tblAlphaThreshold.Enabled), tblAlphaThreshold);

            // chbDitherer.Checked -> VM.UseDithering -> cmbDitherer.Enabled
            CommandBindings.AddPropertyBinding(chbDitherer, nameof(chbDitherer.Checked), nameof(viewModel.UseDithering), viewModel);
            CommandBindings.AddPropertyBinding(viewModel, nameof(viewModel.UseDithering), nameof(cmbDitherer.Enabled), cmbDitherer);

            // VM.Ditherers -> cmbDitherer.DataSource (once)
            cmbDitherer.DataSource = viewModel.Ditherers;

            // VM.SelectedDitherer -> cmbDitherer.SelectedItem (cannot use two-way for SelectedItem because there is no SelectedItemChanged event)
            CommandBindings.AddPropertyBinding(viewModel, nameof(viewModel.SelectedDitherer), nameof(cmbDitherer.SelectedItem), cmbDitherer);

            // VM.SelectedDitherer <- cmbDitherer.SelectedValue (cannot use two-way for SelectedValue because ValueMember is not set)
            CommandBindings.AddPropertyBinding(cmbDitherer, nameof(cmbDitherer.SelectedValue), nameof(viewModel.SelectedDitherer), viewModel);

            // VM.DisplayImage -> pbImage.Image (ToSupportedFormat)
            CommandBindings.AddPropertyBinding(viewModel, nameof(viewModel.DisplayImage), nameof(pbImage.Image), bmp => FormatDisplayImage((Bitmap)bmp!), pbImage);

            // VM.ProgressVisible -> lblProgress.Visible, pbProgress.Visible, timerProgress.Enabled
            CommandBindings.AddPropertyBinding(viewModel, nameof(viewModel.ProgressVisible), nameof(Visible), pbProgress, lblProgress);
            CommandBindings.AddPropertyBinding(viewModel, nameof(viewModel.ProgressVisible), nameof(timerProgress.Enabled), timerProgress);

            // VM.ProgressText -> lblProgress.Text
            CommandBindings.AddPropertyBinding(viewModel, nameof(viewModel.ProgressText), nameof(lblProgress.Text), lblProgress);

            // VM.ProgressMaxValue -> pbProgress.Maximum
            CommandBindings.AddPropertyBinding(viewModel, nameof(viewModel.ProgressMaxValue), nameof(pbProgress.Maximum), pbProgress);

            // VM.IsProgressIndeterminate (bool) -> pbProgress.Style (ProgressBarStyle)
            CommandBindings.AddPropertyBinding(viewModel, nameof(viewModel.IsProgressIndeterminate), nameof(pbProgress.Style),
                b => (bool)b! ? ProgressBarStyle.Marquee : ProgressBarStyle.Blocks, pbProgress);

            // VM.ProgressValue -> pbProgress.Value (by UpdateProgressValue)
            CommandBindings.AddPropertyChangedHandlerBinding(viewModel, () => UpdateProgressValue(viewModel.ProgressValue, pbProgress.ProgressBar), nameof(viewModel.ProgressValue));

            #region Local Methods

            static Bitmap? FormatDisplayImage(Bitmap? bitmap)
            {
                // GDI+ Format16bppGrayScale is not supported by WinForms controls
                if (bitmap?.PixelFormat != PixelFormat.Format16bppGrayScale)
                    return bitmap;

                Bitmap result = bitmap.ConvertPixelFormat(PixelFormat.Format32bppPArgb);
                bitmap.Dispose();
                return result;
            }

            static void UpdateProgressValue(int value, ProgressBar progressBar)
            {
                // Workaround for progress bar with visual styles enabled in which case it advances very slowly
                if (visualStyles && value > progressBar.Value && value < progressBar.Maximum)
                    progressBar.Value = value + 1;
                progressBar.Value = value;
            }

            #endregion
        }

        private void InitCommandBindings()
        {
            // btnOutlineColor.Click -> OnPickOutlineColorCommand
            CommandBindings.Add(OnPickOutlineColorCommand)
                .AddSource(btnOutlineColor, nameof(btnOutlineColor.Click));

            // btnBackColor.Click -> OnPickBackColorCommand
            CommandBindings.Add(OnPickBackColorCommand)
                .AddSource(btnBackColor, nameof(btnBackColor.Click));

            // timerProgress.Tick -> VM.UpdateProgressCommand
            CommandBindings.Add(viewModel.UpdateProgressCommand)
                .AddSource(timerProgress, nameof(timerProgress.Tick));

            // lblProgress.TextChanged, ssStatus.SizeChanged -> OnResizeProgressCommand
            CommandBindings.Add(OnResizeProgressCommand)
                .AddSource(lblProgress, nameof(lblProgress.TextChanged))
                .AddSource(ssStatus, nameof(ssStatus.SizeChanged));
        }

        #endregion

        #region Command Handlers

        private void OnResizeProgressCommand() => pbProgress.Width = ssStatus.ClientSize.Width - lblProgress.Width - this.ScaleWidth(16);

        private void OnPickBackColorCommand()
        {
            colorDialog.Color = viewModel.BackColor;
            if (colorDialog.ShowDialog(this) == DialogResult.OK)
                viewModel.BackColor = colorDialog.Color;
        }

        private void OnPickOutlineColorCommand()
        {
            colorDialog.Color = viewModel.OutlineColor;
            if (colorDialog.ShowDialog(this) == DialogResult.OK)
                viewModel.OutlineColor = colorDialog.Color;
        }

        #endregion

        #endregion
    }
}
