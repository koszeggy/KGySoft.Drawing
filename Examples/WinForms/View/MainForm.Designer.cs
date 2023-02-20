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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.tblContent = new System.Windows.Forms.TableLayoutPanel();
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
            this.lblBackColor = new System.Windows.Forms.Label();
            this.btnBackColor = new System.Windows.Forms.Button();
            this.pnlBackColor = new System.Windows.Forms.Panel();
            this.chbDitherer = new System.Windows.Forms.CheckBox();
            this.cmbDitherer = new System.Windows.Forms.ComboBox();
            this.tblAlphaThreshold = new System.Windows.Forms.TableLayoutPanel();
            this.lblAlphaThresholdValue = new System.Windows.Forms.Label();
            this.lblAlphaThreshold = new System.Windows.Forms.Label();
            this.tbAlphaThreshold = new System.Windows.Forms.TrackBar();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
            this.ssStatus = new System.Windows.Forms.StatusStrip();
            this.lblProgress = new System.Windows.Forms.ToolStripStatusLabel();
            this.pbProgress = new System.Windows.Forms.ToolStripProgressBar();
            this.timerProgress = new System.Windows.Forms.Timer(this.components);
            this.colorDialog = new System.Windows.Forms.ColorDialog();
            this.tblContent.SuspendLayout();
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
            this.tblContent.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
            this.tblContent.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tblContent.Controls.Add(this.chbForceLinear, 1, 3);
            this.tblContent.Controls.Add(this.pbImage, 0, 8);
            this.tblContent.Controls.Add(this.lblImageFile, 0, 0);
            this.tblContent.Controls.Add(this.lblPixelFormat, 0, 2);
            this.tblContent.Controls.Add(this.chbImageOverlay, 0, 1);
            this.tblContent.Controls.Add(this.txtImageFile, 1, 0);
            this.tblContent.Controls.Add(this.txtImageOverlay, 1, 1);
            this.tblContent.Controls.Add(this.cmbPixelFormat, 1, 2);
            this.tblContent.Controls.Add(this.chbOptimizePalette, 1, 4);
            this.tblContent.Controls.Add(this.tblBackColor, 1, 5);
            this.tblContent.Controls.Add(this.chbDitherer, 0, 7);
            this.tblContent.Controls.Add(this.cmbDitherer, 1, 7);
            this.tblContent.Controls.Add(this.tblAlphaThreshold, 1, 6);
            this.tblContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tblContent.Location = new System.Drawing.Point(0, 0);
            this.tblContent.Name = "tblContent";
            this.tblContent.Padding = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.tblContent.RowCount = 9;
            this.tblContent.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
            this.tblContent.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
            this.tblContent.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
            this.tblContent.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
            this.tblContent.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
            this.tblContent.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
            this.tblContent.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
            this.tblContent.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
            this.tblContent.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tblContent.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tblContent.Size = new System.Drawing.Size(584, 389);
            this.tblContent.TabIndex = 0;
            // 
            // chbForceLinear
            // 
            this.chbForceLinear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.chbForceLinear.AutoSize = true;
            this.chbForceLinear.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.chbForceLinear.Location = new System.Drawing.Point(126, 88);
            this.chbForceLinear.Name = "chbForceLinear";
            this.chbForceLinear.Size = new System.Drawing.Size(452, 20);
            this.chbForceLinear.TabIndex = 6;
            this.chbForceLinear.Text = "Force Linear Color Space";
            this.toolTip.SetToolTip(this.chbForceLinear, resources.GetString("chbForceLinear.ToolTip"));
            this.chbForceLinear.UseVisualStyleBackColor = true;
            // 
            // pbImage
            // 
            this.tblContent.SetColumnSpan(this.pbImage, 2);
            this.pbImage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pbImage.Location = new System.Drawing.Point(6, 227);
            this.pbImage.Name = "pbImage";
            this.pbImage.Size = new System.Drawing.Size(572, 159);
            this.pbImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pbImage.TabIndex = 11;
            this.pbImage.TabStop = false;
            // 
            // lblImageFile
            // 
            this.lblImageFile.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblImageFile.AutoSize = true;
            this.lblImageFile.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.lblImageFile.Location = new System.Drawing.Point(6, 6);
            this.lblImageFile.Name = "lblImageFile";
            this.lblImageFile.Size = new System.Drawing.Size(64, 15);
            this.lblImageFile.TabIndex = 0;
            this.lblImageFile.Text = "Image File:";
            // 
            // lblPixelFormat
            // 
            this.lblPixelFormat.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblPixelFormat.AutoSize = true;
            this.lblPixelFormat.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.lblPixelFormat.Location = new System.Drawing.Point(6, 62);
            this.lblPixelFormat.Name = "lblPixelFormat";
            this.lblPixelFormat.Size = new System.Drawing.Size(76, 15);
            this.lblPixelFormat.TabIndex = 4;
            this.lblPixelFormat.Text = "Pixel Format:";
            // 
            // chbImageOverlay
            // 
            this.chbImageOverlay.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chbImageOverlay.AutoSize = true;
            this.chbImageOverlay.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.chbImageOverlay.Location = new System.Drawing.Point(6, 32);
            this.chbImageOverlay.Name = "chbImageOverlay";
            this.chbImageOverlay.Size = new System.Drawing.Size(111, 20);
            this.chbImageOverlay.TabIndex = 2;
            this.chbImageOverlay.Text = "Image Overlay:";
            this.toolTip.SetToolTip(this.chbImageOverlay, "Check to blend an overlay image with the base image");
            this.chbImageOverlay.UseVisualStyleBackColor = true;
            // 
            // txtImageFile
            // 
            this.txtImageFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtImageFile.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.txtImageFile.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.FileSystem;
            this.errorProvider.SetIconAlignment(this.txtImageFile, System.Windows.Forms.ErrorIconAlignment.MiddleLeft);
            this.txtImageFile.Location = new System.Drawing.Point(126, 3);
            this.txtImageFile.Name = "txtImageFile";
            this.txtImageFile.Size = new System.Drawing.Size(452, 23);
            this.txtImageFile.TabIndex = 1;
            this.toolTip.SetToolTip(this.txtImageFile, "The base image file to display");
            // 
            // txtImageOverlay
            // 
            this.txtImageOverlay.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtImageOverlay.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.txtImageOverlay.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.FileSystem;
            this.errorProvider.SetIconAlignment(this.txtImageOverlay, System.Windows.Forms.ErrorIconAlignment.MiddleLeft);
            this.txtImageOverlay.Location = new System.Drawing.Point(126, 31);
            this.txtImageOverlay.Name = "txtImageOverlay";
            this.txtImageOverlay.Size = new System.Drawing.Size(452, 23);
            this.txtImageOverlay.TabIndex = 3;
            this.toolTip.SetToolTip(this.txtImageOverlay, "The overlay image to display");
            // 
            // cmbPixelFormat
            // 
            this.cmbPixelFormat.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbPixelFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbPixelFormat.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.cmbPixelFormat.FormattingEnabled = true;
            this.cmbPixelFormat.Location = new System.Drawing.Point(126, 59);
            this.cmbPixelFormat.Name = "cmbPixelFormat";
            this.cmbPixelFormat.Size = new System.Drawing.Size(452, 23);
            this.cmbPixelFormat.TabIndex = 5;
            this.toolTip.SetToolTip(this.cmbPixelFormat, "The desired target pixel format. For lower bit-per-pixel formats it is recommende" +
        "d to enable dithering.");
            // 
            // chbOptimizePalette
            // 
            this.chbOptimizePalette.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.chbOptimizePalette.AutoSize = true;
            this.chbOptimizePalette.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.chbOptimizePalette.Location = new System.Drawing.Point(126, 116);
            this.chbOptimizePalette.Name = "chbOptimizePalette";
            this.chbOptimizePalette.Size = new System.Drawing.Size(452, 20);
            this.chbOptimizePalette.TabIndex = 8;
            this.chbOptimizePalette.Text = "Optimize Palette";
            this.toolTip.SetToolTip(this.chbOptimizePalette, "When an indexed pixel format is selected, check to use an optimized palette inste" +
        "ad of a predefined one.");
            this.chbOptimizePalette.UseVisualStyleBackColor = true;
            // 
            // tblBackColor
            // 
            this.tblBackColor.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.tblBackColor.ColumnCount = 3;
            this.tblBackColor.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
            this.tblBackColor.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
            this.tblBackColor.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tblBackColor.Controls.Add(this.lblBackColor);
            this.tblBackColor.Controls.Add(this.btnBackColor, 2, 0);
            this.tblBackColor.Controls.Add(this.pnlBackColor, 1, 0);
            this.tblBackColor.Location = new System.Drawing.Point(126, 143);
            this.tblBackColor.Name = "tblBackColor";
            this.tblBackColor.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tblBackColor.Size = new System.Drawing.Size(448, 22);
            this.tblBackColor.TabIndex = 9;
            // 
            // lblBackColor
            // 
            this.lblBackColor.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblBackColor.AutoSize = true;
            this.lblBackColor.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.lblBackColor.Location = new System.Drawing.Point(3, 3);
            this.lblBackColor.Name = "lblBackColor";
            this.lblBackColor.Size = new System.Drawing.Size(67, 15);
            this.lblBackColor.TabIndex = 0;
            this.lblBackColor.Text = "Back Color:";
            this.lblBackColor.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnBackColor
            // 
            this.btnBackColor.AutoSize = true;
            this.btnBackColor.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnBackColor.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnBackColor.Location = new System.Drawing.Point(240, 0);
            this.btnBackColor.Margin = new System.Windows.Forms.Padding(0);
            this.btnBackColor.Name = "btnBackColor";
            this.btnBackColor.Size = new System.Drawing.Size(75, 22);
            this.btnBackColor.TabIndex = 2;
            this.btnBackColor.Text = "Pick Color";
            this.toolTip.SetToolTip(this.btnBackColor, resources.GetString("btnBackColor.ToolTip"));
            this.btnBackColor.UseVisualStyleBackColor = true;
            // 
            // pnlBackColor
            // 
            this.pnlBackColor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlBackColor.Location = new System.Drawing.Point(123, 0);
            this.pnlBackColor.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.pnlBackColor.Name = "pnlBackColor";
            this.pnlBackColor.Size = new System.Drawing.Size(114, 22);
            this.pnlBackColor.TabIndex = 1;
            // 
            // chbDitherer
            // 
            this.chbDitherer.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chbDitherer.AutoSize = true;
            this.chbDitherer.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.chbDitherer.Location = new System.Drawing.Point(6, 200);
            this.chbDitherer.Name = "chbDitherer";
            this.chbDitherer.Size = new System.Drawing.Size(77, 20);
            this.chbDitherer.TabIndex = 11;
            this.chbDitherer.Text = "Ditherer:";
            this.toolTip.SetToolTip(this.chbDitherer, "Check to use a ditherer. For high bit-per-pixel formats it makes little sense as " +
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
            this.cmbDitherer.Location = new System.Drawing.Point(126, 199);
            this.cmbDitherer.Name = "cmbDitherer";
            this.cmbDitherer.Size = new System.Drawing.Size(452, 23);
            this.cmbDitherer.TabIndex = 12;
            this.toolTip.SetToolTip(this.cmbDitherer, resources.GetString("cmbDitherer.ToolTip"));
            // 
            // tblAlphaThreshold
            // 
            this.tblAlphaThreshold.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tblAlphaThreshold.ColumnCount = 3;
            this.tblAlphaThreshold.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
            this.tblAlphaThreshold.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
            this.tblAlphaThreshold.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tblAlphaThreshold.Controls.Add(this.lblAlphaThresholdValue, 2, 0);
            this.tblAlphaThreshold.Controls.Add(this.lblAlphaThreshold, 0, 0);
            this.tblAlphaThreshold.Controls.Add(this.tbAlphaThreshold, 1, 0);
            this.tblAlphaThreshold.Location = new System.Drawing.Point(126, 171);
            this.tblAlphaThreshold.Name = "tblAlphaThreshold";
            this.tblAlphaThreshold.RowCount = 1;
            this.tblAlphaThreshold.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tblAlphaThreshold.Size = new System.Drawing.Size(452, 22);
            this.tblAlphaThreshold.TabIndex = 10;
            // 
            // lblAlphaThresholdValue
            // 
            this.lblAlphaThresholdValue.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblAlphaThresholdValue.AutoSize = true;
            this.lblAlphaThresholdValue.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.lblAlphaThresholdValue.Location = new System.Drawing.Point(243, 3);
            this.lblAlphaThresholdValue.Name = "lblAlphaThresholdValue";
            this.lblAlphaThresholdValue.Size = new System.Drawing.Size(13, 15);
            this.lblAlphaThresholdValue.TabIndex = 2;
            this.lblAlphaThresholdValue.Text = "0";
            this.lblAlphaThresholdValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblAlphaThreshold
            // 
            this.lblAlphaThreshold.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblAlphaThreshold.AutoSize = true;
            this.lblAlphaThreshold.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.lblAlphaThreshold.Location = new System.Drawing.Point(3, 3);
            this.lblAlphaThreshold.Name = "lblAlphaThreshold";
            this.lblAlphaThreshold.Size = new System.Drawing.Size(96, 15);
            this.lblAlphaThreshold.TabIndex = 0;
            this.lblAlphaThreshold.Text = "Alpha Threshold:";
            this.lblAlphaThreshold.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tbAlphaThreshold
            // 
            this.tbAlphaThreshold.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbAlphaThreshold.LargeChange = 64;
            this.tbAlphaThreshold.Location = new System.Drawing.Point(120, 0);
            this.tbAlphaThreshold.Margin = new System.Windows.Forms.Padding(0);
            this.tbAlphaThreshold.Maximum = 255;
            this.tbAlphaThreshold.Name = "tbAlphaThreshold";
            this.tbAlphaThreshold.Size = new System.Drawing.Size(120, 22);
            this.tbAlphaThreshold.TabIndex = 1;
            this.tbAlphaThreshold.TickFrequency = 16;
            this.toolTip.SetToolTip(this.tbAlphaThreshold, resources.GetString("tbAlphaThreshold.ToolTip"));
            // 
            // errorProvider
            // 
            this.errorProvider.ContainerControl = this;
            // 
            // ssStatus
            // 
            this.ssStatus.GripStyle = System.Windows.Forms.ToolStripGripStyle.Visible;
            this.ssStatus.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblProgress,
            this.pbProgress});
            this.ssStatus.Location = new System.Drawing.Point(0, 389);
            this.ssStatus.Name = "ssStatus";
            this.ssStatus.Size = new System.Drawing.Size(584, 22);
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
            this.pbProgress.Size = new System.Drawing.Size(100, 16);
            // 
            // timerProgress
            // 
            this.timerProgress.Interval = 30;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(584, 411);
            this.Controls.Add(this.tblContent);
            this.Controls.Add(this.ssStatus);
            this.MinimumSize = new System.Drawing.Size(480, 350);
            this.Name = "MainForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "KGy SOFT Drawing WinForms Example App";
            this.tblContent.ResumeLayout(false);
            this.tblContent.PerformLayout();
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
            this.PerformLayout();

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
        private Label lblBackColor;
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
    }
}