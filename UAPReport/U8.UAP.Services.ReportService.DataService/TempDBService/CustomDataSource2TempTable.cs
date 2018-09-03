using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using UFIDA.U8.UAP.Services.ReportFilterService;

namespace UFIDA.U8.UAP.Services.ReportData
{
	/// <summary>
	/// �ṩ���Զ�������Դת�����ݵ���ʱ��ķ���
	/// ����:¬����
	/// ʱ��:2007.5.17
	/// </summary>
	class CustomDataSource2TempTable : Base2TempTable
	{
		/// <summary>
		/// ���캯��
		/// </summary>
		/// <param name="rdf">��������Base2TempTable�Ĺ��캯���Ĳ���</param>
		/// <param name="customDataSource">�Զ�������Դ�Ķ���</param>
		public CustomDataSource2TempTable(
			ReportDataFacade rdf,
			CustomDataSource customDataSource )
			: base( rdf, customDataSource )
		{
		}

		protected override void SetSqlOrStoreProcFlag()
		{
			switch( this._customDataSource.Type )
			{
				default:
					break;
				case CustomDataSourceTypeEnum.SQL:
					this._dataSourceType = DataSourceType.FromSql;
					break;
				case CustomDataSourceTypeEnum.StoreProc:
					this._dataSourceType = DataSourceType.FromStoreProcedure;
					break;
			}
		}

		protected override string GetSql()
		{
			return this._customDataSource.SQL;
		}

		protected override SqlCommand GetHandlingCommand()
		{
			 return SqlCommandHelper.GetSqlCommand(
				 base._customDataSource.StoreProcName,
				 GetCmdParas());
		}

		private Hashtable GetCmdParas()
		{
			Hashtable paras = new Hashtable();
			foreach( string key in base._customDataSource.Params.Keys )
			{ 
			    if( key.ToLower() == _cmdParaFilterString.ToLower())
					paras.Add( key, base._customDataSource.FilterString );
				else
					paras.Add( key, base._customDataSource.Params[key] );
			}
			return paras;
		}

		protected override void ThrowExceptionWhenNotAssignDataSourceType()
		{ 
			//StringBuilder exMsg = new StringBuilder();
			//exMsg.Append( "�Զ�������Դ����������Ϊ" );
			//exMsg.Append( "CustomDataSourceTypeEnum.SQL" );
			//exMsg.AppendLine( "��CustomDataSourceTypeEnum.StoreProc" );
			//exMsg.Append( "��������Ϊ:" );
			//exMsg.Append( this._customDataSource.Type.ToString() );
			//throw new TempDBServiceException( 
			//    "ת�����ݵ���ʱ��ʱ�����Զ�������Դ���ʹ���", 
			//    exMsg.ToString());
			System.Diagnostics.Trace.WriteLine( "��������Ϊ��ʱ��" );
		}
	}
}
