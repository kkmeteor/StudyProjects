using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace UFIDA.U8.UAP.Services.ReportData
{
	/// <summary>
	/// 在内存中创建一个级次展开信息的DataTable,并装载上实际的数据
	/// 作者:卢达其
	/// 时间:2007.5.21
	/// </summary>
	class LevelExpandTempDBProduceDataTable
	{
		private string _cnnString = string.Empty;
		private LevelExpandTempDBHashTableToColumnService _columnInfoService = null;

		/// <summary>
		/// 构造函数
		/// </summary>
		/// <param name="columnInfoService">
		/// 列信息服务对象,可以从中获取查询装载数据的"SELECT"语句
		/// 和已经设置好架构的内存中的DataTable对象
		/// </param>
		/// <param name="cnnStrng">数据库连接串</param>
		public LevelExpandTempDBProduceDataTable(
			LevelExpandTempDBHashTableToColumnService columnInfoService,
			string cnnStrng )
		{ 
			this._columnInfoService = columnInfoService;
			this._cnnString = cnnStrng;
		}

		/// <summary>
		/// 装载数据到DataTable,此过程为：
		/// 1.根据物理表的列定义创建DataTable构架
		/// 2.从传入的列定义哈希表的值拼接sql，之后从源表中查询构造成SqlDataReader
		/// 3.调用外部服务获得目标表的数据并添加到DataTable，这里的外部服务
		/// 需要SqlDataReader为参数，返回值填在一个在本类创建的DataRow对象中.
		/// 其中1,2步骤由_columnInfoService来完成
		/// </summary>
		public void LoadDataToDataTable( 
			ILevelExpandTempDBGetDataService exteralService )
		{
			DataTable dt = this._columnInfoService.DataTableInMemory;
			SqlDataReader reader = this.GetReaderForExteralService();
			while( reader.Read() )
			{ 
				DataRow dr = dt.NewRow();
				dt.Rows.Add( dr );

				// 调用外部服务获取实际数据
				try
				{
					exteralService.GetData(
						this._columnInfoService.BasicColumnInfo,
						reader,
						dr );
				}
				catch( Exception e )
				{
					throw new TempDBServiceException( e,
						"调用外部服务以获得级次展开信息时发生错误" );
				}
			}
		}

		private SqlDataReader GetReaderForExteralService()
		{
			string sql = string.Empty;
			try
			{
				sql = this._columnInfoService.SelectDataToLoadSql;
				return SqlHelper.ExecuteReader( this._cnnString, sql );
			}
			catch( Exception e )
			{ 
				throw new TempDBServiceException( e, 
					"为获得级次展开信息而创建查询源表信息的SqlDataReader，查询语句:\r\n" + sql );
			}
		}
	}
}
