using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// GridAlgorithmCalculator 的摘要说明。
	/// </summary>
	[Serializable]
    public class GridDecimalAlgorithmColumn : DecimalAlgorithmColumn, IGridEvent, IGridCollect, ISerializable, ICloneable, IDisposable, IWithSizable, IMergeStyle
	{
        protected EventType _eventtype = EventType.OnTitle ;
        protected bool _bshowatreal;
        protected bool _bsummary;
        protected bool _bMergeCell = false;
        protected bool _bcolumnsummary = true;
        protected bool _bclue;
        protected OperatorType _operator = OperatorType.SUM;
        protected DataSource _unit = new DataSource("EmptyColumn");
        protected bool _bcalcaftercross = false;

        #region constructor
		public GridDecimalAlgorithmColumn():base()
		{
		}

		public GridDecimalAlgorithmColumn(int x,int y):base(x,y)
		{
		}

		public GridDecimalAlgorithmColumn(int x,int y,int w,int h):base(x,y,w,h)
		{
		}

		public GridDecimalAlgorithmColumn(GridDecimalAlgorithmColumn gridalgorithmcalculator):base(gridalgorithmcalculator)
		{
            _eventtype = gridalgorithmcalculator.EventType;
            _bsummary = gridalgorithmcalculator.bSummary;
            _bcolumnsummary = gridalgorithmcalculator.bColumnSummary;
            _bclue = gridalgorithmcalculator.bClue;
            _operator = gridalgorithmcalculator.Operator;
            _unit = gridalgorithmcalculator.Unit;
            _bshowatreal = gridalgorithmcalculator.bShowAtReal;
            _bcalcaftercross = gridalgorithmcalculator.bCalcAfterCross;
            _bMergeCell = gridalgorithmcalculator.bMergeCell;
		}

		public GridDecimalAlgorithmColumn(SerializationInfo info,StreamingContext context):base(info,context)
		{
            _eventtype = (EventType)info.GetValue("EventType", typeof(EventType));
            _bsummary = info.GetBoolean("bSummary");
            _bcolumnsummary = info.GetBoolean("bColumnSummary");
            _bclue = info.GetBoolean("bClue");
            _operator = (OperatorType)info.GetValue("Operator", typeof(OperatorType));
            _unit = (DataSource)info.GetValue("Unit", typeof(DataSource));
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
			_type="GridDecimalAlgorithmColumn";
		}

        public override void SetDefault()
        {
            base.SetDefault();
            _bsummary = false;
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

		#region ISerializable 成员

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info,context);
            info.AddValue("EventType", _eventtype);
            info.AddValue("bSummary", _bsummary);
            info.AddValue("bClue", _bclue);
            info.AddValue("Operator", _operator);
            info.AddValue("Unit", _unit);
            info.AddValue("bColumnSummary", _bcolumnsummary);
            info.AddValue("bShowAtReal", _bshowatreal);
            info.AddValue("bCalcAfterCross", _bcalcaftercross);
            info.AddValue("bMergeCell", _bMergeCell);
		}

		#endregion

		#region ICloneable 成员

		public override object Clone()
		{
			return new GridDecimalAlgorithmColumn(this);
		}

		#endregion

		#region IDisposable 成员

		public override void Dispose()
		{
            base.Dispose();
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

        #region IGridCollect 成员
        [Browsable(false)]
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

        [Browsable(false)]
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

        [Browsable(false)]
        [DisplayText("U8.UAP.Services.ReportElements.Dis23")]
        [LocalizeDescription("U8.UAP.Services.ReportElements.Dis23")]
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
