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
			Trace.WriteLine( "ͨ�������������ɹ���������Դ! ID:" + dataSourceInfor.DataSourceBO.MetaID );
		}

		public static void SaveDirectInsertDataIntoTable(
			DataSourceInfor dataSourceInfor,
			string cnnString )
		{
			string sql = UpgradeSqlProducer.GetSqlDirectSaveDataSourceIntoTable( dataSourceInfor );
			SqlHelper.ExecuteNonQuery( cnnString, sql );
			Trace.WriteLine( "ֱ��д���BD_BusinessObjects���ɹ���������Դ! ID:" + dataSourceInfor.MetaID );
		}
	}
}
