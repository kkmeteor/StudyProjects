using System;
using System.Runtime.Serialization;
using System.ComponentModel;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// DBDateTime ��ժҪ˵����
	/// </summary>
	[Serializable]
	public class DBDateTime:Rect,IDateTime,IDataSource,IMapName,ISerializable,ICloneable,IDisposable
	{
		#region fields
		protected DataSource _datasource = new DataSource("EmptyColumn");
		protected string _formatstring="";
		protected SortOption _sortoption ;
		#endregion

		#region constructor
		public DBDateTime():base()
		{
		}

		public DBDateTime(int x,int y):base(x,y)
		{
		}

		public DBDateTime(int x,int y,int width,int height):base(x,y,width,height)
		{
		}

		public DBDateTime(DBDateTime dbdatetime):base(dbdatetime)
		{
			_datasource=dbdatetime.DataSource;
			_formatstring=dbdatetime.FormatString;
            _sortoption = dbdatetime.SortOption;
		}

		public DBDateTime(GridDateTime griddatetime):base(griddatetime)
		{
			_datasource=griddatetime.DataSource;
			_formatstring=griddatetime.FormatString;
            _sortoption = griddatetime.SortOption;
		}

		protected DBDateTime( SerializationInfo info, StreamingContext context ):base(info,context)
		{
			_datasource=(DataSource)info.GetValue("DataSource",typeof(DataSource));
			_formatstring=info.GetString("FormatString");
            _sortoption = new SortOption();
			_sortoption.SortDirection =(SortDirection)info.GetValue("Direction",typeof(SortDirection));
            if (_version > 1)
                _sortoption.Priority = info.GetInt32("Priority");
		}

        public DBDateTime(DataSource ds)
            : base()
        {
            _datasource = ds;
            _name = ds.Name;
            _cncaption = ds.CNCaption;
            _twcaption = ds.TWCaption;
            _encaption = ds.ENCaption;
            _caption = ds.Caption;
        }

        public DBDateTime(Rect rect)
            : base(rect)
        {
        }
		#endregion

		#region override
		public override void SetType()
		{
			_type="DBDateTime";
		}

		#endregion

		#region IDateTime ��Ա

		#endregion

		#region IFormat ��Ա

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

		#region ISort ��Ա

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

		#region IDataSource ��Ա

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
                if (_caption == "" && value != null)
                {
                    _caption = _datasource.Caption;
                    OnOtherChanged(null);
                }
			}
		}


		#endregion

		#region ISerializable ��Ա

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info,context);
			info.AddValue("DataSource",_datasource);
            info.AddValue("Direction", SortOption.SortDirection);
            info.AddValue("Priority", SortOption.Priority);
			info.AddValue("FormatString",_formatstring);
		}

		#endregion

		#region ICloneable ��Ա

		public override object Clone()
		{
			return new DBDateTime(this);
		}

		#endregion

		#region IDisposable ��Ա

		public override void Dispose()
		{
            _datasource = null;
            _formatstring = null;
            _sortoption = null;
            base.Dispose();
		}

		#endregion

		#region IMapName ��Ա

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
