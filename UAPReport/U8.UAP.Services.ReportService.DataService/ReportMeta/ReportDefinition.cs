using System;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using UFIDA.U8.UAP.Services.ReportResource;

namespace UFIDA.U8.UAP.Services.ReportData
{
	public class ReportDefinition
	{
		#region Parameter
		
		private static string		_DbConnString	= string.Empty;
        private static string       _cYear = string.Empty;
        private bool _bAfterRWizard = false;
        private bool _reportmergecell = false;
		private static string		_cAccId			= string.Empty;

		private bool				_bVB			= true;
		private bool				_bSystem		= false;
		private bool				_canSaveAs		= true;
		private bool				_bUsingReport4MenuId = false;
		
		private string				_ID				= string.Empty;
		private string				_Name			= string.Empty;
		private string				_ProjectID		= string.Empty;
		private string				_DataSourceID	= string.Empty;
		private string				_DataSourceIDExtended = string.Empty;
		private string				_FunctionName	= string.Empty;
		private string				_FilterID		= string.Empty;
		private string				_SubProjectID	= string.Empty;
		private string				_LocaleID		= string.Empty;
		private string				_Description	= string.Empty;
		private string				_ClassName		= string.Empty;
		private string				_FilterClass	= string.Empty;
		private string				_UserID			= string.Empty;
		private string				_Creator		= string.Empty;
		private string				_HelpFileName	= string.Empty;
		private string				_HelpIndex		= string.Empty;
		private string				_HelpKeyWord	= string.Empty;
		private string				_rootReportId	= string.Empty;
		private string				_mappingMenuId	= string.Empty;
		
		private DateTime			_CreatedTime	= DateTime.Now;
		
		private U8LoginInfor _login = null;
		private PersistentObjectState _State = PersistentObjectState.New;
		private SimpleViewCollection	_SimpleViews	= new SimpleViewCollection();
		private ComplexView _ComplexView	= null;

		#endregion

		#region Constructor

		public ReportDefinition()
		{
			_ComplexView = new ComplexView();
		}

		#endregion

		#region Exposed Interfeces

		#region Property

		internal static string cYear
		{
			get { return _cYear; }
			set { _cYear = value; }
		}

		internal static string cAccId
		{
			get { return _cAccId; }
			set { _cAccId = value; }
		}

		internal U8LoginInfor Login
		{
			get { return this._login; }
			set { this._login = value; }
		}

		public bool bVB
		{
			get{ return _bVB; }
			set{ _bVB = value; }
		}
        public bool bAfterRWizard
        {
            get { return _bAfterRWizard; }
            set { _bAfterRWizard = value; }
        }
        public bool ReportMergeCell
        {
            get { return _reportmergecell; }
            set { _reportmergecell = value; }
        }

		public bool bUsingReport4MenuId
		{
			get{ return this._bUsingReport4MenuId; }
		}

		public bool bSystem
		{
			get{ return _bSystem; }
			set{ _bSystem=value; }
		}

		public bool CanSaveAs
		{
			get{ return this._canSaveAs; }
			set{ this._canSaveAs = value; }
		}

		public string ID
		{
			get{ return _ID; }
			set{ _ID = value; }
		}

		public string RootReportId
		{
			get{ return _rootReportId; }
			set{ _rootReportId = value; }
		}

		public string MappingMenuId
		{
			get{ return this._mappingMenuId; }
			set{ this._mappingMenuId = value; }
		}

		public string Name
		{
			get{ return _Name; }
			set{ _Name = value; }
		}

		public string ProjectID
		{
			get{ return _ProjectID; }
			set{ _ProjectID = value; }
		}

		public string DataSourceID
		{
			get{ return _DataSourceID; }
			set{ _DataSourceID = value; }
		}

		public string DataSourceIDExtended
		{
			get{ return _DataSourceIDExtended; }
			set{ _DataSourceIDExtended = value; }
		}

		public string FunctionName
		{
			get{ return _FunctionName; }
			set{ _FunctionName = value; }
		}

		public string FilterID
		{
			get{ return _FilterID; }
			set{ _FilterID = value; }
		}

