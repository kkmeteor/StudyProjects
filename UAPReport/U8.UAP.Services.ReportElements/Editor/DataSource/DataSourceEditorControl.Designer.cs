namespace UFIDA.U8.UAP.Services.ReportElements
{
    partial class DataSourceEditorControl
    {
        /// <summary> 
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 组件设计器生成的代码

        /// <summary> 
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DataSourceEditorControl));
            this.datasourcelist = new System.Windows.Forms.ListView();
            this.txtsearch = new System.Windows.Forms.TextBox();
            this.btSearch = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // datasourcelist
            // 
            this.datasourcelist.AllowDrop = true;
            this.datasourcelist.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.datasourcelist.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.datasourcelist.CheckBoxes = true;
            this.datasourcelist.HideSelection = false;
            this.datasourcelist.Location = new System.Drawing.Point(3, 3);
            this.datasourcelist.MultiSelect = false;
            this.datasourcelist.Name = "datasourcelist";
            this.datasourcelist.Size = new System.Drawing.Size(194, 202);
            this.datasourcelist.TabIndex = 0;
            this.datasourcelist.UseCompatibleStateImageBehavior = false;
            this.datasourcelist.View = System.Windows.Forms.View.Details;
            // 
            // txtsearch
            // 
            this.txtsearch.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtsearch.Location = new System.Drawing.Point(3, 213);
            this.txtsearch.Name = "txtsearch";
            this.txtsearch.Size = new System.Drawing.Size(170, 21);
            this.txtsearch.TabIndex = 1;
            // 
            // btSearch
            // 
            this.btSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btSearch.Image = ((System.Drawing.Image)(resources.GetObject("btSearch.Image")));
            this.btSearch.Location = new System.Drawing.Point(173, 212);
            this.btSearch.Name = "btSearch";
            this.btSearch.Size = new System.Drawing.Size(24, 23);
            this.btSearch.TabIndex = 2;
            this.btSearch.UseVisualStyleBackColor = true;
            this.btSearch.Click += new System.EventHandler(this.btSearch_Click);
            // 
            // DataSourceEditorControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.btSearch);
            this.Controls.Add(this.txtsearch);
            this.Controls.Add(this.datasourcelist);
            this.MinimumSize = new System.Drawing.Size(200, 0);
            this.Name = "DataSourceEditorControl";
            this.Size = new System.Drawing.Size(200, 237);
            this.Load += new System.EventHandler(this.DataSourceEditorControl_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView datasourcelist;
        private System.Windows.Forms.TextBox txtsearch;
        private System.Windows.Forms.Button btSearch;
    }
}
