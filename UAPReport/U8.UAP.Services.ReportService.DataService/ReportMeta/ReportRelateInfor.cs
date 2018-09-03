using System;
using System.Data;
using System.Data.SqlClient;
using System.Runtime.Serialization;
using UFIDA.U8.UAP.Services.ReportResource;
using UFIDA.U8.UAP.Services.ReportElements;

namespace UFIDA.U8.UAP.Services.ReportData
{
	[Serializable]
	public class ReportRelateInfor
	{
		private bool _bVB	= false;
        private string _rootReportId = string.Empty;
		private string _ReportID		= string.Empty;
		private string _FilterId		= string.Empty;
		private string _ClassName		= string.Empty;
		private string _FilterClass		= string.Empty;
		private string _DefaultViewID	= string.Empty;
        private string _filterxml = string.Empty;
		[NonSerialized]
		private string _InputID			= string.Empty;
		[NonSerialized]
		private string _cAccId			= string.Empty;
		[NonSerialized]
		private string _cYear			= string.Empty;
		[NonSerialized]
		private string _UserID			= string.Empty;
		[NonSerialized]
		private string _LocaleID		= string.Empty;
		[NonSerialized]
		private string _DbConnString	= string.Empty;

		private SimpleViewCollection	_Views = null;
		
		public ReportRelateInfor()
		{
			_Views = new SimpleViewCollection();
		}

        public string FilterXML
        {
            get
            {
                return _filterxml;
            }
            set
            {
                _filterxml = value;
            }
        }
		public bool bVB
		{
			get{ return _bVB; }
		}

        public string RootReportId
        {
            get
            {
                return _rootReportId;
            }
            set
            {
                _rootReportId = value;
            }
        }

		public string ReportID
		{
			set{ _ReportID = value; }
			get{ return _ReportID; }
		}

		public string FilterId
		{
			get{ return _FilterId; }
		}

		public string FilterClass
		{
			get{ return _FilterClass; }
		}

		public string ClassName
		{
			get{ return _ClassName; }
            set
            {
                _ClassName = value;
            }
		}
		
		public string DefaultViewID
		{
			get{ return _DefaultViewID; }
		}

		public SimpleViewCollection Views
		{
			get{ return _Views; }
		}

		internal string InputID
		{
			set{ _InputID = value; }
		}

		internal string cAccId
		{
			set{ _cAccId = value; }
		}

		internal string cYear
		{
			set{ _cYear = value; }
		}

		internal string UserID
		{
			set{ _UserID = value; }
		}

		internal string LocaleID
		{
			set{ _LocaleID = value; }
		}

		internal string DbConnString
		{
			set{ _DbConnString = value; }
		}

		internal void Retrieve()
		{
			CheckDbCnnString();

			SqlCommand cmd	= new SqlCommand( "UAP_Report_RuntimeExihibitonGetReportRelateInfor" );
			cmd.CommandType = CommandType.StoredProcedure;
			cmd.Parameters.Add( SqlHelper.GetParameter( "@ID",				SqlDbType.NVarChar,	100,	_InputID ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@cAccId",			SqlDbType.Char,		3,		_cAccId ));
			cmd.Parameters.Add( SqlHelper.GetParameter( "@cYear",			SqlDbType.Char,		4,		_cYear ));
			cmd.Parameters.Add( SqlHelper.GetParameter( "@cUser",			SqlDbType.NVarChar,	20,		_UserID ));
			cmd.Parameters.Add( SqlHelper.GetParameter( "@LocaleID",		SqlDbType.NVarChar,	10,		_LocaleID ));
			cmd.Parameters.Add( SqlHelper.GetParameter( "@DefaultViewID",	SqlDbType.NVarChar,	100 ) );
			cmd.Parameters.Add( SqlHelper.GetParameter( "@ReportID",		SqlDbType.NVarChar,	100 ) );
			
			DataSet ds = SqlHelper.ExecuteDataSet( _DbConnString , cmd );
			if( ds.Tables.Count > 1 )
			{
                string defaultviewid=cmd.Parameters["@DefaultViewID"].Value.ToString();
				for( int i = 0; i < ds.Tables[0].Rows.Count; i++ )
				{
					SimpleView view		= new SimpleView();
					view.ID				= ds.Tables[0].Rows[i][ "ViewGuid" ].ToString();
					view.Name			= ds.Tables[0].Rows[i][ "Name" ].ToString();
					view.ViewType		= Convert.ToInt32( ds.Tables[0].Rows[i][ "ViewType" ] );
				    view.ViewClass		= SqlHelper.GetStringFrom( ds.Tables[0].Rows[i][ "ViewClass" ] );
                    view.ReportMergeCell = SqlHelper.GetBooleanFrom(ds.Tables[0].Rows[i]["ReportMergeCell"], false);
					view.RowsCount		= SqlHelper.GetIntFrom( ds.Tables[0].Rows[i][ "RowsCount" ], 40 );
					view.bShowDetail	= SqlHelper.GetBooleanFrom( ds.Tables[0].Rows[i][ "bShowDetail" ], false );

                    if( view.ID.ToLower()== defaultviewid.ToLower())
                        view.IsDefault = true;

					_Views.Add( view );
				}

				DataRow dr		= ds.Tables[1].Rows[0];
				_ReportID		= cmd.Parameters["@ReportID"].Value.ToString();
				_ClassName		= SqlHelper.GetStringFrom( dr[ "ClassName" ] );
				_FilterId		= SqlHelper.GetStringFrom( dr[ "FilterId" ] );
				_FilterClass	= SqlHelper.GetStringFrom( dr[ "FilterClass" ] );
				_rootReportId	= SqlHelper.GetStringFrom( dr[ "RootReportId" ] );
				_bVB			= SqlHelper.GetBooleanFrom( dr[ "bVB" ], true );


				if( defaultviewid == string.Empty )
                {
					if( _Views.Count <= 0 )
                        throw new ResourceReportException(String4Report.GetString("操作员没有查询权限或视图权限!", _LocaleID));
                    _DefaultViewID = _Views[0].ID;
                } 
				else
					_DefaultViewID = defaultviewid ;
			}
		}

		private void CheckDbCnnString()
		{
			if( _DbConnString == string.Empty )
				ReportDataException.ThrowCnnStringEmptyException( "ReportDefinition.CheckDbCnnString()" );
		}
	}
}