		public string SubProjectID
		{
			get{ return _SubProjectID; }
			set{ _SubProjectID = value; }
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

		public string HelpFileName
		{
			get{ return _HelpFileName; }
		}

		public string HelpIndex
		{
			get{ return _HelpIndex; }
		}

		public string HelpKeyWord
		{
			get{ return _HelpKeyWord; }
		}

		public PersistentObjectState State
		{
			get{ return _State; }
			set{ _State = value; }
		}

		public SimpleViewCollection SimpleViews
		{
			get{ return _SimpleViews; }
			set{ _SimpleViews = value; }
		}

		public ComplexView ComplexView
		{
			get{ return _ComplexView; }
			set{ _ComplexView = value; }
		}

		public string Creator
		{
			get{ return _Creator; }
			set{ _Creator = value; }
		}

		public DateTime CreatedTime
		{
			get{ return _CreatedTime; }
			set{ _CreatedTime = value; }
		}

		internal string UserID
		{
			get{ return _UserID; }
			set{ _UserID = value; }
		}

		internal static string DbConnString
		{
			get{ return _DbConnString; }
			set{ _DbConnString = value; }
		}

		#endregion

		#region Internal Method

		public string GetActualDataSourceId()
		{
			if(!string.IsNullOrEmpty(this.DataSourceIDExtended))
				return this.DataSourceIDExtended;
			return this.DataSourceID;
		}

		internal static void Rename(
			string reportID, 
			string reportName, 
			string localeID, 
			string subProjectID,
            string cAccID,
            string cYear)
		{
			CheckDbCnnString();
			CheckNameExist( reportID, reportName, subProjectID );
			SqlHelper.ExecuteNonQuery( DbConnString , GetCmdReName( reportID, reportName, localeID,cAccID,cYear ) );
		}

		internal static bool IsExistedReportName( 
			string reportID, 
			string reportName, 
			string subProjectID )
		{
			CheckDbCnnString();
			SqlCommand cmd = GetCmdIsExistedReportName( reportID, reportName, subProjectID );
			SqlHelper.ExecuteNonQuery( DbConnString, cmd );

			return Convert.ToBoolean( cmd.Parameters[ "@IsExisted" ].Value );
		}

		internal static void UpdateFilterCondition(
			string reportID, 
			string filterID, 
			string filterClass )
		{ 
			CheckDbCnnString();
			
			SqlCommand cmd = GetCmdUpdateFilterCondition( reportID, filterID, filterClass );
			SqlHelper.ExecuteNonQuery( DbConnString, cmd );
		}

		internal static void UpdateDataSourceCondition(
			string reportID,
			string className,
			string bVB,
			string functionName )
		{ 
			CheckDbCnnString();
			
			string sql = string.Format( 
				@"UPDATE UAP_Report 
				SET ClassName=N'{0}',
				bVB=N'{1}',
				FunctionName=N'{2}' 
				WHERE ID=N'{3}'",
				className, bVB, functionName, reportID );
			SqlHelper.ExecuteNonQuery( DbConnString, sql );
		}

		internal void Save()
		{
			CheckDbCnnString();
			switch( this._State )
			{
				case PersistentObjectState.New:
					Insert();
					break;
				case PersistentObjectState.View:
					Update();
					break;
				case PersistentObjectState.Delete:
					Delete();
					break;
				default:
					break;
			}
		}

		internal void Insert()
		{
			try
			{
				CheckUserID();
				SqlHelper.ExecuteNonQuery( DbConnString , this.GetCmdInsert() );
				this.State = PersistentObjectState.View;
				SetSimpleViewsState( PersistentObjectState.View );
			}
			catch( Exception e )
			{
				DeleteBusinessObjectIfInsertFailed();
				throw e;
			}
		}

		internal void Update()
		{
			SqlHelper.ExecuteNonQuery( DbConnString , this.GetCmdUpdate() );
		}
	
		internal void Delete()
		{
			SqlHelper.ExecuteNonQuery(this.Login.UfDataCnnString, this.GetCmdDelete());
            SqlHelper.ExecuteNonQuery(DbConnString, this.GetCmdDeleteUapRpt());
		}

		internal bool Retrieve( bool isForCustomReportUpgrade )
		{
			return Retrieve( false, false, isForCustomReportUpgrade );
		}

		internal bool Retrieve( 
			bool isByViewID, 
			bool isDesigningTime )
		{
			System.Diagnostics.Trace.WriteLine("进入ReportDefinition.Retrieve()");

			return Retrieve( isByViewID, isDesigningTime, false );
		}

		// isByViewID:是否通过视图id来获取报表数据
		// isDesigningTime:是否设计时调用来区别查看或设置权限
		// isForCustomReportUpgrade:是否升级自定义报表时调用,
		//							此时不需要ComplexView和SimpleView的信息,
		//							也不需要控制权限
		internal bool Retrieve( 
			bool isByViewID, 
			bool isDesigningTime,
			bool isForStaticReport )
		{
			CheckDbCnnString();

			SqlCommand	cmd	= this.GetCmdRetrieve( isByViewID, isDesigningTime, isForStaticReport );
			DataSet		ds	= SqlHelper.ExecuteDataSet( DbConnString , cmd );

			if( ds.Tables.Count < 3 
				|| ds.Tables[0].Rows.Count == 0 
				|| ds.Tables[1].Rows.Count == 0 
				|| ds.Tables[2].Rows.Count == 0 )
			{
				this.LogRetrieveErr(cmd, ds);
				return false;
			}

			this.FillDefinition( ds.Tables[0] );
			string defaultViewID = cmd.Parameters[ "@DefaultViewID" ].Value.ToString();
			this.FillSimpleViewCollection( ds.Tables[1], defaultViewID );
			this.FillComplexView( ds.Tables[2] );
			this.State = PersistentObjectState.View;
			this._ComplexView.CanSaveAs = this.CanSaveAs; //是否可以另存
			return true;
		}

		private void LogRetrieveErr(SqlCommand cmd, DataSet ds)
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine("获取ReportDefinition时数据有错，这是有些无规律问题出错的起点，以下是相关数据:");
			sb.AppendLine("方法:ReportDefinition.Retrieve()");
			sb.AppendLine("错误原因:");
			if (ds.Tables.Count < 3)
				sb.AppendLine("ds.Tables.Count < 3");
			if (ds.Tables[0].Rows.Count == 0)
				sb.AppendLine("ds.Tables[0].Rows.Count == 0");
			if (ds.Tables[1].Rows.Count == 0)
				sb.AppendLine("ds.Tables[1].Rows.Count == 0");
			if (ds.Tables[2].Rows.Count == 0)
				sb.AppendLine("ds.Tables[2].Rows.Count == 0");
			sb.AppendLine("CommandType: " + cmd.CommandText);
			foreach (SqlParameter p in cmd.Parameters)
				sb.AppendLine("ParaName:" + p.ParameterName + ", Value:" + p.Value.ToString());
			Logger logger = Logger.GetLogger(ReportDataFacade._loggerName);
			logger.Info(sb.ToString());
			logger.Close();
		}

