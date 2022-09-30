#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: MainForm.cs
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

using System.Globalization;
using System.Windows.Forms;

using KGySoft.ComponentModel;
using KGySoft.Drawing.Examples.WinForms.ViewModel;

#endregion

namespace KGySoft.Drawing.Examples.WinForms.View
{
    public partial class MainForm : Form
    {
        #region Fields

        private readonly MainViewModel viewModel = default!;
        private readonly CommandBindingsCollection commandBindings = new();

        #endregion

        #region Constructors

        #region Public Constructors

        /// <summary>
        /// This constructor is just for the designer
        /// </summary>
        public MainForm() => InitializeComponent();

        #endregion

        #region Internal Constructors

        internal MainForm(MainViewModel viewModel) : this()
        {
            this.viewModel = viewModel;
            InitPropertyBindings();
            InitCommandBindings();
        }

        #endregion

        #endregion

        #region Methods

        #region Protected Methods

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                commandBindings.Dispose();
                components?.Dispose();
            }

            base.Dispose(disposing);
        }

        #endregion

        #region Private Methods

        private void InitPropertyBindings()
        {
            // VM.ImageFile <-> txtImageFile.Text
            commandBindings.AddTwoWayPropertyBinding(viewModel, nameof(viewModel.ImageFile), txtImageFile, nameof(txtImageFile.Text));

            // chbImageOverlay.Checked -> VM.ShowOverlay
            commandBindings.AddPropertyBinding(chbImageOverlay, nameof(chbImageOverlay.Checked), nameof(viewModel.ShowOverlay), viewModel);

            // VM.OverlayFile <-> txtImageOverlay.Text
            commandBindings.AddTwoWayPropertyBinding(viewModel, nameof(viewModel.OverlayFile), txtImageOverlay, nameof(txtImageOverlay.Text));

            // VM.PixelFormats -> cmbPixelFormat.DataSource (once)
            cmbPixelFormat.DataSource = viewModel.PixelFormats;

            // VM.SelectedFormat -> cmbPixelFormat.SelectedItem (cannot use two-way for SelectedItem because there is no SelectedItemChanged event)
            commandBindings.AddPropertyBinding(viewModel, nameof(viewModel.SelectedFormat), nameof(cmbPixelFormat.SelectedItem), cmbPixelFormat);

            // VM.SelectedFormat <- cmbPixelFormat.SelectedValue (cannot use two-way for SelectedValue because ValueMember is not set)
            commandBindings.AddPropertyBinding(cmbPixelFormat, nameof(cmbPixelFormat.SelectedValue), nameof(viewModel.SelectedFormat), viewModel);

            // chbOptimizePalette.Checked -> VM.OptimizePalette
            commandBindings.AddPropertyBinding(chbOptimizePalette, nameof(chbOptimizePalette.Checked), nameof(viewModel.OptimizePalette), viewModel);

            // VM.OptimizePaletteEnabled -> chbOptimizePalette.Enabled
            commandBindings.AddPropertyBinding(viewModel, nameof(viewModel.OptimizePaletteEnabled), nameof(chbOptimizePalette.Enabled), chbOptimizePalette);

            // VM.BackColor -> pnlBackColor.BackColor
            commandBindings.AddPropertyBinding(viewModel, nameof(viewModel.BackColor), nameof(pnlBackColor.BackColor), pnlBackColor);

            // VM.BackColorEnabled -> tbAlphaThreshold.Enabled
            commandBindings.AddPropertyBinding(viewModel, nameof(viewModel.BackColorEnabled), nameof(tblBackColor.Enabled), tblBackColor);

            // VM.AlphaThreshold (byte) <-> tbAlphaThreshold.Value (int)
            commandBindings.AddTwoWayPropertyBinding(viewModel, nameof(viewModel.AlphaThreshold), tbAlphaThreshold, nameof(tbAlphaThreshold.Value),
                b => (int)(byte)b!, i => (byte)(int)i!);

            // VM.AlphaThreshold (byte) -> lblAlphaThreshold.Text (string)
            commandBindings.AddPropertyBinding(viewModel, nameof(viewModel.AlphaThreshold), nameof(lblAlphaThresholdValue.Text), b => $"{b}", lblAlphaThresholdValue);

            // VM.AlphaThresholdEnabled -> tblAlphaThreshold.Enabled
            commandBindings.AddPropertyBinding(viewModel, nameof(viewModel.AlphaThresholdEnabled), nameof(tblAlphaThreshold.Enabled), tblAlphaThreshold);

            // chbDitherer.Checked -> VM.UseDithering
            commandBindings.AddPropertyBinding(chbDitherer, nameof(chbDitherer.Checked), nameof(viewModel.UseDithering), viewModel);

            // VM.Ditherers -> cmbDitherer.DataSource (once)
            cmbDitherer.DataSource = viewModel.Ditherers;

            // VM.SelectedDitherer -> cmbDitherer.SelectedItem (cannot use two-way for SelectedItem because there is no SelectedItemChanged event)
            commandBindings.AddPropertyBinding(viewModel, nameof(viewModel.SelectedDitherer), nameof(cmbDitherer.SelectedItem), cmbDitherer);

            // VM.SelectedDitherer <- cmbDitherer.SelectedValue (cannot use two-way for SelectedValue because ValueMember is not set)
            commandBindings.AddPropertyBinding(cmbDitherer, nameof(cmbDitherer.SelectedValue), nameof(viewModel.SelectedDitherer), viewModel);

            // VM.DisplayImage -> pbImage.Image
            commandBindings.AddPropertyBinding(viewModel, nameof(viewModel.DisplayImage), nameof(pbImage.Image), pbImage);
        }

        private void InitCommandBindings()
        {
            // btnBackColor.Click -> OnPickBackColorCommand
            commandBindings.Add(OnPickBackColorCommand)
                .AddSource(btnBackColor, nameof(btnBackColor.Click));
            
            // timerProgress.Tick -> VM.UpdateProgressCommand
            commandBindings.Add(viewModel.UpdateProgressCommand)
                .AddSource(timerProgress, nameof(timerProgress.Tick));
        }

        #endregion

        #region Command Handlers

        private void OnPickBackColorCommand()
        {
            colorDialog.Color = viewModel.BackColor;
            if (colorDialog.ShowDialog(this) == DialogResult.OK)
                viewModel.BackColor = colorDialog.Color;
        }

        #endregion

        #endregion
    }
}
