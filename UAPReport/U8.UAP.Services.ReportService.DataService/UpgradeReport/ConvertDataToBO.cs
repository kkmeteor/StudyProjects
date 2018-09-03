using System;
using System.Data;
using UFIDA.U8.UAP.Services.BizDAE.Elements;
using UFIDA.U8.UAP.Services.BizDAE.ConfigureServices;

namespace UFIDA.U8.UAP.Services.ReportData
{
	internal class ConvertDataToBO
	{
		#region Parameter

		private bool			_IsSqlQuerist		= false;
		private bool			_bVB				= false;
		private string			_SqlScript			= string.Empty;
		private string			_dllInfor			= string.Empty;

		private DataTable		_OldDataDataTable	= null;
		private BusinessObject	_BusinessObject		= null;

		#endregion

		#region Exposed Interface

		#region Constructor

		// 传进来的OldDataDataTable是返回结果列的描述
		// 该表内必须包含三个字段ColName,ColDescription,ColType
		public ConvertDataToBO( 
			DataTable OldDataDataTable,
			string subID,
			string boName )
		{
			this._OldDataDataTable	= OldDataDataTable;
			this._BusinessObject	= new BusinessObject( 
				Guid.NewGuid().ToString(),
				"U8CUSTDEF",
				subID,
                UFIDA.U8.UAP.Services.ReportResource.U8ResService.GetResStringEx("U8.UAP.Services.ReportData.ConvertDataToBO.870报表系统报表数据源"));
			this._BusinessObject.Name = boName;
		}

		#region Public Method

		// Sql脚本查询
		public BusinessObject GetBusinessObject( string SqlScript )
		{
			this._IsSqlQuerist	= true;
			this._SqlScript		= SqlScript;
			return this.GetBO();
		}

		// 自定义数据源
		public BusinessObject GetBusinessObject( bool bVB, string dllInfor )
		{
			this._IsSqlQuerist	= false;
			this._bVB			= bVB;
			this._dllInfor		= dllInfor;
			return this.GetBO();
		}

		#endregion

		#endregion

		#endregion
		
		#region Private Method

		private BusinessObject GetBO()
		{
			this._BusinessObject.Functions.Add( this.GetQueryFunction() );
			
			return this._BusinessObject;
		}

		private QueryFunction GetQueryFunction()
		{
            QueryFunction queryFunction = new QueryFunction("ReportFunction", UFIDA.U8.UAP.Services.ReportResource.U8ResService.GetResStringEx("U8.UAP.Services.ReportData.ConvertDataToBO.报表查询函数"));
			queryFunction.QuerySettings.Add( this.GetQuerySetting() );
			return queryFunction;
		}

		private QuerySetting GetQuerySetting()
		{
			string strName			= "ReportQuery";
            string strDescription = UFIDA.U8.UAP.Services.ReportResource.U8ResService.GetResStringEx("U8.UAP.Services.ReportData.ConvertDataToBO.报表数据源查询");
			QuerySetting querySetting = null;
			if( this._IsSqlQuerist )
			{	
				SQLScript sQLScript		= new SQLScript();
				sQLScript.SelectPart	= this._SqlScript;
				querySetting			= new SQLQuerySetting( strName, sQLScript, strDescription );
			}
			else
			{
				string dllType = string.Empty;
				if( this._bVB )
					dllType = "Com";
				else
					dllType = "DotNet";
				querySetting = new CustomQuerySetting( strName, dllType, this._dllInfor, strDescription );
			}

			FillColumnData( querySetting );
			
			return querySetting;
		}

		private void FillColumnData( QuerySetting querySetting )
		{
			for( int i = 0; i < this._OldDataDataTable.Rows.Count; i++ )
			{
				QueryResultColumn tc	= new QueryResultColumn();
				tc.Name			= this._OldDataDataTable.Rows[i][ "ColName" ].ToString();
				tc.Description	= this._OldDataDataTable.Rows[i][ "ColDescription" ].ToString();
				tc.DataType		= this.GetDataType( this._OldDataDataTable.Rows[i][ "ColType" ].ToString() );
				querySetting.QueryResultTable.Columns.Add( tc );
			}
		}

		private DataTypeEnum GetDataType( string strType )
		{
			switch( strType )
			{
				case "Float":
					return DataTypeEnum.Double;
				case "Int":
					return DataTypeEnum.Int32;
				case "DateTime":
					return DataTypeEnum.DateTime;
				case "Boolean":
					return DataTypeEnum.Boolean;
				case "Decimal":
					return DataTypeEnum.Decimal;
				case "Text":
					return DataTypeEnum.String;
				case "GUID":
					return DataTypeEnum.Guid;
				default:
					return DataTypeEnum.String;
			}
		}

		//private void SetDataToDB()
		//{
		//    System.IO.FileStream fs	= new System.IO.FileStream( 
		//        @"C:\Documents and Settings\ldq\桌面\BOCreatTest.xml",
		//        System.IO.FileMode.Append,
		//        System.IO.FileAccess.Write );
		//    System.IO.StreamWriter sw = new System.IO.StreamWriter( fs );

		//    sw.WriteLine( this._BusinessObject.Serialize() );
			
		//    sw.Close();
		//    fs.Close();
		//}
		
		#endregion
	}
}
