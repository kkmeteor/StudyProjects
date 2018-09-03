using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// RowFilter 的摘要说明。
	/// </summary>
	/// 
	[TypeConverterAttribute(typeof(RowFilterTypeConverter))]
    [DescriptionAttribute("U8.WA.GZSQL.frmMainGZBD_c.Picture1.Label2.Caption")]	
	[Serializable]
    public class RowFilter : DisplyTextCustomTypeDescriptor,IDisposable
	{
		private string _FilterString="";
		private string _MapKeys="";
        [NonSerialized]
        private Report _parent;

		public RowFilter()
		{
			
		}

		public RowFilter(string _fs,string _mk)
		{
			_FilterString=_fs;
			_MapKeys=_mk;
		}

        [Browsable(false)]
        public Report Parent
        {
            get
            {
                return _parent;
            }
            set
            {
                _parent = value;
            }
        }

		[Editor(typeof(ExpressionEditor ), typeof(System.Drawing.Design.UITypeEditor))]
        [DisplayText("U8.UAP.Services.ReportElements.RowFilter.过滤串")]
        [LocalizeDescription("U8.UAP.Services.ReportElements.RowFilter.过滤串")]
		public string FilterString
		{
			get
			{
				return _FilterString;
			}
			set
			{
				_FilterString=value;
                if (_FilterString.Trim() == "")
                    _MapKeys = "";
			}
		}

		[Editor(typeof(ExpressionEditor ), typeof(System.Drawing.Design.UITypeEditor))]
        [DisplayText("U8.UAP.Services.ReportElements.RowFilter.对应过滤值")]
        [LocalizeDescription("U8.UAP.Services.ReportElements.RowFilter.对应过滤值")]
		public string MapKeys
		{
			get
			{
				return _MapKeys;
			}
			set
			{
				_MapKeys=value;
			}
		}

        public override string ToString()
        {
            MemoryStream fs = new MemoryStream();
            BinaryFormatter formatter = new BinaryFormatter();
            try
            {
                formatter.Serialize(fs, this);
                return Convert.ToBase64String(fs.ToArray());
            }
            catch (SerializationException ex)
            {
                throw ex;
            }
            finally
            {
                fs.Close();
            }
        }

        public static RowFilter FromString(string s)
        {
            MemoryStream ms = new MemoryStream(Convert.FromBase64String(s));
            try
            {
                RowFilter rf;
                BinaryFormatter formatter = new BinaryFormatter();
                rf = (RowFilter )formatter.Deserialize(ms);
                return rf;
            }
            catch (SerializationException ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                //throw ex;
                return new RowFilter();
            }
            finally
            {
                ms.Close();
            }
        }

        #region IDisposable 成员

        public void Dispose()
        {
            _FilterString=null;
		    _MapKeys=null;
            _parent=null;
        }

        #endregion
    }
}
