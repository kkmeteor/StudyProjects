using System;

namespace UFIDA.U8.UAP.Services.ReportData
{
	internal class UpgradeReportView
	{
		private int				_PaperType	= 1;
		private int				_ViewType	= 1;
		
		private bool			_bSystem	= false;
		private bool			_BlandScape	= false;
		private bool			_IsDefault	= true;

		private string			_ID				= string.Empty;
		private string			_Columns		= string.Empty;
		private string			_GroupSchemas	= string.Empty;
		private string			_CommonFormat	= string.Empty;
		private string			_PageMargins	= "80,80,80,80";
		private string			_LevelExpand	= string.Empty;
		
		private UpgradeReport					_Report				= null;
		private ReportViewLocaleInfoCollection	_ViewLocaleInfos	= null;

		public UpgradeReportView( UpgradeReport report)
		{
			this._ViewLocaleInfos	= new ReportViewLocaleInfoCollection( this );
			this._Report			= report;
		}

		public string ID
		{
			get{ return _ID; }
			set{ _ID = value; }
		}

		public bool bSystem
		{
			get{ return _bSystem; }
			set{ _bSystem=value; }
		}

		public int ViewType
		{
			get{ return _ViewType; }
			set{ _ViewType = value; }
		}

		public string Columns
		{
			get{ return _Columns; }
			set{ _Columns = value; }
		}
		
		public bool BlandScape
		{
			get{ return _BlandScape; }
			set{ _BlandScape = value; }
		}

		public string GroupSchemas
		{
			get{ return _GroupSchemas; }
			set{ _GroupSchemas = value; }
		}

		public string PageMargins
		{
			get{ return _PageMargins; }
			set{ _PageMargins = value; }
		}

		public int PaperType
		{
			get{ return _PaperType; }
			set{ _PaperType = value; }
		}

		public bool IsDefault
		{
			get{ return _IsDefault; }
			set{ _IsDefault = value; }
		}

		public string CommonFormat
		{
			get{ return _CommonFormat; }
			set{ _CommonFormat = value; }
		}

		public string LevelExpand
		{
			get{ return _LevelExpand; }
			set{ _LevelExpand = value; }
		}

		public ReportViewLocaleInfoCollection ViewLocaleInfos
		{
			get{ return _ViewLocaleInfos; }
			set{ _ViewLocaleInfos = value; }
		}

		public UpgradeReport Report
		{
			get{ return _Report; }
			set{ _Report = value; }
		}
	}
}
