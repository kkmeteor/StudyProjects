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
	/// һ��������ĸ�ʽ��Ϣ
	/// </summary>
	[Serializable]
	public class RuntimeFormatGroupFormat : ConfigXmlItem
	{
		/// <summary>
		/// ��ǰ��������
		/// </summary>
		public override string Name 
		{
			get { return this.GetType().Name; }
		}
	}
}