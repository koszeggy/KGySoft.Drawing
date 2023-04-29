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
            lblBackColor = new Label();
            btnBackColor = new Button();
            pnlBackColor = new Panel();
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
            tblContent.Controls.Add(chbForceLinear, 1, 3);
            tblContent.Controls.Add(pbImage, 0, 8);
            tblContent.Controls.Add(lblImageFile, 0, 0);
            tblContent.Controls.Add(lblPixelFormat, 0, 2);
            tblContent.Controls.Add(chbImageOverlay, 0, 1);
            tblContent.Controls.Add(txtImageFile, 1, 0);
            tblContent.Controls.Add(txtImageOverlay, 1, 1);
            tblContent.Controls.Add(cmbPixelFormat, 1, 2);
            tblContent.Controls.Add(chbOptimizePalette, 1, 4);
            tblContent.Controls.Add(tblBackColor, 1, 5);
            tblContent.Controls.Add(chbDitherer, 0, 7);
            tblContent.Controls.Add(cmbDitherer, 1, 7);
            tblContent.Controls.Add(tblAlphaThreshold, 1, 6);
            tblContent.Dock = DockStyle.Fill;
            tblContent.Location = new System.Drawing.Point(0, 0);
            tblContent.Name = "tblContent";
            tblContent.Padding = new Padding(3, 0, 3, 0);
            tblContent.RowCount = 9;
            tblContent.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            tblContent.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            tblContent.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            tblContent.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            tblContent.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            tblContent.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            tblContent.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            tblContent.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            tblContent.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tblContent.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tblContent.Size = new System.Drawing.Size(584, 389);
            tblContent.TabIndex = 0;
            // 
            // chbForceLinear
            // 
            chbForceLinear.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            chbForceLinear.AutoSize = true;
            chbForceLinear.FlatStyle = FlatStyle.System;
            chbForceLinear.Location = new System.Drawing.Point(126, 88);
            chbForceLinear.Name = "chbForceLinear";
            chbForceLinear.Size = new System.Drawing.Size(452, 20);
            chbForceLinear.TabIndex = 6;
            chbForceLinear.Text = "Force Linear Color Space";
            toolTip.SetToolTip(chbForceLinear, resources.GetString("chbForceLinear.ToolTip"));
            chbForceLinear.UseVisualStyleBackColor = true;
            // 
            // pbImage
            // 
            tblContent.SetColumnSpan(pbImage, 2);
            pbImage.Dock = DockStyle.Fill;
            pbImage.Location = new System.Drawing.Point(6, 227);
            pbImage.Name = "pbImage";
            pbImage.Size = new System.Drawing.Size(572, 159);
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
            lblPixelFormat.Location = new System.Drawing.Point(6, 62);
            lblPixelFormat.Name = "lblPixelFormat";
            lblPixelFormat.Size = new System.Drawing.Size(76, 15);
            lblPixelFormat.TabIndex = 4;
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
            cmbPixelFormat.Location = new System.Drawing.Point(126, 59);
            cmbPixelFormat.Name = "cmbPixelFormat";
            cmbPixelFormat.Size = new System.Drawing.Size(452, 23);
            cmbPixelFormat.TabIndex = 5;
            toolTip.SetToolTip(cmbPixelFormat, "The desired target pixel format. For lower bit-per-pixel formats it is recommended to enable dithering.");
            // 
            // chbOptimizePalette
            // 
            chbOptimizePalette.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            chbOptimizePalette.AutoSize = true;
            chbOptimizePalette.FlatStyle = FlatStyle.System;
            chbOptimizePalette.Location = new System.Drawing.Point(126, 116);
            chbOptimizePalette.Name = "chbOptimizePalette";
            chbOptimizePalette.Size = new System.Drawing.Size(452, 20);
            chbOptimizePalette.TabIndex = 8;
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
            tblBackColor.Controls.Add(lblBackColor);
            tblBackColor.Controls.Add(btnBackColor, 2, 0);
            tblBackColor.Controls.Add(pnlBackColor, 1, 0);
            tblBackColor.Location = new System.Drawing.Point(126, 143);
            tblBackColor.Name = "tblBackColor";
            tblBackColor.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tblBackColor.Size = new System.Drawing.Size(448, 22);
            tblBackColor.TabIndex = 9;
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
            pnlBackColor.Dock = DockStyle.Fill;
            pnlBackColor.Location = new System.Drawing.Point(123, 0);
            pnlBackColor.Margin = new Padding(3, 0, 3, 0);
            pnlBackColor.Name = "pnlBackColor";
            pnlBackColor.Size = new System.Drawing.Size(114, 22);
            pnlBackColor.TabIndex = 1;
            // 
            // chbDitherer
            // 
            chbDitherer.Anchor = AnchorStyles.Left;
            chbDitherer.AutoSize = true;
            chbDitherer.FlatStyle = FlatStyle.System;
            chbDitherer.Location = new System.Drawing.Point(6, 200);
            chbDitherer.Name = "chbDitherer";
            chbDitherer.Size = new System.Drawing.Size(77, 20);
            chbDitherer.TabIndex = 11;
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
            cmbDitherer.Location = new System.Drawing.Point(126, 199);
            cmbDitherer.Name = "cmbDitherer";
            cmbDitherer.Size = new System.Drawing.Size(452, 23);
            cmbDitherer.TabIndex = 12;
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
            tblAlphaThreshold.Location = new System.Drawing.Point(126, 171);
            tblAlphaThreshold.Name = "tblAlphaThreshold";
            tblAlphaThreshold.RowCount = 1;
            tblAlphaThreshold.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tblAlphaThreshold.Size = new System.Drawing.Size(452, 22);
            tblAlphaThreshold.TabIndex = 10;
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
            ssStatus.Location = new System.Drawing.Point(0, 389);
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
            ClientSize = new System.Drawing.Size(584, 411);
            Controls.Add(tblContent);
            Controls.Add(ssStatus);
            MinimumSize = new System.Drawing.Size(480, 350);
            Name = "MainForm";
            SizeGripStyle = SizeGripStyle.Hide;
            Text = "KGy SOFT Drawing WinForms Example App";
            tblContent.ResumeLayout(false);
            tblContent.PerformLayout();
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