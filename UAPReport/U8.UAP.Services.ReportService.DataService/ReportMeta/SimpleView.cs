using System;
using System.Data;
using System.Data.SqlClient;
using System.Runtime.Serialization;

namespace UFIDA.U8.UAP.Services.ReportData
{
	[Serializable]
	public class SimpleView
	{
		#region Parameter 

		private int		_ViewType	= 1;
		private int		_RowsCount	= 40;

		private bool	_IsDefault		= false;	
		private bool	_bSystem		= false;
		private bool	_bShowDetail	= false;
		
		private string	_ID				= string.Empty;
		private string	_Name			= string.Empty;
		private string	_LocaleID		= string.Empty;
		private string	_DbConnString	= string.Empty;
		private string	_ReportID		= string.Empty;
		private string	_ReportName		= string.Empty;
		private string	_UserID			= string.Empty;
		private string  _datacnn        = string.Empty;
        private string  _cAccId			= string.Empty;
		private string  _cYear			= string.Empty;
        private string _ViewClass = string.Empty;
        private bool _reportmergecell = false;
		private string	_fontColorStyleId = string.Empty;

		[NonSerialized]
		private SimpleViewCollection	_Parent	= null;
		[NonSerialized]
		private PersistentObjectState	_State	= PersistentObjectState.New;

		#endregion
		
		#region Constructor 

		public SimpleView(){}
		
		#endregion

		#region Exposed Interfeces
		
		#region Property

		public string FontColorStyleId
		{
			get{ return _fontColorStyleId; }
			set{ _fontColorStyleId= value; }
		}

		public int ViewType
		{
			get{ return _ViewType; }
			set{ _ViewType = value; }
		}

		public int RowsCount
		{
			get{ return _RowsCount; }
			set{ _RowsCount = value; }
		}

		public bool IsDefault
		{
			get{ return _IsDefault; }
			set{ _IsDefault = value; }
		}

		public bool bSystem
		{
			get{ return _bSystem; }
			set{ _bSystem = value; }
		}

		public bool bShowDetail
		{
			get{ return _bShowDetail; }
			set{ _bShowDetail = value; }
		}

