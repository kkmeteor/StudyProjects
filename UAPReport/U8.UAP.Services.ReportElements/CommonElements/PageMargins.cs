using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// PageMargins 的摘要说明。
	/// </summary>
	/// 
	[Serializable]
	[TypeConverterAttribute(typeof(PageMarginsTypeConverter))]
	public class PageMargins:ICloneable
	{
		#region fields
		private int _Left=DefaultConfigs.DefaultMargin;
		private int _Top=DefaultConfigs.DefaultMargin;
		private int _Right=DefaultConfigs.DefaultMargin;
		private int _Bottom=DefaultConfigs.DefaultMargin;
		#endregion

		#region constructor
		public PageMargins()
		{
			
		}
        
		public PageMargins(int l,int t,int r,int b)
		{
			_Left=l;
			_Top=t;
			_Right=r;
			_Bottom=b;
		}

		public PageMargins(PageMargins pm)
		{
			_Left=pm.Left;
			_Top=pm.Top;
			_Right=pm.Right;
			_Bottom=pm.Bottom;
		}
		#endregion

		#region property
		public int Left
		{
			get
			{
				return _Left;
			}
			set
			{
				_Left=value;
			}
		}

		public int Top
		{
			get
			{
				return _Top;
			}
			set
			{
				_Top=value;
			}
		}

		public int Right
		{
			get
			{
				return _Right;
			}
			set
			{
				_Right=value;
			}
		}

		public int Bottom
		{
			get
			{
				return _Bottom;
			}
			set
			{
				_Bottom=value;
			}
		}
		#endregion

		#region method
		public string ConvertToString()
		{
			 return _Left.ToString()+","+_Top.ToString()+","+_Right.ToString()+","+_Bottom.ToString();
		}

		public static PageMargins ConvertToPageMargins(string s)
		{
			PageMargins pm=new PageMargins();
			int i=0;
			string[] sPm=null;
			if(s!=null && s.Length>0)
			{
				sPm=s.Split(',');			
				i=sPm.Length;
			}
			switch(i)
			{
				case 0:
					pm.Left=DefaultConfigs.DefaultMargin;
					pm.Top=DefaultConfigs.DefaultMargin;
					pm.Right=DefaultConfigs.DefaultMargin;
					pm.Bottom=DefaultConfigs.DefaultMargin;
					break;
				case 1:
					pm.Left=Convert.ToInt32(sPm[0]);
					pm.Top=DefaultConfigs.DefaultMargin;
					pm.Right=DefaultConfigs.DefaultMargin;
					pm.Bottom=DefaultConfigs.DefaultMargin;
					break;
				case 2:
					pm.Left=Convert.ToInt32(sPm[0]);
					pm.Top=Convert.ToInt32(sPm[1]);
					pm.Right=DefaultConfigs.DefaultMargin;
					pm.Bottom=DefaultConfigs.DefaultMargin;
					break;
				case 3:
					pm.Left=Convert.ToInt32(sPm[0]);
					pm.Top=Convert.ToInt32(sPm[1]);
					pm.Right=Convert.ToInt32(sPm[2]);
					pm.Bottom=DefaultConfigs.DefaultMargin;
					break;
				default:
					pm.Left=Convert.ToInt32(sPm[0]);
					pm.Top=Convert.ToInt32(sPm[1]);
					pm.Right=Convert.ToInt32(sPm[2]);
					pm.Bottom=Convert.ToInt32(sPm[3]);
					break;
			}
			return pm;
		}
		#endregion

		#region ICloneable 成员

		public object Clone()
		{
			return new PageMargins(this);
		}

		#endregion
	}
}
