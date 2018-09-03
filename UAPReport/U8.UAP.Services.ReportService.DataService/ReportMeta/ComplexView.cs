using System;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Runtime.Serialization;

namespace UFIDA.U8.UAP.Services.ReportData
{
    [Serializable]
	public class ComplexView
	{
		private bool			_BlandScape		= false;
		private bool			_IsDefault		= true;
		private bool			_canSaveAs		= true;
		private bool			_bShowDetail	= true;//u8print
        private bool			_bmustshowdetail = true;//canselectprovider

		private int				_ViewType	= -1;
		private int				_PaperType	= 9;
		private int				_RowsCount	= 40;

		private string			_ID				= string.Empty;
		private string			_ReportID		= string.Empty;
		private string			_Name			= string.Empty;
		private string			_Columns		= string.Empty;
		private string			_GroupSchemas	= string.Empty;
		private string			_CommonFormat	= string.Empty;
        private string _LocaleFormat = string.Empty;
		private string			_LocaleID		= string.Empty;
		private string			_DBConnString	= string.Empty;
		private string			_PageMargins	= "80,80,80,80";
		private string			_LevelExpend	= string.Empty;
        private string          _ViewClass = string.Empty;
        private bool          _reportmergecell = false; 
		private string			_assemblyString	= string.Empty;
		private string			_chartString	= string.Empty;
		private string			_runtimeFormat	= string.Empty;
		private string			_fontColorStyleId	= string.Empty;
        private string _pagesetting = string.Empty;
        private string _crossschemas = string.Empty;

		#region Constructor

		public ComplexView(){}

		#endregion

		#region Exposed Interfeces
		
		#region Property
        public string PageSetting
        {
            get
            {
                return _pagesetting;
            }
            set
            {
                _pagesetting = value;
            }
        }

        public bool bMustShowDetail
        {
            get
            {
                return _bmustshowdetail;
            }
            set
            {
                _bmustshowdetail = value;
            }
        }

		public bool CanSaveAs
		{
			get{ return this._canSaveAs; }
			set{ this._canSaveAs = value; }
		}

		public string FontColorStyleId
		{
			get{ return _fontColorStyleId; }
			set{ _fontColorStyleId= value; }
		}

		public string AssemblyString
		{
			get{ return _assemblyString; }
			set{ _assemblyString = value; }
		}

		public string RuntimeFormat
		{
			get{ return this._runtimeFormat; }
			set{ this._runtimeFormat = value; }
		}

		public string ChartString
		{
			get{ return _chartString; }
			set{ _chartString = value; }
		}

		public int RowsCount
		{
			get{ return _RowsCount; }
			set{ _RowsCount = value; }
		}

		public bool bShowDetail
		{
			get{ return _bShowDetail; }
			set{ _bShowDetail = value; }
		}

		public string ViewClass
		{
			get{ return _ViewClass; }
			set{ _ViewClass = value; }
		}

        public bool ReportMergeCell
        {
            get { return _reportmergecell; }
            set { _reportmergecell = value; }
        }
		public string LevelExpend
		{
			get{ return _LevelExpend; }
			set{ _LevelExpend = value; }
		}

		public bool BlandScape
		{
			get{ return _BlandScape; }
			set{ _BlandScape = value; }
		}

		public bool IsDefault
		{
			get{ return _IsDefault; }
			set{ _IsDefault = value; }
		}

		public int ViewType
		{
			get{ return _ViewType; }
			set{ _ViewType = value; }
		}

		public int PaperType
		{
			get{ return _PaperType; }
			set{ _PaperType = value; }
		}

		public string ID
		{
			get{ return _ID; }
			set{ _ID = value; }
		}

		public string ReportID
		{
			get{ return _ReportID; }
			set{ _ReportID = value; }
		}

		public string Name
		{
			get{ return _Name; }
			set{ _Name = value; }
		}

		public string Columns
		{
			get{ return _Columns; }
			set{ _Columns = value; }
		}

        public string CrossSchemas
        {
            get
            {
                return _crossschemas;
            }
            set
            {
                _crossschemas = value;
            }
        }