		public string ID
		{
			get{ return _ID; }
			set{ _ID = value; }
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
		public string DbConnString
		{
			get{ return _DbConnString; }
			set{ _DbConnString = value; }
		}

        public string DataCnn
        {
            get { return _datacnn; }
            set { _datacnn = value; }
        }

		public PersistentObjectState State
		{
			get{ return _State; }
			set{ _State = value; }
		}

		internal string ReportID
		{
			get{ return _ReportID; }
			set{ _ReportID = value; }
		}

		internal string UserID
		{
			get{ return _UserID; }
			set{ _UserID = value; }
		}

		internal string ReportName
		{
			get{ return _ReportName; }
			set{ _ReportName = value; }
		}

		internal string cAccId
		{
			get{ return _cAccId; }
			set{ _cAccId = value; }
		}

		internal string cYear
		{
			get{ return _cYear; }
			set{ _cYear = value; }
		}

		internal SimpleViewCollection Parent
		{
			set{ _Parent = value; }
		}

		#endregion

		#region Internal Method

		internal void Save()
		{
			CheckDbCnnString();
			if ( this.State == PersistentObjectState.New )
				Insert();
			else if ( this.State == PersistentObjectState.View )
				Update();
			else if ( this.State == PersistentObjectState.Delete )
				Delete();
		}

		private void Insert()
		{
			SqlHelper.ExecuteNonQuery( DbConnString , this.GetCmdInsert() );
			this.State = PersistentObjectState.View;
		}

		private void Update()
		{
            SqlConnection cnn = new SqlConnection(DbConnString);
            cnn.Open();
            SqlTransaction sqlTransaction = cnn.BeginTransaction();

            try
            {
                SqlHelper.ExecuteNonQuery(sqlTransaction, this.GetCmdUpdate());
				ReportAuth ra = new ReportAuth();
				ra.UpdateViewNameToBusobject(this.DbConnString, this.DataCnn, sqlTransaction, this._ID);
                sqlTransaction.Commit();
            }
            catch( Exception e )
            {
                sqlTransaction.Rollback();
                throw e;
            }
            finally
            {
                if( cnn.State == ConnectionState.Open )
                    cnn.Close();
            }
            //SqlHelper.ExecuteNonQuery(DbConnString, this.GetCmdUpdate());
		}

		private void Delete()
		{
            SqlConnection cnn = new SqlConnection(DbConnString);
            cnn.Open();
            SqlTransaction sqlTransaction = cnn.BeginTransaction();

            try
            {
                SqlHelper.ExecuteNonQuery(sqlTransaction, this.GetCmdDelete());
                ReportAuth ra = new ReportAuth();
                ra.DeleteViewColFromDatabase(this.DataCnn, sqlTransaction, this._ID);
                _Parent.Remove(this);
                sqlTransaction.Commit();
            }
            catch (Exception e)
            {
                sqlTransaction.Rollback();
                throw e;
            }
            finally
            {
                if (cnn.State == ConnectionState.Open)
                    cnn.Close();
            }
		}

		private void CheckDbCnnString()
		{
			if( _DbConnString == string.Empty )
				ReportDataException.ThrowCnnStringEmptyException( "ReportDefinition.CheckDbCnnString()" );
		}

		private SqlCommand GetCmdInsert()
		{
			SqlCommand cmd	= new SqlCommand( "UAP_Report_SimpleViewInsert" );
			cmd.CommandType	= CommandType.StoredProcedure;
			cmd.Parameters.Add( SqlHelper.GetParameter( "@ReportID",		SqlDbType.NVarChar,	100,	this.ReportID ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@ViewID",			SqlDbType.NVarChar, 100,	this.ID ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@ViewType",		SqlDbType.Int,		(object)this.ViewType ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@ViewName",		SqlDbType.NVarChar,	100,	this.Name ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@UserID",			SqlDbType.NVarChar, 100,	this.UserID ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@IsDefault",		SqlDbType.Bit,		this.IsDefault ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@cAccId",			SqlDbType.NVarChar,	50,		_cAccId ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@cYear",			SqlDbType.NVarChar,	50,		_cYear ) );

			return cmd;
		}

		private SqlCommand GetCmdUpdate()
		{
			SqlCommand cmd	= new SqlCommand( "UAP_Report_SimpleViewUpdate" );
			cmd.CommandType	= CommandType.StoredProcedure;
			cmd.Parameters.Add( SqlHelper.GetParameter( "@ViewID",			SqlDbType.NVarChar,	100,	this.ID ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@ViewName",		SqlDbType.NVarChar,	100,	this.Name ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@LocaleID",		SqlDbType.NVarChar,	100,	this.LocaleID ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@UserID",			SqlDbType.NVarChar, 100,	this.UserID ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@IsDefault",		SqlDbType.Bit,		this.IsDefault ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@ReportName",		SqlDbType.NVarChar,	100,	this.ReportName ) );
            cmd.Parameters.Add( SqlHelper.GetParameter( "@cAccId",          SqlDbType.NVarChar, 50, _cAccId));
            cmd.Parameters.Add( SqlHelper.GetParameter( "@cYear",           SqlDbType.NVarChar, 50, _cYear));
			return cmd;
		}

		private SqlCommand GetCmdDelete()
		{
			SqlCommand cmd	= new SqlCommand( "UAP_Report_SimpleViewDelete" );
			cmd.CommandType	= CommandType.StoredProcedure;
			cmd.Parameters.Add( SqlHelper.GetParameter( "@ViewID",			SqlDbType.NVarChar, 100,	this.ID ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@cAccId",			SqlDbType.NVarChar,	50,		_cAccId ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@cYear",			SqlDbType.NVarChar,	50,		_cYear ) );

			return cmd;
		}

		#endregion
		#endregion
	}
}
