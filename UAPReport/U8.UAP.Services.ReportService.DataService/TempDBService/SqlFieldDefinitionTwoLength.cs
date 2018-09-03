using System;
using System.Collections.Generic;
using System.Text;

namespace UFIDA.U8.UAP.Services.ReportData
{
	/// <summary>
	/// 两个精度定义类型的字段定义
	/// 作者:卢达其
	/// 时间:2007.5.17
	/// </summary>
	class SqlFieldDefinitionTwoLength : AbtractSqlFieldDefinition
	{
		/// <summary>
		/// 需要两个精度定义的类型有:
		/// "decimal",
		/// "numeric"
		/// </summary>
		public override void InitSqlDataTypeKeys()
		{
			this._sqlDataTypeKeys = new string[]{
				"decimal",
				"numeric",
			};
		}

		/// <summary>
		/// 重写父类方法
		/// </summary>
		/// <param name="size">原始的字段长度</param>
		/// <returns>返回固定的decimal长度定义为:"(38,10)"</returns>
		public override string GetDataTypeLength( string size )
		{
			return "(38,10)";
		}
	}
}
