using System;

namespace UFIDA.U8.UAP.Services.ReportData
{
	/// <summary>
	/// ViewAuth 的摘要说明。
	/// </summary>
	public class ViewAuth
	{
//		private AuthCollection	_ac;
		private string	_id;
		private string	_name;
		private int	_viewtype;
		public ViewAuth()
		{
			
		}

//		public AuthCollection AuthCollection
//		{
//			get
//			{
//				return _ac; 
//			}
//			set
//			{
//				_ac =value;
//			}
//		}
		public string ID
		{
			get
			{
				return _id; 
			}
			set
			{
				_id =value;
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
				_name =value;
			}
		}
		public int ViewType
		{
			get
			{
				return _viewtype; 
			}
			set
			{
				_viewtype =value;
			}
		}

	}
}
