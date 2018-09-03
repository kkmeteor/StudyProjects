using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace UFIDA.U8.UAP.Services.ReportData
{
	/// <summary>
	/// �ṩת�����ݵ���ʱ������д�SqlDataReader����ķ�����
	/// ����:¬����
	/// ʱ��:2007.5.18
	/// </summary>
	class CreateTableFromReader
	{		
		private string _cnnString = string.Empty;
		private string _tempTableName = string.Empty;
		private SqlDataReader _reader = null;
		private SqlFieldDefinitionManager _sqlFieldDefinitionManager = null;

		/// <summary>
		/// ���캯��
		/// </summary>
		/// <param name="reader">��Ҫ�Ӵ�SqlDataReader����һ����ͬ�ṹ�������</param>
		/// <param name="cnnString">���ݿ����Ӵ�</param
		/// <param name="destTableName">Ŀ�����������</param>
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
		/// ���������
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
			
			// ÿ�ű�Ĭ����һ������Ϊ��BaseId����������ʶ�ֶ�
			// ע�⣺�˱��������󣬲�Ȼʹ��SqlBulkCopy�������ݵ�ʱ���д�
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
