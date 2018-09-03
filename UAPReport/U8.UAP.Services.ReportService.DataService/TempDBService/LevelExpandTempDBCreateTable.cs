using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace UFIDA.U8.UAP.Services.ReportData
{
	/// <summary>
	/// 创建缓存级次展开数据的物理表
	/// 作者:卢达其
	/// 时间:2007.5.21
	/// </summary>
	class LevelExpandTempDBCreateTable
	{
		private string _cnnString = string.Empty;
		private LevelExpandTempDBHashTableToColumnService _columnInfoService = null;

		/// <summary>
		/// 构造函数。
		/// </summary>
		/// <param name="columnInfoService">
		/// 列信息服务对象,可以从中获取创建物理表的"CREATE TABLE"语句
		/// </param>
		/// <param name="cnnString">数据库连接串</param>
		public LevelExpandTempDBCreateTable( 
			LevelExpandTempDBHashTableToColumnService columnInfoService,
			string cnnString )
		{
			this._cnnString = cnnString;
			this._columnInfoService = columnInfoService;
		}

		public void Create()
		{
			string sql = string.Empty;
			try
			{
				sql = this._columnInfoService.CreateNewTableSql;
				SqlHelper.ExecuteNonQuery( this._cnnString, sql );
			}
			catch( Exception e )
			{
				throw new TempDBServiceException( e, GetExtraMessage( sql ));
			}
		}

		public void DropTableWhenErr()
		{
			string sql = string.Empty;
			try
			{
				sql = this._columnInfoService.CreateNewTableSql;
				SqlHelper.ExecuteNonQuery( this._cnnString, sql );
			}
			catch( Exception e )
			{
				throw new TempDBServiceException( e, GetExtraMessage( sql ));
			}
		}

		private string GetExtraMessage( string sql )
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine( "试图创建缓存级次展开数据缓存表的时候出现错误，相关信息:" );
			sb.AppendLine( "创建表sql语句:" );
			sb.AppendLine( sql );
			return sb.ToString();
		}
	}
}
