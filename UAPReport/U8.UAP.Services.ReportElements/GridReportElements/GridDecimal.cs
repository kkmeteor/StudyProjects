using System;
using System.Runtime.Serialization;
using System.ComponentModel;

namespace UFIDA.U8.UAP.Services.ReportElements
{
    /// <summary>
    /// GridDecimal 的摘要说明。
    /// </summary>
    [Serializable]
    public class GridDecimal : DBDecimal, IGridCollect, IGridEvent, ISerializable, ICloneable, IDisposable, IUserDefine, IWithSizable, IMergeStyle
    {
        #region fields
        protected bool _bsummary = true;
        protected bool _bMergeCell = false;
        protected bool _bcolumnsummary = true;
        protected bool _bclue;
        protected OperatorType _operator = OperatorType.SUM;
        protected DataSource _unit = new DataSource("EmptyColumn");
        protected int _calculateindex;
        protected EventType _eventtype = EventType.OnTitle;
        protected bool _bshowatreal = false;
        protected bool _bcalcaftercross = false;
        protected string _userdefineitem = "";
        #endregion

        #region Constructor
        public GridDecimal()
            : base()
        {
        }

        public GridDecimal(int x, int y)
            : base(x, y)
        {
        }

        public GridDecimal(int x, int y, int width, int height)
            : base(x, y, width, height)
        {
        }

        public GridDecimal(GridDecimal griddecimal)
            : base(griddecimal)
        {
            _bsummary = griddecimal.bSummary;
            _bcolumnsummary = griddecimal.bColumnSummary;
            _bclue = griddecimal.bClue;
            _operator = griddecimal.Operator;
            _unit = griddecimal.Unit;
            _calculateindex = griddecimal.CalculateIndex;
            _eventtype = griddecimal.EventType;
            _bshowatreal = griddecimal.bShowAtReal;
            _bcalcaftercross = griddecimal.bCalcAfterCross;
            _bMergeCell = griddecimal.bMergeCell;
            _userdefineitem = griddecimal.UserDefineItem;
        }

        public GridDecimal(DataSource ds)
            : base(ds)
        {
        }

        public GridDecimal(IDataSource ds)
            : base(ds as Rect)
        {
            _datasource = ds.DataSource;
        }

        public GridDecimal(IMapName map,DataSource ds)
            : base(map as Rect)
        {
            _datasource = ds;
            _bcalcaftercross = false;
            _bsummary = (map as IGridCollect).bSummary;
            _bclue = (map as IGridCollect).bClue;
            _operator = (map as IGridCollect).Operator;
            _unit = (map as IGridCollect).Unit;
            _calculateindex = (map as IGridCollect).CalculateIndex;
            _eventtype = (map as IGridEvent).EventType;
            _bshowatreal = (map as IGridEvent).bShowAtReal;

            _precision = (map as IDecimal).Precision;
            _formatstring = (map as IDecimal).FormatString;
            _sortoption = (map as ISort).SortOption;
            _bshowwhenzero = (map as IDecimal).bShowWhenZero;
            _pointlength = (map as IDecimal).PointLength;
        }

        protected GridDecimal(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _bsummary = info.GetBoolean("bSummary");
            _bclue = info.GetBoolean("bClue");
            _operator = (OperatorType)info.GetValue("Operator", typeof(OperatorType));
            _unit = (DataSource)info.GetValue("Unit", typeof(DataSource));
            _calculateindex = info.GetInt32("CalculateIndex");
            _eventtype = (EventType)info.GetValue("EventType", typeof(EventType));
            _bcolumnsummary = info.GetBoolean("bColumnSummary");
            _bshowatreal = info.GetBoolean("bShowAtReal");
            _bcalcaftercross = info.GetBoolean("bCalcAfterCross");
            if (_version > 1)
                _userdefineitem = info.GetString("UserDefineItem");
            try
            {
                _bMergeCell = info.GetBoolean("bMergeCell");
            }
            catch
            {
                _bMergeCell = false;
            }
        }
        #endregion

        #region override
        public override void SetType()
        {
            _type = "GridDecimal";
        }

        public override void SetDefault()
        {
            base.SetDefault();
            _backcolor = DefaultConfigs.DefaultTitleBackColor;
            _captionalign = System.Drawing.ContentAlignment.MiddleRight;
            _visibleposition = 0;
        }

        [Browsable(false)]
        public override System.Drawing.Color BackColor
        {
            get
            {
                return base.BackColor;
            }
            set
            {
                base.BackColor = value;
            }
        }

        [Browsable(false)]
        public override System.Drawing.Color BorderColor
        {
            get
            {
                return base.BorderColor;
            }
            set
            {
                base.BorderColor = value;
            }
        }

        [Browsable(false)]
        public override System.Drawing.Color ForeColor
        {
            get
            {
                return base.ForeColor;
            }
            set
            {
                base.ForeColor = value;
            }
        }

        [Browsable(false)]
        public override System.Drawing.Font ClientFont
        {
            get
            {
                return base.ClientFont;
            }
            set
            {
                base.ClientFont = value;
            }
        }

        public override void SetSolid()
        {

        }

