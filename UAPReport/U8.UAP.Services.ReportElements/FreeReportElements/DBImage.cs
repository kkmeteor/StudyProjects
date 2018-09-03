using System;
using System.Drawing;
using System.ComponentModel;
using System.Windows.Forms;
using System.Runtime.Serialization;
using System.IO;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// Image 的摘要说明。
	/// </summary>
	[Serializable]
	public class DBImage:Image,IDataSource,IMapName,ISerializable,ICloneable,IDisposable
	{
		#region fields
		protected DataSource _datasource = new DataSource("EmptyColumn");
		#endregion
         
		#region constructor
		public DBImage():base()
		{
		}

		public DBImage(int x,int y):base(x,y)
		{
		}

		public DBImage(int x,int y,int width,int height):base(x,y,width,height)
		{
		}

		public DBImage(DBImage image):base(image)
		{
			_datasource=image.DataSource;
		}

        public DBImage(DataSource ds)
            : base()
        {
            _datasource = ds;
            _name = ds.Name;
            _cncaption = ds.CNCaption;
            _twcaption = ds.TWCaption;
            _encaption = ds.ENCaption;
            _caption = ds.Caption;
        }

		protected DBImage( SerializationInfo info, StreamingContext context ):base(info,context)
		{
			_datasource=(DataSource)info.GetValue("DataSource",typeof(DataSource));
		}
		private string ConvertFromBytes(object value)
		{
			if(value.ToString()=="")
				return "";
			else
				return Convert.ToBase64String((byte[])value);
		}	
		#endregion

		#region override
		public override void SetType()
		{
			_type="DBImage";			
		}
        
		public override void SetDefault()
		{
			base.SetDefault ();
            _borderside.AllBorder ();
			_keeppos=false;
		}
		#endregion

		#region IImage 成员

		[Browsable(false)]
		public override string ImageString
		{
			get
			{
				return base.ImageString ;
			}
			set
			{
				base.ImageString=value;
			}
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
				return _datasource.Name  ;
			}
		}

		#endregion

		#region ISerializable 成员

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info,context);
			info.AddValue("DataSource",_datasource);
		}

		#endregion

		#region ICloneable 成员

		public override object Clone()
		{
			return new DBImage(this);
		}

		#endregion

        #region IDisposable 成员

        public override void Dispose()
        {
            _datasource = null;
            base.Dispose();
        }

        #endregion
    }
}
