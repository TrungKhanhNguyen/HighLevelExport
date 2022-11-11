namespace HighLevelExport
{
    partial class Main
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            Bunifu.UI.WinForms.BunifuButton.BunifuButton.BorderEdges borderEdges6 = new Bunifu.UI.WinForms.BunifuButton.BunifuButton.BorderEdges();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));
            Bunifu.UI.WinForms.BunifuButton.BunifuButton.BorderEdges borderEdges5 = new Bunifu.UI.WinForms.BunifuButton.BunifuButton.BorderEdges();
            this.bunifuElipse1 = new Bunifu.Framework.UI.BunifuElipse(this.components);
            this.bunifuDragControl1 = new Bunifu.Framework.UI.BunifuDragControl(this.components);
            this.btnMinimize = new Bunifu.UI.WinForms.BunifuImageButton();
            this.btnClose = new Bunifu.UI.WinForms.BunifuImageButton();
            this.btnStart = new Bunifu.UI.WinForms.BunifuButton.BunifuButton();
            this.btnStop = new Bunifu.UI.WinForms.BunifuButton.BunifuButton();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.txtMultiLog = new System.Windows.Forms.TextBox();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.startTimeDate = new System.Windows.Forms.DateTimePicker();
            this.startTimeTime = new System.Windows.Forms.DateTimePicker();
            this.endTimeTime = new System.Windows.Forms.DateTimePicker();
            this.endTimeDate = new System.Windows.Forms.DateTimePicker();
            this.SuspendLayout();
            // 
            // bunifuElipse1
            // 
            this.bunifuElipse1.ElipseRadius = 5;
            this.bunifuElipse1.TargetControl = this;
            // 
            // bunifuDragControl1
            // 
            this.bunifuDragControl1.Fixed = true;
            this.bunifuDragControl1.Horizontal = true;
            this.bunifuDragControl1.TargetControl = this;
            this.bunifuDragControl1.Vertical = true;
            // 
            // btnMinimize
            // 
            this.btnMinimize.ActiveImage = null;
            this.btnMinimize.AllowAnimations = true;
            this.btnMinimize.AllowBuffering = false;
            this.btnMinimize.AllowToggling = false;
            this.btnMinimize.AllowZooming = false;
            this.btnMinimize.AllowZoomingOnFocus = false;
            this.btnMinimize.BackColor = System.Drawing.Color.Transparent;
            this.btnMinimize.DialogResult = System.Windows.Forms.DialogResult.None;
            this.btnMinimize.ErrorImage = ((System.Drawing.Image)(resources.GetObject("btnMinimize.ErrorImage")));
            this.btnMinimize.FadeWhenInactive = false;
            this.btnMinimize.Flip = Bunifu.UI.WinForms.BunifuImageButton.FlipOrientation.Normal;
            this.btnMinimize.Image = ((System.Drawing.Image)(resources.GetObject("btnMinimize.Image")));
            this.btnMinimize.ImageActive = null;
            this.btnMinimize.ImageLocation = null;
            this.btnMinimize.ImageMargin = 20;
            this.btnMinimize.ImageSize = new System.Drawing.Size(11, 11);
            this.btnMinimize.ImageZoomSize = new System.Drawing.Size(31, 31);
            this.btnMinimize.InitialImage = ((System.Drawing.Image)(resources.GetObject("btnMinimize.InitialImage")));
            this.btnMinimize.Location = new System.Drawing.Point(258, 12);
            this.btnMinimize.Name = "btnMinimize";
            this.btnMinimize.Rotation = 0;
            this.btnMinimize.ShowActiveImage = true;
            this.btnMinimize.ShowCursorChanges = true;
            this.btnMinimize.ShowImageBorders = false;
            this.btnMinimize.ShowSizeMarkers = false;
            this.btnMinimize.Size = new System.Drawing.Size(31, 31);
            this.btnMinimize.TabIndex = 5;
            this.btnMinimize.ToolTipText = "";
            this.btnMinimize.WaitOnLoad = false;
            this.btnMinimize.Zoom = 20;
            this.btnMinimize.ZoomSpeed = 10;
            // 
            // btnClose
            // 
            this.btnClose.ActiveImage = null;
            this.btnClose.AllowAnimations = true;
            this.btnClose.AllowBuffering = false;
            this.btnClose.AllowToggling = false;
            this.btnClose.AllowZooming = false;
            this.btnClose.AllowZoomingOnFocus = false;
            this.btnClose.BackColor = System.Drawing.Color.Transparent;
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.None;
            this.btnClose.ErrorImage = ((System.Drawing.Image)(resources.GetObject("btnClose.ErrorImage")));
            this.btnClose.FadeWhenInactive = false;
            this.btnClose.Flip = Bunifu.UI.WinForms.BunifuImageButton.FlipOrientation.Normal;
            this.btnClose.Image = ((System.Drawing.Image)(resources.GetObject("btnClose.Image")));
            this.btnClose.ImageActive = null;
            this.btnClose.ImageLocation = null;
            this.btnClose.ImageMargin = 20;
            this.btnClose.ImageSize = new System.Drawing.Size(11, 11);
            this.btnClose.ImageZoomSize = new System.Drawing.Size(31, 31);
            this.btnClose.InitialImage = ((System.Drawing.Image)(resources.GetObject("btnClose.InitialImage")));
            this.btnClose.Location = new System.Drawing.Point(295, 12);
            this.btnClose.Name = "btnClose";
            this.btnClose.Rotation = 0;
            this.btnClose.ShowActiveImage = true;
            this.btnClose.ShowCursorChanges = true;
            this.btnClose.ShowImageBorders = false;
            this.btnClose.ShowSizeMarkers = false;
            this.btnClose.Size = new System.Drawing.Size(31, 31);
            this.btnClose.TabIndex = 4;
            this.btnClose.ToolTipText = "";
            this.btnClose.WaitOnLoad = false;
            this.btnClose.Zoom = 20;
            this.btnClose.ZoomSpeed = 10;
            // 
            // btnStart
            // 
            this.btnStart.AllowAnimations = true;
            this.btnStart.AllowMouseEffects = true;
            this.btnStart.AllowToggling = false;
            this.btnStart.AnimationSpeed = 200;
            this.btnStart.AutoGenerateColors = false;
            this.btnStart.AutoRoundBorders = false;
            this.btnStart.AutoSizeLeftIcon = true;
            this.btnStart.AutoSizeRightIcon = true;
            this.btnStart.BackColor = System.Drawing.Color.Transparent;
            this.btnStart.BackColor1 = System.Drawing.Color.FromArgb(((int)(((byte)(81)))), ((int)(((byte)(180)))), ((int)(((byte)(81)))));
            this.btnStart.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnStart.BackgroundImage")));
            this.btnStart.BorderStyle = Bunifu.UI.WinForms.BunifuButton.BunifuButton.BorderStyles.Solid;
            this.btnStart.ButtonText = "Start";
            this.btnStart.ButtonTextMarginLeft = 0;
            this.btnStart.ColorContrastOnClick = 45;
            this.btnStart.ColorContrastOnHover = 45;
            this.btnStart.Cursor = System.Windows.Forms.Cursors.Default;
            borderEdges6.BottomLeft = true;
            borderEdges6.BottomRight = true;
            borderEdges6.TopLeft = true;
            borderEdges6.TopRight = true;
            this.btnStart.CustomizableEdges = borderEdges6;
            this.btnStart.DialogResult = System.Windows.Forms.DialogResult.None;
            this.btnStart.DisabledBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(191)))), ((int)(((byte)(191)))), ((int)(((byte)(191)))));
            this.btnStart.DisabledFillColor = System.Drawing.Color.FromArgb(((int)(((byte)(204)))), ((int)(((byte)(204)))), ((int)(((byte)(204)))));
            this.btnStart.DisabledForecolor = System.Drawing.Color.FromArgb(((int)(((byte)(168)))), ((int)(((byte)(160)))), ((int)(((byte)(168)))));
            this.btnStart.FocusState = Bunifu.UI.WinForms.BunifuButton.BunifuButton.ButtonStates.Pressed;
            this.btnStart.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnStart.ForeColor = System.Drawing.Color.White;
            this.btnStart.IconLeftAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnStart.IconLeftCursor = System.Windows.Forms.Cursors.Default;
            this.btnStart.IconLeftPadding = new System.Windows.Forms.Padding(11, 3, 3, 3);
            this.btnStart.IconMarginLeft = 11;
            this.btnStart.IconPadding = 10;
            this.btnStart.IconRightAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnStart.IconRightCursor = System.Windows.Forms.Cursors.Default;
            this.btnStart.IconRightPadding = new System.Windows.Forms.Padding(3, 3, 7, 3);
            this.btnStart.IconSize = 25;
            this.btnStart.IdleBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(81)))), ((int)(((byte)(180)))), ((int)(((byte)(81)))));
            this.btnStart.IdleBorderRadius = 1;
            this.btnStart.IdleBorderThickness = 1;
            this.btnStart.IdleFillColor = System.Drawing.Color.FromArgb(((int)(((byte)(81)))), ((int)(((byte)(180)))), ((int)(((byte)(81)))));
            this.btnStart.IdleIconLeftImage = null;
            this.btnStart.IdleIconRightImage = null;
            this.btnStart.IndicateFocus = false;
            this.btnStart.Location = new System.Drawing.Point(10, 113);
            this.btnStart.Name = "btnStart";
            this.btnStart.OnDisabledState.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(191)))), ((int)(((byte)(191)))), ((int)(((byte)(191)))));
            this.btnStart.OnDisabledState.BorderRadius = 1;
            this.btnStart.OnDisabledState.BorderStyle = Bunifu.UI.WinForms.BunifuButton.BunifuButton.BorderStyles.Solid;
            this.btnStart.OnDisabledState.BorderThickness = 1;
            this.btnStart.OnDisabledState.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(204)))), ((int)(((byte)(204)))), ((int)(((byte)(204)))));
            this.btnStart.OnDisabledState.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(168)))), ((int)(((byte)(160)))), ((int)(((byte)(168)))));
            this.btnStart.OnDisabledState.IconLeftImage = null;
            this.btnStart.OnDisabledState.IconRightImage = null;
            this.btnStart.onHoverState.BorderColor = System.Drawing.Color.DarkGreen;
            this.btnStart.onHoverState.BorderRadius = 1;
            this.btnStart.onHoverState.BorderStyle = Bunifu.UI.WinForms.BunifuButton.BunifuButton.BorderStyles.Solid;
            this.btnStart.onHoverState.BorderThickness = 1;
            this.btnStart.onHoverState.FillColor = System.Drawing.Color.DarkGreen;
            this.btnStart.onHoverState.ForeColor = System.Drawing.Color.White;
            this.btnStart.onHoverState.IconLeftImage = null;
            this.btnStart.onHoverState.IconRightImage = null;
            this.btnStart.OnIdleState.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(81)))), ((int)(((byte)(180)))), ((int)(((byte)(81)))));
            this.btnStart.OnIdleState.BorderRadius = 1;
            this.btnStart.OnIdleState.BorderStyle = Bunifu.UI.WinForms.BunifuButton.BunifuButton.BorderStyles.Solid;
            this.btnStart.OnIdleState.BorderThickness = 1;
            this.btnStart.OnIdleState.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(81)))), ((int)(((byte)(180)))), ((int)(((byte)(81)))));
            this.btnStart.OnIdleState.ForeColor = System.Drawing.Color.White;
            this.btnStart.OnIdleState.IconLeftImage = null;
            this.btnStart.OnIdleState.IconRightImage = null;
            this.btnStart.OnPressedState.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(96)))), ((int)(((byte)(144)))));
            this.btnStart.OnPressedState.BorderRadius = 1;
            this.btnStart.OnPressedState.BorderStyle = Bunifu.UI.WinForms.BunifuButton.BunifuButton.BorderStyles.Solid;
            this.btnStart.OnPressedState.BorderThickness = 1;
            this.btnStart.OnPressedState.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(96)))), ((int)(((byte)(144)))));
            this.btnStart.OnPressedState.ForeColor = System.Drawing.Color.White;
            this.btnStart.OnPressedState.IconLeftImage = null;
            this.btnStart.OnPressedState.IconRightImage = null;
            this.btnStart.Size = new System.Drawing.Size(106, 32);
            this.btnStart.TabIndex = 6;
            this.btnStart.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.btnStart.TextAlignment = System.Windows.Forms.HorizontalAlignment.Center;
            this.btnStart.TextMarginLeft = 0;
            this.btnStart.TextPadding = new System.Windows.Forms.Padding(0);
            this.btnStart.UseDefaultRadiusAndThickness = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // btnStop
            // 
            this.btnStop.AllowAnimations = true;
            this.btnStop.AllowMouseEffects = true;
            this.btnStop.AllowToggling = false;
            this.btnStop.AnimationSpeed = 200;
            this.btnStop.AutoGenerateColors = false;
            this.btnStop.AutoRoundBorders = false;
            this.btnStop.AutoSizeLeftIcon = true;
            this.btnStop.AutoSizeRightIcon = true;
            this.btnStop.BackColor = System.Drawing.Color.Transparent;
            this.btnStop.BackColor1 = System.Drawing.Color.Firebrick;
            this.btnStop.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnStop.BackgroundImage")));
            this.btnStop.BorderStyle = Bunifu.UI.WinForms.BunifuButton.BunifuButton.BorderStyles.Solid;
            this.btnStop.ButtonText = "Stop";
            this.btnStop.ButtonTextMarginLeft = 0;
            this.btnStop.ColorContrastOnClick = 45;
            this.btnStop.ColorContrastOnHover = 45;
            this.btnStop.Cursor = System.Windows.Forms.Cursors.Default;
            borderEdges5.BottomLeft = true;
            borderEdges5.BottomRight = true;
            borderEdges5.TopLeft = true;
            borderEdges5.TopRight = true;
            this.btnStop.CustomizableEdges = borderEdges5;
            this.btnStop.DialogResult = System.Windows.Forms.DialogResult.None;
            this.btnStop.DisabledBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(191)))), ((int)(((byte)(191)))), ((int)(((byte)(191)))));
            this.btnStop.DisabledFillColor = System.Drawing.Color.FromArgb(((int)(((byte)(204)))), ((int)(((byte)(204)))), ((int)(((byte)(204)))));
            this.btnStop.DisabledForecolor = System.Drawing.Color.FromArgb(((int)(((byte)(168)))), ((int)(((byte)(160)))), ((int)(((byte)(168)))));
            this.btnStop.FocusState = Bunifu.UI.WinForms.BunifuButton.BunifuButton.ButtonStates.Pressed;
            this.btnStop.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnStop.ForeColor = System.Drawing.Color.White;
            this.btnStop.IconLeftAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnStop.IconLeftCursor = System.Windows.Forms.Cursors.Default;
            this.btnStop.IconLeftPadding = new System.Windows.Forms.Padding(11, 3, 3, 3);
            this.btnStop.IconMarginLeft = 11;
            this.btnStop.IconPadding = 10;
            this.btnStop.IconRightAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnStop.IconRightCursor = System.Windows.Forms.Cursors.Default;
            this.btnStop.IconRightPadding = new System.Windows.Forms.Padding(3, 3, 7, 3);
            this.btnStop.IconSize = 25;
            this.btnStop.IdleBorderColor = System.Drawing.Color.Firebrick;
            this.btnStop.IdleBorderRadius = 1;
            this.btnStop.IdleBorderThickness = 1;
            this.btnStop.IdleFillColor = System.Drawing.Color.Firebrick;
            this.btnStop.IdleIconLeftImage = null;
            this.btnStop.IdleIconRightImage = null;
            this.btnStop.IndicateFocus = false;
            this.btnStop.Location = new System.Drawing.Point(122, 113);
            this.btnStop.Name = "btnStop";
            this.btnStop.OnDisabledState.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(191)))), ((int)(((byte)(191)))), ((int)(((byte)(191)))));
            this.btnStop.OnDisabledState.BorderRadius = 1;
            this.btnStop.OnDisabledState.BorderStyle = Bunifu.UI.WinForms.BunifuButton.BunifuButton.BorderStyles.Solid;
            this.btnStop.OnDisabledState.BorderThickness = 1;
            this.btnStop.OnDisabledState.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(204)))), ((int)(((byte)(204)))), ((int)(((byte)(204)))));
            this.btnStop.OnDisabledState.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(168)))), ((int)(((byte)(160)))), ((int)(((byte)(168)))));
            this.btnStop.OnDisabledState.IconLeftImage = null;
            this.btnStop.OnDisabledState.IconRightImage = null;
            this.btnStop.onHoverState.BorderColor = System.Drawing.Color.DarkRed;
            this.btnStop.onHoverState.BorderRadius = 1;
            this.btnStop.onHoverState.BorderStyle = Bunifu.UI.WinForms.BunifuButton.BunifuButton.BorderStyles.Solid;
            this.btnStop.onHoverState.BorderThickness = 1;
            this.btnStop.onHoverState.FillColor = System.Drawing.Color.DarkRed;
            this.btnStop.onHoverState.ForeColor = System.Drawing.Color.White;
            this.btnStop.onHoverState.IconLeftImage = null;
            this.btnStop.onHoverState.IconRightImage = null;
            this.btnStop.OnIdleState.BorderColor = System.Drawing.Color.Firebrick;
            this.btnStop.OnIdleState.BorderRadius = 1;
            this.btnStop.OnIdleState.BorderStyle = Bunifu.UI.WinForms.BunifuButton.BunifuButton.BorderStyles.Solid;
            this.btnStop.OnIdleState.BorderThickness = 1;
            this.btnStop.OnIdleState.FillColor = System.Drawing.Color.Firebrick;
            this.btnStop.OnIdleState.ForeColor = System.Drawing.Color.White;
            this.btnStop.OnIdleState.IconLeftImage = null;
            this.btnStop.OnIdleState.IconRightImage = null;
            this.btnStop.OnPressedState.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(96)))), ((int)(((byte)(144)))));
            this.btnStop.OnPressedState.BorderRadius = 1;
            this.btnStop.OnPressedState.BorderStyle = Bunifu.UI.WinForms.BunifuButton.BunifuButton.BorderStyles.Solid;
            this.btnStop.OnPressedState.BorderThickness = 1;
            this.btnStop.OnPressedState.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(96)))), ((int)(((byte)(144)))));
            this.btnStop.OnPressedState.ForeColor = System.Drawing.Color.White;
            this.btnStop.OnPressedState.IconLeftImage = null;
            this.btnStop.OnPressedState.IconRightImage = null;
            this.btnStop.Size = new System.Drawing.Size(106, 32);
            this.btnStop.TabIndex = 7;
            this.btnStop.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.btnStop.TextAlignment = System.Windows.Forms.HorizontalAlignment.Center;
            this.btnStop.TextMarginLeft = 0;
            this.btnStop.TextPadding = new System.Windows.Forms.Padding(0);
            this.btnStop.UseDefaultRadiusAndThickness = true;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // linkLabel1
            // 
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.Location = new System.Drawing.Point(243, 132);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(80, 13);
            this.linkLabel1.TabIndex = 8;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "Target Manage";
            // 
            // txtMultiLog
            // 
            this.txtMultiLog.Location = new System.Drawing.Point(10, 151);
            this.txtMultiLog.Multiline = true;
            this.txtMultiLog.Name = "txtMultiLog";
            this.txtMultiLog.Size = new System.Drawing.Size(321, 272);
            this.txtMultiLog.TabIndex = 9;
            // 
            // timer1
            // 
            this.timer1.Interval = 60000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // comboBox1
            // 
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(10, 49);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(319, 21);
            this.comboBox1.TabIndex = 10;
            // 
            // startTimeDate
            // 
            this.startTimeDate.CustomFormat = "dd-MM-yyyy";
            this.startTimeDate.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.startTimeDate.Location = new System.Drawing.Point(10, 77);
            this.startTimeDate.Name = "startTimeDate";
            this.startTimeDate.Size = new System.Drawing.Size(80, 20);
            this.startTimeDate.TabIndex = 11;
            // 
            // startTimeTime
            // 
            this.startTimeTime.CustomFormat = "HH:mm";
            this.startTimeTime.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.startTimeTime.Location = new System.Drawing.Point(96, 77);
            this.startTimeTime.Name = "startTimeTime";
            this.startTimeTime.Size = new System.Drawing.Size(49, 20);
            this.startTimeTime.TabIndex = 12;
            // 
            // endTimeTime
            // 
            this.endTimeTime.CustomFormat = "HH:mm";
            this.endTimeTime.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.endTimeTime.Location = new System.Drawing.Point(280, 77);
            this.endTimeTime.Name = "endTimeTime";
            this.endTimeTime.Size = new System.Drawing.Size(49, 20);
            this.endTimeTime.TabIndex = 14;
            // 
            // endTimeDate
            // 
            this.endTimeDate.CustomFormat = "dd-MM-yyyy";
            this.endTimeDate.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.endTimeDate.Location = new System.Drawing.Point(194, 77);
            this.endTimeDate.Name = "endTimeDate";
            this.endTimeDate.Size = new System.Drawing.Size(80, 20);
            this.endTimeDate.TabIndex = 13;
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(237)))), ((int)(((byte)(240)))));
            this.ClientSize = new System.Drawing.Size(343, 435);
            this.Controls.Add(this.endTimeTime);
            this.Controls.Add(this.endTimeDate);
            this.Controls.Add(this.startTimeTime);
            this.Controls.Add(this.startTimeDate);
            this.Controls.Add(this.comboBox1);
            this.Controls.Add(this.txtMultiLog);
            this.Controls.Add(this.linkLabel1);
            this.Controls.Add(this.btnStop);
            this.Controls.Add(this.btnStart);
            this.Controls.Add(this.btnMinimize);
            this.Controls.Add(this.btnClose);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "Main";
            this.Text = "Main";
            this.Load += new System.EventHandler(this.Main_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Bunifu.Framework.UI.BunifuElipse bunifuElipse1;
        private Bunifu.Framework.UI.BunifuDragControl bunifuDragControl1;
        private Bunifu.UI.WinForms.BunifuImageButton btnMinimize;
        private Bunifu.UI.WinForms.BunifuImageButton btnClose;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private Bunifu.UI.WinForms.BunifuButton.BunifuButton btnStop;
        private Bunifu.UI.WinForms.BunifuButton.BunifuButton btnStart;
        private System.Windows.Forms.TextBox txtMultiLog;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.DateTimePicker startTimeDate;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.DateTimePicker startTimeTime;
        private System.Windows.Forms.DateTimePicker endTimeTime;
        private System.Windows.Forms.DateTimePicker endTimeDate;
    }
}