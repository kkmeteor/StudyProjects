using System;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// AddArgs ��ժҪ˵����
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
