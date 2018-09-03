using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.Drawing.Drawing2D;
using System.Drawing.Design;
using System.Drawing;
using UFIDA.U8.UAP.Services.ReportResource;

namespace UFIDA.U8.UAP.Services.ReportElements
{
    [Serializable]
    public class Indicator:GridDecimal ,ISerializable,IPart,IIndicator 
    {
        private CompareValue _detailcompare=null;
        private CompareValue _totalcompare = null;
        private CompareValue _summarycompare =null;
        public Indicator()
            : base()
        {
        }
        public Indicator(int x, int y)
            : base(x, y)
        {
        }

        public Indicator(DataSource ds)
            : base(ds)
        {
        }

        public Indicator(Indicator indicator)
            : base(indicator)
        {
            _totalcompare = indicator.TotalCompare==null?null:indicator.TotalCompare.Clone();
            _summarycompare = indicator.SummaryCompare==null?null:indicator.SummaryCompare.Clone();
            _detailcompare = indicator.DetailCompare==null?null:indicator.DetailCompare.Clone();
        }

        public Indicator(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _totalcompare = (CompareValue)info.GetValue("TotalCompare", typeof(CompareValue));
            _summarycompare  = (CompareValue)info.GetValue("SummaryCompare", typeof(CompareValue));
            _detailcompare  = (CompareValue)info.GetValue("DetailCompare", typeof(CompareValue));
        }

        public override void SetType()
        {
            _type = "Indicator";
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("TotalCompare", _totalcompare );
            info.AddValue("SummaryCompare", _summarycompare);
            info.AddValue("DetailCompare", _detailcompare);
        }

        public override object Clone()
        {
            return new Indicator(this);
        }

        public override void SetDefault()
        {
            base.SetDefault();
            _visibleposition = 0;
        }

        protected override void draw(System.Drawing.Graphics g)
        {
            System.Drawing.Image image = null;
            if (_understate == ReportStates.Browse)
                image=AnalysisIndicator(CompareValue);

            base.draw(g);

            if(image!=null)
                DrawCompareImage(g,image);
        }

        #region IPart Members

        private IMetrix  _metrix;
        [Browsable(false)]
        public IMetrix  Metrix
        {
            get
            {
                return _metrix;
            }
            set
            {
                _metrix = value;
            }
        }
        [Browsable(false)]
        public PartType PartType
        {
            get { return PartType.Indicator ; }
        }

        #endregion

        [DisplayText("U8.UAP.Report.详细设计")]
        [LocalizeDescription("U8.UAP.Report.详细设计")]
        public CompareValue CompareValue
        {
            get
            {
                if (_detailcompare != null)
                    return _detailcompare;
                else if (_summarycompare != null)
                    return _summarycompare;
                else 
                    return _totalcompare ;
            }
            set
            {
                ;
            }
        }

        [Browsable(false)]
        public CompareValue DetailCompare
        {
            get
            {
                return _detailcompare;
            }
            set
            {
                _detailcompare = value;
            }
        }

        [Browsable(false)]
        public CompareValue TotalCompare
        {
            get
            {
                return _totalcompare;
            }
            set
            {
                _totalcompare = value;
            }
        }

        [Browsable(false)]
        public CompareValue SummaryCompare
        {
            get
            {
                return _summarycompare;
            }
            set
            {
                _summarycompare = value;
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            if (_summarycompare != null)
                _summarycompare.Dispose();
            if (_totalcompare != null)
                _totalcompare.Dispose();
            if (_detailcompare != null)
                _detailcompare.Dispose();
            _detailcompare = null;
            _summarycompare = null;
            _totalcompare = null;
        }
    }

    public interface IIndicator
    {
        CompareValue DetailCompare { get;set;}
        CompareValue TotalCompare { get;set;}
        CompareValue SummaryCompare { get;set;}
        CompareValue CompareValue { get;set;}
    }

    [TypeConverterAttribute(typeof(CompareValueTypeConverter))]
    [Editor(typeof(CompareValueEditor), typeof(UITypeEditor))]
    [Serializable]
    public class CompareValue : DisplyTextCustomTypeDescriptor,ISerializable, IDisposable
    {
        private string _expression1="";
        private string _expression2="";
        private IndicatorPerformance _performance = IndicatorPerformance.GreaterBetter;
        private IndicatorViewType _viewtype= IndicatorViewType.FontColor ;
        private bool _flagonbadonly = true;
        private string _scriptid;
        
        public CompareValue()
        {
            _scriptid = Guid.NewGuid().ToString().Replace('-','_');
        }

        public CompareValue(CompareValue cv)
        {
            _expression1 = cv.Expression1;
            _expression2 = cv.Expression2;
            _performance = cv.Performance;
            _viewtype = cv.ViewStyle;
            _scriptid = Guid.NewGuid().ToString().Replace('-', '_');
            _flagonbadonly = cv.FlagOnBadOnly;
        }

        protected CompareValue(SerializationInfo info, StreamingContext context)
        {
            _expression1 = info.GetString("Expression1");
            _expression2 = info.GetString("Expression2");
            _performance  = (IndicatorPerformance)info.GetValue("Performance", typeof(IndicatorPerformance));
            _viewtype = (IndicatorViewType)info.GetValue("ViewStyle", typeof(IndicatorViewType));
            _scriptid = info.GetString("ScriptID");
            _flagonbadonly = info.GetBoolean("FlagOnBadOnly");
        }

