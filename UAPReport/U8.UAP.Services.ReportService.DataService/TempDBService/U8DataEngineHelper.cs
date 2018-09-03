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
	/// ��װU8�������涨�������Դ�ķ�����
	/// ����:¬����
	/// ʱ��:2007.5.18
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
		/// ���캯��
		/// </summary>
		/// <param name="dataSourceId">����Դid</param>
		/// <param name="functionName">���������ѯ����Դ�Ĳ�ѯ��������</param>
		/// <param name="filterString">���˴�</param>
		/// <param name="loginInfor">u8��¼��Ϣ</param>
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
		/// ��������������ȡ����������Ϣ
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
				string exMsg = "ͨ����������ReportDBServiceProxy.GetReportQueryInfo������ȡ����Դ��Ϣ��ʱ�����.";
				exMsg += "DataSourceId:" + this._dataSourceId;
				throw new TempDBServiceException( e, exMsg );
			}
		}
		
		/// <summary>
		/// 1.this._filterStringΪ��;
		///	ԭʼ��selectPart����{{UFWHERE}},���{{UFWHERE}}�滻��where
		/// 2.this._filterString��Ϊ��,���this._filterString��װ��where�־䲢�滻{{UFWHERE}};
		///  A.ԭʼ��selectPart����{{UFWHERE}}:
		///    ���this._filterString��װ��where�־䲢�滻{{UFWHERE}},
		///    ��ʱ������DynamicQuerySetting.WherePart
		///  B.ԭʼ��selectPart������{{UFWHERE}}��
		///    ��ʱ��������DynamicQuerySetting.WherePart = " AND " + this._filterString
		/// </summary>
		/// <returns>
		/// ����ֵΪһ����̬���ú�sql��Ϣ���������涨��Ķ��� ע�⣺
		/// DynamicQuerySetting�������������������ڴ����ж�̬����sql
		/// ��ʵ�ַ�ʽ��������ô˶���֮��ԭ����UAP��Ԥ�õ�selectpart
		/// �������ԣ�������Ҫ��ԭ�����õ�selectpart��Ӧ��Ϣת�Ƶ�
		/// DynamicQuerySetting������
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
		/// �������˴�:��������Ҫ�����ø�
		/// WherePart�Ĺ��˴�������" and "��ͷ
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

			// ��sourceSelectPart������"{{UFWHERE}}"ʱ,finalSelectPart
			// ��ȻΪ��,�Ӷ���������dynamicQuerySetting.SelectPart
			// ��ʱ���filterString��Ϊ�գ��������ϵĴ�����������
			// dynamicQuerySetting.WherePart֮��
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
	            
				// ��ԭ��ֻ����һ����{{UFWHERE}}����Ϊռλ����û��ʵ�ʵ�where�־�ʱ
				// ����Ҫ�Ѵ�����where�Ӿ�����ġ�AND ��ȥ��
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
				// ��{{UFWHERE}}��λ��selectPart��ĩβʱֱ��ɾ����{{UFWHERE}}��
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
					"�����������ȡ��SQLQuerySetting����Ϊ��",
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
					"�����������ȡ��QuerySetting����Ϊ��",
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
					"���ܴ����������ȡ��ȷ��QueryFunction����",
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
					"���ܴ����������ȡ��ȷ��BusinessObject����",
					"DataSourceId:" + this._dataSourceId );
			}
			return bo;
		}
	}
}
