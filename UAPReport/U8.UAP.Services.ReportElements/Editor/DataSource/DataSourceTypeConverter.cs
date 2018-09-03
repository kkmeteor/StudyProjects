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
	public class DataSourceTypeConverter:TypeConverter
	{
		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		{
			if (destinationType == typeof(DataSource))
				return true;

			return base.CanConvertTo(context, destinationType);
			
		}

		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			if (sourceType == typeof(string))
				return true;

			return base.CanConvertFrom(context, sourceType);
		}

		public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
		{
			if (destinationType == typeof(System.String) && 
				value is DataSource)
			{
				return ((DataSource)value).Caption;
			}
			return base.ConvertTo(context, culture, value, destinationType);
		}

		public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
		{
			if(value is string)
			{
				Drawing d=(Drawing)context.Instance;
				if (context.PropertyDescriptor.Name=="DataSource")
				{
//					(d as IDataSource).DataSource.Caption=value.ToString();
					return (d as IDataSource).DataSource;
				}
				if (context.PropertyDescriptor.Name=="Unit")
				{
//					(d as ICalculator ).Unit.Caption=value.ToString();
					return (d as ICalculator).Unit;
				}
			}
			return base.ConvertFrom(context, culture, value);
		}
	}
}
