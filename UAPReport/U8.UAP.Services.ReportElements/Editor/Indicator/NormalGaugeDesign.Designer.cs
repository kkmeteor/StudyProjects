namespace UFIDA.U8.UAP.Services.ReportElements
{
    partial class NormalGaugeDesign
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
            this.sectionstart = new System.Windows.Forms.NumericUpDown();
            this.sectionend = new System.Windows.Forms.NumericUpDown();
            this.colorDialog1 = new System.Windows.Forms.ColorDialog();
            this.linecolor = new System.Windows.Forms.ComboBox();
            this.tickcolor = new System.Windows.Forms.ComboBox();
            this.fontcolor = new System.Windows.Forms.ComboBox();
            this.btnok = new System.Windows.Forms.Button();
            this.btncancel = new System.Windows.Forms.Button();
            this.lbllinecolor = new System.Windows.Forms.Label();
            this.lbltickcolor = new System.Windows.Forms.Label();
            this.lblfontcolor = new System.Windows.Forms.Label();
            this.lblsectionstart = new System.Windows.Forms.Label();
            this.lblsectionend = new System.Windows.Forms.Label();
            this.lbltickend = new System.Windows.Forms.Label();
            this.lbltickstart = new System.Windows.Forms.Label();
            this.tickend = new System.Windows.Forms.NumericUpDown();
            this.tickstart = new System.Windows.Forms.NumericUpDown();
            this.lbltextloc = new System.Windows.Forms.Label();
            this.textloc = new System.Windows.Forms.NumericUpDown();
            this.lblmaxtick = new System.Windows.Forms.Label();
            this.maxtick = new System.Windows.Forms.NumericUpDown();
            this.rbcircle = new System.Windows.Forms.RadioButton();
            this.rbsemicircle = new System.Windows.Forms.RadioButton();
            this.lblbackcolor = new System.Windows.Forms.Label();
            this.backcolor = new System.Windows.Forms.ComboBox();
            this.lblmintick = new System.Windows.Forms.Label();
            this.mintick = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.sectionstart)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.sectionend)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tickend)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tickstart)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.textloc)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.maxtick)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.mintick)).BeginInit();
            this.SuspendLayout();
            // 
            // sectionstart
            // 
            this.sectionstart.Location = new System.Drawing.Point(100, 78);
            this.sectionstart.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.sectionstart.Name = "sectionstart";
            this.sectionstart.Size = new System.Drawing.Size(75, 21);
            this.sectionstart.TabIndex = 0;
            this.sectionstart.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // sectionend
            // 
            this.sectionend.Location = new System.Drawing.Point(281, 78);
            this.sectionend.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.sectionend.Name = "sectionend";
            this.sectionend.Size = new System.Drawing.Size(75, 21);
            this.sectionend.TabIndex = 1;
            this.sectionend.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // colorDialog1
            // 
            this.colorDialog1.FullOpen = true;
            // 
            // linecolor
            // 
            this.linecolor.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.linecolor.FormattingEnabled = true;
            this.linecolor.Location = new System.Drawing.Point(100, 9);
            this.linecolor.Name = "linecolor";
            this.linecolor.Size = new System.Drawing.Size(76, 20);
            this.linecolor.TabIndex = 2;
            this.linecolor.Click += new System.EventHandler(this.linecolor_Click);
            // 
            // tickcolor
            // 
            this.tickcolor.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.tickcolor.FormattingEnabled = true;
            this.tickcolor.Location = new System.Drawing.Point(282, 9);
            this.tickcolor.Name = "tickcolor";
            this.tickcolor.Size = new System.Drawing.Size(75, 20);
            this.tickcolor.TabIndex = 3;
            this.tickcolor.Click += new System.EventHandler(this.linecolor_Click);
            // 
            // fontcolor
            // 
            this.fontcolor.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.fontcolor.FormattingEnabled = true;
            this.fontcolor.Location = new System.Drawing.Point(467, 9);
            this.fontcolor.Name = "fontcolor";
            this.fontcolor.Size = new System.Drawing.Size(76, 20);
            this.fontcolor.TabIndex = 4;
            this.fontcolor.Click += new System.EventHandler(this.linecolor_Click);
            // 
            // btnok
            // 
            this.btnok.Location = new System.Drawing.Point(407, 152);
            this.btnok.Name = "btnok";
            this.btnok.Size = new System.Drawing.Size(68, 23);
            this.btnok.TabIndex = 5;
            this.btnok.Text = "OK";
            this.btnok.UseVisualStyleBackColor = true;
            this.btnok.Click += new System.EventHandler(this.button1_Click);
            // 
            // btncancel
            // 
            this.btncancel.Location = new System.Drawing.Point(481, 152);
            this.btncancel.Name = "btncancel";
            this.btncancel.Size = new System.Drawing.Size(61, 23);
            this.btncancel.TabIndex = 6;
            this.btncancel.Text = "Cancel";
            this.btncancel.UseVisualStyleBackColor = true;
            this.btncancel.Click += new System.EventHandler(this.btncancel_Click);
            // 
            // lbllinecolor
            // 
            this.lbllinecolor.AutoSize = true;
            this.lbllinecolor.Location = new System.Drawing.Point(12, 12);
            this.lbllinecolor.Name = "lbllinecolor";
            this.lbllinecolor.Size = new System.Drawing.Size(65, 12);
            this.lbllinecolor.TabIndex = 7;
            this.lbllinecolor.Text = "Line Color";
            // 
            // lbltickcolor
            // 
            this.lbltickcolor.AutoSize = true;
            this.lbltickcolor.Location = new System.Drawing.Point(201, 12);
            this.lbltickcolor.Name = "lbltickcolor";
            this.lbltickcolor.Size = new System.Drawing.Size(65, 12);
            this.lbltickcolor.TabIndex = 8;
            this.lbltickcolor.Text = "Tick Color";
            // 
            // lblfontcolor
            // 
            this.lblfontcolor.AutoSize = true;
            this.lblfontcolor.Location = new System.Drawing.Point(387, 12);
            this.lblfontcolor.Name = "lblfontcolor";
            this.lblfontcolor.Size = new System.Drawing.Size(65, 12);
            this.lblfontcolor.TabIndex = 9;
            this.lblfontcolor.Text = "Font Color";
            // 
            // lblsectionstart
            // 
            this.lblsectionstart.AutoSize = true;
            this.lblsectionstart.Location = new System.Drawing.Point(11, 80);
            this.lblsectionstart.Name = "lblsectionstart";
            this.lblsectionstart.Size = new System.Drawing.Size(83, 12);
            this.lblsectionstart.TabIndex = 10;
            this.lblsectionstart.Text = "Section Start";
            // 
            // lblsectionend
            // 
            this.lblsectionend.AutoSize = true;
            this.lblsectionend.Location = new System.Drawing.Point(200, 80);
            this.lblsectionend.Name = "lblsectionend";
            this.lblsectionend.Size = new System.Drawing.Size(71, 12);
            this.lblsectionend.TabIndex = 11;
            this.lblsectionend.Text = "Section End";
            // 
            // lbltickend
            // 
            this.lbltickend.AutoSize = true;
            this.lbltickend.Location = new System.Drawing.Point(201, 44);
            this.lbltickend.Name = "lbltickend";
            this.lbltickend.Size = new System.Drawing.Size(53, 12);
            this.lbltickend.TabIndex = 15;
            this.lbltickend.Text = "Tick End";
            // 
            // lbltickstart
            // 
            this.lbltickstart.AutoSize = true;
            this.lbltickstart.Location = new System.Drawing.Point(12, 44);
            this.lbltickstart.Name = "lbltickstart";
            this.lbltickstart.Size = new System.Drawing.Size(65, 12);
            this.lbltickstart.TabIndex = 14;
            this.lbltickstart.Text = "Tick Start";
            // 
            // tickend
            // 
            this.tickend.Location = new System.Drawing.Point(282, 42);
            this.tickend.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.tickend.Name = "tickend";
            this.tickend.Size = new System.Drawing.Size(75, 21);
            this.tickend.TabIndex = 13;
            this.tickend.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // tickstart
            // 
            this.tickstart.Location = new System.Drawing.Point(101, 42);
            this.tickstart.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.tickstart.Name = "tickstart";
            this.tickstart.Size = new System.Drawing.Size(75, 21);
            this.tickstart.TabIndex = 12;
            this.tickstart.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // lbltextloc
            // 
            this.lbltextloc.AutoSize = true;
            this.lbltextloc.Location = new System.Drawing.Point(386, 44);
            this.lbltextloc.Name = "lbltextloc";
            this.lbltextloc.Size = new System.Drawing.Size(53, 12);
            this.lbltextloc.TabIndex = 17;
            this.lbltextloc.Text = "Text Loc";
            // 
            // textloc
            // 
            this.textloc.Location = new System.Drawing.Point(468, 42);
            this.textloc.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.textloc.Name = "textloc";
            this.textloc.Size = new System.Drawing.Size(75, 21);
            this.textloc.TabIndex = 16;
            this.textloc.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // lblmaxtick
            // 
            this.lblmaxtick.AutoSize = true;
            this.lblmaxtick.Location = new System.Drawing.Point(12, 116);
            this.lblmaxtick.Name = "lblmaxtick";
            this.lblmaxtick.Size = new System.Drawing.Size(47, 12);
            this.lblmaxtick.TabIndex = 19;
            this.lblmaxtick.Text = "MaxTick";
            // 
            // maxtick
            // 
            this.maxtick.Location = new System.Drawing.Point(101, 114);
            this.maxtick.Maximum = new decimal(new int[] {
            10000000,
            0,
            0,
            0});
            this.maxtick.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.maxtick.Name = "maxtick";
            this.maxtick.Size = new System.Drawing.Size(75, 21);
            this.maxtick.TabIndex = 18;
            this.maxtick.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // rbcircle
            // 
            this.rbcircle.AutoSize = true;
            this.rbcircle.Checked = true;
            this.rbcircle.Location = new System.Drawing.Point(202, 155);
            this.rbcircle.Name = "rbcircle";
            this.rbcircle.Size = new System.Drawing.Size(59, 16);
            this.rbcircle.TabIndex = 20;
            this.rbcircle.TabStop = true;
            this.rbcircle.Text = "Circle";
            this.rbcircle.UseVisualStyleBackColor = true;
            // 
            // rbsemicircle
            // 
            this.rbsemicircle.AutoSize = true;
            this.rbsemicircle.Location = new System.Drawing.Point(282, 155);
            this.rbsemicircle.Name = "rbsemicircle";
            this.rbsemicircle.Size = new System.Drawing.Size(89, 16);
            this.rbsemicircle.TabIndex = 21;
            this.rbsemicircle.Text = "Semi Circle";
            this.rbsemicircle.UseVisualStyleBackColor = true;
            // 
            // lblbackcolor
            // 
            this.lblbackcolor.AutoSize = true;
            this.lblbackcolor.Location = new System.Drawing.Point(12, 152);
            this.lblbackcolor.Name = "lblbackcolor";
            this.lblbackcolor.Size = new System.Drawing.Size(71, 12);
            this.lblbackcolor.TabIndex = 23;
            this.lblbackcolor.Text = "Gauge Color";
            // 
            // backcolor
            // 
            this.backcolor.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.backcolor.FormattingEnabled = true;
            this.backcolor.Location = new System.Drawing.Point(100, 149);
            this.backcolor.Name = "backcolor";
            this.backcolor.Size = new System.Drawing.Size(76, 20);
            this.backcolor.TabIndex = 22;
            this.backcolor.Click += new System.EventHandler(this.linecolor_Click);
            // 
            // lblmintick
            // 
            this.lblmintick.AutoSize = true;
            this.lblmintick.Location = new System.Drawing.Point(201, 116);
            this.lblmintick.Name = "lblmintick";
            this.lblmintick.Size = new System.Drawing.Size(47, 12);
            this.lblmintick.TabIndex = 25;
            this.lblmintick.Text = "MinTick";
            this.lblmintick.Visible = false;
            // 
            // mintick
            // 
            this.mintick.Location = new System.Drawing.Point(279, 111);
            this.mintick.Maximum = new decimal(new int[] {
            10000000,
            0,
            0,
            0});
            this.mintick.Name = "mintick";
            this.mintick.Size = new System.Drawing.Size(78, 21);
            this.mintick.TabIndex = 24;
            this.mintick.Visible = false;
            // 
            // NormalGaugeDesign
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(553, 179);
            this.Controls.Add(this.lblmintick);
            this.Controls.Add(this.mintick);
            this.Controls.Add(this.lblbackcolor);
            this.Controls.Add(this.backcolor);
            this.Controls.Add(this.rbsemicircle);
            this.Controls.Add(this.rbcircle);
            this.Controls.Add(this.lblmaxtick);
            this.Controls.Add(this.maxtick);
            this.Controls.Add(this.lbltextloc);
            this.Controls.Add(this.textloc);
            this.Controls.Add(this.lbltickend);
            this.Controls.Add(this.lbltickstart);
            this.Controls.Add(this.tickend);
            this.Controls.Add(this.tickstart);
            this.Controls.Add(this.lblsectionend);
            this.Controls.Add(this.lblsectionstart);
            this.Controls.Add(this.lblfontcolor);
            this.Controls.Add(this.lbltickcolor);
            this.Controls.Add(this.lbllinecolor);
            this.Controls.Add(this.btncancel);
            this.Controls.Add(this.btnok);
            this.Controls.Add(this.fontcolor);
            this.Controls.Add(this.tickcolor);
            this.Controls.Add(this.linecolor);
            this.Controls.Add(this.sectionend);
            this.Controls.Add(this.sectionstart);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "NormalGaugeDesign";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Gauge Detail";
            ((System.ComponentModel.ISupportInitialize)(this.sectionstart)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.sectionend)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tickend)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tickstart)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.textloc)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.maxtick)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.mintick)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.NumericUpDown sectionstart;
        private System.Windows.Forms.NumericUpDown sectionend;
        private System.Windows.Forms.ColorDialog colorDialog1;
        private System.Windows.Forms.ComboBox linecolor;
        private System.Windows.Forms.ComboBox tickcolor;
        private System.Windows.Forms.ComboBox fontcolor;
        private System.Windows.Forms.Button btnok;
        private System.Windows.Forms.Button btncancel;
        private System.Windows.Forms.Label lbllinecolor;
        private System.Windows.Forms.Label lbltickcolor;
        private System.Windows.Forms.Label lblfontcolor;
        private System.Windows.Forms.Label lblsectionstart;
        private System.Windows.Forms.Label lblsectionend;
        private System.Windows.Forms.Label lbltickend;
        private System.Windows.Forms.Label lbltickstart;
        private System.Windows.Forms.NumericUpDown tickend;
        private System.Windows.Forms.NumericUpDown tickstart;
        private System.Windows.Forms.Label lbltextloc;
        private System.Windows.Forms.NumericUpDown textloc;
        private System.Windows.Forms.Label lblmaxtick;
        private System.Windows.Forms.NumericUpDown maxtick;
        private System.Windows.Forms.RadioButton rbcircle;
        private System.Windows.Forms.RadioButton rbsemicircle;
        private System.Windows.Forms.Label lblbackcolor;
        private System.Windows.Forms.ComboBox backcolor;
        private System.Windows.Forms.Label lblmintick;
        private System.Windows.Forms.NumericUpDown mintick;
    }
}