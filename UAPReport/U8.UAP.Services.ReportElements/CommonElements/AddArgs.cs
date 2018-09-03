using System;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// AddArgs 的摘要说明。
	/// </summary>
	public class AddArgs:EventArgs
	{
		private bool _balreadyadd=false;
		public AddArgs():base()
		{
		}
		public bool bAlreadyAdd
		{
			get
			{
				return _balreadyadd;
			}
			set
			{
				_balreadyadd=value;
			}
		}
	}
}