        public string Expression1Script
        {
            get
            {
                if (bPeriodOnPeriod)
                {
                    StringBuilder sbpp = new StringBuilder();
                    sbpp.Append("int crossindex = cell.CrossIndex;\r\n");
                    sbpp.Append("string name = cell.Name;\r\n");
                    sbpp.Append("if (crossindex <= 0)\r\n");
                    sbpp.Append("{\r\n");
                    sbpp.Append("    return \u0022\u0022;\r\n");
                    sbpp.Append("}\r\n");
                    sbpp.Append("else\r\n");
                    sbpp.Append("{\r\n");
                    sbpp.Append("    string[] names = System.Text.RegularExpressions.Regex.Split(name, \u0022__\u0022);\r\n");
                    sbpp.Append("    string cv=cells[names[0] + \u0022__\u0022 + Convert.ToString(crossindex - 1)].ToString();\r\n");
                    sbpp.Append("    return cv==\u0022\u0022?\u00220\u0022:cv;\r\n");
                    sbpp.Append("}\r\n");
                    return sbpp.ToString();
                }
                else
                {
                    return _expression1;
                }
            }
        }

        public string Expression2Script
        {
            get
            {
                return _expression2;
            }
        }

        public string ScriptID
        {
            get
            {
                return _scriptid;
            }
            set
            {
                _scriptid = value;
            }
        }

        public bool bScript
        {
            get
            {
                return bExpression1Script || bExpression2Script;
            }
        }

        public bool bPeriodOnPeriod
        {
            get
            {
                return _expression1 == "PeriodOnPeriod";
            }
        }

        public bool bExpression1Script
        {
            get
            {
                return bPeriodOnPeriod || _expression1.Trim().EndsWith(";") ;
            }
        }

        public bool bExpression2Script
        {
            get
            {
                return _expression2.Trim().EndsWith(";");
            }
        }

        public string Expression1
        {
            get
            {
                return _expression1;
            }
            set
            {
                _expression1 = value;
            }
        }

        public string Expression2
        {
            get
            {
                return _expression2;
            }
            set
            {
                _expression2 = value;
            }
        }

        public IndicatorViewType ViewStyle
        {
            get
            {
                return _viewtype;
            }
            set
            {
                _viewtype = value;
            }
        }

        public IndicatorPerformance Performance
        {
            get
            {
                return _performance ;
            }
            set
            {
                _performance = value;
            }
        }

        public bool FlagOnBadOnly
        {
            get
            {
                return _flagonbadonly;
            }
            set
            {
                _flagonbadonly = value;
            }
        }

        #region ISerializable Members

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Version",90);//890
            info.AddValue("Expression1", _expression1);
            info.AddValue("Expression2", _expression2);
            info.AddValue("Performance", _performance );
            info.AddValue("ViewStyle", _viewtype);
            info.AddValue("ScriptID", _scriptid);
            info.AddValue("FlagOnBadOnly", _flagonbadonly);
        }

        #endregion

        public CompareValue Clone()
        {
            return new CompareValue(this);
        }

        #region IDisposable Members

        public void Dispose()
        {
            _expression1 = null;
            _expression2 = null;
        }

        #endregion
    }

    public class CompareValueTypeConverter : TypeConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(CompareValue ))
                return true;

            return base.CanConvertTo(context, destinationType);

        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
                return true;

            return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(System.String) &&
                value is CompareValue )
            {
                return "";
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            if (value is string)
            {
                return ((IIndicator )context.Instance).CompareValue ;
            }
            return base.ConvertFrom(context, culture, value);
        }
    }

    public class CompareValueEditor : UITypeEditor
    {
        public CompareValueEditor()
        {
        }

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return System.Drawing.Design.UITypeEditorEditStyle.Modal;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            IWindowsFormsEditorService edSvc = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
            if (edSvc == null)
                return value;
            IndicatorDesign id = new IndicatorDesign();
            IIndicator indicator = context.Instance as IIndicator;
            if (indicator is CalculatorIndicator)
            {
                if( indicator.SummaryCompare == null)
                    indicator.SummaryCompare = new CompareValue();
            }
            else
            {
                IMetrix metrix = (indicator as IPart).Metrix;
                if (metrix != null && (metrix as IIndicatorMetrix).CrossPart != null)
                {
                    if (indicator.TotalCompare == null)
                        indicator.TotalCompare = new CompareValue();
                }
                else
                    indicator.TotalCompare = null;
                if (indicator.DetailCompare == null)
                    indicator.DetailCompare = new CompareValue();
                if (indicator.SummaryCompare == null)
                    indicator.SummaryCompare = new CompareValue();
            }
            id.Init((Cell)indicator,indicator.DetailCompare,indicator.SummaryCompare,indicator.TotalCompare );
            edSvc.ShowDialog(id);
            return value;
        }
    }

    public enum IndicatorViewType
    {
        UpDown,
        SmileCry,
        RGBLight,
        Filter,
        OppositeFilter,
        FontColor,
        BackColor
    }

    public enum IndicatorPerformance
    {
        GreaterBetter,
        LessBetter,
        AmidBetter,
        BothSidesBetter
    }
}
