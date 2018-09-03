using System;
using System.Data;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.Serialization;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// DBDecimal 的摘要说明。
	/// </summary>
	[Serializable]
	public class DBDecimal:Rect,ISerializable,ICloneable,IDataSource,IMapName,ISort,IDecimal,IDisposable
	{
		#region fields
		protected DataSource _datasource=new DataSource("EmptyColumn");
		protected PrecisionType _precision=PrecisionType.None;
		protected string _formatstring="";
		protected SortOption _sortoption ;
		protected StimulateBoolean  _bshowwhenzero= StimulateBoolean.None ;
        protected int _pointlength = -1;
		#endregion

		#region Constructor
		public DBDecimal():base()
		{
		}

		public DBDecimal(int x,int y):base(x,y)
		{
		}

		public DBDecimal(int x,int y,int width,int height):base(x,y,width,height)
		{
		}

		public DBDecimal(GridDecimal griddecimal):base(griddecimal)
		{
			_datasource=griddecimal.DataSource;
			_precision=griddecimal.Precision;
			_formatstring=griddecimal.FormatString;
			_sortoption =griddecimal.SortOption ;
            _bshowwhenzero=griddecimal.bShowWhenZero;
            _pointlength = griddecimal.PointLength;
		}

		public DBDecimal(DBDecimal dbdecimal):base(dbdecimal)
		{
			_datasource=dbdecimal.DataSource;
			_precision=dbdecimal.Precision;
			_formatstring=dbdecimal.FormatString;
			_sortoption =dbdecimal.SortOption ;
			_bshowwhenzero=dbdecimal.bShowWhenZero;
            _pointlength = dbdecimal.PointLength;
		}

		protected DBDecimal( SerializationInfo info, StreamingContext context ):base(info,context)
		{
			_datasource=(DataSource)info.GetValue("DataSource",typeof(DataSource));
			_precision=(PrecisionType)info.GetValue("Precision",typeof(PrecisionType));
			_formatstring=info.GetString("FormatString");            
            _pointlength = info.GetInt32("PointLength");
            _sortoption = new SortOption();
            _sortoption.SortDirection =(SortDirection)info.GetValue("Direction",typeof(SortDirection));
            if (_version > 1)
                _sortoption.Priority = info.GetInt32("Priority");
            if(_version>=90)
                _bshowwhenzero = (StimulateBoolean)info.GetValue("bShowWhenZero", typeof(StimulateBoolean));
		}

        public DBDecimal(DataSource ds)
            : base()
        {
            _datasource = ds;
            _name = ds.Name;
            _cncaption = ds.CNCaption;
            _twcaption = ds.TWCaption;
            _encaption = ds.ENCaption;
            _caption = ds.Caption;
        }

        public DBDecimal(Rect rect)
            : base(rect)
        {
           
        }

		#endregion

		#region override
		public override void SetType()
		{
			_type="DBDecimal";
		}
        //public override void SetSolid()
        public override void SetDefault()
        {
            base.SetDefault();
            _captionalign = ContentAlignment.MiddleRight;
        }
		#endregion

		#region ISerializable 成员

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info,context);
			info.AddValue("DataSource",_datasource);
			info.AddValue("Precision",_precision);
			info.AddValue("FormatString",_formatstring);
            info.AddValue("Direction", SortOption.SortDirection);
            info.AddValue("Priority", SortOption.Priority);
			info.AddValue("bShowWhenZero",_bshowwhenzero);
            info.AddValue("PointLength", _pointlength);
		}

		#endregion

		#region IDataSource 成员

		[Browsable(true)]
		[DisplayText("U8.UAP.Services.ReportElements.Dis16")]
		[LocalizeDescription("U8.UAP.Services.ReportElements.Dis16")]
		public virtual DataSource DataSource
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

		#region IDecimal 成员

		[Browsable(true)]
		[DisplayText("U8.UAP.Services.ReportElements.Dis20")]
		[LocalizeDescription("U8.UAP.Services.ReportElements.Dis20")]
        [TypeConverter(typeof(PrecisionTypeConvertor))]
		public UFIDA.U8.UAP.Services.ReportElements.PrecisionType Precision
		{
			get
			{
				return _precision;
			}
			set
			{
				_precision=value;
			}
		}

		[Browsable(true )]
		[DisplayText("U8.UAP.Services.ReportElements.Dis21")]
		[LocalizeDescription("U8.UAP.Services.ReportElements.Dis21")]
		public StimulateBoolean  bShowWhenZero
		{
			get
			{
				return _bshowwhenzero;
			}
			set
			{
				_bshowwhenzero=value;
			}
		}

        [Browsable(false)]
        public int PointLength
        {
            get
            {
                return _pointlength;
            }
            set
            {
                _pointlength = value;
            }
        }

		#endregion

		#region IFormat 成员

		[Browsable(true)]
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

		#region ICloneable 成员

		public override object Clone()
		{
			return new DBDecimal(this);
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
	}
}

