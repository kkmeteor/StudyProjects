/*
 * ����:¬����
 * ʱ��:2009.7.28
 * 
 * ����:����������������,��ô���Ȩ�޴�����Sql���
 * �ӿڶ���
 */

using System;
using System.Collections.Generic;
using System.Text;

namespace UFIDA.U8.UAP.Services.ReportData
{
	internal interface IRowAuthColPicker
	{
		RowAuthContext Context { get; set; }
		bool bHit(string filed);
		string GetWhere(string filed);
	}
}
