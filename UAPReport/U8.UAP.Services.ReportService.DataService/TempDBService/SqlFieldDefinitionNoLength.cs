using System;
using System.Collections.Generic;
using System.Text;

namespace UFIDA.U8.UAP.Services.ReportData
{
	/// <summary>
	/// ���SqlFieldDefinitionDecimal��SqlFieldDefinitionOneLength
	/// ��δ�ܴ�����ֶΣ����ɴ����ͽ����ֶδ���ɲ������ȵ��ֶζ���
	/// ����:¬����
	/// ʱ��:2007.5.17
	/// </summary>
	class SqlFieldDefinitionNoLength : AbtractSqlFieldDefinition
	{
		/// <summary>
		/// ������д������󷽷����˷����ڵ�ǰ�����в�ʹ��
		/// </summary>
		public override void InitSqlDataTypeKeys()
		{
		}

		/// <summary>
		/// �����ƥ������
		/// </summary>
		/// <param name="sqlDataType">ԭʼ���ֶ�����</param>
		/// <returns>
		/// ���SqlFieldDefinitionDecimal��SqlFieldDefinitionOneLength
		/// ��IsMatchToHandle()������falseʱ���˷�������true
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
