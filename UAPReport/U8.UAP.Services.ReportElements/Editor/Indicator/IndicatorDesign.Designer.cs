namespace UFIDA.U8.UAP.Services.ReportElements
{
    partial class IndicatorDesign
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(IndicatorDesign));
            this.btnok = new System.Windows.Forms.Button();
            this.btncancel = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabdetail = new System.Windows.Forms.TabPage();
            this.detailcompare = new UFIDA.U8.UAP.Services.ReportElements.CompareValueControl();
            this.tabsummary = new System.Windows.Forms.TabPage();
            this.summarycompare = new UFIDA.U8.UAP.Services.ReportElements.CompareValueControl();
            this.tabrowtotal = new System.Windows.Forms.TabPage();
            this.totalcompare = new UFIDA.U8.UAP.Services.ReportElements.CompareValueControl();
            this.tabControl1.SuspendLayout();
            this.tabdetail.SuspendLayout();
            this.tabsummary.SuspendLayout();
            this.tabrowtotal.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnok
            // 
            this.btnok.Location = new System.Drawing.Point(355, 329);
            this.btnok.Name = "btnok";
            this.btnok.Size = new System.Drawing.Size(75, 23);
            this.btnok.TabIndex = 28;
            this.btnok.Text = "OK";
            this.btnok.UseVisualStyleBackColor = true;
            this.btnok.Click += new System.EventHandler(this.btnok_Click);
            // 
            // btncancel
            // 
            this.btncancel.Location = new System.Drawing.Point(436, 329);
            this.btncancel.Name = "btncancel";
            this.btncancel.Size = new System.Drawing.Size(75, 23);
            this.btncancel.TabIndex = 29;
            this.btncancel.Text = "Cancel";
            this.btncancel.UseVisualStyleBackColor = true;
            this.btncancel.Click += new System.EventHandler(this.btncancel_Click);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabdetail);
            this.tabControl1.Controls.Add(this.tabsummary);
            this.tabControl1.Controls.Add(this.tabrowtotal);
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(515, 327);
            this.tabControl1.TabIndex = 30;
            // 
            // tabdetail
            // 
            this.tabdetail.Controls.Add(this.detailcompare);
            this.tabdetail.Location = new System.Drawing.Point(4, 21);
            this.tabdetail.Name = "tabdetail";
            this.tabdetail.Padding = new System.Windows.Forms.Padding(3);
            this.tabdetail.Size = new System.Drawing.Size(507, 302);
            this.tabdetail.TabIndex = 0;
            this.tabdetail.Text = "Detail";
            this.tabdetail.UseVisualStyleBackColor = true;
            // 
            // detailcompare
            // 
            this.detailcompare.Location = new System.Drawing.Point(3, 3);
            this.detailcompare.Name = "detailcompare";
            this.detailcompare.Size = new System.Drawing.Size(596, 354);
            this.detailcompare.TabIndex = 0;
            // 
            // tabsummary
            // 
            this.tabsummary.Controls.Add(this.summarycompare);
            this.tabsummary.Location = new System.Drawing.Point(4, 21);
            this.tabsummary.Name = "tabsummary";
            this.tabsummary.Padding = new System.Windows.Forms.Padding(3);
            this.tabsummary.Size = new System.Drawing.Size(507, 302);
            this.tabsummary.TabIndex = 1;
            this.tabsummary.Text = "Summary";
            this.tabsummary.UseVisualStyleBackColor = true;
            // 
            // summarycompare
            // 
            this.summarycompare.Location = new System.Drawing.Point(3, 3);
            this.summarycompare.Name = "summarycompare";
            this.summarycompare.Size = new System.Drawing.Size(596, 354);
            this.summarycompare.TabIndex = 0;
            // 
            // tabrowtotal
            // 
            this.tabrowtotal.Controls.Add(this.totalcompare);
            this.tabrowtotal.Location = new System.Drawing.Point(4, 21);
            this.tabrowtotal.Name = "tabrowtotal";
            this.tabrowtotal.Size = new System.Drawing.Size(507, 302);
            this.tabrowtotal.TabIndex = 2;
            this.tabrowtotal.Text = "Row total";
            this.tabrowtotal.UseVisualStyleBackColor = true;
            // 
            // totalcompare
            // 
            this.totalcompare.Location = new System.Drawing.Point(3, 3);
            this.totalcompare.Name = "totalcompare";
            this.totalcompare.Size = new System.Drawing.Size(596, 354);
            this.totalcompare.TabIndex = 0;
            // 
            // IndicatorDesign
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(516, 355);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.btncancel);
            this.Controls.Add(this.btnok);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "IndicatorDesign";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Indicator design";
            this.Load += new System.EventHandler(this.IndicatorDesign_Load);
            this.tabControl1.ResumeLayout(false);
            this.tabdetail.ResumeLayout(false);
            this.tabsummary.ResumeLayout(false);
            this.tabrowtotal.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnok;
        private System.Windows.Forms.Button btncancel;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabdetail;
        private System.Windows.Forms.TabPage tabsummary;
        private System.Windows.Forms.TabPage tabrowtotal;
        private CompareValueControl compareValueControl1;
        private CompareValueControl detailcompare;
        private CompareValueControl summarycompare;
        private CompareValueControl totalcompare;
    }
}