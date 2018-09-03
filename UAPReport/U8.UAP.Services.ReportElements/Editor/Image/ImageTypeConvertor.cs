using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// BorderSideTypeConvertor 的摘要说明。
	/// </summary>
	public class ImageTypeConverter:TypeConverter
	{
		public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
		{
			if(value==null ||value.ToString()=="")
				return "";
			return "(Image)";
		}
	}
}
