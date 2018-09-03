using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using UFIDA.U8.UAP.Services.ReportFilterService;

namespace UFIDA.U8.UAP.Services.ReportData
{
	/// <summary>
	/// �ṩ��U8�������涨�������Դת�����ݵ���ʱ��ķ���
	/// ����:¬����
	/// ʱ��:2007.5.18
	/// </summary>
	class U8DataEngine2TempTable : Base2TempTable
	{
		private U8DataEngineHelper _u8DataEngineHelper = null;
		private FilterSrv _filterSrv = null;

		/// <summary>
		/// ���캯��
		/// </summary>
		/// <param name="rdf">��������Base2TempTable�Ĺ��캯���Ĳ���</param>
		/// <param name="customDataSource">�Զ�������Դ����,��������Ҫ�����ʹ�����ʱ������</param>
		/// <param name="dataSourceId">��������������Դ�ı�ʶ</param>
		/// <param name="functionName">��������������Դ�Ĳ�ѯ����</param>
		/// <param name="filterSrv">����������Ϣ����</param>
		public U8DataEngine2TempTable( 
			ReportDataFacade rdf,
			CustomDataSource customDataSource,
			string dataSourceId,
            string functionName,
			FilterSrv filterSrv,
			string extendingDataSourceTempDBName)
			: base( rdf, customDataSource )
		{
			this._filterSrv = filterSrv;
			this._u8DataEngineHelper = new U8DataEngineHelper(
				dataSourceId, 
				functionName, 
				customDataSource.FilterString, 
				rdf._U8LoginInfor,
				extendingDataSourceTempDBName );
			this._u8DataEngineHelper.ReadConfigurate();
		}

		protected override void SetSqlOrStoreProcFlag()
		{
			switch( this._u8DataEngineHelper.CmdType )
			{
				default:
					break;
				case CommandType.Text:
					this._dataSourceType = DataSourceType.FromSqlExcuteBySqlCommand;
					break;
				case CommandType.StoredProcedure:
					this._dataSourceType = DataSourceType.FromStoreProcedure;
					break;
			}
		}

		protected override string GetSql()
		{
			throw new TempDBServiceException( 
				"��Զ�������U8DataEngine2TempTable.GetSql()�����������,�е��õ�����ͱ����д���",
				"None" );
		}

		protected override SqlCommand GetHandlingCommand()
		{
			switch( this._u8DataEngineHelper.CmdType )
			{
				default:
					return null;	
				case CommandType.Text:
					return GetSqlTextCmd();
				case CommandType.StoredProcedure:
					return GetStoreProcedureCmd();
			}
		}

		private SqlCommand GetSqlTextCmd()
		{
            string sql = base.GetSqlWithoutOrderBy(this._u8DataEngineHelper.SqlFromDataEngine);
			Hashtable paras = new Hashtable();
			if(sql.Contains("@") && this._filterSrv != null )
			{
				foreach( string key in this._filterSrv.Keys )
				{
                    if (key.Contains("."))
                        continue;
					FilterItem fi = this._filterSrv[key];
                    string parakey = "@" + fi.Key.ToLower();
					if( parakey != _cmdParaFilterString.ToLower()
						&& fi.Value1 != null && !paras.Contains(parakey))
						paras.Add( parakey , fi.Value1 );
                    if (fi.HasSecondValue)
                    {
                        parakey = "@" + fi.Key.ToLower() +"__2";
                        if (fi.Value2 != null && !paras.Contains(parakey))
                            paras.Add(parakey, fi.Value2);
                    }
				}
			}			
			SqlCommand cmd = SqlCommandHelper.GetSqlCommand( sql, paras );
			cmd.CommandType = CommandType.Text;
			return cmd;
		}

		private SqlCommand GetStoreProcedureCmd()
		{ 
			Hashtable paras = new Hashtable();
			if( this._u8DataEngineHelper.CmdParameters != null
				&& this._filterSrv != null )
			{
				foreach( object obj in this._u8DataEngineHelper.CmdParameters )
				{
                    if (obj.ToString().ToLower() == "@tablename")
                        paras.Add("@tablename", null);
                    else if (obj.ToString().ToLower() == "@filterstring")
                        paras.Add("@filterstring", this._u8DataEngineHelper.RawFilterString );
                    else
                    {
                        string key = obj.ToString();
                        if (key.Contains("."))
                            continue;
                        if (key.EndsWith("__2"))
                        {
                            FilterItem fi = this._filterSrv[key.Substring(1,key.Length-4)];
                            if (fi != null && fi.HasSecondValue && fi.Value2 !=null)
                                paras.Add(key, fi.Value2);
                        }
                        else
                        {
                            FilterItem fi = this._filterSrv[key.Substring(1)];
                            if (fi != null && fi.Value1 != null)
                                paras.Add(key, fi.Value1);
                        }
                    }
                    //FilterItem fi = this._filterSrv[obj.ToString()];
                    //if (fi != null)
                    //    paras.Add(fi.Key, fi.Value1);
				}
			}
			
			return SqlCommandHelper.GetSqlCommand( 
				this._u8DataEngineHelper.SqlFromDataEngine, 
				paras );
		}

		protected override void ThrowExceptionWhenNotAssignDataSourceType()
		{ 
			StringBuilder exMsg = new StringBuilder();
			exMsg.Append( "������������Դ�Ĵ�������ֻ����" );
			exMsg.Append( "CommandType.Text" );
			exMsg.AppendLine( "��CommandType.StoredProcedure" );
			exMsg.Append( "��������Ϊ:" );
			exMsg.Append( this._u8DataEngineHelper.CmdType.ToString() );
			throw new TempDBServiceException( 
				"ת�����ݵ���ʱ��ʱ����������������Դ���ʹ���", 
				exMsg.ToString());
		}
	}
}
