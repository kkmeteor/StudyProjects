using System;
using System.Data;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.Serialization;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// DBText ��ժҪ˵����
	/// </summary>
	[Serializable]
	public class DBText:Rect,ISerializable,ICloneable,IDataSource,IMapName,ISort,IDisposable
	{
		#region fields
		protected DataSource _datasource = new DataSource("EmptyColumn");
        protected SortOption _sortoption;
		#endregion

		#region constructor
		public DBText():base()
		{			
		}
		public DBText(int x,int y):base(x,y)
		{
		}

		public DBText(int x,int y,int width,int height):base(x,y,width,height)
		{
		}

		public DBText(GridLabel gridlabel):base(gridlabel)
		{
			_datasource=gridlabel.DataSource;
            _sortoption =gridlabel.SortOption ;
		}

        public DBText(DataSource ds)
            : base()
        {
            _datasource = ds;
            _name = ds.Name;
            _cncaption = ds.CNCaption;
            _twcaption = ds.TWCaption;
            _encaption = ds.ENCaption;
            _caption = ds.Caption;
        }

        public DBText(IDataSource ds)
            : base(ds as Rect )
        {
            _datasource = ds.DataSource;
        }

        public DBText(Label gridlabel)
            : base(gridlabel)
        {
        }

		public DBText(DBText dbtext):base(dbtext)
		{
			_datasource=dbtext.DataSource;
			_sortoption =dbtext.SortOption ;
		}

		protected DBText( SerializationInfo info, StreamingContext context ):base(info,context)
		{
			_datasource=(DataSource)info.GetValue("DataSource",typeof(DataSource));
            _sortoption = new SortOption();
			_sortoption.SortDirection =(SortDirection)info.GetValue("Direction",typeof(SortDirection));
            if (_version > 1)
                _sortoption.Priority = info.GetInt32("Priority");
		}
		#endregion

		#region override
		public override void SetType()
		{
			_type="DBText";
		}

		#endregion

		#region ISerializable ��Ա

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info,context);
			info.AddValue("DataSource",_datasource);
			info.AddValue("Direction",SortOption.SortDirection );
            info.AddValue("Priority", SortOption.Priority);
		}

		#endregion

		#region ICloneable ��Ա

		public override object Clone()
		{
			return new DBText(this);
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

		#region IDisposable ��Ա

		public override void Dispose()
		{
            _datasource = null;
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
