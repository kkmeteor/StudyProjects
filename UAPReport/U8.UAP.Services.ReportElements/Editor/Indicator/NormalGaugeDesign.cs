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
    public partial class NormalGaugeDesign : Form
    {
        private Gauge _gauge;
        public NormalGaugeDesign()
        {
            InitializeComponent();
            this.lblbackcolor.Text = GaugeColor;
            this.lblfontcolor.Text = FontColor;
            this.lbllinecolor.Text = LineColor;
            this.lblmaxtick.Text = MaxTick;
            this.lblmintick.Text  = MinTick;
            this.lblsectionend.Text = SectionEnd;
            this.lblsectionstart.Text = SectionStart;
            this.lbltextloc.Text = TickLoc;
            this.lbltickcolor.Text = TickColor;
            this.lbltickend.Text = TickEnd;
            this.lbltickstart.Text = TickStart;
            this.rbcircle.Text = Circle;
            this.rbsemicircle.Text = SemiCircle;
            this.btncancel.Text = Cancel;
            this.btnok.Text = OK;
            this.Text = Title;
        }

        public NormalGaugeDesign(Gauge gauge):this()
        {
            _gauge = gauge;
            linecolor.BackColor = _gauge.LineColor;
            fontcolor.BackColor = _gauge.FontColor;
            tickcolor.BackColor = _gauge.TickColor;
            sectionstart.Value = _gauge.SectionStart;
            sectionend.Value = _gauge.SectionEnd;
            tickstart.Value = _gauge.TickStart;
            tickend.Value = _gauge.TickEnd;
            textloc.Value = _gauge.TextLoc;
            maxtick.Value = Convert.ToDecimal(_gauge.MaxTick);
            mintick.Value = Convert.ToDecimal(_gauge.MinTick);
            backcolor.BackColor = _gauge.GaugeColor;
            rbcircle.Checked = _gauge.bSemiCircle ? false : true;
            rbsemicircle.Checked = _gauge.bSemiCircle ? true : false;
            if (gauge.GaugeType == GaugeType.Blue3D)
            {
                backcolor.Visible = false;
                rbcircle.Visible = false;
                rbsemicircle.Visible = false;
                lblbackcolor.Visible = false;
            }
            else
            {
                backcolor.Visible = true;
                rbcircle.Visible = true;
                rbsemicircle.Visible = true;
                lblbackcolor.Visible = true;
            }
        }

        public Gauge Gauge
        {
            get
            {
                return _gauge;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (Convert.ToInt32(sectionstart.Value) >= Convert.ToInt32(sectionend.Value))
            {
                MessageBox.Show(SectionStartEndError, String4Report.GetString("Report"));
                return;
            }
            if (Convert.ToInt32(tickstart.Value) >= Convert.ToInt32(tickend.Value))
            {
                MessageBox.Show(TickStartEndError, String4Report.GetString("Report"));
                return;
            }
            _gauge.LineColor = linecolor.BackColor;
            _gauge.FontColor = fontcolor.BackColor;
            _gauge.TickColor = tickcolor.BackColor;
            _gauge.SectionStart = Convert.ToInt32(sectionstart.Value);
            _gauge.SectionEnd = Convert.ToInt32(sectionend.Value);
            _gauge.TickStart = Convert.ToInt32(tickstart.Value);
            _gauge.TickEnd = Convert.ToInt32(tickend.Value);
            _gauge.TextLoc = Convert.ToInt32(textloc.Value);
            _gauge.MaxTick = Convert.ToDouble (maxtick.Value);
            //_gauge.MinTick = Convert.ToDouble(mintick.Value);
            _gauge.GaugeColor  = backcolor.BackColor;
            _gauge.bSemiCircle = rbsemicircle.Checked;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btncancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void linecolor_Click(object sender, EventArgs e)
        {
            ComboBox cb = sender as ComboBox;
            if (this.colorDialog1.ShowDialog() == DialogResult.OK)
                cb.BackColor = this.colorDialog1.Color;
            btnok.Focus();
        }

        #region resource

        public string SectionStartEndError
        {
            get
            {
                switch (System.Threading.Thread.CurrentThread.CurrentUICulture.Name)
                {
                    case "en-US":
                        return "Section start must be less than section end";
                    case "zh-TW":
                        return "區間開始必須小於區間結束";
                    default:
                        return "区间开始必须小于区间结束";
                }
            }
        }
        public string TickStartEndError
        {
            get
            {
                switch (System.Threading.Thread.CurrentThread.CurrentUICulture.Name)
                {
                    case "en-US":
                        return "Tick start must be less than tick end";
                    case "zh-TW":
                        return "刻度開始必須小於刻度結束";
                    default:
                        return "刻度开始必须小于刻度结束";
                }
            }
        }
        public string LineColor
        {
            get
            {
                switch (System.Threading.Thread.CurrentThread.CurrentUICulture.Name)
                {
                    case "en-US":
                        return "Line Color";
                    case "zh-TW":
                        return "線條顏色";
                    default:
                        return "线条颜色";
                }
            }
        }

        public string TickColor
        {
            get
            {
                switch (System.Threading.Thread.CurrentThread.CurrentUICulture.Name)
                {
                    case "en-US":
                        return "Tick Color";
                    case "zh-TW":
                        return "刻度顏色";
                    default:
                        return "刻度颜色";
                }
            }
        }

        public string FontColor
        {
            get
            {
                switch (System.Threading.Thread.CurrentThread.CurrentUICulture.Name)
                {
                    case "en-US":
                        return "Font Color";
                    case "zh-TW":
                        return "字體顏色";
                    default:
                        return "字体颜色";
                }
            }
        }

        public string TickStart
        {
            get
            {
                switch (System.Threading.Thread.CurrentThread.CurrentUICulture.Name)
                {
                    case "en-US":
                        return "Tick Start";
                    case "zh-TW":
                        return "刻度開始於";
                    default:
                        return "刻度开始于";
                }
            }
        }

        public string TickEnd
        {
            get
            {
                switch (System.Threading.Thread.CurrentThread.CurrentUICulture.Name)
                {
                    case "en-US":
                        return "Tick End";
                    case "zh-TW":
                        return "刻度結束於";
                    default:
                        return "刻度结束于";
                }
            }
        }

        public string TickLoc
        {
            get
            {
                switch (System.Threading.Thread.CurrentThread.CurrentUICulture.Name)
                {
                    case "en-US":
                        return "Tick Loc";
                    case "zh-TW":
                        return "刻度位置";
                    default:
                        return "刻度位置";
                }
            }
        }

        public string SectionStart
        {
            get
            {
                switch (System.Threading.Thread.CurrentThread.CurrentUICulture.Name)
                {
                    case "en-US":
                        return "Section Start";
                    case "zh-TW":
                        return "區間開始於";
                    default:
                        return "区间开始于";
                }
            }
        }

        public string SectionEnd
        {
            get
            {
                switch (System.Threading.Thread.CurrentThread.CurrentUICulture.Name)
                {
                    case "en-US":
                        return "Section End";
                    case "zh-TW":
                        return "區間結束於";
                    default:
                        return "区间结束于";
                }
            }
        }

        public string MaxTick
        {
            get
            {
                switch (System.Threading.Thread.CurrentThread.CurrentUICulture.Name)
                {
                    case "en-US":
                        return "Max Tick";
                    case "zh-TW":
                        return "最大刻度";
                    default:
                        return "最大刻度";
                }
            }
        }

        public string MinTick
        {
            get
            {
                switch (System.Threading.Thread.CurrentThread.CurrentUICulture.Name)
                {
                    case "en-US":
                        return "Min Tick";
                    case "zh-TW":
                        return "最小刻度";
                    default:
                        return "最小刻度";
                }
            }
        }

        public string GaugeColor
        {
            get
            {
                switch (System.Threading.Thread.CurrentThread.CurrentUICulture.Name)
                {
                    case "en-US":
                        return "Gauge Color";
                    case "zh-TW":
                        return "盤面顏色";
                    default:
                        return "盘面颜色";
                }
            }
        }

        public string Circle
        {
            get
            {
                switch (System.Threading.Thread.CurrentThread.CurrentUICulture.Name)
                {
                    case "en-US":
                        return "Circle";
                    case "zh-TW":
                        return "整圓";
                    default:
                        return "整圆";
                }
            }
        }

        public string SemiCircle
        {
            get
            {
                switch (System.Threading.Thread.CurrentThread.CurrentUICulture.Name)
                {
                    case "en-US":
                        return "SemiCircle";
                    case "zh-TW":
                        return "半圓";
                    default:
                        return "半圆";
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

        public string Title
        {
            get
            {
                switch (System.Threading.Thread.CurrentThread.CurrentUICulture.Name)
                {
                    case "en-US":
                        return "Guage Design";
                    case "zh-TW":
                        return "儀錶設計";
                    default:
                        return "仪表设计";
                }
            }
        }
        #endregion
    }
}