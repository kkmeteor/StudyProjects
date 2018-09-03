using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace UFIDA.U8.UAP.Services.ReportData
{
	class SqlFieldDefinitionManager : ISqlFieldDefinition
	{
		private ArrayList _sqlFieldDefinitionTypeHandlers = null;

		public SqlFieldDefinitionManager()
		{
			this._sqlFieldDefinitionTypeHandlers = new ArrayList();
			this._sqlFieldDefinitionTypeHandlers.Add( new SqlFieldDefinitionOneLength());
			this._sqlFieldDefinitionTypeHandlers.Add( new SqlFieldDefinitionNoLength());
			this._sqlFieldDefinitionTypeHandlers.Add( new SqlFieldDefinitionTwoLength());
		}

		public virtual string ToSqlFieldDefinition( DataRow dr )
		{
			foreach( ISqlFieldDefinition handler in this._sqlFieldDefinitionTypeHandlers )
			{
				string result = handler.ToSqlFieldDefinition( dr );
				if( !string.IsNullOrEmpty( result ))
					return result;
			}

			string msg = string.Format( 
				"没有适合的类型处理生成临时表的字段定义,出错类型：{0}",
				dr["DataTypeName"].ToString() );
			throw new Exception( msg );
		}
	}
}
