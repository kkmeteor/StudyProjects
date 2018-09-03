using System;
using System.Data;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.Serialization;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// GroupObject 的摘要说明。
	/// </summary>
	[Serializable]
    public class GroupObject : Rect, ISort, IDataSource, IFormat, IMapName, IGroup, ISerializable, ICloneable, IDisposable, IBDateTime, IInformationSender, IDateTimeDimensionLevel
	{
		#region fields
		protected DataSource _datasource = new DataSource("EmptyColumn") ;
		protected SortOption _sortoption ;
        protected string _formatstring = "";
        protected bool _bdatetime = false;
		public bool _bDecimalGroup=false;
        protected string _informationid = "";
        protected DateTimeDimensionLevel _ddlelvel = DateTimeDimensionLevel.时间;
        private bool _showyear = true;
        private bool _supportswitch = false;
        private bool _showWeekRange = false;
		#endregion

		#region constructor
		public GroupObject():base()
		{
		}

		public GroupObject(int x,int y):base(x,y)
		{
		}

		public GroupObject(int x,int y,int width,int height):base(x,y,width,height)
		{
        }

        public GroupObject(DataSource ds)
            : base()
        {
            _datasource = ds;
            _name = ds.Name;
            _bdatetime = (ds.Type == DataType.DateTime);
            _cncaption = ds.CNCaption;
            _twcaption = ds.TWCaption;
            _encaption = ds.ENCaption;
            _caption = ds.Caption;
        }

		public GroupObject(GroupObject groupobject):base(groupobject)
		{
			_datasource=groupobject.DataSource;
			_sortoption =groupobject.SortOption;
			_bdatetime=groupobject.bDateTime;
			_formatstring=groupobject.FormatString;
            _informationid = groupobject.InformationID;
            _showyear = groupobject.ShowYear;
            _showWeekRange = groupobject.ShowWeekRange;
            _supportswitch = groupobject.SupportSwitch;
		}

		protected GroupObject( SerializationInfo info, StreamingContext context ):base(info,context)
		{
			_datasource=(DataSource)info.GetValue("DataSource",typeof(DataSource));			
			_bdatetime=info.GetBoolean("bDateTime");
			_formatstring=info.GetString("FormatString");
            _bDecimalGroup = info.GetBoolean("bDecimalGroup");
            _sortoption = new SortOption();
            _sortoption.SortDirection =(SortDirection)info.GetValue("Direction",typeof(SortDirection));
            if (_version > 1)
                _sortoption.Priority = info.GetInt32("Priority");
            if (_version > 91)
                _informationid = info.GetString("InformationID");
            if (_version >= 100)
            {
                _ddlelvel = (DateTimeDimensionLevel)info.GetValue("DDLevel", typeof(DateTimeDimensionLevel));
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
		}

		public GroupObject(GridLabel gridlabel):base(gridlabel)
		{
			_datasource=gridlabel.DataSource;
            _sortoption =gridlabel.SortOption ;
		}

        public GroupObject(GridDecimal gridlabel)
            : base(gridlabel)
        {
            _datasource = gridlabel.DataSource;
            _sortoption  = gridlabel.SortOption ;
            _bDecimalGroup = true;
        }

		public GroupObject(GridDateTime griddatetime):base(griddatetime)
		{
			_datasource=griddatetime.DataSource;
			_sortoption =griddatetime.SortOption ;
			_bdatetime=true;
			_formatstring=griddatetime.FormatString;
            _showyear = griddatetime.ShowYear;
            _showWeekRange = griddatetime.ShowWeekRange;
            _supportswitch = griddatetime.SupportSwitch;
        }

        public GroupObject(GroupDimension  griddatetime)
            : base(griddatetime)
        {
            _datasource = griddatetime.DataSource;
            _sortoption = griddatetime.SortOption;
            _bdatetime = griddatetime.bDateTime ;
            _formatstring = griddatetime.FormatString;
            _informationid = griddatetime.InformationID;
            _showyear = griddatetime.ShowYear;
            _showWeekRange = griddatetime.ShowWeekRange;
            _supportswitch = griddatetime.SupportSwitch;
        }
		#endregion

		#region override
		public override void SetType()
		{
			_type="GroupObject";
		}
        
        protected override Font GetClientFont(ServerFont sf)
        {
            if (!string.IsNullOrEmpty(_informationid))
                sf.UnderLine = true;
            else
                sf.UnderLine = false;
            return base.GetClientFont(sf);
        }

        protected override Color GetInfomationForeColr()
        {
            Color c = _forecolor;
            if (!string.IsNullOrEmpty(_informationid))
                c = Color.Blue;
            return c;
        }
		#endregion

		#region property	
        [Browsable(true)]
        [DisplayText("U8.Report.bDateTime")]
        [LocalizeDescription("U8.Report.bDateTime")]
		public bool bDateTime
		{
			get
			{
				return _bdatetime;
			}
            set
            {
                _bdatetime = value;
            }
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

		#region ISerializable 成员

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info,context);
			info.AddValue("DataSource",_datasource);
            info.AddValue("Direction", SortOption.SortDirection);
            info.AddValue("Priority", SortOption.Priority);
			info.AddValue("bDateTime",_bdatetime);
			info.AddValue("FormatString",_formatstring);
            info.AddValue("InformationID", _informationid);
            info.AddValue("DDLevel", _ddlelvel);
            info.AddValue("ShowYear", _showyear);
            info.AddValue("ShowWeekRange", _showWeekRange);
            info.AddValue("SupportSwitch", _supportswitch);
            info.AddValue("bDecimalGroup", _bDecimalGroup);
		}

		#endregion

		#region ICloneable 成员

		public override object Clone()
		{
			return new GroupObject(this);
		}

		#endregion

		#region IDisposable 成员

		public override void Dispose()
		{
            _datasource = null;
            _formatstring = null;
            _sortoption = null;
            base.Dispose();
		}

		#endregion

		#region IDataSource 成员

		[Browsable(true)]
		[DisplayText("U8.UAP.Services.ReportElements.Dis16")]
		[LocalizeDescription("U8.UAP.Services.ReportElements.Dis16")]
		public DataSource DataSource
		{
			get
			{
				return _datasource;
			}
			set
			{
				_datasource=value;
				if(_datasource.Type==DataType.DateTime)
					_bdatetime=true;
				else
					_bdatetime=false;
                if (_caption == "" && value != null)
                {
                    _caption = _datasource.Caption;
                    OnOtherChanged(null);
                }
			}
		}
		#endregion

		#region IMapName 成员

		[Browsable(false)]
		public string MapName
		{
			get
			{
				return _datasource.Name  ;
			}
		}

		#endregion

		#region IFormat 成员

		[DisplayText("U8.UAP.Services.ReportElements.Dis22")]
		[LocalizeDescription("U8.UAP.Services.ReportElements.Dis22")]
		public string FormatString
		{
			get
			{
				return _formatstring;
			}
			set
			{
				_formatstring=value;
			}
		}

		#endregion

        #region IInformationSender Members
        [Browsable(false)]
        public string InformationID
        {
            get
            {
                return _informationid;
            }
            set
            {
                _informationid = value;
            }
        }

        #endregion

        [DisplayText("时间维度")]
        [LocalizeDescription("时间维度")]
        public DateTimeDimensionLevel DDLevel
        {
            get
            {
                return _ddlelvel;
            }
            set
            {
                _ddlelvel = value;
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
    }
}
