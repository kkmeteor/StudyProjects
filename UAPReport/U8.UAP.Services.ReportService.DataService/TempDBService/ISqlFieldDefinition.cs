using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace UFIDA.U8.UAP.Services.ReportData
{
	/// <summary>
	/// ����CREATE TABLE������ֶζ���ķ���
	/// ����:¬����
	/// ʱ��:2007.5.17
	/// </summary>
	internal interface ISqlFieldDefinition
	{
		/// <summary>
		/// ����CREATE TABLE������ֶζ���ķ���Ψһ����
		/// </summary>
		/// <param name="dr">
		/// ��SqlDataReader��GetSchemaTable()��ȡ��DataTable��
		/// һ��DataRow�������ݱ���ֶζ�������
		/// </param>
		/// <returns>���յ��ֶζ��崮</returns>
		string ToSqlFieldDefinition( DataRow dr );
	}
}
