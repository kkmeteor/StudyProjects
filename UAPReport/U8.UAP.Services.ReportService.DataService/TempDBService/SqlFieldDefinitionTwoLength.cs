using System;
using System.Collections.Generic;
using System.Text;

namespace UFIDA.U8.UAP.Services.ReportData
{
	/// <summary>
	/// �������ȶ������͵��ֶζ���
	/// ����:¬����
	/// ʱ��:2007.5.17
	/// </summary>
	class SqlFieldDefinitionTwoLength : AbtractSqlFieldDefinition
	{
		/// <summary>
		/// ��Ҫ�������ȶ����������:
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
		/// ��д���෽��
		/// </summary>
		/// <param name="size">ԭʼ���ֶγ���</param>
		/// <returns>���ع̶���decimal���ȶ���Ϊ:"(38,10)"</returns>
		public override string GetDataTypeLength( string size )
		{
			return "(38,10)";
		}
	}
}
