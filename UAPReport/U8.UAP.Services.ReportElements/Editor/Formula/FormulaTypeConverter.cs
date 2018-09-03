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
	public class FormulaTypeConverter:ExpandableObjectConverter
	{
		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		{
			if (destinationType == typeof(Formula))
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
				value is Formula)
			{				
				return ((Formula)value).FormulaExpression;
			}
			return base.ConvertTo(context, culture, value, destinationType);
		}

		public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
		{
			if (value is string) 
			{
				try 
				{
					Formula f=context.Instance as Formula;
					f.FormulaExpression=value.ToString();
					return f;
				}
				catch 
				{
					throw new ArgumentException(
                        UFIDA.U8.UAP.Services.ReportResource.U8ResService.GetResString("U8.UAP.Services.ReportElements.BorderSideTypeConverter.ConvertFrom.Ex1", "zh-CN") + (string)value +
                        UFIDA.U8.UAP.Services.ReportResource.U8ResService.GetResString("U8.UAP.Services.ReportElements.Curves.value_NameCheck.Ex2", "zh-CN"));
				}
			}  
			return base.ConvertFrom(context, culture, value);

		}
	}
}
