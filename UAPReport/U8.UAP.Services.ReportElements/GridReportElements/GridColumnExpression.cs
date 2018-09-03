using System;
using System.Runtime.Serialization;
using System.ComponentModel;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// GridColumnExpression 的摘要说明。
	/// </summary>
	[Serializable]
    public class GridColumnExpression : ColumnExpression, IGridEvent, ISerializable, ICloneable, IDisposable, IWithSizable, IMergeStyle
	{
        protected EventType _eventtype = EventType.OnTitle ;
        protected bool _bshowatreal;
        protected bool _bMergeCell = false;
		#region constructor
		public GridColumnExpression():base()
		{
		}

		public GridColumnExpression(int x,int y):base(x,y)
		{
		}

		public GridColumnExpression(int x,int y,int width,int height):base(x,y,width,height)
		{
		}

		public GridColumnExpression(string name,string expression):base()
		{
			_name=name;
			Expression=expression;
		}

		public GridColumnExpression(int x,int y,string name,string expression):base(x,y)
		{
			_name=name;
			Expression=expression;
		}

		public GridColumnExpression(int x,int y,int width,int height,string name,string expression):base(x,y,width,height)
		{
			_name=name;
			Expression=expression;
		}

		public GridColumnExpression(GridColumnExpression gridcolumnexpression):base(gridcolumnexpression)
		{
            _eventtype = gridcolumnexpression.EventType;
            _bshowatreal = gridcolumnexpression.bShowAtReal;
            _bMergeCell = gridcolumnexpression.bMergeCell;
        }

        public GridColumnExpression(Rect groupobject)
            : base(groupobject)
        {
        }

        public GridColumnExpression(IDateTimeDimensionLevel groupobject,string accid)
            : base(groupobject as Rect)
        {
            GridDateTime  ch = groupobject as GridDateTime ;
            _name = ch.Name + "_Dimension";
            _scriptid = ch.ScriptID;
            _sortoption  = ch.SortOption;
            _eventtype = ch.EventType;
            _bshowatreal = ch.bShowAtReal;
            if (string.IsNullOrEmpty(_prepaintevent) || _eventtype == ReportElements.EventType.OnTitle )
                Expression = DateTimeDimensionHelper.GetExpressionAll(groupobject.DDLevel,groupobject.ShowYear ,groupobject.ShowWeekRange, ch.DataSource.Name,accid );
            else
                Expression = DateTimeDimensionHelper.GetExpressionOnly(groupobject.DDLevel,groupobject.ShowYear , ch.DataSource.Name,accid );            
        }

		protected GridColumnExpression( SerializationInfo info, StreamingContext context ):base(info,context)
		{
            _eventtype = (EventType)info.GetValue("EventType", typeof(EventType));
            _bshowatreal = info.GetBoolean("bShowAtReal");
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
			_type="GridColumnExpression";
		}
        public override void SetDefault()
        {
            base.SetDefault();
            _backcolor = DefaultConfigs.DefaultTitleBackColor;
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
            info.AddValue("bShowAtReal", _bshowatreal);
            info.AddValue("bMergeCell", _bMergeCell);
		}

		#endregion

		#region ICloneable 成员

		public override object Clone()
		{
			return new GridColumnExpression(this);
		}

		#endregion

		#region IDisposable 成员

		public override void Dispose()
		{
			// TODO:  添加 GridColumnExpression.Dispose 实现
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
