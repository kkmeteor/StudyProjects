using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using UFIDA.U8.UAP.Services.ReportFilterService;
using UFIDA.U8.UAP.Services.BizDAE.Elements;
using UFIDA.U8.UAP.Services.BizDAE.ConfigureServices;
using UFIDA.U8.UAP.Services.BizDAE.DBServices;
using UFIDA.U8.UAP.Services.BizDAE.DBServices.QueryServices;

namespace UFIDA.U8.UAP.Services.ReportData
{
	/// <summary>
	/// 包装U8数据引擎定义的数据源的服务类
	/// 作者:卢达其
	/// 时间:2007.5.18
	/// </summary>
	class U8DataEngineHelper
	{
		private const string _UFWHERE = "{{UFWHERE}}";

		private string _sqlFromDataEngine = string.Empty;
		private ArrayList _cmdParameters = null;
		private CommandType _commandType = CommandType.Text;

		private string _dataSourceId = string.Empty;
		private string _filterString = string.Empty;
		private string _u8DataEnginefunctionName = string.Empty;
		private string _extendingDataSourceTempDBName = string.Empty;
		private U8LoginInfor _loginInfor = null; 
		
		/// <summary>
		/// 构造函数
		/// </summary>
		/// <param name="dataSourceId">数据源id</param>
		/// <param name="functionName">数据引擎查询数据源的查询函数名称</param>
		/// <param name="filterString">过滤串</param>
		/// <param name="loginInfor">u8登录信息</param>
		public U8DataEngineHelper(
			string dataSourceId,
            string functionName,
			string filterString,
			U8LoginInfor loginInfor,
			string extendingDataSourceTempDBName)
		{ 
			this._dataSourceId = dataSourceId;
			this._u8DataEnginefunctionName = functionName;
			this._filterString = filterString;
			this._loginInfor = loginInfor;
			this._extendingDataSourceTempDBName = extendingDataSourceTempDBName;
		}

        public string RawFilterString
        {
            get
            {
                return _filterString;
            }
        }

		public string SqlFromDataEngine
		{
			get { return this._sqlFromDataEngine; }
		}

		public ArrayList CmdParameters
		{
			get { return this._cmdParameters; }
		}

		public CommandType CmdType
		{
			get { return this._commandType; }
		}

		/// <summary>
		/// 从数据引擎服务读取数据配置信息
		/// </summary>
		public void ReadConfigurate()
		{
			QueryRequest request = new QueryRequest(
				this._dataSourceId,
				this._u8DataEnginefunctionName,
				this._loginInfor.UfDataCnnString,
				this._loginInfor.UfMetaCnnString,
				DataCollectionTypeEnum.DataReader );
            request.UserId = _loginInfor.UserID;
            request.AuthFunctionID = "R";
			request.DynamicQuerySettings.Add( GetDynamicQuerySetting() );
			try
			{
				ReportDBServiceProxy proxy = new ReportDBServiceProxy( this._loginInfor.AppServer );
                System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
                watch.Start();
				proxy.GetReportQueryInfo( request,
					out this._sqlFromDataEngine,
					out this._commandType,
					out this._cmdParameters );
                watch.Stop();
                System.Diagnostics.Trace.WriteLine("Invoke ReportDBServiceProxy GetReportQueryInfo Use Time :" + watch.ElapsedMilliseconds);
			}
			catch( Exception e )
			{ 
				string exMsg = "通过数据引擎ReportDBServiceProxy.GetReportQueryInfo方法获取数据源信息的时候出错.";
				exMsg += "DataSourceId:" + this._dataSourceId;
				throw new TempDBServiceException( e, exMsg );
			}
		}
		
		/// <summary>
		/// 1.this._filterString为空;
		///	原始的selectPart含有{{UFWHERE}},则把{{UFWHERE}}替换成where
		/// 2.this._filterString不为空,则把this._filterString包装成where字句并替换{{UFWHERE}};
		///  A.原始的selectPart含有{{UFWHERE}}:
		///    则把this._filterString包装成where字句并替换{{UFWHERE}},
		///    此时不设置DynamicQuerySetting.WherePart
		///  B.原始的selectPart不含有{{UFWHERE}}：
		///    此时仅仅设置DynamicQuerySetting.WherePart = " AND " + this._filterString
		/// </summary>
		/// <returns>
		/// 返回值为一个动态设置好sql信息的数据引擎定义的对象。 注意：
		/// DynamicQuerySetting对象是数据引擎允许在代码中动态设置sql
		/// 的实现方式，因此设置此对象之后原来在UAP中预置的selectpart
		/// 将被忽略，所以需要将原来设置的selectpart相应信息转移到
		/// DynamicQuerySetting对象中
		/// </returns>
		private DynamicQuerySetting GetDynamicQuerySetting()
		{
			QuerySetting qs = this.GetQuerySetting();
			if( qs is SQLQuerySetting )
				return this.GetDynamicQuert4SqlQuerySetting( qs );
			DynamicQuerySetting dynamicQuerySetting = new DynamicQuerySetting();
			this.SetFilterString( dynamicQuerySetting );
			
			if(!string.IsNullOrEmpty(this._extendingDataSourceTempDBName))
				dynamicQuerySetting.ChildQueryTmpTables[this._extendingDataSourceTempDBName] = this._extendingDataSourceTempDBName;
			return dynamicQuerySetting;
		}

