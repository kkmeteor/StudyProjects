/*
 * ����:¬����
 * ʱ��:2008.3.18
 */

using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;

namespace UFIDA.U8.UAP.Services.ReportData
{
	/// <summary>
	/// ������ĳ����Ŀ�����ݴ��ݸ������������ķ���
	/// </summary>
	internal interface IUpgradeItem
	{
		/// <summary>
		/// ���ݻ�����Ϣ
		/// </summary>
		/// <param name="infos">������Ϣ����</param>
		void DeliverEnvironmentInfos( Hashtable infos );

		/// <summary>
		/// ����������ɵ�����
		/// </summary>
		/// <param name="urmw">
		/// �������ݰ�װ�����书��������������ת���ɳ־û�����
		/// </param>
		void SetMeta( UpgradeReportMetaWrapper urmw );
	}
}
