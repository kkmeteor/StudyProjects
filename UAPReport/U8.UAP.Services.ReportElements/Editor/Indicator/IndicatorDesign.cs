using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using UFIDA.U8.UAP.Services.ReportResource;

namespace UFIDA.U8.UAP.Services.ReportElements
{
    public partial class IndicatorDesign : Form
    {
        public IndicatorDesign()
        {
            InitializeComponent();
            totalcompare.NotShowFilter();
            summarycompare.NotShowFilter();
            totalcompare.NoPop();
            this.Text = Title;
            this.btncancel.Text = Cancel;
            this.btnok.Text = OK;
            this.tabdetail.Text = Detail;
            this.tabrowtotal.Text  = RowTotal;
            this.tabsummary.Text = Summary;
        }

        public void Init(Cell cell,CompareValue dcv,CompareValue scv,CompareValue tcv)
        {
            detailcompare.Init(cell, dcv);
            if (dcv == null)
                tabControl1.Controls.Remove(tabdetail);

            totalcompare.Init(cell, tcv);
            if (tcv == null)
            {
                tabControl1.Controls.Remove(tabrowtotal);
                detailcompare.NoPop();
                summarycompare.NoPop();
            }
                
            summarycompare.Init(cell, scv);
        }

        private void IndicatorDesign_Load(object sender, EventArgs e)
        {
        }

        private void btnok_Click(object sender, EventArgs e)
        {
            try
            {
                detailcompare.EndEdit();
            }
            catch (Exception ex)
            {
                //tabControl1.SelectedIndex =0;
                MessageBox.Show(ex.Message, String4Report.GetString("Report"));
                return;
            }

            try
            {
                totalcompare.EndEdit();
            }
            catch (Exception ex)
            {
                //tabControl1.SelectedIndex = 2;
                MessageBox.Show(ex.Message, String4Report.GetString("Report"));
                return;
            }

            try
            {
                summarycompare.EndEdit();
            }
            catch (Exception ex)
            {
                //tabControl1.SelectedIndex = 1;
                MessageBox.Show(ex.Message, String4Report.GetString("Report"));
                return;
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btncancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        #region resource
        public string Title
        {
            get
            {
                switch (System.Threading.Thread.CurrentThread.CurrentUICulture.Name)
                {
                    case "en-US":
                        return "Indicator Design";
                    case "zh-TW":
                        return "指標設計";
                    default:
                        return "指标设计";
                }
            }
        }

        public string OK
        {
            get
            {
                switch (System.Threading.Thread.CurrentThread.CurrentUICulture.Name)
                {
                    case "en-US":
                        return "OK";
                    case "zh-TW":
                        return "確定";
                    default:
                        return "确定";
                }
            }
        }

        public string Cancel
        {
            get
            {
                switch (System.Threading.Thread.CurrentThread.CurrentUICulture.Name)
                {
                    case "en-US":
                        return "Cancel";
                    case "zh-TW":
                        return "取消";
                    default:
                        return "取消";
                }
            }
        }

        public string Detail
        {
            get
            {
                switch (System.Threading.Thread.CurrentThread.CurrentUICulture.Name)
                {
                    case "en-US":
                        return "Detail";
                    case "zh-TW":
                        return "明細";
                    default:
                        return "明细";
                }
            }
        }

        public string Summary
        {
            get
            {
                switch (System.Threading.Thread.CurrentThread.CurrentUICulture.Name)
                {
                    case "en-US":
                        return "Summary";
                    case "zh-TW":
                        return "匯總";
                    default:
                        return "汇总";
                }
            }
        }

        public string RowTotal
        {
            get
            {
                switch (System.Threading.Thread.CurrentThread.CurrentUICulture.Name)
                {
                    case "en-US":
                        return "Row Summary";
                    case "zh-TW":
                        return "行匯總";
                    default:
                        return "行汇总";
                }
            }
        }
        #endregion
    }
}