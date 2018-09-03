using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Data.SqlClient;
using UFIDA.U8.UAP.Services.BizDAE.ConfigureServices;

namespace UFIDA.U8.UAP.Services.ReportData
{
	internal class SaveDataSourceEngine
	{
		public static void SaveByDataEngineService( 
			DataSourceInfor dataSourceInfor, 
			SqlTransaction tran )
		{
			ConfigureService  proxy = new  ConfigureService( );
            proxy.UpdateBusinessObject( dataSourceInfor.DataSourceBO ,tran );
			Trace.WriteLine( "通过数据引擎服务成功保存数据源! ID:" + dataSourceInfor.DataSourceBO.MetaID );
		}

		public static void SaveDirectInsertDataIntoTable(
			DataSourceInfor dataSourceInfor,
			string cnnString )
		{
			string sql = UpgradeSqlProducer.GetSqlDirectSaveDataSourceIntoTable( dataSourceInfor );
			SqlHelper.ExecuteNonQuery( cnnString, sql );
			Trace.WriteLine( "直接写入表BD_BusinessObjects，成功保存数据源! ID:" + dataSourceInfor.MetaID );
		}
	}
}
