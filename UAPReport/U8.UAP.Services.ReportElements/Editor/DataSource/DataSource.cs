using System;
using System.Data;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Runtime.Serialization;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// DataSource 的摘要说明。
	/// </summary>
	[Serializable]
	[TypeConverter(typeof(DataSourceTypeConverter))]
	[Editor(typeof(DataSourceEditor), typeof(System.Drawing.Design.UITypeEditor))]
	public class DataSource:ICloneable
	{
		#region fields
		private string _name="";
		private string _caption="";
        private DataType _type=DataType.String ;
        private bool _bappend = false;
        private string _tag;
        private string _twcaption;
        private string _encaption;
        private bool _bdimension = false;
		#endregion

		#region constructor
		public DataSource()
		{
		}

		public DataSource(string n)
		{
			_name=n;
		}

		public DataSource(string n,string c)
		{
			_name=n;
			_caption=c;
		}

		public DataSource(string n,string c,DataType t)
		{
			_name=n;
			_caption=c;
			_type=t;
		}

		public DataSource(string n,DataType t)
		{
			_name=n;
			_type=t;
		}

		public DataSource(DataSource ds)
		{
			_name=ds.Name;
            _caption = ds.CNCaption;
            _twcaption = ds.TWCaption;
            _encaption = ds.ENCaption;
			_type=ds.Type;
            _tag = ds.Tag;
            _bappend = ds.bAppend;
            _bdimension = ds.bDimension;
		}
		#endregion

		#region property
        public bool IsEmpty
        {
            get
            {
                return string.IsNullOrEmpty(_name) || _name.ToLower() == "emptycolumn";
            }
        }

        public bool IsADecimal
        {
            get
            {
                return _type == DataType.Currency || _type == DataType.Decimal || _type == DataType.Int;
            }
        }

        public bool bDimension
        {
            get
            {
                return _bdimension;
            }
            set
            {
                _bdimension = value;
            }
        }

		public string Name
		{
			get
			{
				return _name;
			}
			set
			{
				_name=value;
			}
		}

        public bool bAppend
        {
            get
            {
                return _bappend;
            }
            set
            {
                _bappend = value;
            }
        }

        public string Tag
        {
            get
            {
                return _tag;
            }
            set
            {
                _tag = value;
            }
        }

		public string Caption
		{
			get
			{
                string s = _caption;
                if (System.Threading.Thread.CurrentThread.CurrentUICulture.Name == "en-US")
                {
                    s = _encaption;
                }
                else if (System.Threading.Thread.CurrentThread.CurrentUICulture.Name == "zh-TW")
                {
                    s = _twcaption;
                }
                return string.IsNullOrEmpty(s) ? _caption : s;
			}
			set
			{
				_caption=value;
			}
		}

		public DataType Type
		{
			get
			{
				return _type;
			}
			set
			{
				_type=value;
			}
		}

        public string CNCaption
        {
            get
            {
                return _caption;
            }
            set
            {
                _caption = value;
            }
        }

        public string TWCaption
        {
            get
            {
                return _twcaption;
            }
            set
            {
                _twcaption = value;
            }
        }

        public string ENCaption
        {
            get
            {
                return _encaption;
            }
            set
            {
                _encaption = value;
            }
        }

		#endregion

		#region ICloneable
		public object Clone()
		{
			return new DataSource(this);
		}
		#endregion


    }
}