		#endregion

		#endregion
		
		#region Private Method

		private void SetSimpleViewsState( PersistentObjectState state )
		{ 
			for( int i=0; i < this.SimpleViews.Count; i++ )
				this.SimpleViews[i].State = state;
		}

		private void DeleteBusinessObjectIfInsertFailed()
		{
			SqlCommand cmd	= new SqlCommand( "UAP_Report_OthersDeleteBusinessObject" );
			cmd.CommandType	= CommandType.StoredProcedure;
			cmd.Parameters.Add( SqlHelper.GetParameter( "@ReportID",		SqlDbType.NVarChar,	100,	this.ID ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@DataSourceID",	SqlDbType.NVarChar, 50,		this.DataSourceID ) );
			cmd.Parameters.Add(SqlHelper.GetParameter("@DataSourceIDExtended", SqlDbType.NVarChar,50,this.DataSourceIDExtended));
            
			SqlHelper.ExecuteNonQuery( _DbConnString, cmd );
		}

		private static void CheckNameExist(
			string reportID, 
			string reportName, 
			string subProjectID )
		{
			if( IsExistedReportName( reportID, reportName, subProjectID ) )
			{
                ReportDataException.ThrowException(UFIDA.U8.UAP.Services.ReportResource.U8ResService.GetResStringEx("U8.UAP.Services.ReportData.ReportDefinition.在该子系统(包括不同语言下)中已经存在相同名称的报表"),
					"ReportDefinition.CheckNameExist()" );
			}
		}

		private static void CheckDbCnnString()
		{
			if( DbConnString == string.Empty )
				ReportDataException.ThrowCnnStringEmptyException( "ReportDefinition.CheckDbCnnString()" );
		}

		private void CheckUserID()
		{
			if( _UserID == string.Empty )
			{
                ReportDataException.ThrowException(UFIDA.U8.UAP.Services.ReportResource.U8ResService.GetResStringEx("U8.UAP.Services.ReportData.ReportDefinition.在调用Insert()时UserID不能为空"),
					"ReportDefinition.CheckUserID()" );
			}
		}

		private static SqlCommand GetCmdUpdateFilterCondition(
			string reportID, 
			string filterID, 
			string filterClass )
		{
			SqlCommand cmd	= new SqlCommand( "UAP_Report_DefinitionUpdateFilterCondition" );
			cmd.CommandType = CommandType.StoredProcedure;

			cmd.Parameters.Add( SqlHelper.GetParameter( "@ReportID",	SqlDbType.NVarChar,	100,	reportID ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@FilterID",	SqlDbType.NVarChar,	100,	filterID ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@FilterClass",	SqlDbType.NVarChar,	800,	filterClass ) );

			return cmd;
		}

		private SqlCommand GetCmdInsert()
		{
			SqlCommand cmd	= new SqlCommand( "UAP_Report_DefinitionInsert" );
			cmd.CommandType	= CommandType.StoredProcedure;
			
			// Definition Properties
			cmd.Parameters.Add( SqlHelper.GetParameter( "@ReportID",		SqlDbType.NVarChar,	100,	this.ID ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@ReportName",		SqlDbType.NVarChar, 100,	this.Name ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@FilterID",		SqlDbType.NVarChar,	100,	this.FilterID ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@SubID",			SqlDbType.NVarChar,	50,		this.SubProjectID ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@DataSourceID",	SqlDbType.NVarChar, 100,	this.DataSourceID ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@LocaleID",		SqlDbType.NVarChar, 10,		this.LocaleID ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@FunctionName",	SqlDbType.NVarChar, 50,		this.FunctionName ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@Description",		SqlDbType.NVarChar, 256,	this.Description ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@ProjectID",		SqlDbType.NVarChar, 50,		this.ProjectID ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@ClassName",		SqlDbType.NVarChar, 800,	this.ClassName ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@FilterClass",		SqlDbType.NVarChar, 800,	this.FilterClass ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@Creator",			SqlDbType.NVarChar, 100,	this.Creator ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@CreatedTime",		SqlDbType.DateTime,	this.CreatedTime ) );

			// SimpleViwe Properties
			cmd.Parameters.Add( SqlHelper.GetParameter( "@bVB",				SqlDbType.Bit, this.bVB ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@bSystem",			SqlDbType.Bit, this.bSystem ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@ViewID" ,			SqlDbType.NVarChar, 100,	this.SimpleViews[0].ID ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@ViewType",		SqlDbType.Int,		(object)this.SimpleViews[0].ViewType ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@ViewName",		SqlDbType.NVarChar, 100,	this.SimpleViews[0].Name ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@UserID",			SqlDbType.NVarChar, 100,	_UserID ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@cAccId",			SqlDbType.NVarChar, 50,		_cAccId ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@cYear",			SqlDbType.NVarChar, 50,		_cYear ) );

			return cmd;
		}

		private SqlCommand GetCmdUpdate()
		{
			SqlCommand cmd	= new SqlCommand( "UAP_Report_DefinitionUpdate" );
			cmd.CommandType = CommandType.StoredProcedure;
			
			// Definition Properties
			cmd.Parameters.Add( SqlHelper.GetParameter( "@ReportID",		SqlDbType.NVarChar,	100,	this.ID ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@ReportName",		SqlDbType.NVarChar, 100,	this.Name ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@FilterID",		SqlDbType.NVarChar,	100,	this.FilterID ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@DataSourceID" ,	SqlDbType.NVarChar, 100,	this.DataSourceID ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@LocaleID",		SqlDbType.NVarChar, 10,		this.LocaleID ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@FunctionName",	SqlDbType.NVarChar, 50,		this.FunctionName ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@Description",		SqlDbType.NVarChar, 256,	this.Description ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@ClassName",		SqlDbType.NVarChar, 800,	this.ClassName ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@FilterClass",		SqlDbType.NVarChar, 800,	this.FilterClass ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@bVB",				SqlDbType.Bit, this.bVB ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@HelpFileName",	SqlDbType.NVarChar, 200,	this.HelpFileName ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@HelpIndex",		SqlDbType.NVarChar, 30,		this.HelpIndex ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@HelpKeyWord",		SqlDbType.NVarChar, 50,		this.HelpKeyWord ) );
			
			// Viwe Properties
			cmd.Parameters.Add( SqlHelper.GetParameter( "@ViewID",			SqlDbType.NVarChar, 100,	this.ComplexView.ID ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@LocaleFormat",	SqlDbType.NText,	this.ComplexView.LocaleFormat ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@Columns",			SqlDbType.NText,	this.ComplexView.Columns ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@BlandScape" ,		SqlDbType.Bit,		this.ComplexView.BlandScape ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@PageMargins",		SqlDbType.NVarChar,	50,		this.ComplexView.PageMargins ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@CommonFormat",	SqlDbType.NText,	this.ComplexView.CommonFormat ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@ViewClass" ,		SqlDbType.NVarChar,	100,	this.ComplexView.ViewClass ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@RowsCount",		SqlDbType.Int,		(object)this.ComplexView.RowsCount ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@bShowDetail",		SqlDbType.Bit,		this.ComplexView.bShowDetail ) );
            cmd.Parameters.Add( SqlHelper.GetParameter( "@bMustShowDetail", SqlDbType.Bit, this.ComplexView.bMustShowDetail));
			cmd.Parameters.Add( SqlHelper.GetParameter( "@AssemblyString",	SqlDbType.NText,	this.ComplexView.AssemblyString ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@ChartString",		SqlDbType.NText,	this.ComplexView.ChartString ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@RuntimeFormat",	SqlDbType.NText,	this.ComplexView.RuntimeFormat ) );
			
			// 为了区分其不是输出参数，把PaperType的类型转换为object
			// GetParameter的重载列表中第三个参数且是最后一个参数是int时
			// 表明此参数是输出类型
			cmd.Parameters.Add( SqlHelper.GetParameter( "@PaperType",		SqlDbType.Int,		(object)this.ComplexView.PaperType ) );
            cmd.Parameters.Add( SqlHelper.GetParameter( "@cAccId",          SqlDbType.NVarChar, 50, _cAccId));
            cmd.Parameters.Add( SqlHelper.GetParameter( "@cYear",           SqlDbType.NVarChar, 50, _cYear));
            cmd.Parameters.Add(SqlHelper.GetParameter("@bAfterRWizard", SqlDbType.Bit, this._bAfterRWizard));
            cmd.Parameters.Add(SqlHelper.GetParameter("@reportmergecell", SqlDbType.Bit, this._reportmergecell));
			
			return cmd;
		}

		private SqlCommand GetCmdDelete()
		{
			SqlCommand cmd	= new SqlCommand( "AA_DeleteReport" );
			cmd.CommandType = CommandType.StoredProcedure;
			cmd.Parameters.Add(SqlHelper.GetParameter( "@reportID", SqlDbType.NVarChar,	100, this.ID ));            
			return cmd;
		}
        private SqlCommand GetCmdDeleteUapRpt()
        {
            SqlCommand cmd = new SqlCommand("UAP_Report_DefinitionDelete");
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add(SqlHelper.GetParameter("@reportID", SqlDbType.NVarChar, 100, this.ID));
            cmd.Parameters.Add(SqlHelper.GetParameter("@cAccId",SqlDbType.NVarChar,10,_cAccId));
            cmd.Parameters.Add(SqlHelper.GetParameter("@cYear", SqlDbType.NVarChar, 10, _cYear));
            return cmd;        
        }
		
		private static SqlCommand GetCmdIsExistedReportName( 
			string reportID, 
			string reportName, 
			string subProjectID )
		{
			SqlCommand cmd	= new SqlCommand( "UAP_Report_DefinitionIsExistedReportName" );
			cmd.CommandType = CommandType.StoredProcedure;

			cmd.Parameters.Add( SqlHelper.GetParameter( "@ReportID",	SqlDbType.NVarChar,	100, reportID ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@ReportName",	SqlDbType.NVarChar,	100, reportName ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@SubID",		SqlDbType.NVarChar,	50, subProjectID ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@IsExisted",	SqlDbType.Bit ) );

			return cmd;
		}

		private static SqlCommand GetCmdReName(
			string reportID, 
			string reportName, 
			string localeID,
            string cAccId,
            string cYear)
		{
			SqlCommand cmd	= new SqlCommand( "UAP_Report_DefinitionReName" );
			cmd.CommandType = CommandType.StoredProcedure;

			cmd.Parameters.Add( SqlHelper.GetParameter( "@ReportID",	SqlDbType.NVarChar,	100, reportID ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@ReportName",	SqlDbType.NVarChar,	100, reportName ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@LocaleID",	SqlDbType.NVarChar,	10, localeID ) );
            cmd.Parameters.Add( SqlHelper.GetParameter( "@cAccId",	SqlDbType.NVarChar,	50, cAccId ) );
            cmd.Parameters.Add( SqlHelper.GetParameter( "@cYear",	SqlDbType.NVarChar,	50, cYear ) );
			return cmd;
		}

		private SqlCommand GetCmdRetrieve( 
			bool IsByViewID, 
			bool IsDesigningTime,
			bool isForStaticReport )
		{
			string FirstParamName	= string.Empty;
			string StoreProduceName = string.Empty;
			if( IsByViewID )
			{
				FirstParamName		= "@ViewID";
				StoreProduceName	= "UAP_Report_DefinitionRetrieveByView";
			}
			else
			{
				FirstParamName		= "@ReportID";
				StoreProduceName	= "UAP_Report_DefinitionRetrieve";
			}
				
			SqlCommand cmd	= new SqlCommand( StoreProduceName );
			cmd.CommandType = CommandType.StoredProcedure;

			cmd.Parameters.Add( SqlHelper.GetParameter( FirstParamName,		SqlDbType.NVarChar,	100, this.ID ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@LocaleID",		SqlDbType.NVarChar,	100, this.LocaleID ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@UserID",			SqlDbType.NVarChar,	100, this.UserID ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@DefaultViewID",	SqlDbType.NVarChar,	100 ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@cAccId",			SqlDbType.Char, 3,  _cAccId ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@cYear",			SqlDbType.Char, 4,	_cYear ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@IsForStaticReport", SqlDbType.Bit,	isForStaticReport ) );

			if( IsDesigningTime )
				cmd.Parameters.Add( SqlHelper.GetParameter( "@AuthOP",		SqlDbType.Char, 3,	"_02" ) );
			else
				cmd.Parameters.Add( SqlHelper.GetParameter( "@AuthOP",		SqlDbType.Char, 3,	"_01" ) );

			return cmd;
		}
		
		private void FillDefinition( DataTable table )
		{
			DataRow dr			= table.Rows[0];
			this._ID			= SqlHelper.GetStringFrom( dr[ "ID" ] );
			this._Name			= SqlHelper.GetStringFrom( dr[ "Name" ] );
			this._ProjectID		= SqlHelper.GetStringFrom( dr[ "ProjectID" ] );
			this._SubProjectID	= SqlHelper.GetStringFrom( dr[ "SubID" ] );
			this._FilterID		= SqlHelper.GetStringFrom( dr[ "FilterID" ] );
			this._LocaleID		= SqlHelper.GetStringFrom( dr[ "LocaleID"] );
			this._DataSourceID	= SqlHelper.GetStringFrom( dr[ "DataSourceID" ] );
			this._FunctionName	= SqlHelper.GetStringFrom( dr[ "FunctionName" ] );
			this._ClassName		= SqlHelper.GetStringFrom( dr[ "ClassName" ] );
			this._FilterClass	= SqlHelper.GetStringFrom( dr[ "FilterClass" ] );
			this._HelpFileName	= SqlHelper.GetStringFrom( dr[ "HelpFileName" ] );
			this._HelpIndex		= SqlHelper.GetStringFrom( dr[ "HelpIndex" ] );
			this._HelpKeyWord	= SqlHelper.GetStringFrom( dr[ "HelpKeyWord" ] );
			this._Creator		= SqlHelper.GetStringFrom( dr[ "Creator" ], "System" );
            this._bVB = SqlHelper.GetBooleanFrom(dr["bvb"], true);
			this._bSystem		= SqlHelper.GetBooleanFrom( dr[ "bSystem" ], true );
			this._canSaveAs		= SqlHelper.GetBooleanFrom( dr[ "CanSaveAs" ], true );
			this._bUsingReport4MenuId = SqlHelper.GetBooleanFrom( dr[ "bUsingReport4MenuId" ], false );
			this._CreatedTime	= SqlHelper.GetDataTimeFrom( dr[ "CreatedTime" ], "2006-09-19" );
			this._rootReportId	= SqlHelper.GetStringFrom( dr[ "RootReportId" ] );
			this._mappingMenuId	= SqlHelper.GetStringFrom( dr[ "MappingMenuId" ] );
			this._DataSourceIDExtended = SqlHelper.GetStringFrom( dr[ "DataSourceIdExtended" ], string.Empty );
		}

		private void FillComplexView( DataTable table )
		{
			DataRow dr					= table.Rows[0];
			_ComplexView.ID				= SqlHelper.GetStringFrom( dr[ "ID" ] );
			_ComplexView.Name			= SqlHelper.GetStringFrom( dr[ "Name" ] );
			_ComplexView.ReportID		= SqlHelper.GetStringFrom( dr[ "ReportID" ] );
			_ComplexView.Columns		= SqlHelper.GetStringFrom( dr[ "Columns" ] );
			_ComplexView.GroupSchemas	= SqlHelper.GetStringFrom( dr[ "GroupSchemas" ] );
			_ComplexView.PageMargins	= SqlHelper.GetStringFrom( dr[ "PageMargins" ] );
			_ComplexView.CommonFormat	= SqlHelper.GetStringFrom( dr[ "Format" ] );
			_ComplexView.LocaleFormat	= SqlHelper.GetStringFrom( dr[ "LocaleFormat" ] );
			_ComplexView.LocaleID		= SqlHelper.GetStringFrom( dr[ "LocaleID" ] );

			_ComplexView.LevelExpend	= SqlHelper.GetStringFrom( dr[ "LevelExpend" ] );
			_ComplexView.AssemblyString	= SqlHelper.GetStringFrom( dr[ "Assemblystring" ] );
			_ComplexView.ChartString	= SqlHelper.GetStringFrom( dr[ "Chartstring" ] );
			_ComplexView.ViewType		= SqlHelper.GetIntFrom( dr[ "ViewType" ], 0 );
			_ComplexView.PaperType		= SqlHelper.GetIntFrom( dr[ "PaperType" ], 9 );
            _ComplexView.BlandScape = SqlHelper.GetBooleanFrom(dr["BlandScape"], false);
            _ComplexView.ReportMergeCell = SqlHelper.GetBooleanFrom(dr["ReportMergeCell"], false);
			_ComplexView.ViewClass		= SqlHelper.GetStringFrom( dr[ "ViewClass" ] );
			_ComplexView.RowsCount		= SqlHelper.GetIntFrom( dr[ "RowsCount" ], 40 );
			_ComplexView.bShowDetail	= SqlHelper.GetBooleanFrom( dr[ "bShowDetail" ], true );
            _ComplexView.bMustShowDetail  = SqlHelper.GetBooleanFrom(dr["bMustShowDetail"], true);
			_ComplexView.RuntimeFormat	= SqlHelper.GetStringFrom( dr[ "RuntimeFormat" ], string.Empty );
			_ComplexView.FontColorStyleId	= SqlHelper.GetStringFrom( dr[ "FontColorStyleId" ], string.Empty );
            _ComplexView.CrossSchemas = SqlHelper.GetStringFrom(dr["PreservedField"]);            
		}

		private void FillSimpleViewCollection( DataTable table , string defaultViewID )
		{
			bool HasDefault = false;
			for( int i = 0; i < table.Rows.Count; i++ )
			{	
				SimpleView view = GetSimpleViewFrom( table.Rows[i] );
				if( view.ID == defaultViewID )
				{
					view.IsDefault	= true;
					HasDefault		= true;
				}
				view.State = PersistentObjectState.View;
				_SimpleViews.Add( view );
			}

			if( ! HasDefault && _SimpleViews.Count > 0 )
				_SimpleViews[0].IsDefault = true;
		}

		private SimpleView GetSimpleViewFrom( DataRow dr )
		{ 
			SimpleView view		= new SimpleView();
			view.ID				= SqlHelper.GetStringFrom( dr[ "ViewID" ] );
			view.Name			= SqlHelper.GetStringFrom( dr[ "Name" ] );
			view.LocaleID		= SqlHelper.GetStringFrom( dr[ "LocaleID" ] );
			view.ViewType		= SqlHelper.GetIntFrom( dr[ "ViewType" ], 0 );
			view.bSystem		= SqlHelper.GetBooleanFrom( dr[ "bSystem" ], false );
            view.ViewClass = SqlHelper.GetStringFrom(dr["ViewClass"]);
			view.RowsCount		= SqlHelper.GetIntFrom( dr[ "RowsCount" ], 40 );
			view.bShowDetail	= SqlHelper.GetBooleanFrom( dr[ "bShowDetail" ], true );
			view.FontColorStyleId = SqlHelper.GetStringFrom( dr[ "FontColorStyleId" ], string.Empty );
			return view;
		}

		#endregion
	}
}
