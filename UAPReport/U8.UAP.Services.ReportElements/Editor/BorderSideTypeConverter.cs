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
	public class BorderSideTypeConverter:ExpandableObjectConverter
	{
		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		{
			if (destinationType == typeof(BorderSide))
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
				value is BorderSide)
			{
				string s ="";
				BorderSide bs = (BorderSide)value;
				if (bs.Left)
					s+="Left,";
				if (bs.Top)
					s+="Top,";
				if (bs.Right)
					s+="Right,";
				if (bs.Bottom)
					s+="Bottom,";
				if (s!="")
					s=s.Substring(0,s.Length-1);

				return s;
			}
			return base.ConvertTo(context, culture, value, destinationType);
		}

		public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
		{
			bool bLeft=false;
			bool bTop=false;
			bool bRight=false;
			bool bBottom=false;
			if (value is string) 
			{
				try 
				{
					string s = (string) value;
					string[] sBs=s.Split(',');
					BorderSide bs=((Drawing)context.Instance).Border;
					//BorderSide bs=new BorderSide();
					for(int i=0;i<sBs.Length;i++)
					{
						if (sBs[i].Trim()=="Left")
						{
							bLeft=true;
							bs.Left=true;
						}
						if (sBs[i].Trim()=="Right")
						{
							bRight=true;
							bs.Right=true;
						}
						if (sBs[i].Trim()=="Top")
						{
							bTop=true;
							bs.Top=true;
						}
						if (sBs[i].Trim()=="Bottom")
						{
							bBottom=true;
							bs.Bottom=true;
						}
					}
					if(!bLeft)
						bs.Left=false;
					if(!bTop)
						bs.Top=false;
					if(!bRight)
						bs.Right=false;
					if(!bBottom)
						bs.Bottom=false;
					return bs;
				}
				catch 
				{
					throw new ArgumentException(
                        UFIDA.U8.UAP.Services.ReportResource.U8ResService.GetResString("U8.UAP.Services.ReportElements.BorderSideTypeConverter.ConvertFrom.Ex1", "zh-CN") + (string)value +
                        UFIDA.U8.UAP.Services.ReportResource.U8ResService.GetResString("U8.UAP.Services.ReportElements.BorderSideTypeConverter.ConvertFrom.Ex2","zh-CN"));
				}
			}  
			return base.ConvertFrom(context, culture, value);

		}
	}
}
