using System;
using System.Runtime.Serialization;
using System.ComponentModel;

namespace UFIDA.U8.UAP.Services.ReportElements
{
    /// <summary>
    /// GridCalculateColumn 的摘要说明。
    /// </summary>
    [Serializable]
    public class GridCalculateColumn : CalculateColumn, IGridCollect, IGridEvent, ISerializable, ICloneable, IDisposable, IWithSizable, IMergeStyle
    {
        #region fields
        protected bool _bsummary=true;
        protected bool _bclue;
        protected OperatorType _operator = OperatorType.SUM;
        protected DataSource _unit = new DataSource("EmptyColumn");
        protected EventType _eventtype = EventType.OnTitle;
        protected bool _bcolumnsummary = true;
        protected bool _bshowatreal;
        protected bool _bcalcaftercross = false;
        protected bool _bMergeCell = false;
        #endregion

        #region calculator

        public GridCalculateColumn()
            : base()
        {
        }

        public GridCalculateColumn(int x, int y)
            : base(x, y)
        {
        }

        public GridCalculateColumn(int x, int y, int width, int height)
            : base(x, y, width, height)
        {
        }

        public GridCalculateColumn(string name, string expression)
            : base()
        {
            _name = name;
            Expression = expression;
        }

        public GridCalculateColumn(string name, string expression, int x, int y)
            : base(x, y)
        {
            _name = name;
            Expression = expression;
        }

        public GridCalculateColumn(string name, string expression, int x, int y, int width, int height)
            : base(x, y, width, height)
        {
            _name = name;
            Expression = expression;
        }

        public GridCalculateColumn(GridDecimal griddecimal)
            : base(griddecimal)
        {
            _bsummary = griddecimal.bSummary;
            _bcolumnsummary = griddecimal.bColumnSummary;
            _bclue = griddecimal.bClue;
            _operator = griddecimal.Operator;
            _unit = griddecimal.Unit;
            _eventtype = griddecimal.EventType;
            _bshowatreal = griddecimal.bShowAtReal;
            _bcalcaftercross = griddecimal.bCalcAfterCross;
            _expression = griddecimal.DataSource.Name;
        }

        public GridCalculateColumn(GridLabel gridlalel)
            : base(gridlalel)
        {
            _expression = gridlalel.DataSource.Name;
            _bMergeCell = gridlalel.bMergeCell;
        }

        public GridCalculateColumn(GridCalculateColumn gridcalculatecolumn)
            : base(gridcalculatecolumn)
        {
            _bsummary = gridcalculatecolumn.bSummary;
            _bcolumnsummary = gridcalculatecolumn.bColumnSummary;
            _bclue = gridcalculatecolumn.bClue;
            _operator = gridcalculatecolumn.Operator;
            _unit = gridcalculatecolumn.Unit;
            _eventtype = gridcalculatecolumn.EventType;
            _bshowatreal = gridcalculatecolumn.bShowAtReal;
            _bcalcaftercross = gridcalculatecolumn.bCalcAfterCross;
        }

        protected GridCalculateColumn(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _bsummary = info.GetBoolean("bSummary");
            _bclue = info.GetBoolean("bClue");
            _operator = (OperatorType)info.GetValue("Operator", typeof(OperatorType));
            _unit = (DataSource)info.GetValue("Unit", typeof(DataSource));
            _eventtype = (EventType)info.GetValue("EventType", typeof(EventType));
            _bcolumnsummary = info.GetBoolean("bColumnSummary");
            _bshowatreal = info.GetBoolean("bShowAtReal");
            _bcalcaftercross = info.GetBoolean("bCalcAfterCross");
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
            _type = "GridCalculateColumn";
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
                return _bcalcaftercross;
            }
            set
            {
                _bcalcaftercross = value;
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
            info.AddValue("EventType", _eventtype);
            info.AddValue("bColumnSummary", _bcolumnsummary);
            info.AddValue("bShowAtReal", _bshowatreal);
            info.AddValue("bCalcAfterCross", _bcalcaftercross);
            info.AddValue("bMergeCell", _bMergeCell);
        }

        #endregion

        #region ICloneable 成员

        public override object Clone()
        {
            return new GridCalculateColumn(this);
        }

        #endregion

        #region IDisposable 成员

        public override void Dispose()
        {
            base.Dispose();
            _unit = null;
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
