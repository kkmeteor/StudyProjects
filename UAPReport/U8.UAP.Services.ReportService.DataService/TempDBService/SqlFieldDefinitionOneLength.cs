using System;
using System.Collections.Generic;
using System.Text;

namespace UFIDA.U8.UAP.Services.ReportData
{
	/// <summary>
	/// 处理有一个长度定义的字段定义
	/// 作者:卢达其
	/// 时间:2007.5.17
	/// </summary>
	class SqlFieldDefinitionOneLength : AbtractSqlFieldDefinition
	{
		/// <summary>
		/// 有一个长度定义的字段类型包含有:
		/// "char",
		/// "nchar",
		/// "varchar",
		/// "nvarchar",
		/// </summary>
		public override void InitSqlDataTypeKeys()
		{
			this._sqlDataTypeKeys = new string[]{
				"char",
				"nchar",
				"varchar",
				"nvarchar",
			};
		}

		/// <summary>
		/// 返回字段原始的长度定义
		/// </summary>
		/// <param name="size">字段原始的长度</param>
		/// <returns>返回值的形式如:"(100)"</returns>
		public override string GetDataTypeLength( string size )
		{
			return string.Format( "({0})", size );
		}
	}
}
