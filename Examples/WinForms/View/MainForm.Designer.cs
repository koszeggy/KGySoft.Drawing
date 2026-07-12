using System.Windows.Forms;

using KGySoft.WinForms.Components;

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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.tblContent = new System.Windows.Forms.TableLayoutPanel();
            this.tblOverlayShape = new System.Windows.Forms.TableLayoutPanel();
            this.cmbOverlayShape = new System.Windows.Forms.ComboBox();
            this.lblOverlayShape = new System.Windows.Forms.Label();
            this.tblOutline = new System.Windows.Forms.TableLayoutPanel();
            this.pnlOutline = new System.Windows.Forms.Panel();
            this.numOutline = new System.Windows.Forms.NumericUpDown();
            this.lblOutline = new System.Windows.Forms.Label();
            this.btnOutlineColor = new System.Windows.Forms.Button();
            this.pnlOutlineColor = new System.Windows.Forms.Panel();
            this.chbForceLinear = new System.Windows.Forms.CheckBox();
            this.pbImage = new System.Windows.Forms.PictureBox();
            this.lblImageFile = new System.Windows.Forms.Label();
            this.lblPixelFormat = new System.Windows.Forms.Label();
            this.chbImageOverlay = new System.Windows.Forms.CheckBox();
            this.txtImageFile = new System.Windows.Forms.TextBox();
            this.txtImageOverlay = new System.Windows.Forms.TextBox();
            this.cmbPixelFormat = new System.Windows.Forms.ComboBox();
            this.chbOptimizePalette = new System.Windows.Forms.CheckBox();
            this.tblBackColor = new System.Windows.Forms.TableLayoutPanel();
            this.btnBackColor = new System.Windows.Forms.Button();
            this.pnlBackColor = new System.Windows.Forms.Panel();
            this.lblBackColor = new System.Windows.Forms.Label();
            this.chbDitherer = new System.Windows.Forms.CheckBox();
            this.cmbDitherer = new System.Windows.Forms.ComboBox();
            this.tblAlphaThreshold = new System.Windows.Forms.TableLayoutPanel();
            this.lblAlphaThresholdValue = new System.Windows.Forms.Label();
            this.lblAlphaThreshold = new System.Windows.Forms.Label();
            this.tbAlphaThreshold = new System.Windows.Forms.TrackBar();
            this.errorProvider = new KGySoft.WinForms.Components.AdvancedErrorProvider(this.components);
            this.ssStatus = new System.Windows.Forms.StatusStrip();
            this.lblProgress = new System.Windows.Forms.ToolStripStatusLabel();
            this.pbProgress = new System.Windows.Forms.ToolStripProgressBar();
            this.timerProgress = new System.Windows.Forms.Timer(this.components);
            this.colorDialog = new System.Windows.Forms.ColorDialog();
            this.tblContent.SuspendLayout();
            this.tblOverlayShape.SuspendLayout();
            this.tblOutline.SuspendLayout();
            this.pnlOutline.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numOutline)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbImage)).BeginInit();
            this.tblBackColor.SuspendLayout();
            this.tblAlphaThreshold.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tbAlphaThreshold)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).BeginInit();
            this.ssStatus.SuspendLayout();
            this.SuspendLayout();
            // 
            // tblContent
            // 
            this.tblContent.ColumnCount = 2;
            this.tblContent.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 103F));
            this.tblContent.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tblContent.Controls.Add(this.tblOverlayShape, 1, 2);
            this.tblContent.Controls.Add(this.tblOutline, 1, 3);
            this.tblContent.Controls.Add(this.chbForceLinear, 1, 5);
            this.tblContent.Controls.Add(this.pbImage, 0, 10);
            this.tblContent.Controls.Add(this.lblImageFile, 0, 0);
            this.tblContent.Controls.Add(this.lblPixelFormat, 0, 4);
            this.tblContent.Controls.Add(this.chbImageOverlay, 0, 1);
            this.tblContent.Controls.Add(this.txtImageFile, 1, 0);
            this.tblContent.Controls.Add(this.txtImageOverlay, 1, 1);
            this.tblContent.Controls.Add(this.cmbPixelFormat, 1, 4);
            this.tblContent.Controls.Add(this.chbOptimizePalette, 1, 6);
            this.tblContent.Controls.Add(this.tblBackColor, 1, 7);
            this.tblContent.Controls.Add(this.chbDitherer, 0, 9);
            this.tblContent.Controls.Add(this.cmbDitherer, 1, 9);
            this.tblContent.Controls.Add(this.tblAlphaThreshold, 1, 8);
            this.tblContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tblContent.Location = new System.Drawing.Point(0, 0);
            this.tblContent.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.tblContent.Name = "tblContent";
            this.tblContent.Padding = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.tblContent.RowCount = 11;
            this.tblContent.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
            this.tblContent.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
            this.tblContent.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
            this.tblContent.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
            this.tblContent.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
            this.tblContent.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
            this.tblContent.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
            this.tblContent.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
            this.tblContent.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
            this.tblContent.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
            this.tblContent.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tblContent.Size = new System.Drawing.Size(500, 377);
            this.tblContent.TabIndex = 0;
            // 
            // tblOverlayShape
            // 
            this.tblOverlayShape.ColumnCount = 2;
            this.tblOverlayShape.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 103F));
            this.tblOverlayShape.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tblOverlayShape.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 17F));
            this.tblOverlayShape.Controls.Add(this.cmbOverlayShape, 1, 0);
            this.tblOverlayShape.Controls.Add(this.lblOverlayShape, 0, 0);
            this.tblOverlayShape.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tblOverlayShape.Location = new System.Drawing.Point(107, 48);
            this.tblOverlayShape.Margin = new System.Windows.Forms.Padding(2, 0, 0, 0);
            this.tblOverlayShape.Name = "tblOverlayShape";
            this.tblOverlayShape.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tblOverlayShape.Size = new System.Drawing.Size(391, 24);
            this.tblOverlayShape.TabIndex = 4;
            // 
            // cmbOverlayShape
            // 
            this.cmbOverlayShape.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbOverlayShape.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbOverlayShape.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.cmbOverlayShape.FormattingEnabled = true;
            this.cmbOverlayShape.Location = new System.Drawing.Point(105, 3);
            this.cmbOverlayShape.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.cmbOverlayShape.Name = "cmbOverlayShape";
            this.cmbOverlayShape.Size = new System.Drawing.Size(284, 21);
            this.cmbOverlayShape.TabIndex = 1;
            this.BaseToolTip.SetToolTip(this.cmbOverlayShape, "An optional shape for the overlay image.");
            // 
            // lblOverlayShape
            // 
            this.lblOverlayShape.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblOverlayShape.AutoSize = true;
            this.lblOverlayShape.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.lblOverlayShape.Location = new System.Drawing.Point(2, 5);
            this.lblOverlayShape.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblOverlayShape.Name = "lblOverlayShape";
            this.lblOverlayShape.Size = new System.Drawing.Size(80, 13);
            this.lblOverlayShape.TabIndex = 0;
            this.lblOverlayShape.Text = "Overlay Shape:";
            this.lblOverlayShape.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tblOutline
            // 
            this.tblOutline.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.tblOutline.ColumnCount = 3;
            this.tblOutline.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 103F));
            this.tblOutline.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 103F));
            this.tblOutline.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tblOutline.Controls.Add(this.pnlOutline, 0, 0);
            this.tblOutline.Controls.Add(this.btnOutlineColor, 2, 0);
            this.tblOutline.Controls.Add(this.pnlOutlineColor, 1, 0);
            this.tblOutline.Location = new System.Drawing.Point(107, 75);
            this.tblOutline.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.tblOutline.Name = "tblOutline";
            this.tblOutline.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tblOutline.Size = new System.Drawing.Size(384, 18);
            this.tblOutline.TabIndex = 5;
            // 
            // pnlOutline
            // 
            this.pnlOutline.Controls.Add(this.numOutline);
            this.pnlOutline.Controls.Add(this.lblOutline);
            this.pnlOutline.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlOutline.Location = new System.Drawing.Point(2, 0);
            this.pnlOutline.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.pnlOutline.Name = "pnlOutline";
            this.pnlOutline.Size = new System.Drawing.Size(99, 18);
            this.pnlOutline.TabIndex = 0;
            // 
            // numOutline
            // 
            this.numOutline.Dock = System.Windows.Forms.DockStyle.Right;
            this.numOutline.Location = new System.Drawing.Point(55, 0);
            this.numOutline.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.numOutline.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numOutline.Name = "numOutline";
            this.numOutline.Size = new System.Drawing.Size(44, 20);
            this.numOutline.TabIndex = 1;
            this.BaseToolTip.SetToolTip(this.numOutline, "The outline width of the overlay shape.");
            // 
            // lblOutline
            // 
            this.lblOutline.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblOutline.AutoSize = true;
            this.lblOutline.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.lblOutline.Location = new System.Drawing.Point(0, 3);
            this.lblOutline.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblOutline.Name = "lblOutline";
            this.lblOutline.Size = new System.Drawing.Size(43, 13);
            this.lblOutline.TabIndex = 0;
            this.lblOutline.Text = "Outline:";
            this.lblOutline.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnOutlineColor
            // 
            this.btnOutlineColor.AutoSize = true;
            this.btnOutlineColor.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnOutlineColor.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnOutlineColor.Location = new System.Drawing.Point(206, 0);
            this.btnOutlineColor.Margin = new System.Windows.Forms.Padding(0);
            this.btnOutlineColor.Name = "btnOutlineColor";
            this.btnOutlineColor.Size = new System.Drawing.Size(90, 18);
            this.btnOutlineColor.TabIndex = 2;
            this.btnOutlineColor.Text = "Pick Color";
            this.BaseToolTip.SetToolTip(this.btnOutlineColor, "When there is a selected overlay shape and the outline width is larger than zero," +
        " picks a color for the shape outline.");
            this.btnOutlineColor.UseVisualStyleBackColor = true;
            // 
            // pnlOutlineColor
            // 
            this.pnlOutlineColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlOutlineColor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlOutlineColor.Location = new System.Drawing.Point(105, 0);
            this.pnlOutlineColor.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.pnlOutlineColor.Name = "pnlOutlineColor";
            this.pnlOutlineColor.Size = new System.Drawing.Size(99, 18);
            this.pnlOutlineColor.TabIndex = 1;
            // 
            // chbForceLinear
            // 
            this.chbForceLinear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.chbForceLinear.AutoSize = true;
            this.chbForceLinear.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.chbForceLinear.Location = new System.Drawing.Point(107, 123);
            this.chbForceLinear.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.chbForceLinear.Name = "chbForceLinear";
            this.chbForceLinear.Size = new System.Drawing.Size(389, 18);
            this.chbForceLinear.TabIndex = 8;
            this.chbForceLinear.Text = "Force Linear Color Space";
            this.BaseToolTip.SetToolTip(this.chbForceLinear, resources.GetString("chbForceLinear.ToolTip"));
            this.chbForceLinear.UseVisualStyleBackColor = true;
            // 
            // pbImage
            // 
            this.tblContent.SetColumnSpan(this.pbImage, 2);
            this.pbImage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pbImage.Location = new System.Drawing.Point(4, 243);
            this.pbImage.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.pbImage.Name = "pbImage";
            this.pbImage.Size = new System.Drawing.Size(492, 131);
            this.pbImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pbImage.TabIndex = 11;
            this.pbImage.TabStop = false;
            // 
            // lblImageFile
            // 
            this.lblImageFile.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblImageFile.AutoSize = true;
            this.lblImageFile.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.lblImageFile.Location = new System.Drawing.Point(4, 5);
            this.lblImageFile.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblImageFile.Name = "lblImageFile";
            this.lblImageFile.Size = new System.Drawing.Size(58, 13);
            this.lblImageFile.TabIndex = 0;
            this.lblImageFile.Text = "Image File:";
            // 
            // lblPixelFormat
            // 
            this.lblPixelFormat.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblPixelFormat.AutoSize = true;
            this.lblPixelFormat.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.lblPixelFormat.Location = new System.Drawing.Point(4, 101);
            this.lblPixelFormat.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblPixelFormat.Name = "lblPixelFormat";
            this.lblPixelFormat.Size = new System.Drawing.Size(67, 13);
            this.lblPixelFormat.TabIndex = 6;
            this.lblPixelFormat.Text = "Pixel Format:";
            // 
            // chbImageOverlay
            // 
            this.chbImageOverlay.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chbImageOverlay.AutoSize = true;
            this.chbImageOverlay.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.chbImageOverlay.Location = new System.Drawing.Point(4, 27);
            this.chbImageOverlay.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.chbImageOverlay.Name = "chbImageOverlay";
            this.chbImageOverlay.Size = new System.Drawing.Size(99, 18);
            this.chbImageOverlay.TabIndex = 2;
            this.chbImageOverlay.Text = "Image Overlay:";
            this.BaseToolTip.SetToolTip(this.chbImageOverlay, "Check to blend an overlay image with the base image");
            this.chbImageOverlay.UseVisualStyleBackColor = true;
            // 
            // txtImageFile
            // 
            this.txtImageFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtImageFile.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.txtImageFile.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.FileSystem;
            this.errorProvider.SetIconAlignment(this.txtImageFile, System.Windows.Forms.ErrorIconAlignment.MiddleLeft);
            this.txtImageFile.Location = new System.Drawing.Point(107, 3);
            this.txtImageFile.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.txtImageFile.Name = "txtImageFile";
            this.txtImageFile.Size = new System.Drawing.Size(389, 20);
            this.txtImageFile.TabIndex = 1;
            this.BaseToolTip.SetToolTip(this.txtImageFile, "The base image file to display");
            // 
            // txtImageOverlay
            // 
            this.txtImageOverlay.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtImageOverlay.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.txtImageOverlay.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.FileSystem;
            this.errorProvider.SetIconAlignment(this.txtImageOverlay, System.Windows.Forms.ErrorIconAlignment.MiddleLeft);
            this.txtImageOverlay.Location = new System.Drawing.Point(107, 27);
            this.txtImageOverlay.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.txtImageOverlay.Name = "txtImageOverlay";
            this.txtImageOverlay.Size = new System.Drawing.Size(389, 20);
            this.txtImageOverlay.TabIndex = 3;
            this.BaseToolTip.SetToolTip(this.txtImageOverlay, "The overlay image to display");
            // 
            // cmbPixelFormat
            // 
            this.cmbPixelFormat.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbPixelFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbPixelFormat.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.cmbPixelFormat.FormattingEnabled = true;
            this.cmbPixelFormat.Location = new System.Drawing.Point(107, 99);
            this.cmbPixelFormat.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.cmbPixelFormat.Name = "cmbPixelFormat";
            this.cmbPixelFormat.Size = new System.Drawing.Size(389, 21);
            this.cmbPixelFormat.TabIndex = 7;
            this.BaseToolTip.SetToolTip(this.cmbPixelFormat, "The desired target pixel format. For lower bit-per-pixel formats it is recommende" +
        "d to enable dithering.");
            // 
            // chbOptimizePalette
            // 
            this.chbOptimizePalette.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.chbOptimizePalette.AutoSize = true;
            this.chbOptimizePalette.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.chbOptimizePalette.Location = new System.Drawing.Point(107, 147);
            this.chbOptimizePalette.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.chbOptimizePalette.Name = "chbOptimizePalette";
            this.chbOptimizePalette.Size = new System.Drawing.Size(389, 18);
            this.chbOptimizePalette.TabIndex = 9;
            this.chbOptimizePalette.Text = "Optimize Palette";
            this.BaseToolTip.SetToolTip(this.chbOptimizePalette, "When an indexed pixel format is selected, check to use an optimized palette inste" +
        "ad of a predefined one.");
            this.chbOptimizePalette.UseVisualStyleBackColor = true;
            // 
            // tblBackColor
            // 
            this.tblBackColor.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.tblBackColor.ColumnCount = 3;
            this.tblBackColor.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 103F));
            this.tblBackColor.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 103F));
            this.tblBackColor.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tblBackColor.Controls.Add(this.btnBackColor, 2, 0);
            this.tblBackColor.Controls.Add(this.pnlBackColor, 1, 0);
            this.tblBackColor.Controls.Add(this.lblBackColor, 0, 0);
            this.tblBackColor.Location = new System.Drawing.Point(107, 171);
            this.tblBackColor.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.tblBackColor.Name = "tblBackColor";
            this.tblBackColor.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tblBackColor.Size = new System.Drawing.Size(384, 18);
            this.tblBackColor.TabIndex = 10;
            // 
            // btnBackColor
            // 
            this.btnBackColor.AutoSize = true;
            this.btnBackColor.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnBackColor.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnBackColor.Location = new System.Drawing.Point(206, 0);
            this.btnBackColor.Margin = new System.Windows.Forms.Padding(0);
            this.btnBackColor.Name = "btnBackColor";
            this.btnBackColor.Size = new System.Drawing.Size(90, 18);
            this.btnBackColor.TabIndex = 2;
            this.btnBackColor.Text = "Pick Color";
            this.BaseToolTip.SetToolTip(this.btnBackColor, resources.GetString("btnBackColor.ToolTip"));
            this.btnBackColor.UseVisualStyleBackColor = true;
            // 
            // pnlBackColor
            // 
            this.pnlBackColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlBackColor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlBackColor.Location = new System.Drawing.Point(105, 0);
            this.pnlBackColor.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.pnlBackColor.Name = "pnlBackColor";
            this.pnlBackColor.Size = new System.Drawing.Size(99, 18);
            this.pnlBackColor.TabIndex = 1;
            // 
            // lblBackColor
            // 
            this.lblBackColor.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblBackColor.AutoSize = true;
            this.lblBackColor.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.lblBackColor.Location = new System.Drawing.Point(2, 2);
            this.lblBackColor.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblBackColor.Name = "lblBackColor";
            this.lblBackColor.Size = new System.Drawing.Size(62, 13);
            this.lblBackColor.TabIndex = 0;
            this.lblBackColor.Text = "Back Color:";
            this.lblBackColor.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // chbDitherer
            // 
            this.chbDitherer.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chbDitherer.AutoSize = true;
            this.chbDitherer.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.chbDitherer.Location = new System.Drawing.Point(4, 219);
            this.chbDitherer.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.chbDitherer.Name = "chbDitherer";
            this.chbDitherer.Size = new System.Drawing.Size(72, 18);
            this.chbDitherer.TabIndex = 12;
            this.chbDitherer.Text = "Ditherer:";
            this.BaseToolTip.SetToolTip(this.chbDitherer, "Check to use a ditherer. For high bit-per-pixel formats it makes little sense as " +
        "for those its only practical effect is just removing possible partial transparen" +
        "cy.");
            this.chbDitherer.UseVisualStyleBackColor = true;
            // 
            // cmbDitherer
            // 
            this.cmbDitherer.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbDitherer.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbDitherer.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.cmbDitherer.FormattingEnabled = true;
            this.cmbDitherer.Location = new System.Drawing.Point(107, 219);
            this.cmbDitherer.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.cmbDitherer.Name = "cmbDitherer";
            this.cmbDitherer.Size = new System.Drawing.Size(389, 21);
            this.cmbDitherer.TabIndex = 13;
            this.BaseToolTip.SetToolTip(this.cmbDitherer, resources.GetString("cmbDitherer.ToolTip"));
            // 
            // tblAlphaThreshold
            // 
            this.tblAlphaThreshold.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tblAlphaThreshold.ColumnCount = 3;
            this.tblAlphaThreshold.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 103F));
            this.tblAlphaThreshold.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 103F));
            this.tblAlphaThreshold.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tblAlphaThreshold.Controls.Add(this.lblAlphaThresholdValue, 2, 0);
            this.tblAlphaThreshold.Controls.Add(this.lblAlphaThreshold, 0, 0);
            this.tblAlphaThreshold.Controls.Add(this.tbAlphaThreshold, 1, 0);
            this.tblAlphaThreshold.Location = new System.Drawing.Point(107, 195);
            this.tblAlphaThreshold.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.tblAlphaThreshold.Name = "tblAlphaThreshold";
            this.tblAlphaThreshold.RowCount = 1;
            this.tblAlphaThreshold.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tblAlphaThreshold.Size = new System.Drawing.Size(389, 18);
            this.tblAlphaThreshold.TabIndex = 11;
            // 
            // lblAlphaThresholdValue
            // 
            this.lblAlphaThresholdValue.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblAlphaThresholdValue.AutoSize = true;
            this.lblAlphaThresholdValue.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.lblAlphaThresholdValue.Location = new System.Drawing.Point(208, 2);
            this.lblAlphaThresholdValue.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblAlphaThresholdValue.Name = "lblAlphaThresholdValue";
            this.lblAlphaThresholdValue.Size = new System.Drawing.Size(13, 13);
            this.lblAlphaThresholdValue.TabIndex = 2;
            this.lblAlphaThresholdValue.Text = "0";
            this.lblAlphaThresholdValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblAlphaThreshold
            // 
            this.lblAlphaThreshold.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblAlphaThreshold.AutoSize = true;
            this.lblAlphaThreshold.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.lblAlphaThreshold.Location = new System.Drawing.Point(2, 2);
            this.lblAlphaThreshold.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblAlphaThreshold.Name = "lblAlphaThreshold";
            this.lblAlphaThreshold.Size = new System.Drawing.Size(87, 13);
            this.lblAlphaThreshold.TabIndex = 0;
            this.lblAlphaThreshold.Text = "Alpha Threshold:";
            this.lblAlphaThreshold.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tbAlphaThreshold
            // 
            this.tbAlphaThreshold.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbAlphaThreshold.LargeChange = 64;
            this.tbAlphaThreshold.Location = new System.Drawing.Point(103, 0);
            this.tbAlphaThreshold.Margin = new System.Windows.Forms.Padding(0);
            this.tbAlphaThreshold.Maximum = 255;
            this.tbAlphaThreshold.Name = "tbAlphaThreshold";
            this.tbAlphaThreshold.Size = new System.Drawing.Size(103, 18);
            this.tbAlphaThreshold.TabIndex = 1;
            this.tbAlphaThreshold.TickFrequency = 16;
            this.BaseToolTip.SetToolTip(this.tbAlphaThreshold, resources.GetString("tbAlphaThreshold.ToolTip"));
            // 
            // errorProvider
            // 
            this.errorProvider.ContainerControl = this;
            // 
            // ssStatus
            // 
            this.ssStatus.AutoSize = false;
            this.ssStatus.GripStyle = System.Windows.Forms.ToolStripGripStyle.Visible;
            this.ssStatus.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.ssStatus.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblProgress,
            this.pbProgress});
            this.ssStatus.Location = new System.Drawing.Point(0, 377);
            this.ssStatus.Name = "ssStatus";
            this.ssStatus.Padding = new System.Windows.Forms.Padding(1, 0, 12, 0);
            this.ssStatus.Size = new System.Drawing.Size(500, 22);
            this.ssStatus.TabIndex = 1;
            // 
            // lblProgress
            // 
            this.lblProgress.Name = "lblProgress";
            this.lblProgress.Size = new System.Drawing.Size(52, 17);
            this.lblProgress.Text = "Progress";
            this.lblProgress.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // pbProgress
            // 
            this.pbProgress.AutoSize = false;
            this.pbProgress.Name = "pbProgress";
            this.pbProgress.Size = new System.Drawing.Size(86, 16);
            // 
            // timerProgress
            // 
            this.timerProgress.Interval = 30;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(500, 399);
            this.Controls.Add(this.tblContent);
            this.Controls.Add(this.ssStatus);
            this.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.MinimumSize = new System.Drawing.Size(412, 346);
            this.Name = "MainForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "KGy SOFT Drawing WinForms Example App";
            this.tblContent.ResumeLayout(false);
            this.tblContent.PerformLayout();
            this.tblOverlayShape.ResumeLayout(false);
            this.tblOverlayShape.PerformLayout();
            this.tblOutline.ResumeLayout(false);
            this.tblOutline.PerformLayout();
            this.pnlOutline.ResumeLayout(false);
            this.pnlOutline.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numOutline)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbImage)).EndInit();
            this.tblBackColor.ResumeLayout(false);
            this.tblBackColor.PerformLayout();
            this.tblAlphaThreshold.ResumeLayout(false);
            this.tblAlphaThreshold.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tbAlphaThreshold)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).EndInit();
            this.ssStatus.ResumeLayout(false);
            this.ssStatus.PerformLayout();
            this.ResumeLayout(false);

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
        private AdvancedErrorProvider errorProvider;
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