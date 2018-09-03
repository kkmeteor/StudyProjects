using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	public class HelpSettingTypeConverter:ExpandableObjectConverter
	{
		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		{
			if (destinationType == typeof(HelpSetting ))
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
				value is HelpSetting )
			{
				HelpSetting  hs = (HelpSetting )value;
				return hs.KeyWord;
			}
			return base.ConvertTo(context, culture, value, destinationType);
		}

		public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
		{
            if (value is string)
            {
                string s = (string)value;
                HelpSetting hs = ((Report)context.Instance).HelpInfo;
                hs.KeyWord = s;
                return hs;
            }
			return base.ConvertFrom(context, culture, value);
		}
	}
}