		/// <summary>
		/// 调整过滤串:数据引擎要求设置给
		/// WherePart的过滤串必须是" and "开头
		/// </summary>
		private void SetFilterString( DynamicQuerySetting dynamicQuerySetting )
		{ 
			if( !string.IsNullOrEmpty( this._filterString ))
				dynamicQuerySetting.WherePart = " AND " + this._filterString;
		}

		private DynamicQuerySetting GetDynamicQuert4SqlQuerySetting( QuerySetting qs )
		{
			DynamicQuerySetting dynamicQuerySetting = new DynamicQuerySetting();
			string sourceSelectPart = this.GetSourceSelectPart( qs );
			string finalSelectPart = string.Empty;
			if( !string.IsNullOrEmpty( this._filterString ) )
			{
				finalSelectPart = GetFinalSelectPartWhenHasFilter( sourceSelectPart );
				if( !ContainsUfWhereString( sourceSelectPart ) )
					this.SetFilterString( dynamicQuerySetting );
			}
			else
				finalSelectPart = GetFinalSelectPartWhenNoFilter( sourceSelectPart );

			// 当sourceSelectPart不包含"{{UFWHERE}}"时,finalSelectPart
			// 必然为空,从而不会设置dynamicQuerySetting.SelectPart
			// 此时如果filterString不为空，会在以上的处理中设置在
			// dynamicQuerySetting.WherePart之中
			if( !string.IsNullOrEmpty( finalSelectPart ) )
				dynamicQuerySetting.SelectPart = finalSelectPart;
			return dynamicQuerySetting;
		}

		private bool ContainsUfWhereString( string selectPart )
		{
			if( !string.IsNullOrEmpty( selectPart )
				&& selectPart.IndexOf( "{{UFWHERE}}" ) != -1 )
				return true;
			return false;
		}

		private string GetFinalSelectPartWhenHasFilter( string sourceSelectPart )
		{ 
			string selectPart = sourceSelectPart;
			if ( ContainsUfWhereString( selectPart ))
	        {
	            string UfWhere = " WHERE " + this._filterString + " AND ";
	            selectPart = selectPart.Replace( _UFWHERE, UfWhere);
	            
				// 当原来只是有一个“{{UFWHERE}}”作为占位，而没有实际的where字句时
				// 必须要把处理后的where子句的最后的“AND ”去掉
				if (selectPart.Substring(selectPart.Length - 4) == "AND ")
	                selectPart = selectPart.Remove( selectPart.Length - 4 );
				return selectPart;
	        }
	        return null;
		}

		private string GetFinalSelectPartWhenNoFilter( string sourceSelectPart )
		{ 
			string selectPart = sourceSelectPart;
			if ( ContainsUfWhereString( selectPart ))
	        {
				// “{{UFWHERE}}”位于selectPart的末尾时直接删除“{{UFWHERE}}”
	            if (selectPart.Substring(selectPart.Length - 11) == _UFWHERE )
		            return selectPart.Replace( _UFWHERE, string.Empty );
		        else
		            return selectPart.Replace( _UFWHERE, " WHERE ");
	        }
	        return null;
		}

		private string GetSourceSelectPart( QuerySetting qs )
		{
			SQLQuerySetting sQLQuerySetting = qs as SQLQuerySetting;
			if( sQLQuerySetting != null 
			    && sQLQuerySetting.Script == null )
			{
				throw new TempDBServiceException(
					"从数据引擎获取的SQLQuerySetting对象为空",
					"DataSourceId:" + this._dataSourceId );
			}
			return sQLQuerySetting.Script.SelectPart;
		}

		private QuerySetting GetQuerySetting()
		{ 
			BusinessObject bo = this.GetBOFromDataEngine();
			QueryFunction qf = this.GetQueryFunctionFrom( bo );
			QuerySetting ruerySetting = qf.QuerySettings[0];
            if( ruerySetting == null )
			{
                throw new TempDBServiceException(
					"从数据引擎获取的QuerySetting对象为空",
					"DataSourceId:" + this._dataSourceId );
            }
			return ruerySetting;
		}

		private QueryFunction GetQueryFunctionFrom( BusinessObject bo )
		{ 
			QueryFunction qf = (QueryFunction)bo.Functions[0];
			if ( qf == null
	            && qf.QuerySettings == null
	            && qf.QuerySettings.Count == 0 )
	        {
	            throw new TempDBServiceException(
					"不能从数据引擎获取正确的QueryFunction对象",
					"DataSourceId:" + this._dataSourceId );
	        }
			return qf;
		}

		private BusinessObject GetBOFromDataEngine()
		{ 
			ConfigureServiceProxy proxy = new ConfigureServiceProxy(
		        this._loginInfor.AppServer,
		        this._loginInfor.UfMetaCnnString );
			proxy.LanguageId = "zh-CN";
			BusinessObject bo = proxy.GetBusinessObject( this._dataSourceId );
			if( bo == null
		        || bo.Functions == null
		        || bo.Functions.Count == 0 )
			{ 
				throw new TempDBServiceException(
					"不能从数据引擎获取正确的BusinessObject对象",
					"DataSourceId:" + this._dataSourceId );
			}
			return bo;
		}
	}
}
