using System;
using System.Runtime.Serialization;
using System.ComponentModel;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// GridDateTime ��ժҪ˵����
	/// </summary>
	[Serializable]
    public class GridDateTime : DBDateTime, IGridEvent, ISerializable, ICloneable, IDisposable, IUserDefine, IWithSizable, IDateTimeDimensionLevel, IMergeStyle
	{
        protected EventType _eventtype = EventType.OnTitle ;
        protected bool _bshowatreal;
        protected string _userdefineitem = "";
        protected bool _bMergeCell = false;
        protected DateTimeDimensionLevel _ddlevel = DateTimeDimensionLevel.ʱ��;
        private bool _showyear=true;
        private bool _showWeekRange = false;
        private bool _supportswitch=false;
		#region constructor
		public GridDateTime():base()
		{
		}

		public GridDateTime(int x,int y):base(x,y)
		{
		}

		public GridDateTime(int x,int y,int width,int height):base(x,y,width,height)
		{
		}

		public GridDateTime(GridDateTime griddatetime):base(griddatetime)
		{
            _eventtype = griddatetime.EventType;
            _bshowatreal = griddatetime.bShowAtReal;
            _ddlevel = griddatetime.DDLevel;
            _showyear = griddatetime.ShowYear;
            _showWeekRange = griddatetime.ShowWeekRange;
            _supportswitch = griddatetime.SupportSwitch;
            _bMergeCell = griddatetime.bMergeCell;
            _userdefineitem = griddatetime.UserDefineItem;
        }

        public GridDateTime(GroupDimension griddatetime)
            : base(griddatetime as Rect)
        {
            _eventtype = griddatetime.EventType;
            _bshowatreal = griddatetime.bShowAtReal;
            _sortoption = griddatetime.SortOption;
            _datasource = griddatetime.DataSource;
            _formatstring = griddatetime.FormatString;
            _ddlevel = griddatetime.DDLevel;
            _showyear = griddatetime.ShowYear;
            _showWeekRange = griddatetime.ShowWeekRange;
            _supportswitch = griddatetime.SupportSwitch;

        }

		protected GridDateTime( SerializationInfo info, StreamingContext context ):base(info,context)
		{
            _eventtype = (EventType)info.GetValue("EventType", typeof(EventType));
            _bshowatreal = info.GetBoolean("bShowAtReal");
            if (_version > 1)
                _userdefineitem = info.GetString("UserDefineItem");

            if (_version >= 100)
            {
                _ddlevel = (DateTimeDimensionLevel)info.GetValue("DDLevel", typeof(DateTimeDimensionLevel));
                _showyear = info.GetBoolean("ShowYear");
                try
                {
                    _showWeekRange = info.GetBoolean("ShowWeekRange");
                }
                catch
                {
                }
                _supportswitch = info.GetBoolean("SupportSwitch");
            }
            try
            {
                _bMergeCell = info.GetBoolean("bMergeCell");
            }
            catch
            {
                _bMergeCell = false;
            }
		}

        public GridDateTime(DataSource ds)
            : base(ds)
        {
        }

        public GridDateTime(IDataSource ds)
            : base(ds as Rect)
        {
            _datasource = ds.DataSource;
        }
		#endregion

		#region override
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

		public override void SetType()
		{
			_type="GridDateTime";
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

		#region ISerializable ��Ա

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info,context);
            info.AddValue("EventType", _eventtype);
            info.AddValue("bShowAtReal", _bshowatreal);
            info.AddValue("UserDefineItem", _userdefineitem);
            info.AddValue("DDLevel", _ddlevel);
            info.AddValue("ShowYear", _showyear);
            info.AddValue("ShowWeekRange", _showWeekRange);
            info.AddValue("SupportSwitch", _supportswitch);
            info.AddValue("bMergeCell", _bMergeCell);
		}

		#endregion

		#region ICloneable ��Ա

		public override object Clone()
		{
			return new GridDateTime(this);
		}

		#endregion

		#region IDisposable ��Ա

		public override void Dispose()
		{
            base.Dispose();
		}

		#endregion

        #region IGridEvent ��Ա

        [DisplayText("U8.UAP.Services.ReportElements.GridCalculateColumn.����¼�������")]
        [LocalizeDescription("U8.UAP.Services.ReportElements.GridCalculateColumn.����¼�������")]
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

        [DisplayText("U8.UAP.Services.ReportElements.GridCalculateColumn.��ʾ�ڽ����к���")]
        [LocalizeDescription("U8.UAP.Services.ReportElements.GridCalculateColumn.��ʾ�ڽ����к���")]
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

        #region IUserDefine ��Ա
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

        [DisplayText("ʱ��ά��")]
        [LocalizeDescription("ʱ��ά��")]
        public DateTimeDimensionLevel DDLevel
        {
            get
            {
                return _ddlevel;
            }
            set
            {
                _ddlevel = value;
            }
        }

        [DisplayText("U8.UAP.Report.ShowYear")]
        [LocalizeDescription("U8.UAP.Report.ShowYear")]
        public bool ShowYear
        {
            get
            {
                return _showyear;
            }
            set
            {
                _showyear = value;
            }
        }
        [DisplayText("U8.UAP.Report.ShowWeekRange")]
        [LocalizeDescription("U8.UAP.Report.ShowWeekRange")]
        public bool ShowWeekRange
        {
            get
            {
                return _showWeekRange;
            }
            set
            {
                _showWeekRange = value;
            }
        }
        [DisplayText("U8.UAP.Report.SupportSwitch")]
        [LocalizeDescription("U8.UAP.Report.SupportSwitch")]
        public bool SupportSwitch
        {
            get
            {
                return _supportswitch;
            }
            set
            {
                _supportswitch = value;
            }
        }
        #region IMergeSyle ��Ա

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
