using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace UFIDA.U8.UAP.Services.ReportData
{
	/// <summary>
	/// 提供向TempDB缓存级次展开数据的服务
	/// 作者:卢达其
	/// 时间:2007.5.21
	/// </summary>
	class LevelExpandTempDBManager
	{
		private string _tempdbCnnString = string.Empty;
		private string _destTableName = string.Empty;
		private ReportDataFacade _reportDataFacade = null;
		private LevelExpandTempDBHashTableToColumnService _columnInfoService = null;
		private ILevelExpandTempDBGetDataService _exteralService = null;

		/// <summary>
		/// 构造函数。此类将在ReportDataFacade中创建并使用
		/// </summary>
		/// <param name="rdf">
		/// ReportDataFacade类，主要为了使用其中的TempDBCnnString
		/// </param>
		/// <param name="columnInfo">
		/// 目标表的列信息，其为一个哈希表：
		/// 1.键为目标表的列名称;
		/// 2.值为映射到源表中字段名称.
		/// 这两部分都将作为目标表中的字段
		/// </param>
		/// <param name="sourceTableName">源表的名称</param>
		/// <param name="destTableName">目标表的名称</param>
		/// /// <param name="exteralService">计算级次展开信息的服务对象</param>
		public LevelExpandTempDBManager(
			ReportDataFacade rdf,
			Hashtable columnInfo,
			string sourceTableName,
			string destTableName,
			ILevelExpandTempDBGetDataService exteralService )
		{ 
			this._reportDataFacade = rdf;
			this._destTableName = destTableName;
			this._exteralService = exteralService;
			this.Check( columnInfo, sourceTableName );
			this._tempdbCnnString = this._reportDataFacade._U8LoginInfor.TempDBCnnString;
			this._columnInfoService = new LevelExpandTempDBHashTableToColumnService(
				columnInfo, sourceTableName, destTableName );
		}

		private void Check(
			Hashtable columnInfo,
			string sourceTableName )
		{
			string msg = string.Empty;
			if( this._exteralService == null )
				msg = "级次展开信息的服务对象exteralService为空\r\n";
			if( this._reportDataFacade._U8LoginInfor == null
				|| string.IsNullOrEmpty( this._reportDataFacade._U8LoginInfor.TempDBCnnString ))
				msg += "U8LoginInfor和U8LoginInfor.TempDBCnnString为空\r\n";
			if( columnInfo == null
				|| columnInfo.Count == 0 )
				msg += "列定义columnInfo哈希表为空\r\n";
			if( string.IsNullOrEmpty( this._destTableName )
				|| string.IsNullOrEmpty( sourceTableName ))
				msg = "源表或目标表名称为空\r\n";
			if( !string.IsNullOrEmpty( msg ))
				throw new TempDBServiceException( msg, "None" );
		}

		/// <summary>
		/// 建立缓存数据的过程为：
		/// 1.在数据库中创建目标表
		/// 2.在内存中创建一个与目标相同的DataTable，并装载好实际数据
		/// 3.使用SqlBulkCopy将内存中的DataTable复制到目标表
		/// </summary>
		public void CacheExpandData()
		{
			try
			{
				this.CreateDestinationTable();
				this.LoadMomeryDataTable();
				this.MoveDataToDataBase();
			}
			catch( Exception e )
			{ 
				// 删除已经创建的临时表
				try
				{
					string sql = this._columnInfoService.DropTableSql;
					SqlHelper.ExecuteNonQuery( this._tempdbCnnString, sql );
				}
				catch { }
				throw e;
			}
		}

		private void CreateDestinationTable()
		{ 
			LevelExpandTempDBCreateTable tableCreator = new LevelExpandTempDBCreateTable(
				this._columnInfoService,
				this._tempdbCnnString );
			tableCreator.Create();
		}

		private void LoadMomeryDataTable()
		{ 
			LevelExpandTempDBProduceDataTable dataTableLoader = new LevelExpandTempDBProduceDataTable(
				this._columnInfoService,
				this._tempdbCnnString );
			dataTableLoader.LoadDataToDataTable( this._exteralService );
		}

		private void MoveDataToDataBase()
		{ 
			SqlBulkCopy sbc = new SqlBulkCopy( this._tempdbCnnString );
			sbc.DestinationTableName = this._destTableName;
			sbc.WriteToServer( this._columnInfoService.DataTableInMemory );
		}
	}
}
