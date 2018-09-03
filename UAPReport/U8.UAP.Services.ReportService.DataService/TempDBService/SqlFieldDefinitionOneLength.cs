using System;
using System.Collections.Generic;
using System.Text;

namespace UFIDA.U8.UAP.Services.ReportData
{
	/// <summary>
	/// ������һ�����ȶ�����ֶζ���
	/// ����:¬����
	/// ʱ��:2007.5.17
	/// </summary>
	class SqlFieldDefinitionOneLength : AbtractSqlFieldDefinition
	{
		/// <summary>
		/// ��һ�����ȶ�����ֶ����Ͱ�����:
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
		/// �����ֶ�ԭʼ�ĳ��ȶ���
		/// </summary>
		/// <param name="size">�ֶ�ԭʼ�ĳ���</param>
		/// <returns>����ֵ����ʽ��:"(100)"</returns>
		public override string GetDataTypeLength( string size )
		{
			return string.Format( "({0})", size );
		}
	}
}
