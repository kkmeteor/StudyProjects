using System;
using System.Drawing;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// GridLabel 的摘要说明。
	/// </summary>
	[Serializable]
    public class GridLabel : Label, IDataSource, IMapName, ISort, IGridEvent, ISerializable, ICloneable, IDisposable, IWithSizable,IMergeStyle
	{
		#region fields
		protected DataSource _datasource = new DataSource("EmptyColumn");
		protected SortOption _sortoption;
        protected EventType _eventtype = EventType.OnTitle ;
        protected bool _bshowatreal;
        protected bool _bMergeCell = false;
		#endregion

		#region constructor
		public GridLabel():base()
		{
		}

		public GridLabel(int x,int y):base(x,y)
		{
		}

		public GridLabel(int x,int y,int width,int height):base(x,y,width,height)
		{
		}

		public GridLabel(string caption):base()
		{
			_caption=caption;
		}
		public GridLabel(int x,int y,string caption):base(x,y)
		{
			_caption=caption;
		}

		public GridLabel(int x,int y,int width,int height,string caption):base(x,y,width,height)
		{
			_caption=caption;
		}

		public GridLabel(GridLabel gridlabel):base(gridlabel)
		{
			_datasource=gridlabel.DataSource;
			_sortoption =gridlabel.SortOption ;
            _eventtype = gridlabel.EventType;
            _bshowatreal = gridlabel.bShowAtReal;
            _bMergeCell = gridlabel.bMergeCell;
		}

        public GridLabel(DataSource ds)
            : base()
        {
            _datasource = ds;
            _name = ds.Name;
            _cncaption = ds.CNCaption;
            _twcaption = ds.TWCaption;
            _encaption = ds.ENCaption;
            _caption = ds.Caption;
        }

		public GridLabel(IDataSource ds)
            : base(ds as Rect )
        {
            _datasource = ds.DataSource;
        }

        public GridLabel(IMapName map,DataSource ds)
            : base(map as Rect)
        {
            _datasource = ds;
            _eventtype = (map as IGridEvent).EventType;
            _bshowatreal = (map as IGridEvent).bShowAtReal;

            _sortoption  = (map as ISort).SortOption ;
        }

		protected GridLabel( SerializationInfo info, StreamingContext context ):base(info,context)
		{
			_datasource=(DataSource)info.GetValue("DataSource",typeof(DataSource));			
            _eventtype = (EventType)info.GetValue("EventType", typeof(EventType));
            _bshowatreal = info.GetBoolean("bShowAtReal");
            _sortoption = new SortOption();
            _sortoption.SortDirection =(SortDirection)info.GetValue("Direction",typeof(SortDirection));
            if (_version > 1)
                _sortoption.Priority = info.GetInt32("Priority");
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
			_type="GridLabel";
		}

		public override void SetDefault()
		{
            base.SetDefault();
			//_backcolor=Color.White;
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

        [Browsable(false)]
        public override LabelType LabelType
        {
            get
            {
                return base.LabelType;
            }
            set
            {
                base.LabelType = value;
            }
        }
		#endregion

		#region IDataSource 成员

		[DisplayText("U8.UAP.Services.ReportElements.Dis16")]
		[LocalizeDescription("U8.UAP.Services.ReportElements.Dis16")]
		public virtual  DataSource DataSource
		{
			get
			{
				return _datasource;
			}
			set
			{
				_datasource=value;
                if (_caption == "" && value != null)
                {
                    _caption = _datasource.Caption;
                    OnOtherChanged(null);
                }
			}
		}

		#endregion

		#region ISerializable 成员

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info,context);
			info.AddValue("DataSource",_datasource);
            info.AddValue("Direction", SortOption.SortDirection);
            info.AddValue("Priority", SortOption.Priority);
            info.AddValue("EventType", _eventtype);
            info.AddValue("bShowAtReal", _bshowatreal);
            info.AddValue("bMergeCell", _bMergeCell);
		}

		#endregion

		#region ICloneable 成员

		public override object Clone()
		{
			return new GridLabel(this);
		}

		#endregion

		#region IDisposable 成员

		public override void Dispose()
		{
            _datasource = null;
            _sortoption = null;
            base.Dispose();
		}

		#endregion

		#region ISort 成员

        [Browsable(true)]
        [DisplayText("U8.Report.SortOption")]
        [LocalizeDescription("U8.Report.SortOption")]
        public SortOption SortOption
        {
            get
            {
                if (_sortoption == null)
                    _sortoption = new SortOption();
                return _sortoption;
            }
            set
            {
                _sortoption = value;
            }
        }

		#endregion

		#region IMapName 成员

		[Browsable(false)]
		public string MapName
		{
			get
			{
				return _datasource.Name ;
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
