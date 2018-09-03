using System;

namespace UFIDA.U8.UAP.Services.ReportData
{
	public class ReportInfo
	{
		private string _reportID;
		private string _reportName;
		private string _subID;
		
		private RunTimeReportType _rptType;

		public ReportInfo(
			string reportID, 
			string reportName, 
			string subID,
			RunTimeReportType rptType )
		{
			this._reportID		= reportID;
			this._reportName	= reportName;
			this._subID			= subID;
			this._rptType		= rptType;
		}

		public string ReportID
		{
			get{ return _reportID; }
		}
	
		public string ReportName
		{
			get{ return _reportName; }
		}

		public string SubID
		{
			get{ return _subID; }
		}

		public RunTimeReportType RptType
		{
			get{ return _rptType; }
		}
	}
}
