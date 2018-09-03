using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using UFIDA.U8.UAP.Services.ReportFilterService;

namespace UFIDA.U8.UAP.Services.ReportData
{
	/// <summary>
	/// 提供从自定义数据源转移数据到临时表的服务
	/// 作者:卢达其
	/// 时间:2007.5.17
	/// </summary>
	class CustomDataSource2TempTable : Base2TempTable
	{
		/// <summary>
		/// 构造函数
		/// </summary>
		/// <param name="rdf">传给父类Base2TempTable的构造函数的参数</param>
		/// <param name="customDataSource">自定义数据源的定义</param>
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
			//exMsg.Append( "自定义数据源，处理类型为" );
			//exMsg.Append( "CustomDataSourceTypeEnum.SQL" );
			//exMsg.AppendLine( "或CustomDataSourceTypeEnum.StoreProc" );
			//exMsg.Append( "而现在其为:" );
			//exMsg.Append( this._customDataSource.Type.ToString() );
			//throw new TempDBServiceException( 
			//    "转移数据到临时表时发现自定义数据源类型错误", 
			//    exMsg.ToString());
			System.Diagnostics.Trace.WriteLine( "处理类型为临时表" );
		}
	}
}
