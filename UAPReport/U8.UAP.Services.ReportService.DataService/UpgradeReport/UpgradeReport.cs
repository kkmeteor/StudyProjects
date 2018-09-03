/*
 * 作者:卢达其
 * 时间:2008.3.18-872重构
 */

using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using UFIDA.U8.UAP.Services.BizDAE.Elements;
using UFIDA.U8.UAP.Services.BizDAE.ConfigureServices;

namespace UFIDA.U8.UAP.Services.ReportData
{
	internal class UpgradeReport
	{
		private bool				_bSystem		= false;

		private string				_ID				= string.Empty;
		private string				_ProjectID		= string.Empty;
		private string				_FilterID		= string.Empty;
		private string				_SubProjectID	= string.Empty;
		private string				_Description	= string.Empty;
		private string				_ClassName		= string.Empty;
		private string				_FilterClass	= string.Empty;
		private string				_DbConnString	= string.Empty;
		private string				_AppServer		= string.Empty;
		private string				_HelpFileName	= string.Empty;
		private string				_HelpIndex		= string.Empty;
		private string				_HelpKeyWord	= string.Empty;
        private string              _cAccId         = string.Empty;
        private string              _cYear          = string.Empty;
		private string              _reportName861  = string.Empty;
		private string              _mappingMenuId  = string.Empty;
        private string              _rootReportId   = string.Empty;

		private DateTime			_CreatedTime	= DateTime.Now;

		private ReportLocaleInfoCollection	_ReportLocaleInfos	= null;
		private UpgradeReportViewCollection	_ReportViews		= null;
		private DataSourceInfor				_DataSourceInfor	= null;

        private bool _isNeedExpandEn = false;
        private bool _isNeedExpandTW = false;

        public UpgradeReport()
		{
			this._ReportLocaleInfos = new ReportLocaleInfoCollection( this );
			this._ReportViews		= new UpgradeReportViewCollection();
			this._DataSourceInfor	= new DataSourceInfor();
		}

		public bool bSystem
		{
			get{ return _bSystem; }
			set{ _bSystem = value; }
		}

        public string cAccId
        {
            get { return _cAccId; }
            set { _cAccId = value; }
        }

        public string cYear
        {
            get { return _cYear; }
            set { _cYear = value; }
        }

		public string HelpFileName
		{
			get{ return _HelpFileName; }
			set{ _HelpFileName = value; }
		}

		public string HelpIndex
		{
			get{ return _HelpIndex; }
			set{ _HelpIndex = value; }
		}

		public string HelpKeyWord
		{
			get{ return _HelpKeyWord; }
			set{ _HelpKeyWord = value; }
		}

		public string DbConnString
		{
			get{ return _DbConnString; }
			set{ _DbConnString = value; }
		}

		public DataSourceInfor DataSourceInfo
		{
			get { return _DataSourceInfor; }
			set { _DataSourceInfor = value; }
		}

		public string AppServer
		{
			get{ return _AppServer; }
			set{ _AppServer = value; }
		}

		public string Description
		{
			get{ return this._Description; }
			set{ this._Description = value; }
		}

		public string ClassName
		{
			get{ return _ClassName; }
			set{ _ClassName = value; }
		}

		public string FilterClass
		{
			get{ return _FilterClass; }
			set{ _FilterClass = value; }
		}

		public string ID
		{
			get{ return _ID; }
			set{ _ID = value; }
		}

		public string ProjectID
		{
			get{ return _ProjectID;}
			set{ _ProjectID = value; }
		}

		public string SubProjectID
		{
			get{ return _SubProjectID; }
			set{ _SubProjectID = value; }
		}

		public string FilterID
		{
			get{ return _FilterID; }
			set{ _FilterID = value; }
		}

		public string ReportName861
		{
			get{ return _reportName861; }
			set{ _reportName861 = value; }
		}

		public string MappingMenuId
		{
			get{ return _mappingMenuId; }
			set{ _mappingMenuId = value; }
		}

        public string RootReportId
        {
            get { return _rootReportId; }
            set { _rootReportId = value; }
        }

		public ReportLocaleInfoCollection ReportLocaleInfos
		{
			get{ return _ReportLocaleInfos; }
		}

		public UpgradeReportViewCollection ReportViews
		{
			get{ return	_ReportViews; }
		}
        public bool IsNeedExpandTW
        {
            get { return _isNeedExpandTW; }
            set { _isNeedExpandTW = value; }
        }

