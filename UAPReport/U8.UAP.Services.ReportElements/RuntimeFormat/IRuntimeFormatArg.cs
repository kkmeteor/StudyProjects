/*
 * ����:¬����
 * ʱ��:2008.4.7
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// ��������ʱ��ʽ���ݲ�����ز����ӿ�
	/// </summary>
	interface IRuntimeFormatArg
	{
		/// <summary>
		/// ���ݲ��������ʶ
		/// </summary>
		string Id { get; }
		
		/// <summary>
		/// ���ݲ�������ֵ
		/// </summary>
		object Tag { get; }
	}
}