        [Browsable(false)]
        public override System.Drawing.ContentAlignment CaptionAlign
        {
            get
            {
                return base.CaptionAlign;
            }
            set
            {
                base.CaptionAlign = value;
            }
        }

        #endregion

        #region IGridCollect 成员

        [DisplayText("U8.UAP.Services.ReportElements.Dis27")]
        [LocalizeDescription("U8.UAP.Services.ReportElements.Dis27")]
        public bool bSummary
        {
            get
            {
                return _bsummary;
            }
            set
            {
                _bsummary = value;
            }
        }

        [DisplayText("U8.UAP.Services.ReportElements.GridCalculateColumn.是否横向汇总")]
        [LocalizeDescription("U8.UAP.Services.ReportElements.GridCalculateColumn.是否横向汇总")]
        public bool bColumnSummary
        {
            get
            {
                return _bcolumnsummary;
            }
            set
            {
                _bcolumnsummary = value;
            }
        }


        [DisplayText("U8.UAP.Services.ReportElements.GridCalculateColumn.是否行线索")]
        [LocalizeDescription("U8.UAP.Services.ReportElements.GridCalculateColumn.是否行线索")]
        public bool bClue
        {
            get
            {
                return _bclue;
            }
            set
            {
                _bclue = value;
            }
        }

        [Browsable(false)]
        [DisplayText("交叉后计算")]
        [LocalizeDescription("交叉后计算")]
        public bool bCalcAfterCross
        {
            get
            {
                return false;
            }
            set
            {
                //_bcalcaftercross = value;
            }
        }

        #endregion

        #region ICalculator 成员

        [DisplayText("U8.UAP.Services.ReportElements.Dis23")]
        [LocalizeDescription("U8.UAP.Services.ReportElements.Dis23")]
        [TypeConverter(typeof(OperatorTypeConvertor))]
        public UFIDA.U8.UAP.Services.ReportElements.OperatorType Operator
        {
            get
            {
                return _operator;
            }
            set
            {
                _operator = value;
            }
        }

        [Browsable(false)]
        [DisplayText("U8.UAP.Services.ReportElements.Dis24")]
        [LocalizeDescription("U8.UAP.Services.ReportElements.Dis24")]
        public DataSource Unit
        {
            get
            {
                return _unit;
            }
            set
            {
                _unit = value;
            }
        }

        #endregion

        #region ISerializable 成员

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("bSummary", _bsummary);
            info.AddValue("bClue", _bclue);
            info.AddValue("Operator", _operator);
            info.AddValue("Unit", _unit);
            info.AddValue("CalculateIndex", _calculateindex);
            info.AddValue("EventType", _eventtype);
            info.AddValue("bColumnSummary", _bcolumnsummary);
            info.AddValue("bShowAtReal", _bshowatreal);
            info.AddValue("bCalcAfterCross", _bcalcaftercross);
            info.AddValue("UserDefineItem", _userdefineitem);
            info.AddValue("bMergeCell", _bMergeCell);
        }

        #endregion

        #region ICloneable 成员

        public override object Clone()
        {
            return new GridDecimal(this);
        }

        #endregion

        #region IDisposable 成员

        public override void Dispose()
        {
            base.Dispose();
        }

        #endregion

        #region ICalculateSequence 成员

        [DisplayText("U8.UAP.Services.ReportElements.Dis18")]
        [LocalizeDescription("U8.UAP.Services.ReportElements.Dis18")]
        public int CalculateIndex
        {
            get
            {
                return _calculateindex;
            }
            set
            {
                _calculateindex = value;
            }
        }

        #endregion

        #region IGridEvent 成员
        [DisplayText("U8.UAP.Services.ReportElements.GridCalculateColumn.输出事件作用域")]
        [LocalizeDescription("U8.UAP.Services.ReportElements.GridCalculateColumn.输出事件作用域")]
        public EventType EventType
        {
            get
            {
                return _eventtype;
            }
            set
            {
                _eventtype = value;
            }
        }

        [DisplayText("U8.UAP.Services.ReportElements.GridCalculateColumn.显示在交叉列后面")]
        [LocalizeDescription("U8.UAP.Services.ReportElements.GridCalculateColumn.显示在交叉列后面")]
        public bool bShowAtReal
        {
            get
            {
                return _bshowatreal;
            }
            set
            {
                _bshowatreal = value;
            }
        }
        #endregion

        #region IUserDefine 成员
        [DisplayText("U8.Report.UserDefineItem")]
        [LocalizeDescription("U8.Report.UserDefineItem")]
        [Editor(typeof(ExpressionEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public string UserDefineItem
        {
            get
            {
                return _userdefineitem;
            }
            set
            {
                _userdefineitem = value;
            }
        }

        #endregion

        #region IMergeSyle 成员

        [Browsable(true)]
        [DisplayText("U8.UAP.Services.Report.MergeCell")]
        [LocalizeDescription("U8.UAP.Services.Report.MergeCell")]
        public virtual bool bMergeCell
        {
            get
            {
                return _bMergeCell;
            }
            set
            {
                _bMergeCell = value;
            }
        }

        #endregion
    }
}
