using System;

namespace UFIDA.U8.UAP.Services.ReportData
{
	internal class ReportViewLocaleInfo
	{
		private string			_name;
		private string			_localeFormat;
		private string			_localeID;

		public ReportViewLocaleInfo()
		{
			this._name			= string.Empty;
			this._localeFormat	= string.Empty;
			this._localeID		= string.Empty;
		}

		public string Name
		{
			get{ return _name; }
			set{ _name = value; }
		}

		public string LocaleFormat
		{
			get{ return _localeFormat; }
			set{ _localeFormat = value; }
		}

		public string LocaleID
		{
			get{ return _localeID; }
			set{ _localeID = value; }
		}
	}
}
