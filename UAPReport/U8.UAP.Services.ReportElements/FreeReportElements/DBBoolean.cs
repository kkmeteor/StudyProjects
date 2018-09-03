using System;
using System.Runtime.Serialization;
using System.ComponentModel;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// DBBoolean 的摘要说明。
	/// </summary>
	[Serializable]
	public class DBBoolean:Rect,IBoolean,IDataSource,IMapName,ISerializable,ICloneable,IDisposable
	{
		#region fields
		protected bool _checked;
		protected DataSource _datasource = new DataSource("EmptyColumn") ;
		#endregion

		#region constructor
		public DBBoolean():base()
		{
		}

		public DBBoolean(int x,int y):base(x,y)
		{
		}

		public DBBoolean(int x,int y,int width,int height):base(x,y,width,height)
		{
		}

		public DBBoolean(bool bcheck):base()
		{
			_checked=bcheck;
		}

		public DBBoolean(bool bcheck,int x,int y):base(x,y)
		{
			_checked=bcheck;
		}

		public DBBoolean(bool bcheck,int x,int y,int width,int height):base(x,y,width,height)
		{
			_checked=bcheck;
		}

		public DBBoolean(GridBoolean gridboolean):base(gridboolean)
		{
			_checked=gridboolean.Checked;
			_datasource=gridboolean.DataSource;
		}

		public DBBoolean(DBBoolean dbboolean):base(dbboolean)
		{
			_checked=dbboolean.Checked;
			_datasource=dbboolean.DataSource;
		}

		public DBBoolean(SerializationInfo info,StreamingContext context):base(info,context)
		{
			_checked=info.GetBoolean("Checked");
			_datasource=(DataSource)info.GetValue("DataSource",typeof(DataSource));
		}
		#endregion

		#region override
		public override void SetType()
		{
			_type="DBBoolean";
		}

		#endregion

		#region IBoolean 成员

		[Browsable(false)]
		public bool Checked
		{
			get
			{
				return _checked;
			}
			set
			{
				_checked=value;
			}
		}

		#endregion

		#region ISerializable 成员

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info,context);
			info.AddValue("Checked",_checked);
			info.AddValue("DataSource",_datasource);
		}

		#endregion

		#region ICloneable 成员

		public override object Clone()
		{
			return new DBBoolean(this);
		}

		#endregion

		#region IDisposable 成员

		public override void Dispose()
		{
            _datasource = null;
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
				return _datasource.Name;
			}
		}

		#endregion
	}
}