		public string GroupSchemas
		{
			get{ return _GroupSchemas; }
			set{ _GroupSchemas = value; }
		}

		public string CommonFormat
		{
			get{ return _CommonFormat; }
			set{ _CommonFormat = value; }
		}

		public string LocaleFormat
		{
			get{ return _LocaleFormat; }
			set{ _LocaleFormat = value; }
		}
        
		public string LocaleID
		{
			get{ return _LocaleID; }
			set{ _LocaleID = value; }
		}

		public string DBConnString
		{
			get{ return _DBConnString; }
			set{ _DBConnString = value; }
		}

		public string PageMargins
		{
			get{ return _PageMargins; }
			set{ _PageMargins = value; }
		}

        public string[] GetColumns()
        {
            return _Columns.Split(new string[] { "@#$" }, StringSplitOptions.RemoveEmptyEntries);
        }
		
		#endregion

		#region Internal Method

        internal void SaveCrossSchemas()
        {
            SqlHelper.ExecuteNonQuery(DBConnString, this.GetSaveCrossSchemas());
        }

		internal void SaveLevelExpand()
		{
			SqlHelper.ExecuteNonQuery( DBConnString, this.GetSqlLevelExpend() );
		}

		internal void SaveGroupSchemas()
		{
			SqlHelper.ExecuteNonQuery( DBConnString, this.GetSaveGroupSchemas(),IsolationLevel.Serializable );            
		}

		internal void SavePrintSetting()
		{
            SqlHelper.ExecuteNonQuery(DBConnString, this.GetSavePrintSetting(), IsolationLevel.Serializable);
		}

		#endregion
		#endregion

		#region Private Method
        private SqlCommand GetSaveCrossSchemas()
        {
            SqlCommand cmd = new SqlCommand("UPDATE UAP_ReportView SET PreservedField=@PreservedField WHERE ID=@ViewID ");
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add(SqlHelper.GetParameter("@ViewID", SqlDbType.NVarChar, 100, this.ID));
            cmd.Parameters.Add(SqlHelper.GetParameter("@PreservedField", SqlDbType.NText, this.CrossSchemas ));

            return cmd;
        }

		private SqlCommand GetSaveGroupSchemas()
		{
			SqlCommand cmd		= new SqlCommand( "UAP_Report_ComplexViewSaveGroupSchemas" );
			cmd.CommandType		= CommandType.StoredProcedure;
			cmd.Parameters.Add( SqlHelper.GetParameter( "@ViewID",			SqlDbType.NVarChar, 100, this.ID ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@GroupSchemas",	SqlDbType.NText, this.GroupSchemas ) );
			
			return cmd;
		}

		private SqlCommand GetSavePrintSetting()
		{
            //SqlCommand cmd		= new SqlCommand( "UAP_Report_ComplexViewSavePrintSetting" );
            //cmd.CommandType		= CommandType.StoredProcedure;
            SqlCommand cmd = new SqlCommand("UPDATE UAP_ReportView SET BlandScape=@BlandScape,PageMargins= @PageMargins,Columns= @Columns,PageSetting=@PageSetting WHERE ID=@ViewID ");
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add(SqlHelper.GetParameter("@ViewID", SqlDbType.NVarChar, 100, this.ID));
            cmd.Parameters.Add(SqlHelper.GetParameter("@BlandScape", SqlDbType.Bit, this.BlandScape));
            cmd.Parameters.Add(SqlHelper.GetParameter("@PageMargins", SqlDbType.NVarChar, 50, this.PageMargins));
            cmd.Parameters.Add(SqlHelper.GetParameter("@Columns", SqlDbType.NVarChar, 100, this.Columns));
            cmd.Parameters.Add(SqlHelper.GetParameter("@PageSetting", SqlDbType.NText, this.PageSetting));

            return cmd;
		}

		private string GetSqlLevelExpend()
		{
			return string.Format( "UPDATE UAP_ReportView SET LevelExpend=N'{0}' WHERE ID=N'{1}'",
				_LevelExpend,
				_ID );
		}

		#endregion
	}
}
