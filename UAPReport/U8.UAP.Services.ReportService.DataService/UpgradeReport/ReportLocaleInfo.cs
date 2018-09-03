using System;

namespace UFIDA.U8.UAP.Services.ReportData
{
	public class ReportLocaleInfo
	{
		private string	_Name			= string.Empty;
		private string	_LocaleID		= string.Empty;
		private string	_Description	= string.Empty;

		public ReportLocaleInfo()
		{
		}

		public string Name
		{
			get{ return _Name; }
			set{ _Name = value; }
		}

		public string LocaleID
		{
			get{ return _LocaleID; }
			set{ _LocaleID = value; }
		}

		public string Description
		{
			get{ return _Description; }
			set{ _Description = value; }
		}
	}
}
