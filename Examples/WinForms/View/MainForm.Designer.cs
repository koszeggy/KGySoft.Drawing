using System.Windows.Forms;

namespace KGySoft.Drawing.Examples.WinForms.View
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            tblContent = new TableLayoutPanel();
            tblOverlayShape = new TableLayoutPanel();
            cmbOverlayShape = new ComboBox();
            lblOverlayShape = new Label();
            tblOutline = new TableLayoutPanel();
            pnlOutline = new Panel();
            numOutline = new NumericUpDown();
            lblOutline = new Label();
            btnOutlineColor = new Button();
            pnlOutlineColor = new Panel();
            chbForceLinear = new CheckBox();
            pbImage = new PictureBox();
            lblImageFile = new Label();
            lblPixelFormat = new Label();
            chbImageOverlay = new CheckBox();
            txtImageFile = new TextBox();
            txtImageOverlay = new TextBox();
            cmbPixelFormat = new ComboBox();
            chbOptimizePalette = new CheckBox();
            tblBackColor = new TableLayoutPanel();
            btnBackColor = new Button();
            pnlBackColor = new Panel();
            lblBackColor = new Label();
            chbDitherer = new CheckBox();
            cmbDitherer = new ComboBox();
            tblAlphaThreshold = new TableLayoutPanel();
            lblAlphaThresholdValue = new Label();
            lblAlphaThreshold = new Label();
            tbAlphaThreshold = new TrackBar();
            toolTip = new ToolTip(components);
            errorProvider = new ErrorProvider(components);
            ssStatus = new StatusStrip();
            lblProgress = new ToolStripStatusLabel();
            pbProgress = new ToolStripProgressBar();
            timerProgress = new Timer(components);
            colorDialog = new ColorDialog();
            tblContent.SuspendLayout();
            tblOverlayShape.SuspendLayout();
            tblOutline.SuspendLayout();
            pnlOutline.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numOutline).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pbImage).BeginInit();
            tblBackColor.SuspendLayout();
            tblAlphaThreshold.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)tbAlphaThreshold).BeginInit();
            ((System.ComponentModel.ISupportInitialize)errorProvider).BeginInit();
            ssStatus.SuspendLayout();
            SuspendLayout();
            // 
            // tblContent
            // 
            tblContent.ColumnCount = 2;
            tblContent.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120F));
            tblContent.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tblContent.Controls.Add(tblOverlayShape, 1, 2);
            tblContent.Controls.Add(tblOutline, 1, 3);
            tblContent.Controls.Add(chbForceLinear, 1, 5);
            tblContent.Controls.Add(pbImage, 0, 10);
            tblContent.Controls.Add(lblImageFile, 0, 0);
            tblContent.Controls.Add(lblPixelFormat, 0, 4);
            tblContent.Controls.Add(chbImageOverlay, 0, 1);
            tblContent.Controls.Add(txtImageFile, 1, 0);
            tblContent.Controls.Add(txtImageOverlay, 1, 1);
            tblContent.Controls.Add(cmbPixelFormat, 1, 4);
            tblContent.Controls.Add(chbOptimizePalette, 1, 6);
            tblContent.Controls.Add(tblBackColor, 1, 7);
            tblContent.Controls.Add(chbDitherer, 0, 9);
            tblContent.Controls.Add(cmbDitherer, 1, 9);
            tblContent.Controls.Add(tblAlphaThreshold, 1, 8);
            tblContent.Dock = DockStyle.Fill;
            tblContent.Location = new System.Drawing.Point(0, 0);
            tblContent.Name = "tblContent";
            tblContent.Padding = new Padding(3, 0, 3, 0);
            tblContent.RowCount = 11;
            tblContent.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            tblContent.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            tblContent.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            tblContent.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            tblContent.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            tblContent.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            tblContent.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            tblContent.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            tblContent.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            tblContent.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            tblContent.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tblContent.Size = new System.Drawing.Size(584, 439);
            tblContent.TabIndex = 0;
            // 
            // tblOverlayShape
            // 
            tblOverlayShape.ColumnCount = 2;
            tblOverlayShape.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120F));
            tblOverlayShape.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tblOverlayShape.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20F));
            tblOverlayShape.Controls.Add(cmbOverlayShape, 1, 0);
            tblOverlayShape.Controls.Add(lblOverlayShape, 0, 0);
            tblOverlayShape.Dock = DockStyle.Fill;
            tblOverlayShape.Location = new System.Drawing.Point(126, 56);
            tblOverlayShape.Margin = new Padding(3, 0, 0, 0);
            tblOverlayShape.Name = "tblOverlayShape";
            tblOverlayShape.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tblOverlayShape.Size = new System.Drawing.Size(455, 28);
            tblOverlayShape.TabIndex = 4;
            // 
            // cmbOverlayShape
            // 
            cmbOverlayShape.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            cmbOverlayShape.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbOverlayShape.FlatStyle = FlatStyle.System;
            cmbOverlayShape.FormattingEnabled = true;
            cmbOverlayShape.Location = new System.Drawing.Point(123, 3);
            cmbOverlayShape.Name = "cmbOverlayShape";
            cmbOverlayShape.Size = new System.Drawing.Size(329, 23);
            cmbOverlayShape.TabIndex = 1;
            toolTip.SetToolTip(cmbOverlayShape, "An optional shape for the overlay image.");
            // 
            // lblOverlayShape
            // 
            lblOverlayShape.Anchor = AnchorStyles.Left;
            lblOverlayShape.AutoSize = true;
            lblOverlayShape.FlatStyle = FlatStyle.System;
            lblOverlayShape.Location = new System.Drawing.Point(3, 6);
            lblOverlayShape.Name = "lblOverlayShape";
            lblOverlayShape.Size = new System.Drawing.Size(85, 15);
            lblOverlayShape.TabIndex = 0;
            lblOverlayShape.Text = "Overlay Shape:";
            lblOverlayShape.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tblOutline
            // 
            tblOutline.Anchor = AnchorStyles.Left;
            tblOutline.ColumnCount = 3;
            tblOutline.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120F));
            tblOutline.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120F));
            tblOutline.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tblOutline.Controls.Add(pnlOutline, 0, 0);
            tblOutline.Controls.Add(btnOutlineColor, 2, 0);
            tblOutline.Controls.Add(pnlOutlineColor, 1, 0);
            tblOutline.Location = new System.Drawing.Point(126, 87);
            tblOutline.Name = "tblOutline";
            tblOutline.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tblOutline.Size = new System.Drawing.Size(448, 22);
            tblOutline.TabIndex = 5;
            // 
            // pnlOutline
            // 
            pnlOutline.Controls.Add(numOutline);
            pnlOutline.Controls.Add(lblOutline);
            pnlOutline.Dock = DockStyle.Fill;
            pnlOutline.Location = new System.Drawing.Point(3, 0);
            pnlOutline.Margin = new Padding(3, 0, 3, 0);
            pnlOutline.Name = "pnlOutline";
            pnlOutline.Size = new System.Drawing.Size(114, 22);
            pnlOutline.TabIndex = 0;
            // 
            // numOutline
            // 
            numOutline.Dock = DockStyle.Right;
            numOutline.Location = new System.Drawing.Point(62, 0);
            numOutline.Maximum = new decimal(new int[] { 10, 0, 0, 0 });
            numOutline.Name = "numOutline";
            numOutline.Size = new System.Drawing.Size(52, 23);
            numOutline.TabIndex = 1;
            toolTip.SetToolTip(numOutline, "The outline width of the overlay shape.");
            // 
            // lblOutline
            // 
            lblOutline.Anchor = AnchorStyles.Left;
            lblOutline.AutoSize = true;
            lblOutline.FlatStyle = FlatStyle.System;
            lblOutline.Location = new System.Drawing.Point(0, 3);
            lblOutline.Name = "lblOutline";
            lblOutline.Size = new System.Drawing.Size(49, 15);
            lblOutline.TabIndex = 0;
            lblOutline.Text = "Outline:";
            lblOutline.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnOutlineColor
            // 
            btnOutlineColor.AutoSize = true;
            btnOutlineColor.Dock = DockStyle.Left;
            btnOutlineColor.FlatStyle = FlatStyle.System;
            btnOutlineColor.Location = new System.Drawing.Point(240, 0);
            btnOutlineColor.Margin = new Padding(0);
            btnOutlineColor.Name = "btnOutlineColor";
            btnOutlineColor.Size = new System.Drawing.Size(75, 22);
            btnOutlineColor.TabIndex = 2;
            btnOutlineColor.Text = "Pick Color";
            toolTip.SetToolTip(btnOutlineColor, "When there is a selected overlay shape and the outline width is larger than zero, picks a color for the shape outline.");
            btnOutlineColor.UseVisualStyleBackColor = true;
            // 
            // pnlOutlineColor
            // 
            pnlOutlineColor.BorderStyle = BorderStyle.FixedSingle;
            pnlOutlineColor.Dock = DockStyle.Fill;
            pnlOutlineColor.Location = new System.Drawing.Point(123, 0);
            pnlOutlineColor.Margin = new Padding(3, 0, 3, 0);
            pnlOutlineColor.Name = "pnlOutlineColor";
            pnlOutlineColor.Size = new System.Drawing.Size(114, 22);
            pnlOutlineColor.TabIndex = 1;
            // 
            // chbForceLinear
            // 
            chbForceLinear.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            chbForceLinear.AutoSize = true;
            chbForceLinear.FlatStyle = FlatStyle.System;
            chbForceLinear.Location = new System.Drawing.Point(126, 144);
            chbForceLinear.Name = "chbForceLinear";
            chbForceLinear.Size = new System.Drawing.Size(452, 20);
            chbForceLinear.TabIndex = 8;
            chbForceLinear.Text = "Force Linear Color Space";
            toolTip.SetToolTip(chbForceLinear, resources.GetString("chbForceLinear.ToolTip"));
            chbForceLinear.UseVisualStyleBackColor = true;
            // 
            // pbImage
            // 
            tblContent.SetColumnSpan(pbImage, 2);
            pbImage.Dock = DockStyle.Fill;
            pbImage.Location = new System.Drawing.Point(6, 283);
            pbImage.Name = "pbImage";
            pbImage.Size = new System.Drawing.Size(572, 153);
            pbImage.SizeMode = PictureBoxSizeMode.Zoom;
            pbImage.TabIndex = 11;
            pbImage.TabStop = false;
            // 
            // lblImageFile
            // 
            lblImageFile.Anchor = AnchorStyles.Left;
            lblImageFile.AutoSize = true;
            lblImageFile.FlatStyle = FlatStyle.System;
            lblImageFile.Location = new System.Drawing.Point(6, 6);
            lblImageFile.Name = "lblImageFile";
            lblImageFile.Size = new System.Drawing.Size(64, 15);
            lblImageFile.TabIndex = 0;
            lblImageFile.Text = "Image File:";
            // 
            // lblPixelFormat
            // 
            lblPixelFormat.Anchor = AnchorStyles.Left;
            lblPixelFormat.AutoSize = true;
            lblPixelFormat.FlatStyle = FlatStyle.System;
            lblPixelFormat.Location = new System.Drawing.Point(6, 118);
            lblPixelFormat.Name = "lblPixelFormat";
            lblPixelFormat.Size = new System.Drawing.Size(76, 15);
            lblPixelFormat.TabIndex = 6;
            lblPixelFormat.Text = "Pixel Format:";
            // 
            // chbImageOverlay
            // 
            chbImageOverlay.Anchor = AnchorStyles.Left;
            chbImageOverlay.AutoSize = true;
            chbImageOverlay.FlatStyle = FlatStyle.System;
            chbImageOverlay.Location = new System.Drawing.Point(6, 32);
            chbImageOverlay.Name = "chbImageOverlay";
            chbImageOverlay.Size = new System.Drawing.Size(111, 20);
            chbImageOverlay.TabIndex = 2;
            chbImageOverlay.Text = "Image Overlay:";
            toolTip.SetToolTip(chbImageOverlay, "Check to blend an overlay image with the base image");
            chbImageOverlay.UseVisualStyleBackColor = true;
            // 
            // txtImageFile
            // 
            txtImageFile.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            txtImageFile.AutoCompleteMode = AutoCompleteMode.Suggest;
            txtImageFile.AutoCompleteSource = AutoCompleteSource.FileSystem;
            errorProvider.SetIconAlignment(txtImageFile, ErrorIconAlignment.MiddleLeft);
            txtImageFile.Location = new System.Drawing.Point(126, 3);
            txtImageFile.Name = "txtImageFile";
            txtImageFile.Size = new System.Drawing.Size(452, 23);
            txtImageFile.TabIndex = 1;
            toolTip.SetToolTip(txtImageFile, "The base image file to display");
            // 
            // txtImageOverlay
            // 
            txtImageOverlay.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            txtImageOverlay.AutoCompleteMode = AutoCompleteMode.Suggest;
            txtImageOverlay.AutoCompleteSource = AutoCompleteSource.FileSystem;
            errorProvider.SetIconAlignment(txtImageOverlay, ErrorIconAlignment.MiddleLeft);
            txtImageOverlay.Location = new System.Drawing.Point(126, 31);
            txtImageOverlay.Name = "txtImageOverlay";
            txtImageOverlay.Size = new System.Drawing.Size(452, 23);
            txtImageOverlay.TabIndex = 3;
            toolTip.SetToolTip(txtImageOverlay, "The overlay image to display");
            // 
            // cmbPixelFormat
            // 
            cmbPixelFormat.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            cmbPixelFormat.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbPixelFormat.FlatStyle = FlatStyle.System;
            cmbPixelFormat.FormattingEnabled = true;
            cmbPixelFormat.Location = new System.Drawing.Point(126, 115);
            cmbPixelFormat.Name = "cmbPixelFormat";
            cmbPixelFormat.Size = new System.Drawing.Size(452, 23);
            cmbPixelFormat.TabIndex = 7;
            toolTip.SetToolTip(cmbPixelFormat, "The desired target pixel format. For lower bit-per-pixel formats it is recommended to enable dithering.");
            // 
            // chbOptimizePalette
            // 
            chbOptimizePalette.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            chbOptimizePalette.AutoSize = true;
            chbOptimizePalette.FlatStyle = FlatStyle.System;
            chbOptimizePalette.Location = new System.Drawing.Point(126, 172);
            chbOptimizePalette.Name = "chbOptimizePalette";
            chbOptimizePalette.Size = new System.Drawing.Size(452, 20);
            chbOptimizePalette.TabIndex = 9;
            chbOptimizePalette.Text = "Optimize Palette";
            toolTip.SetToolTip(chbOptimizePalette, "When an indexed pixel format is selected, check to use an optimized palette instead of a predefined one.");
            chbOptimizePalette.UseVisualStyleBackColor = true;
            // 
            // tblBackColor
            // 
            tblBackColor.Anchor = AnchorStyles.Left;
            tblBackColor.ColumnCount = 3;
            tblBackColor.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120F));
            tblBackColor.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120F));
            tblBackColor.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tblBackColor.Controls.Add(btnBackColor, 2, 0);
            tblBackColor.Controls.Add(pnlBackColor, 1, 0);
            tblBackColor.Controls.Add(lblBackColor, 0, 0);
            tblBackColor.Location = new System.Drawing.Point(126, 199);
            tblBackColor.Name = "tblBackColor";
            tblBackColor.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tblBackColor.Size = new System.Drawing.Size(448, 22);
            tblBackColor.TabIndex = 10;
            // 
            // btnBackColor
            // 
            btnBackColor.AutoSize = true;
            btnBackColor.Dock = DockStyle.Left;
            btnBackColor.FlatStyle = FlatStyle.System;
            btnBackColor.Location = new System.Drawing.Point(240, 0);
            btnBackColor.Margin = new Padding(0);
            btnBackColor.Name = "btnBackColor";
            btnBackColor.Size = new System.Drawing.Size(75, 22);
            btnBackColor.TabIndex = 2;
            btnBackColor.Text = "Pick Color";
            toolTip.SetToolTip(btnBackColor, resources.GetString("btnBackColor.ToolTip"));
            btnBackColor.UseVisualStyleBackColor = true;
            // 
            // pnlBackColor
            // 
            pnlBackColor.BorderStyle = BorderStyle.FixedSingle;
            pnlBackColor.Dock = DockStyle.Fill;
            pnlBackColor.Location = new System.Drawing.Point(123, 0);
            pnlBackColor.Margin = new Padding(3, 0, 3, 0);
            pnlBackColor.Name = "pnlBackColor";
            pnlBackColor.Size = new System.Drawing.Size(114, 22);
            pnlBackColor.TabIndex = 1;
            // 
            // lblBackColor
            // 
            lblBackColor.Anchor = AnchorStyles.Left;
            lblBackColor.AutoSize = true;
            lblBackColor.FlatStyle = FlatStyle.System;
            lblBackColor.Location = new System.Drawing.Point(3, 3);
            lblBackColor.Name = "lblBackColor";
            lblBackColor.Size = new System.Drawing.Size(67, 15);
            lblBackColor.TabIndex = 0;
            lblBackColor.Text = "Back Color:";
            lblBackColor.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // chbDitherer
            // 
            chbDitherer.Anchor = AnchorStyles.Left;
            chbDitherer.AutoSize = true;
            chbDitherer.FlatStyle = FlatStyle.System;
            chbDitherer.Location = new System.Drawing.Point(6, 256);
            chbDitherer.Name = "chbDitherer";
            chbDitherer.Size = new System.Drawing.Size(77, 20);
            chbDitherer.TabIndex = 12;
            chbDitherer.Text = "Ditherer:";
            toolTip.SetToolTip(chbDitherer, "Check to use a ditherer. For high bit-per-pixel formats it makes little sense as for those its only practical effect is just removing possible partial transparency.");
            chbDitherer.UseVisualStyleBackColor = true;
            // 
            // cmbDitherer
            // 
            cmbDitherer.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            cmbDitherer.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbDitherer.FlatStyle = FlatStyle.System;
            cmbDitherer.FormattingEnabled = true;
            cmbDitherer.Location = new System.Drawing.Point(126, 255);
            cmbDitherer.Name = "cmbDitherer";
            cmbDitherer.Size = new System.Drawing.Size(452, 23);
            cmbDitherer.TabIndex = 13;
            toolTip.SetToolTip(cmbDitherer, resources.GetString("cmbDitherer.ToolTip"));
            // 
            // tblAlphaThreshold
            // 
            tblAlphaThreshold.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            tblAlphaThreshold.ColumnCount = 3;
            tblAlphaThreshold.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120F));
            tblAlphaThreshold.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120F));
            tblAlphaThreshold.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tblAlphaThreshold.Controls.Add(lblAlphaThresholdValue, 2, 0);
            tblAlphaThreshold.Controls.Add(lblAlphaThreshold, 0, 0);
            tblAlphaThreshold.Controls.Add(tbAlphaThreshold, 1, 0);
            tblAlphaThreshold.Location = new System.Drawing.Point(126, 227);
            tblAlphaThreshold.Name = "tblAlphaThreshold";
            tblAlphaThreshold.RowCount = 1;
            tblAlphaThreshold.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tblAlphaThreshold.Size = new System.Drawing.Size(452, 22);
            tblAlphaThreshold.TabIndex = 11;
            // 
            // lblAlphaThresholdValue
            // 
            lblAlphaThresholdValue.Anchor = AnchorStyles.Left;
            lblAlphaThresholdValue.AutoSize = true;
            lblAlphaThresholdValue.FlatStyle = FlatStyle.System;
            lblAlphaThresholdValue.Location = new System.Drawing.Point(243, 3);
            lblAlphaThresholdValue.Name = "lblAlphaThresholdValue";
            lblAlphaThresholdValue.Size = new System.Drawing.Size(13, 15);
            lblAlphaThresholdValue.TabIndex = 2;
            lblAlphaThresholdValue.Text = "0";
            lblAlphaThresholdValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblAlphaThreshold
            // 
            lblAlphaThreshold.Anchor = AnchorStyles.Left;
            lblAlphaThreshold.AutoSize = true;
            lblAlphaThreshold.FlatStyle = FlatStyle.System;
            lblAlphaThreshold.Location = new System.Drawing.Point(3, 3);
            lblAlphaThreshold.Name = "lblAlphaThreshold";
            lblAlphaThreshold.Size = new System.Drawing.Size(96, 15);
            lblAlphaThreshold.TabIndex = 0;
            lblAlphaThreshold.Text = "Alpha Threshold:";
            lblAlphaThreshold.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tbAlphaThreshold
            // 
            tbAlphaThreshold.Dock = DockStyle.Fill;
            tbAlphaThreshold.LargeChange = 64;
            tbAlphaThreshold.Location = new System.Drawing.Point(120, 0);
            tbAlphaThreshold.Margin = new Padding(0);
            tbAlphaThreshold.Maximum = 255;
            tbAlphaThreshold.Name = "tbAlphaThreshold";
            tbAlphaThreshold.Size = new System.Drawing.Size(120, 22);
            tbAlphaThreshold.TabIndex = 1;
            tbAlphaThreshold.TickFrequency = 16;
            toolTip.SetToolTip(tbAlphaThreshold, resources.GetString("tbAlphaThreshold.ToolTip"));
            // 
            // errorProvider
            // 
            errorProvider.ContainerControl = this;
            // 
            // ssStatus
            // 
            ssStatus.GripStyle = ToolStripGripStyle.Visible;
            ssStatus.Items.AddRange(new ToolStripItem[] { lblProgress, pbProgress });
            ssStatus.Location = new System.Drawing.Point(0, 439);
            ssStatus.Name = "ssStatus";
            ssStatus.Size = new System.Drawing.Size(584, 22);
            ssStatus.TabIndex = 1;
            // 
            // lblProgress
            // 
            lblProgress.Name = "lblProgress";
            lblProgress.Size = new System.Drawing.Size(52, 17);
            lblProgress.Text = "Progress";
            lblProgress.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // pbProgress
            // 
            pbProgress.AutoSize = false;
            pbProgress.Name = "pbProgress";
            pbProgress.Size = new System.Drawing.Size(100, 16);
            // 
            // timerProgress
            // 
            timerProgress.Interval = 30;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(584, 461);
            Controls.Add(tblContent);
            Controls.Add(ssStatus);
            MinimumSize = new System.Drawing.Size(480, 400);
            Name = "MainForm";
            SizeGripStyle = SizeGripStyle.Hide;
            Text = "KGy SOFT Drawing WinForms Example App";
            tblContent.ResumeLayout(false);
            tblContent.PerformLayout();
            tblOverlayShape.ResumeLayout(false);
            tblOverlayShape.PerformLayout();
            tblOutline.ResumeLayout(false);
            tblOutline.PerformLayout();
            pnlOutline.ResumeLayout(false);
            pnlOutline.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numOutline).EndInit();
            ((System.ComponentModel.ISupportInitialize)pbImage).EndInit();
            tblBackColor.ResumeLayout(false);
            tblBackColor.PerformLayout();
            tblAlphaThreshold.ResumeLayout(false);
            tblAlphaThreshold.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)tbAlphaThreshold).EndInit();
            ((System.ComponentModel.ISupportInitialize)errorProvider).EndInit();
            ssStatus.ResumeLayout(false);
            ssStatus.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TableLayoutPanel tblContent;
        private Label lblImageFile;
        private Label lblPixelFormat;
        private CheckBox chbImageOverlay;
        private TextBox txtImageFile;
        private TextBox txtImageOverlay;
        private ComboBox cmbPixelFormat;
        private CheckBox chbOptimizePalette;
        private TableLayoutPanel tblBackColor;
        private CheckBox chbDitherer;
        private ComboBox cmbDitherer;
        private TableLayoutPanel tblAlphaThreshold;
        private PictureBox pbImage;
        private Label lblAlphaThreshold;
        private Button btnBackColor;
        private Panel pnlBackColor;
        private Label lblAlphaThresholdValue;
        private TrackBar tbAlphaThreshold;
        private ToolTip toolTip;
        private ErrorProvider errorProvider;
        private StatusStrip ssStatus;
        private ToolStripStatusLabel lblProgress;
        private ToolStripProgressBar pbProgress;
        private System.Windows.Forms.Timer timerProgress;
        private ColorDialog colorDialog;
        private CheckBox chbForceLinear;
        private TableLayoutPanel tblOutline;
        private Button btnOutlineColor;
        private Panel pnlOutlineColor;
        private Panel pnlOutline;
        private NumericUpDown numOutline;
        private Label lblOutline;
        private Label lblBackColor;
        private TableLayoutPanel tblOverlayShape;
        private Label lblOverlayShape;
        private ComboBox cmbOverlayShape;
    }
}