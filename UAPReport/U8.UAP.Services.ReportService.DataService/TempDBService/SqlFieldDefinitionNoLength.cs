using System;
using System.Collections.Generic;
using System.Text;

namespace UFIDA.U8.UAP.Services.ReportData
{
	/// <summary>
	/// 如果SqlFieldDefinitionDecimal和SqlFieldDefinitionOneLength
	/// 都未能处理的字段，将由此类型将此字段处理成不带长度的字段定义
	/// 作者:卢达其
	/// 时间:2007.5.17
	/// </summary>
	class SqlFieldDefinitionNoLength : AbtractSqlFieldDefinition
	{
		/// <summary>
		/// 仅仅重写父类抽象方法，此方法在当前类型中不使用
		/// </summary>
		public override void InitSqlDataTypeKeys()
		{
		}

		/// <summary>
		/// 处理的匹配条件
		/// </summary>
		/// <param name="sqlDataType">原始的字段类型</param>
		/// <returns>
		/// 如果SqlFieldDefinitionDecimal和SqlFieldDefinitionOneLength
		/// 的IsMatchToHandle()都返回false时，此方法返回true
		/// </returns>
		public override bool IsMatchToHandle( string sqlDataType )
		{
			SqlFieldDefinitionOneLength f1Filter = new SqlFieldDefinitionOneLength();
			SqlFieldDefinitionTwoLength f2Filter = new SqlFieldDefinitionTwoLength();
			if( f1Filter.IsMatchToHandle( sqlDataType )
				|| f2Filter.IsMatchToHandle( sqlDataType ))
				return false;
			return true;
		}
	}
}
