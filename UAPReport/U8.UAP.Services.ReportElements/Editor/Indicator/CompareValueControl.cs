using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace UFIDA.U8.UAP.Services.ReportElements
{
    public partial class CompareValueControl : UserControl
    {
        private CompareValue _cv;
        private Cell _cell;
        public CompareValueControl()
        {
            InitializeComponent();
            this.group1.Text = Group1;
            this.group2.Text = Group2;
            this.rbpop.Text = POP;
            this.rbstatic1.Text = Static;
            this.rbstatic2.Text = Static;
            this.rbalgorithm1.Text = Algorithm;
            this.rbalgorithm2.Text = Algorithm;
            this.valueoption.Text = ValueOptionText ;
            this.greaterbetter.Text = GreaterBetter ;
            this.lessbetter.Text = LessBetter ;
            this.amidbetter.Text = AmidBetter ;
            this.bothsidesbetter.Text = BothSidesBetter;
            this.showoption.Text = ShowOptionText ;
            this.fontcolor.Text = FontColor;
            this.backcolor.Text = BackColorText;
            //this.filter.Text = Filter;
            //this.oppositefilter.Text = OppositeFilter;
            this.flagonbadonly.Text = FlagOnBadOnlyText;
            this.btndesign1.Text = DesignText;
            this.btndesign2.Text = DesignText;
        }

        public void Init(Cell cell,CompareValue cv)
        {
            _cv = cv;
            _cell = cell;
            if (cv == null)
                return;
            if (_cv.bPeriodOnPeriod )
            {
                rbpop.Checked = true;
                this.expression1.Text = "";
            }
            else
                expression1.Text = _cv.Expression1;
            expression2.Text = _cv.Expression2;

            rbstatic1.Checked = !expression1.Text.Trim().EndsWith(";");
            rbalgorithm1.Checked = expression1.Text.Trim().EndsWith(";");
            rbstatic2.Checked = !expression2.Text.Trim().EndsWith(";");
            rbalgorithm2.Checked = expression2.Text.Trim().EndsWith(";");

            #region value option
            greaterbetter.Checked = false;
            lessbetter.Checked = false;
            amidbetter.Checked = false;
            bothsidesbetter.Checked = false;
            switch (_cv.Performance)
            {
                case IndicatorPerformance.GreaterBetter:
                    greaterbetter.Checked = true;
                    break;
                case IndicatorPerformance.LessBetter:
                    lessbetter.Checked = true;
                    break;
                case IndicatorPerformance.AmidBetter:
                    amidbetter.Checked = true;
                    break;
                case IndicatorPerformance.BothSidesBetter:
                    bothsidesbetter.Checked = true;
                    break;
            }
            #endregion

            #region show option
            backcolor.Checked = false;
            fontcolor.Checked = false;
            rgblight.Checked = false;
            smilecry.Checked = false;
            updown.Checked = false;
            switch (_cv.ViewStyle)
            {
                case IndicatorViewType.BackColor :
                    backcolor.Checked = true;
                    break;
                case IndicatorViewType.FontColor :
                    fontcolor.Checked = true;
                    break;
                case IndicatorViewType.RGBLight:
                    rgblight.Checked = true;
                    break;
                case IndicatorViewType.SmileCry:
                    smilecry.Checked = true;
                    break;
                case IndicatorViewType.UpDown:
                    updown.Checked = true;
                    break;
            }
            flagonbadonly.Checked = cv.FlagOnBadOnly;
            #endregion

        }

        public void EndEdit()
        {
            if (_cv == null)
                return ;

            if (expression1.Text.Trim()!="" && !expression1.Text.Trim().EndsWith(";"))
            {
                try
                {
                    Convert.ToDouble(expression1.Text.Trim());
                }
                catch (Exception e)
                {
                    throw new Exception(InvalidStatic);
                }
            }
            if (expression2.Text.Trim() != "" && !expression2.Text.Trim().EndsWith(";"))
            {
                try
                {
                    Convert.ToDouble(expression2.Text.Trim());
                }
                catch (Exception e)
                {
                    throw new Exception(InvalidStatic);
                }
            }
            _cv.Expression1 = expression1.Text;
            _cv.Expression2 = expression2.Text;

            if (rbpop.Checked)
            {
                _cv.Expression1 = "PeriodOnPeriod";
            }

            if (greaterbetter.Checked)
                _cv.Performance = IndicatorPerformance.GreaterBetter;
            else if (lessbetter.Checked)
                _cv.Performance = IndicatorPerformance.LessBetter;
            else if (amidbetter.Checked)
                _cv.Performance = IndicatorPerformance.AmidBetter;
            else
                _cv.Performance = IndicatorPerformance.BothSidesBetter;

            if (rgblight.Checked)
                _cv.ViewStyle = IndicatorViewType.RGBLight;
            else if (updown.Checked)
                _cv.ViewStyle = IndicatorViewType.UpDown;
            else if (smilecry.Checked)
                _cv.ViewStyle = IndicatorViewType.SmileCry;
            else if (fontcolor.Checked)
                _cv.ViewStyle = IndicatorViewType.FontColor;
            else
                _cv.ViewStyle = IndicatorViewType.BackColor;

            _cv.FlagOnBadOnly = flagonbadonly.Checked;
        }

        public void NotShowFilter()
        {
            //filter.Enabled  = false;
            //oppositefilter.Enabled  = false;
        }

        public void NoPop()
        {
            rbpop.Enabled = false;
        }

        private void rbpop_CheckedChanged(object sender, EventArgs e)
        {
            if (rbpop.Checked)
            {
                UnableExpression1();
                UnableExpression2();
                updown.Enabled = true;
                updown.Checked = true;
                smilecry.Enabled = false;
                rgblight.Enabled = false;
                backcolor.Enabled = false;
                fontcolor.Enabled = false;
            }
            else
            {
                EnableExpression1();
                //EnableExpression2();
                updown.Enabled = true;
                smilecry.Enabled = true;
                rgblight.Enabled = true;
                backcolor.Enabled = true;
                fontcolor.Enabled = true;
            }
        }

        #region resource
        public string InvalidStatic
        {
            get
            {
                switch (System.Threading.Thread.CurrentThread.CurrentUICulture.Name)
                {
                    case "en-US":
                        return "Invalid static value";
                    case "zh-TW":
                        return "���^ֵһ";
                    default:
                        return "�Ƿ��̶�ֵ";
                }
            }
        }
        public string Group1
        {
            get
            {
                switch (System.Threading.Thread.CurrentThread.CurrentUICulture.Name)
                {
                    case "en-US":
                        return "Compare value 1";
                    case "zh-TW":
                        return "���^ֵһ";
                    default:
                        return "�Ƚ�ֵһ";
                }
            }
        }
        public string Group2
        {
            get
            {
                switch (System.Threading.Thread.CurrentThread.CurrentUICulture.Name)
                {
                    case "en-US":
                        return "Compare value 2";
                    case "zh-TW":
                        return "���^ֵ��";
                    default:
                        return "�Ƚ�ֵ��";
                }
            }
        }
        public string POP
        {
            get
            {
                switch (System.Threading.Thread.CurrentThread.CurrentUICulture.Name)
                {
                    case "en-US":
                        return "Period on Period";
                    case "zh-TW":
                        return "�h��";
                    default:
                        return "����";
                }
            }
        }
        public string Static
        {
            get
            {
                switch (System.Threading.Thread.CurrentThread.CurrentUICulture.Name)
                {
                    case "en-US":
                        return "Static";
                    case "zh-TW":
                        return "�̶�ֵ";
                    default:
                        return "�̶�ֵ";
                }
            }
        }
       
        public string Algorithm
        {
            get
            {
                switch (System.Threading.Thread.CurrentThread.CurrentUICulture.Name)
                {
                    case "en-US":
                        return "Script";
                    case "zh-TW":
                        return "�_��";
                    default:
                        return "�ű�";
                }
            }
        }
        public string DesignText
        {
            get
            {
                switch (System.Threading.Thread.CurrentThread.CurrentUICulture.Name)
                {
                    case "en-US":
                        return "Design";
                    case "zh-TW":
                        return "�OӋ";
                    default:
                        return "���";
                }
            }
        }
        public string ValueOptionText
        {
            get
            {
                switch (System.Threading.Thread.CurrentThread.CurrentUICulture.Name)
                {
                    case "en-US":
                        return "Value Option";
                    case "zh-TW":
                        return "ֵ�x�";
                    default:
                        return "ֵѡ��";
                }
            }
        }
        public string ShowOptionText
        {
            get
            {
                switch (System.Threading.Thread.CurrentThread.CurrentUICulture.Name)
                {
                    case "en-US":
                        return "Show Option";
                    case "zh-TW":
                        return "�@ʾ�x�";
                    default:
                        return "��ʾѡ��";
                }
            }
        }
        public string GreaterBetter
        {
            get
            {
                switch (System.Threading.Thread.CurrentThread.CurrentUICulture.Name)
                {
                    case "en-US":
                        return "The greater the better";
                    case "zh-TW":
                        return "ֵԽ��Խ��";
                    default:
                        return "ֵԽ��Խ��";
                }
            }
        }
        public string LessBetter
        {
            get
            {
                switch (System.Threading.Thread.CurrentThread.CurrentUICulture.Name)
                {
                    case "en-US":
                        return "The less the better";
                    case "zh-TW":
                        return "ֵԽСԽ��";
                    default:
                        return "ֵԽСԽ��";
                }
            }
        }
        public string AmidBetter
        {
            get
            {
                switch (System.Threading.Thread.CurrentThread.CurrentUICulture.Name)
                {
                    case "en-US":
                        return "Amid the better";
                    case "zh-TW":
                        return "ֵ�����g��";
                    default:
                        return "ֵ���м��";
                }
            }
        }
        public string BothSidesBetter
        {
            get
            {
                switch (System.Threading.Thread.CurrentThread.CurrentUICulture.Name)
                {
                    case "en-US":
                        return "Both sides the better";
                    case "zh-TW":
                        return "ֵ�ڃ��^��";
                    default:
                        return "ֵ����ͷ��";
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
        public string BackColorText
        {
            get
            {
                switch (System.Threading.Thread.CurrentThread.CurrentUICulture.Name)
                {
                    case "en-US":
                        return "Back Color";
                    case "zh-TW":
                        return "����ɫ";
                    default:
                        return "����ɫ";
                }
            }
        }
        public string Filter
        {
            get
            {
                switch (System.Threading.Thread.CurrentThread.CurrentUICulture.Name)
                {
                    case "en-US":
                        return "Filter";
                    case "zh-TW":
                        return "�^�V";
                    default:
                        return "����";
                }
            }
        }
        public string OppositeFilter
        {
            get
            {
                switch (System.Threading.Thread.CurrentThread.CurrentUICulture.Name)
                {
                    case "en-US":
                        return "Opposite filter";
                    case "zh-TW":
                        return "�����^�V";
                    default:
                        return "�������";
                }
            }
        }

        public string FlagOnBadOnlyText
        {
            get
            {
                switch (System.Threading.Thread.CurrentThread.CurrentUICulture.Name)
                {
                    case "en-US":
                        return "Flag on bad / down only";
                    case "zh-TW":
                        return "ֻ���R���õ�/�½���ֵ";
                    default:
                        return "ֻ��ʶ���õ�/�½���ֵ";
                }
            }
        }
        #endregion

        private void expression1_TextChanged(object sender, EventArgs e)
        {

        }

        private void btndesign1_Click(object sender, EventArgs e)
        {
            Design(expression1);
        }
        private void btndesign2_Click(object sender, EventArgs e)
        {
            Design(expression2);
        }
        private void Design(TextBox tb)
        {
            ExpressionEditor ee = new ExpressionEditor();
            ExpressionEditorForm f = null;
            //if (_cell is ICalculateColumn)
            //    f = ee.GetExpressionEditorForm(tb.Text, _cell, "Expression");
            //else
            f = ee.GetExpressionEditorForm(tb.Text, _cell, "Algorithm");
            if (f != null && f.ShowDialog() == DialogResult.OK)
            {
                tb.Text = f.Source;
            }
        }

        private void rbalgorithm1_CheckedChanged(object sender, EventArgs e)
        {
            if (rbalgorithm1.Checked)
                btndesign1.Enabled = true;
            else
                btndesign1.Enabled = false;
        }

        private void rbalgorithm2_CheckedChanged(object sender, EventArgs e)
        {
            if (rbalgorithm2.Checked)
                btndesign2.Enabled = true;
            else
                btndesign2.Enabled = false;
        }

        private void updown_CheckedChanged(object sender, EventArgs e)
        {
            if (updown.Checked)
            {
                UnableValueOptions();
                UnableExpression2();
            }
        }

        private void smilecry_CheckedChanged(object sender, EventArgs e)
        {
            if (smilecry.Checked)
            {
                EnableValueOptions();
                UnableExpression2();
            }
        }

        private void rgblight_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            if (rb.Checked)
            {
                EnableValueOptions();
                EnableExpression2();
            }
        }

        private void UnableExpression1()
        {
            expression1.Tag = expression1.Text;
            expression1.Text = "";
            expression1.Enabled = false;

            rbstatic1.Tag = rbstatic1.Checked;
            rbalgorithm1.Tag = rbalgorithm1.Checked;
            rbstatic1.Checked = true;
            rbstatic1.Enabled = false;
            rbalgorithm1.Enabled = false;
        }
        private void UnableExpression2()
        {
            expression2.Tag = expression2.Text;
            expression2.Text = "";
            expression2.Enabled = false;

            rbstatic2.Tag = rbstatic2.Checked;
            rbalgorithm2.Tag = rbalgorithm2.Checked;
            rbstatic2.Checked = true;
            rbstatic2.Enabled = false;
            rbalgorithm2.Enabled = false;
        }
        private void UnableValueOptions()
        {
            this.greaterbetter.Enabled = false;
            this.lessbetter.Enabled = false;
            this.amidbetter.Enabled = false;
            this.bothsidesbetter.Enabled = false;
        }

        private void EnableExpression1()
        {
            rbstatic1.Enabled = true;
            rbalgorithm1.Enabled = true;
            expression1.Enabled = true;
            if(rbstatic1.Tag!=null)
                rbstatic1.Checked = (bool)rbstatic1.Tag;
            if (rbalgorithm1.Tag != null)
                rbalgorithm1.Checked = (bool)rbalgorithm1.Tag;
            if(expression1.Tag!=null)
                expression1.Text = expression1.Tag.ToString();
        }
        private void EnableExpression2()
        {
            rbstatic2.Enabled = true;
            rbalgorithm2.Enabled = true;
            expression2.Enabled = true;
            if (rbstatic2.Tag != null)
                rbstatic2.Checked = (bool)rbstatic2.Tag;
            if (rbalgorithm2.Tag != null)
                rbalgorithm2.Checked = (bool)rbalgorithm2.Tag;
            if (expression2.Tag != null)
                expression2.Text = expression2.Tag.ToString();
        }
        private void EnableValueOptions()
        {
            this.greaterbetter.Enabled = true;
            this.lessbetter.Enabled = true;
            this.amidbetter.Enabled = true;
            this.bothsidesbetter.Enabled = true;
        }

        
    }
}
