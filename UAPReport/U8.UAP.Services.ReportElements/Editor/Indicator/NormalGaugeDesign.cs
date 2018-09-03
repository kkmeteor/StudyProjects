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
                        return "�^�g�_ʼ���С춅^�g�Y��";
                    default:
                        return "���俪ʼ����С���������";
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
                        return "�̶��_ʼ���С춿̶ȽY��";
                    default:
                        return "�̶ȿ�ʼ����С�ڿ̶Ƚ���";
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
                        return "���l�ɫ";
                    default:
                        return "������ɫ";
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
                        return "�̶��ɫ";
                    default:
                        return "�̶���ɫ";
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
                        return "���w�ɫ";
                    default:
                        return "������ɫ";
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
                        return "�̶��_ʼ�";
                    default:
                        return "�̶ȿ�ʼ��";
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
                        return "�̶ȽY���";
                    default:
                        return "�̶Ƚ�����";
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
                        return "�̶�λ��";
                    default:
                        return "�̶�λ��";
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
                        return "�^�g�_ʼ�";
                    default:
                        return "���俪ʼ��";
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
                        return "�^�g�Y���";
                    default:
                        return "���������";
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
                        return "���̶�";
                    default:
                        return "���̶�";
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
                        return "��С�̶�";
                    default:
                        return "��С�̶�";
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
                        return "�P���ɫ";
                    default:
                        return "������ɫ";
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
                        return "���A";
                    default:
                        return "��Բ";
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
                        return "��A";
                    default:
                        return "��Բ";
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
                        return "�_��";
                    default:
                        return "ȷ��";
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
                        return "ȡ��";
                    default:
                        return "ȡ��";
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
                        return "�x�l�OӋ";
                    default:
                        return "�Ǳ����";
                }
            }
        }
        #endregion
    }
}