        public bool IsNeedExpandEN
        {
            get { return _isNeedExpandEn; }
            set { _isNeedExpandEn = value; }
        }

		public void Save()
		{
		    SqlConnection cnn = new SqlConnection( this.DbConnString );
		    cnn.Open();
		    SqlTransaction tr = cnn.BeginTransaction();
			
		    try
		    {
				SqlCommand cmd = this.GetCmdSave();
		        SqlHelper.ExecuteNonQuery( tr , cmd );
				string err = cmd.Parameters[ "@err" ].Value.ToString();
				if( !string.IsNullOrEmpty( err ))
					throw new Exception( "升级错误:" + err );
                if(_DataSourceInfor.DataSourceBO!=null)
                    SaveDataSourceEngine.SaveByDataEngineService(_DataSourceInfor, tr);
				tr.Commit();
		    }
		    catch( Exception e )
		    {
		        tr.Rollback();
		        throw e;
		    }
		}

        public void SaveBO()
        {
            SqlConnection cnn = new SqlConnection(this.DbConnString);
            cnn.Open();
            SqlTransaction tr = cnn.BeginTransaction();

            try
            {
                SqlCommand cmd = this.GetCmdCleanBO();
                SqlHelper.ExecuteNonQuery(tr, cmd);
                if (_DataSourceInfor.DataSourceBO != null)
                    SaveDataSourceEngine.SaveByDataEngineService(_DataSourceInfor, tr);
                tr.Commit();
            }
            catch (Exception e)
            {
                tr.Rollback();
                throw e;
            }
        }

     
        public void SaveLang(string localeId)
        {
            SqlConnection cnn = new SqlConnection(this.DbConnString);
            cnn.Open();
            SqlTransaction tr = cnn.BeginTransaction();

            try
            {
                SqlCommand cmd = this.GetCmdExpandLang(localeId);
                SqlHelper.ExecuteNonQuery(tr, cmd);
                //if (_DataSourceInfor.DataSourceBO != null)
                //    SaveDataSourceEngine.SaveByDataEngineService(_DataSourceInfor, tr);
                tr.Commit();
            }
            catch (Exception e)
            {
                tr.Rollback();
                throw e;
            }
        }
  
