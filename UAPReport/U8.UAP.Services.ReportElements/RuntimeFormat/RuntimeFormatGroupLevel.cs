/*
 * ����:¬����
 * ʱ��:2008.3.20
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using UFIDA.U8.UAP.Services.ReportResource;

namespace UFIDA.U8.UAP.Services.ReportElements
{
	/// <summary>
	/// һ�������鼶��ĸ�ʽ��Ϣ
	/// </summary>
	[Serializable]
	public class RuntimeFormatGroupLevel : ConfigXmlItem
	{
		/// <summary>
		/// ��ǰ��������
		/// </summary>
		public override string Name 
		{
			get { return this.GetType().Name; }
		}

		/// <summary>
		/// ��ȡһ���е�����ʱ��ʽ
		/// </summary>
		public RuntimeFormatGridColumn GetColumn(string colName)
		{ 
			return this.GetSubItem(colName) as RuntimeFormatGridColumn;
		}
	}
}
