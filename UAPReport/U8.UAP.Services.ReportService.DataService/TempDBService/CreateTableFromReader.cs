using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace UFIDA.U8.UAP.Services.ReportData
{
	/// <summary>
	/// 提供转移数据到临时表过程中从SqlDataReader建表的服务类
	/// 作者:卢达其
	/// 时间:2007.5.18
	/// </summary>
	class CreateTableFromReader
	{		
		private string _cnnString = string.Empty;
		private string _tempTableName = string.Empty;
		private SqlDataReader _reader = null;
		private SqlFieldDefinitionManager _sqlFieldDefinitionManager = null;

		/// <summary>
		/// 构造函数
		/// </summary>
		/// <param name="reader">需要从此SqlDataReader构建一张相同结构的物理表</param>
		/// <param name="cnnString">数据库连接串</param
		/// <param name="destTableName">目标物理表名称</param>
		public CreateTableFromReader( 
			SqlDataReader reader,
			string cnnString,
			string destTableName )
		{
			this._reader = reader;
			this._cnnString = cnnString;
			this._tempTableName = destTableName;
			this._sqlFieldDefinitionManager = new SqlFieldDefinitionManager();
		}

		/// <summary>
		/// 构建物理表
		/// </summary>
		public void CreateTable()
		{ 
			string sql = string.Empty;
			try
			{
				sql = GetCreateSql();
				SqlHelper.ExecuteNonQuery( this._cnnString, sql );
			}
			catch( Exception e )
			{ 
				throw new TempDBServiceException( e, sql );
			}
		}

		private string GetCreateSql()
		{ 
			StringBuilder sb = new StringBuilder();
			sb.Append( "CREATE TABLE " );
			sb.Append( this._tempTableName );
			sb.AppendLine( " (" );
			sb.Append( GetFieldDefinition( this._reader.GetSchemaTable() ) );
			
			// 每张表都默认有一个名称为“BaseId”的自增标识字段
			// 注意：此必须加在最后，不然使用SqlBulkCopy拷贝数据的时候有错
			sb.AppendLine( "," );
			sb.AppendLine( "BaseId int IDENTITY(1,1)" );

			sb.Append( ")" );
			return sb.ToString();
		}

		private string GetFieldDefinition( DataTable dt )
		{
			StringBuilder sb = new StringBuilder();
			foreach( DataRow dr in dt.Rows )
			{ 
				sb.Append( this._sqlFieldDefinitionManager.ToSqlFieldDefinition( dr ));
				sb.AppendLine( "," );
			}
			string fiels = sb.ToString().Trim();
			return fiels.Substring( 0, fiels.Length -1 );
		}
	}
}
