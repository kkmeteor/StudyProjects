using UFIDA.U8.UAP.Services.ReportExhibition;
namespace UFIDA.U8.Portal.ReportFacade
{
    partial class PortalViewControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PortalViewControl));
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.Previous = new System.Windows.Forms.ToolStripLabel();
            this.Next = new System.Windows.Forms.ToolStripLabel();
            this.tlbchart = new System.Windows.Forms.ToolStripButton();
            this.tlbmatrix = new System.Windows.Forms.ToolStripButton();
            this.tlbrefresh = new System.Windows.Forms.ToolStripButton();
            this.tlbquery = new System.Windows.Forms.ToolStripButton();
            this.tlbprint = new System.Windows.Forms.ToolStripButton();
            this.tlbprintview = new System.Windows.Forms.ToolStripButton();
            this.tlbsetting = new System.Windows.Forms.ToolStripButton();
            this.tlbtime = new System.Windows.Forms.ToolStripButton();
            this.tlcdimension = new System.Windows.Forms.ToolStripComboBox();
            this.tlcfilter = new System.Windows.Forms.ToolStripComboBox();
            this.picwait = new UFIDA.U8.UAP.Services.ReportExhibition.ViewTipControl();
            this.reportcontrol = new UFIDA.U8.UAP.Services.ReportExhibition.FreeReportControl();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStrip1
            // 
            this.toolStrip1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Previous,
            this.Next,
            this.tlbchart,
            this.tlbmatrix,
            this.tlbrefresh,
            this.tlbquery,
            this.tlbprint,
            this.tlbprintview,
            this.tlbsetting,
            this.tlbtime,
            this.tlcdimension,
            this.tlcfilter});
            this.toolStrip1.Location = new System.Drawing.Point(0, 287);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(413, 25);
            this.toolStrip1.TabIndex = 0;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // Previous
            // 
            this.Previous.ForeColor = System.Drawing.Color.Blue;
            this.Previous.Name = "Previous";
            this.Previous.Size = new System.Drawing.Size(17, 22);
            this.Previous.Text = "...";
            this.Previous.Visible = false;
            // 
            // Next
            // 
            this.Next.ForeColor = System.Drawing.Color.Blue;
            this.Next.Name = "Next";
            this.Next.Size = new System.Drawing.Size(17, 22);
            this.Next.Text = "...";
            this.Next.Visible = false;
            // 
            // tlbchart
            // 
            this.tlbchart.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.tlbchart.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tlbchart.Image = ((System.Drawing.Image)(resources.GetObject("tlbchart.Image")));
            this.tlbchart.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tlbchart.Name = "tlbchart";
            this.tlbchart.Size = new System.Drawing.Size(23, 22);
            this.tlbchart.Visible = false;
            this.tlbchart.Click += new System.EventHandler(this.tlbchart_Click);
            // 
            // tlbmatrix
            // 
            this.tlbmatrix.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.tlbmatrix.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tlbmatrix.Image = ((System.Drawing.Image)(resources.GetObject("tlbmatrix.Image")));
            this.tlbmatrix.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tlbmatrix.Name = "tlbmatrix";
            this.tlbmatrix.Size = new System.Drawing.Size(23, 22);
            this.tlbmatrix.Visible = false;
            this.tlbmatrix.Click += new System.EventHandler(this.tlbmatrix_Click);
            // 
            // tlbrefresh
            // 
            this.tlbrefresh.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.tlbrefresh.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tlbrefresh.Image = ((System.Drawing.Image)(resources.GetObject("tlbrefresh.Image")));
            this.tlbrefresh.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tlbrefresh.Name = "tlbrefresh";
            this.tlbrefresh.Size = new System.Drawing.Size(23, 22);
            this.tlbrefresh.Text = "À¢–¬";
            this.tlbrefresh.Click += new System.EventHandler(this.tlbrefresh_Click);
            // 
            // tlbquery
            // 
            this.tlbquery.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.tlbquery.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tlbquery.Image = ((System.Drawing.Image)(resources.GetObject("tlbquery.Image")));
            this.tlbquery.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tlbquery.Name = "tlbquery";
            this.tlbquery.Size = new System.Drawing.Size(23, 22);
            this.tlbquery.Text = "≤È—Ø";
            this.tlbquery.Click += new System.EventHandler(this.tlbquery_Click);
            // 
            // tlbprint
            // 
            this.tlbprint.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.tlbprint.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tlbprint.Image = ((System.Drawing.Image)(resources.GetObject("tlbprint.Image")));
            this.tlbprint.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tlbprint.Name = "tlbprint";
            this.tlbprint.Size = new System.Drawing.Size(23, 22);
            this.tlbprint.Text = "¥Ú”°";
            this.tlbprint.Click += new System.EventHandler(this.tlbprint_Click);
            // 
            // tlbprintview
            // 
            this.tlbprintview.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.tlbprintview.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tlbprintview.Image = ((System.Drawing.Image)(resources.GetObject("tlbprintview.Image")));
            this.tlbprintview.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tlbprintview.Name = "tlbprintview";
            this.tlbprintview.Size = new System.Drawing.Size(23, 22);
            this.tlbprintview.Text = "‘§¿¿";
            this.tlbprintview.Click += new System.EventHandler(this.tlbprintview_Click);
            // 
            // tlbsetting
            // 
            this.tlbsetting.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.tlbsetting.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tlbsetting.Image = ((System.Drawing.Image)(resources.GetObject("tlbsetting.Image")));
            this.tlbsetting.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tlbsetting.Name = "tlbsetting";
            this.tlbsetting.Size = new System.Drawing.Size(23, 22);
            this.tlbsetting.Click += new System.EventHandler(this.tlbsetting_Click);
            // 
            // tlbtime
            // 
            this.tlbtime.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.tlbtime.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tlbtime.Image = ((System.Drawing.Image)(resources.GetObject("tlbtime.Image")));
            this.tlbtime.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tlbtime.Name = "tlbtime";
            this.tlbtime.Size = new System.Drawing.Size(23, 22);
            // 
            // tlcdimension
            // 
            this.tlcdimension.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.tlcdimension.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.tlcdimension.Name = "tlcdimension";
            this.tlcdimension.Size = new System.Drawing.Size(75, 25);
            this.tlcdimension.Visible = false;
            // 
            // tlcfilter
            // 
            this.tlcfilter.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.tlcfilter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.tlcfilter.Name = "tlcfilter";
            this.tlcfilter.Size = new System.Drawing.Size(75, 25);
            this.tlcfilter.Visible = false;
            this.tlcfilter.DropDown += new System.EventHandler(this.tlcfilter_DropDown);
            this.tlcfilter.SelectedIndexChanged += new System.EventHandler(this.tlcfilter_SelectedIndexChanged);
            // 
            // picwait
            // 
            this.picwait.BackColor = System.Drawing.Color.White;
            this.picwait.Dock = System.Windows.Forms.DockStyle.Fill;
            this.picwait.Location = new System.Drawing.Point(0, 0);
            this.picwait.Name = "picwait";
            this.picwait.Size = new System.Drawing.Size(413, 287);
            this.picwait.TabIndex = 2;
            // 
            // reportcontrol
            // 
            this.reportcontrol.BackColor = System.Drawing.SystemColors.Control;
            this.reportcontrol.Dock = System.Windows.Forms.DockStyle.Fill;
            this.reportcontrol.Location = new System.Drawing.Point(0, 0);
            this.reportcontrol.Name = "reportcontrol";
            this.reportcontrol.Size = new System.Drawing.Size(413, 287);
            this.reportcontrol.TabIndex = 1;
            // 
            // PortalViewControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.picwait);
            this.Controls.Add(this.reportcontrol);
            this.Controls.Add(this.toolStrip1);
            this.Name = "PortalViewControl";
            this.Size = new System.Drawing.Size(413, 312);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip1;
        private FreeReportControl reportcontrol;
        private ViewTipControl picwait;
        private System.Windows.Forms.ToolStripButton tlbprint;
        private System.Windows.Forms.ToolStripButton tlbprintview;
        private System.Windows.Forms.ToolStripButton tlbrefresh;
        private System.Windows.Forms.ToolStripComboBox tlcfilter;
        private System.Windows.Forms.ToolStripButton tlbtime;
        private System.Windows.Forms.ToolStripLabel Previous;
        private System.Windows.Forms.ToolStripLabel Next;
        private System.Windows.Forms.ToolStripButton tlbmatrix;
        private System.Windows.Forms.ToolStripButton tlbchart;
        private System.Windows.Forms.ToolStripButton tlbsetting;
        private System.Windows.Forms.ToolStripComboBox tlcdimension;
        private System.Windows.Forms.ToolStripButton tlbquery;
    }
}