        public void SaveFactoryView()
        {
            SqlConnection cnn = new SqlConnection(this.DbConnString);
            cnn.Open();
            SqlTransaction tr = cnn.BeginTransaction();

            try
            {
                SqlCommand cmd = this.GetCmdSaveFactoryView();
                SqlHelper.ExecuteNonQuery(tr, cmd);
                //if (_DataSourceInfor.DataSourceBO != null)
                //    SaveDataSourceEngine.SaveByDataEngineService(_DataSourceInfor, tr);
                tr.Commit();
            }
            catch (Exception e)
            {
                tr.Rollback();
                throw e;
            }
        
        }
		private SqlCommand GetCmdSave()
		{
			SqlCommand cmd	= new SqlCommand( "UAP_Report_Upgrade872" );
			cmd.CommandType = CommandType.StoredProcedure;
			
			// Definition Properties
			cmd.Parameters.Add( SqlHelper.GetParameter( "@ReportID",		SqlDbType.NVarChar,	100,	this.ID ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@ReportNameCN",	SqlDbType.NVarChar, 100,	this.ReportLocaleInfos[ "zh-CN" ].Name ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@ReportNameTW",	SqlDbType.NVarChar, 100,	this.ReportLocaleInfos[ "zh-TW" ].Name ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@ReportNameUS",	SqlDbType.NVarChar, 100,	this.ReportLocaleInfos[ "en-US" ].Name ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@FilterID",		SqlDbType.NVarChar,	100,	this.FilterID ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@SubID",			SqlDbType.NVarChar, 50,		this.SubProjectID ) );
            cmd.Parameters.Add( SqlHelper.GetParameter( "@DataSourceID",    SqlDbType.NVarChar, 100, this.DataSourceInfo.DataSourceBO==null?"":this.DataSourceInfo.DataSourceBO.MetaID));
			cmd.Parameters.Add( SqlHelper.GetParameter( "@FunctionName",	SqlDbType.NVarChar, 50,		"ReportFunction" ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@Description",		SqlDbType.NVarChar, 256,	this.Description ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@ClassName",		SqlDbType.NVarChar, 800,	this.ClassName ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@FilterClass",	 	SqlDbType.NVarChar, 800,	this.FilterClass ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@CreatedTime",		SqlDbType.DateTime,	this._CreatedTime ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@DescriptionCN",	SqlDbType.NVarChar, 300,	this.ReportLocaleInfos[ "zh-CN" ].Description ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@DescriptionTW",	SqlDbType.NVarChar, 300,	this.ReportLocaleInfos[ "zh-TW" ].Description ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@DescriptionUS",	SqlDbType.NVarChar, 300,	this.ReportLocaleInfos[ "en-US" ].Description ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@Creator",			SqlDbType.NVarChar, 100,	"Administrator" ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@HelpFileName",	SqlDbType.NVarChar, 300,	this.HelpFileName) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@HelpIndex",		SqlDbType.NVarChar, 30,		this.HelpIndex ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@HelpKeyWord",		SqlDbType.NVarChar, 50,		this.HelpKeyWord ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@bSystem",			SqlDbType.Bit,		this.bSystem ) );
            cmd.Parameters.Add( SqlHelper.GetParameter( "@RootReportId",    SqlDbType.NVarChar, 100,    this.RootReportId));

			UpgradeReportView view = this.ReportViews[0];
			cmd.Parameters.Add( SqlHelper.GetParameter( "@ViewID",			SqlDbType.NVarChar, 100,	view.ID ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@Columns",			SqlDbType.NText,	view.Columns ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@BlandScape" ,		SqlDbType.Bit,		view.BlandScape ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@PageMargins",		SqlDbType.NVarChar,	50,		view.PageMargins ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@CommonFormat",	SqlDbType.NText,	view.CommonFormat ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@ViewNameCN",		SqlDbType.NVarChar, 100,	"标准视图" ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@ViewNameTW",		SqlDbType.NVarChar, 100,	"標準視圖" ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@ViewNameUS",		SqlDbType.NVarChar, 100,	"Default view" ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@LocaleFormatCN",	SqlDbType.NText,	view.ViewLocaleInfos[ "zh-CN" ].LocaleFormat ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@LocaleFormatTW",	SqlDbType.NText,	view.ViewLocaleInfos[ "zh-TW" ].LocaleFormat ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@LocaleFormatUS",	SqlDbType.NText,	view.ViewLocaleInfos[ "en-US" ].LocaleFormat ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@GroupSchemas",	SqlDbType.NText,	view.GroupSchemas ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@LevelExpand",		SqlDbType.NText,	view.LevelExpand ) );
            cmd.Parameters.Add( SqlHelper.GetParameter( "@cAccId",          SqlDbType.NVarChar, 100, _cAccId ));
            cmd.Parameters.Add( SqlHelper.GetParameter( "@cYear",           SqlDbType.VarChar,  100, _cYear ));

			// 为了区分其不是输出参数，把PaperType的类型转换为
			// object.GetParameter的重载列表中第三个参数且是最
			// 后一个参数是int时表明此参数是输出类型
			cmd.Parameters.Add( SqlHelper.GetParameter( "@PaperType",		SqlDbType.Int,		(object)view.PaperType ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@ViewType",		SqlDbType.NVarChar, (object)view.ViewType ) );

			cmd.Parameters.Add( SqlHelper.GetParameter( "@ReportName861",	SqlDbType.NVarChar, 200,	this.ReportName861 ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@MappingMenuId",	SqlDbType.NVarChar, 200,	this.MappingMenuId ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@err",				SqlDbType.NVarChar, 2000 ) );

            cmd.Parameters.Add(SqlHelper.GetParameter("@IsNeedExpandEN", SqlDbType.Bit, this.IsNeedExpandEN));
            cmd.Parameters.Add(SqlHelper.GetParameter("@IsNeedExpandTW", SqlDbType.Bit, this.IsNeedExpandTW));
			
			return cmd;
		}
        private SqlCommand GetCmdCleanBO()
        {
            SqlCommand cmd = new SqlCommand("UAP_Report_CleanBOBeforeUpgrade");
            cmd.CommandType = CommandType.StoredProcedure;

            // Definition Properties
            cmd.Parameters.Add(SqlHelper.GetParameter("@ReportID", SqlDbType.NVarChar, 100, this.ID));
            cmd.Parameters.Add(SqlHelper.GetParameter("@NewDataSourceID", SqlDbType.NVarChar, 100, this.DataSourceInfo.DataSourceBO == null ? "" : this.DataSourceInfo.DataSourceBO.MetaID));
            cmd.Parameters.Add(SqlHelper.GetParameter("@FunctionName", SqlDbType.NVarChar, 50, "ReportFunction"));

            return cmd;
        }
        private SqlCommand GetCmdExpandLang(string localeId)
        {
            SqlCommand cmd = new SqlCommand("UAP_Report_ExpandLangAfterUpgrade");
            cmd.CommandType = CommandType.StoredProcedure;

            // Definition Properties
            cmd.Parameters.Add(SqlHelper.GetParameter("@LocaleId", SqlDbType.NVarChar, 20, localeId));
            UpgradeReportView view = this.ReportViews[0];
            cmd.Parameters.Add(SqlHelper.GetParameter("@ViewID", SqlDbType.NVarChar, 100, view.ID));
            cmd.Parameters.Add(SqlHelper.GetParameter("@LocaleFormatLang", SqlDbType.NText, view.ViewLocaleInfos[localeId].LocaleFormat));
            return cmd;

        }
        private SqlCommand GetCmdSaveFactoryView()
        {
            SqlCommand cmd = new SqlCommand("UAP_Report_Upgrade872ViewFactoryData");
            cmd.CommandType = CommandType.StoredProcedure;

            // Definition Properties
            cmd.Parameters.Add(SqlHelper.GetParameter("@ReportID", SqlDbType.NVarChar, 100, this.ID));          
            cmd.Parameters.Add(SqlHelper.GetParameter("@bSystem", SqlDbType.Bit, this.bSystem));
            
            UpgradeReportView view = this.ReportViews[0];
            cmd.Parameters.Add(SqlHelper.GetParameter("@ViewID", SqlDbType.NVarChar, 100, view.ID));
            cmd.Parameters.Add(SqlHelper.GetParameter("@Columns", SqlDbType.NText, view.Columns));
            cmd.Parameters.Add(SqlHelper.GetParameter("@BlandScape", SqlDbType.Bit, view.BlandScape));
            cmd.Parameters.Add(SqlHelper.GetParameter("@PageMargins", SqlDbType.NVarChar, 50, view.PageMargins));
            cmd.Parameters.Add(SqlHelper.GetParameter("@CommonFormat", SqlDbType.NText, view.CommonFormat));
            cmd.Parameters.Add(SqlHelper.GetParameter("@ViewNameCN", SqlDbType.NVarChar, 100, "标准视图"));
            cmd.Parameters.Add(SqlHelper.GetParameter("@ViewNameTW", SqlDbType.NVarChar, 100, "標準視圖"));
            cmd.Parameters.Add(SqlHelper.GetParameter("@ViewNameUS", SqlDbType.NVarChar, 100, "Default view"));
            cmd.Parameters.Add(SqlHelper.GetParameter("@LocaleFormatCN", SqlDbType.NText, view.ViewLocaleInfos["zh-CN"].LocaleFormat));
            cmd.Parameters.Add(SqlHelper.GetParameter("@LocaleFormatTW", SqlDbType.NText, view.ViewLocaleInfos["zh-TW"].LocaleFormat));
            cmd.Parameters.Add(SqlHelper.GetParameter("@LocaleFormatUS", SqlDbType.NText, view.ViewLocaleInfos["en-US"].LocaleFormat));
            cmd.Parameters.Add(SqlHelper.GetParameter("@GroupSchemas", SqlDbType.NText, view.GroupSchemas));
            cmd.Parameters.Add(SqlHelper.GetParameter("@LevelExpand", SqlDbType.NText, view.LevelExpand));
            cmd.Parameters.Add(SqlHelper.GetParameter("@cAccId", SqlDbType.NVarChar, 100, _cAccId));
            cmd.Parameters.Add(SqlHelper.GetParameter("@cYear", SqlDbType.VarChar, 100, _cYear));

            //// 为了区分其不是输出参数，把PaperType的类型转换为
            //// object.GetParameter的重载列表中第三个参数且是最
            //// 后一个参数是int时表明此参数是输出类型
           cmd.Parameters.Add(SqlHelper.GetParameter("@PaperType", SqlDbType.Int, (object)view.PaperType));
           cmd.Parameters.Add(SqlHelper.GetParameter("@ViewType", SqlDbType.NVarChar, (object)view.ViewType));
           return cmd;
        }
	}
}
